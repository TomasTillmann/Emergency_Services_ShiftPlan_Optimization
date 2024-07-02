using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ESSP.DataModel;

public class Depot
{
  public int Index { get; init; }
  public Coordinate Location { get; init; }
  public List<Ambulance> Ambulances { get; init; } = new();
  public List<MedicTeam> MedicTeams { get; init; } = new();

  public override string ToString()
  {
    return $"{Index}: {Location}";
  }
}

