using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public class LexComparer
{
  private readonly Simulation _simulation;

  public LexComparer(World world, Constraints constraints, IDistanceCalculator distanceCalculator)
  {
    _simulation = new Simulation(world, constraints, distanceCalculator);
  }

  public int Compare(EmergencyServicePlan p1, EmergencyServicePlan p2, ImmutableArray<Incident> incidents)
  {
    _simulation.Run(p1, incidents.AsSpan());
    int handled1 = _simulation.HandledIncidentsCount;
    int cost1 = p1.Cost;

    _simulation.Run(p2, incidents.AsSpan());
    int handled2 = _simulation.HandledIncidentsCount;
    int cost2 = p2.Cost;

    if (handled1 > handled2 || (handled1 == handled2 && cost1 < cost2))
    {
      return -1;
    }
    else if (handled1 == handled2 && cost1 == cost2)
    {
      return 0;
    }
    else
    {
      return 1;
    }
  }
}

