using ESSP.DataModel;

namespace Optimizing;

public interface IRandomMoveSampler
{
  MoveSequenceDuo Sample(EmergencyServicePlan plan);
}




