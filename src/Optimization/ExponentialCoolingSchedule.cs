using System.Diagnostics;

namespace Optimizing;

/// <summary>
/// Exponential cooling schedule for simulated annealing optimizer.
/// </summary>
public class ExponentialCoolingSchedule : ICoolingSchedule
{
  /// <summary>
  /// By what percentage to decrease the temperature. Has to be in (0, 1) interval.
  /// </summary>
  public double Rate { get; set; }

  public ExponentialCoolingSchedule(double rate)
  {
    Debug.Assert(rate > 0 && rate < 1);
    Rate = rate;
  }

  /// <inheritdoc />
  public double Calculate(double currentTemp)
  {
    return Rate * currentTemp;
  }
}




