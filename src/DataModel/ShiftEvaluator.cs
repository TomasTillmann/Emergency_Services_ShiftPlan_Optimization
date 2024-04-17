using System.Collections.Immutable;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public class MedicTeamsEvaluator
{
  private readonly PlannableIncident.Factory _plannableIncidentFactory;
  private readonly int _goldenTimeSec;

  public MedicTeamsEvaluator(DistanceCalculator distanceCalculator, ImmutableArray<Hospital> hospitals, int goldenTimeSec)
  {
    this._plannableIncidentFactory = new PlannableIncident.Factory(distanceCalculator, hospitals);
    _goldenTimeSec = goldenTimeSec;
  }

  public bool IsHandling(MedicTeam medicTeam, in Incident incident)
  {
    PlannableIncident plannableIncident = _plannableIncidentFactory.Get(incident, medicTeam);

    return IsHandling(medicTeam, plannableIncident);
  }

  public bool IsHandling(MedicTeam medicTeam, PlannableIncident plannableIncident)
  {
    // Not allocated.
    if (medicTeam.Shift.DurationSec == 0)
    {
      return false;
    }

    // 1
    if (plannableIncident.ToIncidentDrive.StartSec + plannableIncident.ToIncidentDrive.DurationSec
        > plannableIncident.Incident.OccurenceSec + _goldenTimeSec)
    {
      return false;
    }

    // 1, 2, 3, 4
    if (plannableIncident.InHospitalDelivery.EndSec > medicTeam.Shift.EndSec)
    {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Rreturns better shift out of the two based on defined conditions.
  /// If both shifts are equaly good, <paramref name="medicTeam1"/> is returned.
  /// </summary>
  public MedicTeam GetBetter(MedicTeam medicTeam1, MedicTeam medicTeam2, in Incident incident)
  {
    // 1
    bool isShift1Free = medicTeam1.IsFree(incident.OccurenceSec);
    if (!(isShift1Free && medicTeam2.IsFree(incident.OccurenceSec)))
    {
      return isShift1Free ? medicTeam1 : medicTeam2;
    }

    // 2
    Interval shift1ToIncidentDrive = _plannableIncidentFactory.GetToIncidentDrive(incident.OccurenceSec, incident.Location, medicTeam1);
    Interval shift2ToIncidentDrive = _plannableIncidentFactory.GetToIncidentDrive(incident.OccurenceSec, incident.Location, medicTeam2);

    if (shift1ToIncidentDrive.EndSec != shift2ToIncidentDrive.EndSec)
    {
      return shift1ToIncidentDrive.EndSec < shift2ToIncidentDrive.EndSec ? medicTeam1 : medicTeam2;
    }

    // 3
    if (medicTeam1.TimeActive != medicTeam2.TimeActive)
    {
      return medicTeam1.TimeActive < medicTeam2.TimeActive ? medicTeam1 : medicTeam2;
    }

    return medicTeam1;
  }
}

