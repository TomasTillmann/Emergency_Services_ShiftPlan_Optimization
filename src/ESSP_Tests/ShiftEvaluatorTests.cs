using ESSP.DataModel;

namespace ESSP_Tests;

public abstract class ShiftEvaluatorTestsBase : Tests
{
    public static IEnumerable<TestCaseData> HandlingShiftsTestSource()
    {
        yield return new TestCaseData(new Incident(
            coordinate: new Coordinate { X = 0.ToMeters(), Y = 0.ToMeters() },
            occurence: 10.ToSeconds(),
            onSceneDuration: 240.ToSeconds(),
            inHospitalDelivery: 5.ToMinutes().ToSeconds(),
            type: new IncidentType
            {
                Name = "I1",
                AllowedAmbulanceTypes = new HashSet<AmbulanceType> { new AmbulanceType("A1", 300) },
                MaximumResponseTime = 30.ToMinutes().ToSeconds(),
            }
        ));
    }
}

public class ShiftEvaluatorTests : ShiftEvaluatorTestsBase
{
    [TestCaseSource(nameof(HandlingShiftsTestSource))]
    public void IsHandlingTest(Incident incident)
    {
        //TODO: implement
    }
}
