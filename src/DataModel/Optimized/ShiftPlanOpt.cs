using System.Collections.Generic;
using System.Collections.Immutable;

namespace ESSP.DataModel;

public class ShiftPlanOpt
{
  public ShiftOpt[] Shifts { get; init; }

  public void ClearPlannedIncidents()
  {
    for (int i = 0; i < Shifts.Length; ++i)
    {
      Shifts[i].ClearPlannedIncidents();
    }
  }

  public static ShiftPlanOpt GetFrom(ImmutableArray<DepotOpt> depots, int incidentsSize)
  {
    List<ShiftOpt> shifts = new();

    foreach (DepotOpt depot in depots)
    {
      foreach (AmbulanceOpt ambulance in depot.Ambulances)
      {
        shifts.Add(
            new ShiftOpt(incidentsSize)
            {
              Ambulance = ambulance,
              Depot = depot,
            }
        );
      }
    }

    ShiftPlanOpt shiftPlan = new()
    {
      Shifts = shifts.ToArray()
    };

    return shiftPlan;
  }
}

