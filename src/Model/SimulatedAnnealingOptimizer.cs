﻿using ESSP.DataModel;
using Logging;
using Model.Extensions;
using Optimizing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization;

public class SimulatedAnnealingOptimizer : MetaheuristicOptimizer
{
    #region Parameters
    public readonly double LowestTemperature;
    public readonly double HighestTemperature;
    public readonly double TemperatureReductionFactor;
    public readonly int NeighboursLimit;
    #endregion

    public readonly Random Random;

    private readonly int maxShiftPlanCost;

    public SimulatedAnnealingOptimizer(World world, Domain constraints, double lowestTemperature, double highestTemperature, double temperatureReductionFactor, int neighbourLimit, Random? random = null) : base(world, constraints)
    {
        Random = random ?? new Random();
        LowestTemperature = lowestTemperature;
        HighestTemperature = highestTemperature;
        TemperatureReductionFactor = temperatureReductionFactor;
        NeighboursLimit = neighbourLimit;

        ShiftPlan maximalShiftPlan = ShiftPlan.ConstructFrom(world.Depots, 0.ToSeconds(), constraints.AllowedShiftDurations.Max());
        maxShiftPlanCost = maximalShiftPlan.GetCost();

    }

    public override IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets)
    {
        int Fitness(ShiftPlan shiftPlan)
        {
            return this.Fitness(shiftPlan, incidentsSets);
        }

        ShiftPlan initShiftPlan
            = ShiftPlan.ConstructRandom(world.Depots, Constraints.AllowedShiftStartingTimes.ToList(),
            Constraints.AllowedShiftDurations.ToList(), Random);

        ShiftPlan globalBest = initShiftPlan;
        int globalBestFitness = Fitness(globalBest);

        Move? currentBestMove = null;
        ShiftPlan currentBest = initShiftPlan;
        int currentBestFitness = globalBestFitness;


        Stopwatch sw = new Stopwatch();
        for(double currentTemperature = HighestTemperature; currentTemperature > LowestTemperature; currentTemperature *= TemperatureReductionFactor)
        {
            sw.Start();

            List<Move> moves = GetNeighborhoodMoves(currentBest).ToList();
            if (moves.Count > NeighboursLimit)
            {
                moves = moves.GetRandomSamples(NeighboursLimit, Random);
            }

            foreach (Move move in moves)
            {
                ModifyMakeMove(currentBest, move);
                int neighbourFitness = Fitness(currentBest);

                if(neighbourFitness < currentBestFitness)
                {
                    currentBestMove = move; 
                    currentBestFitness = neighbourFitness;

                    if(currentBestFitness < globalBestFitness)
                    {
                        globalBest = currentBest.Copy();
                        globalBestFitness = currentBestFitness;
                    }
                }
                else if(Accept(currentBestFitness - neighbourFitness, currentTemperature))
                {
                    currentBestMove = move; 
                    currentBestFitness = neighbourFitness;
                }

                ModifyUnmakeMove(currentBest, move);
            }

            if(currentBestMove is null || currentBest is null)
            {
                throw new ArgumentException("All neighbours either have worse fitness and even none was accepted, leading to no move being selected.");
            }

            currentBest = ModifyMakeMove(currentBest.Copy(), currentBestMove);

            Logger.Instance.WriteLineForce($"Temperature: {currentTemperature} / {LowestTemperature}");
            Logger.Instance.WriteLineForce($"One step took: {sw.ElapsedMilliseconds}ms"); sw.Reset();
            Logger.Instance.WriteLineForce($"Global best: {globalBestFitness} ({globalBest})");
            Logger.Instance.WriteLineForce($"Current best: {currentBestFitness} ({currentBest})");
            Logger.Instance.WriteLineForce();
        }

        return new List<ShiftPlan> { globalBest };
    }
    public override int Fitness(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents)
    {
        int eval = base.Fitness(shiftPlan, successRatedIncidents, out double meanSuccessRate);

        // damping, to better navigate
        if (eval == int.MaxValue)
        {
            return maxShiftPlanCost + (int)((1 - meanSuccessRate) * 100);
            //return int.MaxValue;
        }

        return eval;
    }

    public bool Accept(double difference, double temperature)
    {
        const double boltzmanConstant = 1.00000000000000000000000380649;
        double probability = Math.Exp(-difference / (boltzmanConstant * temperature));
        double random = Random.Next(0, 100) / 100d;

        return random < probability;
    }
}