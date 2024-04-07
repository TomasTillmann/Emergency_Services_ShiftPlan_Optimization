using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public interface IOptimizer : IDisposable
{
  public Constraints Constraints { get; }

  public World World { get; }

  /// <summary>
  /// <see cref="Shift.Work"/> Interval of each <see cref="Shift"/>.
  /// Initialized at the construction time of the optimizer in <see cref="Optimizer"/> by <see cref="Optimizer.InitWeights"/>.
  /// You can set to any value you would like and the optimizer will start optimizing from it.
  /// </summary>
  Weights StartWeights { get; set; }

  /// <summary>
  /// Loss function which is tried to be minimezed by the <see cref="Optimizer"/>
  /// </summary>
  public ILoss Loss { get; set; }

  /// <summary>
  /// Tries to find the most optimal shift plans from starting <see cref="StartWeights"/>.
  /// You can initialize <see cref="StartWeights"/> before calling <see cref="FindOptimal(ImmutableArray{SuccessRatedIncidents})"/>.
  /// <see cref="StartWeights"/> are initialized in <see cref="Optimizer"/> constructor by <see cref="Optimizer.InitWeights"/>.
  /// Note, that more <see cref="ShiftPlan"/> can have the same, most optimal loss, that's why enumeration is returned.
  /// The loss is calculated by provided <see cref="ILoss"/> implementation.
  /// </summary>
  IEnumerable<Weights> FindOptimal(ImmutableArray<SuccessRatedIncidents> incidentsSet);
}

