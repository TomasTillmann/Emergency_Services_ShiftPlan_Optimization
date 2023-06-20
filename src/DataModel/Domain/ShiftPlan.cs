using DataModel.Interfaces;
using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESSP.DataModel;

public class ShiftPlan
{
    public List<Shift> Shifts { get; set; }

    public ShiftPlan(List<Shift> shifts)
    {
        Shifts = shifts;
    }

    public static ShiftPlan ConstructFrom(IReadOnlyList<Depot> depots, Seconds allShiftsStartingTime, Seconds allShiftsDuration)
    {
        List<Shift> shifts = new();

        foreach (Depot depot in depots)
        {
            foreach (Ambulance ambulance in depot.Ambulances)
            {
                shifts.Add(new Shift(ambulance, depot, Interval.GetByStartAndDuration(allShiftsStartingTime, allShiftsDuration)));
            }
        }

        return new ShiftPlan(shifts); 
    }

    public double GetCost()
    {
        return Shifts.Select(shift => shift.Ambulance.Type.Cost * shift.Work.Duration.Value).Sum();
    }

    public ShiftPlan Clone()
    {
        return new ShiftPlan(Shifts.Select(shift => new Shift(shift.Ambulance, shift.Depot, shift.Work)).ToList());
    }

    public override string ToString()
    {
        return Shifts.Visualize(toString: shift => $"{shift.Work.Start}-{shift.Work.End}"); 
    }
}