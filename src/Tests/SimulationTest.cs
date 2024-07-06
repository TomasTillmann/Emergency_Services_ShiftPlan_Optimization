using System.Collections.Immutable;
using ESSP.DataModel;
using FluentAssertions;
using Optimizing;
using Simulating;

namespace MoveGenerationTest;

public class SimulationTest
{
    [Theory]
    [InlineData(10, "ref")]
    [InlineData(100, "ref")]
    [InlineData(200, "ref")]
    [InlineData(10, "FillFrom")]
    [InlineData(100, "FillFrom")]
    [InlineData(200, "FillFrom")]
    public void SimulationStateChangingEquivalenceTest(int incidentsCount, string type)
    {
        Random random = new Random(66);
        var input = new SimulationTestInput(random);
        World world = input.GetWorld();
        Constraints constraints = input.GetConstraints();
        ShiftTimes shiftTimes = input.GetShiftTimes();
        ImmutableArray<Incident> incidents = input.GetIncidents(incidentsCount);
        IPlanSampler sampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.8, random);
        EmergencyServicePlan plan = sampler.Sample();

        Simulation simulation = new(world, constraints);
        int expectedHandled = simulation.Run(plan, incidents.AsSpan());
        for (int i = 1; i < incidentsCount - 1; ++i)
        {
            Simulation simulation1 = new Simulation(world, constraints);
            Simulation simulation2 = new Simulation(world, constraints);
            simulation1.Run(plan, incidents.AsSpan(0, incidentsCount - i));
            if (type == "FillFrom")
            {
                for (int j = 0; j < 10; ++j) // to see whether can be done on one instance repeatedly
                {
                    simulation2.State.FillFrom(simulation1.State);
                    simulation2.Run(plan, incidents.AsSpan(incidentsCount - i, i), resetState: false);
                    (simulation1.HandledIncidentsCount + simulation2.HandledIncidentsCount).Should().Be(expectedHandled);
                }
            }
            else
            {
                // same instance
                simulation2.State = simulation1.State;
                simulation2.Run(plan, incidents.AsSpan(incidentsCount - i, i), resetState: false);
                (simulation1.HandledIncidentsCount + simulation2.HandledIncidentsCount).Should().Be(expectedHandled);
            }
        }
    }
}