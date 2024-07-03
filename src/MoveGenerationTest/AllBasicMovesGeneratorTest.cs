using System.Diagnostics;
using System.Text.Json;
using ESSP.DataModel;
using Optimizing;
using FluentAssertions;
using Microsoft.VisualBasic;

namespace MoveGenerationTest;

public static class BasicMovesGeneratorHelperExtensions
{
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
        //File.WriteAllText("FromEmptyTeamAllocationsTestExpected.json",
         //               JsonSerializer.Serialize(moves, new JsonSerializerOptions { WriteIndented = true }));
         var expected = JsonSerializer.Deserialize<List<MoveSequenceDuo>>(File.ReadAllText("FromEmptyTeamAllocationsTestExpected.json"));
         moves.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public void FromRandom1()
    {
        Random random = new Random(66);
        var input = new MoveGeneratorTestInput(random);
        World world = input.GetWorld();
        Constraints constraints = input.GetConstraints();
        ShiftTimes shiftTimes = input.GetShiftTimes();
        EmergencyServicePlan plan = EmergencyServicePlan.GetNewEmpty(world);
        
        
        AllBasicMovesGenerator moveGenerator = new(shiftTimes, constraints);
        List<MoveSequenceDuo> moves = moveGenerator.GetMoves(plan).Enumerate();
         moves.Should().BeEquivalentTo(new List<MoveSequenceDuo>());
    }
    
    [Fact]
    public void FromEmptyAmbulanceAllocation()
    {
    }
    
    [Fact]
    public void FromEmptyAmbulanceDeallocation()
    {
    }
}