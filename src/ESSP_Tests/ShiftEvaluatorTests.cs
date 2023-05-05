using ESSP.DataModel;
using Microsoft.VisualStudio.CodeCoverage;
using Simulating;

namespace ESSP_Tests;

public class ShiftEvaluatorTests : Tests 
{
    [Test]
    public void IsHandlingTestHandles()
    {
        Shift shift = new(testDataProvider.GetAmbulances().First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));

        // Everything is satisfied.
        Incident incident = testDataProvider.GenerateIncident();
        incident.Occurence = 10.ToSeconds();
        incident.Type = new IncidentType("sample", 10.ToHours().ToSeconds(), new HashSet<AmbulanceType> { shift.Ambulance.Type } );
        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);
        ShiftEvaluator shiftEvaluator = new(plannableIncidentFactory);


        Assert.That(shiftEvaluator.IsHandling(shift, plannableIncident), Is.EqualTo(true));
    }

    [Test]
    public void IsHandlingTestResponseTimeNotSatisfied()
    {
        Shift shift = new(testDataProvider.GetAmbulances().First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));

        // Not enought time to finish handling the incident. The shift ends sooner.
        Incident incident = testDataProvider.GenerateIncident();
        incident.Occurence = 10.ToSeconds();
        incident.Location = shift.Ambulance.Location + new Coordinate(1000.ToMeters(), 1000.ToMeters());
        incident.Type = new IncidentType("sample", 10.ToSeconds(), new HashSet<AmbulanceType> { shift.Ambulance.Type } );
        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);
        ShiftEvaluator shiftEvaluator = new(plannableIncidentFactory);


        Assert.That(shiftEvaluator.IsHandling(shift, plannableIncident), Is.EqualTo(false));
    }

    [Test]
    public void IsHandlingTestAmbulanceTypeNotSatisfied()
    {
        Shift shift = new(testDataProvider.GetAmbulances().First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));

        // ambulance type not allowed
        Incident incident = testDataProvider.GenerateIncident();
        incident.Occurence = 10.ToSeconds();
        incident.Type = new IncidentType("sample", 10.ToHours().ToSeconds(), new HashSet<AmbulanceType> { new AmbulanceType() });
        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);
        ShiftEvaluator shiftEvaluator = new(plannableIncidentFactory);


        Assert.That(shiftEvaluator.IsHandling(shift, plannableIncident), Is.EqualTo(false));
    }

    public void IsHandlingTestAmbulanceTypesAllAllowed()
    {
        Shift shift = new(testDataProvider.GetAmbulances().First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));

        // If no ambulance types are provided, all ambulance types are allowed - Open world.
        Incident incident = testDataProvider.GenerateIncident();
        incident.Occurence = 10.ToSeconds();
        incident.Type = new IncidentType("sample", 10.ToHours().ToSeconds(), new HashSet<AmbulanceType> { });
        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);
        ShiftEvaluator shiftEvaluator = new(plannableIncidentFactory);


        Assert.That(shiftEvaluator.IsHandling(shift, plannableIncident), Is.EqualTo(true));
    }

    [Test]
    public void IsHandlingTestCantFinishIncidentTillEndOfShift()
    {
        Shift shift = new(testDataProvider.GetAmbulances().First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));

        // can't finish till the end of shift 
        Incident incident = testDataProvider.GenerateIncident();
        incident.Occurence = (7.ToHours().ToMinutes() + 55.ToMinutes()).ToSeconds();
        incident.Type = new IncidentType("sample", 10.ToHours().ToSeconds(), new HashSet<AmbulanceType> { shift.Ambulance.Type });
        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);
        ShiftEvaluator shiftEvaluator = new(plannableIncidentFactory);


        Assert.That(shiftEvaluator.IsHandling(shift, plannableIncident), Is.EqualTo(false));
    }
}
