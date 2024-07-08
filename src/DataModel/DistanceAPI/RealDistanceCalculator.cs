using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Threading.Tasks;
using DataModel.Interfaces;
using ESSP.DataModel;
using Newtonsoft.Json.Linq;

namespace DistanceAPI;

public class RealDistanceCalculator(ImmutableArray<Hospital> hospitals) : IDistanceCalculator
{
    private readonly string _apiKey = ApiKeyParser.ApiKey;
    private readonly ImmutableArray<Hospital> _hospitals = hospitals = hospitals;
    private readonly Dictionary<Coordinate, Hospital> _nearestHospital = new();
    private readonly Dictionary<(Coordinate, Coordinate), int> _travelDuration = new();
    private readonly Dictionary<(Coordinate, Coordinate, int), Coordinate> _newLocation
        = new(new NewLocationComparer(5.ToMinutes().ToSeconds().Value, 0.001 /*about 100m*/));
    
    public int NearestHospitalHits { get; private set; }
    public int NearestHospitalTotal { get; private set; }
    
    public int TravelDurationHits { get; private set; }
    public int TravelDurationTotal { get; private set; }
    
    public int IntermediateLocationsHits { get; private set; }
    public int IntermediateLocationsTotal { get; private set; }

    public Hospital GetNearestHospital(Coordinate location)
    {
        ++NearestHospitalTotal;
        if (_nearestHospital.TryGetValue(location, out var nearestHospital))
        {
            ++NearestHospitalHits;
            return nearestHospital;
        }
        
        int bestDurationSec = int.MaxValue;
        int hospitalIndex = -1;
        for (int i = 0; i < _hospitals.Length; ++i)
        {
            int durationSec = GetTravelDurationSec(location, _hospitals[i].Location);
            if (durationSec < bestDurationSec)
            {
                bestDurationSec = durationSec;
                hospitalIndex = i;
            }
        }

        _nearestHospital[location] = _hospitals[hospitalIndex];
        return _hospitals[hospitalIndex];
    }

    public int GetTravelDurationSec(Coordinate from, Coordinate to)
    {
        ++TravelDurationTotal;
        if (_travelDuration.TryGetValue((from, to), out var duration))
        {
            ++TravelDurationHits;
            return duration;
        }
        
        var res = GetTravelDurationSecAsync(from, to).Result;
        _travelDuration[(from, to)] = res;
        return res;
    }

    public Coordinate GetIntermediateLocation(Coordinate from, Coordinate to, int durationDrivingSec)
    {
        ++IntermediateLocationsTotal;
        if (_newLocation.TryGetValue((from, to, durationDrivingSec), out var newLocation))
        {
            ++IntermediateLocationsHits;
            return newLocation;
        }
        var res = GetNewLocationAsync(from, to, durationDrivingSec).Result;
        _newLocation[(from, to, durationDrivingSec)] = res;
        return res;
    }

    private async Task<int> GetTravelDurationSecAsync(Coordinate from, Coordinate to)
    {
        string url = $"https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=" +
                     $"{from.Latitude},{from.Longitude}&destinations={to.Latitude},{to.Longitude}&key={_apiKey}";

        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string responseData = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseData); // dynamic
            int durationInSeconds = (int)json["rows"][0]["elements"][0]["duration"]["value"];
            //var value = json?.rows[0]?.elements[0]?.duration?.value;
            return durationInSeconds;
        }
        throw new Exception("Error fetching data from Google API");
    }

    private async Task<Coordinate> GetNewLocationAsync(Coordinate from, Coordinate to, int durationDrivingSec)
    {
        string url = $"https://maps.googleapis.com/maps/api/directions/json?origin=" +
                     $"{from.Latitude},{from.Longitude}&destination={to.Latitude},{to.Longitude}&key={_apiKey}";

        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string responseData = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseData);

            var steps = json["routes"][0]["legs"][0]["steps"];
            int accumulatedTime = 0;

            foreach (var step in steps)
            {
                int stepDuration = (int)step["duration"]["value"];
                accumulatedTime += stepDuration;

                if (accumulatedTime >= durationDrivingSec)
                {
                    double startLat = (double)step["start_location"]["lat"];
                    double startLng = (double)step["start_location"]["lng"];
                    double endLat = (double)step["end_location"]["lat"];
                    double endLng = (double)step["end_location"]["lng"];

                    int timeIntoStep = stepDuration - (accumulatedTime - durationDrivingSec);
                    double fraction = (double)timeIntoStep / stepDuration;
                    double intermediateLat = startLat + (endLat - startLat) * fraction;
                    double intermediateLng = startLng + (endLng - startLng) * fraction;

                    return new Coordinate { Latitude = intermediateLat, Longitude = intermediateLng };
                }
            }

            // desired travel time exceeds desired travel time, meaning, the vehicle is already at the destination
            return to;
        }
        
        throw new Exception("Error fetching data from Google API");
    }

    private class NewLocationComparer(int allowedDurationErrorSec, double allowedDistanceFromLocationErrorDegs) : IEqualityComparer<(Coordinate From, Coordinate To, int Duration)>
    {
        public int AllowedDurationErrorSec { get; set; } = allowedDurationErrorSec;
        public double AllowedDistanceFromLocationErrorDegs { get; set; } = allowedDistanceFromLocationErrorDegs;

        public bool Equals((Coordinate From, Coordinate To, int Duration) x, (Coordinate From, Coordinate To, int Duration) y)
        {
            // From location can potentially be always different, because it can be a location of rerouting ambulance.
            // This means, it is desirable to very similar from locations think about as equivalent.
            // On the other hand, to location will always be depot, hospital or incident, so there is no need to care for an error.
            return Math.Abs(x.From.Longitude - y.From.Longitude) <= AllowedDistanceFromLocationErrorDegs
                   && Math.Abs(x.From.Latitude - y.From.Latitude) <= AllowedDistanceFromLocationErrorDegs
                   && x.To == y.To
                   && Math.Abs(x.Duration - y.Duration) <= AllowedDurationErrorSec;
        }

        public int GetHashCode((Coordinate From, Coordinate To, int Duration) obj)
        {
            // important not to hash using Duration too
            return HashCode.Combine(obj.To);
        }
    }
}