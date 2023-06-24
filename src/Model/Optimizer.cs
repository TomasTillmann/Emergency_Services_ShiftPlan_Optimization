using ESSP.DataModel;
using Microsoft.Win32.SafeHandles;
using Simulating;

namespace Optimizing;

public abstract class Optimizer : IOptimizer
{
    public Domain Constraints { get; }
    public World World { get; }

    protected Simulation Simulation;

    public Optimizer(World world, Domain constraints)
    {
        if (constraints.AllowedShiftStartingTimes.Count() == 0 || constraints.AllowedShiftDurations.Count() == 0)
        {
            throw new ArgumentException("Constraints need to be set.");
        }

        Constraints = constraints;
        this.World = world;
        Simulation = new Simulation(world);
    }

    public virtual int Fitness(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents)
    {
        return Fitness(shiftPlan, successRatedIncidents, out _);
    }

    public virtual int Fitness(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents, out double meanSuccessRate)
    {
        if(!IsValid(shiftPlan, successRatedIncidents, out meanSuccessRate))
        {
            return int.MaxValue;
        }

        return shiftPlan.GetCost();
    }

    public bool IsValid(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents)
    {
        return IsValid(shiftPlan, successRatedIncidents, out _);
    }

    public bool IsValid(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents, out double meanSuccessRate)
    {
        double successRateSum = 0;
        bool isValid = true;

        foreach(SuccessRatedIncidents successRatedIncident in successRatedIncidents)
        {
            Statistics stats = Simulation.Run(successRatedIncident.Value, shiftPlan);
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
        return ShiftPlan.ConstructFrom(Simulation.Depots, 0.ToSeconds(), 0.ToSeconds());
    }
}

