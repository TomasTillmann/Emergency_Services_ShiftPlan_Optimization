using ESSP.DataModel;

namespace Optimizing;

/// <summary>
/// Samples random plan satisfying constraints.
/// </summary>
public abstract class PlanSampler : IPlanSampler
{
  public World World { get; set; }
  protected Random Random { get; }
  protected ShiftTimes ShiftTimes { get; }
  protected Constraints Constraints { get; }

  public PlanSampler(World world, ShiftTimes shiftTimes, Constraints constraints, Random? random = null)
  {
    Random = random ?? new Random();
    World = world;
    ShiftTimes = shiftTimes;
    Constraints = constraints;
  }

  /// <inheritdoc />
  public abstract EmergencyServicePlan Sample();
}
