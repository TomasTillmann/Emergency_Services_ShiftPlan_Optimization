using System;
using System.Collections.Generic;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulation;

class ShiftEvaluator
{
    PlannableIncident.Factory plannableIncidentFactory;

    public ShiftEvaluator(PlannableIncident.Factory plannableIncidentFactory)
    {
        this.plannableIncidentFactory = plannableIncidentFactory;
    }

    public bool IsHandling(Shift shift, Incident incident, Seconds currentTime)
    {
        PlannableIncident plannableIncident = plannableIncidentFactory.Get(incident, shift, shift.WhenFree(currentTime));

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
        PlannableIncident shift1PlannableIncident = plannableIncidentFactory.Get(incident, shift1, shift1.WhenFree(currentTime));
        PlannableIncident shift2PlannableIncident = plannableIncidentFactory.Get(incident, shift2, shift2.WhenFree(currentTime));

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
