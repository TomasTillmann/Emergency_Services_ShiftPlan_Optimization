using DataModel.Interfaces;

namespace ESSP.DataModel;
public class Ambulance : ILocatable
{
    public AmbulanceType Type { get; }

    public Coordinate Location { get; set; }

    public Ambulance(AmbulanceType type)
    {
        Type = type;
    }
}