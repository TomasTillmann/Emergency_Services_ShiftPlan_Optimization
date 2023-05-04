using DataModel.Interfaces;

namespace ESSP.DataModel;
public class Incident : ILocatable
{
    public Coordinate Location { get; init; }

    public Seconds Occurence { get; init; }

    public Seconds OnSceneDuration { get; init; }

    public Seconds InHospitalDelivery { get; init; }

    public IncidentType Type { get; init; }

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