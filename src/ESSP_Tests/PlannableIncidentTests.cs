using DataProviding;
using ESSP.DataModel;
using System;

namespace ESSP_Tests;

public abstract class PlannableIncidentTestsBase : Tests
{
    protected PlannableIncident.Factory plannableIncidentFactory { get; }

    public PlannableIncidentTestsBase()
    {
        plannableIncidentFactory = new(distanceCalculator, dataProvider.GetHospitals());
    }

    protected static IEnumerable<TestCaseData> GetPlannableIncidentImmidiatelyAvailableTestSource()
    {
        for (int i = 0; i < 100; i++)
        {
            yield return new TestCaseData(GetIncident());
        }
    }

    protected static IEnumerable<TestCaseData> GetPlannableIncidentOnBusyShiftTestSource()
    {
        List<Incident> incidents = new();
        for (int i = 0; i < 10; i++)
        {
            incidents.Add(GetIncident());
        }

        foreach(Incident incident1 in incidents)
        {
            foreach(Incident incident2 in incidents)
            {
                yield return new TestCaseData(incident1, incident2);
            }
        }
    }

    private static Incident GetIncident()
    {
        Random random = new(111);
        DataProvider dataProvider = new DataProvider(10_000.ToMeters(), 10_000.ToMeters());
        List<AmbulanceType> ambTypes = dataProvider.GetAmbulanceTypes();
        int start = random.Next(0, ambTypes.Count - 2);

        return new Incident(
            coordinate: new Coordinate { X = random.Next(0, 10_000).ToMeters(), Y = random.Next(0, 10_000).ToMeters() },
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
}

public class PlannableIncidentTests : PlannableIncidentTestsBase
{
    [TestCaseSource(nameof(GetPlannableIncidentImmidiatelyAvailableTestSource))]
    public void GetPlannableIncidentImmidiatelyAvailableTest(Incident incident)
    {
        Seconds currentTime = 300.ToSeconds();
        Shift shift = new(dataProvider.GetAmbulances().First(), dataProvider.GetDepots().First(), Interval.GetByStartAndDuration(300.ToSeconds(), 24.ToHours().ToSeconds()));


        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift, currentTime);


        Interval toIncident = Interval.GetByStartAndDuration(300.ToSeconds(), distanceCalculator.GetTravelDuration(shift.Ambulance, incident, currentTime));
        Assert.That(plannableIncident.ToIncidentDrive,
            Is.EqualTo(toIncident));

        Assert.That(plannableIncident.OnSceneDuration.Duration,
            Is.EqualTo(incident.OnSceneDuration));

        Hospital nearestHospital = distanceCalculator.GetNearestLocatable(incident, dataProvider.GetHospitals()).First();
        Assert.That(nearestHospital, Is.EqualTo(plannableIncident.NearestHospital));

        Seconds toHospitalDuration = distanceCalculator.GetTravelDuration(incident, nearestHospital, currentTime);
        Assert.That(plannableIncident.ToHospitalDrive.Duration,
            Is.EqualTo(toHospitalDuration));

        Assert.That(plannableIncident.InHospitalDelivery.Duration,
            Is.EqualTo(incident.InHospitalDelivery));

        Seconds toDepotDuration = distanceCalculator.GetTravelDuration(nearestHospital, shift.Depot, currentTime);
        Assert.That(plannableIncident.ToDepotDrive.Duration,
            Is.EqualTo(toDepotDuration));

        Assert.That(Interval.GetByStartAndDuration(300.ToSeconds(), toIncident.Duration + incident.OnSceneDuration + toHospitalDuration + incident.InHospitalDelivery + toDepotDuration),
            Is.EqualTo(plannableIncident.WholeInterval));

        Assert.That(Interval.GetByStartAndDuration(300.ToSeconds(), toIncident.Duration + incident.OnSceneDuration + toHospitalDuration + incident.InHospitalDelivery),
            Is.EqualTo(plannableIncident.IncidentHandling));
    }

    [TestCaseSource(nameof(GetPlannableIncidentOnBusyShiftTestSource))]
    public void GetPlannableIncidentOnBusyShiftTest(Incident incident1, Incident incident2)
    {
        Seconds currentTime = 300.ToSeconds();
        Shift shift = new(dataProvider.GetAmbulances().First(), dataProvider.GetDepots().First(), Interval.GetByStartAndDuration(300.ToSeconds(), 24.ToHours().ToSeconds()));
        shift.Plan(plannableIncidentFactory.Get(incident2, shift, currentTime));


        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident1, shift, currentTime);


        Seconds startingTimeOfDrive = shift.PlannedIncident(currentTime).ToDepotDrive.Start;
        Interval toIncident = Interval.GetByStartAndDuration(startingTimeOfDrive, distanceCalculator.GetTravelDuration(shift.PlannedIncident(currentTime).NearestHospital, incident1, currentTime));
        Assert.That(plannableIncident.ToIncidentDrive,
            Is.EqualTo(toIncident));

        Assert.That(plannableIncident.OnSceneDuration.Duration,
            Is.EqualTo(incident1.OnSceneDuration));

        Hospital nearestHospital = distanceCalculator.GetNearestLocatable(incident1, dataProvider.GetHospitals()).First();
        Assert.That(nearestHospital, Is.EqualTo(plannableIncident.NearestHospital));

        Seconds toHospitalDuration = distanceCalculator.GetTravelDuration(incident1, nearestHospital, currentTime);
        Assert.That(plannableIncident.ToHospitalDrive.Duration,
            Is.EqualTo(toHospitalDuration));

        Assert.That(plannableIncident.InHospitalDelivery.Duration,
            Is.EqualTo(incident1.InHospitalDelivery));

        Seconds toDepotDuration = distanceCalculator.GetTravelDuration(nearestHospital, shift.Depot, currentTime);
        Assert.That(plannableIncident.ToDepotDrive.Duration,
            Is.EqualTo(toDepotDuration));

        Assert.That(Interval.GetByStartAndDuration(startingTimeOfDrive, toIncident.Duration + incident1.OnSceneDuration + toHospitalDuration + incident1.InHospitalDelivery + toDepotDuration),
            Is.EqualTo(plannableIncident.WholeInterval));

        Assert.That(Interval.GetByStartAndDuration(startingTimeOfDrive, toIncident.Duration + incident1.OnSceneDuration + toHospitalDuration + incident1.InHospitalDelivery),
            Is.EqualTo(plannableIncident.IncidentHandling));
    }
}