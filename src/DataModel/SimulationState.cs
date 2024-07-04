namespace ESSP.DataModel;

public class SimulationState
{
  private readonly MedicTeamState[][] _teamStates;
  private readonly AmbulanceState[][] _ambulanceStates;

  public MedicTeamState TeamState(MedicTeamId teamId) => _teamStates[teamId.DepotIndex][teamId.OnDepotIndex];
  public AmbulanceState AmbulanceState(AmbulanceId ambId) => _ambulanceStates[ambId.DepotIndex][ambId.OnDepotIndex];

  public SimulationState(int availableDepotsCount, Constraints constraints)
  {
    _teamStates = new MedicTeamState[availableDepotsCount][];
    for (int i = 0; i < availableDepotsCount; ++i)
    {
      _teamStates[i] = new MedicTeamState[constraints.MaxTeamsPerDepotCount[i]];
      for (int j = 0; j < constraints.MaxTeamsPerDepotCount[i]; ++j)
      {
        _teamStates[i][j] = new MedicTeamState();
      }
    }

    _ambulanceStates = new AmbulanceState[availableDepotsCount][];
    for (int i = 0; i < availableDepotsCount; ++i)
    {
      _ambulanceStates[i] = new AmbulanceState[constraints.MaxAmbulancesPerDepotCount[i]];
      for (int j = 0; j < constraints.MaxAmbulancesPerDepotCount[i]; ++j)
      {
        _ambulanceStates[i][j] = new AmbulanceState();
      }
    }
  }

  public void PlanIncident(MedicTeamId teamId, PlannableIncident incident)
  {
    _teamStates[teamId.DepotIndex][teamId.OnDepotIndex].LastPlannedIncident.FillFrom(incident);
    _ambulanceStates[teamId.DepotIndex][incident.AmbulanceIndex].WhenFreeSec = incident.ToDepotDrive.EndSec;
    _teamStates[teamId.DepotIndex][teamId.OnDepotIndex].TimeActiveSec += incident.IncidentHandling.DurationSec;
  }

  /// <summary>
  /// Clears state of only plan's dimensions.
  /// </summary>
  public void Clear(EmergencyServicePlan plan)
  {
    for (int i = 0; i < plan.Assignments.Length; ++i)
    {
      for (int j = 0; j < plan.Assignments[i].MedicTeams.Count; ++j)
      {
        _teamStates[i][j].Clear();
      }

      for (int j = 0; j < plan.Assignments[i].Ambulances.Count; ++j)
      {
        _ambulanceStates[i][j].Clear();
      }
    }
  }
}

