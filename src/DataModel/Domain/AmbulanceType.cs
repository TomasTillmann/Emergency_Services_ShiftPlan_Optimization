namespace ESSP.DataModel;
public readonly struct AmbulanceType
{
    public string Name { get; init; }
    public int Cost { get; init; }

    public AmbulanceType(string name, int cost)
    {
        Name = name;
        Cost = cost;
    }

    public override string ToString()
    {
        return $"Name: {Name}"; 
    }
}