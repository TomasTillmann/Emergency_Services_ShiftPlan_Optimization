using DataModel.Interfaces;
using DataProviding;
using ESSP.DataModel;

namespace ESSP_Tests
{
    public abstract class Tests
    {
        protected TestDataProvider dataProvider { get; } 
        protected IDistanceCalculator distanceCalculator { get; }

        public Tests()
        {
            dataProvider = new();
            distanceCalculator = dataProvider.GetDistanceCalculator();
        }

        public string CollectionMessage<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            return string.Join("\n", expected.Visualize(), actual.Visualize());
        }
    }
}