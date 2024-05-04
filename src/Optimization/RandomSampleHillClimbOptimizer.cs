using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public class RandomSampleHillClimbOptimizer : Optimizer
{
  private readonly HillClimbOptimizer _hillClimbOptimizer;

  public int SamplesCount { get; set; }
  public int Iterations { get; }

  public RandomSampleHillClimbOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, ILoss loss, int samples, int iterations, Random? random = null)
  : base(world, constraints, shiftTimes, loss, random)
  {
    _hillClimbOptimizer = new(world, constraints, shiftTimes, loss, iterations: iterations, random: random);
    SamplesCount = samples;
    Iterations = iterations;
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
      double currentLoss = Loss.Get(optimal, incidents);

      if (currentLoss < bestLoss)
      {
        bestLoss = currentLoss;
        bestWeights = optimal;
      }

      visualizer.PlotGraph(Loss, optimal, incidents, Debug);
    }

    return new List<Weights> { bestWeights };
  }
}


