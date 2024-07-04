using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public class OptimalMovesSearchOptimizer
{
  private readonly OptimalMovesGenerator _movesGenerator;
  private readonly LexComparer _lexComparer;
  private readonly MoveMaker _moveMaker;

  private EmergencyServicePlan best;
  private ImmutableArray<Incident> incidents;

  public World World { get; set; }
  public Constraints Constraints { get; set; }

  public OptimalMovesSearchOptimizer(World world, Constraints constraints)
  {
    World = world;
    Constraints = constraints;
    _lexComparer = new(world, constraints);
    _moveMaker = new();
  }

  public List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    _movesGenerator.Incidents = incidents;
    best = EmergencyServicePlan.GetNewEmpty(World);
    incidents = incidents;

    OptimalMovesSearch(EmergencyServicePlan.GetNewEmpty(World), 0, incidents.Length);
    return new List<EmergencyServicePlan> { best };
  }

  private void OptimalMovesSearch(EmergencyServicePlan current, int k, int h)
  {
    if (k == h)
    {
      int res = _lexComparer.Compare(best, current, incidents);
      if (res > 0)
      {
        best.FillFrom(current);
      }
    }

    _movesGenerator.K = k;
    foreach (MoveSequenceDuo move in _movesGenerator.GetMoves(current))
    {
      _moveMaker.ModifyMakeMove(current, move.Normal);
      OptimalMovesSearch(current, k + 1, h);
      _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
    }
  }
}
