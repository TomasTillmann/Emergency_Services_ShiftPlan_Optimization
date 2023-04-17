using System;

namespace ESSP.DataModel
{
    public struct Coordinate
    {
        public Meters X { get; }

        public Meters Y { get; }

        public Coordinate(Meters x, Meters y)
        {
            X = x;
            Y = y;
        }

        #region Operators

        public static Coordinate operator -(Coordinate x, Coordinate y) => new Coordinate(x.X - y.X, x.Y - y.Y);
        public static Coordinate operator +(Coordinate x, Coordinate y) => new Coordinate(x.X + y.X, x.Y + y.Y);

        #endregion

        public static Coordinate GetRandom(Random random, Coordinate areaPosition, Meters xSize, Meters ySize)
        {
            return new Coordinate
            (
                random.Next(areaPosition.X.Value, xSize.Value).ToMeters(),
                random.Next(areaPosition.Y.Value, ySize.Value).ToMeters()
            );
        }

        public override string ToString()
        {
            return $"COORDINATE: {{ Lat: {X}, Long: {Y} }}";
        }
    }
}