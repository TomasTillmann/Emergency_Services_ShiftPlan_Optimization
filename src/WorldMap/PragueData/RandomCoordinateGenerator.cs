using System;
using NetTopologySuite.Geometries;

namespace ESSP.DataModel;

public class RandomCoordinateGenerator(Random random = null)
{
    private readonly Random _random = random ?? new Random();

    public CoordinateModel GenerateRandomCoordinateIn(Polygon polygon)
    {
        Envelope envelope = polygon.EnvelopeInternal;

        NetTopologySuite.Geometries.Coordinate randomCoordinate;
        do
        {
            double randomX = envelope.MinX + (_random.NextDouble() * (envelope.MaxX - envelope.MinX));
            double randomY = envelope.MinY + (_random.NextDouble() * (envelope.MaxY - envelope.MinY));
            randomCoordinate = new NetTopologySuite.Geometries.Coordinate(randomX, randomY);
        }
        while (!polygon.Contains(new Point(randomCoordinate)));

        return new CoordinateModel(randomCoordinate.X, randomCoordinate.Y);
    }
}