using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ESSP.DataModel;

public class EmergencyServicePlan
{
  public ImmutableArray<DepotAssignment> Assignments { get; init; }

  public MedicTeam Team(MedicTeamId teamId) => Assignments[teamId.DepotIndex].MedicTeams[teamId.OnDepotIndex];
  public Ambulance Ambulance(AmbulanceId ambId) => Assignments[ambId.DepotIndex].Ambulances[ambId.OnDepotIndex];

  public int AmbulancesCount { get; private set; }
  public int MedicTeamsCount { get; private set; }
  public int TotalShiftDuration { get; private set; }
  public List<MedicTeam> AvailableMedicTeams { get; }
  public List<Ambulance> AvailableAmbulances { get; }

  public int Cost => TotalShiftDuration + AmbulancesCount;

  public static EmergencyServicePlan GetNewEmpty(World world)
    => new(world.Depots.Length, world.AvailableMedicTeams, world.AvailableAmbulances);

  // creates an empty plan
  private EmergencyServicePlan(int depotCount, ImmutableArray<MedicTeam> allAvailableTeams, ImmutableArray<Ambulance> allAvailableAmbulances)
  {
    Assignments = Enumerable.Repeat(new DepotAssignment(), depotCount).ToImmutableArray();
    var assignmentsTemp = new DepotAssignment[depotCount];
    for (int i = 0; i < Assignments.Length; ++i)
    {
      assignmentsTemp[i] = new DepotAssignment();
    }
    Assignments = assignmentsTemp.ToImmutableArray();

    AvailableMedicTeams = allAvailableTeams.ToList();
    AvailableAmbulances = allAvailableAmbulances.ToList();
  }

  public void AllocateTeam(int depotIndex, Interval shift)
  {
    MedicTeam team = AvailableMedicTeams[AvailableMedicTeams.Count - 1];
    AvailableMedicTeams.RemoveAt(AvailableMedicTeams.Count - 1);
    team.Shift = shift;
    Assignments[depotIndex].MedicTeams.Add(team);
    ++MedicTeamsCount;
    TotalShiftDuration += team.Shift.DurationSec;
  }

  public void DeallocateTeam(int depotIndex, int teamIndex)
  {
    MedicTeam team = Assignments[depotIndex].MedicTeams[teamIndex];
    Assignments[depotIndex].MedicTeams.RemoveAt(teamIndex);
    AvailableMedicTeams.Add(team);
    --MedicTeamsCount;
    TotalShiftDuration -= team.Shift.DurationSec;
  }

  public void ChangeShift(int depotIndex, int teamIndex, Interval shift)
  {
    TotalShiftDuration -= Assignments[depotIndex].MedicTeams[teamIndex].Shift.DurationSec;
    TotalShiftDuration += shift.DurationSec;
    Assignments[depotIndex].MedicTeams[teamIndex].Shift = shift;
  }

  public void AllocateAmbulance(int depotIndex)
  {
    Ambulance ambulance = AvailableAmbulances[AvailableAmbulances.Count - 1];
    AvailableAmbulances.RemoveAt(AvailableAmbulances.Count - 1);
    Assignments[depotIndex].Ambulances.Add(ambulance);
    ++AmbulancesCount;
  }

  public void DeallocateAmbulance(int depotIndex)
  {
    int index = Assignments[depotIndex].Ambulances.Count - 1;
    Ambulance ambulance = Assignments[depotIndex].Ambulances[index];
    Assignments[depotIndex].Ambulances.RemoveAt(index);
    AvailableAmbulances.Add(ambulance);
    --AmbulancesCount;
  }

  public void FillFrom(EmergencyServicePlan other)
  {
    for (int depotIndex = 0; depotIndex < other.Assignments.Length; ++depotIndex)
    {
      Assignments[depotIndex].MedicTeams.Clear();
      Assignments[depotIndex].MedicTeams.AddRange(other.Assignments[depotIndex].MedicTeams);

      Assignments[depotIndex].Ambulances.Clear();
      Assignments[depotIndex].Ambulances.AddRange(other.Assignments[depotIndex].Ambulances);
    }
  }

  public static double GetMaxCost(World world, ShiftTimes shiftTimes)
  {
    return world.AvailableMedicTeams.Length * shiftTimes.MaxDurationSec + world.AvailableAmbulances.Length;
  }

  public bool CanLonger(MedicTeamId teamId, ShiftTimes shiftTimes)
  {
    return Assignments[teamId.DepotIndex].MedicTeams[teamId.OnDepotIndex].Shift.DurationSec != shiftTimes.MaxDurationSec;
  }

  public bool CanShorten(MedicTeamId teamId, ShiftTimes shiftTimes)
  {
    return Assignments[teamId.DepotIndex].MedicTeams[teamId.OnDepotIndex].Shift.DurationSec != shiftTimes.MinDurationSec;
  }

  public bool CanEarlier(MedicTeamId teamId, ShiftTimes shiftTimes)
  {
    return Assignments[teamId.DepotIndex].MedicTeams[teamId.OnDepotIndex].Shift.StartSec !=
           shiftTimes.EarliestStartingTimeSec;
  }

  public bool CanLater(MedicTeamId teamId, ShiftTimes shiftTimes)
  {
    return Assignments[teamId.DepotIndex].MedicTeams[teamId.OnDepotIndex].Shift.StartSec !=
           shiftTimes.LatestStartingTimeSec;
  }

  public bool CanAllocateTeam(int depotIndex, Constraints constraints)
  {
    return Assignments[depotIndex].MedicTeams.Count < constraints.MaxTeamsPerDepotCount[depotIndex]
           && AvailableMedicTeams.Count > 0;
  }

  public bool CanDeallocateTeam(MedicTeamId teamId, ShiftTimes shiftTimes)
  {
    return Team(teamId).Shift.DurationSec == shiftTimes.MinDurationSec;
  }

  public bool CanAllocateAmbulance(int depotIndex, Constraints constraints)
  {
    return Assignments[depotIndex].Ambulances.Count < constraints.MaxAmbulancesPerDepotCount[depotIndex]
           && AvailableAmbulances.Count > 0;
  }

  public bool CanDeallocateAmbulance(int depotIndex)
  {
    return Assignments[depotIndex].Ambulances.Count > 0;
  }
}
