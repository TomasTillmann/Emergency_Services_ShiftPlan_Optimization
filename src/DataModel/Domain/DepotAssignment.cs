using System.Collections.Generic;

namespace ESSP.DataModel;

public readonly struct DepotAssignment
{
  public List<MedicTeam> MedicTeams { get; init; } = new();
  public List<Ambulance> Ambulances { get; init; } = new();

  public DepotAssignment() { }
}

