using ESSP.DataModel;
using System.Collections.Immutable;

namespace Optimizing;

public sealed class TabuSearchOptimizer : LocalSearchOptimizer
{
  #region Params

  public int Iterations { get; set; }
  public int TabuSize { get; set; }
  public Random Random => _random;

  #endregion

  /// <param name="tabuSize">If set too high, it could happen, that all neighbours are tabu (and aspiration criterion is not satisfied either). Also it is slower.
  /// <param name="neighboursLimit">If count of neighbours is exceeded, only first <paramref name="neighboursLimit"/> neighbours will be tried, permutated.
  public TabuSearchOptimizer
  (
    World world,
    Constraints constraints,
    ShiftTimes shiftTimes,
    ILoss loss,
    int iterations = 50,
    int tabuSize = 50,
    bool shouldPermutate = true,
    int neighboursLimit = int.MaxValue,
    Random? random = null
  )
  : base(world, constraints, shiftTimes, loss, shouldPermutate, neighboursLimit, random)
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
      Debug.WriteLine($"step: {step}");
      Debug.WriteLine($"globalBestLoss: {globalBestLoss}");
      currentBestLoss = double.MaxValue;
      Move currentBestMove = default(Move);

      GetMovesToNeighbours(currentWeights);
      Debug.WriteLine(string.Join(", ", movesBuffer));

      for (int i = 0; i < movesBuffer.Count; ++i)
      {
        Move currentMove = movesBuffer[i];

        ModifyMakeMove(currentWeights, currentMove);

        double neighbourLoss = Loss.Get(currentWeights, incidents);
        // Debug.WriteLine($"Neighbour loss: {neighbourLoss}"); // SPAM

        if (neighbourLoss < currentBestLoss)
        {
          bool isInTabu = false;
          for (int tabuIndex = 0; tabuIndex < TabuSize; ++tabuIndex)
          {
            if (tabu[tabuIndex] == currentMove)
            {
              isInTabu = true;
              break;
            }
          }

          if (isInTabu)
          {
            Debug.WriteLine($"move: {currentMove} is in tabu");
            // aspiration criterion
            if (neighbourLoss < globalBestLoss)
            {
              Debug.WriteLine($"curr loss updated to - aspiration criterion satisfied: {neighbourLoss}");
              currentBestMove = currentMove;
              currentBestLoss = neighbourLoss;
            }
          }
          else
          {
            Debug.WriteLine($"curr loss updated to: {neighbourLoss}");
            currentBestMove = currentMove;
            currentBestLoss = neighbourLoss;
          }
        }

        ModifyUnmakeMove(currentWeights, currentMove);
      }

      // All neighbours are tabu and worse than global best. Happens very rarely. 
      if (currentBestLoss == double.MaxValue)
      {
        Debug.WriteLine($"stuck");
        return new List<Weights> { globalBestWeights };
      }

      // move in the best direction
      Debug.WriteLine($"made move: {currentBestMove}");
      ModifyMakeMove(currentWeights, currentBestMove);

      // update global best
      if (currentBestLoss < globalBestLoss)
      {
        Debug.WriteLine($"global best loss updated to: {currentBestLoss}");
        globalBestWeights = currentWeights.Copy();
        globalBestLoss = currentBestLoss;
      }

      // add move to tabu
      tabu[cyclicTabuIndex++] = new Move
      {
        OnDepotIndex = currentBestMove.OnDepotIndex,
        DepotIndex = currentBestMove.DepotIndex,
        MoveType = LocalSearchOptimizer.GetInverseMoveType(currentBestMove.MoveType)
      };
      cyclicTabuIndex %= TabuSize;
      Debug.WriteLine($"tabu: {string.Join(", ", tabu.ToImmutableArray())}");

      Debug.WriteLine("======");
    }

    return new List<Weights> { globalBestWeights };
  }
}
