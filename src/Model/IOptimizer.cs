using ESSP.DataModel;

namespace Optimizing;

public interface IOptimizer
{
    public IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets);
}

