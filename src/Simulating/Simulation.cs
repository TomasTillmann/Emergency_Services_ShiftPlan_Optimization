using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulating;

public sealed class Simulation : ISimulation
{
  public World World { get; }

  public EmergencyServicePlan EmergencyServicePlan { get; }

  /// <summary>
  /// Total count of incidents the simulation was run on. 
  /// </summary>
  public int TotalIncidents { get; private set; }

  public int NotHandledIncidents { get; private set; }

  public int HandledIncidents => TotalIncidents - NotHandledIncidents;

  /// <summary>
  /// Success rate of last run simulation.
  /// </summary>
  public double SuccessRate => (TotalIncidents - NotHandledIncidents) / (double)TotalIncidents;

  public List<Incident> UnhandledIncidents { get; } = new();

  private readonly MedicTeamsEvaluator _medicTeamsEvaluator;
  private readonly PlannableIncident.Factory _plannableIncidentFactory;

  public Simulation(World world)
  {
    World = world;
    _plannableIncidentFactory = new PlannableIncident.Factory(world.DistanceCalculator, world.Hospitals);
    _medicTeamsEvaluator = new MedicTeamsEvaluator(world.DistanceCalculator, world.Hospitals, world.GoldenTimeSec);

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
  public void Run(ImmutableArray<Incident> incidents)
  {
    // Clear planned incidents from previous iterations.
    for (int i = 0; i < EmergencyServicePlan.AllocatedMedicTeamsCount; ++i)
    {
      EmergencyServicePlan.AvailableMedicTeams[i].ClearPlannedIncidents();
      EmergencyServicePlan.AvailableMedicTeams[i].TimeActive = 0;
    }

    // Set WhenFree to 0 seconds to all ambulances
    for (int i = 0; i < EmergencyServicePlan.AllocatedAmbulancesCount; ++i)
    {
      EmergencyServicePlan.AvailableAmbulances[i].WhenFreeSec = 0;
    }

    UnhandledIncidents.Clear(); // delete for faster perf, only for debuggin purposes

    NotHandledIncidents = 0;
    TotalIncidents = incidents.Length;

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
        NotHandledIncidents++;
        UnhandledIncidents.Add(currentIncident); // remove for faster perf
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
