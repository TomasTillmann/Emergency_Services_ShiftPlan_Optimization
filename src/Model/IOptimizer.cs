using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public interface IOptimizer
{
    IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets);
    int Fitness(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents);
    int Fitness(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents, out double meanSuccessRate);
    bool IsValid(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents);
    bool IsValid(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents, out double meanSuccessRate);
}

