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

    public List<Shift> GetHandlingShifts(List<Shift> shifts, Incident currentIncident, SimulationState state)
    {
        List<Shift> handlingShifts = new();
        foreach (Shift shift in shifts)
        {
            if (IsHandling(shift, currentIncident, state.CurrentTime))
            {
                handlingShifts.Add(shift);
            }
        }

        return handlingShifts;
    }

    public bool IsHandling(Shift shift, Incident incident, Seconds currentTime)
    {
        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift);

        // 1
        if (plannableIncident.ToIncidentDrive.Duration > incident.Type.MaximumResponseTime)
        {
            return false;
        }

        // 1, 2, 3, 4
        if (plannableIncident.IncidentHandling.Duration > (shift.Work.End - currentTime))
        {
            return false;
        }

        return true;
    }

    public Shift GetBestShift(List<Shift> shifts, Incident currentIncident, SimulationState state)
    {
        Shift bestShift = shifts.First();
        foreach (Shift shift in shifts)
        {
            bestShift = GetBetter(shift, bestShift, currentIncident, state.CurrentTime);
        }

        return bestShift;
    }

    /// <summary>
    /// Rreturns better shift out of the two based on defined conditions.
    /// If both shifts are equaly good, <paramref name="shift1"/> is returned.
    /// </summary>
    public Shift GetBetter(Shift shift1, Shift shift2, Incident incident, Seconds currentTime)
    {
        // 1
        if (!shift1.IsFree(currentTime) || !shift2.IsFree(currentTime))
        {
            return shift1.IsFree(currentTime) ? shift1 : shift2;
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
