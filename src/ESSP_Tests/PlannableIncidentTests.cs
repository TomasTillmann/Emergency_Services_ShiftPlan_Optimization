using DataHandling;
using ESSP.DataModel;
using System;

namespace ESSP_Tests;

public abstract class PlannableIncidentTestsBase : Tests
{
    protected PlannableIncident.Factory plannableIncidentFactory { get; }

    public PlannableIncidentTestsBase()
    {
        plannableIncidentFactory = new(distanceCalculator, testDataProvider.GetHospitals());
    }

    protected static IEnumerable<TestCaseData> GetPlannableIncidentImmidiatelyAvailableTestSource()
    {
        for (int i = 0; i < 100; i++)
        {
            yield return new TestCaseData(testDataProvider.GenerateIncident());
        }
    }

    protected static IEnumerable<TestCaseData> GetPlannableIncidentOnBusyShiftTestSource()
    {
        List<Incident> incidents = new();
        for (int i = 0; i < 10; i++)
        {
            incidents.Add(testDataProvider.GenerateIncident());
        }

        foreach(Incident incident1 in incidents)
        {
            foreach(Incident incident2 in incidents)
            {
                yield return new TestCaseData(incident1, incident2);
            }
        }
    }

    // TODO: Tests don't run for some reason when this is used
    protected static IEnumerable<TestCaseData> GetPlannableIncidentMidwayInterruptionTestSource()
    {
        TestDataProvider dataProvider = new();
        List<Incident> incidents = new();

        for (int i = 0; i < 10; i++)
        {
            incidents.Add(dataProvider.GenerateIncident());
        }

        foreach (Incident incident in incidents)
        {
            Shift shift = new(dataProvider.GetAmbulances().First(), dataProvider.GetDepots().First(), Interval.GetByStartAndDuration(300.ToSeconds(), 24.ToHours().ToSeconds()));
            PlannableIncident plannableIncident = new PlannableIncident.Factory(dataProvider.GetDistanceCalculator(), dataProvider.GetHospitals())
                .Get(incident, shift);

            shift.Plan(plannableIncident);
            Incident incidentInterruptingMidway = dataProvider.GenerateIncident();
            incidentInterruptingMidway.Occurence = ((plannableIncident.ToDepotDrive.Start.Value + plannableIncident.ToDepotDrive.End.Value) / 2).ToSeconds();

            Coordinate newAmbLocation = dataProvider.GetDistanceCalculator()
                .GetNewLocation(plannableIncident.NearestHospital.Location, incident.Location, incidentInterruptingMidway.Occurence - plannableIncident.ToDepotDrive.Start, 300.ToSeconds());

            yield return new TestCaseData(incidentInterruptingMidway, shift, newAmbLocation);
        }
    }
}

public class PlannableIncidentTests : PlannableIncidentTestsBase
{
    [TestCaseSource(nameof(GetPlannableIncidentImmidiatelyAvailableTestSource))]
    public void GetPlannableIncidentImmidiatelyAvailableTest(Incident incident)
    {
        Seconds timeOfIncidentOccurence = incident.Occurence; 
        Shift shift = new(testDataProvider.GetAmbulances().First(), testDataProvider.GetDepots().First(), Interval.GetByStartAndDuration(300.ToSeconds(), 24.ToHours().ToSeconds()));


        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);


        Interval toIncident = Interval.GetByStartAndDuration(timeOfIncidentOccurence, distanceCalculator.GetTravelDuration(shift.Ambulance, incident, timeOfIncidentOccurence));
        Assert.That(plannableIncident.ToIncidentDrive,
            Is.EqualTo(toIncident));

        Assert.That(plannableIncident.OnSceneDuration.Duration,
            Is.EqualTo(incident.OnSceneDuration));

        Hospital nearestHospital = distanceCalculator.GetNearestLocatable(incident, testDataProvider.GetHospitals()).First();
        Assert.That(nearestHospital, Is.EqualTo(plannableIncident.NearestHospital));

        Seconds toHospitalDuration = distanceCalculator.GetTravelDuration(incident, nearestHospital, timeOfIncidentOccurence);
        Assert.That(plannableIncident.ToHospitalDrive.Duration,
            Is.EqualTo(toHospitalDuration));

        Assert.That(plannableIncident.InHospitalDelivery.Duration,
            Is.EqualTo(incident.InHospitalDelivery));

        Seconds toDepotDuration = distanceCalculator.GetTravelDuration(nearestHospital, shift.Depot, timeOfIncidentOccurence);
        Assert.That(plannableIncident.ToDepotDrive.Duration,
            Is.EqualTo(toDepotDuration));

        Assert.That(Interval.GetByStartAndDuration(timeOfIncidentOccurence, toIncident.Duration + incident.OnSceneDuration + toHospitalDuration + incident.InHospitalDelivery + toDepotDuration),
            Is.EqualTo(plannableIncident.WholeInterval));

