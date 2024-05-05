using System.Collections.Immutable;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public class SuccessRateLoss : Loss
{
  public SuccessRateLoss(Simulation simulation, ShiftTimes shiftTimes)
  : base(simulation, shiftTimes) { }

  public override double GetLoss()
  {
    return -Simulation.SuccessRate;
  }
}
