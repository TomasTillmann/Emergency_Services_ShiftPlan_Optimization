using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

/// <summary>
/// Implement this interface if you want your optimizer to be debugged step by step, and to expose vital parts of it's changing internal state to the caller.
/// Implementation of this interface is not supposed to be fast, but for the caller to be able to debug and see what 
/// is going on inside the optimizer after each step.
/// </summary>
public interface IStepOptimizer : IOptimizer
{
  /// <summary>
  /// Found optimal shift plans. Is set only after <see cref="IsFinished" />.
  /// </summary>
  IEnumerable<Weights> OptimalWeights { get; }

  /// <summary>
  /// Current step number.
  /// </summary>
  int CurrStep { get; }

  /// <summary>
  /// Call before first call to <see cref="Step" />.
  /// Initializes / resets internal state of optimizer appropriately.
  /// </summary>
  void InitStepOptimizer(ImmutableArray<SuccessRatedIncidents> incidentsSets);

  /// <summary>
  /// Does one step of the optimizer. 
  /// </summary>
  void Step();

  /// <summary>
  /// Whether the optimizer finished and optimalShift is found and initialized.
  /// </summary>
  /// <returns></returns>
  bool IsFinished();
}
