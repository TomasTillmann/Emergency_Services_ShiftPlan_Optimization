using System.Collections.Generic;

namespace ESSP.DataModel;

public class AmbulanceType
{
  public HashSet<string> AllowedIncidentTypes { get; init; }

  public int Cost { get; init; }

  public override string ToString()
  {
    return $"{Cost}";
  }
}
