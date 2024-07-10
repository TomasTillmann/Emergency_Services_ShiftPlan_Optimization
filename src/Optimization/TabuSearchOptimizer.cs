using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using DataModel.Interfaces;
using DistanceAPI;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public class TabuSearchOptimizer : NeighbourOptimizer
{
  public EmergencyServicePlan StartPlan { get; set; }
  public int TabuTenure { get; set; }
  public int PlateuIteration { get; set; }
  public int MaxIterations { get; set; }
  public int PlansVisited { get; private set; }
  public int TabuHit { get; private set; }
  
  public TextWriter Writer { get; set; }
  public TextWriter BestPlansWriter { get; set; }

  private readonly MoveMaker _moveMaker = new();
  private readonly IDistanceCalculator _distanceCalculator;
  private readonly Stopwatch _sw = new();

  public TabuSearchOptimizer(World world, Constraints constraints, IDistanceCalculator distanceCalculator, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator, int tabuTenure, int maxIterations = int.MaxValue)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
    MaxIterations = maxIterations;
    TabuTenure = tabuTenure;
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
    _distanceCalculator = distanceCalculator;
  }

  public override List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    _sw.Restart();
    _sw.Start();
    PlansVisited = 0;
    TabuHit = 0;
    
    HashSet<MoveSequence> tabu = new(TabuTenure, new MoveSequenceComparer());
    MoveSequence[] tabuQueue = new MoveSequence[TabuTenure];
    int tabuQueueIndex = -1;

    EmergencyServicePlan current = EmergencyServicePlan.GetNewEmpty(World);
    current.FillFrom(StartPlan);

    EmergencyServicePlan bestPlan = EmergencyServicePlan.GetNewEmpty(World);
    bestPlan.FillFrom(current);
    double bestPlanEval = UtilityFunction.Evaluate(bestPlan, incidents.AsSpan());

    MoveSequenceDuo bestMove = MoveSequenceDuo.GetNewEmpty(MoveGenerator.MovesBufferSize);
    for (PlateuIteration = 0; PlateuIteration < MaxIterations; ++PlateuIteration)
    {
      double bestNeighborEval = double.MinValue;
      int neighbor = 0;
      foreach (var move in MoveGenerator.GetMoves(current))
      {
        Writer.WriteLine($"elapsed: {_sw.Elapsed.TotalSeconds}, PlateuIteration: {PlateuIteration}, neighbor: {neighbor++}, PlansVisited: {PlansVisited}, TabuHit: {TabuHit}");
        Writer.Flush();
        ++PlansVisited;
        _moveMaker.ModifyMakeMove(current, move.Normal);

        double neighbourEval = UtilityFunction.Evaluate(current, incidents.AsSpan());

        if (tabu.Contains(move.Normal))
        {
          ++TabuHit;
        }

        if (neighbourEval > bestNeighborEval && !tabu.Contains(move.Normal))
        {
          Simulation simulation = new(World, Constraints, _distanceCalculator);
          Writer.WriteLine($"UPDATE: elapsed: {_sw.Elapsed.TotalSeconds}, cost: {current.Cost}, allocatedTeams: {current.MedicTeamsCount}, allocatedAmbulances: {current.AmbulancesCount}, handled: {simulation.HandledIncidentsCount}, eval: {neighbourEval}");
          BestPlansWriter.WriteLine(JsonSerializer.Serialize(current));
          BestPlansWriter.WriteLine("GANT");
          new GaantView(World, Constraints, _distanceCalculator).Show(current, incidents.AsSpan(), BestPlansWriter);
          BestPlansWriter.WriteLine("-----------");
          BestPlansWriter.Flush();
          bestNeighborEval = neighbourEval;
          bestMove.FillFrom(move);
        }
        else if (neighbourEval > bestPlanEval)
        {
          Simulation simulation = new(World, Constraints, _distanceCalculator);
          Writer.WriteLine($"UPDATE: elapsed: {_sw.Elapsed.TotalSeconds}, cost: {current.Cost}, allocatedTeams: {current.MedicTeamsCount}, allocatedAmbulances: {current.AmbulancesCount}, handled: {simulation.HandledIncidentsCount}, eval: {neighbourEval}");
          BestPlansWriter.WriteLine(JsonSerializer.Serialize(current));
          BestPlansWriter.WriteLine("GANT");
          new GaantView(World, Constraints, _distanceCalculator).Show(current, incidents.AsSpan(), BestPlansWriter);
          BestPlansWriter.WriteLine("-----------");
          BestPlansWriter.Flush();
          bestNeighborEval = neighbourEval;
          bestMove.FillFrom(move);
        }

        _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
      }

      // plateu
      if (bestNeighborEval == double.MinValue)
      {
        return [bestPlan];
      }

      _moveMaker.ModifyMakeMove(current, bestMove.Normal);

      if (bestNeighborEval > bestPlanEval)
      {
        bestPlan.FillFrom(current);
        bestPlanEval = bestNeighborEval;
      }

      int position = (tabuQueueIndex + 1) % TabuTenure;
      //tabu.Remove(tabuQueue[position]);
      MoveSequence bestMoveCopy = MoveSequence.GetNewFrom(bestMove.Inverse);
      tabu.Add(bestMoveCopy);
      tabuQueueIndex = position;
      tabuQueue[tabuQueueIndex] = bestMoveCopy;
    }

    return [bestPlan];
  }
}

