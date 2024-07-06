using ESSP.DataModel;

namespace Optimizing;

public class AllBasicMovesGenerator : MoveGeneratorBase
{
  public AllBasicMovesGenerator(ShiftTimes shiftTimes, Constraints constraints)
  : base(shiftTimes, constraints, 2)
  {
  }

  public override IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan)
  {
    for (int depotIndex = 0; depotIndex < plan.Assignments.Length; ++depotIndex)
    {
      for (int teamIndex = 0; teamIndex < plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
      {
        MedicTeamId teamId = new(depotIndex, teamIndex);

        foreach (var move in GetShiftChanges(plan, teamId))
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

  private IEnumerable<MoveSequenceDuo> GetShiftChanges(EmergencyServicePlan plan, MedicTeamId teamId)
  {
    Interval shift = plan.Team(teamId).Shift;
    if (plan.CanLonger(teamId, ShiftTimes))
    {
      ChangeShift(teamId, shift, Interval.GetByStartAndDuration(shift.StartSec, ShiftTimes.GetLonger(shift.DurationSec)));
      yield return Moves;
    }

    if (plan.CanShorten(teamId, ShiftTimes))
    {
      ChangeShift(teamId, shift, Interval.GetByStartAndDuration(shift.StartSec, ShiftTimes.GetShorter(shift.DurationSec)));
      yield return Moves;
    }

    if (plan.CanEarlier(teamId, ShiftTimes))
    {
      ChangeShift(teamId, shift, Interval.GetByStartAndDuration(ShiftTimes.GetEarlier(shift.StartSec), shift.DurationSec));
      yield return Moves;
    }

    if (plan.CanLater(teamId, ShiftTimes))
    {
      ChangeShift(teamId, shift, Interval.GetByStartAndDuration(ShiftTimes.GetLater(shift.StartSec), shift.DurationSec));
      yield return Moves;
    }
  }

  private IEnumerable<MoveSequenceDuo> GetTeamAllocations(EmergencyServicePlan plan, int depotIndex)
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

  private IEnumerable<MoveSequenceDuo> GetTeamDeallocations(EmergencyServicePlan plan, MedicTeamId teamId)
  {
    if (plan.CanDeallocateTeam(teamId, ShiftTimes))
    {
      DeallocateTeam(teamId, plan.Team(teamId).Shift);
      yield return Moves;
    }
  }

  private IEnumerable<MoveSequenceDuo> GetAmbMoves(EmergencyServicePlan plan, int depotIndex)
  {
    if (plan.CanDeallocateAmbulance(depotIndex))
    {
      DeallocateAmbulance(depotIndex);
      yield return Moves;
    }
  }
}


