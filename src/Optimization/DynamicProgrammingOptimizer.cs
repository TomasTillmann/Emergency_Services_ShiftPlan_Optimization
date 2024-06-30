namespace Optimizing;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ESSP.DataModel;

struct OptimalMove
{
  public EmergencyServicePlan Make(EmergencyServicePlan p)
  {
    throw new NotImplementedException();
  }
}

public class DynamicProgrammingOptimizer : LocalSearchOptimizer
{
  public DynamicProgrammingOptimizer(
      World world,
      Constraints constraints,
      ShiftTimes shiftTimes,
      IObjectiveFunction loss,
      bool shouldPermutate = true,
      int neighboursLimit = int.MaxValue,
      Random? random = null)
  : base(world, constraints, shiftTimes, loss, shouldPermutate, neighboursLimit, random) { }

  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
  }

  private Dictionary<(int, EmergencyServicePlan), List<EmergencyServicePlan>> cache = new();
  private List<EmergencyServicePlan> OptimalMovesSearch(EmergencyServicePlan p, ReadOnlySpan<Incident> incidents, int k, int h)
  {
    if (k == h)
    {
      return new() { p };
    }
    else if (cache.ContainsKey((k, p)))
    {
      return cache[(k, p)];
    }

    List<EmergencyServicePlan> optimalSoFar = new();
    double eval = double.MinValue;

    IEnumerable<OptimalMove> optimalMoves = GenerateOptimalMoves(p, incidents.Slice(0, k));
    foreach (OptimalMove move in optimalMoves)
    {
      IEnumerable<EmergencyServicePlan> optimalPlans = OptimalMovesSearch(move.Make(p), incidents, k + 1, h);
    }
  }

  private IEnumerable<OptimalMove> GenerateOptimalMoves(EmergencyServicePlan p, ReadOnlySpan<Incident> I)
  {
    return null;
  }
}

