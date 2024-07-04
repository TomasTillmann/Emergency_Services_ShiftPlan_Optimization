using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public abstract class UtilityFunctionBase : IUtilityFunction
{
  public int HandledIncidentsCount => _simulation.HandledIncidentsCount;
  public int UnhandledIncidentsCount => _simulation.UnhandledIncidentsCount;
  public int TotalIncidentsCount => _simulation.TotalIncidentsCount;
  public int PlanCost { get; private set; }

  public double HandledIncidentsCountScaled => HandledIncidentsCount / (double)TotalIncidentsCount;
  public double PlanCostScaled => PlanCost / _maxPlanCost;
  
  private readonly double _maxPlanCost;
  private readonly Simulation _simulation;

  public UtilityFunctionBase(Simulation simulation, double maxPlanCost)
  {
    _simulation = simulation;
    _maxPlanCost = maxPlanCost;
  }

  public virtual double Evaluate(EmergencyServicePlan plan, ReadOnlySpan<Incident> incidents)
  {
    PlanCost = plan.Cost;
    _simulation.Run(plan, incidents);
    return Get();
  }

  protected abstract double Get();
}

