using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public class HillClimbOptimizer : LocalSearchOptimizer
{
  public int Steps { get; set; }

  public bool ContinueIfStuck { get; }

  public HillClimbOptimizer
  (
    World world,
    Constraints constraints,
    ShiftTimes shiftTimes,
    ILoss loss,
    bool continueIfStuck = false,
    bool shouldPermutate = true,
    int neighboursLimit = int.MaxValue,
    int iterations = 50,
    Random? random = null
  )
  : base(world, constraints, shiftTimes, loss, shouldPermutate, neighboursLimit, random)
  {
    Steps = iterations;
    ContinueIfStuck = continueIfStuck;
  }

  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    Weights currentWeights = StartWeights;
    Weights globalBestWeights = currentWeights;

    Visualizer visualizer = new(Debug);

    double currentBestLoss = Loss.Get(currentWeights, incidents);
    double globalBestLoss = currentBestLoss;

    for (int step = 0; step < Steps; ++step)
    {
      Debug.WriteLine($"step: {step}");
      //Debug.WriteLine($"globalBestLoss: {globalBestLoss}");

      GetMovesToNeighbours(currentWeights);
      //Debug.WriteLine("---moves");
      //Debug.WriteLine(string.Join("\n", movesBuffer));
      //Debug.WriteLine("---moves");
      Debug.WriteLine("moves count: " + movesBuffer.Count);
      Move currentBestMove = Move.Identity;
      Debug.Flush();

      for (int i = 0; i < movesBuffer.Count; ++i)
      {
        Move move = movesBuffer[i];

        ModifyMakeMove(currentWeights, move);

        double neighbourLoss = Loss.Get(currentWeights, incidents);
        //Debug.WriteLine($"--- {move}, {neighbourLoss}"); // SPAM

        if (neighbourLoss < currentBestLoss)
        {
          //Debug.WriteLine($"curr loss updated to: {neighbourLoss}");
          currentBestMove = move;
          currentBestLoss = neighbourLoss;
        }

        ModifyUnmakeMove(currentWeights, move);
      }

      // In local minima.
      if (currentBestMove.MoveType == Move.Identity.MoveType && !ContinueIfStuck)
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
        //Debug.WriteLine($"global best loss updated to: {currentBestLoss}");
        globalBestWeights = currentWeights.Copy();
        globalBestLoss = currentBestLoss;
      }

      //visualizer.PlotGraph(Loss, currentWeights, incidents, Debug);
      //Debug.WriteLine("======");
    }

    return new List<Weights> { globalBestWeights };
  }
}
