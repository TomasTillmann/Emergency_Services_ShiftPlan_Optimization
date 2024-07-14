using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public abstract class UtilityFunctionBase : IUtilityFunction
{
  /// <summary>
  /// Total handled incidents count by the simulation on evaluated plan.
  /// </summary>
  public int HandledIncidentsCount => _simulation.HandledIncidentsCount;
  
  /// <summary>
  /// Total unhandled incidents count by the simulation on evaluated plan.
  /// </summary>
  public int UnhandledIncidentsCount => _simulation.UnhandledIncidentsCount;
  
  /// <summary>
  /// Total incidents count.
  /// </summary>
  public int TotalIncidentsCount => _simulation.TotalIncidentsCount;
  
  /// <summary>
  /// Plans cost, based on shift durations, team and ambulances counts.
  /// </summary>
  public int PlanCost { get; private set; }

  /// <summary>
  /// Precalculated Handled incidents count, scaled to <0,1>
  /// </summary>
  public double HandledIncidentsCountScaled => HandledIncidentsCount / (double)TotalIncidentsCount;
  
  /// <summary>
  /// Plans cost, based on shift durations, team and ambulances count. Scaled to <0,1> interval.
  /// </summary>
  public double PlanCostScaled => PlanCost / _maxPlanCost;
  
  private readonly double _maxPlanCost;
  private readonly Simulation _simulation;

  public UtilityFunctionBase(Simulation simulation, double maxPlanCost)
  {
    _simulation = simulation;
    _maxPlanCost = maxPlanCost;
  }

  /// <summary>
  /// Gets the evaluation of this utility function on <paramref name="plan"/> and <paramref name="incidents"/>.
  /// </summary>
  public virtual double Evaluate(EmergencyServicePlan plan, ReadOnlySpan<Incident> incidents)
  {
    PlanCost = plan.Cost;
    _simulation.Run(plan, incidents);
    return Get();
  }

  /// <summary>
  /// Specific implementation.
  /// Simulation has alraedy been called before calling this method, so the stats are up to date.
  /// </summary>
  protected abstract double Get();
}

