using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

  public int GetCost()
  {
    return Shifts.Select(shift => shift.Ambulance.Type.Cost * shift.Work.DurationSec).Sum();
  }

  //HACK: dirty implementation with the weights
  public static ShiftPlan GetFrom(ImmutableArray<Depot> depots, int incidentsSize, Weights? weights = null)
  {
    List<Shift> shifts = new();

    int i = 0;
    foreach (Depot depot in depots)
    {
      foreach (Ambulance ambulance in depot.Ambulances)
      {
        shifts.Add(
            new Shift(incidentsSize)
            {
              Ambulance = ambulance,
              Depot = depot,
              Work = weights is null ? Interval.GetByStartAndDuration(0.ToSeconds().Value, 24.ToHours().ToMinutes().ToSeconds().Value) : weights.Value[i++]
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

