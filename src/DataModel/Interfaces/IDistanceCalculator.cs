using ESSP.DataModel;

namespace DataModel.Interfaces;

public interface IDistanceCalculator
{
    Hospital GetNearestHospital(Coordinate location);
    int GetTravelDurationSec(Coordinate from, Coordinate to);
    Coordinate GetIntermediateLocation(Coordinate from, Coordinate to, int durationDrivingSec);
}