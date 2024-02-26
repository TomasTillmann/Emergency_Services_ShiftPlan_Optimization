using ESSP.DataModel;

namespace Optimizing;

/// <summary>
/// Implement this interface if you want your optimizer to be debugged step by step, and to expose vital parts of it's changing internal state to the caller.
/// </summary>
public interface IStepOptimizer : IOptimizer
{
    /// <summary>
    /// Found optimal shift plan. Is set only after <see cref="IsFinished" />.
    /// </summary>
    ShiftPlan OptimalShiftPlan { get; }
    
    /// <summary>
    /// Current step number.
    /// </summary>
    int CurrStep { get; }

    /// <summary>
    /// Call before first call to <see cref="Step" />.
    /// </summary>
    void StepThroughInit(List<SuccessRatedIncidents> incidentsSets);

    /// <summary>
    /// Does one move in the space.
    /// </summary>
    void Step();

    /// <summary>
    /// Runs step until it's finished. More efficient than doing Steps for yourself, since for example, the stats are not updated.
    /// </summary>
    void Run()
    {
        while (!IsFinished())
        {
            Step();
        }
    }
    
    /// <summary>
    /// Whether the optimizer finished and optimalShift is found and initialized.
    /// </summary>
    /// <returns></returns>
    bool IsFinished();
}

public interface ILocalSearchStepOptimizer : IStepOptimizer
{
    ShiftPlan StartShiftPlan { get; set; }
}