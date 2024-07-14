using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

/// <summary>
/// Finds optimal plan according to given <see cref="UtilityFunction" />.
/// </summary>
public interface IOptimizer
{
  /// <summary>
  /// Utility function, by which plans are evaluated and optimal one is found by.
  /// </summary>
  IUtilityFunction UtilityFunction { get; set; }

  /// <summary>
  /// Gets the best found plans.
  /// </summary>
  List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents);
}

