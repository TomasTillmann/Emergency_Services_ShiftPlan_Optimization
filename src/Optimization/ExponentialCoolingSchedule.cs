namespace Optimizing;

public class ExponentialCoolingSchedule : ICoolingSchedule
{
  public double Rate { get; set; }

  public ExponentialCoolingSchedule(double rate)
  {
    Rate = rate;
  }

  public double Calculate(double currentTemp)
  {
    return Rate * currentTemp;
  }
}




