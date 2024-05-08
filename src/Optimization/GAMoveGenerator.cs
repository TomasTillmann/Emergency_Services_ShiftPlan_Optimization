using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public class MoveGenerator : MoveOptimizer
{
  public MoveGenerator(World world, Constraints constraints, ShiftTimes shiftTimes, IObjectiveFunction loss, Random? random = null)
  : base(world, constraints, shiftTimes, loss, random) { }

  public void ModifyMakeMove(Weights weights, Move move)
  {
    base.ModifyMakeMove(weights, move);
  }

  public void ModifyUnmakeMove(Weights weights, Move move)
  {
    base.ModifyUnmakeMove(weights, move);
  }

  // TODO: refactor move generator from move optimizer, so this method will not have to me not implemented like this
  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    throw new NotImplementedException();
  }
}
