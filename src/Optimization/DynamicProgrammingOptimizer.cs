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

public class DynamicProgrammingOptimizer : Optimizer
{
  private readonly LexObjectiveFunction _lex;
  public DynamicProgrammingOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, LexObjectiveFunction lex, Random? random = null) : base(world, constraints, shiftTimes, null, random)
  {
    _lex = lex;
  }

  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    return OptimalMovesSearch(EmergencyServicePlan.Empty, incidents, 0, incidents.Count);
  }

  private Dictionary<(int, EmergencyServicePlan), List<EmergencyServicePlan>> cache = new();
  private List<EmergencyServicePlan> OptimalMovesSearch(EmergencyServicePlan p, ReadOnlySpan<Incident> I, int k, int h)
  {
    if (k == h)
    {
      return new() { p };
    }
    else if (cache.ContainsKey((k, p)))
    {
      return cache[(k, p)];
    }

    ReadOnlySpan<Incident> I_k = I.Slice(0, k);
    List<EmergencyServicePlan> optimal = new();

    IEnumerable<OptimalMove> optimalMoves = GenerateOptimalMoves(p, I_k);
    foreach (OptimalMove move in optimalMoves)
    {
      List<EmergencyServicePlan> optimalPlans = OptimalMovesSearch(move.Make(p), I, k + 1, h);
      if (optimal.Count == 0)
      {
        optimal = optimalPlans;
      }
      else
      {
        int decision = _lex.GetLoss(optimal[0], optimalPlans[0], I_k);
        if (decision == 0)
        {
          optimal.AddRange(optimalPlans);
        }
        else if (decision == 1)
        {
          optimal = optimalPlans;
        }
      }
    }
    cache[(k, p)] = optimal;
    return optimal;
  }

  private IEnumerable<OptimalMove> GenerateOptimalMoves(EmergencyServicePlan p, ReadOnlySpan<Incident> I)
  {
    return null;
  }
}

