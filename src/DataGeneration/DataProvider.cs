using DataModel.Interfaces;
using ESSP.DataModel;
using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataHandling;

public class DataProvider
{
    private Meters dimX;
    private Meters dimY;
    private Random random = new(42);

    private World world;

    private List<Depot> depots;

    private List<Hospital> hospitals;

    private List<Ambulance> ambulances;

    private List<AmbulanceType> ambulanceTypes;

    private List<IncidentType> incidentTypes;

    private IDistanceCalculator distanceCalculator = new DistanceCalculator();

    public DataProvider(int ambulancesCount)
    {
        dimX = 50_000.ToMeters();
        dimY = 50_000.ToMeters();

        GenerateAmbulanceTypes();
        GenerateIncidentTypes();
        GenerateAmbulances(ambulancesCount);
        GenerateDepots();
        GenerateHospitals();
        GenerateWorld();
    }

    public SuccessRatedIncidents GetIncidents(int count, Seconds duration, double successRateThreshold = 0.8)
    {
        List<Incident> incidents = new();
        for(int i = 0; i < count; ++i)
        {
            incidents.Add
            (
                new Incident
                (
                    location: new Coordinate(random.Next(0, dimX.Value).ToMeters(), random.Next(0, dimY.Value).ToMeters()),
                    occurence: random.Next(0, duration.Value).ToSeconds(),
                    onSceneDuration: random.Next(10, 40).ToMinutes().ToSeconds(),
                    inHospitalDelivery: random.Next(5, 20).ToMinutes().ToSeconds(),
                    type: incidentTypes[random.Next(0, incidentTypes.Count - 1)]
                )
            );
        }

        incidents.Sort((x, y) => x.Occurence.CompareTo(y.Occurence));

        return new SuccessRatedIncidents(incidents, successRateThreshold);
    }

    public World GetWorld()
    {
        return world;
    }

    public List<IncidentType> GetIncidentTypes()
    {
        return incidentTypes;
    }

    public List<Hospital> GetHospitals()
    {
        return hospitals;
    }

    public List<Depot> GetDepots()
    {
        return depots;
    }

    public List<Ambulance> GetAmbulances()
    {
        return ambulances;
    }

    public List<AmbulanceType> GetAmbulanceTypes()
    {
        return ambulanceTypes; 
    }

    public IDistanceCalculator GetDistanceCalculator()
    {
        return distanceCalculator; 
    }

    private void GenerateIncidentTypes()
    {
        incidentTypes = new List<IncidentType>()
        {
            //new IncidentType("IncType1", 15.ToMinutes().ToSeconds(), ambulanceTypes.GetRangeRandom(random).ToHashSet()),
            //new IncidentType("IncType2", 30.ToMinutes().ToSeconds(), ambulanceTypes.GetRangeRandom(random).ToHashSet()),
            //new IncidentType("IncType3", 30.ToMinutes().ToSeconds(), ambulanceTypes.GetRangeRandom(random).ToHashSet()),
            //new IncidentType("IncType4", 1.ToHours().ToSeconds(), ambulanceTypes.GetRangeRandom(random).ToHashSet()),
            //new IncidentType("IncType5", 1.ToHours().ToSeconds(), ambulanceTypes.GetRangeRandom(random).ToHashSet()),
            //new IncidentType("IncType6", 3.ToHours().ToSeconds(), ambulanceTypes.GetRangeRandom(random).ToHashSet()),
            new IncidentType("IncType7", 1.ToHours().ToSeconds(), ambulanceTypes.GetRandomRange(random, minCount: 1).ToHashSet()),
        };
    }

    private void GenerateWorld()
    {
        world = new World(depots, hospitals, distanceCalculator);
    }

    private void GenerateHospitals()
    {
        hospitals = new List<Hospital>();

        Meters stepY = (dimY.Value / 5).ToMeters();
        Meters stepX = (dimX.Value / 5).ToMeters();

        for(Meters y = 300.ToMeters(); y < dimY; y += stepY)
            for(Meters x = 300.ToMeters(); x < dimX; x += stepX)
                hospitals.Add(new Hospital(new Coordinate(x, y)));
    }

    private void GenerateDepots()
    {
        depots = new List<Depot>();
        List<Ambulance> ambulances = new(this.ambulances);

        Meters stepY = (dimY.Value / 5).ToMeters();
        Meters stepX = (dimX.Value / 5).ToMeters();

        for (Meters y = 10.ToMeters(); y < dimY; y += stepY)
        {
            for (Meters x = 10.ToMeters(); x < dimX; x += stepX)
            {
                HashSet<Ambulance> selectedAmbulances = ambulances.GetRandomRange(random, minCount: 1, maxCount: 20).ToHashSet();
                ambulances.RemoveAll(amb => selectedAmbulances.Contains(amb));

                depots.Add(new Depot(new Coordinate(x, y), selectedAmbulances.ToList()));
            }
        }
    }

    private void GenerateAmbulances(int ambulancesCount)
    {
#if false
        ambulances = new List<Ambulance>
        {
            new Ambulance(ambulanceTypes[0], new Coordinate(), 15.ToSeconds()),
            new Ambulance(ambulanceTypes[0], new Coordinate(), 15.ToSeconds()),
            new Ambulance(ambulanceTypes[0], new Coordinate(), 15.ToSeconds()),
            new Ambulance(ambulanceTypes[0], new Coordinate(), 15.ToSeconds()),
            new Ambulance(ambulanceTypes[0], new Coordinate(), 15.ToSeconds()),
        };
#endif

#if true
        ambulances = new List<Ambulance>();
        //List<Seconds> reroutePenalties = new() { 15.ToSeconds(), 30.ToSeconds(), 70.ToSeconds() };
        List<Seconds> reroutePenalties = new() { 500.ToSeconds() };

        for(int i = 0; i < ambulancesCount; ++i)
        {
            ambulances.Add(new Ambulance(ambulanceTypes.GetRandomElement(random), new Coordinate(), reroutePenalties.GetRandomElement(random)));
        }
#endif
    }

    private void GenerateAmbulanceTypes()
    {
        ambulanceTypes = new List<AmbulanceType>()
        {
            new AmbulanceType("AmbT1", 200),
            //new AmbulanceType("AmbT2", 400),
            //new AmbulanceType("AmbT3", 800),
            //new AmbulanceType("AmbT4", 1000),
            //new AmbulanceType("AmbT5", 1500),
            //new AmbulanceType("AmbT6", 5000),
        };
    }

    public Domain GetDomain()
    {
#if false
        List<Seconds> allowedShiftStartingTimes = new();
        for(Hours hour = 0.ToHours(); hour <= 20.ToHours(); ++hour)
        {
            allowedShiftStartingTimes.Add(hour.ToSeconds());
        }
#endif

        HashSet<Seconds> allowedShiftStartingTimes = new()
        {
            0.ToHours(),
            6.ToHours(),
            12.ToHours(),
            18.ToHours(),
            //18.ToHours().ToSeconds()
            //0.ToSeconds(),
            //1.ToSeconds(),
            //2.ToSeconds()
        };

        HashSet<Seconds> allowedShiftDurations = new()
        {
            0.ToHours(),
            4.ToHours(),
            6.ToHours(),
            8.ToHours(),
            12.ToHours(),
            //24.ToHours().ToSeconds(),
            //10.ToSeconds(),
            //100.ToSeconds()
        };

        return new Domain(allowedShiftStartingTimes, allowedShiftDurations);
    }
}