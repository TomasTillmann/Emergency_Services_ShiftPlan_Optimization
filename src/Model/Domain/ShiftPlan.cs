using DataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESSP.DataModel;
public class ShiftPlan
{
    public IList<Shift> Shifts { get; }

    public ShiftPlan(IList<Shift> shifts)
    {
        Shifts = shifts;
    }

    public double GetCost()
    {
        throw new NotImplementedException();
    }
}