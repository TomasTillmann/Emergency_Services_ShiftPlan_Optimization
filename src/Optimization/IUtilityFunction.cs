using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public interface IUtilityFunction
{
  int HandledIncidentsCount { get; } 
  int UnhandledIncidentsCount { get; }
  int TotalIncidentsCount { get; }
  int PlanCost { get; }
  double HandledIncidentsCountScaled { get; } 
  double PlanCostScaled { get; }
  double Evaluate(EmergencyServicePlan plan, ReadOnlySpan<Incident> incidents);
}
