using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ESSP.DataModel;

public class MedicTeam
{
  private static long _nextId = 1;

  public long Id { get; init; }

  public Interval Shift { get; set; } = Interval.GetByStartAndDuration(0, 0);
  public Depot Depot { get; set; }

  public int TimeActiveSec { get; set; }

  public PlannableIncident LastPlannedIncident { get; }

  private readonly List<PlannableIncident> _plannedIncidents;

  public MedicTeam()
  {
    _plannedIncidents = new List<PlannableIncident>();

    LastPlannedIncident = new PlannableIncident(PlannableIncident.Factory.Empty);
    LastPlannedIncident.ToDepotDrive = Interval.GetByStartAndEnd(0, 0);
    Id = _nextId++;
  }

  /// <summary>
  /// This is used when <see cref="Simulation"/> is in debug mode. Otherwise the efficient version is used. 
  /// This is useful in <see cref="Visualizer"/>, since it can than visuazlie the history of planned incidents of this medic team.
  /// </summary>
  public void PlanAndAddToHistory(PlannableIncident plannableIncident)
  {
    _plannedIncidents.Add(new PlannableIncident(plannableIncident));
    PlanEfficient(plannableIncident);
  }

  public void PlanEfficient(PlannableIncident plannableIncident)
  {
    LastPlannedIncident.FillFrom(plannableIncident);
    Depot.Ambulances[plannableIncident.AmbulanceIndex].WhenFreeSec = plannableIncident.ToDepotDrive.EndSec;
    TimeActiveSec += plannableIncident.IncidentHandling.DurationSec;
  }

  /// <summary>
  /// Returns true if shift is free in current time and false if not.
  /// </summary>
  public bool IsFree(int currentTimeSec) => WhenFree(currentTimeSec) == currentTimeSec;

  /// <summary>
  /// Returns when the shift is free.
  /// Returns either currentTime, when the shift is free in currentTime,
  /// or when the shift starts driving to depot from handled incident, depending which is earlier.
  /// </summary>
  public int WhenFree(int currentTimeSec) => IsInDepot(currentTimeSec) ? currentTimeSec : Math.Max(LastPlannedIncident.ToDepotDrive.StartSec, currentTimeSec);

  public bool IsInDepot(int currentTimeSec) => LastPlannedIncident.ToDepotDrive.EndSec <= currentTimeSec;

  /// <summary>
  /// Used only in <see cref="Visualizer"/>.
  /// Returns incident which is / was handled in <paramref name="currentTime"/>.
  /// If no incidents were planned on this shift at <paramref name="currentTime"/>, returns <see langword="null"/>.
  /// </summary>
  public PlannableIncident PlannedIncident(int currentTimeSec)
  {
    if (!_plannedIncidents.Any())
    {
      return null;
    }

    foreach (var inc in _plannedIncidents)
    {
      if (inc.WholeInterval.IsInInterval(currentTimeSec))
      {
        return inc;
      }
    }

    return null;
  }

  public void ClearPlannedIncidents()
  {
    _plannedIncidents.Clear();
  }

  public void ResetLastPlannedIncident()
  {
    LastPlannedIncident.FillFrom(PlannableIncident.Factory.Empty);
  }
}

