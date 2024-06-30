namespace Optimizing;

using System;
using System.Collections.Immutable;
using ESSP.DataModel;
using Simulating;

public class LexObjectiveFunction
{
  public Simulation Simulation { get; }
  public LexObjectiveFunction(Simulation simulation)
  {
    Simulation = simulation;
  }

  public int GetLoss(EmergencyServicePlan plan1, EmergencyServicePlan plan2, ReadOnlySpan<Incident> incidents)
  {
    Simulation.EmergencyServicePlan = plan1;
    Simulation.Run(incidents);
    int c1 = Simulation.HandledIncidentsCount;

    Simulation.EmergencyServicePlan = plan2;
    Simulation.Run(incidents);
    int c2 = Simulation.HandledIncidentsCount;

    double cost1 = plan1.GetCost();
    double cost2 = plan2.GetCost();

    // first plan is better
    if ((c1 > c2) || (c1 == c2 && cost1 < cost2))
    {
      return -1;
    }
    // equally good
    if (c1 == c2 && cost1 == cost2)
    {
      return 0;
    }
    else
    {
      return 1;
    }
  }
}


