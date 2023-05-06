using System;
using System.Collections.Generic;
using System.Linq;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulating;

class ShiftEvaluator
{
    PlannableIncident.Factory plannableIncidentFactory;

    public ShiftEvaluator(PlannableIncident.Factory plannableIncidentFactory)
    {
        this.plannableIncidentFactory = plannableIncidentFactory;
    }

    public List<Shift> GetHandlingShifts(List<Shift> shifts, Incident currentIncident)
    {
        List<Shift> handlingShifts = new();
        foreach (Shift shift in shifts)
        {
            if (IsHandling(shift, currentIncident))
            {
                handlingShifts.Add(shift);
            }
        }

        return handlingShifts;
    }

    public bool IsHandling(Shift shift, Incident incident)
    {
        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);

        return IsHandling(shift, plannableIncident);
    }

    public bool IsHandling(Shift shift, PlannableIncident plannableIncident)
    {
        // 1
        if (plannableIncident.ToIncidentDrive.Duration > plannableIncident.Incident.Type.MaximumResponseTime)
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
    /// <paramref name="handlingShifts"/> needs to be handling shifts, otherwise undefined behaviour happens.
    /// </summary>
    public Shift GetBestShift(List<Shift> handlingShifts, Incident incident)
    {
        Shift bestShift = handlingShifts.First();
        foreach (Shift shift in handlingShifts)
        {
            bestShift = GetBetter(bestShift, shift, incident);
        }

        return bestShift;
    }

    /// <summary>
    /// Rreturns better shift out of the two based on defined conditions.
    /// If both shifts are equaly good, <paramref name="shift1"/> is returned.
    /// </summary>
    public Shift GetBetter(Shift shift1, Shift shift2, Incident incident)
    {
        // 1
        if (!shift1.IsFree(incident.Occurence) || !shift2.IsFree(incident.Occurence))
        {
            return shift1.IsFree(incident.Occurence) ? shift1 : shift2;
        }

        // 2
        PlannableIncident shift1PlannableIncident = plannableIncidentFactory.Get(incident, shift1);
        PlannableIncident shift2PlannableIncident = plannableIncidentFactory.Get(incident, shift2);

        if (shift1PlannableIncident.ToIncidentDrive.End != shift2PlannableIncident.ToIncidentDrive.End)
        {
            return shift1PlannableIncident.ToIncidentDrive.End < shift2PlannableIncident.ToIncidentDrive.End ? shift1 : shift2;
        }

        // 3
        if (shift1.TimeActive() != shift2.TimeActive())
        {
            return shift1.TimeActive() < shift2.TimeActive() ? shift1 : shift2;
        }

        // 4
        return shift1.Ambulance.Type.Cost <= shift2.Ambulance.Type.Cost ? shift1 : shift2;
    }
}
