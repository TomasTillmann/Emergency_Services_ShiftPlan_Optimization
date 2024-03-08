using System.Collections.Generic;
using Model.Extensions;

namespace ESSP.DataModel;

public sealed class Statistics
{
  public IList<Incident> UnhandledIncidents { get; private set; } = new List<Incident>();
  public IReadOnlyCollection<Incident> AllIncidents { get; private set; }
  public double SuccessRate => 1 - (double)UnhandledIncidents.Count / AllIncidents.Count;

  public Statistics(IReadOnlyCollection<Incident> allIncidents)
  {
    AllIncidents = allIncidents;
  }

  public Statistics()
  {
    AllIncidents = new List<Incident>();
  }

  public void SetUnhandled(Incident incident)
  {
    UnhandledIncidents.Add(incident);
  }

  public void Reset(IReadOnlyCollection<Incident> allIncidents)
  {
    UnhandledIncidents.Clear();
    AllIncidents = allIncidents;
  }

  public override string ToString()
  {
    return $"SuccessRate: {SuccessRate}\n" +
           $"AllIncidents: Count: {AllIncidents.Count}, {AllIncidents.Visualize("| ")}\n" +
           $"UnhandledIncidents: Count: {UnhandledIncidents.Count}, {UnhandledIncidents.Visualize("| ")}\n";
  }
}
