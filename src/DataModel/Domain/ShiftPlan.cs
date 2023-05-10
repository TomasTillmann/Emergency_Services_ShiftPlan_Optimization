using DataModel.Interfaces;
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
        throw new NotImplementedException();
    }
}