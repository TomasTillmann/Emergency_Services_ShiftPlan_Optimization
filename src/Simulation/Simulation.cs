using System;
using System.Collections.Generic;
using System.Linq;
using DataModel.Interfaces;
using ESSP.DataModel;
using Logging;
using Model.Extensions;

namespace Simulating;

public sealed class Statistics
{
    public IList<Incident> UnhandledIncidents { get; private set; } = new List<Incident>();

    public IReadOnlyCollection<Incident> AllIncidents { get; private set; }

    public double SuccessRate => 1 - (double)UnhandledIncidents.Count/AllIncidents.Count;

    internal Statistics(IReadOnlyCollection<Incident> allIncidents)
    {
        AllIncidents = allIncidents;
    }

    internal Statistics()
    {
        AllIncidents = new List<Incident>();
    }

    internal void SetUnhandled(Incident incident)
    {
        UnhandledIncidents.Add(incident);
    }

    internal void Reset(IReadOnlyCollection<Incident> allIncidents)
    {
        UnhandledIncidents.Clear();
        AllIncidents = allIncidents;
    }

    public override string ToString()
    {
        return $"SuccessRate: {SuccessRate}\n" +
            $"AllIncidents: Count: {AllIncidents.Count}, {AllIncidents.Visualize("| ")}\n" +
            $"UnhandledIncidents: Count: {UnhandledIncidents.Count}, {UnhandledIncidents.Visualize("| ")}\n";
    }
}

public sealed class Simulation
{
    public IReadOnlyList<Depot> Depots { get; }
    public IDistanceCalculator DistanceCalculator { get; }
    public Seconds CurrentTime { get; private set; } = 0.ToSeconds();

    private Statistics statistics;
    private ShiftPlan shiftPlan;
    private ShiftEvaluator shiftEvaluator;
    private PlannableIncident.Factory plannableIncidentFactory;

    public Simulation(World world)
    {
        Depots = world.Depots;
        DistanceCalculator = world.DistanceCalculator;
        statistics = new Statistics();

        plannableIncidentFactory = new PlannableIncident.Factory(DistanceCalculator, world.Hospitals);
        shiftEvaluator = new ShiftEvaluator(plannableIncidentFactory);
    }

    /// <summary>
    /// Runs the simulation for given <paramref name="shiftPlan"/> on given <paramref name="incidents"/>.
    /// Returns statistics, including success rate of given <paramref name="shiftPlan"/>.
    /// </summary>
    /// <param name="incidents">Have to be sorted in ascending order by occurence. It is not sorted nor checked internally for faster performance.</param>
    /// <param name="shiftPlan"></param>
    /// <returns></returns>
    public Statistics Run(IReadOnlyList<Incident> incidents, ShiftPlan shiftPlan)
    {
        Initialization(shiftPlan, incidents);

        foreach (Incident currentIncident in incidents)
        {
            UpdateSystem(currentIncident);
            Step(currentIncident);
        }

        return statistics;
    }

    private void Initialization(ShiftPlan shiftPlan, IReadOnlyList<Incident> allIncidents)
    {
        statistics.Reset(allIncidents);
        CurrentTime = 0.ToSeconds();

        this.shiftPlan = shiftPlan;
    }

    private void UpdateSystem(Incident incident)
    {
        CurrentTime = incident.Occurence;
    }

    private void Step(Incident currentIncident)
    {
        Shift bestShift = null; 
        foreach(Shift shift in shiftPlan.Shifts)
        {
            if (shiftEvaluator.IsHandling(shift, currentIncident))
            {
                if(bestShift is null)
                {
                    bestShift = shift;
                    continue;
                }

                bestShift = shiftEvaluator.GetBetter(bestShift, shift, currentIncident); 
            }
        }

        if (bestShift is null)
        {
            statistics.SetUnhandled(currentIncident);
            return;
        }

        bestShift.Plan(plannableIncidentFactory.Get(currentIncident, bestShift));
    }
}
