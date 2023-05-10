using DataHandling;
using DataModel.Interfaces;
using ESSP.DataModel;
using Model.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optimizing;
using Simulating;

namespace Client;

class Program
{
#if false
    static void Main(string[] args)
    {
        DataProvider dataProvider = new();

        List<Hospital> hospitals = dataProvider.GetHospitals();
        List<Depot> depots = dataProvider.GetDepots();
        IDistanceCalculator distanceCalculator = dataProvider.GetDistanceCalculator();

        List<Incidents> incidents = new()
        {
            dataProvider.GetIncidents(100, 11.ToHours())
        };

        Optimizer optimizer = new Optimizer(new World(depots, hospitals), distanceCalculator, dataProvider.GetConstraints());
        ShiftPlan shiftPlan = dataProvider.GetShiftPlan();

        optimizer.FindOptimal(shiftPlan, incidents);
    }
#endif

#if true
    static void Main(string[] args)
    {
        DataProvider dataProvider = new();
        World world = dataProvider.GetWorld();
        Incidents incidents = dataProvider.GetIncidents(100, 11.ToHours());
        ShiftPlan shiftPlan = dataProvider.GetShiftPlan();
        shiftPlan.ModifyToLargest(new Constraints(null, new List<Seconds> { 12.ToHours().ToSeconds() }));


        DataSerializer.Serialize(world, "test2/world.json");
        DataSerializer.Serialize(incidents, "test2/incidents.json");
        DataSerializer.Serialize(shiftPlan, "test2/shiftPlan.json");
        DataSerializer.Serialize(dataProvider.GetDistanceCalculator(), "test2/distanceCalculator2D.json");

        Simulation simulation = new(world, dataProvider.GetDistanceCalculator());
        Statistics stats = simulation.Run(incidents.Value, shiftPlan);

        DataSerializer.Serialize(stats, "test2/stats_result.json");
        DataSerializer.Serialize(shiftPlan, "test2/shiftPlan_result.json");
    }
#endif

}
