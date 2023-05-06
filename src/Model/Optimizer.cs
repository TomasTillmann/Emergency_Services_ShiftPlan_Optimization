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
        Statistics stats;
        shiftPlan.ModifyToLargest(Constraints);

        foreach(Incidents incident in incidents)
        {
            stats = simulation.Run(incident.Value, shiftPlan);
        }
    }
}