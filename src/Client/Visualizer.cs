using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

namespace Client;

public class Visualizer : IDisposable
{
  private readonly TextWriter _writer;

  public Visualizer(TextWriter writer)
  {
    _writer = writer;
  }

  public void Dispose()
  {
    _writer.Flush();
    _writer.Close();
    _writer.Dispose();
  }

  public void WriteGraph(EmergencyServicePlan plan, ImmutableArray<Incident> incidents, TextWriter writer = null)
  {
    writer ??= _writer;

    int index = 0;
    for (int i = 0; i < plan.AllocatedMedicTeamsCount; ++i)
    {
      MedicTeam medicTeam = plan.AvailableMedicTeams[i];

      writer.Write($"{medicTeam.Depot.Index}: ");
      Seconds end = 24.ToHours().ToMinutes().ToSeconds();

      for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
      {
        if (time.Value % 1.ToHours().ToSeconds().Value == 0)
        {
          writer.Write($"{time.Value / (60 * 60)}");
        }
        else
        {
          writer.Write(medicTeam.Shift.IsInInterval(time.Value) ? "-" : " ");
        }
      }

      writer.WriteLine();
      PlannableIncident last = medicTeam.PlannedIncident(0);
      for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
      {
        var current = medicTeam.PlannedIncident(time.Value);
        char c;

        if (current is null)
        {
          c = ' ';
        }
        else if (current.ToIncidentDrive.IsInInterval(time.Value))
        {
          c = '>';
        }
        else if (current == last)
        {
          c = '=';
        }
        else
        {
          c = 'I';
        }

        writer.Write(c);
        last = current;
      }

      writer.WriteLine();
    }

    writer.Flush();
  }

  public void PlotGraph(ILoss loss, Weights weights, ImmutableArray<Incident> incidents, TextWriter writer = null)
  {
    writer ??= _writer;

    weights.MapTo(loss.Simulation.EmergencyServicePlan);
    loss.Simulation.Run(incidents.AsSpan());

    WriteGraph(loss.Simulation.EmergencyServicePlan, incidents, writer);
    writer.WriteLine();
    writer.WriteLine("Unhandled:");
    writer.WriteLine(string.Join("\n", loss.Simulation.UnhandledIncidents));
    writer.WriteLine($"Success rate: {loss.GetSuccessRate()}");
    writer.WriteLine($"cost: {loss.GetCost()}");
    writer.WriteLine($"effectivity: {loss.GetEffectivity()}");
    writer.WriteLine($"loss: {loss.GetLoss()}");

    writer.Flush();
  }
}
