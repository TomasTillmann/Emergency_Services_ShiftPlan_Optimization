using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public abstract class UtilityFunctionBase : IUtilityFunction
{
  protected int HandledIncidentsCount => _simulation.HandledIncidentsCount;
  protected int UnhandledIncidentsCount => _simulation.UnhandledIncidentsCount;
  protected int TotalIncidentsCount => _simulation.TotalIncidentsCount;
  protected int PlanCost { get; private set; }

  protected double HandledIncidentsCountScaled => HandledIncidentsCount / (double)TotalIncidentsCount;
  protected double PlanCostScaled => PlanCost / _maxPlanCost;
  private readonly double _maxPlanCost;

  private readonly Simulation _simulation;

  public UtilityFunctionBase(Simulation simulation, double maxPlanCost)
  {
    _simulation = simulation;
    _maxPlanCost = maxPlanCost;
  }

  public virtual double Get(EmergencyServicePlan plan, ReadOnlySpan<Incident> incidents)
  {
    PlanCost = plan.Cost;
    _simulation.Run(plan, incidents);
    return Get();
  }

  protected abstract double Get();
}

