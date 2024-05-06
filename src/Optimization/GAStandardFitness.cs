using ESSP.DataModel;
using Simulating;

public class GAStandardFitness : ObjectiveFunction
{
  private readonly StandardLoss _loss;

  public GAStandardFitness(Simulation simulation, ShiftTimes shiftTimes)
  : base(simulation, shiftTimes)
  {
    _loss = new StandardLoss(simulation, shiftTimes);
  }

  public override double GetLoss()
  {
    return -1 * _loss.GetLoss() + 1;
  }
}

