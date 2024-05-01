namespace ESSP.DataModel;

public class CoordinateMapper
{
  public CoordinateModel Map(Coordinate coordinate)
  {
    return new CoordinateModel
    {
      XMet = coordinate.XMet,
      YMet = coordinate.YMet
    };
  }

  public Coordinate MapBack(CoordinateModel model)
  {
    return new Coordinate
    {
      XMet = model.XMet,
      YMet = model.YMet
    };
  }
}

