using ESSP.DataModel;
using System.Collections.Generic;

namespace ESSP.DataModel;

public class Constraints
{
    public IReadOnlyList<Seconds> AllowedShiftStartingTimes { get; }

    public IReadOnlyList<Seconds> AllowedShiftDurations { get; }

    public Constraints(IReadOnlyList<Seconds> allowedShiftStartingTimes, IReadOnlyList<Seconds> allowedShiftDurations)
    {
        AllowedShiftStartingTimes = allowedShiftStartingTimes;
        AllowedShiftDurations = allowedShiftDurations;
    }
}
