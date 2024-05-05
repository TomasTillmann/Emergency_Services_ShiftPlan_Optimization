using System.Collections.Generic;

namespace ESSP.DataModel;

public record AmbulanceTypeModel
{
  public HashSet<string> AllowedIncidentTypes { get; set; }
  public int Cost { get; set; }
}
