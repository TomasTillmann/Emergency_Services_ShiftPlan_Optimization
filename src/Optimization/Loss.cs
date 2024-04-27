using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public abstract class Loss : ILoss
{
  public ISimulation Simulation { get; }

  public Loss(Simulation simulation)
  {
    Simulation = simulation;
  }

  public abstract double Get(Weights weights, ReadOnlySpan<Incident> incidents);

  public abstract double Get(Weights weights, ImmutableArray<Incident> incidents);

  public double GetCost(Weights weights)
  {
    weights.MapTo(Simulation.EmergencyServicePlan);
    return Simulation.EmergencyServicePlan.GetShiftDurationsSum();
  }

  protected void RunSimulation(Weights weights, ReadOnlySpan<Incident> incidents)
  {
    weights.MapTo(Simulation.EmergencyServicePlan);
    Simulation.Run(incidents);
  }
}

