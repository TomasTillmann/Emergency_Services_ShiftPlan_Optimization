namespace ESSP.DataModel;

public class AmbulanceMapper
{
  private readonly CoordinateMapper _coordinateMapper = new();
  private readonly AmbulanceTypeMapper _ambTypeMapper = new();

  public AmbulanceModel Map(Ambulance ambulance)
  {
    return new AmbulanceModel
    {
      Type = _ambTypeMapper.Map(ambulance.Type)
    };
  }

  public Ambulance MapBack(AmbulanceModel model)
  {
    return new Ambulance
    {
      Type = _ambTypeMapper.MapBack(model.Type)
    };
  }
}


