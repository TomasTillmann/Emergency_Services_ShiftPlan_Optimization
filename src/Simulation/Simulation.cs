using System;
using System.Collections.Generic;
using DataModel.Interfaces;
using ESSP.DataModel;
using Logging;
using Model.Extensions;

namespace Simulating;

public sealed class Statistics
{
    public IList<Incident> UnhandledIncidents { get; private init; } = new List<Incident>();

    public IList<Incident> HandledIncidents { get; private init; } = new List<Incident>();

    public IReadOnlyCollection<Incident> AllIncidents { get; init; }

    public double SuccessRate => 1 - (double)UnhandledIncidents.Count/AllIncidents.Count;

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

internal class SimulationState
{
    public Seconds CurrentTime { get; set; } = 0.ToSeconds();
    public Seconds StepDuration { get; set; }
}

public sealed class Simulation
{
    public IReadOnlyList<Depot> Depots { get; }
    public Seconds Time => state.CurrentTime;
    public IDistanceCalculator DistanceCalculator { get; }

    private SimulationState state;
    private Statistics statistics;
    private ShiftPlan shiftPlan;
    private ShiftEvaluator shiftEvaluator;
    private PlannableIncident.Factory plannableIncidentFactory;

    private Logger Logger = Logger.Instance;

    public Simulation(World world)
    {
        Depots = world.Depots;
        DistanceCalculator = world.DistanceCalculator;
        plannableIncidentFactory = new PlannableIncident.Factory(DistanceCalculator, world.Hospitals);
        shiftEvaluator = new ShiftEvaluator(plannableIncidentFactory);
    }

    public Statistics Run(List<Incident> incidents, ShiftPlan shiftPlan)
    {
        Initialization(shiftPlan, incidents);

        //int incident = 1;
        foreach (Incident currentIncident in incidents)
        {
            //Console.WriteLine($"Incident: {incident++} / {incidents.Count}");

            UpdateSystem(currentIncident);
            Step(currentIncident);

            //Console.WriteLine($"Success rate: {statistics.SuccessRate * 100}%");
            Logger.WriteLine();
            //Console.WriteLine();
        }

        Logger.WriteLine($"Success rate: {statistics.SuccessRate * 100}%");

        //Console.WriteLine();
        //Console.WriteLine($"Success rate: {statistics.SuccessRate * 100}%");

        return statistics;
    }

    private void Initialization(ShiftPlan shiftPlan, List<Incident> allIncidents)
    {
        allIncidents.Sort((x, y) => x.Occurence.CompareTo(y.Occurence));

        statistics = new Statistics(allIncidents);
        state = new SimulationState();
        this.shiftPlan = shiftPlan;
        shiftPlan.Shifts.ForEach(shift => shift.ClearPlannedIncidents());
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

    //TODO: Refactor, so only one for loop is necessary to decide the best shift.
    private void Step(Incident currentIncident)
    {
        Logger.WriteLine($"Incident: {currentIncident}");
        Logger.WriteLine($"Shifts:\n{shiftPlan.Shifts.Visualize("\n")}");

        List<Shift> handlingShifts = shiftEvaluator.GetHandlingShifts(shiftPlan.Shifts, currentIncident);
        if (handlingShifts.Count == 0)
        {
            Logger.WriteLine("Unhandled");
            //Console.WriteLine("Unhandled");
            statistics.SetUnhandled(currentIncident);
            return;
        }

        Shift bestShift = shiftEvaluator.GetBestShift(handlingShifts, currentIncident);

        Logger.WriteLine($"Best shift:\n{bestShift}");
        //Console.WriteLine($"Best shift:\n{bestShift}");

        bestShift.Plan(plannableIncidentFactory.Get(currentIncident, bestShift));

        statistics.SetHandled(currentIncident);
    }
}
