using ESSP.DataModel;

namespace Optimizing;

public interface IPlanSampler
{
  EmergencyServicePlan Sample();
}

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

  public abstract EmergencyServicePlan Sample();
}
