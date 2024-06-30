using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulating;

public sealed class Simulation
{
  public World World { get; }

  public EmergencyServicePlan EmergencyServicePlan { get; set; }

  public bool Info { get; set; }

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

  public Simulation(World world, bool info = false)
  {
    World = world;
    _plannableIncidentFactory = new PlannableIncident.Factory(world.DistanceCalculator, world.Hospitals);
    _medicTeamsEvaluator = new MedicTeamsEvaluator(world.DistanceCalculator, world.Hospitals, world.GoldenTimeSec);
    Info = info;

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
    ResetState(incidents.Length);

    PlannableIncident plannableIncidentForBestShift;
    Incident currentIncident;
    MedicTeam bestMedicTeam;
    MedicTeam medicTeam;

    for (int i = 0; i < incidents.Length; ++i)
    {
      currentIncident = incidents[i];

      // Has to be assigned to something in order to compile, but it will be reassigned to first handling shift found.
      // If not, than the other loop won't happen, therefore it's value is irrelevant.
      bestMedicTeam = default(MedicTeam);

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
        if (Info) UnhandledIncidents.Add(i);
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

      plannableIncidentForBestShift = _plannableIncidentFactory.Get(in currentIncident, bestMedicTeam);
      if (Info) bestMedicTeam.PlanAndAddToHistory(plannableIncidentForBestShift); else bestMedicTeam.PlanEfficient(plannableIncidentForBestShift);
    }
  }

  private void ResetState(int incidentsCount)
  {
    // Clear planned incidents and last planned incident from previous iterations.
    for (int i = 0; i < EmergencyServicePlan.AllocatedMedicTeamsCount; ++i)
    {
      EmergencyServicePlan.AvailableMedicTeams[i].ClearPlannedIncidents();
      EmergencyServicePlan.AvailableMedicTeams[i].ResetLastPlannedIncident();
      EmergencyServicePlan.AvailableMedicTeams[i].TimeActiveSec = 0;
    }

    // Set WhenFree to 0 seconds to all ambulances
    for (int i = 0; i < EmergencyServicePlan.AllocatedAmbulancesCount; ++i)
    {
      EmergencyServicePlan.AvailableAmbulances[i].WhenFreeSec = 0;
    }

    if (Info)
    {
      UnhandledIncidents.Clear();
    }

    NotHandledIncidentsCount = 0;
    TotalIncidentsCount = incidentsCount;
  }
}
