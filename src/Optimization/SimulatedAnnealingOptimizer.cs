using ESSP.DataModel;

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

  private readonly MoveMaker _moveMaker = new();
  private readonly GaantView _gaantView;

  public SimulatedAnnealingOptimizer(World world, Constraints constraints, IUtilityFunction utilityFunction, IRandomMoveSampler randomMoveSampler,
      double startTemp, double finalTemp, int M_k, ICoolingSchedule coolingSchedule, Random random = null)
  : base(world, constraints, utilityFunction, randomMoveSampler)
  {
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
    StartTemp = startTemp;
    FinalTemp = finalTemp;
    this.M_k = M_k;
    CoolingSchedule = coolingSchedule;
    _gaantView = new GaantView(world, constraints);
    Random = random ?? new Random();
  }

  public override List<EmergencyServicePlan> GetBest(ReadOnlySpan<Incident> incidents)
  {
    using StreamWriter writer = new("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/log.txt");

    EmergencyServicePlan current = EmergencyServicePlan.GetNewFrom(World, StartPlan);
    double currentEval = UtilityFunction.Evaluate(current, incidents);

    EmergencyServicePlan best = EmergencyServicePlan.GetNewFrom(World, current);
    double bestEval = currentEval;

    double temp = StartTemp;

    while (temp > FinalTemp)
    {
      ++Iteration;
      Console.WriteLine($"{Iteration}: {temp}");

      for (int m = 0; m < M_k; ++m)
      {
        MoveSequenceDuo move = RandomMoveSampler.Sample(current);
        _moveMaker.ModifyMakeMove(current, move.Normal);

        double neighbourEval = UtilityFunction.Evaluate(current, incidents);
        double delta = currentEval - neighbourEval;
        if (delta > 0)
        {
          double probabilityToNotAccept = 1 - Math.Exp(-delta / temp);
          if (Random.NextDouble() < probabilityToNotAccept)
          {
            _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
            continue;
          }
        }

        currentEval = neighbourEval;
        if (currentEval > bestEval)
        {
          _gaantView.Show(current, incidents, writer);
          best.FillFrom(current);
          bestEval = currentEval;
        }
      }

      temp = CoolingSchedule.Calculate(temp);
    }

    return [best];
  }
}


