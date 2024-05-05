using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public interface ILoss
{
  /// <summary>
  /// Underlying simulation. 
  /// </summary>
  Simulation Simulation { get; }

  /// <summary>
  /// Get loss from last simulation run.
  /// </summary>
  double GetLoss();

  /// <summary>
  /// Get success rate from last simulation run.
  /// </summary>
  double GetSuccessRate();

  /// <summary>
  /// Get cost from last simulation run.
  /// </summary>
  double GetCost();

  /// <summary>
  /// Get effectivity from last simulation run.
  /// </summary>
  double GetEffectivity();

  /// <summary>
  /// Get loss. Runs the simulation. 
  /// </summary>
  double Get(Weights weights, ReadOnlySpan<Incident> incidents);

  /// <summary>
  /// Get loss. Runs the simulation. 
  /// </summary>
  double Get(Weights weights, ImmutableArray<Incident> incidents);

}
