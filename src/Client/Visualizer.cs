using ESSP.DataModel;

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

  public void WriteGraph(ShiftPlan shiftPlan, Seconds end, TextWriter writer = null)
  {
    writer ??= _writer;

    int index = 1;
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
      writer.Write($"{shift.Id}: ");

      for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
      {
        writer.Write(shift.PlannedIncident(time.Value) is not null ? "=" : " ");
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
}
