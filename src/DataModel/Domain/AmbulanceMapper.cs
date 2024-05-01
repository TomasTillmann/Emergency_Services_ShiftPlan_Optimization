namespace ESSP.DataModel;

public class AmbulanceMapper
{
  private readonly CoordinateMapper _coordinateMapper = new();

  public AmbulanceModel Map(Ambulance ambulance)
  {
    return new AmbulanceModel();
  }

  public Ambulance MapBack(AmbulanceModel model)
  {
    return new Ambulance();
  }
}


