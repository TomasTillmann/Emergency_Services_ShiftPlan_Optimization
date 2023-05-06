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
            dataProvider.GetIncidents(20, 24.ToHours())
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
        Incidents incidents = dataProvider.GetIncidents(20, 24.ToHours());
        incidents.Value = incidents.Value.GetRange(0, 6);
        ShiftPlan shiftPlan = dataProvider.GetShiftPlan();
        ShiftPlan shiftPlan2 = new(shiftPlan.Shifts.GetRange(0, 6));
        shiftPlan2.ModifyToLargest(new Constraints(null, new List<Seconds> { 12.ToHours().ToSeconds() }));


        DataSerializer.Serialize(world, "test1/world.json");
        DataSerializer.Serialize(incidents, "test1/incidents.json");
        DataSerializer.Serialize(shiftPlan2, "test1/shiftPlan.json");
        DataSerializer.Serialize(dataProvider.GetDistanceCalculator(), "test1/distanceCalculator2D.json");

        Simulation simulation = new(world, dataProvider.GetDistanceCalculator());
        Statistics stats = simulation.Run(incidents.Value, shiftPlan2);

        DataSerializer.Serialize(stats, "test1/stats_result.json");
        DataSerializer.Serialize(shiftPlan2, "test1/shiftPlan_result.json");
    }
#endif

    //static void Main()
    //{

    //}
}
