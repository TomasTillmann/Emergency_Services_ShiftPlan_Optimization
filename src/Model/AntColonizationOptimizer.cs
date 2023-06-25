using ESSP.DataModel;
using Logging;
using Model.Extensions;
using Optimizing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Optimization;

public readonly struct Index
{
    public readonly int I;
    public readonly int J;

    public Index(int i, int j)
    {
        I = i;
        J = j;
    }

    public override string ToString()
    {
        return $"({I}, {J})"; 
    }
}

public class Ant
{
    public Index Location;
    public Index[] VisitedLocations;
    public Seconds Duration { get; set; } = 0.ToSeconds();
    public int Fitness { get; set; }

    private int[] CoveragePerHour;

    public Ant(Index initialLocation, int partitionsCount, Hours simulationDuration)
    {
        Location = initialLocation;
        VisitedLocations = new Index[partitionsCount];

        CoveragePerHour = new int[simulationDuration.Value];
        ResetCoverage();
    }

    public float CoverageGain(Interval interval)
    {
        if(interval.Duration == 0.ToSeconds())
        {
            return 0f;
        }

        int startHour = interval.Start.Value / 60 / 60;
        int endHour = Math.Min(CoveragePerHour.Length, startHour + interval.Duration.Value / 60 / 60);

        float coverageSum = 0;
        for (int i = startHour; i < endHour; i++)
        {
            coverageSum += 1f / CoveragePerHour[i];
        }

        return coverageSum / (endHour - startHour); 
    }

    public void AddToCoverage(Interval interval)
    {
        int startHour = interval.Start.Value / 60 / 60;
        int endHour = Math.Min(CoveragePerHour.Length, startHour + interval.Duration.Value / 60 / 60);

        for (int i = startHour; i < endHour; i++)
        {
            CoveragePerHour[i] += 1;
        }
    }

    public void ResetCoverage()
    {
        for(int i = 0; i < CoveragePerHour.Length; i++)
        {
            CoveragePerHour[i] = 1;
        }
    }
}

public class Component
{
    public float Pheromones { get; set; }

    public Component(float initialPheronomes)
    {
        Pheromones = initialPheronomes;
    }

    public override string ToString()
    {
        return $"{Pheromones}"; 
    }
}

public class ComponentGraph
{
    private Component[,] components;

    public readonly int Partitions;
    public readonly int Intervals;

    public ComponentGraph(int shifts, int intervals)
    {
        components = new Component[shifts, intervals];
        this.Partitions = shifts;
        this.Intervals = intervals;
    }

    public Component this[Index index] { get => components[index.I, index.J]; set => components[index.I, index.J] = value; }
    public Component this[int i, int j] { get => components[i, j]; set => components[i, j] = value; }

    public Index[] GetNeighbours(Index index)
    {
        Index[] neighbours = new Index[Intervals];
        for (int j = 0; j < Intervals; j++)
        {
            neighbours[j] = new Index(index.I + 1, j);
        }

        return neighbours;
    }

    public IEnumerable<Component> All()
    {
        foreach(Component component in components)
        {
            yield return component;
        }
    }
}

public class AntColonizationOptimizer : MetaheuristicOptimizer
{

    #region Params
    public readonly int Iterations;
    public readonly int Permutations;
    public readonly float InitialPheromone;
    public readonly float PheromoneEvaporationRate;
    public readonly float Alpha;
    public readonly float Beta;
    public readonly Hours SimulationDuration;
    public readonly int EstimatedMinimalShiftPlanDuration;
    public readonly int EstimatedMaximalShiftPlanDuration;
    public readonly Random Random;
    #endregion

    public int EstimatedMaxFitness { get; private set; } 
    public int EstimatedMinFitness { get; private set; }

    protected ComponentGraph Graph { get; private set; } = null!;

    protected Interval[] Intervals = null!;

    protected float[] PreferShorterIntervalsCosts = null!;

    private int[] permutationsTempIndeces;

    public AntColonizationOptimizer(World world, Domain constraints, int iterations, int permutations, float initialPheromone, float pheromoneEvaporationRate, float alpha, float beta, Hours simulationDuration, Seconds estimatedMinimalShiftPlanDuration, Seconds estimatedMaximalShiftPlanDuration, Random? random = null) : base(world, constraints)
    {
        if(estimatedMaximalShiftPlanDuration < estimatedMinimalShiftPlanDuration)
        {
            throw new ArgumentException("Minimal must be greater than or at least equal to.");
        }

        if(initialPheromone < 0 || initialPheromone > 1)
        {
            throw new ArgumentException("Pheromone has to be between 0 and 1.");
        }


        Iterations = iterations;
        Permutations = permutations;
        InitialPheromone = initialPheromone;
        PheromoneEvaporationRate = pheromoneEvaporationRate;
        Alpha = alpha;
        Beta = beta;
        SimulationDuration = simulationDuration;
        EstimatedMinimalShiftPlanDuration = estimatedMinimalShiftPlanDuration.Value;
        EstimatedMaximalShiftPlanDuration = estimatedMaximalShiftPlanDuration.Value;
        Random = random ?? new Random();

        permutationsTempIndeces = new int[Permutations];

        InitIntervals();
        InitPreferShorterIntervals();
        InitGraph();
        InitEstimatedMinMaxFintess(Graph.Partitions);

        if(Permutations > Graph.Partitions)
        {
            throw new ArgumentException("Permutations have to be less than or equal to number of shifts.");
        }
    }

