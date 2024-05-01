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
      Ambulances = depot.Ambulances.Select(amb => _ambulanceMapper.Map(amb)).ToList(),
      Index = depot.Index,
    };
  }

  public Depot MapBack(DepotModel model)
  {
    return new Depot
    {
      Location = _coordinateMapper.MapBack(model.Location),
      Ambulances = model.Ambulances.Select(amb => _ambulanceMapper.MapBack(amb)).ToList(),
      Index = model.Index,
    };
  }
}


