using System.Collections.Immutable;
using ESSP.DataModel;
using Optimization;
using MyExtensions;

namespace Optimizing;

public class OptimalMovesSearchOptimizer
{
  private readonly GaantView _gaantView;
  private readonly LexComparer _lexComparer;
  private readonly MoveMaker _moveMaker;
  private readonly ShiftTimes _shiftTimes;
  private StreamWriter writer;

  private EmergencyServicePlan best;
  private OptimalMovesGenerator[] movesGenerators;

  public World World { get; set; }
  public Constraints Constraints { get; set; }

  public OptimalMovesSearchOptimizer(World world, ShiftTimes shiftTimes, Constraints constraints)
  {
    World = world;
    Constraints = constraints;
    _lexComparer = new(world, constraints);
    _moveMaker = new();
    _shiftTimes = shiftTimes;
    _gaantView = new(world, constraints);
  }

  public List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    writer = new("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/log.txt");
    movesGenerators = new OptimalMovesGenerator[incidents.Length];
    for (int i = 0; i < movesGenerators.Length; ++i)
    {
      movesGenerators[i] = new(World, _shiftTimes, Constraints, 2);
      movesGenerators[i].Incidents = incidents;
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
      // current is better than best
      if (res == 1)
      {
        writer.WriteLine("UPDATE");
        _gaantView.Show(current, incidents.AsSpan(), writer);
        best.FillFrom(current);
      }

      return;
    }

    movesGenerators[k].K = k;
    //writer.WriteLine($"k: {k}" + string.Join(", ", _movesGenerator.GetMoves(current).Enumerate(2)));
    foreach (MoveSequenceDuo move in movesGenerators[k].GetMoves(current))
    {
      writer.WriteLine($"k: {k}\n{move}");
      _moveMaker.ModifyMakeMove(current, move.Normal);
      OptimalMovesSearch(current, incidents, k + 1, h);
      _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
    }
  }
}
