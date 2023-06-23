using ESSP.DataModel;
using Optimization;
using System.Diagnostics.CodeAnalysis;

namespace Optimizing;

public abstract class MetaheuristicOptimizer : Optimizer
{

    private ShiftsTravel shiftTravel;

    protected MetaheuristicOptimizer(World world, Domain constraints) : base(world, constraints)
    {
        shiftTravel = new ShiftsTravel(constraints);
    }

    protected TShifts ModifyMakeMove<TShifts>(TShifts movable, Move move) where TShifts : IShifts
    {
        return shiftTravel.ModifyMakeMove(movable, move);
    }

    protected TShifts ModifyUnmakeMove<TShifts>(TShifts movable, Move move) where TShifts : IShifts
    {
        return shiftTravel.ModifyUnmakeMove(movable, move);
    }

    protected IEnumerable<Move> GetNeighborhoodMoves(IShifts movable)
    {
        return shiftTravel.GetNeighborhoodMoves(movable);
    }

    protected bool TryGenerateMove(Interval work, int shiftIndex, MoveType type, [NotNullWhen(true)] out Move? move)
    {
        return shiftTravel.TryGenerateMove(work, shiftIndex, type, out move);
    }
}
