namespace ESSP.DataModel;

public class CoordinateMapper
{
  public CoordinateModel Map(Coordinate coordinate)
  {
    return new CoordinateModel
    {
      Latitude = coordinate.Latitude,
      Longitude = coordinate.Longitude
    };
  }

  public Coordinate MapBack(CoordinateModel model)
  {
    return new Coordinate
    {
      Latitude = model.Latitude,
      Longitude = model.Longitude
    };
  }
}

