using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ESSP.DataModel;

public class EmergencyServicePlan
{
  public ImmutableArray<Depot> Depots { get; init; }

  public MedicTeam Team(MedicTeamId teamId) => Depots[teamId.DepotIndex].MedicTeams[teamId.OnDepotIndex];
  public Ambulance Ambulance(AmbulanceId ambId) => Depots[ambId.DepotIndex].Ambulances[ambId.OnDepotIndex];

  public int AmbulancesCount { get; private set; }
  public int MedicTeamsCount { get; private set; }
  public int TotalShiftDuration { get; private set; }

  public int Cost => TotalShiftDuration + AmbulancesCount;

  public void AllocateTeam(int depotIndex, MedicTeam team)
  {
    Depots[depotIndex].MedicTeams.Add(team);
    ++MedicTeamsCount;
    TotalShiftDuration += team.Shift.DurationSec;
  }

  public void DeallocateTeam(int depotIndex, int teamIndex)
  {
    // TODO: reorganizes stuff - can it be done faster?
    MedicTeam team = Depots[depotIndex].MedicTeams[teamIndex];
    Depots[depotIndex].MedicTeams.RemoveAt(teamIndex);
    --MedicTeamsCount;
    TotalShiftDuration -= team.Shift.DurationSec;
  }

  public void ChangeShift(int depotIndex, int teamIndex, Interval shift)
  {
    TotalShiftDuration -= Depots[depotIndex].MedicTeams[teamIndex].Shift.DurationSec;
    TotalShiftDuration += shift.DurationSec;
    Depots[depotIndex].MedicTeams[teamIndex].Shift = shift;
  }

  public void AllocateAmbulance(int depotIndex, Ambulance ambulance)
  {
    Depots[depotIndex].Ambulances.Add(ambulance);
    ++AmbulancesCount;
  }

  public void DeallocateAmbulance(int depotIndex, int ambulanceIndex)
  {
    Depots[depotIndex].Ambulances.RemoveAt(ambulanceIndex);
    --AmbulancesCount;
  }

  public IEnumerable<MedicTeam> MedicTeams()
  {
    for (int i = 0; i < Depots.Length; ++i)
    {
      for (int j = 0; j < Depots[i].MedicTeams.Count; ++j)
      {
        yield return Depots[i].MedicTeams[j];
      }
    }
  }

  public IEnumerable<Ambulance> Ambulances()
  {
    for (int i = 0; i < Depots.Length; ++i)
    {
      for (int j = 0; j < Depots[i].Ambulances.Count; ++j)
      {
        yield return Depots[i].Ambulances[j];
      }
    }
  }
}
