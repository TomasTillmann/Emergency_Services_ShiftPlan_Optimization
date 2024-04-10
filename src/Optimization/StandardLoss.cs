using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public class StandardLoss : Loss
{
  private readonly double _maxShiftPlanCost;

  public StandardLoss(World world, Constraints constraints)
  : base(world, constraints)
  {
    _maxShiftPlanCost = world.AllAmbulancesCount * constraints.MaxDurationSec / 24 / 24 * world.AmbTypes.Select(t => t.Cost).Max();
  }

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

    //HACK:
    var incidents = incidentsSet.First();
    //

    Simulation.Run(incidents.Value, SimulateOnThisShiftPlan);

    double cost = SimulateOnThisShiftPlan.GetCost() / _maxShiftPlanCost;
    double handled = Simulation.SuccessRate;
    double thresh = incidents.SuccessRate;

    double eps = 0.001;
    double handledPart = (thresh + eps) * handled;
    double costPart = (1 - thresh + eps) * cost;
    double loss = costPart - handledPart;

    return loss;
  }
}