    private void InitIntervals()
    {
        List<Interval> intervals = new();
        foreach (Seconds startingTime in Constraints.AllowedShiftStartingTimes)
        {
            foreach (Seconds duration in Constraints.AllowedShiftDurations)
            {
                intervals.Add(Interval.GetByStartAndDuration(startingTime, duration));
            }
        }

        Intervals = intervals.ToArray();
    }

    public void InitPreferShorterIntervals()
    {
        PreferShorterIntervalsCosts = new float[Intervals.Length];
        float total = Intervals.Sum(interval => interval.Duration.Value);
        for (int i = 0; i < Intervals.Length; i++)
        {
            PreferShorterIntervalsCosts[i] = 1 - Intervals[i].Duration.Value / total;
        }
    }

    public void InitGraph()
    {
        Graph = new ComponentGraph(World.Depots.Select(depot => depot.Ambulances.Count).Sum(), Intervals.Length);
        for (int partition = 0; partition < Graph.Partitions; partition++)
        {
            for (int interval = 0; interval < Graph.Intervals; interval++)
            {
                Graph[partition, interval] = new Component(InitialPheromone);
            }
        }
    }

    public void InitEstimatedMinMaxFintess(int shiftCount)
    {
        EstimatedMinFitness = Intervals.Min(interval => interval.Duration.Value) * shiftCount;

        // +100 is specifically chosen because this is the fitness function
        EstimatedMaxFitness = MaxShiftPlanCost + 100; 
    }

