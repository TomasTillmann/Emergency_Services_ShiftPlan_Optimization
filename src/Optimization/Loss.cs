using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public abstract class Loss : ILoss
{
  public Simulation Simulation { get; }

  protected double MaxEmergencyServicePlanShiftDuration { get; }

  protected double AvailableAmbulancesTotalCount { get; }

  public Loss(Simulation simulation, ShiftTimes shiftTimes)
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
    return Simulation.EmergencyServicePlan.GetCost() / (MaxEmergencyServicePlanShiftDuration + AvailableAmbulancesTotalCount);
  }

  /// <inheritdoc/>
  public double GetEffectivity()
  {
    return Simulation.EmergencyServicePlan.GetShiftDurationsSum() / (Simulation.EmergencyServicePlan.GetTotalTimeActive() + 0.00000000000000000000001);
  }

  public double Get(Weights weights, ReadOnlySpan<Incident> incidents)
  {
    RunSimulation(weights, incidents);
    return GetLoss();
  }


  public double Get(Weights weights, ImmutableArray<Incident> incidents)
  {
    return Get(weights, incidents.AsSpan());
  }

  protected void RunSimulation(Weights weights, ReadOnlySpan<Incident> incidents)
  {
    weights.MapTo(Simulation.EmergencyServicePlan);
    Simulation.Run(incidents);
  }
}

