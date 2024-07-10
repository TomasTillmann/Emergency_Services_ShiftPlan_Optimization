using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using DataModel.Interfaces;
using DistanceAPI;
using ESSP.DataModel;
using MyExtensions;
using Simulating;

namespace Optimizing;

public class OptimalMovesSearchOptimizer
{
  private readonly GaantView _gaantView;
  private readonly LexComparer _lexComparer;
  private readonly MoveMaker _moveMaker;
  private readonly ShiftTimes _shiftTimes;
  private readonly Random _random;
  private readonly HashSet<(int K, EmergencyServicePlan Plan)> _cache;
  private readonly Stopwatch _sw = new();

  private EmergencyServicePlan best;
  private OptimalMovesGenerator[] movesGenerators;

  public TextWriter Writer { get; set; } = Console.Out;
  public TextWriter BestPlansWriter { get; set; } = Console.Out;
  public World World { get; set; }
  public Constraints Constraints { get; set; }
  public int PlansVisited { get; set; }
  public int VisitPlans { get; set; }

  private readonly IDistanceCalculator _distanceCalculator;
  
  public OptimalMovesSearchOptimizer(World world, ShiftTimes shiftTimes, IDistanceCalculator distanceCalculator, Constraints constraints, int visitPlans, Random random = null)
  {
    World = world;
    Constraints = constraints;
    VisitPlans = visitPlans;
    _lexComparer = new(world, constraints, distanceCalculator);
    _moveMaker = new();
    _shiftTimes = shiftTimes;
    _cache = new(new OptimalMovesPlanComparer());
    _random = random ?? new Random();
    _distanceCalculator = distanceCalculator;

    _gaantView = new(world, constraints, distanceCalculator);
  }

  public List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    PlansVisited = 0;

    movesGenerators = new OptimalMovesGenerator[incidents.Length];
    for (int k = 0; k < movesGenerators.Length; ++k)
    {
      movesGenerators[k] = new(World, _shiftTimes, Constraints, _distanceCalculator, 2, _random);
      movesGenerators[k].Incidents = incidents;
      movesGenerators[k].writer = Writer;
      movesGenerators[k].K = k;
    }

    best = EmergencyServicePlan.GetNewEmpty(World);
    _sw.Restart();
    _sw.Start();
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
      Simulation simulation = new(World, Constraints, _distanceCalculator);
      // current is better than best
      if (res == 1)
      {
        Writer.WriteLine($"UPDATE: elapsed: {_sw.Elapsed.TotalSeconds}, cost: {current.Cost}, allocatedTeams: {current.MedicTeamsCount}, allocatedAmbulances: {current.AmbulancesCount}, handled: {simulation.HandledIncidentsCount}");
        Writer.Flush();
        best.FillFrom(current);
      }
      
      Writer.WriteLine($"elapsed: {_sw.Elapsed.TotalSeconds}, cost: {current.Cost}, allocatedTeams: {current.MedicTeamsCount}, allocatedAmbulances: {current.AmbulancesCount}, handled: {simulation.HandledIncidentsCount}");
      Writer.Flush();
      BestPlansWriter.WriteLine("GANT");
      _gaantView.Show(current, incidents.AsSpan(), BestPlansWriter);
      BestPlansWriter.WriteLine(JsonSerializer.Serialize(current));
      BestPlansWriter.WriteLine("-----------");
      BestPlansWriter.Flush();
      BestPlansWriter.Flush();

      return;
    }

    Writer.WriteLine($"elapsed: {_sw.Elapsed.TotalSeconds}, k: {k}, before enumeration");
    Writer.Flush();
    var moves = movesGenerators[k].GetMoves(current).Enumerate(2);
    var index = _random.Next(moves.Count);
    Writer.WriteLine($"elapsed: {_sw.Elapsed.TotalSeconds}, k: {k}, move: {moves[index]}, after enumeration");
    Writer.WriteLine($"{index}/{moves.Count}");
    Writer.Flush();
    
    _moveMaker.ModifyMakeMove(current, moves[index].Normal);
    if (!_cache.Contains((k, current)))
    {
      OptimalMovesSearch(current, incidents, k + 1, h);
      _cache.Add((k, current));
    }
    _moveMaker.ModifyMakeInverseMove(current, moves[index].Inverse);
  }
}
