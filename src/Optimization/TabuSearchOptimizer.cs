using ESSP.DataModel;
using System.Collections.Immutable;

namespace Optimizing;

//TODO: support for end the searh when converges to some minima and doesnt want to go anywhere, instead of just iterations
public sealed class TabuSearchOptimizer : LocalSearchOptimizer
{
  #region Params

  public int Iterations { get; set; }
  public int TabuSize { get; set; }
  public Random Random => _random;

  #endregion

  /// <param name="tabuSize">If set too high, it could happen, that all neighbours are tabu (and aspiration criterion is not satisfied either).
  /// <param name="shiftChangesLimit">If count of neighbours is exceeded, only first <paramref name="shiftChangesLimit"/> neighbours will be tried.
  public TabuSearchOptimizer
  (
    World world,
    Constraints constraints,
    ShiftTimes shiftTimes,
    ILoss loss,
    int iterations = 50,
    int tabuSize = 50,
    int shiftChangesLimit = int.MaxValue,
    int allocationsLimit = int.MaxValue,
    Random? random = null
  )
  : base(world, constraints, shiftTimes, loss, shiftChangesLimit, allocationsLimit, random)
  {
    Iterations = iterations;
    TabuSize = tabuSize;
  }

  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    Weights currentWeights = StartWeights;
    Weights globalBestWeights = currentWeights;

    double currentBestLoss = double.MaxValue;
    double globalBestLoss = currentBestLoss;

    Span<Move> tabu = stackalloc Move[TabuSize];
    int cyclicTabuIndex = 0;
    for (int i = 0; i < TabuSize; ++i)
    {
      tabu[i] = new Move
      {
        MoveType = MoveType.NoMove
      };
    }

    for (int step = 0; step < Iterations; ++step)
    {
      //Console.WriteLine($"step: {step}");
      //Debug.WriteLine($"globalBestLoss: {globalBestLoss}");
      currentBestLoss = double.MaxValue;
      Move currentBestMove = default(Move);

      GetMovesToNeighbours(currentWeights);

      for (int i = 0; i < movesBuffer.Count; ++i)
      {
        Move move = movesBuffer[i];

        ModifyMakeMove(currentWeights, move);

        double neighbourLoss = Loss.Get(currentWeights, incidents);
        //Debug.WriteLine($"Neighbour loss: {neighbourLoss}");

        if (neighbourLoss < currentBestLoss)
        {
          bool isInTabu = false;
          for (int tabuIndex = 0; tabuIndex < cyclicTabuIndex; ++tabuIndex)
          {
            if (tabu[tabuIndex] == currentBestMove)
            {
              isInTabu = true;
              break;
            }
          }

          if (isInTabu)
          {
            // aspiration criterion
            if (neighbourLoss < globalBestLoss)
            {
              //Debug.WriteLine($"curr loss updated to: {neighbourLoss}");
              currentBestMove = move;
              currentBestLoss = neighbourLoss;
            }
          }
          else
          {
            //Debug.WriteLine($"curr loss updated to: {neighbourLoss}");
            currentBestMove = move;
            currentBestLoss = neighbourLoss;
          }
        }

        ModifyUnmakeMove(currentWeights, move);
      }

      // All neighbours are tabu and worse than global best. Happens very rarely. 
      if (currentBestLoss == double.MaxValue)
      {
        //Debug.WriteLine($"stuck");
        return new List<Weights> { globalBestWeights };
      }

      // move in the best direction
      //Debug.WriteLine($"made move: {currentBestMove}");
      ModifyMakeMove(currentWeights, currentBestMove);

      // update global best
      if (currentBestLoss < globalBestLoss)
      {
        // Debug.WriteLine($"global best loss updated to: {currentBestLoss}");
        globalBestWeights = currentWeights.Copy();
        globalBestLoss = currentBestLoss;
      }

      // add move to tabu
      tabu[cyclicTabuIndex++] = new Move
      {
        MedicTeamOnDepotIndex = currentBestMove.MedicTeamOnDepotIndex,
        DepotIndex = currentBestMove.DepotIndex,
        MoveType = LocalSearchOptimizer.GetInverseMoveType(currentBestMove.MoveType)
      };
      cyclicTabuIndex %= TabuSize;

      //Debug.WriteLine("======");
    }

    return new List<Weights> { globalBestWeights };
  }

  // private void StepInternal()
  // {
  //   GetMovesToNeighbours(_weights);

  //   //_debug.WriteLine("moves: " + string.Join(", ", movesBuffer));
  //   //Debug.WriteLine("global best loss: " + _globalBestLoss);
  //   //_debug.WriteLine("global best weights: " + string.Join(", ", _globalBestWeights));

  //   _currentBestLoss = double.MaxValue;
  //   for (int i = 0; i < movesBuffer.Count; ++i)
  //   {
  //     Move move = movesBuffer[i];

  //     ModifyMakeMove(_weights, move);
  //     //_debug.WriteLine("neighbour: " + string.Join(", ", _globalBestWeights));

  //     double neighbourLoss = GetLossInternal(_weights);
  //     //_debug.WriteLine("neighbour loss: " + neighbourLoss);

  //     // Not in tabu, or aspiration criterion is satisfied (possibly in tabu).
  //     if (!_tabu.Contains(move) || neighbourLoss < _globalBestLoss)
  //     {
  //       // Is better than current? Always true when aspiration criterion was satisfied.
  //       if (neighbourLoss < _currentBestLoss)
  //       {
  //         _currentBestMove = move;
  //         //_debug.WriteLine("current best move updated to: " + _currentBestMove);

  //         _currentBestLoss = neighbourLoss;
  //         //Debug.WriteLine("current best loss updated to: " + _currentBestLoss);
  //       }
  //     }

  //     ModifyUnmakeMove(_weights, move);
  //   }

  //   // This happens iff all neighbours are in tabu and worse than already found global best.
  //   if (_currentBestLoss == double.MaxValue)
  //   {
  //     //Debug.WriteLine("STUCK");
  //     _isStuck = true;
  //     return;
  //   }

  //   // move in the best direction
  //   ModifyMakeMove(_weights, _currentBestMove);

  //   // update global best
  //   if (_currentBestLoss < _globalBestLoss)
  //   {
  //     _globalBestWeights = _weights.Copy();
  //     //Debug.WriteLine("global best weights updated to: " + string.Join(", ", _globalBestWeights));
  //     //Debug.WriteLine("global best move updated to: " + _currentBestMove);

  //     _globalBestLoss = _currentBestLoss;
  //     //_debug.WriteLine("global best loss updated to: " + _globalBestLoss);
  //   }

  //   // Tabu the inverse move, not the actual move. Only this way, it will not "move back".
  //   _tabu.AddLast
  //   (
  //     new Move
  //     {
  //       MedicTeamOnDepotIndex = _currentBestMove.MedicTeamOnDepotIndex,
  //       MoveType = LocalSearchOptimizer.GetInverseMoveType(_currentBestMove.MoveType)
  //     }
  //   );

  //   if (_tabu.Count > TabuSize)
  //   {
  //     _tabu.RemoveFirst();
  //   }

  //   Debug.WriteLine("==================");
  // }
}
