using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace ESSP.DataModel;

public class PolygonParser
{
    public static NetTopologySuite.Geometries.Polygon ParsePolygon(string input)
    {
        var coordinates = new List<NetTopologySuite.Geometries.Coordinate>();
        var lines = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                if (double.TryParse(parts[0].Trim(), out double longitude) && double.TryParse(parts[1].Trim(), out double latitude))
                {
                    coordinates.Add(new NetTopologySuite.Geometries.Coordinate(longitude, latitude));
                }
            }
        }

        var geometryFactory = new GeometryFactory();
        var linearRing = geometryFactory.CreateLinearRing(coordinates.ToArray());
        var polygon = geometryFactory.CreatePolygon(linearRing);
        return polygon;
    }
}