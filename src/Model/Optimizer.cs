using DataModel.Interfaces;
using ESSP.DataModel;
using Model.Extensions;
using Simulating;

namespace Optimizing;

public class Optimizer
{
    public Constraints Constraints { get; }

    private Simulation simulation;

    public Optimizer(World world, IDistanceCalculator distanceCalculator, Constraints constraints)
    {
        Constraints = constraints;
        simulation = new Simulation(world, distanceCalculator);
    }

    public void FindOptimal(ShiftPlan shiftPlan, List<Incidents> incidents)
    {
        shiftPlan = LargestShiftPlan(shiftPlan);
        foreach(Incidents incident in incidents)
        {
            simulation.Run(incident.Value, shiftPlan);
        }
    }

    private ShiftPlan LargestShiftPlan(ShiftPlan shiftPlan)
    {
        Seconds largestDuration = Constraints.AllowedShiftDurations.FindMaxSubset(_ => _).First();

        foreach (Shift shift in shiftPlan.Shifts)
        {
            shift.Work = Interval.GetByStartAndDuration(0.ToSeconds(), largestDuration);
        }

        return shiftPlan;
    }
}