using ESSP.DataModel;

namespace Optimizing;

public class AllBasicMovesGenerator : MoveGeneratorBase
{
  public AllBasicMovesGenerator(ShiftTimes shiftTimes, Constraints constraints)
  : base(shiftTimes, constraints, 1)
  {
    Moves.Count = 1;
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
      Moves.Inverse.MovesBuffer[0] = new Move
      {
        Type = MoveType.ShiftChange,
        Shift = shift,
        DepotIndex = teamId.DepotIndex,
        OnDepotIndex = teamId.OnDepotIndex
      };

      Moves.Normal.MovesBuffer[0] = new Move
      {
        Type = MoveType.ShiftChange,
        Shift = Interval.GetByStartAndDuration(shift.StartSec, ShiftTimes.GetLonger(shift.DurationSec)),
        DepotIndex = teamId.DepotIndex,
        OnDepotIndex = teamId.OnDepotIndex
      };

      yield return Moves;
    }

    if (plan.CanShorten(teamId, ShiftTimes))
    {
      Moves.Inverse.MovesBuffer[0] = new Move
      {
        Type = MoveType.ShiftChange,
        Shift = shift,
        DepotIndex = teamId.DepotIndex,
        OnDepotIndex = teamId.OnDepotIndex
      };

      Moves.Normal.MovesBuffer[0] = new Move
      {
        Type = MoveType.ShiftChange,
        Shift = Interval.GetByStartAndDuration(shift.StartSec, ShiftTimes.GetShorter(shift.DurationSec)),
        DepotIndex = teamId.DepotIndex,
        OnDepotIndex = teamId.OnDepotIndex
      };

      yield return Moves;
    }

    if (plan.CanEarlier(teamId, ShiftTimes))
    {
      Moves.Inverse.MovesBuffer[0] = new Move
      {
        Type = MoveType.ShiftChange,
        Shift = shift,
        DepotIndex = teamId.DepotIndex,
        OnDepotIndex = teamId.OnDepotIndex
      };

      Moves.Normal.MovesBuffer[0] = new Move
      {
        Type = MoveType.ShiftChange,
        Shift = Interval.GetByStartAndDuration(ShiftTimes.GetEarlier(shift.StartSec), shift.DurationSec),
        DepotIndex = teamId.DepotIndex,
        OnDepotIndex = teamId.OnDepotIndex
      };

      yield return Moves;
    }

    if (plan.CanLater(teamId, ShiftTimes))
    {
      Moves.Inverse.MovesBuffer[0] = new Move
      {
        Type = MoveType.ShiftChange,
        Shift = shift,
        DepotIndex = teamId.DepotIndex,
        OnDepotIndex = teamId.OnDepotIndex
      };

      Moves.Normal.MovesBuffer[0] = new Move
      {
        Type = MoveType.ShiftChange,
        Shift = Interval.GetByStartAndDuration(ShiftTimes.GetLater(shift.StartSec), shift.DurationSec),
        DepotIndex = teamId.DepotIndex,
        OnDepotIndex = teamId.OnDepotIndex
      };

      yield return Moves;
    }
  }

  private IEnumerable<MoveSequenceDuo> GetTeamAllocations(EmergencyServicePlan plan, int depotIndex)
  {
    if (plan.CanAllocateTeam(depotIndex, Constraints))
    {
      Moves.Inverse.MovesBuffer[0] = new Move
      {
        Type = MoveType.TeamDeallocation,
        DepotIndex = depotIndex,
        OnDepotIndex = plan.Assignments[depotIndex].MedicTeams.Count
      };

      for (int i = 0; i < ShiftTimes.AllowedStartingTimesSecSorted.Length; ++i)
      {
        Moves.Normal.MovesBuffer[0] = new Move
        {
          Type = MoveType.TeamAllocation,
          DepotIndex = depotIndex,
          Shift = Interval.GetByStartAndDuration(ShiftTimes.AllowedStartingTimesSecSorted[i], ShiftTimes.MinDurationSec)
        };

        yield return Moves;
      }
    }
  }

  private IEnumerable<MoveSequenceDuo> GetTeamDeallocations(EmergencyServicePlan plan, MedicTeamId teamId)
  {
    if (plan.CanDeallocateTeam(teamId, ShiftTimes))
    {
      Moves.Inverse.MovesBuffer[0] = new Move
      {
        Type = MoveType.TeamAllocation,
        DepotIndex = teamId.DepotIndex,
        Shift = plan.Team(teamId).Shift
      };

      Moves.Normal.MovesBuffer[0] = new Move
      {
        Type = MoveType.TeamDeallocation,
        DepotIndex = teamId.DepotIndex,
        OnDepotIndex = teamId.OnDepotIndex
      };

      yield return Moves;
    }
  }

  private IEnumerable<MoveSequenceDuo> GetAmbMoves(EmergencyServicePlan plan, int depotIndex)
  {
    if (plan.CanAllocateAmbulance(depotIndex, Constraints))
    {
      Moves.Inverse.MovesBuffer[0] = new Move
      {
        Type = MoveType.AmbulanceDeallocation,
        DepotIndex = depotIndex
      };

      Moves.Normal.MovesBuffer[0] = new Move
      {
        Type = MoveType.AmbulanceAllocation,
        DepotIndex = depotIndex
      };

      yield return Moves;
    }

    if (plan.CanDeallocateAmbulance(depotIndex))
    {
      Moves.Inverse.MovesBuffer[0] = new Move
      {
        Type = MoveType.AmbulanceAllocation,
        DepotIndex = depotIndex
      };

      Moves.Normal.MovesBuffer[0] = new Move
      {
        Type = MoveType.AmbulanceDeallocation,
        DepotIndex = depotIndex
      };

      yield return Moves;
    }
  }
}