        Assert.That(Interval.GetByStartAndDuration(timeOfIncidentOccurence, toIncident.Duration + incident.OnSceneDuration + toHospitalDuration + incident.InHospitalDelivery),
            Is.EqualTo(plannableIncident.IncidentHandling));
    }

    [TestCaseSource(nameof(GetPlannableIncidentOnBusyShiftTestSource))]
    public void GetPlannableIncidentOnBusyShiftTest(Incident incident1, Incident incident2)
    {
        Seconds timeOfIncidentOccurence = incident1.Occurence; 
        Shift shift = new(testDataProvider.GetAmbulances().First(), testDataProvider.GetDepots().First(), Interval.GetByStartAndDuration(timeOfIncidentOccurence, 24.ToHours().ToSeconds()));
        shift.Plan(plannableIncidentFactory.Get(incident2, shift));


        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident1, shift);


        Seconds startingTimeOfDrive = shift.PlannedIncident(timeOfIncidentOccurence).ToDepotDrive.Start;
        Interval toIncident = Interval.GetByStartAndDuration(startingTimeOfDrive, distanceCalculator.GetTravelDuration(shift.PlannedIncident(timeOfIncidentOccurence).NearestHospital, incident1, timeOfIncidentOccurence));
        Assert.That(plannableIncident.ToIncidentDrive,
            Is.EqualTo(toIncident));

        Assert.That(plannableIncident.OnSceneDuration.Duration,
            Is.EqualTo(incident1.OnSceneDuration));

        Hospital nearestHospital = distanceCalculator.GetNearestLocatable(incident1, testDataProvider.GetHospitals()).First();
        Assert.That(nearestHospital, Is.EqualTo(plannableIncident.NearestHospital));

        Seconds toHospitalDuration = distanceCalculator.GetTravelDuration(incident1, nearestHospital, timeOfIncidentOccurence);
        Assert.That(plannableIncident.ToHospitalDrive.Duration,
            Is.EqualTo(toHospitalDuration));

        Assert.That(plannableIncident.InHospitalDelivery.Duration,
            Is.EqualTo(incident1.InHospitalDelivery));

        Seconds toDepotDuration = distanceCalculator.GetTravelDuration(nearestHospital, shift.Depot, timeOfIncidentOccurence);
        Assert.That(plannableIncident.ToDepotDrive.Duration,
            Is.EqualTo(toDepotDuration));

        Assert.That(Interval.GetByStartAndDuration(startingTimeOfDrive, toIncident.Duration + incident1.OnSceneDuration + toHospitalDuration + incident1.InHospitalDelivery + toDepotDuration),
            Is.EqualTo(plannableIncident.WholeInterval));

        Assert.That(Interval.GetByStartAndDuration(startingTimeOfDrive, toIncident.Duration + incident1.OnSceneDuration + toHospitalDuration + incident1.InHospitalDelivery),
            Is.EqualTo(plannableIncident.IncidentHandling));
    }

    //[TestCaseSource(nameof(GetPlannableIncidentMidwayInterruptionTestSource))]
    [Test]
    public void GetPlannableIncidentMidwayInterruptionTest()
    {
        TestDataProvider dataProvider = new();
        Shift shift = new(dataProvider.GetAmbulances().First(), dataProvider.GetDepots().First(), Interval.GetByStartAndDuration(300.ToSeconds(), 24.ToHours().ToSeconds()));

        Incident incidentMidwayInterrupted = dataProvider.GenerateIncident();
        PlannableIncident plannableIncidentMidwayInterrupted = new PlannableIncident.Factory(dataProvider.GetDistanceCalculator(), dataProvider.GetHospitals())
            .Get(incidentMidwayInterrupted, shift);
        shift.Plan(plannableIncidentMidwayInterrupted);

        Incident incidentInterruptingMidway = dataProvider.GenerateIncident();
        incidentInterruptingMidway.Occurence = ((plannableIncidentMidwayInterrupted.ToDepotDrive.Start.Value + plannableIncidentMidwayInterrupted.ToDepotDrive.End.Value) / 2).ToSeconds();


        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incidentInterruptingMidway, shift);


        // 8983 = time of occurence + reroute penalty + time traveled from handled incident to depot
        // 9154 = time traveled to current incident from new ambulances location (location from hospital to depot in time of occurence - start of drive to depot duration)
        Interval toIncident = Interval.GetByStartAndEnd(8983.ToSeconds(), 9154.ToSeconds());

        Assert.That(plannableIncident.ToIncidentDrive,
            Is.EqualTo(toIncident));

        Assert.That(plannableIncident.OnSceneDuration.Duration,
            Is.EqualTo(incidentInterruptingMidway.OnSceneDuration));

        Hospital nearestHospital = distanceCalculator.GetNearestLocatable(incidentInterruptingMidway, dataProvider.GetHospitals()).First();
        Assert.That(plannableIncident.NearestHospital.Location, Is.EqualTo(nearestHospital.Location));

        Seconds toHospitalDuration = distanceCalculator.GetTravelDuration(incidentInterruptingMidway, nearestHospital, incidentInterruptingMidway.Occurence);
        Assert.That(plannableIncident.ToHospitalDrive.Duration,
            Is.EqualTo(toHospitalDuration));

        Assert.That(plannableIncident.InHospitalDelivery.Duration,
            Is.EqualTo(incidentInterruptingMidway.InHospitalDelivery));

        Seconds toDepotDuration = distanceCalculator.GetTravelDuration(nearestHospital, shift.Depot, incidentInterruptingMidway.Occurence);
        Assert.That(plannableIncident.ToDepotDrive.Duration,
            Is.EqualTo(toDepotDuration));

        Assert.That(Interval.GetByStartAndDuration(8983.ToSeconds(), toIncident.Duration + incidentInterruptingMidway.OnSceneDuration + toHospitalDuration + incidentInterruptingMidway.InHospitalDelivery + toDepotDuration),
            Is.EqualTo(plannableIncident.WholeInterval));

        Assert.That(Interval.GetByStartAndDuration(8983.ToSeconds(), toIncident.Duration + incidentInterruptingMidway.OnSceneDuration + toHospitalDuration + incidentInterruptingMidway.InHospitalDelivery),
            Is.EqualTo(plannableIncident.IncidentHandling));
    }
}