using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulating;

/// <summary>
/// Discrete event simulation of emergency service plan on given incidents set.
/// </summary>
public class Simulation
{
  private ISimulationState _state;
  
  /// <summary>
  /// Simulation state. You can freely change the state.
  /// </summary>
  public ISimulationState State
  {
    get => _state;
    set
    {
      _state = value;
      _medicTeamsEvaluator.State = value;
      _plannableIncidentFactory.State = value;
    }
  }

  private EmergencyServicePlan Plan { get; set; }

  #region Stats

  /// <summary>
  /// Total count of incidents the simulation was run on. 
  /// </summary>
  public int TotalIncidentsCount { get; private set; }

  /// <summary>
  /// Total count of unhandled incidents.
  /// </summary>
  public int UnhandledIncidentsCount { get; private set; }

  /// <summary>
  /// Total count of handled incidents.
  /// </summary>
  public int HandledIncidentsCount => TotalIncidentsCount - UnhandledIncidentsCount;

  #endregion

  public List<int> UnhandledIncidents { get; } = new();

  private readonly MedicTeamsEvaluator _medicTeamsEvaluator;
  private readonly PlannableIncident.Factory _plannableIncidentFactory;

  public Simulation(World world, Constraints constraints, IDistanceCalculator distanceCalculator)
  {
    _plannableIncidentFactory = new PlannableIncident.Factory(world, distanceCalculator);
    _medicTeamsEvaluator = new MedicTeamsEvaluator(world, distanceCalculator);
    State = new SimulationState(world.Depots.Length, constraints);
  }

  public int Run(EmergencyServicePlan plan, ReadOnlySpan<Incident> incidents, bool resetState = true)
  {
    Plan = plan;
    TotalIncidentsCount = incidents.Length;
    Prepare(resetState);

    PlannableIncident plannableIncidentForBestShift;
    Incident currentIncident;
    MedicTeamId bestMedicTeam;

    for (int i = 0; i < TotalIncidentsCount; ++i)
    {
      currentIncident = incidents[i];
      bestMedicTeam = new MedicTeamId { DepotIndex = -1, OnDepotIndex = -1 };

      // Find handling medic team. 
      int depotIndex = -1;
      int teamIndex = -1;
      for (depotIndex = 0; depotIndex < Plan.Assignments.Length; ++depotIndex)
      {
        for (teamIndex = 0; teamIndex < Plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
        {
          if (_medicTeamsEvaluator.IsHandling(new MedicTeamId(depotIndex, teamIndex), in currentIncident))
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
        UnhandledIncidents.Add(i);
        continue;
      }

      if (depotIndex != Plan.Assignments.Length)
      {
        for (teamIndex += 1; teamIndex < Plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
        {
          if (_medicTeamsEvaluator.IsHandling(new MedicTeamId(depotIndex, teamIndex), in currentIncident))
          {
            bestMedicTeam = _medicTeamsEvaluator.GetBetter(bestMedicTeam, new MedicTeamId(depotIndex, teamIndex), in currentIncident);
          }
        }
      }

      for (depotIndex += 1; depotIndex < Plan.Assignments.Length; ++depotIndex)
      {
        for (teamIndex = 0; teamIndex < Plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
        {
          if (_medicTeamsEvaluator.IsHandling(new MedicTeamId(depotIndex, teamIndex), in currentIncident))
          {
            bestMedicTeam = _medicTeamsEvaluator.GetBetter(bestMedicTeam, new MedicTeamId(depotIndex, teamIndex), in currentIncident);
          }
        }
      }

      plannableIncidentForBestShift = _plannableIncidentFactory.Get(bestMedicTeam, in currentIncident);
      State.PlanIncident(bestMedicTeam, plannableIncidentForBestShift);
    }

    return HandledIncidentsCount;
  }

  private void Prepare(bool resetState)
  {
    _plannableIncidentFactory.Plan = Plan;
    _medicTeamsEvaluator.Plan = Plan;
    Reset(resetState);
  }

  private void Reset(bool resetState)
  {
    if (resetState)
    {
      ResetState();
    }
    ResetStats();
  }

  private void ResetState()
  {
    State.Clear();
  }

  private void ResetStats()
  {
    UnhandledIncidents.Clear();
    UnhandledIncidentsCount = 0;
  }
}
