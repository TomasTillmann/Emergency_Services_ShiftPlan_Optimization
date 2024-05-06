using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public class GeneticAlgorithmOptimizer : Optimizer
{
  public int PopulationSize { get; set; }
  public int Iterations { get; set; }
  public double MutationP { get; set; }

  public GeneticAlgorithmOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, ILoss loss, int populationSize, int iterations, double mutationP, Random? random = null)
  : base(world, constraints, shiftTimes, loss, random)
  {
    PopulationSize = populationSize;
    Iterations = iterations;
    MutationP = mutationP;
  }

  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    Weights[] oldPopulation = new Weights[PopulationSize];
    Weights[] newPopulation = new Weights[PopulationSize];
    Span<double> fitness = stackalloc double[PopulationSize];

    PopulateRandomly(oldPopulation);
    PopulateRandomly(newPopulation);

    double fitnessSum;
    for (int iteration = 0; iteration < Iterations; ++iteration)
    {
      fitnessSum = 0;
      for (int i = 0; i < PopulationSize; ++i)
      {
        fitness[i] = -1 * Loss.Get(oldPopulation[i], incidents);
        fitnessSum += fitness[i];
      }

      Debug.WriteLine(string.Join(", ", fitness.ToArray().OrderByDescending(_ => _)));

      for (int i = 0; i < PopulationSize - 1; i += 2)
      {
        int parent1 = Select(fitness, fitnessSum);
        Debug.WriteLine($"Select parent1: {parent1}");
        Debug.WriteLine($"Select parent1 fitness: {fitness[parent1]}");
        int parent2 = Select(fitness, fitnessSum);
        Debug.WriteLine($"Select parent2 fitness: {fitness[parent2]}");

        Crossover(parent1, parent2, i, i + 1, oldPopulation, newPopulation, out bool isChild1Feasible, out bool isChild2Feasible);
        if (!isChild1Feasible || !isChild2Feasible)
        {
          Debug.WriteLine("Infeasible");
          continue;
        }

        Mutation(i, MutationP, newPopulation);
        Mutation(i + 1, MutationP, newPopulation);
      }

      Weights[] temp = oldPopulation;
      oldPopulation = newPopulation;
      newPopulation = temp;
    }

    return new List<Weights> { oldPopulation[fitness.ToArray().Select((_, Index) => (_, Index)).Max().Index] };
  }

  private void PopulateRandomly(Span<Weights> weights)
  {
    for (int i = 0; i < PopulationSize; ++i)
    {
      weights[i] = Weights.GetUniformlyRandom(World, Constraints, ShiftTimes, random: _random);
      // Debug.WriteLine(weights[i]);
    }
    // Debug.WriteLine("-------------------------");
  }

  // Wheel of fortune
  private int Select(Span<double> fitness, double fitnessSum)
  {
    double partSum = 0;
    int j = 0;
    double rand = _random.NextDouble() * fitnessSum;

    while (partSum < rand && j != PopulationSize - 1)
    {
      ++j;
      partSum += fitness[j];
    }

    return j;
  }

  private void Crossover(int parent1, int parent2, int child1, int child2, Weights[] oldPopulation, Weights[] newPopulation, out bool isChild1Feasible, out bool isChild2Feasible)
  {
    // Sacrificing DRY for best performance

    Debug.WriteLine("ENTRY CROSSOVER");

    int depotIndex = _random.Next(0, World.Depots.Length);
    Debug.WriteLine($"depotIndex: {depotIndex}");
    int onDepotIndex = _random.Next(0, Constraints.MaxMedicTeamsOnDepotCount);
    Debug.WriteLine($"onDepotIndex: {onDepotIndex}");

    Debug.WriteLine("parent 1");
    Debug.WriteLine(oldPopulation[parent1]);

    Debug.WriteLine("parent 2");
    Debug.WriteLine(oldPopulation[parent2]);

    newPopulation[child1].AllAllocatedMedicTeamsCount = 0;
    newPopulation[child2].AllAllocatedMedicTeamsCount = 0;
    newPopulation[child1].AllAllocatedAmbulancesCount = 0;
    newPopulation[child2].AllAllocatedAmbulancesCount = 0;

    for (int i = 0; i < depotIndex; ++i)
    {
      for (int j = 0; j < Constraints.MaxMedicTeamsOnDepotCount; ++j)
      {
        newPopulation[child1].MedicTeamAllocations[i, j] = oldPopulation[parent1].MedicTeamAllocations[i, j];
        newPopulation[child2].MedicTeamAllocations[i, j] = oldPopulation[parent2].MedicTeamAllocations[i, j];
      }

      newPopulation[child1].MedicTeamsPerDepotCount[i] = oldPopulation[parent1].MedicTeamsPerDepotCount[i];
      newPopulation[child1].AmbulancesPerDepotCount[i] = oldPopulation[parent1].AmbulancesPerDepotCount[i];
      newPopulation[child2].MedicTeamsPerDepotCount[i] = oldPopulation[parent2].MedicTeamsPerDepotCount[i];
      newPopulation[child2].AmbulancesPerDepotCount[i] = oldPopulation[parent2].AmbulancesPerDepotCount[i];

      newPopulation[child1].AllAllocatedMedicTeamsCount += oldPopulation[parent1].MedicTeamsPerDepotCount[i];
      newPopulation[child1].AllAllocatedAmbulancesCount += oldPopulation[parent1].AmbulancesPerDepotCount[i];
      newPopulation[child2].AllAllocatedMedicTeamsCount += oldPopulation[parent2].MedicTeamsPerDepotCount[i];
      newPopulation[child2].AllAllocatedAmbulancesCount += oldPopulation[parent2].AmbulancesPerDepotCount[i];
    }

    //Debug.WriteLine("prvni pulka child1");
    //Debug.WriteLine(newPopulation[child1]);

    newPopulation[child1].AmbulancesPerDepotCount[depotIndex] = oldPopulation[parent1].AmbulancesPerDepotCount[depotIndex];
    newPopulation[child1].AllAllocatedAmbulancesCount += oldPopulation[parent1].AmbulancesPerDepotCount[depotIndex];
    newPopulation[child1].MedicTeamsPerDepotCount[depotIndex] = 0;

    newPopulation[child2].AmbulancesPerDepotCount[depotIndex] = oldPopulation[parent2].AmbulancesPerDepotCount[depotIndex];
    newPopulation[child2].AllAllocatedAmbulancesCount += oldPopulation[parent2].AmbulancesPerDepotCount[depotIndex];
    newPopulation[child2].MedicTeamsPerDepotCount[depotIndex] = 0;

    for (int j = 0; j < onDepotIndex; ++j)
    {
      newPopulation[child1].MedicTeamAllocations[depotIndex, j] = oldPopulation[parent1].MedicTeamAllocations[depotIndex, j];
      if (newPopulation[child1].MedicTeamAllocations[depotIndex, j].DurationSec != 0) ++newPopulation[child1].MedicTeamsPerDepotCount[depotIndex];

      newPopulation[child2].MedicTeamAllocations[depotIndex, j] = oldPopulation[parent2].MedicTeamAllocations[depotIndex, j];
      if (newPopulation[child2].MedicTeamAllocations[depotIndex, j].DurationSec != 0) ++newPopulation[child2].MedicTeamsPerDepotCount[depotIndex];
    }

    for (int j = onDepotIndex; j < Constraints.MaxMedicTeamsOnDepotCount; ++j)
    {
      newPopulation[child1].MedicTeamAllocations[depotIndex, j] = oldPopulation[parent2].MedicTeamAllocations[depotIndex, j];
      if (newPopulation[child1].MedicTeamAllocations[depotIndex, j].DurationSec != 0) ++newPopulation[child1].MedicTeamsPerDepotCount[depotIndex];

      newPopulation[child2].MedicTeamAllocations[depotIndex, j] = oldPopulation[parent1].MedicTeamAllocations[depotIndex, j];
      if (newPopulation[child2].MedicTeamAllocations[depotIndex, j].DurationSec != 0) ++newPopulation[child2].MedicTeamsPerDepotCount[depotIndex];
    }

    newPopulation[child1].AllAllocatedMedicTeamsCount += newPopulation[child1].MedicTeamsPerDepotCount[depotIndex];
    newPopulation[child2].AllAllocatedMedicTeamsCount += newPopulation[child2].MedicTeamsPerDepotCount[depotIndex];

    //Debug.WriteLine("depotIndex child1");
    //Debug.WriteLine(newPopulation[child1]);

    for (int i = depotIndex + 1; i < World.Depots.Length; ++i)
    {
      for (int j = 0; j < Constraints.MaxMedicTeamsOnDepotCount; ++j)
      {
        newPopulation[child1].MedicTeamAllocations[i, j] = oldPopulation[parent2].MedicTeamAllocations[i, j];
        newPopulation[child2].MedicTeamAllocations[i, j] = oldPopulation[parent1].MedicTeamAllocations[i, j];
      }

      newPopulation[child1].MedicTeamsPerDepotCount[i] = oldPopulation[parent2].MedicTeamsPerDepotCount[i];
      newPopulation[child1].AmbulancesPerDepotCount[i] = oldPopulation[parent2].AmbulancesPerDepotCount[i];
      newPopulation[child2].MedicTeamsPerDepotCount[i] = oldPopulation[parent1].MedicTeamsPerDepotCount[i];
      newPopulation[child2].AmbulancesPerDepotCount[i] = oldPopulation[parent1].AmbulancesPerDepotCount[i];

      newPopulation[child1].AllAllocatedAmbulancesCount += oldPopulation[parent2].AmbulancesPerDepotCount[i];
      newPopulation[child1].AllAllocatedMedicTeamsCount += oldPopulation[parent2].MedicTeamsPerDepotCount[i];
      newPopulation[child2].AllAllocatedAmbulancesCount += oldPopulation[parent1].AmbulancesPerDepotCount[i];
      newPopulation[child2].AllAllocatedMedicTeamsCount += oldPopulation[parent1].MedicTeamsPerDepotCount[i];
    }

    Debug.WriteLine("child 1");
    Debug.WriteLine(newPopulation[child1]);

    Debug.WriteLine("child 2");
    Debug.WriteLine(newPopulation[child2]);

    Debug.WriteLine("EXIT CROSSOVER");
    Debug.Flush();

    isChild1Feasible = newPopulation[child1].AllAllocatedAmbulancesCount < Constraints.AvailableAmbulancesCount && newPopulation[child1].AllAllocatedMedicTeamsCount < Constraints.AvailableMedicTeamsCount;
    isChild2Feasible = newPopulation[child2].AllAllocatedAmbulancesCount < Constraints.AvailableAmbulancesCount && newPopulation[child2].AllAllocatedMedicTeamsCount < Constraints.AvailableMedicTeamsCount;

  }

  /// Swaps all shifts on two randomly selected depots, including ambulances
  private void Mutation(int index, double mutationP, Weights[] newPopulation)
  {
    if (_random.NextDouble() < mutationP)
    {
      Debug.WriteLine("MUTATION!");
      Debug.WriteLine("Before mutation: ");
      Debug.WriteLine(newPopulation[index]);

      int depot1 = _random.Next(0, World.Depots.Length);
      int depot2 = _random.Next(0, World.Depots.Length);

      for (int onDepotIndex = 0; onDepotIndex < Constraints.MaxMedicTeamsOnDepotCount; ++onDepotIndex)
      {
        Interval shift = newPopulation[index].MedicTeamAllocations[depot1, onDepotIndex];
        newPopulation[index].MedicTeamAllocations[depot1, onDepotIndex] = newPopulation[index].MedicTeamAllocations[depot2, onDepotIndex];
        newPopulation[index].MedicTeamAllocations[depot2, onDepotIndex] = shift;
      }

      int medicTeamsCount = newPopulation[index].MedicTeamsPerDepotCount[depot1];
      newPopulation[index].MedicTeamsPerDepotCount[depot1] = newPopulation[index].MedicTeamsPerDepotCount[depot2];
      newPopulation[index].MedicTeamsPerDepotCount[depot2] = medicTeamsCount;

      int ambulancesCount = newPopulation[index].AmbulancesPerDepotCount[depot1];
      newPopulation[index].AmbulancesPerDepotCount[depot1] = newPopulation[index].AmbulancesPerDepotCount[depot2];
      newPopulation[index].AmbulancesPerDepotCount[depot2] = ambulancesCount;

      Debug.WriteLine("After mutation: ");
      Debug.WriteLine(newPopulation[index]);
    }
  }
}
