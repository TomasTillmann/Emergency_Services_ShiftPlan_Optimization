using ESSP.DataModel;
using Optimizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSP_Tests;

public class TabuSearchOptimizerTests : Tests
{
    [Test]
    public void EmptyShiftPlanFitness()
    {
        ShiftPlan empty = ShiftPlan.ConstructEmpty(testDataProvider.GetDepots());

        var tabu = new TabuSearchOptimizer(world, testDataProvider.GetConstraints(), 100, 10);
        var incidents = testDataProvider.GetIncidents(10, 23.ToHours());

        var eval = tabu.Fitness(empty, new List<SuccessRatedIncidents> { incidents });
        Console.WriteLine(eval);
    }
}
