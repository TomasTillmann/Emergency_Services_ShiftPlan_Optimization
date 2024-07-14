using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using DataModel.Interfaces;
using DistanceAPI;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

/// <summary>
/// Implementation of tabu search optimizer.
/// </summary>
public class TabuSearchOptimizer : NeighbourOptimizer
{
  /// <summary>
  /// Plan from which to start the local search.
  /// By default is set to empty plan.
  /// </summary>
  public EmergencyServicePlan StartPlan { get; set; }
  
  /// <summary>
  /// Size of tabu.
  /// </summary>
  public int TabuTenure { get; set; }
  
  /// <summary>
  /// The iteration at which the local search plateud, meaning, it found the local optima.
  /// </summary>
  public int PlateuIteration { get; set; }
  
  
  /// <summary>
  /// Maximum number of iterations allowed to make.
  /// </summary>
  public int MaxIterations { get; set; }

  private readonly MoveMaker _moveMaker = new();

  public TabuSearchOptimizer(World world, Constraints constraints, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator, int tabuTenure, int maxIterations = int.MaxValue)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
    MaxIterations = maxIterations;
    TabuTenure = tabuTenure;
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
  }

  /// <inheritdoc />
  public override List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    HashSet<MoveSequence> tabu = new(TabuTenure, new MoveSequenceComparer());
    MoveSequence[] tabuQueue = new MoveSequence[TabuTenure];
    int tabuQueueIndex = -1;

    EmergencyServicePlan current = EmergencyServicePlan.GetNewEmpty(World);
    current.FillFrom(StartPlan);

    EmergencyServicePlan bestPlan = EmergencyServicePlan.GetNewEmpty(World);
    bestPlan.FillFrom(current);
    double bestPlanEval = UtilityFunction.Evaluate(bestPlan, incidents.AsSpan());

    MoveSequenceDuo bestMove = MoveSequenceDuo.GetNewEmpty(MoveGenerator.MovesBufferSize);
    for (PlateuIteration = 0; PlateuIteration < MaxIterations; ++PlateuIteration)
    {
      double bestNeighborEval = double.MinValue;
      int neighbor = 0;
      foreach (var move in MoveGenerator.GetMoves(current))
      {
        _moveMaker.ModifyMakeMove(current, move.Normal);

        double neighbourEval = UtilityFunction.Evaluate(current, incidents.AsSpan());
        if (neighbourEval > bestNeighborEval && !tabu.Contains(move.Normal))
        {
          bestNeighborEval = neighbourEval;
          bestMove.FillFrom(move);
        }
        else if (neighbourEval > bestPlanEval)
        {
          bestNeighborEval = neighbourEval;
          bestMove.FillFrom(move);
        }

        _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
      }

      // plateu
      if (bestNeighborEval == double.MinValue)
      {
        return [bestPlan];
      }

      _moveMaker.ModifyMakeMove(current, bestMove.Normal);

      if (bestNeighborEval > bestPlanEval)
      {
        bestPlan.FillFrom(current);
        bestPlanEval = bestNeighborEval;
      }

      int position = (tabuQueueIndex + 1) % TabuTenure;
      tabu.Remove(tabuQueue[position]);
      MoveSequence bestMoveCopy = MoveSequence.GetNewFrom(bestMove.Inverse);
      tabu.Add(bestMoveCopy);
      tabuQueueIndex = position;
      tabuQueue[tabuQueueIndex] = bestMoveCopy;
    }

    return [bestPlan];
  }
}

