using ESSP.DataModel;
using Optimizing;

namespace Client;

public class Report : IDisposable
{
  private readonly TextWriter _writer;
  public Report(TextWriter writer)
  {
    _writer = writer;
  }

  public void Run(List<IOptimizer> optimizers, List<SuccessRatedIncidents> successRatedIncidents)
  {
    foreach (IOptimizer optimizer in optimizers)
    {
      foreach (SuccessRatedIncidents successRatedIncident in successRatedIncidents)
      {
        IEnumerable<ShiftPlan> optimalShiftPlan = optimizer.FindOptimal(successRatedIncidents);
      }
    }
  }

  public void Dispose()
  {
    _writer.Flush();
    _writer.Dispose();
  }
}
