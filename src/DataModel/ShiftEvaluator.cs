using System.Collections.Generic;

namespace ESSP.DataModel;

public class ShiftEvaluator
{
  PlannableIncident.Factory plannableIncidentFactory;

  public ShiftEvaluator(PlannableIncident.Factory plannableIncidentFactory)
  {
    this.plannableIncidentFactory = plannableIncidentFactory;
  }

  public bool IsHandling(Shift shift, Incident incident)
  {
    PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);

    return IsHandling(shift, plannableIncident);
  }

  public bool IsHandling(Shift shift, PlannableIncident plannableIncident)
  {
    // 1
    if (plannableIncident.ToIncidentDrive.Start + plannableIncident.ToIncidentDrive.Duration > plannableIncident.Incident.Occurence + plannableIncident.Incident.Type.MaximumResponseTime)
    {
      return false;
    }

    // 1, 2, 3, 4
    if (plannableIncident.InHospitalDelivery.End > shift.Work.End)
    {
      return false;
    }

    HashSet<AmbulanceType> allowedAmbulanceTypes = plannableIncident.Incident.Type.AllowedAmbulanceTypes;

    // Open world principle.
    if (allowedAmbulanceTypes.Count != 0 && !allowedAmbulanceTypes.Contains(shift.Ambulance.Type))
    {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Rreturns better shift out of the two based on defined conditions.
  /// If both shifts are equaly good, <paramref name="shift1"/> is returned.
  /// </summary>
  public Shift GetBetter(Shift shift1, Shift shift2, Incident incident)
  {
    // 1
    bool isShift1Free = shift1.IsFree(incident.Occurence);
    if (!isShift1Free || !shift2.IsFree(incident.Occurence))
    {
      return isShift1Free ? shift1 : shift2;
    }

    // 2
    PlannableIncident shift1PlannableIncident = plannableIncidentFactory.Get(incident, shift1);
    PlannableIncident shift2PlannableIncident = plannableIncidentFactory.Get(incident, shift2);

    if (shift1PlannableIncident.ToIncidentDrive.End != shift2PlannableIncident.ToIncidentDrive.End)
    {
      return shift1PlannableIncident.ToIncidentDrive.End < shift2PlannableIncident.ToIncidentDrive.End ? shift1 : shift2;
    }

    // 3
    Seconds shift1TimeActive = shift1.TimeActive();
    Seconds shift2TimeActive = shift2.TimeActive();
    if (shift1TimeActive != shift2TimeActive)
    {
      return shift1TimeActive < shift2TimeActive ? shift1 : shift2;
    }

    // 4
    return shift1.Ambulance.Type.Cost <= shift2.Ambulance.Type.Cost ? shift1 : shift2;
  }
}
