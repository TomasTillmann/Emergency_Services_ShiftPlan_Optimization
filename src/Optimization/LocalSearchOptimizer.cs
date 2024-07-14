
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using DataModel.Interfaces;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

/// <summary>
/// Local search optimizier implementation.
/// </summary>
public class LocalSearchOptimizer : NeighbourOptimizer
{
  /// <summary>
  /// Plan from which to start the local search.
  /// By default is set to empty plan.
  /// </summary>
  public EmergencyServicePlan StartPlan { get; set; }
  
  /// <summary>
  /// The iteration at which the local search plateud, meaning, it found the local optima.
  /// </summary>
  public int PlateuIteration { get; private set; }
  
  /// <summary>
  /// Maximum number of iterations allowed to make.
  /// </summary>
  public int MaxIterations { get; set; }

  public LocalSearchOptimizer(int maxIterations, World world, Constraints constraints, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
    MaxIterations = maxIterations;
  }

  /// <inheritdoc />
  public override List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    EmergencyServicePlan current = EmergencyServicePlan.GetNewEmpty(World);
    current.FillFrom(StartPlan);

    MoveSequence bestMove = MoveSequence.GetNewEmpty(MoveGenerator.MovesBufferSize);
    for (PlateuIteration = 0; PlateuIteration < MaxIterations; ++PlateuIteration)
    {
      bestMove.Count = 0;
      double bestEval = UtilityFunction.Evaluate(current, incidents.AsSpan());

      foreach (MoveSequenceDuo moves in MoveGenerator.GetMoves(current))
      {
        ModifyMakeMove(current, moves.Normal);

        double neighbourEval = UtilityFunction.Evaluate(current, incidents.AsSpan());
        if (neighbourEval >= bestEval)
        {
          bestMove.FillFrom(moves.Normal);
          bestEval = neighbourEval;
        }

        ModifyMakeInverseMove(current, moves.Inverse);
      }

      // plateu
      if (bestMove.Count == 0)
      {
        break;
      }

      ModifyMakeMove(current, bestMove);
    }
    return new List<EmergencyServicePlan>()
    {
      current
    };
  }
}
