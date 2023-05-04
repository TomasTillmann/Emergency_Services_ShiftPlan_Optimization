using DataModel.Interfaces;
using ESSP.DataModel;

namespace ESSP_Tests
{
    public static class Helpers
    {
        public static string Visualize<T>(this IEnumerable<T> enumerable)
        {
            const string separator = ", ";
            string str = "";
            foreach(T item in enumerable)
            {
                str += item?.ToString() + separator;
            }

            // remove the last separator
            if(str.Length > 0)
            {
                str.Substring(0, str.Length - separator.Length);
            }

            return str;
        }
    }

    public class DistanceCalculatorTests : Tests
    {
        private DistanceCalculator distanceCalculator;
        [SetUp]
        public void Setup()
        {
            distanceCalculator = new();
        }

        [Test]
        public void GetNearestLocatableTest()
        {
            ILocatable amb1 = new Ambulance(new AmbulanceType("aaa", 100), new Coordinate { X = 100.ToMeters(), Y = 100.ToMeters() }, 10.ToSeconds());
            ILocatable h1 = new Hospital(new Coordinate { X = 150.ToMeters(), Y = 150.ToMeters() });
            ILocatable h2 = new Hospital(new Coordinate { X = 250.ToMeters(), Y = 150.ToMeters() });
            ILocatable h3 = new Hospital(new Coordinate { X = 150.ToMeters(), Y = 170.ToMeters() });
            ILocatable h4 = new Hospital(new Coordinate { X = 300.ToMeters(), Y = 99.ToMeters() });
            ILocatable h5 = new Hospital(new Coordinate { X = 0.ToMeters(), Y = 0.ToMeters() });

            IEnumerable<ILocatable> nearest = distanceCalculator.GetNearestLocatable(amb1, new ILocatable[] { h1, h2, h3, h4, h5 });

            IEnumerable<ILocatable> expected = new ILocatable[] { h1 };
            CollectionAssert.AreEquivalent(expected, nearest,
                CollectionMessage(expected.Select(locatable => locatable.Location),
                                  nearest.Select(locatable => locatable.Location)));
        }

        [Test]
        public void GetTravelDurationTest()
        {
            ILocatable amb1 = new Ambulance(new AmbulanceType("aaa", 100), new Coordinate { X = 100.ToMeters(), Y = 100.ToMeters() }, 10.ToSeconds());
            ILocatable h1 = new Hospital(new Coordinate { X = 120.ToMeters(), Y = 150.ToMeters() });

            Seconds duration = distanceCalculator.GetTravelDuration(amb1, h1, 0.ToSeconds());

            Seconds expected = ((int)(Math.Sqrt(20 * 20 + 50 * 50) / DistanceCalculator.Speed.Value)).ToSeconds();
            Assert.That(duration, Is.EqualTo(expected));
        }

        [Test]
        public void GetTravelDurationSameLocationTest()
        {
            ILocatable amb1 = new Ambulance(new AmbulanceType("aaa", 100), new Coordinate { X = 100.ToMeters(), Y = 100.ToMeters() }, 10.ToSeconds());
            ILocatable h1 = new Hospital(new Coordinate { X = 100.ToMeters(), Y = 100.ToMeters() });

            Seconds duration = distanceCalculator.GetTravelDuration(amb1, h1, 0.ToSeconds());

            Seconds expected = 0.ToSeconds(); 
            Assert.That(duration, Is.EqualTo(expected));
        }

        [Test]
        public void GetTravelDurationBigNumberTest()
        {
            ILocatable amb1 = new Ambulance(new AmbulanceType("aaa", 100), new Coordinate { X = 100.ToMeters(), Y = 100.ToMeters() }, 10.ToSeconds());
            ILocatable h1 = new Hospital(new Coordinate { X = 300_000.ToMeters(), Y = 300_000.ToMeters() });

            Seconds duration = distanceCalculator.GetTravelDuration(amb1, h1, 0.ToSeconds());

            Seconds expected = ((int)(Math.Sqrt((ulong)299_900 * 299_900 + (ulong)299_900 * 299_900) / DistanceCalculator.Speed.Value)).ToSeconds();
            Assert.That(duration, Is.EqualTo(expected));
        }

