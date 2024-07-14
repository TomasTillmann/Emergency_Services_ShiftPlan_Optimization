using Simulating;

namespace Optimizing;

/// <summary>
/// Weighted exponential sum utility function with euclidian norm.
/// </summary>
public class EuclidianWeightedExponentialSum : UtilityFunctionBase
{
    /// <summary>
    /// Alpha
    /// </summary>
    public double Alpha { get; set; }
    
    public EuclidianWeightedExponentialSum(Simulation simulation, double maxPlanCost, double alpha)
        : base(simulation, maxPlanCost)
    {
        Alpha = alpha;
    }

    /// <inheritdoc />
    protected override double Get()
    {
        return Alpha * Math.Pow((1 - HandledIncidentsCountScaled), Alpha) + (1 - Alpha) * Math.Pow(PlanCostScaled, Alpha);
    }
}
