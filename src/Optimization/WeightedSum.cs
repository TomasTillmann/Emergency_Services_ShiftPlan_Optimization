using Simulating;

namespace Optimizing;

/// <summary>
/// Implementation of weighted sum utility function.
/// </summary>
public class WeightedSum : UtilityFunctionBase
{
  public double Alpha { get; set; }

  public WeightedSum(Simulation simulation, double maxPlanCost, double alpha = 0.99)
  : base(simulation, maxPlanCost)
  {
    Alpha = alpha;
  }

  protected override double Get()
  {
    return Alpha * HandledIncidentsCountScaled - (1 - Alpha) * PlanCostScaled;
  }
}

