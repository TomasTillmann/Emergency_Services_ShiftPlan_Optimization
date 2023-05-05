using DataModel.Interfaces;
using DataProviding;
using ESSP.DataModel;

namespace ESSP_Tests
{
    public abstract class Tests
    {
        protected static TestDataProvider testDataProvider { get; }
        protected static IDistanceCalculator distanceCalculator { get; }
        protected static PlannableIncident.Factory plannableIncidentFactory { get; }

        static Tests()
        {
            testDataProvider = new();
            distanceCalculator = testDataProvider.GetDistanceCalculator();
            plannableIncidentFactory = new(distanceCalculator, testDataProvider.GetHospitals());
        }

        public string CollectionMessage<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            return string.Join("\n", expected.Visualize(), actual.Visualize());
        }
    }
}