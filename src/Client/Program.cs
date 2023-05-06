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
}
