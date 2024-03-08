using DataModel.Interfaces;
using ESSP.DataModel;

namespace ESSP.SimulatingOptimized;

public sealed class SimulationOptimized
{
  public IReadOnlyList<Depot> Depots { get; }
  public IDistanceCalculator DistanceCalculator { get; }
  public Seconds CurrentTime { get; private set; } = 0.ToSeconds();

  private readonly ShiftEvaluator shiftEvaluator;
  private readonly PlannableIncident.Factory plannableIncidentFactory;

  public SimulationOptimized(World world)
  {
    Depots = world.Depots;
    DistanceCalculator = world.DistanceCalculator;

    plannableIncidentFactory = new PlannableIncident.Factory(DistanceCalculator, world.Hospitals);
    shiftEvaluator = new ShiftEvaluator(plannableIncidentFactory);
  }

  /// <summary>
  /// Runs the simulation for given <paramref name="shiftPlan"/> on given <paramref name="incidents"/>.
  /// Returns statistics, including success rate of given <paramref name="shiftPlan"/>.
  /// </summary>
  /// <param name="incidents">Have to be sorted in ascending order by occurence. It is not sorted nor checked internally for faster performance.</param>
  /// <param name="shiftPlan"></param>
  /// <returns></returns>
  public void Run(IReadOnlyList<Incident> incidents, ShiftPlan shiftPlan)
  {
    CurrentTime = 0.ToSeconds();

    foreach (Incident currentIncident in incidents)
    {
      CurrentTime = currentIncident.Occurence;
      Step(currentIncident);
    }
  }

  private void Step(Incident currentIncident)
  {
    Shift bestShift = null;
    foreach (Shift shift in shiftPlan.Shifts)
    {
      if (shiftEvaluator.IsHandling(shift, currentIncident))
      {
        if (bestShift is null)
        {
          bestShift = shift;
          continue;
        }

        bestShift = shiftEvaluator.GetBetter(bestShift, shift, currentIncident);
      }
    }

    if (bestShift is null)
    {
      statistics.SetUnhandled(currentIncident);
      return;
    }

    bestShift.Plan(plannableIncidentFactory.Get(currentIncident, bestShift));
  }
}
