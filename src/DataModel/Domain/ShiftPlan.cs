using Model.Extensions;
using Optimizing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESSP.DataModel;

public class ShiftPlan : IShifts
{
  public List<Shift> Shifts { get; set; }

  public int Count => Shifts.Count;
  public Shift this[int index] { get => Shifts[index]; set => Shifts[index] = value; }

  public ShiftPlan(List<Shift> shifts)
  {
    Shifts = shifts;
  }

  public static ShiftPlan ConstructRandom(IReadOnlyList<Depot> depots, List<Seconds> allowedStartingTimes, List<Seconds> allowedShiftDurations, Random? random = null)
  {
    ShiftPlan shiftPlanDefault = ConstructEmpty(depots);
    foreach (Shift shift in shiftPlanDefault.Shifts)
    {
      shift.Work = Interval.GetByStartAndDuration(allowedStartingTimes.GetRandomElement(random), allowedShiftDurations.GetRandomElement(random));
    }

    return shiftPlanDefault;
  }

  public static ShiftPlan ConstructFrom(IReadOnlyList<Depot> depots, Seconds allShiftsStartingTime, Seconds allShiftsDuration)
  {
    ShiftPlan defaultShiftPlan = ConstructEmpty(depots);
    foreach (Shift shift in defaultShiftPlan.Shifts)
    {
      shift.Work = Interval.GetByStartAndDuration(allShiftsStartingTime, allShiftsDuration);
    }

    return defaultShiftPlan;
  }

  public static ShiftPlan ConstructEmpty(IReadOnlyList<Depot> depots)
  {
    List<Shift> shifts = new();

    foreach (Depot depot in depots)
    {
      foreach (Ambulance ambulance in depot.Ambulances)
      {
        shifts.Add(new Shift(ambulance, depot, Interval.Empty));
      }
    }

    return new ShiftPlan(shifts);
  }

  public int GetCost()
  {
    return Shifts.Select(shift => shift.Ambulance.Type.Cost * shift.Work.Duration.Value).Sum();
  }

  public void ClearAllPlannedIncidents()
  {
    Shifts.ForEach(shift => shift.ClearPlannedIncidents());
  }

  public ShiftPlan Copy()
  {
    return new ShiftPlan(Shifts.Select(shift => new Shift(shift.Ambulance, shift.Depot, shift.Work)).ToList());
  }

  public override string ToString()
  {
    return Shifts.Visualize(toString: shift => $"{shift.Work.Start}-{shift.Work.End}");
  }
}
