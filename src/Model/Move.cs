namespace Optimizing;

public class Move
{
    public int ShiftIndex { get; }
    public MoveType Type { get; }

    public Move(int shiftIndex, MoveType type)
    {
        ShiftIndex = shiftIndex;
        Type = type;
    }

    public override string ToString()
    {
        return $"({Type}, {ShiftIndex})";
    }
}
