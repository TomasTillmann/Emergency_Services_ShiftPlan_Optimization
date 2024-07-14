using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using DataModel.Interfaces;
using DistanceAPI;
using ESSP.DataModel;
using MyExtensions;
using Simulating;

namespace Optimizing;

/// <summary>
/// Implementation of optimal moves search optimizer.
/// </summary>
public class OptimalMovesSearchOptimizer
{
  private readonly LexComparer _lexComparer;
  private readonly MoveMaker _moveMaker;
  private readonly ShiftTimes _shiftTimes;
  private readonly Random _random;
  private readonly HashSet<(int K, EmergencyServicePlan Plan)> _cache;

  private EmergencyServicePlan best;
  private OptimalMovesGenerator[] movesGenerators;

  public World World { get; set; }
  public int PlansVisited { get; private set; }
  public int VisitPlans { get; set; }
  public Constraints Constraints { get; set; }

  private readonly IDistanceCalculator _distanceCalculator;
  
  public OptimalMovesSearchOptimizer(World world, ShiftTimes shiftTimes, IDistanceCalculator distanceCalculator, Constraints constraints, int visitPlans, Random random = null)
  {
    World = world;
    Constraints = constraints;
    _lexComparer = new(world, constraints, distanceCalculator);
    _moveMaker = new();
    _shiftTimes = shiftTimes;
    _cache = new(new OptimalMovesPlanComparer());
    _random = random ?? new Random();
    _distanceCalculator = distanceCalculator;
  }

  /// <inheritdoc />
  public List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    movesGenerators = new OptimalMovesGenerator[incidents.Length];
    for (int k = 0; k < movesGenerators.Length; ++k)
    {
      movesGenerators[k] = new(World, _shiftTimes, Constraints, _distanceCalculator, 2, _random);
      movesGenerators[k].Incidents = incidents;
      movesGenerators[k].K = k;
    }

    best = EmergencyServicePlan.GetNewEmpty(World);
    for (PlansVisited = 0; PlansVisited < VisitPlans; ++PlansVisited)
    {
      OptimalMovesSearch(EmergencyServicePlan.GetNewEmpty(World), incidents, 1, incidents.Length);
    }
    return new List<EmergencyServicePlan> { best };
  }

  private void OptimalMovesSearch(EmergencyServicePlan current, ImmutableArray<Incident> incidents, int k, int h)
  {
    if (k == h)
    {
      int res = _lexComparer.Compare(best, current, incidents);
      // current is better than best
      if (res == 1)
      {
        best.FillFrom(current);
      }

      return;
    }

    var moves = movesGenerators[k].GetMoves(current).Enumerate(2);
    var index = _random.Next(moves.Count);
    
    _moveMaker.ModifyMakeMove(current, moves[index].Normal);
    if (!_cache.Contains((k, current)))
    {
      OptimalMovesSearch(current, incidents, k + 1, h);
      _cache.Add((k, current));
    }
    _moveMaker.ModifyMakeInverseMove(current, moves[index].Inverse);
  }
}
