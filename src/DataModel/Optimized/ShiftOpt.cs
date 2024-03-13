using System;

namespace ESSP.DataModel;

// TODO: Maybe make a struct, to use caches when iterating over. But maybe it is redundant.
public class ShiftOpt
{
  private static long _nextId = 1;

  public long Id { get; init; }
  public AmbulanceOpt Ambulance { get; init; }
  public DepotOpt Depot { get; init; }
  public IntervalOpt Work { get; init; } = IntervalOpt.GetByStartAndDuration(0, 24.ToHours().ToSeconds().Value);

  public int TimeActive { get; private set; }

  private int _index;
  private PlannableIncidentOpt[] _plannedIncidents { get; init; }

  public ShiftOpt(int plannedIncidentsSize)
  {
    _plannedIncidents = new PlannableIncidentOpt[plannedIncidentsSize];
    _index = -1;

    Id = _nextId++;
  }

  public void Plan(PlannableIncidentOpt plannableIncident)
  {
    _plannedIncidents[++_index] = plannableIncident;
    TimeActive += plannableIncident.IncidentHandling.Duration;
  }

  public PlannableIncidentOpt GetCurrentlyHandlingIncident()
  {
    return _plannedIncidents[_index];
  }

  public bool IsInDepot(int currentTimeSec)
  {
    if (_index == -1)
    {
      return true;
    }

    return _plannedIncidents[_index].ToDepotDrive.EndSec <= currentTimeSec;
  }

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
    if (_index == -1)
    {
      return currentTimeSec;
    }

    int toDepotDriveStartSec = _plannedIncidents[_index].ToDepotDrive.StartSec;
    return toDepotDriveStartSec < currentTimeSec ? currentTimeSec : toDepotDriveStartSec;
  }

  /// <summary>
  /// Returns incident which is / was handled in <paramref name="currentTime"/>.
  /// If no incidents were planned on this shift at <paramref name="currentTime"/>, returns <see langword="null"/>.
  /// </summary>
  public PlannableIncidentOpt PlannedIncident(int currentTimeSec)
  {
    //TODO: this method is not called anywhere
    if (_index == -1)
    {
      return null;
    }

    for (int i = 0; i < _index; ++i)
    {
      if (_plannedIncidents[i].WholeInterval.IsInInterval(currentTimeSec))
      {
        return _plannedIncidents[i];
      }
    }

    return null;
  }

  public void ClearPlannedIncidents()
  {
    Array.Clear(_plannedIncidents);
  }
}

