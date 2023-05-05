using DataModel.Interfaces;
using DataProviding;
using ESSP.DataModel;

namespace ESSP_Tests
{
    public abstract class Tests
    {
        protected static TestDataProvider dataProvider { get; } = new();
        protected IDistanceCalculator distanceCalculator { get; }

        public Tests()
        {
            distanceCalculator = dataProvider.GetDistanceCalculator();
        }

        public string CollectionMessage<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            return string.Join("\n", expected.Visualize(), actual.Visualize());
        }
    }
}