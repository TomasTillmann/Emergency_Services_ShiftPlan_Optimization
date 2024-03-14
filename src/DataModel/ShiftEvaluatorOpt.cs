using System.Collections.Immutable;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public class ShiftEvaluatorOpt
{
  private PlannableIncidentOpt.Factory plannableIncidentFactory;
  private IncTypeToAllowedAmbTypesTable ambToIncTypesTable;

  public ShiftEvaluatorOpt(DistanceCalculatorOpt distanceCalculator, ImmutableArray<HospitalOpt> hospitals, IncTypeToAllowedAmbTypesTable ambToIncTypesTable)
  {
    this.plannableIncidentFactory = new PlannableIncidentOpt.Factory(distanceCalculator, hospitals);
    this.ambToIncTypesTable = ambToIncTypesTable;
  }

  public bool IsHandling(ShiftOpt shift, in IncidentOpt incident)
  {
    PlannableIncidentOpt plannableIncident = plannableIncidentFactory.Get(incident, shift);

    return IsHandling(shift, in plannableIncident);
  }

  private bool IsHandling(ShiftOpt shift, in PlannableIncidentOpt plannableIncident)
  {
    // 1
    if (plannableIncident.ToIncidentDrive.StartSec + plannableIncident.ToIncidentDrive.Duration
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
  public ShiftOpt GetBetter(ShiftOpt shift1, ShiftOpt shift2, in IncidentOpt incident)
  {
    // 1
    bool isShift1Free = shift1.IsFree(incident.OccurenceSec);
    if (!(isShift1Free && shift2.IsFree(incident.OccurenceSec)))
    {
      return isShift1Free ? shift1 : shift2;
    }

    // 2
    IntervalOpt shift1ToIncidentDrive = plannableIncidentFactory.GetToIncidentDrive(incident.OccurenceSec, incident.Location, shift1);
    IntervalOpt shift2ToIncidentDrive = plannableIncidentFactory.GetToIncidentDrive(incident.OccurenceSec, incident.Location, shift2);

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

