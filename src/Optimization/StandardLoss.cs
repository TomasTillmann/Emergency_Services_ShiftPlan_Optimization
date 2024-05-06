using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public class StandardLoss : ObjectiveFunction
{
  private readonly double _maxEmergencyServicePlanShiftDuration;
  private readonly double _availableAmbulancesTotalCount;

  /// <summary>
  /// Number between 0 and 1.
  /// How important is success rate. 
  /// </summary>
  public double Alpha { get; set; }

  /// <summary>
  /// Number between 0 and 1.
  /// How important is cost. 
  /// </summary>
  public double Beta { get; set; }

  /// <summary>
  /// Number between 0 and 1.
  /// How important is effectivity. 
  /// </summary>
  public double Gamma { get; set; }

  /// <summary>
  /// Number between 0 and 1.
  /// Defines the ideal exhaustion of a shift plan.
  /// e.g. 0.8 means every medic team should have ideally been 0.8 of assigned shift active.
  /// </summary>
  public double EffectivityTarget { get; set; }

  public StandardLoss(Simulation simulation, ShiftTimes shiftTimes, double alpha = 0.8, double beta = 0.15, double gamma = 0.05, double effectivityTarget = 0.75)
  : base(simulation, shiftTimes)
  {
    Alpha = alpha;
    Beta = beta;
    Gamma = gamma;
    EffectivityTarget = effectivityTarget;
  }

  /// <inheritdoc/>
  public override double GetLoss()
  {
    double successRate = GetSuccessRate();
    double planCost = GetCost();
    // double exhaustion = GetEffectivity();

    double handledPart = Alpha * successRate;
    double costPart = Beta * planCost;
    // double exhaustionError = Gamma * Math.Abs(exhaustion - EffectivityTarget);

    double loss = costPart /* exhaustionError */ - handledPart;

    return loss;
  }
}
