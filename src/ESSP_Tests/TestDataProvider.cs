using DataModel.Interfaces;
using ESSP.DataModel;

namespace ESSP_Tests;

public class TestDataProvider
{
    private Meters dimX;
    private Meters dimY;
    private Random random = new(42);

    private List<Hospital> hospitals;

    private List<Shift> shifts;

    private List<Depot> depots;

    private List<Ambulance> ambulances;

    private List<AmbulanceType> ambulanceTypes;

    private List<IncidentType> incidentTypes;

    private IDistanceCalculator distanceCalculator = new DistanceCalculator();

    public TestDataProvider()
    {
        this.dimX = 10_000.ToMeters();
        this.dimY = 10_000.ToMeters(); 

        GenerateAmbulanceTypes();
        GenerateIncidentTypes();
        GenerateAmbulances();
        GenerateDepots();
        GenerateShifts();
        GenerateHospitals();
    }

    public Incidents GetIncidents(int count, Hours duration)
    {
        List<Incident> incidents = new();
        for (int i = 0; i < count; ++i)
        {
            incidents.Add
            (
                new Incident
                (
                    new Coordinate(random.Next(0, dimX.Value).ToMeters(), random.Next(0, dimY.Value).ToMeters()),
                    random.Next(0, duration.ToSeconds().Value).ToSeconds(),
                    random.Next(0, 30).ToMinutes().ToSeconds(),
                    random.Next(0, 30).ToMinutes().ToSeconds(),
                    incidentTypes[random.Next(0, incidentTypes.Count - 1)]
                )
            );
        }

        incidents.Sort((x, y) => x.Occurence.CompareTo(y.Occurence));

        return new Incidents(incidents, 0.8);
    }

    public Incident GenerateIncident()
    {
        Random random = new(111);
        TestDataProvider dataProvider = new();
        List<AmbulanceType> ambTypes = dataProvider.GetAmbulanceTypes();
        int start = random.Next(0, ambTypes.Count - 2);

        return new Incident(
            location: new Coordinate { X = random.Next(0, 10_000).ToMeters(), Y = random.Next(0, 10_000).ToMeters() },
            occurence: random.Next(0, 8.ToHours().ToSeconds().Value).ToSeconds(),
            onSceneDuration: random.Next(30, 30.ToMinutes().ToSeconds().Value).ToSeconds(),
            inHospitalDelivery: random.Next(5.ToMinutes().ToSeconds().Value, 20.ToMinutes().ToSeconds().Value).ToSeconds(),
            type: new IncidentType
            {
                Name = $"IncidentTypeTest",
                AllowedAmbulanceTypes = ambTypes.GetRange(start, ambTypes.Count - start).ToHashSet(),
                MaximumResponseTime = random.Next(30.ToMinutes().ToSeconds().Value, 1.ToHours().ToSeconds().Value).ToSeconds(),
            }
        );
    }

    public List<IncidentType> GetIncidentTypes()
    {
        return incidentTypes;
    }

    public List<Hospital> GetHospitals()
    {
        return hospitals;
    }

    public ShiftPlan GetShiftPlan()
    {
        return new ShiftPlan(shifts);
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
            new IncidentType("IncType1", 1.ToHours().ToSeconds(), ambulanceTypes.GetRange(0,3).ToHashSet()),
            new IncidentType("IncType2", 1.ToHours().ToSeconds(), ambulanceTypes.GetRange(1,2).ToHashSet()),
            new IncidentType("IncType3", 1.ToHours().ToSeconds(), ambulanceTypes.GetRange(2,1).ToHashSet()),
        };
    }

    private void GenerateHospitals()
    {
        hospitals = new List<Hospital>()
        {
            new Hospital(new Coordinate(200.ToMeters(), 200.ToMeters())),
            new Hospital(new Coordinate(600.ToMeters(), 800.ToMeters())),
        };
    }

    private void GenerateShifts()
    {
        shifts = new List<Shift>();

        foreach (Depot depot in depots)
        {
            foreach (Ambulance ambulance in depot.Ambulances)
            {
                shifts.Add(new Shift(ambulance, depot, Interval.GetByStartAndDuration(0.ToSeconds(), 0.ToSeconds())));
            }
        }
    }

    private void GenerateDepots()
    {
        depots = new List<Depot>()
        {
            new Depot(new Coordinate(100.ToMeters(), 250.ToMeters()), ambulances.GetRange(0, 4)),
            new Depot(new Coordinate(300.ToMeters(), 800.ToMeters()), ambulances.GetRange(4, 2)),
        };
    }

    private void GenerateAmbulances()
    {
        ambulances = new List<Ambulance>()
        {
            new Ambulance(ambulanceTypes[0], new Coordinate(), 15.ToSeconds()),
            new Ambulance(ambulanceTypes[0], new Coordinate(), 15.ToSeconds()),
            new Ambulance(ambulanceTypes[1], new Coordinate(), 15.ToSeconds()),
            new Ambulance(ambulanceTypes[1], new Coordinate(), 15.ToSeconds()),
            new Ambulance(ambulanceTypes[1], new Coordinate(), 30.ToSeconds()),
            new Ambulance(ambulanceTypes[2], new Coordinate(), 50.ToSeconds()),
        };
    }

    private void GenerateAmbulanceTypes()
    {
        ambulanceTypes = new List<AmbulanceType>()
        {
            new AmbulanceType("AmbT1", 200),
            new AmbulanceType("AmbT2", 400),
            new AmbulanceType("AmbT3", 800),
        };
    }

    public Constraints GetConstraints()
    {
        List<Seconds> allowedShiftStartingTimes = new()
        {
            //TODO:
        };

        List<Seconds> allowedShiftDurations = new()
        {
            4.ToHours().ToSeconds(),
            6.ToHours().ToSeconds(),
            8.ToHours().ToSeconds(),
            9.ToHours().ToSeconds(),
            10.ToHours().ToSeconds(),
            11.ToHours().ToSeconds(),
            12.ToHours().ToSeconds(),
        };

        return new Constraints(allowedShiftStartingTimes, allowedShiftDurations);
    }
}