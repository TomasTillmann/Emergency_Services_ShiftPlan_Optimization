using DataModel.Interfaces;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

/// <summary>
/// Helpful for visualization of emergency service plan's behaviour on some set of incidents.
/// https://en.wikipedia.org/wiki/Gantt_chart
/// </summary>
public class GanttView
{
  private readonly Simulation _simulation;
  private readonly SimulationStateVerbose _state;

  public GanttView(World world, Constraints constraints, IDistanceCalculator distanceCalculator)
  {
    _simulation = new Simulation(world, constraints, distanceCalculator);
    _state = new SimulationStateVerbose(world.Depots.Length, constraints);
    _simulation.State = _state;
  }

  /// <summary>
  /// Show gantt view (chart) of <see cref="plan"/> behaviour on given set of <see cref="incidents"/>.
  /// </summary>
  /// <param name="writer">Defaults to <see cref="Console.Out"/>. Gannt chart is written to this writer.</param>
  public void Show(EmergencyServicePlan plan, ReadOnlySpan<Incident> incidents, TextWriter writer = null)
  {
    writer = writer ?? Console.Out;
    _simulation.Run(plan, incidents);

    for (int depotIndex = 0; depotIndex < plan.Assignments.Length; ++depotIndex)
    {
      for (int onDepotIndex = 0; onDepotIndex < plan.Assignments[depotIndex].MedicTeams.Count; ++onDepotIndex)
      {
        writer.Write($":{depotIndex}:");
        Seconds end = 24.ToHours().ToMinutes().ToSeconds();
        for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
        {
          if (time.Value % 1.ToHours().ToSeconds().Value == 0)
          {
            writer.Write($"{time.Value / (60 * 60)}");
          }
          else
          {
            writer.Write(plan.Team(new MedicTeamId(depotIndex, onDepotIndex)).Shift.IsInInterval(time.Value) ? "-" : " ");
          }
        }
        writer.WriteLine();

        List<PlannableIncident> plannedIncidents =
            _state.GetAllPlannedIncidents(new MedicTeamId(depotIndex, onDepotIndex));
        for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
        {
          char c = ' ';
          foreach (PlannableIncident current in plannedIncidents)
          {
            if (current.ToIncidentDrive.IsInInterval(time.Value))
            {
              c = '>';
            }
            else if (current.OnSceneDuration.IsInInterval((time.Value)))
            {
              c = 'o';
            }
            else if (current.ToDepotDrive.IsInInterval(time.Value))
            {
              c = '<';
            }
            else if (current.WholeInterval.IsInInterval(time.Value))
            {
              c = '=';
            }
          }
          writer.Write(c);
        }
        writer.WriteLine();
      }
    }

    writer.WriteLine($"handled count: {_simulation.HandledIncidentsCount} / {_simulation.TotalIncidentsCount}, cost: {plan.Cost}");
    writer.WriteLine($"Unhandled: " + string.Join(", ", _simulation.UnhandledIncidents));
    writer.Flush();
  }
}
