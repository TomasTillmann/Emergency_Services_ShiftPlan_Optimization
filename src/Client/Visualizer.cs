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

  public void WriteGraph(ImmutableArray<MedicTeam> medicTeams, ImmutableArray<Incident> incidents, TextWriter writer = null)
  {
    writer ??= _writer;

    int index = 0;
    foreach (var medicTeam in medicTeams)
    {
      writer.Write($"{index++}: ");
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
        char c = current == last ? '=' : 'I';
        if (current is null) c = ' ';
        writer.Write(c);
        last = current;
      }

      writer.WriteLine();
    }

    writer.Flush();
  }

  public void WriteWeights(Weights weights)
  {
    int index = 1;
    foreach (var weight in weights.Shifts)
    {
      _writer.Write($"{index++}: ");

      for (Seconds time = 0.ToSeconds(); time < 24.ToHours().ToMinutes().ToSeconds(); time += (5 * 60).ToSeconds())
      {
        if (time.Value % 1.ToHours().ToSeconds().Value == 0)
        {
          _writer.Write($"{time.Value / (60 * 60)}");
        }
        else
        {
          _writer.Write(weight.IsInInterval(time.Value) ? "-" : " ");
        }
      }

      _writer.WriteLine();
    }

    _writer.Flush();
  }

  public void PlotGraph(IOptimizer optimizer, Weights weights, ImmutableArray<Incident> incidents, TextWriter writer = null)
  {
    writer ??= _writer;

    optimizer.Loss.Map(weights);
    optimizer.Loss.Simulation.Run(incidents);

    WriteGraph(optimizer.Loss.Simulation.EmergencyServicePlan.Teams, incidents, writer);
    writer.WriteLine();
    writer.WriteLine("Unhandled:");
    writer.WriteLine(string.Join("\n", optimizer.Loss.Simulation.UnhandledIncidents));
    writer.WriteLine($"Success rate: {optimizer.Loss.Simulation.SuccessRate}");

    writer.Flush();
  }
}
