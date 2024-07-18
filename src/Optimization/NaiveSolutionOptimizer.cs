using System.Collections.Immutable;
using System.Diagnostics;
using DataModel.Interfaces;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public class NaiveSolutionOptimizer : OptimizerBase
{
    private readonly Random _random;
    public ShiftTimes ShiftTimes { get; }
    public int SamplePlans { get; }
    
    public NaiveSolutionOptimizer(World world, Constraints constraints, IUtilityFunction utilityFunction, ShiftTimes shiftTimes, int samplePlans, Random? random = null)
    : base(world, constraints, utilityFunction)
    {
        if (SamplePlans < 0) throw new ArgumentException();
        
        _random = random ?? new();
        SamplePlans = samplePlans;
        ShiftTimes = shiftTimes;
    }

    public override List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
    {
        var sampler = new PlanSamplerUniform(World, ShiftTimes, Constraints, _random.NextDouble(), _random);
        EmergencyServicePlan best = sampler.Sample();
        double bestEval = double.MinValue;
        Stopwatch sw = Stopwatch.StartNew();
        
        for (int i = 0; i < SamplePlans; ++i)
        {
            sampler = new PlanSamplerUniform(World, ShiftTimes, Constraints, _random.NextDouble(), _random);
            var plan = sampler.Sample();
            var eval = UtilityFunction.Evaluate(plan, incidents.AsSpan());
            if (eval > bestEval)
            {
                Console.WriteLine($"UPDATE: elapsed: {sw.Elapsed.TotalSeconds}, handled: {UtilityFunction.HandledIncidentsCount}, cost: {plan.Cost}");
                best = plan;
                bestEval = eval;
            }
        }

        return [best];
    }
}