using ESSP.DataModel;
using Microsoft.Win32.SafeHandles;
using Simulating;

namespace Optimizing;

public abstract class Optimizer : IOptimizer
{
    public Constraints Constraints { get; }

    protected Simulation simulation;
    protected World world;

    public Optimizer(World world, Constraints constraints)
    {
        if (constraints.AllowedShiftStartingTimes.Count() == 0 || constraints.AllowedShiftDurations.Count() == 0)
        {
            throw new ArgumentException("Constraints need to be set.");
        }

        Constraints = constraints;
        this.world = world;
        simulation = new Simulation(world);
    }

    protected virtual int Fitness(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents, out double meanSuccessRate)
    {
        if(!IsValid(shiftPlan, successRatedIncidents, out meanSuccessRate))
        {
            return int.MaxValue;
        }

        return shiftPlan.GetCost();
    }

    protected bool IsValid(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents)
    {
        return IsValid(shiftPlan, successRatedIncidents, out _);
    }

    protected bool IsValid(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents, out double meanSuccessRate)
    {
        double successRateSum = 0;
        bool isValid = true;

        foreach(SuccessRatedIncidents successRatedIncident in successRatedIncidents)
        {
            Statistics stats = simulation.Run(successRatedIncident.Value, shiftPlan);
            shiftPlan.Shifts.ForEach(shift => shift.ClearPlannedIncidents());

            if (stats.SuccessRate < successRatedIncident.SuccessRate)
            {
                successRateSum += stats.SuccessRate;
                isValid = false;
            }
        }

        meanSuccessRate = successRateSum / successRatedIncidents.Count;
        return isValid;
    }

    public abstract IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets);

    protected ShiftPlan GetEmptyShiftPlan()
    {
        return ShiftPlan.ConstructFrom(simulation.Depots, 0.ToSeconds(), 0.ToSeconds());
    }
}

