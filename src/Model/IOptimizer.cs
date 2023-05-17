using ESSP.DataModel;

namespace Optimizing;

public interface IOptimizer
{
    public IEnumerable<ShiftPlan> FindOptimal(ShiftPlan shiftPlan, List<IncidentsSet> incidentsSets);
}

