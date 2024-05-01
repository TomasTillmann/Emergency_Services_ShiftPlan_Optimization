using System.Collections.Generic;

namespace ESSP.DataModel;

public record DepotModel
{
  public int Index { get; set; }
  public CoordinateModel Location { get; set; }
  public List<AmbulanceModel> Ambulances { get; set; }
}


