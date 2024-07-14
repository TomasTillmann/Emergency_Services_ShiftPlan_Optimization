using ESSP.DataModel;

namespace Optimizing;

/// <summary>
/// Utility function, evaluating plans against given incidents set
/// </summary>
public interface IUtilityFunction
{
  public int HandledIncidentsCount { get; }
  public int UnhandledIncidentsCount { get; }
  public int TotalIncidentsCount { get; }
  public int PlanCost { get; }

  public double HandledIncidentsCountScaled { get; }
  public double PlanCostScaled { get; }

  double Evaluate(EmergencyServicePlan plan, ReadOnlySpan<Incident> incidents);
}
