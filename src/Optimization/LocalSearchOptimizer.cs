
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using DataModel.Interfaces;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public class LocalSearchOptimizer : NeighbourOptimizer
{
  public EmergencyServicePlan StartPlan { get; set; }
  public int PlateuIteration { get; private set; }
  public int MaxIterations { get; set; }
  
  public int PlansVisited { get; private set; }
  
  public TextWriter Writer { get; set; }
  public TextWriter BestPlansWriter { get; set; }

  private readonly Stopwatch _sw = new();
  private readonly IDistanceCalculator _distanceCalculator;

  public LocalSearchOptimizer(int maxIterations, World world, Constraints constraints, IDistanceCalculator distanceCalculator, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
    MaxIterations = maxIterations;
    _distanceCalculator = distanceCalculator;
  }

  public override List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    _sw.Restart();
    _sw.Start();
    PlansVisited = 0;
    EmergencyServicePlan current = EmergencyServicePlan.GetNewEmpty(World);
    current.FillFrom(StartPlan);

    MoveSequence bestMove = MoveSequence.GetNewEmpty(MoveGenerator.MovesBufferSize);
    for (PlateuIteration = 0; PlateuIteration < MaxIterations; ++PlateuIteration)
    {
      bestMove.Count = 0;
      double bestEval = UtilityFunction.Evaluate(current, incidents.AsSpan());
      Writer.WriteLine($"elapsed: {_sw.Elapsed.TotalSeconds}, Iteration: {PlateuIteration}, PlansVisited: {PlansVisited}, bestEval: {bestEval}");
      Writer.Flush();

      foreach (MoveSequenceDuo moves in MoveGenerator.GetMoves(current))
      {
        ++PlansVisited;
        Console.WriteLine($"PlansVisited: {PlansVisited}");
        ModifyMakeMove(current, moves.Normal);

        double neighbourEval = UtilityFunction.Evaluate(current, incidents.AsSpan());
        if (neighbourEval >= bestEval)
        {
          Simulation simulation = new(World, Constraints, _distanceCalculator);
          simulation.Run(current, incidents.AsSpan());
          if (PlansVisited % 10 == 0)
          {
            Writer.WriteLine($"UPDATE: elapsed: {_sw.Elapsed.TotalSeconds}, cost: {current.Cost}, allocatedTeams: {current.MedicTeamsCount}, allocatedAmbulances: {current.AmbulancesCount}, handled: {simulation.HandledIncidentsCount}, eval: {neighbourEval}");
            BestPlansWriter.WriteLine(JsonSerializer.Serialize(current));
          }
          //BestPlansWriter.WriteLine("GANT");
          //new GaantView(World, Constraints, _distanceCalculator).Show(current, incidents.AsSpan(), BestPlansWriter);
          //BestPlansWriter.WriteLine("-----------");
          BestPlansWriter.Flush();
          Writer.Flush();
          bestMove.FillFrom(moves.Normal);
          bestEval = neighbourEval;
        }

        ModifyMakeInverseMove(current, moves.Inverse);
      }
      

      // plateu
      if (bestMove.Count == 0)
      {
        break;
      }

      ModifyMakeMove(current, bestMove);
    }
    return new List<EmergencyServicePlan>()
    {
      current
    };
  }
}