        [Test]
        public void GetNewLocationInXDirectionUpTest()
        {
            ILocatable amb1 = new Ambulance(new AmbulanceType("aaa", 100), new Coordinate { X = 100.ToMeters(), Y = 100.ToMeters() }, 10.ToSeconds());
            ILocatable h1 = new Hospital(new Coordinate { X = 5000.ToMeters(), Y = 100.ToMeters() });

            Coordinate location = distanceCalculator.GetNewLocation(amb1.Location, h1.Location, 30.ToSeconds(), 0.ToSeconds());

            Coordinate expected = new Coordinate { X = (100 + DistanceCalculator.Speed.Value * 30).ToMeters(), Y = 100.ToMeters() }; 
            Assert.That(location, Is.EqualTo(expected));
        }

        [Test]
        public void GetNewLocationInXDirectionDownTest()
        {
            ILocatable amb1 = new Ambulance(new AmbulanceType("aaa", 100), new Coordinate { X = 5000.ToMeters(), Y = 100.ToMeters() }, 10.ToSeconds());
            ILocatable h1 = new Hospital(new Coordinate { X = 100.ToMeters(), Y = 100.ToMeters() });

            Coordinate location = distanceCalculator.GetNewLocation(amb1.Location, h1.Location, 30.ToSeconds(), 0.ToSeconds());

            Coordinate expected = new Coordinate { X = (5000 - DistanceCalculator.Speed.Value * 30).ToMeters(), Y = 100.ToMeters() }; 
            Assert.That(location, Is.EqualTo(expected));
        }

        [Test]
        public void GetNewLocationInYDirectionLeftTest()
        {
            ILocatable amb1 = new Ambulance(new AmbulanceType("aaa", 100), new Coordinate { X = 100.ToMeters(), Y = 5000.ToMeters() }, 10.ToSeconds());
            ILocatable h1 = new Hospital(new Coordinate { X = 100.ToMeters(), Y = 100.ToMeters() });

            Coordinate location = distanceCalculator.GetNewLocation(amb1.Location, h1.Location, 30.ToSeconds(), 0.ToSeconds());

            Coordinate expected = new Coordinate { Y = (5000 - DistanceCalculator.Speed.Value * 30).ToMeters(), X = 100.ToMeters() }; 
            Assert.That(location, Is.EqualTo(expected));
        }

        [Test]
        public void GetNewLocationInYDirectionRightTest()
        {
            ILocatable amb1 = new Ambulance(new AmbulanceType("aaa", 100), new Coordinate { X = 100.ToMeters(), Y = 100.ToMeters() }, 10.ToSeconds());
            ILocatable h1 = new Hospital(new Coordinate { X = 100.ToMeters(), Y = 5000.ToMeters() });

            Coordinate location = distanceCalculator.GetNewLocation(amb1.Location, h1.Location, 30.ToSeconds(), 0.ToSeconds());

            Coordinate expected = new Coordinate { Y = (100 + DistanceCalculator.Speed.Value * 30).ToMeters(), X = 100.ToMeters() }; 
            Assert.That(location, Is.EqualTo(expected));
        }

        public void GetNewLocationCroppedTest()
        {
            ILocatable amb1 = new Ambulance(new AmbulanceType("aaa", 100), new Coordinate { X = 120.ToMeters(), Y = 100.ToMeters() }, 10.ToSeconds());
            ILocatable h1 = new Hospital(new Coordinate { X = 100.ToMeters(), Y = 100.ToMeters() });

            Coordinate location = h1.Location;

            Coordinate expected = new Coordinate { X = (5000 - DistanceCalculator.Speed.Value * 30).ToMeters(), Y = 100.ToMeters() }; 
            Assert.That(location, Is.EqualTo(expected));
        }
    }
}