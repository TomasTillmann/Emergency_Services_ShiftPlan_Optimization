using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DistanceAPI;
using ESSP.DataModel;
using NetTopologySuite.Geometries;

public class PragueInput : IInputParametrization
{
  private readonly Random _random;
  private readonly DataModelGenerator _dataGenerator = new();
  private readonly int _depotsCount = 20;

  public PragueInput(Random? random = null)
  {
    _random = random ?? new Random();
  }

  public Constraints GetConstraints()
  {
    return new Constraints
    {
      MaxTeamsPerDepotCount = Enumerable.Repeat(30, _depotsCount).ToImmutableArray(),
      MaxAmbulancesPerDepotCount = Enumerable.Repeat(30, _depotsCount).ToImmutableArray(),
    };
  }

  public World GetWorld()
  {
    // World init
    var worldModel = new WorldModel
    {
      Depots = new List<DepotModel>
      {
        new DepotModel { Location = new CoordinateModel(50.1069319, 14.5721689) }, // cerny most - praha 14
        new DepotModel { Location = new CoordinateModel(50.0313689, 14.5979964) }, // nove namesti - praha 22
        new DepotModel { Location = new CoordinateModel(50.1288628, 14.4901978) }, // lovosicka - praha 9
        new DepotModel { Location = new CoordinateModel(50.1140217, 14.4831528) }, // kundratka - praha 8
        new DepotModel { Location = new CoordinateModel(50.0655589, 14.5004358) }, // prubezna - praha 10
        new DepotModel { Location = new CoordinateModel(50.0344381, 14.5145294) }, // markusova - praha 11
        new DepotModel { Location = new CoordinateModel(50.0838322, 14.4585972) }, // chelcickeho - praha 3
        new DepotModel { Location = new CoordinateModel(50.0706575, 14.4645578) }, // 28 pluku - praha 10
        new DepotModel { Location = new CoordinateModel(50.0411625, 14.4488722) }, // na krcske starni - praha 4
        new DepotModel { Location = new CoordinateModel(50.0964272, 14.4312383) }, // Dukelskych hrdinu - praha 7
        new DepotModel { Location = new CoordinateModel(50.0692436, 14.4203850) }, // Na slupi - praha 2
        new DepotModel { Location = new CoordinateModel(50.0349783, 14.4105578) }, // Nad malym mytem - praha 4
        new DepotModel { Location = new CoordinateModel(50.0894797, 14.3986064) }, // hrad - praha 1
        new DepotModel { Location = new CoordinateModel(50.0870406, 14.3962389) }, // vlasska - praha 1
        new DepotModel { Location = new CoordinateModel(50.0639811, 14.4088542) }, // nadrazni 1573
        new DepotModel { Location = new CoordinateModel(50.0593000, 14.3730000) }, // Jinonicka - praha 5
        new DepotModel { Location = new CoordinateModel(49.9923106, 14.3450892) }, // V sudehc - praha 16
        new DepotModel { Location = new CoordinateModel(50.0448289, 14.3123114) }, // Vackova - praha 13
        new DepotModel { Location = new CoordinateModel(50.1009361, 14.2919736) }, // Za teplnarnou - praha 6
        new DepotModel { Location = new CoordinateModel(50.1083953, 14.2621233) }  // letiste vaclava havla - praha 6
      },
      Hospitals = new List<HospitalModel>()
      {
        new HospitalModel { Location = new CoordinateModel(50.092729, 14.42138) }, // Hospital Na Františku Prague
        new HospitalModel { Location = new CoordinateModel(50.0762201, 14.4754635) }, // University Hospital Královské Vinohrady
        new HospitalModel { Location = new CoordinateModel(50.07339090000001, 14.421015) }, // Všeobecná fakultní nemocnice v Praze
        new HospitalModel { Location = new CoordinateModel(50.0898072, 14.363407) }, // Military University Hospital Prague
        new HospitalModel { Location = new CoordinateModel(50.0747247, 14.3544551) }, // Na Homolce Hospital
        new HospitalModel { Location = new CoordinateModel(50.1153394, 14.4642339) }, // Hospital Na Bulovce
        new HospitalModel { Location = new CoordinateModel(50.08706000000001, 14.3959883) }, // The Hospital Church of the Merciful Sisters of Saint Karla Boromejský in Prague
        new HospitalModel { Location = new CoordinateModel(50.0300576, 14.4564294) }, // Fakultní Thomayerova nemocnice
        new HospitalModel { Location = new CoordinateModel(50.0734726, 14.3406404) }, // Motol University Hospital
        new HospitalModel { Location = new CoordinateModel(50.0714652, 14.42698) }, // Maternity hospital Apolinář
        new HospitalModel { Location = new CoordinateModel(50.0735033, 14.3399829) }, // Accident and Emergency
        new HospitalModel { Location = new CoordinateModel(50.0731972, 14.4273412) }  // General Faculty Hospital - Department of Occupational Illnesses
      },
      AvailableMedicTeams = Enumerable.Range(0, 200).Select(_ => new MedicTeamModel()).ToList(),
      AvailableAmbulances = Enumerable.Range(0, 140).Select(_ => new AmbulanceModel()).ToList()
    };

    return WorldMapper.MapBack(worldModel);
  }

