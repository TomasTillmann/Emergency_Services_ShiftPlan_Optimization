using DataModel.Interfaces;

namespace ESSP.DataModel;

public class Hospital : ILocatable
{
  public Coordinate Location { get; set; }

  public Hospital(Coordinate coordinate)
  {
    Location = coordinate;
  }
}
