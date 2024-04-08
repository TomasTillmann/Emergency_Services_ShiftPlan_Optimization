using System.Collections.Immutable;
using ESSP.DataModel;
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

  public void WriteGraph(ShiftPlan shiftPlan, ImmutableArray<Incident> incidents, Seconds end, TextWriter writer = null)
  {
    writer ??= _writer;

    int index = 0;
    foreach (var shift in shiftPlan.Shifts)
    {
      writer.Write($"{index++}: ");

      for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
      {
        if (time.Value % 1.ToHours().ToSeconds().Value == 0)
        {
          writer.Write($"{time.Value / (60 * 60)}");
        }
        else
        {
          writer.Write(shift.Work.IsInInterval(time.Value) ? "-" : " ");
        }
      }

      writer.WriteLine();
      PlannableIncident last = shift.PlannedIncident(0);
      for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
      {
        var current = shift.PlannedIncident(time.Value);
        char c = current == last ? '=' : 'I';
        if (current is null) c = ' ';
        writer.Write(c);
        last = current;
      }

      writer.WriteLine();
    }

    writer.Flush();
  }

  public void WriteWeights(Weights weights, Seconds end)
  {
    int index = 1;
    foreach (var weight in weights.Value)
    {
      _writer.Write($"{index++}: ");

      for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
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

  public void PlotGraph(Weights weights, World world, ImmutableArray<Incident> incidents, TextWriter writer = null)
  {
    writer ??= _writer;

    ShiftPlan shiftPlan = ShiftPlan.GetFrom(world.Depots, weights);

    Simulation simulation = new(world);
    simulation.Run(incidents, shiftPlan);

    WriteGraph(shiftPlan, incidents, 24.ToHours().ToSeconds(), writer);
    writer.WriteLine();
    writer.WriteLine("Unhandled:");
    writer.WriteLine(string.Join("\n", simulation.UnhandledIncidents));
    writer.WriteLine($"Success rate: {simulation.SuccessRate}");

    writer.Flush();
  }
}
