using System;
using MathNet.Numerics.Distributions;
using NetTopologySuite.Geometries;

namespace ESSP.DataModel;

public class RandomCoordinateGenerator(Normal distributionLatitude, Normal distributionLongitude)
{
    private readonly Normal _distributionLatitude = distributionLatitude; 
    private readonly Normal _distributionLongitude = distributionLongitude; 

    public CoordinateModel GenerateRandomCoordinateIn(Polygon polygon)
    {
        NetTopologySuite.Geometries.Coordinate randomCoordinate;
        do
        {
            double randomX = _distributionLatitude.Sample();
            double randomY = _distributionLongitude.Sample();
            randomCoordinate = new NetTopologySuite.Geometries.Coordinate(randomX, randomY);
        }
        while (!polygon.Contains(new Point(randomCoordinate)));

        return new CoordinateModel(randomCoordinate.X, randomCoordinate.Y);
    }
}