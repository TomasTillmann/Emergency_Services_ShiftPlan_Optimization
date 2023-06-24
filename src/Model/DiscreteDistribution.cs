namespace Optimizing;

public class DiscreteDistribution
{
    private double[] distribution;
    private Random random;

    public DiscreteDistribution(Random? random = null)
    {
        this.random = random ?? new Random();
        distribution = new double[0];
    }

    public DiscreteDistribution BasedOn(double[] distribution)
    {
        this.distribution = distribution;
        return this;
    }

    public T Sample<T>(T[] data)
    {
        double r = random.NextDouble();

        for(int i = 0; i < distribution.Length - 1; i++)
        {
            if(r < distribution[i] && r >= distribution[i + 1])
            {
                return data[i];
            }
        }

        return data[distribution.Length - 1];
    }
}
