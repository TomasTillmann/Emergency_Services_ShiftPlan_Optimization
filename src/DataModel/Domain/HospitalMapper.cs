namespace ESSP.DataModel;

public class HospitalMapper
{
  private readonly CoordinateMapper _coordinateMapper = new();

  public HospitalModel Map(Hospital hospital)
  {
    return new HospitalModel
    {
      Location = _coordinateMapper.Map(hospital.Location)
    };
  }

  public Hospital MapBack(HospitalModel model)
  {
    return new Hospital
    {
      Location = _coordinateMapper.MapBack(model.Location)
    };
  }
}


