using Avalonia.Controls;
using Mapsui;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.UI.Avalonia;
using Mapsui.Utilities;
using Mapsui.Nts;
using System.Collections.Generic;
using System.Linq;
using ESSP.DataModel;
using Mapsui.Layers;
using Mapsui.Tiling;

namespace WorldMap;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Input1 input = new Input1();
        var world = input.GetWorld();
        var incidents = input.GetIncidents(330);

        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        
        map.Layers.Add(GetMapLayer(
            world.Depots.Select(depot => depot.Location),
            "Depots",
            new Mapsui.Styles.SymbolStyle
            {
                Fill = new Mapsui.Styles.Brush { Color = Mapsui.Styles.Color.Grey },
                SymbolScale = 0.5,
                Outline = new Mapsui.Styles.Pen { Color = Mapsui.Styles.Color.Black, Width = 1 }
            }
            )
        );
        
        map.Layers.Add(GetMapLayer(
            world.Hospitals.Select(hospital => hospital.Location),
            "Hospitals",
            new Mapsui.Styles.SymbolStyle
            {
                Fill = new Mapsui.Styles.Brush { Color = Mapsui.Styles.Color.Green },
                SymbolScale = 0.5,
                Outline = new Mapsui.Styles.Pen { Color = Mapsui.Styles.Color.Black, Width = 1 }
            }
            )
        );
        
        map.Layers.Add(GetMapLayer(
            incidents.Select(inc => inc.Location), 
            "Incidents",
            new Mapsui.Styles.SymbolStyle
            {
                Fill = new Mapsui.Styles.Brush { Color = Mapsui.Styles.Color.Blue },
                SymbolScale = 0.5,
                Outline = new Mapsui.Styles.Pen { Color = Mapsui.Styles.Color.Black, Width = 1 }
            }
            )
        );

        // Set the map's center and zoom level
        //map.Navigator.CenterOn(point, 14);

        // Assign the map to the MapControl
        mapView.Map = map;
    }

    private MemoryLayer GetMapLayer(IEnumerable<Coordinate> locations, string layerName, Mapsui.Styles.Style featureStyle)
    {
        var features = new List<IFeature>();

        foreach (var location in locations)
        {
            var pointFeature = GetLocationOnMap(location);
            pointFeature.Styles.Add(featureStyle);
            features.Add(pointFeature);
        }
        
        return new MemoryLayer
        {
            Features = features,
            Name = layerName
        };
    }

    private PointFeature GetLocationOnMap(Coordinate location)
    {
        var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
        var point = new MPoint(sphericalMercatorCoordinate.x, sphericalMercatorCoordinate.y);
        return new PointFeature(point);
    }
}