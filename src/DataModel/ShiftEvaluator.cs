using System.Collections.Immutable;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public class ShiftEvaluator
{
  private PlannableIncident.Factory plannableIncidentFactory;
  private IncTypeToAllowedAmbTypesTable ambToIncTypesTable;

  public ShiftEvaluator(DistanceCalculator distanceCalculator, ImmutableArray<Hospital> hospitals, IncTypeToAllowedAmbTypesTable ambToIncTypesTable)
  {
    this.plannableIncidentFactory = new PlannableIncident.Factory(distanceCalculator, hospitals);
    this.ambToIncTypesTable = ambToIncTypesTable;
  }

  public bool IsHandling(Shift shift, in Incident incident)
  {
    PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);

    return IsHandling(shift, in plannableIncident);
  }

  private bool IsHandling(Shift shift, in PlannableIncident plannableIncident)
  {
    // 1
    if (plannableIncident.ToIncidentDrive.StartSec + plannableIncident.ToIncidentDrive.DurationSec
        > plannableIncident.Incident.OccurenceSec + plannableIncident.Incident.Type.MaximumResponseTimeSec)
    {
      return false;
    }

    // 1, 2, 3, 4
    if (plannableIncident.InHospitalDelivery.EndSec > shift.Work.EndSec)
    {
      return false;
    }

    if (!ambToIncTypesTable.IsAllowed(plannableIncident.Incident.Type, shift.Ambulance.Type))
    {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Rreturns better shift out of the two based on defined conditions.
  /// If both shifts are equaly good, <paramref name="shift1"/> is returned.
  /// </summary>
  public Shift GetBetter(Shift shift1, Shift shift2, in Incident incident)
  {
    // 1
    bool isShift1Free = shift1.IsFree(incident.OccurenceSec);
    if (!(isShift1Free && shift2.IsFree(incident.OccurenceSec)))
    {
      return isShift1Free ? shift1 : shift2;
    }

    // 2
    Interval shift1ToIncidentDrive = plannableIncidentFactory.GetToIncidentDrive(incident.OccurenceSec, incident.Location, shift1);
    Interval shift2ToIncidentDrive = plannableIncidentFactory.GetToIncidentDrive(incident.OccurenceSec, incident.Location, shift2);

    if (shift1ToIncidentDrive.EndSec != shift2ToIncidentDrive.EndSec)
    {
      return shift1ToIncidentDrive.EndSec < shift2ToIncidentDrive.EndSec ? shift1 : shift2;
    }

    // 3
    if (shift1.TimeActive != shift2.TimeActive)
    {
      return shift1.TimeActive < shift2.TimeActive ? shift1 : shift2;
    }

    // 4
    return shift1.Ambulance.Type.Cost <= shift2.Ambulance.Type.Cost ? shift1 : shift2;
  }
}

