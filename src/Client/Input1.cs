using System.Collections.Immutable;
using ESSP.DataModel;

public class Input1 : IInputParametrization
{
  public Input Get()
  {
    WorldMapper worldMapper = new();
    DataModelGenerator dataGenerator = new();

    // World init
    World world = worldMapper.MapBack(dataGenerator.GenerateWorldModel(
      worldSize: new CoordinateModel { XMet = 50_000, YMet = 50_000 },
      depotsCount: 10,
      hospitalsCount: 20,
      ambulancesOnDepotNormalExpected: 5,
      ambulanceOnDepotNormalStddev: 1,
      ambTypes: new AmbulanceTypeModel[] {
        new AmbulanceTypeModel
        {
          Name = "A1",
          Cost = 40
        },
       new AmbulanceTypeModel
       {
         Name = "A2",
         Cost = 100
       },
       new AmbulanceTypeModel
       {
         Name = "A3",
         Cost = 120
       },
       new AmbulanceTypeModel
       {
         Name = "A4",
         Cost = 500
       },
      },
      ambTypeCategorical: new double[] { 0.5, 0.3, 0.15, 0.05 },
      incToAmbTypesTable: new Dictionary<string, HashSet<string>>
      {
        { "I1", new HashSet<string> { "A1", "A2", "A3", "A4" } },
        //{ "I2", new HashSet<string> { "A2", "A3", "A4" } }
      },
      random: new Random(42)
    ));

    // Incidents init
    IncidentMapper incidentMapper = new();
    ImmutableArray<Incident> incidents = dataGenerator.GenerateIncidentModels(
      worldSize: new CoordinateModel { XMet = 50_000, YMet = 50_000 },
      incidentsCount: 400,
      duration: 21.ToHours().ToSeconds(),
      onSceneDurationNormalExpected: 20.ToMinutes().ToSeconds(),
      onSceneDurationNormalStddev: 10.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalExpected: 15.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalStddev: 10.ToMinutes().ToSeconds(),
      incTypes: new IncidentTypeModel[] {
       new IncidentTypeModel
       {
         Name = "I1",
         MaximumResponseTimeSec = 2.ToHours().ToMinutes().ToSeconds().Value
       },
       // new IncidentTypeModel
       // {
       //   Name = "I2",
       //   MaximumResponseTimeSec = 1.ToHours().ToMinutes().ToSeconds().Value
       // },
       // new IncidentTypeModel
       // {
       //   Name = "I3",
       //   MaximumResponseTimeSec = 30.ToMinutes().ToSeconds().Value
       // },
      },
      //incTypesCategorical: new double[] { 0.7, 0.2, 0.1 },
      incTypesCategorical: new double[] { 1 },
      random: new Random(42)
    ).Select(inc => incidentMapper.MapBack(inc)).ToImmutableArray();
    //

    // Constraints init
    Constraints constraints = new()
    {
      AllowedShiftStartingTimesSec = new HashSet<int>()
      {
        0.ToHours().ToMinutes().ToSeconds().Value,
        8.ToHours().ToMinutes().ToSeconds().Value,
        12.ToHours().ToMinutes().ToSeconds().Value,
      },

      AllowedShiftDurationsSec = new HashSet<int>()
      {
        4.ToHours().ToMinutes().ToSeconds().Value,
        8.ToHours().ToMinutes().ToSeconds().Value,
        12.ToHours().ToMinutes().ToSeconds().Value,
      }
    };
    //

    // Success rated incidents init
    var successRatedIncidents = new List<SuccessRatedIncidents>()
    {
      new SuccessRatedIncidents { Value = incidents, SuccessRate = 1 }
    }.ToImmutableArray();
    //

    return new Input
    {
      World = world,
      Constraints = constraints,
      SuccessRatedIncidents = successRatedIncidents
    };
  }
}
