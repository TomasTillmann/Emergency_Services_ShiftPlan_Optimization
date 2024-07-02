using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public abstract class ObjectiveFunction : IObjectiveFunction
{
  public Simulation Simulation { get; }

  protected double MaxEmergencyServicePlanShiftDuration { get; }

  protected double AvailableAmbulancesTotalCount { get; }

  public ObjectiveFunction(Simulation simulation, ShiftTimes shiftTimes)
  {
    Simulation = simulation;
    MaxEmergencyServicePlanShiftDuration = simulation.World.AvailableMedicTeams.Length * shiftTimes.MaxDurationSec / 60;
    AvailableAmbulancesTotalCount = simulation.World.AvailableAmbulances.Length;
  }

  /// <inheritdoc/>
  public abstract double GetLoss();

  /// <inheritdoc/>
  public double GetSuccessRate()
  {
    return Simulation.SuccessRate;
  }

  /// <inheritdoc/>
  public double GetCost()
  {
    return Simulation.Plan.GetCost() / (MaxEmergencyServicePlanShiftDuration + AvailableAmbulancesTotalCount);
  }

  /// <inheritdoc/>
  public double GetEffectivity()
  {
    return Simulation.Plan.GetShiftDurationsSum() / (Simulation.Plan.GetTotalTimeActive() + 0.00000000000000000000001);
  }

  public double Get(Weights weights, ImmutableArray<Incident> incidents)
  {
    return Get(weights, incidents.AsSpan());
  }

  public double Get(Weights weights, ReadOnlySpan<Incident> incidents)
  {
    RunSimulation(weights, incidents);
    return GetLoss();
  }

  public double Get(EmergencyServicePlan plan, ReadOnlySpan<Incident> incidents)
  {
    Simulation.Plan = plan;
    Simulation.Run(incidents);
    return GetLoss();
  }

  protected void RunSimulation(Weights weights, ReadOnlySpan<Incident> incidents)
  {
    weights.MapTo(Simulation.Plan);
    Simulation.Run(incidents);
  }
}

