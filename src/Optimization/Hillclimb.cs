using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public class HillClimbOptimizer : LocalSearchOptimizer
{
  public int Steps { get; set; }

  public HillClimbOptimizer
  (
    World world,
    Constraints constraints,
    ILoss loss,
    int neighboursLimit = int.MaxValue,
    int steps = 50,
    Random? random = null
  )
  : base(world, constraints, loss, neighboursLimit, random)
  {
    Steps = steps;
  }

  public override IEnumerable<Weights> FindOptimal(ImmutableArray<SuccessRatedIncidents> incidentsSets)
  {
    Weights currentWeights = StartWeights;
    Weights globalBestWeights = currentWeights;

    double currentBestLoss = Loss.Get(currentWeights, incidentsSets);
    double globalBestLoss = currentBestLoss;

    for (int step = 0; step < Steps; ++step)
    {
      Debug.WriteLine($"globalBestLoss: {globalBestLoss}");

      int neighboursCount = GetMovesToNeighbours(currentWeights);
      Move currentBestMove = Move.Identity;

      for (int i = 0; i < neighboursCount; ++i)
      {
        Move move = movesBuffer[i];

        ModifyMakeMove(currentWeights, move);

        double neighbourLoss = Loss.Get(currentWeights, incidentsSets);
        if (neighbourLoss < currentBestLoss)
        {
          Debug.WriteLine($"curr loss updated to: {neighbourLoss}");
          currentBestMove = move;
          currentBestLoss = neighbourLoss;
        }

        ModifyUnmakeMove(currentWeights, move);
      }

      // In local minima.
      if (currentBestMove.MoveType == Move.Identity.MoveType)
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

      Debug.WriteLine("======");
    }

    return new List<Weights> { globalBestWeights };
  }
}
