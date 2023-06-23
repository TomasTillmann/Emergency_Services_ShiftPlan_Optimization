using ESSP.DataModel;

namespace Optimizing;

public interface IShifts
{
    int Count { get; }
    Shift this[int index] { get; set; }
    void ClearAllPlannedIncidents();
    int GetCost();
}
