using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using DataModel.Interfaces;
using DistanceAPI;
using ESSP.DataModel;
using MyExtensions;
using Simulating;

namespace Optimizing;

public class SimulatedAnnealingOptimizer : NeighbourOptimizer
{
  public EmergencyServicePlan StartPlan { get; set; }
  public double StartTemp { get; set; }
  public double FinalTemp { get; set; }
  public int M_k { get; set; }
  public ICoolingSchedule CoolingSchedule { get; set; }
  public Random Random { get; set; }

  public int Iteration { get; private set; }
  
  public TextWriter Writer { get; set; }
  public TextWriter BestPlansWriter { get; set; }

  public int PlansVisited { get; private set; }
  private readonly MoveMaker _moveMaker = new();
  private readonly IDistanceCalculator _distanceCalculator;
  private readonly Stopwatch _sw = new();
  private readonly IMoveGenerator _moveGenerator;

  public SimulatedAnnealingOptimizer(World world, Constraints constraints, IDistanceCalculator distanceCalculator, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator,
      double startTemp, double finalTemp, int M_k, ICoolingSchedule coolingSchedule, Random random = null)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
    StartTemp = startTemp;
    FinalTemp = finalTemp;
    this.M_k = M_k;
    CoolingSchedule = coolingSchedule;
    _distanceCalculator = distanceCalculator;
    _moveGenerator = moveGenerator;
    Random = random ?? new Random();
  }

  public override List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    PlansVisited = 0;
    _sw.Restart();
    _sw.Start();
    EmergencyServicePlan current = EmergencyServicePlan.GetNewFrom(World, StartPlan);
    double currentEval = UtilityFunction.Evaluate(current, incidents.AsSpan());

    EmergencyServicePlan best = EmergencyServicePlan.GetNewFrom(World, current);
    double bestEval = currentEval;

    double temp = StartTemp;
    while (temp > FinalTemp)
    {
      ++Iteration;
      int neighbor = 0;
      for (int m = 0; m < M_k; ++m)
      {
        ++PlansVisited;
        //Writer.WriteLine($"elapsed: {_sw.Elapsed.TotalSeconds}, temp: {temp}, Iteration: {Iteration}, neighbor: {neighbor++}, PlansVisited: {PlansVisited}");
        //Writer.Flush();
        //MoveSequenceDuo move = RandomMoveSampler.Sample(current);
        var moves = _moveGenerator.GetMoves(current).Enumerate(2).ToList();
        int index = Random.Next(moves.Count);
        var move = moves[index];
        
        _moveMaker.ModifyMakeMove(current, move.Normal);

        double neighbourEval = UtilityFunction.Evaluate(current, incidents.AsSpan());
        double delta = currentEval - neighbourEval;
        
        Writer.WriteLine($"X: elapsed: {_sw.Elapsed.TotalSeconds}, cost: {current.Cost}, allocatedTeams: {current.MedicTeamsCount}, allocatedAmbulances: {current.AmbulancesCount}, handled: {UtilityFunction.HandledIncidentsCount}, eval: {currentEval}, m: {m}, temp: {temp}");
        //Writer.WriteLine(move);
        Writer.Flush();
        
        if (delta > 0)
        {
          double probabilityToNotAccept = 1 - Math.Exp(-delta / temp);
          if (Random.NextDouble() < probabilityToNotAccept)
          {
            Console.WriteLine("NOT ACCEPTED");
            _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
            --PlansVisited;
            continue;
          }
        }

        currentEval = neighbourEval;
        if (currentEval > bestEval)
        {
          Simulation simulation = new(World, Constraints, _distanceCalculator);
          simulation.Run(current, incidents.AsSpan());
          Writer.WriteLine($"UPDATE: elapsed: {_sw.Elapsed.TotalSeconds}, cost: {current.Cost}, allocatedTeams: {current.MedicTeamsCount}, allocatedAmbulances: {current.AmbulancesCount}, handled: {simulation.HandledIncidentsCount}, eval: {currentEval}, m: {m}, temp: {temp}");
          BestPlansWriter.WriteLine(JsonSerializer.Serialize(current));
          //BestPlansWriter.WriteLine("GANT");
          //new GaantView(World, Constraints, _distanceCalculator).Show(current, incidents.AsSpan(), BestPlansWriter);
          //BestPlansWriter.WriteLine("-----------");
          BestPlansWriter.Flush();
          Writer.Flush();
          best.FillFrom(current);
          bestEval = currentEval;
        }
      }

      temp = CoolingSchedule.Calculate(temp);
    }

    return [best];
  }
}


