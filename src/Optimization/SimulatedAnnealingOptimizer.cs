using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using DataModel.Interfaces;
using DistanceAPI;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public class SimulatedAnnealingOptimizer : NeighbourSamplerOptimizer
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

  public SimulatedAnnealingOptimizer(World world, Constraints constraints, IDistanceCalculator distanceCalculator, IUtilityFunction utilityFunction, IRandomMoveSampler randomMoveSampler,
      double startTemp, double finalTemp, int M_k, ICoolingSchedule coolingSchedule, Random random = null)
  : base(world, constraints, utilityFunction, randomMoveSampler)
  {
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
    StartTemp = startTemp;
    FinalTemp = finalTemp;
    this.M_k = M_k;
    CoolingSchedule = coolingSchedule;
    _distanceCalculator = distanceCalculator;
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
        Writer.WriteLine($"elapsed: {_sw.Elapsed.TotalSeconds}, temp: {temp}, Iteration: {Iteration}, neighbor: {neighbor++}, PlansVisited: {PlansVisited}");
        Writer.Flush();
        MoveSequenceDuo move = RandomMoveSampler.Sample(current);
        _moveMaker.ModifyMakeMove(current, move.Normal);

        double neighbourEval = UtilityFunction.Evaluate(current, incidents.AsSpan());
        double delta = currentEval - neighbourEval;
        if (delta > 0)
        {
          double probabilityToNotAccept = 1 - Math.Exp(-delta / temp);
          if (Random.NextDouble() < probabilityToNotAccept)
          {
            _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
            --PlansVisited;
            continue;
          }
        }

        currentEval = neighbourEval;
        if (currentEval > bestEval)
        {
          Simulation simulation = new(World, Constraints, _distanceCalculator);
          Writer.WriteLine($"UPDATE: elapsed: {_sw.Elapsed.TotalSeconds}, cost: {current.Cost}, allocatedTeams: {current.MedicTeamsCount}, allocatedAmbulances: {current.AmbulancesCount}, handled: {simulation.HandledIncidentsCount}, eval: {currentEval}, m: {m}");
          BestPlansWriter.WriteLine(JsonSerializer.Serialize(current));
          BestPlansWriter.WriteLine("GANT");
          new GaantView(World, Constraints, _distanceCalculator).Show(current, incidents.AsSpan(), BestPlansWriter);
          BestPlansWriter.WriteLine("-----------");
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


