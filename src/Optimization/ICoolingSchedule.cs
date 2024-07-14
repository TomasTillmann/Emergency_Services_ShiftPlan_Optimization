namespace Optimizing;

/// <summary>
/// Cooling schedule for simulated annealing.
/// </summary>
public interface ICoolingSchedule
{
  /// <summary>
  /// Gets the new temperature.
  /// </summary>
  double Calculate(double currentTemp);
}



