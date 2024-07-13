using System;
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
using System.Net.Http;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using ESSP.DataModel;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.Tiling.Utilities;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Coordinate = ESSP.DataModel.Coordinate;
using IFeature = Mapsui.IFeature;

namespace WorldMap;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var map = new Map();
        map.Layers.Add(OpenStreetMapPositron.CreateTileLayer());
        
        PragueInput input = new PragueInput();
        var world = input.GetWorld();
        var incidents = input.GetMondayIncidents(300, new Random(420));
        
        SymbolStyle.DefaultHeight = 1;
        SymbolStyle.DefaultWidth = 1;
        map.Layers.Add(GetMapLayer(
            world.Depots.Select(depot => depot.Location),
            "Depots",
            new Mapsui.Styles.SymbolStyle
            {
                //Fill = new Mapsui.Styles.Brush { Color = Mapsui.Styles.Color.Grey },
                Outline = new Pen { Color = Color.Black, Width = 15},
            }
            )
        );
        
        map.Layers.Add(GetMapLayer(
            world.Hospitals.Select(hospital => hospital.Location),
            "Hospitals",
            new Mapsui.Styles.SymbolStyle()
            {
                //Fill = new Mapsui.Styles.Brush { Color = Mapsui.Styles.Color.Green },
                Outline = new Pen { Color = Color.LimeGreen, Width = 15},
            }
            )
        );

        var busyIncidents = incidents.Where(inc => inc.OccurenceSec >= 9.ToHours().ToMinutes().ToSeconds().Value &&
                                                   inc.OccurenceSec <= 12.ToHours().ToMinutes().ToSeconds().Value).ToHashSet();
        
        map.Layers.Add(GetMapLayer(
            busyIncidents.Select(inc => inc.Location), 
            "Incidents",
            new Mapsui.Styles.SymbolStyle
            {
                //Fill = new Mapsui.Styles.Brush { Color = Mapsui.Styles.Color.Blue },
                //Outline = new Pen { Color = Color.Red, Width = 20},
                Outline = new Pen { Color = Color.DarkRed, Width = 5},
                //SymbolScale = 0.5
            }
            )
        );
        
        map.Layers.Add(GetMapLayer(
            incidents.Where(inc => !busyIncidents.Contains(inc)).Select(inc => inc.Location), 
            "Incidents",
            new Mapsui.Styles.SymbolStyle
            {
                //Fill = new Mapsui.Styles.Brush { Color = Mapsui.Styles.Color.Blue },
                //Outline = new Pen { Color = Color.Red, Width = 20},
                Outline = new Pen { Color = Color.Red, Width = 5},
                //SymbolScale = 0.5
            }
            )
        );

        Polygon praguePolygon = input.GetPraguePolygon();
        var pragueFeature = new Mapsui.Nts.GeometryFeature(praguePolygon);
        pragueFeature.Styles.Add(
            new Mapsui.Styles.VectorStyle
            {
                Fill = new Brush(Color.FromArgb(128, 255, 0, 0)), // Red color with 50% transparency
                Outline = new Pen(Color.FromArgb(255, 255, 0, 0), 2) // Red outline
            }
        );
        map.Layers.Add(new MemoryLayer
        {
            Features = new List<IFeature> { pragueFeature }
        });

        // Assign the map to the MapControl
        mapView.Map = map;
    }

    private MemoryLayer GetMapLayer(IEnumerable<Coordinate> locations, string layerName, Mapsui.Styles.Style featureStyle = null)
    {
        var features = new List<IFeature>();

        foreach (var location in locations)
        {
            var pointFeature = GetLocationOnMap(location);
            if (featureStyle is not null) pointFeature.Styles.Add(featureStyle);
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

public static class OpenStreetMapPositron
{
    public static IPersistentCache<byte[]>? DefaultCache;
    private static readonly Attribution _cartoDbAttribution = new Attribution("© OpenStreetMap contributors, © CartoDB", "https://www.openstreetmap.org/copyright");

    public static TileLayer CreateTileLayer(string? userAgent = null)
    {
        if (userAgent == null)
            userAgent = HttpClientTools.GetDefaultApplicationUserAgent();
        TileLayer tileLayer = new TileLayer((ITileSource) CreateTileSource(userAgent));
        tileLayer.Name = nameof (OpenStreetMapPositron);
        return tileLayer;
    }

    private static HttpTileSource CreateTileSource(string userAgent)
    {
        GlobalSphericalMercator sphericalMercator = new GlobalSphericalMercator();
        Attribution? nullable = new Attribution?(_cartoDbAttribution);
        IPersistentCache<byte[]> defaultCache = DefaultCache;
        Attribution? attribution = nullable;
        Action<HttpRequestMessage> configureHttpRequestMessage = (Action<HttpRequestMessage>) (r => r.Headers.TryAddWithoutValidation("User-Agent", userAgent));
        return new HttpTileSource(
            (ITileSchema) sphericalMercator,
            "https://a.basemaps.cartocdn.com/rastertiles/light_all/{z}/{x}/{y}.png",
            name: nameof (OpenStreetMap),
            persistentCache: defaultCache,
            attribution: attribution,
            configureHttpRequestMessage: configureHttpRequestMessage);
    }
}