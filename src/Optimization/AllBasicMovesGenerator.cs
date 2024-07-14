using ESSP.DataModel;

namespace Optimizing;

/// <summary>
/// Generates basic moves:
/// 1. Allocate team and ambulance
/// 2. Deallocate ambulance
/// 3. Deallocate team
/// 4. Change shift time by the shortest possible amount
/// 5. Move start of the shift by the shortest possible duration
/// </summary>
public class AllBasicMovesGenerator : MoveGeneratorBase
{
  public AllBasicMovesGenerator(ShiftTimes shiftTimes, Constraints constraints)
  : base(shiftTimes, constraints, 2)
  {
  }

  /// <inheritdoc />
  public override IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan)
  {
    for (int depotIndex = 0; depotIndex < plan.Assignments.Length; ++depotIndex)
    {
      for (int teamIndex = 0; teamIndex < plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
      {
        MedicTeamId teamId = new(depotIndex, teamIndex);
        Interval oldShift = plan.Team(teamId).Shift;

        foreach (var move in GetLongerShift(plan, teamId, oldShift))
        {
          yield return move;
        }

        foreach (var move in GetShorterShift(plan, teamId, oldShift))
        {
          yield return move;
        }

        foreach (var move in GetEarlierShift(plan, teamId, oldShift))
        {
          yield return move;
        }

        foreach (var move in GetLaterShift(plan, teamId, oldShift))
        {
          yield return move;
        }

        foreach (var move in GetTeamDeallocations(plan, teamId))
        {
          yield return move;
        }
      }

      foreach (var move in GetTeamAllocations(plan, depotIndex))
      {
        yield return move;
      }

      foreach (var move in GetAmbMoves(plan, depotIndex))
      {
        yield return move;
      }
    }
  }

  protected IEnumerable<MoveSequenceDuo> GetLongerShift(EmergencyServicePlan plan, MedicTeamId teamId, Interval shift)
  {
    if (plan.CanLonger(teamId, ShiftTimes))
    {
      ChangeShift(teamId, shift, Interval.GetByStartAndDuration(shift.StartSec, ShiftTimes.GetLonger(shift.DurationSec)));
      yield return Moves;
    }
  }

  protected IEnumerable<MoveSequenceDuo> GetShorterShift(EmergencyServicePlan plan, MedicTeamId teamId, Interval shift)
  {
    if (plan.CanShorten(teamId, ShiftTimes))
    {
      ChangeShift(teamId, shift, Interval.GetByStartAndDuration(shift.StartSec, ShiftTimes.GetShorter(shift.DurationSec)));
      yield return Moves;
    }
  }

  protected IEnumerable<MoveSequenceDuo> GetEarlierShift(EmergencyServicePlan plan, MedicTeamId teamId, Interval shift)
  {
    if (plan.CanEarlier(teamId, ShiftTimes))
    {
      ChangeShift(teamId, shift, Interval.GetByStartAndDuration(ShiftTimes.GetEarlier(shift.StartSec), shift.DurationSec));
      yield return Moves;
    }
  }

  protected IEnumerable<MoveSequenceDuo> GetLaterShift(EmergencyServicePlan plan, MedicTeamId teamId, Interval shift)
  {
    if (plan.CanLater(teamId, ShiftTimes))
    {
      ChangeShift(teamId, shift, Interval.GetByStartAndDuration(ShiftTimes.GetLater(shift.StartSec), shift.DurationSec));
      yield return Moves;
    }
  }

  protected IEnumerable<MoveSequenceDuo> GetTeamAllocations(EmergencyServicePlan plan, int depotIndex)
  {
    if (plan.CanAllocateTeam(depotIndex, Constraints) && plan.CanAllocateAmbulance(depotIndex, Constraints))
    {
      for (int i = 0; i < ShiftTimes.AllowedStartingTimesSecSorted.Length; ++i)
      {
        AllocateTeamAndAmbulance(depotIndex, Interval.GetByStartAndDuration(ShiftTimes.AllowedStartingTimesSecSorted[i], ShiftTimes.MinDurationSec), plan.Assignments[depotIndex].MedicTeams.Count);
        yield return Moves;
      }
    }
  }

  protected IEnumerable<MoveSequenceDuo> GetTeamDeallocations(EmergencyServicePlan plan, MedicTeamId teamId)
  {
    if (plan.CanDeallocateTeam(teamId, ShiftTimes))
    {
      DeallocateTeam(teamId, plan.Team(teamId).Shift);
      yield return Moves;
    }
  }

  protected IEnumerable<MoveSequenceDuo> GetAmbMoves(EmergencyServicePlan plan, int depotIndex)
  {
    if (plan.CanDeallocateAmbulance(depotIndex))
    {
      DeallocateAmbulance(depotIndex);
      yield return Moves;
    }
  }
}