  public ImmutableArray<Incident> GetIncidents(int count = 330)
  {
    // Incidents init
    Polygon polygon = GetPraguePolygon();
    // Prague has area of 500km^2, and its symmetric, meaning its about 25 * 25 km.
    // The stddev is hence chosen as ~10km, in order to concentrate most of the incidents in the center of Prague.
    List<CoordinateModel> locations = _dataGenerator.GetRandomIncidentsLocationsInPolygon(polygon, count, 0.1 /* ~11km */, _random);
    
    ImmutableArray<Incident> incidents = _dataGenerator.GenerateIncidentModelsFromCoordinates(
      coordinates: locations,
      totalDuration: 21.ToHours().ToSeconds(),
      onSceneDurationNormalExpected: 20.ToMinutes().ToSeconds(),
      onSceneDurationNormalStddev: 5.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalExpected: 15.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalStddev: 5.ToMinutes().ToSeconds(),
      random: _random
    ).Select(IncidentMapper.MapBack).ToImmutableArray();

    return incidents;
  }

  public ImmutableArray<Incident> GetMondayIncidents(int count = 400)
  {
    // #average incidents count / hours in a day * 9h-12h * dvakrat, protoze od 9 do 12 se deje dvakrat tolik incidentu oproti prumeru
    // 330 / 24 * 3 * 2 ~ 82
    var percentage = (count / 24 * 3 * 2) / (double)count;
    
    List<CoordinateModel> locations = _dataGenerator.GetRandomIncidentsLocationsInPolygon(GetPraguePolygon(), count, 0.08, _random);
    
    ImmutableArray<Incident> incidents = _dataGenerator.GenerateIncidentModelsFromCoordinatesNormal(
      coordinates: locations,
      totalDuration: 21.ToHours().ToSeconds(),
      onSceneDurationNormalExpected: 20.ToMinutes().ToSeconds(),
      onSceneDurationNormalStddev: 15.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalExpected: 15.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalStddev: 5.ToMinutes().ToSeconds(),
      9,
      12,
      percentage,
      random: _random
    ).Select(IncidentMapper.MapBack).ToImmutableArray();

    return incidents;
  }

  public ShiftTimes GetShiftTimes()
  {
    // shift times init
    ShiftTimes shiftTimes = new()
    {
      AllowedShiftStartingTimesSec = new HashSet<int>()
      {
        0.ToHours().ToMinutes().ToSeconds().Value,
        //2.ToHours().ToMinutes().ToSeconds().Value,
        4.ToHours().ToMinutes().ToSeconds().Value,
        // 6.ToHours().ToMinutes().ToSeconds().Value,
        8.ToHours().ToMinutes().ToSeconds().Value,
        //10.ToHours().ToMinutes().ToSeconds().Value,
        12.ToHours().ToMinutes().ToSeconds().Value,
        //14.ToHours().ToMinutes().ToSeconds().Value,
        16.ToHours().ToMinutes().ToSeconds().Value,
        //18.ToHours().ToMinutes().ToSeconds().Value,
        20.ToHours().ToMinutes().ToSeconds().Value,
      },

      AllowedShiftDurationsSec = new HashSet<int>()
      {
        4.ToHours().ToMinutes().ToSeconds().Value,
        6.ToHours().ToMinutes().ToSeconds().Value,
        8.ToHours().ToMinutes().ToSeconds().Value,
        10.ToHours().ToMinutes().ToSeconds().Value,
        12.ToHours().ToMinutes().ToSeconds().Value,
      }
    };
    //

    return shiftTimes;
  }
   
  public Polygon GetPraguePolygon()
  {
    return PolygonParser.ParsePolygon(File.ReadAllText("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/WorldModel/Data/PraguePolygon"));
  }
}
