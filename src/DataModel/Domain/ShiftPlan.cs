using System.Collections.Generic;
using System.Collections.Immutable;

namespace ESSP.DataModel;

public class ShiftPlan
{
  public ImmutableArray<Shift> Shifts { get; init; }

  public void ClearPlannedIncidents()
  {
    for (int i = 0; i < Shifts.Length; ++i)
    {
      Shifts[i].ClearPlannedIncidents();
    }
  }

  public static ShiftPlan GetFrom(ImmutableArray<Depot> depots, int incidentsSize)
  {
    List<Shift> shifts = new();

    foreach (Depot depot in depots)
    {
      foreach (Ambulance ambulance in depot.Ambulances)
      {
        shifts.Add(
            new Shift(incidentsSize)
            {
              Ambulance = ambulance,
              Depot = depot,
            }
        );
      }
    }

    ShiftPlan shiftPlan = new()
    {
      Shifts = shifts.ToImmutableArray()
    };

    return shiftPlan;
  }
}

