using ESSP.DataModel;
using System.Collections.Generic;

namespace ESSP.DataModel;

public class Constraints
{
    public HashSet<Seconds> AllowedShiftStartingTimes { get; set; } = new();

    public HashSet<Seconds> AllowedShiftDurations { get; set; } = new();

    public Constraints(HashSet<Seconds> allowedShiftStartingTimes, HashSet<Seconds> allowedShiftDurations)
    {
        AllowedShiftStartingTimes = allowedShiftStartingTimes;
        AllowedShiftDurations = allowedShiftDurations;
    }

    public Constraints() { }
}
