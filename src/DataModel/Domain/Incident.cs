using DataModel.Interfaces;

namespace ESSP.DataModel;
public class Incident : ILocatable
{
    public Coordinate Location { get; }

    public Seconds Occurence { get; }

    public Seconds OnSceneDuration { get; }

    public Seconds InHospitalDelivery { get; }

    public IncidentType Type { get; }

    public Incident(Coordinate coordinate, Seconds occurence, Seconds onSceneDuration, Seconds inHospitalDelivery, IncidentType type)
    {
        Location = coordinate;
        Occurence = occurence;
        OnSceneDuration = onSceneDuration;
        InHospitalDelivery = inHospitalDelivery;
        Type = type;
    }

    public override string ToString()
    {
        return $"Location: {Location}, Occurence: {Occurence}";
    }
}