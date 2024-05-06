using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public class RandomSampleHillClimbOptimizer : Optimizer
{
  private readonly HillClimbOptimizer _hillClimbOptimizer;

  public int Iterations { get; }
  public int NeighboursLimit { get; }
  public bool ContinueIfStuck { get; }

  public int SamplesCount { get; set; }

  public RandomSampleHillClimbOptimizer(
      World world,
      Constraints constraints,
      ShiftTimes shiftTimes,
      IObjectiveFunction loss,
      int samples,
      int iterations,
      bool continueIfStuck = false,
      int neighboursLimit = int.MaxValue,
      Random? random = null)
  : base(world, constraints, shiftTimes, loss, random)
  {
    _hillClimbOptimizer = new(world, constraints, shiftTimes, loss, continueIfStuck: continueIfStuck, neighboursLimit: neighboursLimit, iterations: iterations, random: random);
    Iterations = iterations;
    NeighboursLimit = neighboursLimit;
    ContinueIfStuck = continueIfStuck;
    SamplesCount = samples;
  }

  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    Visualizer visualizer = new(Debug);

    Weights bestWeights = default(Weights);
    double bestLoss = double.MaxValue;

    for (int i = 0; i < SamplesCount; ++i)
    {
      _hillClimbOptimizer.StartWeights = Weights.GetUniformlyRandom(World, Constraints, ShiftTimes, _random);
      Weights optimal = _hillClimbOptimizer.FindOptimal(incidents).First();
      double currentLoss = ObjectiveFunction.Get(optimal, incidents);

      if (currentLoss < bestLoss)
      {
        bestLoss = currentLoss;
        bestWeights = optimal.Copy();
      }

      visualizer.PlotGraph(ObjectiveFunction, optimal, incidents, Debug);
    }

    return new List<Weights> { bestWeights };
  }
}


