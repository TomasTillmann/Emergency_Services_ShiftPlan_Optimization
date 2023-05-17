using ESSP.DataModel;
using System.Collections.Generic;

namespace ESSP.DataModel;

public class Constraints
{
    public List<Seconds> AllowedShiftStartingTimes { get; set; } = new();

    public List<Seconds> AllowedShiftDurations { get; set; } = new();

    public Constraints(List<Seconds> allowedShiftStartingTimes, List<Seconds> allowedShiftDurations)
    {
        AllowedShiftStartingTimes = allowedShiftStartingTimes;
        AllowedShiftDurations = allowedShiftDurations;
    }

    public Constraints() { }
}
