using ESSP.DataModel;

namespace Optimizing;

public abstract class MetaheuristicOptimizer : Optimizer
{
    protected int MaxShiftPlanCost { get; private set; }

    protected MetaheuristicOptimizer(World world, Domain constraints) : base(world, constraints)
    {
        // TODO: Doesnt take into acount ambulance type
        ShiftPlan maximalShiftPlan = ShiftPlan.ConstructFrom(world.Depots, 0.ToSeconds(), constraints.AllowedShiftDurations.Max());
        MaxShiftPlanCost = maximalShiftPlan.GetCost();
    }

    public int DampedFitness(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents)
    {
        int eval = base.Fitness(shiftPlan, successRatedIncidents, out double meanSuccessRate);

        // damping, to better navigate
        if (eval == int.MaxValue)
        {
            return MaxShiftPlanCost + (int)((1 - meanSuccessRate) * 100);
            //return int.MaxValue;
        }

        return eval;
    }
}
