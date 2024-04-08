using System.Collections.Immutable;
using ESSP.DataModel;

public record Input
{
  public World World { get; set; }
  public Constraints Constraints { get; set; }
  public ImmutableArray<SuccessRatedIncidents> SuccessRatedIncidents { get; set; }
}

