using System.Collections.Immutable;
using ESSP.DataModel;

public class SuccessRatedIncidents
{
  public ImmutableArray<Incident> Value { get; init; }
  public double SuccessRate { get; init; }
}