    public override IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets)
    {
        Ant[] ants = InitializeAnts();
        Index[] globalBest = new Index[Graph.Partitions];
        int globalBestFitness = int.MaxValue;

        Stopwatch sw = new Stopwatch();
        for(int currentTime = 0; currentTime < Iterations; currentTime++)
        {
            sw.Start();
            ConstructAntsSolutions(ants);
            UpdateBestAndAssignAntsFitness(ref globalBest, ref globalBestFitness, ants, incidentsSets);
            UpdatePheromones(ants);

            //LogInfo(ants, globalBest, sw, incidentsSets);
            //Logger.Instance.WriteLineForce($"one iter took: {sw.Elapsed}"); sw.Reset();
            ResetAnts(ants);
            DaemonActions();
        }

        return new List<ShiftPlan> { ConvertSolution(globalBest) };
    }

    protected void ConstructAntsSolutions(Ant[] ants)
    {
        double[] neighbourEvals = new double[Graph.Intervals];
        double totalEval;
        DiscreteDistribution distribution = new();

        for (int partition = 0; partition < Graph.Partitions - 1; partition++)
        {
            for(int antIndex = 0; antIndex < ants.Length; antIndex++)
            {
                Ant ant = ants[antIndex];
                totalEval = 0;
                Index[] neighbours = Graph.GetNeighbours(ant.Location);

                for(int i = 0; i < neighbours.Length; i++)
                {
                    double heuristic = Math.Pow(Heuristic(ant, neighbours[i]), Alpha);
                    double pheromones = Math.Pow(Pheromones(ant, neighbours[i]), Beta);
                    Logger.Instance.WriteLineForce($"ph read: {pheromones}, on component: {Graph[neighbours[i]].Pheromones}");
                    neighbourEvals[i] = heuristic * pheromones;

                    totalEval += neighbourEvals[i];
                }

                for(int i = 0; i < neighbours.Length; i++)
                {
                    neighbourEvals[i] = neighbourEvals[i] / totalEval;
                }

                Logger.Instance.WriteLineForce(neighbours.Select(i => Intervals[i.J]).Visualize(" | "));
                Logger.Instance.WriteLineForce(neighbourEvals.Visualize(" | "));


                //int randomIndex = Random.Next(0, neighbours.Length);
                //Index randomNeighbour = neighbours[randomIndex]; 
                Index randomNeighbour = distribution.BasedOn(neighbourEvals).Sample(neighbours);

                //Logger.Instance.WriteLineForce($"Selected: {randomNeighbour}");

                ant.Location = randomNeighbour;
                ant.AddToCoverage(Intervals[randomNeighbour.J]);
                ant.Duration += Intervals[randomNeighbour.J].Duration;
                ant.VisitedLocations[partition] = randomNeighbour;
            }
        }
    }

    protected void UpdateBestAndAssignAntsFitness(ref Index[] globalBest, ref int globalBestFitness, Ant[] ants, List<SuccessRatedIncidents> incidentsSets)
    {
        for(int i = 0; i < ants.Length; i++)
        {
            ants[i].Fitness = Fitness(ants[i].VisitedLocations, incidentsSets);
            if(ants[i].Fitness < globalBestFitness)
            {
                globalBest = ants[i].VisitedLocations;
                globalBestFitness = ants[i].Fitness;
            }
        }
    }

    private int Fitness(Index[] solution, List<SuccessRatedIncidents> incidentsSets)
    {
        ShiftPlan shiftPlan = ConvertSolution(solution);
        int fitness = DampedFitness(shiftPlan, incidentsSets);
        return fitness;
    }

    private ShiftPlan ConvertSolution(Index[] solution)
    {
        // convert ant solution to shift plan
        ShiftPlan shiftPlan = ShiftPlan.ConstructEmpty(World.Depots);
        for(int i = 0; i < solution.Length; i++)
        {
            shiftPlan.Shifts[i].Work = Intervals[solution[i].J];
        }
        //

        return shiftPlan;
    }

    public float Heuristic(Ant ant, Index component)
    {
        const float epsilon = 0.001f;

        float coverageGain = 1f * (ant.CoverageGain(Intervals[component.J]) + epsilon);

        float minimalCost = 0.5f * (PreferShorterIntervalsCosts[component.J] + epsilon);

        float inZone = Math.Abs((EstimatedMaximalShiftPlanDuration + EstimatedMinimalShiftPlanDuration) / 2f - ant.Duration.Value)
            / (EstimatedMaximalShiftPlanDuration - EstimatedMinimalShiftPlanDuration) / 2f;
        inZone = 1f * (inZone + epsilon);

        float res = (coverageGain + minimalCost + inZone) / 3;

        //float res = (coverageGain + minimalCost) / 2;

        //float res = coverageGain;

        //float res = (float)Random.NextDouble(); 

        return res;
    }

    public float Pheromones(Ant ant, Index component)
    {
        return Graph[component].Pheromones;
    }

    protected void UpdatePheromones(Ant[] ants)
    {
        for(int i = 0; i < ants.Length; i++)
        {
            for (int j = 0; j < ants[i].VisitedLocations.Length; j++)
            {
                Component component = Graph[ants[i].VisitedLocations[j]];

                component.Pheromones = (1 - PheromoneEvaporationRate) * component.Pheromones;
                component.Pheromones += 1 - (ants[i].Fitness / (float)(EstimatedMaxFitness - EstimatedMinFitness)); 
            }
        }
    }

    protected void DaemonActions()
    {
        PermutatePartitions();
    }

    private void PermutatePartitions()
    {
        for(int i = 0; i < Permutations; i++)
        {
            permutationsTempIndeces[i] = Random.Next(0, Graph.Partitions);
        }

        Component temp;
        for(int i = 0; i < Permutations; i++)
        {
            int randomPartition = Random.Next(0, Graph.Partitions);
            for(int j = 0; j < Graph.Intervals; j++)
            {
                temp = Graph[i, j];
                Graph[i, j] = Graph[randomPartition, j];
                Graph[randomPartition, j] = temp;
            }
        }
    }

    protected void ResetAnts(Ant[] ants)
    {
        for(int j = 0; j < ants.Length; j++)
        {
            ants[j].Fitness = 0;
            ants[j].Duration = 0.ToSeconds();
            ants[j].Location = new Index(0, j);
            ants[j].ResetCoverage();
        }
    }

    private Ant[] InitializeAnts(int? count = null)
    {
        count = count ?? Graph.Intervals;
        Ant[] ants = new Ant[count.Value];

        for(int j = 0; j < count; j++)
        {
            ants[j] = new Ant(new Index(0, j), Graph.Partitions, SimulationDuration);
        }

        return ants;
    }

    private void LogInfo(Ant[] ants, Index[] globalBest, Stopwatch sw, List<SuccessRatedIncidents> incidentsSet)
    {
        Logger.Instance.WriteLineForce($"One step took: {sw.ElapsedMilliseconds}ms"); sw.Reset();
        Logger.Instance.WriteLineForce($"Global best: {Fitness(globalBest, incidentsSet)} ({globalBest.Visualize("| ")})");
        ants.ToList().ForEach(ant => Logger.Instance.WriteLineForce($"Visited:\n{ant.VisitedLocations.Visualize("| ")}"));
        for(int i = 0; i < Graph.Partitions; i++)
        {
            for(int j = 0; j < Graph.Intervals; j++)
            {
                Logger.Instance.WriteLineForce($"{i},{j}: {Graph[i,j]}");
            }
        }
    }
}
