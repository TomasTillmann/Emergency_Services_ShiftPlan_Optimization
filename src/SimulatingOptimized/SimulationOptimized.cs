using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace SimulatingOptimized;

public sealed class SimulationOptimized
{
  public ImmutableArray<Depot> Depots { get; }
  public DistanceCalculator DistanceCalculator { get; }

  public int CurrentTimeSec { get; private set; } = 0;

  private readonly ShiftEvaluator _shiftEvaluator;
  private readonly PlannableIncident.Factory _plannableIncidentFactory;

  public SimulationOptimized(World world) : this(world.Depots, world.Hospitals, world.IncTypeToAllowedAmbTypesTable, world.DistanceCalculator) { }

  public SimulationOptimized(ImmutableArray<Depot> depots, ImmutableArray<Hospital> hospitals, IncTypeToAllowedAmbTypesTable ambToIncTypesTable, DistanceCalculator distanceCalculator)
  {
    Depots = depots;
    DistanceCalculator = distanceCalculator;

    _plannableIncidentFactory = new PlannableIncident.Factory(DistanceCalculator, hospitals);
    _shiftEvaluator = new ShiftEvaluator(distanceCalculator, hospitals, ambToIncTypesTable);
  }

  /// <param name="incidents"/> have to be sorted in order of <see cref="Incident.OccurenceSec"/>
  /// <param name="simulateOnThisShiftPlan"/> needs to have cleared PlannedIncidents, by <see cref="ShiftPlan.ClearAllPlannedIncidents()" 
  public void Run(ImmutableArray<Incident> incidents, ShiftPlan simulateOnThisShiftPlan)
  {
    // Prepare shiftPlan for simulation.  
    // simulateOnThisShiftPlan.ClearPlannedIncidents();

    // Sort in order to simulate incidents in order of occurence
    // Array.Sort(incidents, (x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));

    for (int i = 0; i < incidents.Length; ++i)
    {
      // ref
      Incident currentIncident = incidents[i];
      CurrentTimeSec = currentIncident.OccurenceSec;

      Step(in currentIncident, simulateOnThisShiftPlan);
    }
  }

  private void Step(in Incident currentIncident, ShiftPlan simulateOnThisShiftPlan)
  {
    //TODO: is this O(1)?
    //Span<ShiftOpt> shifts = simulateOnThisShiftPlan.Shifts.AsSpan();

    Shift[] shifts = simulateOnThisShiftPlan.Shifts;

    // Has to be assigned to something in order to compile, but it will be reassigned to first handling shift found.
    // If not, than the other loop won't happen, therefore it's value is irrelevant.
    Shift bestShift = shifts[0];
    Shift shift;

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
