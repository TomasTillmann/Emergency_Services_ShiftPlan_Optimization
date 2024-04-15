using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public abstract class Loss : ILoss
{
  public ISimulation Simulation { get; }

  public Loss(Simulation simulation)
  {
    Simulation = simulation;
  }

  public abstract double Get(Weights weights, SuccessRatedIncidents incidentsSet);

  public void Map(Weights weights)
  {
    int medicTeamAllocation;
    int ambulanceAllocation;
    int medicTeamsOffset = 0;
    int ambulancesOffset = 0;

    for (int i = 0; i < Simulation.World.Depots.Length; ++i)
    {

      medicTeamAllocation = weights.MedicTeamAllocations[i];
      for (int j = 0; j < medicTeamAllocation; ++j)
      {
        Simulation.EmergencyServicePlan.MedicTeams[medicTeamsOffset + j].Shift = weights.MedicTeamShifts[medicTeamsOffset + j];
        Simulation.EmergencyServicePlan.MedicTeams[medicTeamsOffset + j].Depot = Simulation.World.Depots[i];
      }
      medicTeamsOffset += medicTeamAllocation;

      ambulanceAllocation = weights.AmbulancesAllocations[i];
      Simulation.World.Depots[i].Ambulances.Clear();
      for (int j = 0; j < ambulanceAllocation; ++j)
      {
        Simulation.World.Depots[i].Ambulances.Add(Simulation.EmergencyServicePlan.Ambulances[ambulancesOffset + j]);
      }
      ambulancesOffset += ambulanceAllocation;
    }

    Simulation.EmergencyServicePlan.AllocatedTeamsCount = weights.AllocatedTeamsCount;
    Simulation.EmergencyServicePlan.AllocatedAmbulancesCount = weights.AllocatedAmbulancesCount;
  }

  public double GetEmergencyServicePlanCost(Weights weights)
  {
    Map(weights);
    return Simulation.EmergencyServicePlan.GetShiftDurationsSum();
  }

  protected void RunSimulation(Weights weights, SuccessRatedIncidents incidents)
  {
    Map(weights);
    Simulation.Run(incidents.Value);
  }
}

