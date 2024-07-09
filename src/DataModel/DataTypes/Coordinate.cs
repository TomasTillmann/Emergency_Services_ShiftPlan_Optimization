using System;

namespace ESSP.DataModel
{
  public readonly struct Coordinate
  {
    ///<summary>about 10 meters</summary> 
    public static double Epsilon = 0.0001;
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public Coordinate(double latitude, double longitude)
    {
      Latitude = latitude;
      Longitude = longitude;
    }
    
    public Coordinate() { }
    
    public static bool operator ==(Coordinate x, Coordinate y) => Math.Abs(x.Latitude - y.Latitude) <= Epsilon && Math.Abs(x.Longitude - y.Longitude) <= Epsilon;
    public static bool operator !=(Coordinate x, Coordinate y) => !(x == y);

    public override string ToString()
    {
      return $"({Latitude}, {Longitude})";
    }
  }
}

