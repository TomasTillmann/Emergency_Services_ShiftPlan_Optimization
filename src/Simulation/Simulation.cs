using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataModel.Interfaces;
using ESSP.DataModel;
using Logging;
using Model.Extensions;

namespace Simulating;

public sealed class Statistics
{
    public IList<Incident> UnhandledIncidents { get; internal set; } = new List<Incident>();
    public IList<Incident> HandledIncidents { get; internal set; } = new List<Incident>();

    public IReadOnlyCollection<Incident> AllIncidents { get; }

    public double Threshold => (double)HandledIncidents.Count / AllIncidents.Count;

    internal Statistics(IReadOnlyCollection<Incident> allIncidents)
    {
        AllIncidents = allIncidents;
    }

    private Statistics() { }

    internal void SetUnhandled(Incident incident)
    {
        UnhandledIncidents.Add(incident);
    }

    internal void SetHandled(Incident incident)
    {
        HandledIncidents.Add(incident);
    }
}

internal class State
{
    public Seconds CurrentTime { get; set; } = 0.ToSeconds();
    public Seconds StepDuration { get; set; }
}

public sealed class Simulation
{
    public IReadOnlyList<Depot> Depots { get; }
    public Seconds Time => state.CurrentTime;
    public IDistanceCalculator DistanceCalculator { get; }

    private State state;
    private Statistics statistics;
    private ShiftPlan shiftPlan;
    private ShiftEvaluator shiftEvaluator;
    private PlannableIncident.Factory plannableIncidentFactory;

    private Logger Logger = Logger.Instance;

    public Simulation(World world, IDistanceCalculator distanceCalculator)
    {
        Depots = world.Depots;
        DistanceCalculator = distanceCalculator;
        plannableIncidentFactory = new PlannableIncident.Factory(distanceCalculator, world.Hospitals);
        shiftEvaluator = new ShiftEvaluator(plannableIncidentFactory);
    }

    /// <summary>
    /// Incidents need to be sorted by occurence.
    /// </summary>
    public Statistics Run(IReadOnlyCollection<Incident> incidents, ShiftPlan shiftPlan)
    {
        Initialization(shiftPlan, incidents);

        foreach (Incident currentIncident in incidents)
        {
            UpdateSystem(currentIncident);
            Step(currentIncident);
            Logger.WriteLine();
        }

        Logger.WriteLine($"Threshold: {statistics.Threshold}");

        return statistics;
    }

    private void Initialization(ShiftPlan shiftPlan, IReadOnlyCollection<Incident> allIncidents)
    {
        statistics = new Statistics(allIncidents);
        state = new State();
        this.shiftPlan = shiftPlan;
    }

    private void UpdateSystem(Incident incident)
    {
        UpdateState(incident);
    }

    private void UpdateState(Incident incident)
    {
        Seconds lastTime = state.CurrentTime;

        state.CurrentTime = incident.Occurence;
        state.StepDuration = state.CurrentTime - lastTime;
    }

    private void Step(Incident currentIncident)
    {
        Logger.WriteLine($"Occurence: {state.CurrentTime}");
        Logger.WriteLine($"Incident: {currentIncident}");
        Logger.WriteLine($"Shifts:\n{shiftPlan.Shifts.Visualize("\n")}");

        List<Shift> handlingShifts = GetHandlingShifts(currentIncident);
        if (handlingShifts.Count == 0)
        {
            Logger.WriteLine($"Unhandled");
            statistics.SetUnhandled(currentIncident);
            return;
        }

        Shift bestShift = GetBestShift(handlingShifts, currentIncident);

        Logger.WriteLine($"Best shift:\n{bestShift}");

        bestShift.Plan(plannableIncidentFactory.Get(currentIncident, bestShift, state.CurrentTime));

        statistics.SetHandled(currentIncident);
    }

    private List<Shift> GetHandlingShifts(Incident currentIncident)
    {
        List<Shift> handlingShifts = new();
        foreach (Shift shift in shiftPlan.Shifts)
        {
            if (shiftEvaluator.IsHandling(shift, currentIncident, state.CurrentTime))
            {
                handlingShifts.Add(shift);
            }
        }

        return handlingShifts;
    }

    private Shift GetBestShift(List<Shift> handlingShifts, Incident currentIncident)
    {
        Shift bestShift = handlingShifts.First();
        foreach (Shift shift in handlingShifts)
        {
            bestShift = shiftEvaluator.GetBetter(shift, bestShift, currentIncident, state.CurrentTime);
        }

        return bestShift;
    }
}
