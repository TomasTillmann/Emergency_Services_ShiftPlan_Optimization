using ESSP.DataModel;
using FluentAssertions;
using Optimizing;

namespace MoveGenerationTest;

public class EmergencyServicePlanTest
{
  [Fact]
  public void CorrectEmptyInit()
  {
    Random random = new(69);
    var input = new EmergencyServicePlanTestInput(random);
    World world = input.GetWorld();
    
    EmergencyServicePlan empty = EmergencyServicePlan.GetNewEmpty(world);

    AssertPlanIsEmpty(empty, world);
  }

  [Fact]
  public void AllocateTeamTest()
  {
    Random random = new(69);
    var input = new EmergencyServicePlanTestInput(random);
    World world = input.GetWorld();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    Constraints constraints = input.GetConstraints();
    PlanSampler sampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.5, random);

    EmergencyServicePlan plan = EmergencyServicePlan.GetNewEmpty(world);

    Interval expectedInterval = Interval.GetByStartAndDuration(10, 80);
    plan.AllocateTeam(0, expectedInterval);
    
    plan.MedicTeamsCount.Should().Be(1);
    plan.AmbulancesCount.Should().Be(0);
    plan.TotalShiftDuration.Should().Be(expectedInterval.DurationSec);
    plan.Assignments.Length.Should().Be(world.Depots.Length);
    plan.AvailableMedicTeams.Count.Should().Be(world.AvailableMedicTeams.Length - 1);
    plan.AvailableAmbulances.Count.Should().Be(world.AvailableAmbulances.Length);
    plan.Assignments[0].MedicTeams.Count.Should().Be(1);
    plan.Assignments[0].MedicTeams[0].Shift.Should().Be(expectedInterval);
    plan.Assignments[0].Ambulances.Count.Should().Be(0);
    for (int depotIndex = 1; depotIndex < world.Depots.Length; ++depotIndex)
    {
      plan.Assignments[depotIndex].MedicTeams.Count.Should().Be(0);
      plan.Assignments[depotIndex].Ambulances.Count.Should().Be(0);
      
      for (int teamIndex = 0; teamIndex < plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
      {
        plan.Assignments[depotIndex].MedicTeams[teamIndex].Shift.Should().Be(0);
      }
    }
  }
  
  [Fact]
  public void DeallocateTeamTest()
  {
    Random random = new(69);
    var input = new EmergencyServicePlanTestInput(random);
    World world = input.GetWorld();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    Constraints constraints = input.GetConstraints();
    PlanSampler sampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.5, random);
    EmergencyServicePlan plan = EmergencyServicePlan.GetNewEmpty(world);
    Interval expectedInterval = Interval.GetByStartAndDuration(10, 80);
    plan.AllocateTeam(0, expectedInterval);
    
    plan.DeallocateTeam(0, 0);
    
    plan.MedicTeamsCount.Should().Be(0);
    plan.AmbulancesCount.Should().Be(0);
    plan.TotalShiftDuration.Should().Be(0);
    plan.Assignments.Length.Should().Be(world.Depots.Length);
    plan.AvailableMedicTeams.Count.Should().Be(world.AvailableMedicTeams.Length);
    plan.AvailableAmbulances.Count.Should().Be(world.AvailableAmbulances.Length);
    plan.Assignments[0].MedicTeams.Count.Should().Be(0);
    plan.Assignments[0].Ambulances.Count.Should().Be(0);
    for (int depotIndex = 1; depotIndex < world.Depots.Length; ++depotIndex)
    {
      plan.Assignments[depotIndex].MedicTeams.Count.Should().Be(0);
      plan.Assignments[depotIndex].Ambulances.Count.Should().Be(0);
      
      for (int teamIndex = 0; teamIndex < plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
      {
        plan.Assignments[depotIndex].MedicTeams[teamIndex].Shift.Should().Be(0);
      }
    }
  }
  
  [Fact]
  public void AllocateAmbulanceTest()
  {
    Random random = new(69);
    var input = new EmergencyServicePlanTestInput(random);
    World world = input.GetWorld();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    Constraints constraints = input.GetConstraints();
    PlanSampler sampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.5, random);
    EmergencyServicePlan plan = EmergencyServicePlan.GetNewEmpty(world);
    
    plan.AllocateAmbulance(0);
    
    plan.MedicTeamsCount.Should().Be(0);
    plan.AmbulancesCount.Should().Be(1);
    plan.TotalShiftDuration.Should().Be(0);
    plan.Assignments.Length.Should().Be(world.Depots.Length);
    plan.AvailableMedicTeams.Count.Should().Be(world.AvailableMedicTeams.Length);
    plan.AvailableAmbulances.Count.Should().Be(world.AvailableAmbulances.Length - 1);
    plan.Assignments[0].MedicTeams.Count.Should().Be(0);
    plan.Assignments[0].Ambulances.Count.Should().Be(1);
    for (int depotIndex = 1; depotIndex < world.Depots.Length; ++depotIndex)
    {
      plan.Assignments[depotIndex].MedicTeams.Count.Should().Be(0);
      plan.Assignments[depotIndex].Ambulances.Count.Should().Be(0);
      
      for (int teamIndex = 0; teamIndex < plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
      {
        plan.Assignments[depotIndex].MedicTeams[teamIndex].Shift.Should().Be(0);
      }
    }
  }
  
