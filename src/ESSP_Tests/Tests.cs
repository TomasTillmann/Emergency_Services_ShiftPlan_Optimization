using DataModel.Interfaces;
using DataProviding;
using ESSP.DataModel;

namespace ESSP_Tests
{
    public abstract class Tests
    {
        protected DataProvider dataProvider { get; } 
        protected IDistanceCalculator distanceCalculator { get; }

        public Tests()
        {
            dataProvider = new (10_000.ToMeters(), 10_000.ToMeters());
            distanceCalculator = dataProvider.GetDistanceCalculator();
        }

        public string CollectionMessage<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            return string.Join("\n", expected.Visualize(), actual.Visualize());
        }
    }
}