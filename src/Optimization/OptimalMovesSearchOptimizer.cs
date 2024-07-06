using System.Collections.Immutable;
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
    _cache = new(new PlanComparer());
    _random = random ?? new Random();

    _gaantView = new(world, constraints);
  }

  public List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    writer = new("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/log.txt");
    PlansVisited = 0;
    movesGenerators = new OptimalMovesGenerator[incidents.Length];
    for (int i = 0; i < movesGenerators.Length; ++i)
    {
      movesGenerators[i] = new(World, _shiftTimes, Constraints, 2, _random);
      movesGenerators[i].Incidents = incidents;
      movesGenerators[i].writer = writer;
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
      Console.WriteLine(PlansVisited++);
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

    movesGenerators[k].K = k;
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
        _cache.Add((k, current));
        OptimalMovesSearch(current, incidents, k + 1, h);
      }
      else
      {
        Console.WriteLine($"visited");
      }
      _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
      //writer.Flush();
    }
  }

  private class PlanComparer : IEqualityComparer<(int K, EmergencyServicePlan Plan)>
  {
    public bool Equals((int K, EmergencyServicePlan Plan) x, (int K, EmergencyServicePlan Plan) y)
    {
      if (x.K != y.K)
      {
        return false;
      }

      if (x.Plan.AmbulancesCount != y.Plan.AmbulancesCount
          || x.Plan.MedicTeamsCount != y.Plan.MedicTeamsCount
          || x.Plan.TotalShiftDuration != y.Plan.TotalShiftDuration
         )
      {
        return false;
      }

      for (int depotIndex = 0; depotIndex < x.Plan.Assignments.Length; ++depotIndex)
      {
        if (x.Plan.Assignments[depotIndex].MedicTeams.Count != y.Plan.Assignments[depotIndex].MedicTeams.Count)
        {
          return false;
        }

        if (x.Plan.Assignments[depotIndex].Ambulances.Count != y.Plan.Assignments[depotIndex].Ambulances.Count)
        {
          return false;
        }

        for (int teamIndex = 0; teamIndex < x.Plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
        {
          if (x.Plan.Assignments[depotIndex].MedicTeams[teamIndex].Shift !=
              y.Plan.Assignments[depotIndex].MedicTeams[teamIndex].Shift)
          {
            return false;
          }
        }
      }

      return true;
    }

    public int GetHashCode((int K, EmergencyServicePlan Plan) obj)
    {
      return HashCode.Combine(obj.K, obj.Plan.AmbulancesCount, obj.Plan.MedicTeamsCount, obj.Plan.TotalShiftDuration, obj.Plan.Assignments);
    }
  }
}
