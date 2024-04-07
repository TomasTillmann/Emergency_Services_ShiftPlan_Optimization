using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public class StandardLoss : Loss
{
  public StandardLoss(World world, int incidentsSize)
  : base(world, incidentsSize) { }

  public override double Get(Weights weights, ImmutableArray<SuccessRatedIncidents> incidentsSet)
  {
    //TODO: dont overwrite all weight always, but only those that actually changed
    // create shift plan according to weights
    for (int i = 0; i < weights.Value.Length; ++i)
    {
      Shift shift = SimulateOnThisShiftPlan.Shifts[i];
      shift.Work = weights.Value[i];
    }
    //

    //HACK: do for all incidentsSet
    Simulation.Run(incidentsSet.First().Value, SimulateOnThisShiftPlan);

    double threshold = incidentsSet.First().SuccessRate;
    double successRate = Simulation.SuccessRate;
    double cost = SimulateOnThisShiftPlan.GetCost();

    // TODO:
    // We need a loss which:
    // 1. Is lower when cost is lower and higher when cost is higher.
    // 2. Is lower when successRate is higher and higher when successRate is lower
    // 3. Prioritize successRate over cost. SuccessRate needs to be higher than threshold, even if it means much costlier shiftPlan.
    // 4. Have high entropy.

    //HACK:
    // big jump, some smoother function wigh high entropy will be much better

    if (successRate >= threshold)
    {
      return cost;
    }

    return (int.MaxValue / 2) + (1 - successRate) * cost;
  }
}
