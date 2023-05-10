using ESSP.DataModel;

namespace Optimizing;

public interface IOptimizer
{
    public ShiftPlan FindOptimal(ShiftPlan shiftPlan, List<IncidentsSet> incidentsSets);
}

