using ESSP.DataModel;
using System.Collections.Generic;

namespace ESSP.DataModel;

public class Domain
{
    public HashSet<Seconds> AllowedShiftStartingTimes { get; set; } = new();

    public HashSet<Seconds> AllowedShiftDurations { get; set; } = new();

    public Domain(HashSet<Seconds> allowedShiftStartingTimes, HashSet<Seconds> allowedShiftDurations)
    {
        AllowedShiftStartingTimes = allowedShiftStartingTimes;
        AllowedShiftDurations = allowedShiftDurations;
    }

    public Domain() { }
}
