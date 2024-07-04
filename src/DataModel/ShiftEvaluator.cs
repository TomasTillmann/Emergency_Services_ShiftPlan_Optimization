using System;
using System.Collections.Immutable;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public class MedicTeamsEvaluator
{
  private readonly PlannableIncident.Factory _plannableIncidentFactory;

  private ISimulationState _state;

  public ISimulationState State
  {
    get => _state;
    set
    {
      _state = value;
      _plannableIncidentFactory.State = value;
    }
  }

  private EmergencyServicePlan _plan;

  public EmergencyServicePlan Plan
  {
    get => _plan;
    set
    {
      _plan = value;
      _plannableIncidentFactory.Plan = value;
    }
  }

  public MedicTeamsEvaluator(World world)
  {
    this._plannableIncidentFactory = new PlannableIncident.Factory(world);
  }

  public bool IsHandling(MedicTeamId teamId, in Incident incident)
  {
    PlannableIncident plannableIncident = _plannableIncidentFactory.Get(teamId, in incident);
    return IsHandling(teamId, plannableIncident);
  }

  public bool IsHandling(MedicTeamId teamId, PlannableIncident plannableIncident)
  {
    // No ambulance available.
    if (plannableIncident.AmbulanceIndex == -1)
    {
      return false;
    }

    // 1
    if (plannableIncident.ToIncidentDrive.StartSec + plannableIncident.ToIncidentDrive.DurationSec
        > plannableIncident.Incident.OccurenceSec + plannableIncident.Incident.GoldTimeSec)
    {
      return false;
    }

    // 1, 2, 3, 4
    MedicTeam team = Plan.Team(teamId);
    const int overdue = 60 * 60;
    if (plannableIncident.InHospitalDelivery.EndSec > team.Shift.EndSec + overdue)
    {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Rreturns better shift out of the two based on defined conditions.
  /// If both shifts are equaly good, <paramref name="medicTeam1"/> is returned.
  /// </summary>
  public MedicTeamId GetBetter(MedicTeamId medicTeamId1, MedicTeamId medicTeamId2, in Incident incident)
  {
    MedicTeamState medicTeam1State = State.TeamState(medicTeamId1);
    MedicTeamState medicTeam2State = State.TeamState(medicTeamId2);

    // 1
    bool isShift1Free = medicTeam1State.IsFree(incident.OccurenceSec);
    if (!(isShift1Free && medicTeam2State.IsFree(incident.OccurenceSec)))
    {
      return isShift1Free ? medicTeamId1 : medicTeamId2;
    }

    // 2
    Interval shift1ToIncidentDrive = _plannableIncidentFactory.GetToIncidentDrive(incident.OccurenceSec, incident.Location, medicTeamId1);
    Interval shift2ToIncidentDrive = _plannableIncidentFactory.GetToIncidentDrive(incident.OccurenceSec, incident.Location, medicTeamId2);

    if (shift1ToIncidentDrive.EndSec != shift2ToIncidentDrive.EndSec)
    {
      return shift1ToIncidentDrive.EndSec < shift2ToIncidentDrive.EndSec ? medicTeamId1 : medicTeamId2;
    }

    // 3
    if (medicTeam1State.TimeActiveSec != medicTeam2State.TimeActiveSec)
    {
      return medicTeam1State.TimeActiveSec < medicTeam2State.TimeActiveSec ? medicTeamId1 : medicTeamId2;
    }

    return medicTeamId1;
  }
}

