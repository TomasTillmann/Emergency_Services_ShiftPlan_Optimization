using DataModel.Interfaces;
using DataHandling;
using ESSP.DataModel;

namespace ESSP_Tests
{
    public abstract class Tests
    {
        protected static TestDataProvider testDataProvider { get; }
        protected static IDistanceCalculator distanceCalculator { get; }
        protected static PlannableIncident.Factory plannableIncidentFactory { get; }

        protected static World world { get; } 

        static Tests()
        {
            testDataProvider = new();
            distanceCalculator = testDataProvider.GetDistanceCalculator();
            plannableIncidentFactory = new(distanceCalculator, testDataProvider.GetHospitals());
            world = new World(testDataProvider.GetDepots(), testDataProvider.GetHospitals(), testDataProvider.GetDistanceCalculator());
        }

        public string CollectionMessage<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            return string.Join("\n", expected.Visualize(), actual.Visualize());
        }
    }
}