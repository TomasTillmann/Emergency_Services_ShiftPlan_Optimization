using ESSP.DataModel;

namespace Optimizing;

public abstract class OptimizerBase : IOptimizer
{
  public IUtilityFunction UtilityFunction { get; set; }
  public World World { get; set; }
  public Constraints Constraints { get; set; }


  protected EmergencyServicePlan EmptyPlan { get; }
  protected List<MedicTeam> AvailableMedicTeams { get; init; }
  protected List<Ambulance> AvailableAmbulances { get; init; }

  public OptimizerBase(World world, Constraints constraints, IUtilityFunction utilityFunction)
  {
    World = world;
    Constraints = constraints;
    UtilityFunction = utilityFunction;

    EmptyPlan = EmergencyServicePlan.GetNewEmpty(world);
  }

  public abstract List<EmergencyServicePlan> GetBest(ReadOnlySpan<Incident> incidents);
}
