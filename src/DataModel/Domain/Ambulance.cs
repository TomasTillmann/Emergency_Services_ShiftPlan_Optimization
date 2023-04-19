using DataModel.Interfaces;

namespace ESSP.DataModel;
public class Ambulance : ILocatable
{
    public AmbulanceType Type { get; }
    public Seconds ReroutePenalty { get; }
    public Coordinate Location { get; set; }

    public Ambulance(AmbulanceType type, Coordinate location, Seconds reroutePenalty)
    {
        Type = type;
        Location = location;
        ReroutePenalty = reroutePenalty;
    }
}