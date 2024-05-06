using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public class RandomWalkOptimizer : LocalSearchOptimizer
{
  public int Iterations { get; set; }

  public RandomWalkOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, IObjectiveFunction loss, int iterations, bool shouldPermutate = true, int neighboursLimit = 1, Random? random = null)
  : base(world, constraints, shiftTimes, loss, shouldPermutate, neighboursLimit, random)
  {
    Iterations = iterations;
  }

  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    double bestLoss = double.MaxValue;
    Weights bestWeights = default(Weights);

    Weights weights = StartWeights;

    for (int step = 0; step < Iterations; ++step)
    {
      Debug.WriteLine($"Step: {step}");

      GetMovesToNeighbours(weights);
      //Debug.WriteLine($"{movesBuffer.Count}");
      //Debug.WriteLine($"{string.Join(", ", movesBuffer)}");
      Move move = movesBuffer[_random.Next(0, movesBuffer.Count)];
      //Debug.WriteLine($"{move}");
      ModifyMakeMove(weights, move);
      double currentLoss = ObjectiveFunction.Get(weights, incidents);

      if (currentLoss < bestLoss)
      {
        bestLoss = currentLoss;
        bestWeights = weights.Copy();
      }
    }

    return new List<Weights> { bestWeights };
  }
}


