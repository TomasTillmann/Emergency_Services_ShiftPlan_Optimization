using ESSP.DataModel;

namespace Optimizing;

/// <summary>
/// Samples plans.
/// </summary>
public interface IPlanSampler
{
    /// <summary>
    /// Samples a plan.
    /// </summary>
    EmergencyServicePlan Sample();
}