using ESSP.DataModel;

namespace DataModel.Interfaces;

public interface ILocatable
{
    Coordinate Location { get; }
}