  [Fact]
  public void DeallocateAmbulanceTest()
  {
    Random random = new(69);
    var input = new EmergencyServicePlanTestInput(random);
    World world = input.GetWorld();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    Constraints constraints = input.GetConstraints();
    PlanSampler sampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.5, random);
    EmergencyServicePlan plan = EmergencyServicePlan.GetNewEmpty(world);
    
    plan.AllocateAmbulance(0);
    plan.DeallocateAmbulance(0);
    
    plan.MedicTeamsCount.Should().Be(0);
    plan.AmbulancesCount.Should().Be(0);
    plan.TotalShiftDuration.Should().Be(0);
    plan.Assignments.Length.Should().Be(world.Depots.Length);
    plan.AvailableMedicTeams.Count.Should().Be(world.AvailableMedicTeams.Length);
    plan.AvailableAmbulances.Count.Should().Be(world.AvailableAmbulances.Length);
    plan.Assignments[0].MedicTeams.Count.Should().Be(0);
    plan.Assignments[0].Ambulances.Count.Should().Be(0);
    for (int depotIndex = 1; depotIndex < world.Depots.Length; ++depotIndex)
    {
      plan.Assignments[depotIndex].MedicTeams.Count.Should().Be(0);
      plan.Assignments[depotIndex].Ambulances.Count.Should().Be(0);
      
      for (int teamIndex = 0; teamIndex < plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
      {
        plan.Assignments[depotIndex].MedicTeams[teamIndex].Shift.Should().Be(0);
      }
    }
  }

  [Fact]
  public void DeallocateAllTest()
  {
    
    Random random = new(69);
    var input = new EmergencyServicePlanTestInput(random);
    World world = input.GetWorld();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    Constraints constraints = input.GetConstraints();
    PlanSampler sampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.5, random);
    EmergencyServicePlan plan = sampler.Sample();
    
    plan.DeallocateAll();
    
    AssertPlanIsEmpty(plan, world);
  }
  
  [Theory]
  //[InlineData("fromEmpty")]
  [InlineData("fromRandom")]

  public void FillFromRandomTest(string type)
  {
    Random random = new(69);
    var input = new EmergencyServicePlanTestInput(random);
    World world = input.GetWorld();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    Constraints constraints = input.GetConstraints();
    PlanSampler sampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.5, random);
    EmergencyServicePlan filledPlan = type == "fromEmpty" ? EmergencyServicePlan.GetNewEmpty(world) : sampler.Sample();
    EmergencyServicePlan randomPlan = sampler.Sample();
    
    filledPlan.FillFrom(randomPlan);
    
    filledPlan.MedicTeamsCount.Should().Be(randomPlan.MedicTeamsCount);
    filledPlan.TotalShiftDuration.Should().Be(randomPlan.TotalShiftDuration);
    filledPlan.AmbulancesCount.Should().Be(randomPlan.AmbulancesCount);
    filledPlan.Assignments.Length.Should().Be(world.Depots.Length);
    filledPlan.AvailableAmbulances.Count.Should().Be(randomPlan.AvailableAmbulances.Count);
    filledPlan.AvailableMedicTeams.Count.Should().Be(randomPlan.AvailableMedicTeams.Count);
    for (int depotIndex = 0; depotIndex < world.Depots.Length; ++depotIndex)
    {
      filledPlan.Assignments[depotIndex].MedicTeams.Count.Should().Be(randomPlan.Assignments[depotIndex].MedicTeams.Count);
      filledPlan.Assignments[depotIndex].Ambulances.Count.Should().Be(randomPlan.Assignments[depotIndex].Ambulances.Count);
      
      for (int teamIndex = 0; teamIndex < filledPlan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
      {
        // same shift value
        filledPlan.Assignments[depotIndex].MedicTeams[teamIndex].Shift.Should().Be(randomPlan.Assignments[depotIndex].MedicTeams[teamIndex].Shift);
        
        // different instance
        filledPlan.Assignments[depotIndex].MedicTeams[teamIndex].Should().NotBe(randomPlan.Assignments[depotIndex].MedicTeams[teamIndex]);
      }
      
      for (int ambIndex = 0; ambIndex < filledPlan.Assignments[depotIndex].Ambulances.Count; ++ambIndex)
      {
        // different instance
        filledPlan.Assignments[depotIndex].Ambulances[ambIndex].Should().NotBe(randomPlan.Assignments[depotIndex].Ambulances[ambIndex]);
      }
    }
  }

  private void AssertPlanIsEmpty(EmergencyServicePlan empty, World world)
  {
    empty.MedicTeamsCount.Should().Be(0);
    empty.TotalShiftDuration.Should().Be(0);
    empty.AmbulancesCount.Should().Be(0);
    empty.Assignments.Length.Should().Be(world.Depots.Length);
    empty.AvailableAmbulances.Count.Should().Be(world.AvailableAmbulances.Length);
    empty.AvailableMedicTeams.Count.Should().Be(world.AvailableMedicTeams.Length);
    for (int depotIndex = 0; depotIndex < world.Depots.Length; ++depotIndex)
    {
      empty.Assignments[depotIndex].MedicTeams.Count.Should().Be(0);
      empty.Assignments[depotIndex].Ambulances.Count.Should().Be(0);
    }
  }
}