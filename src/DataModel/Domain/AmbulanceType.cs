namespace ESSP.DataModel;
public readonly struct AmbulanceType
{
    public string Name { get; }
    public double Cost { get; }

    public AmbulanceType(string name, double cost)
    {
        Name = name;
        Cost = cost;
    }

    public override string ToString()
    {
        return $"Name: {Name}"; 
    }
}