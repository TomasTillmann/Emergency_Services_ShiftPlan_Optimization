﻿using System;
using System.Collections.Generic;
using System.Linq;
using DataModel.Interfaces;
using Newtonsoft.Json;

namespace ESSP.DataModel;
public class Shift : IIdentifiable
{
    [JsonIgnore]
    private static uint IdGenerator = 1;

    public uint Id { get; }

    public Ambulance Ambulance { get; }

    public Depot Depot { get; }

    public Interval Work { get; internal set; }

    public IReadOnlyList<PlannableIncident> PlannedIncidents => plannedIncidents;

    private List<PlannableIncident> plannedIncidents = new();

    public Shift(Ambulance ambulance, Depot depot, Interval available)
    {
        Ambulance = ambulance;
        Work = available;
        Depot = depot;

        Id = IdGenerator++;
    }

    public bool IsInDepot(Seconds currentTime)
    {
        if (PlannedIncidents.Count == 0)
        {
            return true;
        }

        return PlannedIncidents.Last().ToDepotDrive.End >= currentTime;
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

        return PlannedIncidents.Select(inc => inc.IncidentHandling.Duration.Value).Sum().ToSeconds();
    }

    public void Plan(PlannableIncident currentIncident)
    {
        plannedIncidents.Add(currentIncident);
    }

    /// <summary>
    /// Returns incident which is / was handled in <paramref name="time"/>.
    /// If no incidents were planned on this shift, returns <see langword="switch"/>.
    /// </summary>
    public PlannableIncident PlannedIncident(Seconds time)
    {
        if (PlannedIncidents.Count == 0)
        {
            return null;
        }

        // always has only one element
        return PlannedIncidents.Where(inc => inc.WholeInterval.Contains(time)).First();
    }
}