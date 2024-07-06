using System.Collections.Generic;

namespace ESSP.DataModel;

public class SimulationStateVerbose : SimulationState
{
    private readonly Dictionary<MedicTeamId, List<PlannableIncident>> _plannedIncidentsForTeam = new();
  
    public SimulationStateVerbose(int availableDepotsCount, Constraints constraints) : base(availableDepotsCount, constraints)
    {
    }
  
    public override void PlanIncident(MedicTeamId teamId, PlannableIncident incident)
    {
        base.PlanIncident(teamId, incident);
        PlannableIncident copy = PlannableIncident.Factory.GetNewEmpty;
        copy.FillFrom(incident);
        if (!_plannedIncidentsForTeam.TryAdd(teamId, new List<PlannableIncident>() { copy }))
        {
            copy.FillFrom(incident);
            _plannedIncidentsForTeam[teamId].Add(copy);
        }
    }

    public override void Clear(EmergencyServicePlan plan)
    {
        base.Clear(plan);
        _plannedIncidentsForTeam.Clear();
    }

    public List<PlannableIncident> GetAllPlannedIncidents(MedicTeamId teamId)
    {
        if (!_plannedIncidentsForTeam.ContainsKey(teamId)) return new List<PlannableIncident>();
        return _plannedIncidentsForTeam[teamId];
    }
}