using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class HospitalModel
{
    public string Name { get; set; }
    public CoordinateModel Location { get; set; }
}

public class CoordinateModel
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public class GooglePlacesService
{
    private readonly string _apiKey;
    private const string PlacesApiUrl = "https://maps.googleapis.com/maps/api/place/textsearch/json";

    public GooglePlacesService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<List<HospitalModel>> GetHospitalsInPragueAsync()
    {
        string query = "hospitals in Prague, Czech Republic";
        string url = $"{PlacesApiUrl}?query={Uri.EscapeDataString(query)}&key={_apiKey}";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<JObject>(json);

                if (data["status"].ToString() == "OK")
                {
                    var hospitals = new List<HospitalModel>();

                    foreach (var result in data["results"])
                    {
                        var hospital = new HospitalModel
                        {
                            Name = result["name"].ToString(),
                            Location = new CoordinateModel
                            {
                                Lat = Convert.ToDouble(result["geometry"]["location"]["lat"]),
                                Lng = Convert.ToDouble(result["geometry"]["location"]["lng"])
                            }
                        };

                        hospitals.Add(hospital);
                    }

                    return hospitals;
                }
                else
                {
                    throw new Exception($"Error from Google Places API: {data["status"].ToString()}");
                }
            }
            else
            {
                throw new HttpRequestException($"Failed to fetch data from Google Places API. Status code: {response.StatusCode}");
            }
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = File.ReadAllText("/home/tom/GoogleAPI/DistanceMatrixAPIkey.txt"); 
        var service = new GooglePlacesService(apiKey);

        try
        {
            var hospitals = await service.GetHospitalsInPragueAsync();

            foreach (var hospital in hospitals)
            {
                Console.WriteLine($"{hospital.Name}: {hospital.Location.Lat}, {hospital.Location.Lng}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

