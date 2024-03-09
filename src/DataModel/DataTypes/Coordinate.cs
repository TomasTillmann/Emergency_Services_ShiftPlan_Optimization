using System;

namespace ESSP.DataModel
{
  public readonly struct Coordinate
  {
    public Meters X { get; init; }

    public Meters Y { get; init; }

    public Coordinate(Meters x, Meters y)
    {
      X = x;
      Y = y;
    }

    public static Coordinate FromMeters(int x, int y)
    {
      return new Coordinate(x.ToMeters(), y.ToMeters());
    }

    #region Operators

    public static Coordinate operator -(Coordinate c1, Coordinate c2) => new Coordinate(c1.X - c2.X, c1.Y - c2.Y);
    public static Coordinate operator +(Coordinate c1, Coordinate c2) => new Coordinate(c1.X + c2.X, c1.Y + c2.Y);
    public static bool operator ==(Coordinate c1, Coordinate c2) => c1.X == c2.X && c1.Y == c2.Y;
    public static bool operator !=(Coordinate c1, Coordinate c2) => c1.X != c2.X && c1.Y != c2.Y;

    #endregion

    public static Coordinate GetRandom(Random random, Coordinate areaPosition, Meters xSize, Meters ySize)
    {
      return new Coordinate
      (
          random.Next(areaPosition.X.Value, xSize.Value).ToMeters(),
          random.Next(areaPosition.Y.Value, ySize.Value).ToMeters()
      );
    }

    public override bool Equals(object obj)
    {
      if (obj is Coordinate coord)
      {
        return this == coord;
      }

      return false;
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(X.GetHashCode(), Y.GetHashCode());
    }

    public override string ToString()
    {
      return $"X: {X}, Y: {Y}";
    }

  }
}
