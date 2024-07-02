namespace ESSP.DataModel;

using System;
using System.Collections.Immutable;
using Optimizing;
using Simulating;

public class IncidentsNormalizer
{
  private readonly Simulation _simulation;
  private readonly ShiftTimes _shiftTimes;

  public IncidentsNormalizer(World world, ShiftTimes shiftTimes)
  {
    _simulation = new Simulation(world, info: true);
    _shiftTimes = shiftTimes;
  }

  public int[] GetIncidentsHandlingHistorgram(ImmutableArray<Incident> incidents)
  {
    int[] incidentsHandledHistogram = new int[incidents.Length];
    int max = _shiftTimes.MaxDurationSec;
    ReadOnlySpan<Incident> incidentsSpan = incidents.AsSpan();

    _simulation.Plan.AllocatedMedicTeamsCount = 1;
    _simulation.Plan.AllocatedAmbulancesCount = 1;

    for (int depotIndex = 0; depotIndex < _simulation.World.Depots.Length; ++depotIndex)
    {
      for (int startSecIndex = 0; startSecIndex < _shiftTimes.AllowedStartingTimesSecSorted.Length; ++startSecIndex)
      {
        _simulation.Plan.AvailableMedicTeams[0].Depot = _simulation.World.Depots[depotIndex];
        _simulation.Plan.AvailableMedicTeams[0].Depot.Ambulances.Add(_simulation.Plan.AvailableAmbulances[0]);
        _simulation.Plan.AvailableMedicTeams[0].Shift = Interval.GetByStartAndDuration(_shiftTimes.AllowedStartingTimesSecSorted[startSecIndex], max);

        for (int incidentIndex = 0; incidentIndex < incidents.Length; ++incidentIndex)
        {
          _simulation.Run(incidentsSpan.Slice(incidentIndex, 1));

          if (!_simulation.UnhandledIncidents.Any())
          {
            ++incidentsHandledHistogram[incidentIndex];
          }
        }

        _simulation.Plan.AvailableMedicTeams[0].Depot.Ambulances.Clear();
        _simulation.Plan.AvailableMedicTeams[0].Depot = null;
      }
    }

    return incidentsHandledHistogram;
  }

  public ImmutableArray<Incident> Normalize(ImmutableArray<Incident> incidents)
  {
    int[] histogram = GetIncidentsHandlingHistorgram(incidents);
    List<Incident> normalizedIncidents = new();

    for (int i = 0; i < histogram.Length; ++i)
    {
      if (histogram[i] != 0)
      {
        normalizedIncidents.Add(incidents[i]);
      }
    }

    return normalizedIncidents.ToImmutableArray();
  }
}

