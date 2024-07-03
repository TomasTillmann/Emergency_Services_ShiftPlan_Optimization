using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulating;

public sealed class Simulation
{
  public SimulationState State { get; set; }

  private EmergencyServicePlan Plan { get; set; }

  #region Stats

  /// <summary>
  /// Total count of incidents the simulation was run on. 
  /// </summary>
  public int TotalIncidentsCount { get; private set; }

  public int UnhandledIncidentsCount { get; private set; }

  public int HandledIncidentsCount => TotalIncidentsCount - UnhandledIncidentsCount;

  /// <summary>
  /// Success rate of last run simulation.
  /// </summary>
  public double SuccessRate => (TotalIncidentsCount - UnhandledIncidentsCount) / (double)TotalIncidentsCount;

  #endregion

  public List<int> UnhandledIncidents { get; } = new();

  private readonly MedicTeamsEvaluator _medicTeamsEvaluator;
  private readonly PlannableIncident.Factory _plannableIncidentFactory;

  public Simulation(World world, Constraints constraints)
  {
    State = new SimulationState(world.Depots.Length, constraints);

    _plannableIncidentFactory = new PlannableIncident.Factory(world);
    _medicTeamsEvaluator = new MedicTeamsEvaluator(world);
  }

  /// <summary>
  /// Performs simulation on Depots.
  /// </summary>
  public void Run(EmergencyServicePlan plan, ReadOnlySpan<Incident> incidents)
  {
    Plan = plan;
    TotalIncidentsCount = incidents.Length;
    Prepare();

    PlannableIncident plannableIncidentForBestShift;
    Incident currentIncident;
    MedicTeamId bestMedicTeam;
    MedicTeam medicTeam;

    for (int i = 0; i < TotalIncidentsCount; ++i)
    {
      currentIncident = incidents[i];
      bestMedicTeam = new MedicTeamId { DepotIndex = -1, OnDepotIndex = -1 };

      // Find handling medic team. 
      int depotIndex = 0;
      int teamIndex = 0;
      for (; depotIndex < Plan.Assignments.Length; ++depotIndex)
      {
        for (; teamIndex < Plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
        {
          if (_medicTeamsEvaluator.IsHandling(new MedicTeamId(teamIndex, depotIndex), in currentIncident))
          {
            bestMedicTeam = new MedicTeamId { DepotIndex = depotIndex, OnDepotIndex = teamIndex };
            break;
          }
        }
      }

      // No handling medic team exists. 
      if (bestMedicTeam.DepotIndex == -1 && bestMedicTeam.OnDepotIndex == -1)
      {
        UnhandledIncidentsCount++;
        continue;
      }

      for (; depotIndex < Plan.Assignments.Length; ++depotIndex)
      {
        for (teamIndex = teamIndex + 1; teamIndex < Plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
        {
          if (_medicTeamsEvaluator.IsHandling(new MedicTeamId(teamIndex, depotIndex), in currentIncident))
          {
            bestMedicTeam = _medicTeamsEvaluator.GetBetter(bestMedicTeam, new MedicTeamId(depotIndex, teamIndex), in currentIncident);
          }
        }
      }

      plannableIncidentForBestShift = _plannableIncidentFactory.Get(bestMedicTeam, in currentIncident);
      State.PlanIncident(bestMedicTeam, plannableIncidentForBestShift);
    }
  }

  private void Prepare()
  {
    _plannableIncidentFactory.Plan = Plan;
    _medicTeamsEvaluator.Plan = Plan;
    Reset();
  }

  private void Reset()
  {
    ResetState();
    ResetStats();
  }

  private void ResetState()
  {
    State.Clear(Plan);
  }

  private void ResetStats()
  {
    UnhandledIncidentsCount = 0;
  }
}
