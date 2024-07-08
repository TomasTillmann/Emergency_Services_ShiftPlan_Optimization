using ESSP.DataModel;
using MyExtensions;

namespace Optimizing;

public class RandomBasicMoveSampler : AllBasicMovesGenerator, IRandomMoveSampler
{
  public Random Random { get; }
  private readonly List<MoveSequenceDuo> _candidates = new(6);

  public RandomBasicMoveSampler(ShiftTimes shiftTimes, Constraints constraints, Random random = null)
  : base(shiftTimes, constraints)
  {
    Random = random ?? new Random();
  }

  public MoveSequenceDuo Sample(EmergencyServicePlan plan)
  {
    _candidates.Clear();
    int randomDepotIndex = Random.Next(0, plan.Assignments.Length);
    IEnumerator<MoveSequenceDuo> enumerator;

    if (plan.Assignments[randomDepotIndex].MedicTeams.Count > 0)
    {
      int randomTeamIndex = Random.Next(0, plan.Assignments[randomDepotIndex].MedicTeams.Count);
      MedicTeamId teamId = new(randomDepotIndex, randomTeamIndex);
      Interval oldShift = plan.Team(teamId).Shift;

      enumerator = GetShorterShift(plan, teamId, oldShift).GetEnumerator();
      if (enumerator.MoveNext())
      {
        _candidates.Add(enumerator.Current);
      }
      enumerator.Dispose();

      enumerator = GetLongerShift(plan, teamId, oldShift).GetEnumerator();
      if (enumerator.MoveNext())
      {
        _candidates.Add(enumerator.Current);
      }
      enumerator.Dispose();

      enumerator = GetEarlierShift(plan, teamId, oldShift).GetEnumerator();
      if (enumerator.MoveNext())
      {
        _candidates.Add(enumerator.Current);
      }
      enumerator.Dispose();

      enumerator = GetLaterShift(plan, teamId, oldShift).GetEnumerator();
      if (enumerator.MoveNext())
      {
        _candidates.Add(enumerator.Current);
      }
      enumerator.Dispose();
    }

    enumerator = GetTeamAllocations(plan, randomDepotIndex).GetEnumerator();
    if (enumerator.MoveNext())
    {
      _candidates.Add(enumerator.Current);
    }
    enumerator.Dispose();

    enumerator = GetAmbMoves(plan, randomDepotIndex).GetEnumerator();
    if (enumerator.MoveNext())
    {
      _candidates.Add(enumerator.Current);
    }
    enumerator.Dispose();

    if (_candidates.Count > 0)
    {
      return _candidates[Random.Next(_candidates.Count)];
    }
    else
    {
      return MoveSequenceDuo.GetNewEmpty(2);
    }
  }
}



