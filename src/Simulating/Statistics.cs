using System.Collections.Generic;
using ESSP.DataModel;
using Model.Extensions;

namespace Simulating;

public sealed class Statistics
{
    public IList<Incident> UnhandledIncidents { get; private set; } = new List<Incident>();
    public IReadOnlyCollection<Incident> AllIncidents { get; private set; }
    public double SuccessRate => 1 - (double)UnhandledIncidents.Count / AllIncidents.Count;

    internal Statistics(IReadOnlyCollection<Incident> allIncidents)
    {
        AllIncidents = allIncidents;
    }

    internal Statistics()
    {
        AllIncidents = new List<Incident>();
    }

    internal void SetUnhandled(Incident incident)
    {
        UnhandledIncidents.Add(incident);
    }

    internal void Reset(IReadOnlyCollection<Incident> allIncidents)
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