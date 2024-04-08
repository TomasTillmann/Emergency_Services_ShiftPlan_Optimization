using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public abstract class Loss : ILoss
{
  protected ISimulation Simulation { get; }
  protected ShiftPlan SimulateOnThisShiftPlan { get; }
  protected Constraints Constraints { get; }

  public Loss(World world, Constraints constraints)
  {
    Simulation = new Simulation(world);
    SimulateOnThisShiftPlan = ShiftPlan.GetFrom(world.Depots);
    Constraints = constraints;
  }

  public abstract double Get(Weights weights, ImmutableArray<SuccessRatedIncidents> incidentsSet);
}

