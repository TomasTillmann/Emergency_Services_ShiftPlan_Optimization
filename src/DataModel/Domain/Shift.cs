using System.Collections.Generic;
using System.Linq;

namespace ESSP.DataModel;

public class Shift
{
  private static long _nextId = 1;

  public long Id { get; init; }
  public Ambulance Ambulance { get; init; }
  public Depot Depot { get; init; }

  public Interval Work { get; set; } = Interval.GetByStartAndDuration(0, 24.ToHours().ToSeconds().Value);

  public int TimeActive { get; private set; }

  private List<PlannableIncident> _plannedIncidents { get; init; }

  public Shift()
  {
    _plannedIncidents = new List<PlannableIncident>();

    Id = _nextId++;
  }

  public void Plan(PlannableIncident plannableIncident)
  {
    _plannedIncidents.Add(plannableIncident);
    TimeActive += plannableIncident.IncidentHandling.DurationSec;
  }

  public PlannableIncident GetCurrentlyHandlingIncident()
  {
    return _plannedIncidents.Last();
  }

  public bool IsInDepot(int currentTimeSec)
  {
    if (!_plannedIncidents.Any())
    {
      return true;
    }

    return _plannedIncidents.Last().ToDepotDrive.EndSec <= currentTimeSec;
  }

  /// <summary>
  /// Returns true if shift is free in current time and false if not.
  /// </summary>
  public bool IsFree(int currentTimeSec)
  {
    return WhenFree(currentTimeSec) == currentTimeSec;
  }

  /// <summary>
  /// Returns when the shift is free.
  /// Returns either currentTime, when the shift is free in currentTime,
  /// or when the shift starts driving to depot from handled incident, depending which is earlier.
  /// </summary>
  public int WhenFree(int currentTimeSec)
  {
    if (!_plannedIncidents.Any())
    {
      return currentTimeSec;
    }

    int toDepotDriveStartSec = _plannedIncidents.Last().ToDepotDrive.StartSec;
    return toDepotDriveStartSec < currentTimeSec ? currentTimeSec : toDepotDriveStartSec;
  }

  /// <summary>
  /// Used only in <see cref="Visualizer"/>
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
}

