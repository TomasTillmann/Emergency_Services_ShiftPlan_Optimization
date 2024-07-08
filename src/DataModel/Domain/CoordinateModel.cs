namespace ESSP.DataModel;

public record CoordinateModel
{
  public double Latitude { get; set; }
  public double Longitude { get; set; }
  
  public CoordinateModel(double latitude, double longitude)
  {
    Latitude = latitude;
    Longitude = longitude;
  }
  
  public CoordinateModel() {}
}


