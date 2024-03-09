using ESSP.DataModel;

namespace ESSP_Tests;

public class ShiftEvaluatorTests : Tests
{
  private ShiftEvaluator shiftEvaluator = new(plannableIncidentFactory);

  #region IsHandlingTests

  [Test]
  public void IsHandlingTestHandles()
  {
    Shift shift = new(testDataProvider.GetAmbulances().First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));

    // Everything is satisfied.
    Incident incident = testDataProvider.GenerateIncident();
    incident.Occurence = 10.ToSeconds();
    incident.Type = new IncidentType("sample", 10.ToHours().ToSeconds(), new HashSet<AmbulanceType> { shift.Ambulance.Type });
    PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);


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
    incident.Type = new IncidentType("sample", 10.ToSeconds(), new HashSet<AmbulanceType> { shift.Ambulance.Type });
    PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);


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


    Assert.That(shiftEvaluator.IsHandling(shift, plannableIncident), Is.EqualTo(false));
  }

  #endregion

  #region GetBetterTests

  [Test]
  public void GetBetterTest_OneIsFree()
  {
    Shift shift1 = new(testDataProvider.GetAmbulances().First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));
    Shift shift2 = new(testDataProvider.GetAmbulances().First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));

    Incident incident = testDataProvider.GenerateIncident();
    incident.Occurence = 300.ToSeconds();
    shift2.Plan(plannableIncidentFactory.Get(incident, shift2));

    Incident anotherIncident = testDataProvider.GenerateIncident();
    anotherIncident.Occurence = 350.ToSeconds();


    Shift betterShift = shiftEvaluator.GetBetter(shift1, shift2, anotherIncident);


    Assert.That(betterShift, Is.EqualTo(shift1));
  }

  [Test]
  public void GetBetterTest_OneIsFasterOnTheScene()
  {
    Shift shift1 = new(testDataProvider.GetDepots().Last().Ambulances.First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));
    Shift shift2 = new(testDataProvider.GetDepots().First().Ambulances.First(), testDataProvider.GetDepots().First(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));

    Incident incident = testDataProvider.GenerateIncident();

    // shift1 is much closer than shift2 to the incident 
    incident.Location = shift1.Ambulance.Location + new Coordinate { X = 10.ToMeters(), Y = 10.ToMeters() };


    Shift betterShift = shiftEvaluator.GetBetter(shift1, shift2, incident);


    Assert.That(betterShift, Is.EqualTo(shift1));
  }

  [Test]
  public void GetBetterTest_OneHasLessTimeActive()
  {
    // completely same
    Shift shift1 = new(testDataProvider.GetDepots().Last().Ambulances.First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));
    Shift shift2 = new(testDataProvider.GetDepots().Last().Ambulances.First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));
    //

    Incident incident = testDataProvider.GenerateIncident();
    incident.Occurence = 10.ToSeconds();
    shift2.Plan(plannableIncidentFactory.Get(incident, shift2));

    Incident anotherIncident = testDataProvider.GenerateIncident();

    // ends so both shifts are now free (and still are at the same location, so the distance to the incident is same for both
    anotherIncident.Occurence = plannableIncidentFactory.Get(incident, shift2).ToDepotDrive.End + 300.ToSeconds();


    Shift betterShift = shiftEvaluator.GetBetter(shift1, shift2, anotherIncident);


    Assert.That(betterShift, Is.EqualTo(shift1));
  }

  [Test]
  public void GetBetterTest_OneHasCheaperAmbulanceType()
  {
    // completely same - different ambulances only
    Shift shift1 = new(testDataProvider.GetDepots().Last().Ambulances.First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));
    Shift shift2 = new(testDataProvider.GetDepots().Last().Ambulances.Last(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));

    shift2.Ambulance.Type = new AmbulanceType("cheaperAmbulanceType", shift1.Ambulance.Type.Cost - 10);
    //

    Incident incident = testDataProvider.GenerateIncident();


    Shift betterShift = shiftEvaluator.GetBetter(shift1, shift2, incident);


    Assert.That(betterShift, Is.EqualTo(shift2));
  }

  [Test]
  public void GetBetterTest_Identical()
  {
    Shift shift1 = new(testDataProvider.GetDepots().Last().Ambulances.First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));
    Shift shift2 = new(testDataProvider.GetDepots().Last().Ambulances.First(), testDataProvider.GetDepots().Last(), Interval.GetByStartAndDuration(0.ToSeconds(), 8.ToHours().ToSeconds()));

    Incident incident = testDataProvider.GenerateIncident();


    Shift betterShift = shiftEvaluator.GetBetter(shift1, shift2, incident);


    Assert.That(betterShift, Is.EqualTo(shift1));
  }

  #endregion
}
