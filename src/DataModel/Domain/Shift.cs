using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataModel.Interfaces;
using Model.Extensions;
using Newtonsoft.Json;

namespace ESSP.DataModel;
public class Shift : IIdentifiable
{
    private static uint IdGenerator = 1;

    public uint Id { get; init; }

    public Ambulance Ambulance { get; init; }

    public Depot Depot { get; init; }

    public Interval Work { get; set; }

    public IReadOnlyList<PlannableIncident> PlannedIncidents => plannedIncidents;

    private List<PlannableIncident> plannedIncidents = new();

    public Shift(Ambulance ambulance, Depot depot, Interval work)
    {
        Ambulance = ambulance;
        Work = work;
        Depot = depot;

        Id = IdGenerator++;
    }

    public bool IsInDepot(Seconds currentTime)
    {
        if (PlannedIncidents.Count == 0)
        {
            return true;
        }

        return PlannedIncidents.Last().ToDepotDrive.End <= currentTime;
    }

    public bool IsFree(Seconds currentTime)
    {
        return WhenFree(currentTime) == currentTime;
    }

    /// <summary>
    /// Returns either currentTime or when the shift starts driving to depot from handled incident, depending which is earlier.
    /// </summary>
    public Seconds WhenFree(Seconds currentTime)
    {
        if (PlannedIncidents.Count == 0)
        {
            return currentTime;
        }

        Seconds toDepotDriveStart = PlannedIncidents.Last().ToDepotDrive.Start;
        return toDepotDriveStart < currentTime ? currentTime : toDepotDriveStart;
    }

    public Seconds TimeActive()
    {
        if (PlannedIncidents.Count == 0)
        {
            return 0.ToSeconds();
        }

        return PlannedIncidents.Sum(inc => inc.IncidentHandling.Duration.Value).ToSeconds();
    }

    public void Plan(PlannableIncident currentIncident)
    {
        Debug.Assert(Interval.GetByStartAndEnd(currentIncident.ToIncidentDrive.Start, currentIncident.ToDepotDrive.Start).IsSubsetOf(Work));

        plannedIncidents.Add(currentIncident);
    }

    /// <summary>
    /// Returns incident which is / was handled in <paramref name="currentTime"/>.
    /// If no incidents were planned on this shift at <paramref name="currentTime"/>, returns <see langword="null"/>.
    /// </summary>
    public PlannableIncident PlannedIncident(Seconds currentTime)
    {
        if (PlannedIncidents.Count == 0)
        {
            return null;
        }

        // always has only one element
        return PlannedIncidents.Where(inc => inc.WholeInterval.IsInInterval(currentTime)).FirstOrDefault();
    }

    public void ClearPlannedIncidents()
    {
        plannedIncidents.Clear();
    }

    public override string ToString()
    {
        return $"({Id}) AmbulanceLoc: {Ambulance.Location}, WorkStart: {Work.Start}, WorkEnd: {Work.End}, AmbulanceType: {Ambulance.Type.Name}, Cost: {Ambulance.Type.Cost}\n\tPlanned:\n{plannedIncidents.Visualize("\n", 1)}"; 
    }
}