using System;

namespace ESSP.DataModel;

public class MedicTeamState
{
  public int TimeActiveSec { get; set; }
  public PlannableIncident LastPlannedIncident { get; set; } = PlannableIncident.Factory.Empty;

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

  public void Clear()
  {
    TimeActiveSec = 0;
    LastPlannedIncident = Plann
  }
}

