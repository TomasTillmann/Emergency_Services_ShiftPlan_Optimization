using System.Collections.Immutable;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

/// K nicemu nejspis
public class FindMaxSuccessRateShiftPlan
{
  private readonly ShiftTimes _shiftTimes;
  private readonly Simulation _simulation;
  private readonly Constraints _constraints;
  private readonly int _max;

  private readonly List<Weights> _optimals = new();

  private ImmutableArray<Incident> incidents;
  private double _globalBestSuccessRate;
  private Weights _globalBestWeights;

  public FindMaxSuccessRateShiftPlan(World world, ShiftTimes shiftTimes, Constraints constraints)
  {
    _shiftTimes = shiftTimes;
    _simulation = new Simulation(world);
    _constraints = constraints;
    _max = shiftTimes.MaxDurationSec;
  }

  public IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    this.incidents = incidents;

    TryAllStartingTimesForDepot(depotIndex: 0, availableMedicTeamIndex: 0, availableAmbulanceIndex: 0);

    if (_optimals.Count == 0)
    {
      _optimals.Add(_globalBestWeights);
    }

    return _optimals;
  }

  private int medicTeamsOnDepotCount = 0;
  private void TryAllStartingTimesForDepot(int depotIndex, int availableMedicTeamIndex, int availableAmbulanceIndex)
  {
    if (availableMedicTeamIndex == _constraints.AvailableMedicTeamsCount - 1)
    {
      _simulation.Run(incidents.AsSpan());

      if (_simulation.SuccessRate == 1)
      {
        _optimals.Add(Weights.GetFrom(_simulation.Plan, _constraints.MaxMedicTeamsOnDepotCount));
      }
      else if (_simulation.SuccessRate > _globalBestSuccessRate)
      {
        _globalBestSuccessRate = _simulation.SuccessRate;
        _globalBestWeights = Weights.GetFrom(_simulation.Plan, _constraints.MaxMedicTeamsOnDepotCount);
      }

      return;
    }

    ++_simulation.Plan.AllocatedMedicTeamsCount;
    ++_simulation.Plan.AllocatedAmbulancesCount;

    _simulation.Plan.AvailableMedicTeams[availableMedicTeamIndex].Depot = _simulation.World.Depots[depotIndex];
    _simulation.Plan.AvailableMedicTeams[availableMedicTeamIndex].Depot.Ambulances.Add(_simulation.Plan.AvailableAmbulances[availableAmbulanceIndex]);

    for (int startSecIndex = 0; startSecIndex < _shiftTimes.AllowedStartingTimesSecSorted.Length; ++startSecIndex)
    {
      _simulation.Plan.AvailableMedicTeams[availableMedicTeamIndex].Shift = Interval.GetByStartAndDuration(_shiftTimes.AllowedStartingTimesSecSorted[startSecIndex], _max);

      if (medicTeamsOnDepotCount == _constraints.MaxMedicTeamsOnDepotCount)
      {
        medicTeamsOnDepotCount = 1;
        TryAllStartingTimesForDepot(depotIndex + 1, availableMedicTeamIndex + 1, availableAmbulanceIndex + 1);
      }
      else
      {
        ++medicTeamsOnDepotCount;
        TryAllStartingTimesForDepot(depotIndex, availableMedicTeamIndex + 1, availableAmbulanceIndex + 1);
      }
      --medicTeamsOnDepotCount;
    }

    _simulation.Plan.AvailableMedicTeams[availableMedicTeamIndex].Depot.Ambulances.Clear();
    _simulation.Plan.AvailableMedicTeams[availableMedicTeamIndex].Depot = null;

    --_simulation.Plan.AllocatedMedicTeamsCount;
    --_simulation.Plan.AllocatedAmbulancesCount;
  }
}
