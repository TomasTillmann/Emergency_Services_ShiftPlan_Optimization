using System.Collections.Generic;

namespace ESSP.DataModel;

public readonly struct DepotAssignment
{
  public readonly List<MedicTeam> MedicTeams = new();
  public readonly List<Ambulance> Ambulances = new();

  public DepotAssignment() { }
}

