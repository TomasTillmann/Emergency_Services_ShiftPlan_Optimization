using ESSP.DataModel;
using Optimizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization;

public class SimulatedAnnealingOptimizer : MetaheuristicOptimizer
{
    public SimulatedAnnealingOptimizer(World world, Domain constraints) : base(world, constraints)
    {
    }

    public override IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets)
    {
        throw new NotImplementedException();
    }
}
