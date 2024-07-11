namespace ESSP.DataModel;

public interface ISimulationState
{
  MedicTeamState TeamState(MedicTeamId teamId);
  AmbulanceState AmbulanceState(AmbulanceId ambId);
  void PlanIncident(MedicTeamId teamId, PlannableIncident incident);
  void FillFrom(ISimulationState other);
  void Clear();
}

