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

  public void WriteGraph(ShiftPlanOpt shiftPlan, Seconds end)
  {
    int index = 1;
    foreach (var shift in shiftPlan.Shifts)
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
          _writer.Write(shift.Work.IsInInterval(time.Value) ? "-" : " ");
        }
      }

      _writer.WriteLine();
      _writer.Write($"{shift.Id}: ");

      for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
      {
        _writer.Write(shift.PlannedIncident(time.Value) is not null ? "=" : " ");
      }

      _writer.WriteLine();
    }

    _writer.Flush();
  }
}
