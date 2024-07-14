using Simulating;

namespace Optimizing;

/// <summary>
/// Goal programming using euclidian norm.
/// </summary>
public class EuclidianGoalProgramming : UtilityFunctionBase
{
    public EuclidianGoalProgramming(Simulation simulation, double maxPlanCost)
        : base(simulation, maxPlanCost)
    {
    }

    /// <inheritdoc />
    protected override double Get()
    {
        return Math.Sqrt(Math.Pow(1 - HandledIncidentsCountScaled, 2) + Math.Pow(PlanCostScaled, 2));
    }
}