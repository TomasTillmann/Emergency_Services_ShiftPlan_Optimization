using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulating;

public sealed class Simulation : ISimulation
{
  public ImmutableArray<Depot> Depots { get; }
  public DistanceCalculator DistanceCalculator { get; }

  /// <summary>
  /// Success rate of last run simulation.
  /// </summary>
  public double SuccessRate => (_totalIncidentsCount - _notHandledIncidentsCount) / (double)_totalIncidentsCount;

  /// <summary>
  /// Current time of the occuring incident. 
  /// </summary>
  public int CurrentTimeSec { get; private set; }

  /// <summary>
  /// Total count of incidents the simulation was run on. 
  /// </summary>
  public int TotalIncidents => _totalIncidentsCount;

  public int NotHandledIncidents => _notHandledIncidentsCount;

  public int HandledIncidents => _totalIncidentsCount - _notHandledIncidentsCount;

  public List<Incident> UnhandledIncidents = new();

  private readonly ShiftEvaluator _shiftEvaluator;
  private readonly PlannableIncident.Factory _plannableIncidentFactory;

  private int _totalIncidentsCount;
  private int _notHandledIncidentsCount;

  public Simulation(World world)
  : this(world.Depots, world.Hospitals, world.IncTypeToAllowedAmbTypesTable, world.DistanceCalculator) { }

  public Simulation
  (
     ImmutableArray<Depot> depots,
     ImmutableArray<Hospital> hospitals,
     IncTypeToAllowedAmbTypesTable ambToIncTypesTable,
     DistanceCalculator distanceCalculator
  )
  {
    Depots = depots;
    DistanceCalculator = distanceCalculator;

    _plannableIncidentFactory = new PlannableIncident.Factory(DistanceCalculator, hospitals);
    _shiftEvaluator = new ShiftEvaluator(distanceCalculator, hospitals, ambToIncTypesTable);
  }

  /// <param name="incidents"/> have to be sorted in order of <see cref="Incident.OccurenceSec"/>
  /// <param name="simulateOnThisShiftPlan"/> Will clear PlannedIncidents, by <see cref="ShiftPlan.ClearAllPlannedIncidents()" 
  public void Run(ImmutableArray<Incident> incidents, ShiftPlan simulateOnThisShiftPlan)
  {
    simulateOnThisShiftPlan.ClearPlannedIncidents();
    _totalIncidentsCount = incidents.Length;
    _notHandledIncidentsCount = 0;

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
    ImmutableArray<Shift> shifts = simulateOnThisShiftPlan.Shifts;

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
      _notHandledIncidentsCount++;
      UnhandledIncidents.Add(currentIncident); // remove for faster perf
      return;
    }

    bestShift.Plan(_plannableIncidentFactory.GetCopy(in currentIncident, bestShift));
  }
}
