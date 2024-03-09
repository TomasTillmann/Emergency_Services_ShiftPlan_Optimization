using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace SimulatingOptimized;

public sealed class SimulationOptimized
{
  public ImmutableArray<DepotOpt> Depots { get; }
  public DistanceCalculatorOpt DistanceCalculator { get; }

  public int CurrentTimeSec { get; private set; } = 0;

  private readonly ShiftEvaluatorOpt _shiftEvaluator;
  private readonly PlannableIncidentOpt.Factory _plannableIncidentFactory;

  public SimulationOptimized(WorldOpt world) : this(world.Depots, world.Hospitals, world.IncTypeToAllowedAmbTypesTable, world.DistanceCalculator) { }

  public SimulationOptimized(ImmutableArray<DepotOpt> depots, ImmutableArray<HospitalOpt> hospitals, IncTypeToAllowedAmbTypesTable ambToIncTypesTable, DistanceCalculatorOpt distanceCalculator)
  {
    Depots = depots;
    DistanceCalculator = distanceCalculator;

    _plannableIncidentFactory = new PlannableIncidentOpt.Factory(DistanceCalculator, hospitals);
    _shiftEvaluator = new ShiftEvaluatorOpt(distanceCalculator, hospitals, ambToIncTypesTable);
  }

  public ShiftPlanOpt Run(IncidentOpt[] incidents)
  {
    ShiftPlanOpt simulateOnThisShiftPlan = ShiftPlanOpt.GetFrom(Depots, incidents.Length);

    // Prepare shiftPlan for simulation.  
    simulateOnThisShiftPlan.ClearPlannedIncidents();

    // Sort in order to simulate incidents in order of occurence
    Array.Sort(incidents, (x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));

    for (int i = 0; i < incidents.Length; ++i)
    {
      // ref
      IncidentOpt currentIncident = incidents[i];
      CurrentTimeSec = currentIncident.OccurenceSec;

      Step(in currentIncident, simulateOnThisShiftPlan);
    }

    return simulateOnThisShiftPlan;
  }

  private void Step(in IncidentOpt currentIncident, ShiftPlanOpt simulateOnThisShiftPlan)
  {
    //TODO: is this O(1)?
    //Span<ShiftOpt> shifts = simulateOnThisShiftPlan.Shifts.AsSpan();

    ShiftOpt[] shifts = simulateOnThisShiftPlan.Shifts;

    // Has to be assigned to something in order to compile, but it will be reassigned to first handling shift found.
    // If not, than the other loop won't happen, therefore it's value is irrelevant.
    ShiftOpt bestShift = shifts[0];
    ShiftOpt shift;

    // find handlingShift
    int findBetterFromIndex = shifts.Length;
    for (int i = 0; i < shifts.Length; ++i)
    {
      shift = shifts[i];

      if (_shiftEvaluator.IsHandling(shift, in currentIncident))
      {
        bestShift = shift;
        findBetterFromIndex = i + 1;
        break;
      }
    }

    for (int i = findBetterFromIndex; i < shifts.Length; ++i)
    {
      shift = shifts[i];

      if (_shiftEvaluator.IsHandling(shift, in currentIncident))
      {
        bestShift = _shiftEvaluator.GetBetter(bestShift, shift, in currentIncident);
      }
    }

    // no hadnling shift exists
    if (findBetterFromIndex == shifts.Length)
    {
      // Do nothing, cannot plan this incident.
      // TODO: Update some stats
      return;
    }

    bestShift.Plan(_plannableIncidentFactory.GetCopy(in currentIncident, bestShift));
  }
}
