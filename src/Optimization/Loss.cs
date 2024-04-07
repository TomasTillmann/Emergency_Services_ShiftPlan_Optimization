using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public abstract class Loss : ILoss
{
  protected ISimulation Simulation { get; }
  protected ShiftPlan SimulateOnThisShiftPlan { get; }

  public Loss(World world, int incidentsSize)
  {
    Simulation = new Simulation(world);
    SimulateOnThisShiftPlan = ShiftPlan.GetFrom(world.Depots);
  }

  public abstract double Get(Weights weights, ImmutableArray<SuccessRatedIncidents> incidentsSet);
}

