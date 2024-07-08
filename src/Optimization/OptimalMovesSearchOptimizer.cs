using System.Collections.Immutable;
using DistanceAPI;
using ESSP.DataModel;
using MyExtensions;

namespace Optimizing;

public class OptimalMovesSearchOptimizer
{
  private readonly GaantView _gaantView;
  private readonly LexComparer _lexComparer;
  private readonly MoveMaker _moveMaker;
  private readonly ShiftTimes _shiftTimes;
  private readonly Random _random;
  private readonly HashSet<(int K, EmergencyServicePlan Plan)> _cache;

  private StreamWriter writer;
  private EmergencyServicePlan best;
  private OptimalMovesGenerator[] movesGenerators;

  public World World { get; set; }
  public Constraints Constraints { get; set; }

  public int PlansVisited { get; set; }
  public OptimalMovesSearchOptimizer(World world, ShiftTimes shiftTimes, Constraints constraints, Random random = null)
  {
    World = world;
    Constraints = constraints;
    _lexComparer = new(world, constraints);
    _moveMaker = new();
    _shiftTimes = shiftTimes;
    _cache = new(new OptimalMovesPlanComparer());
    _random = random ?? new Random();

    _gaantView = new(world, constraints);
  }

  public List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    writer = new("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/log.txt");
    PlansVisited = 0;

    movesGenerators = new OptimalMovesGenerator[incidents.Length];
    for (int k = 0; k < movesGenerators.Length; ++k)
    {
      movesGenerators[k] = new(World, _shiftTimes, Constraints, 2, _random);
      movesGenerators[k].Incidents = incidents;
      movesGenerators[k].writer = writer;
      movesGenerators[k].K = k;
    }

    best = EmergencyServicePlan.GetNewEmpty(World);
    OptimalMovesSearch(EmergencyServicePlan.GetNewEmpty(World), incidents, 1, incidents.Length);
    writer.Dispose();
    return new List<EmergencyServicePlan> { best };
  }

  private void OptimalMovesSearch(EmergencyServicePlan current, ImmutableArray<Incident> incidents, int k, int h)
  {
    if (k == h)
    {
      int res = _lexComparer.Compare(best, current, incidents);
      if (PlansVisited % 50 == 0)
      {
        Console.WriteLine(PlansVisited++);
        var calc = ((RealDistanceCalculator)World.DistanceCalculator);
        Console.WriteLine($"Nearest hospitals: {calc.NearestHospitalHits} / {calc.NearestHospitalTotal}");
        Console.WriteLine($"Travel durations: {calc.TravelDurationHits} / {calc.TravelDurationTotal}");
        Console.WriteLine($"Intermediate locations: {calc.IntermediateLocationsHits} / {calc.IntermediateLocationsTotal}");
      }
      // current is better than best
      if (res == 1)
      {
        writer.WriteLine("UPDATE");
        _gaantView.Show(current, incidents.AsSpan(), writer);
        writer.WriteLine("cost: " + current.Cost);
        best.FillFrom(current);
      }

      return;
    }

    int m = 0;
    //writer.WriteLine($"k: {k}" + string.Join(", ", _movesGenerator.GetMoves(current).Enumerate(2)));
    foreach (MoveSequenceDuo move in movesGenerators[k].GetMoves(current))
    {
      ++m;
      Console.WriteLine($"k: {k}, m: {m}");

      //writer.WriteLine($"k: {k}\n{move}");
      _moveMaker.ModifyMakeMove(current, move.Normal);
      if (!_cache.Contains((k, current)))
      {
        OptimalMovesSearch(current, incidents, k + 1, h);
        _cache.Add((k, current));
      }
      
      _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
      //writer.Flush();
    }
  }
}
