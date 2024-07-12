using ESSP.DataModel;
using FluentAssertions;
using Optimizing;
using Simulating;

namespace MoveGenerationTest;

public class TabuSearchTest
{
  [Theory]
  [InlineData(1)]
  [InlineData(50)]
  public void TabuComparerMoveTest(int count)
  {
    Random random = new(69);
    var input = new EmergencyServicePlanTestInput(random);
    World world = input.GetWorld();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    Constraints constraints = input.GetConstraints();
    PlanSampler sampler = new PlanSamplerUniform(world, shiftTimes, constraints, 0.5, random);

    var movesGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
    for (int i = 0; i < count; ++i)
    {
      HashSet<MoveSequence> tabu = new(new MoveSequenceComparer());
      EmergencyServicePlan plan = sampler.Sample();
      IEnumerable<MoveSequenceDuo> moves = movesGenerator.GetMoves(plan);
      foreach (var move in moves)
      {
        tabu.Add(move.Normal);
        tabu.Add(move.Inverse);
      }

      foreach (var move in moves)
      {
        tabu.Contains(move.Normal).Should().Be(true);
        tabu.Contains(move.Inverse).Should().Be(true);
      }
    }
  }
}
