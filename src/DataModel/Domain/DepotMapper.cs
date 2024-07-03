using System.Linq;

namespace ESSP.DataModel;

public class DepotMapper
{
  private readonly AmbulanceMapper _ambulanceMapper = new();
  private readonly CoordinateMapper _coordinateMapper = new();

  public DepotModel Map(Depot depot)
  {
    return new DepotModel
    {
      Location = _coordinateMapper.Map(depot.Location),
    };
  }

  public Depot MapBack(DepotModel model)
  {
    return new Depot
    {
      Location = _coordinateMapper.MapBack(model.Location),
    };
  }
}


