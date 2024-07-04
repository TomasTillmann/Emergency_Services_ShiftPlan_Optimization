using System.Diagnostics;
using System.Text.Json;
using ESSP.DataModel;
using Optimizing;
using FluentAssertions;
using Microsoft.VisualBasic;

namespace MoveGenerationTest;

public static class BasicMovesGeneratorHelperExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="moves">If produced by <see cref="MoveGeneratorBase"/>, the instance is shared, due to performance reasons.
    /// For correct enumeration, calling e.g. ToList() is not sufficient. You need to copy the shred instance for every one enumeration.</param>
    /// <returns></returns>
    public static List<MoveSequenceDuo> Enumerate(this IEnumerable<MoveSequenceDuo> moves)
    {
        return moves
            .Select(shared =>
            {
                MoveSequenceDuo x = new MoveSequenceDuo(1);
                shared.Normal.Count = shared.Count; 
                shared.Inverse.Count = shared.Count; 
                x.Normal.FillFrom(shared.Normal);
                x.Inverse.FillFrom(shared.Inverse);
                return x;
            }).ToList();
    }
}

public class AllBasicMovesGeneratorTest
{
    [Fact]
    public void FromEmpty()
    {
        Random random = new Random(66);
        var input = new MoveGeneratorTestInput(random);
        World world = input.GetWorld();
        Constraints constraints = input.GetConstraints();
        ShiftTimes shiftTimes = input.GetShiftTimes();
        EmergencyServicePlan plan = EmergencyServicePlan.GetNewEmpty(world);
        
        AllBasicMovesGenerator moveGenerator = new(shiftTimes, constraints);
        List<MoveSequenceDuo> moves = moveGenerator.GetMoves(plan).Enumerate();
         var expected = JsonSerializer.Deserialize<List<MoveSequenceDuo>>(File.ReadAllText("FromEmptyTeamAllocationsTestExpected.json"));
         moves.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public void FromRandom1Percentage_100()
    {
        Random random = new Random(66);
        var input = new MoveGeneratorTestInput(random);
        World world = input.GetWorld();
        Constraints constraints = input.GetConstraints();
        ShiftTimes shiftTimes = input.GetShiftTimes();
        IPlanSampler sampler = new PlanSamperUniform(world, shiftTimes, constraints, 1.0, random);
        EmergencyServicePlan plan = sampler.Sample(); 
        
        AllBasicMovesGenerator moveGenerator = new(shiftTimes, constraints);
        List<MoveSequenceDuo> moves = moveGenerator.GetMoves(plan).Enumerate();
         var expected = JsonSerializer.Deserialize<List<MoveSequenceDuo>>(File.ReadAllText("FromRandom1AllocationsTestExpected.json"));
         moves.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public void FromRandom2Percentage_50()
    {
        Random random = new Random(66);
        var input = new MoveGeneratorTestInput(random);
        World world = input.GetWorld();
        Constraints constraints = input.GetConstraints();
        ShiftTimes shiftTimes = input.GetShiftTimes();
        IPlanSampler sampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.8, random);
        EmergencyServicePlan plan = sampler.Sample(); 
        
        AllBasicMovesGenerator moveGenerator = new(shiftTimes, constraints);
        List<MoveSequenceDuo> moves = moveGenerator.GetMoves(plan).Enumerate();
         var expected = JsonSerializer.Deserialize<List<MoveSequenceDuo>>(File.ReadAllText("FromRandom2AllocationsTestExpected.json"));
         moves.Should().BeEquivalentTo(expected);
    }
}