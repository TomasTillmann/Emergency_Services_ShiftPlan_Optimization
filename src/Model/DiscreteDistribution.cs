namespace Optimizing;

public class DiscreteDistribution
{
    private (double Value, int Index)[] distribution;
    private Random random;

    public DiscreteDistribution(Random? random = null)
    {
        this.random = random ?? new Random();
        distribution = new (double, int)[0];
    }

    public DiscreteDistribution BasedOn(double[] distribution)
    {
        this.distribution = distribution.Select((value, i) => (value, i)).ToArray();
        Array.Sort(this.distribution, (p1, p2) => p1.Value.CompareTo(p2.Value));
        return this;
    }

    public T Sample<T>(T[] data)
    {
        double r = random.NextDouble() * distribution.Last().Value;

        for (int i = 0; i < distribution.Length - 1; i++)
        {
            if(distribution[i].Value < r && r <= distribution[i + 1].Value)
            {
                return data[distribution[i + 1].Index];
            }
        }

        return data[distribution.First().Index];
    }
}
