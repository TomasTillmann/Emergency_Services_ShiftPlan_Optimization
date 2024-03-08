namespace ESSP.DataModel;

public readonly ref struct ShiftOpt
{
  public AmbulanceOpt Ambulance { get; init; }
  public DepotOpt Depot { get; init; }
  public IntervalOpt Work { get; init; }

  public PlannableIncidentOpt[] PlannedIncidents { get; init; }

  public ShiftOpt(int plannedIncidentsSize)
  {
    PlannedIncidents = new PlannableIncidentOpt[plannedIncidentsSize];
  }
}

