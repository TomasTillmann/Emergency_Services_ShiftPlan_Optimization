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

    public double GetCost()
    {
        return Shifts.Select(shift => shift.Ambulance.Type.Cost * shift.Work.Duration.Value).Sum();
    }

    public override string ToString()
    {
        return Shifts.Visualize(toString: shift => $"{shift.Work.Start}-{shift.Work.End}"); 
    }
}