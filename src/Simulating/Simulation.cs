using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulating;

public sealed class Simulation
{
  public World World { get; }

  public EmergencyServicePlan EmergencyServicePlan { get; }

  /// <summary>
  /// Total count of incidents the simulation was run on. 
  /// </summary>
  public int TotalIncidentsCount { get; private set; }

  public int NotHandledIncidentsCount { get; private set; }

  public int HandledIncidentsCount => TotalIncidentsCount - NotHandledIncidentsCount;

  /// <summary>
  /// Success rate of last run simulation.
  /// </summary>
  public double SuccessRate => (TotalIncidentsCount - NotHandledIncidentsCount) / (double)TotalIncidentsCount;

  public List<int> UnhandledIncidents { get; } = new();

  private readonly MedicTeamsEvaluator _medicTeamsEvaluator;
  private readonly PlannableIncident.Factory _plannableIncidentFactory;
  private readonly bool _info;

  public Simulation(World world, bool info = false)
  {
    World = world;
    _plannableIncidentFactory = new PlannableIncident.Factory(world.DistanceCalculator, world.Hospitals);
    _medicTeamsEvaluator = new MedicTeamsEvaluator(world.DistanceCalculator, world.Hospitals, world.GoldenTimeSec);
    _info = info;

    EmergencyServicePlan = new EmergencyServicePlan
    {
      AvailableMedicTeams = World.AvailableMedicTeams,
      AvailableAmbulances = World.AvailableAmbulances,
      Depots = World.Depots
    };
  }

  /// <summary>
  /// Performs simulation on Depots.
  /// </summary>
  public void Run(ReadOnlySpan<Incident> incidents)
  {
    // Clear planned incidents from previous iterations.
    for (int i = 0; i < EmergencyServicePlan.AllocatedMedicTeamsCount; ++i)
    {
      EmergencyServicePlan.AvailableMedicTeams[i].ClearPlannedIncidents();
      EmergencyServicePlan.AvailableMedicTeams[i].TimeActiveSec = 0;
    }

    // Set WhenFree to 0 seconds to all ambulances
    for (int i = 0; i < EmergencyServicePlan.AllocatedAmbulancesCount; ++i)
    {
      EmergencyServicePlan.AvailableAmbulances[i].WhenFreeSec = 0;
    }

    if (_info)
    {
      UnhandledIncidents.Clear();
    }

    NotHandledIncidentsCount = 0;
    TotalIncidentsCount = incidents.Length;

    for (int i = 0; i < incidents.Length; ++i)
    {
      Incident currentIncident = incidents[i];

      // Has to be assigned to something in order to compile, but it will be reassigned to first handling shift found.
      // If not, than the other loop won't happen, therefore it's value is irrelevant.
      MedicTeam bestMedicTeam = default(MedicTeam);
      MedicTeam medicTeam;

      // Find handling medic team. 
      int findBetterFromIndex = int.MaxValue;
      for (int j = 0; j < EmergencyServicePlan.AllocatedMedicTeamsCount; ++j)
      {
        medicTeam = EmergencyServicePlan.AvailableMedicTeams[j];

        if (_medicTeamsEvaluator.IsHandling(medicTeam, in currentIncident))
        {
          bestMedicTeam = medicTeam;
          findBetterFromIndex = j + 1;
          break;
        }
      }

      // No hadnling medic team exists. 
      if (findBetterFromIndex == int.MaxValue)
      {
        NotHandledIncidentsCount++;
        if (_info) UnhandledIncidents.Add(i);
        continue;
      }

      for (int j = findBetterFromIndex; j < EmergencyServicePlan.AllocatedMedicTeamsCount; ++j)
      {
        medicTeam = EmergencyServicePlan.AvailableMedicTeams[j];

        if (_medicTeamsEvaluator.IsHandling(medicTeam, in currentIncident))
        {
          bestMedicTeam = _medicTeamsEvaluator.GetBetter(bestMedicTeam, medicTeam, in currentIncident);
        }
      }

      //TODO: heap allocation all the time, this is bad.
      bestMedicTeam.Plan(_plannableIncidentFactory.GetCopy(in currentIncident, bestMedicTeam));
    }
  }
}
