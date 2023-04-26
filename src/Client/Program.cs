using DataProviding;
using DataModel.Interfaces;
using ESSP.DataModel;
using Optimizing;

namespace Client;

class Program
{
    static void Main(string[] args)
    {
        DataProvider dataProvider = new(1000.ToMeters(), 1000.ToMeters());

        List<Hospital> hospitals = dataProvider.GetHospitals();
        List<Depot> depots = dataProvider.GetDepots();
        IDistanceCalculator distanceCalculator = dataProvider.GetDistanceCalculator();

        List<Incidents> incidents = new()
        {
            dataProvider.GetIncidents(10, 2.ToHours())
        };

        Optimizer optimizer = new Optimizer(new World(depots, hospitals), distanceCalculator, dataProvider.GetConstraints());
        ShiftPlan shiftPlan = dataProvider.GetShiftPlan();

        optimizer.FindOptimal(shiftPlan, incidents);
    }
}
