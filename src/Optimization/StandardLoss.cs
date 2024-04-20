using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public class StandardLoss : Loss
{
  private readonly double _maxEmergencyServicePlanShiftDuration;
  private readonly double _availableAmbulancesTotalCount;

  /// <summary>
  /// Number between 0 and 1.
  /// By how much should be preferred success rate over cost.
  /// </summary>
  public double Alpha { get; set; }

  /// <summary>
  /// Number between 0 and 1.
  /// How important is effectivity.
  /// </summary>
  public double Beta { get; set; }

  /// <summary>
  /// Number between 0 and 1.
  /// Defines the ideal exhaustion of a shift plan.
  /// e.g. 0.8 means every medic team should have ideally been 0.8 of assigned shift active.
  /// </summary>
  public double EffectivityTarget { get; set; }

  public StandardLoss(Simulation simulation, ShiftTimes shiftTimes, double alpha = 0.9, double beta = 0.6, double effectivityTarget = 0.75)
  : base(simulation)
  {
    Alpha = alpha;
    Beta = beta;
    EffectivityTarget = effectivityTarget;
    _maxEmergencyServicePlanShiftDuration = simulation.World.AvailableMedicTeams.Length * shiftTimes.MaxDurationSec / 60;
    _availableAmbulancesTotalCount = simulation.World.AvailableAmbulances.Length;
  }

  public override double Get(Weights weights, Incidents incidents)
  {
    RunSimulation(weights, incidents);

    double exhaustion = Simulation.EmergencyServicePlan.GetExhaustionSum() / Simulation.EmergencyServicePlan.GetShiftDurationsSum();
    double planCost = Simulation.EmergencyServicePlan.GetCost() / (_maxEmergencyServicePlanShiftDuration + _availableAmbulancesTotalCount);
    double handled = Simulation.SuccessRate;

    double handledPart = Alpha * handled;
    double costPart = (1 - Alpha) * planCost;
    double exhaustionError = Beta * Math.Abs(exhaustion - EffectivityTarget);

    double loss = costPart + exhaustionError - handledPart;

    return loss;
  }
}
