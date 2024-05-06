// #define DEBUG
#define STATS

using System.Collections.Immutable;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public class GeneticAlgorithmOptimizer : Optimizer
{
  public int PopulationSize { get; set; }
  public int Populations { get; set; }
  public double MutationP { get; set; }
  public float LossCoeff { get; set; }

  /// Is the objective function.
  /// Has to return nonnegative values (>= 0).
  public IObjectiveFunction Fitness => ObjectiveFunction;

  public GeneticAlgorithmOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, int populationSize, int populations, double mutationP, float lossCoeff, IObjectiveFunction fitness = null, Random? random = null)
  : base(DoubleAmbulanceAndTeamsCapacity(world), constraints, shiftTimes, fitness ?? new GAStandardFitness(new Simulation(world), shiftTimes), random)
  {
    PopulationSize = populationSize;
    Populations = populations;
    MutationP = mutationP;
    LossCoeff = lossCoeff;
  }

  //TODO: nahradit Math.Max ifem, pry by melo byt o dost rychlejsi pro doubly
  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    Weights optimalOverAll = default(Weights);
    double optimalOverAllFitness = double.MinValue;

    Weights[] oldPopulation = new Weights[PopulationSize];
    Weights[] newPopulation = new Weights[PopulationSize];
    Span<double> fitness = stackalloc double[PopulationSize];

    PopulateRandomly(oldPopulation);
    PopulateRandomly(newPopulation);

    double fitnessSum;
    for (int population = 0; population < Populations; ++population)
    {
#if STATS
      double maxFitness = double.MinValue;
      double minFitness = double.MaxValue;
      double averageFitness = 0;
#endif

      fitnessSum = 0;

      for (int i = 0; i < PopulationSize; ++i)
      {
        fitness[i] = Math.Max(0, Fitness.Get(oldPopulation[i], incidents) - LossCoeff * Penalty(oldPopulation[i]));
        if (fitness[i] > optimalOverAllFitness)
        {
          optimalOverAllFitness = fitness[i];
          optimalOverAll = oldPopulation[i].Copy();
        }

#if STATS
        if (fitness[i] < minFitness)
        {
          minFitness = fitness[i];
        }

        if (fitness[i] > maxFitness)
        {
          maxFitness = fitness[i];
        }
#endif

        fitnessSum += fitness[i];
      }

#if STATS
      averageFitness = fitnessSum / PopulationSize;
      //Debug.WriteLine(string.Join("\n", fitness.ToArray().OrderByDescending(_ => _)));
      Debug.WriteLine($"Population: {population}, MaxFitness: {maxFitness}, MinFitness: {minFitness}, AverageFitness: {averageFitness}");
      Debug.WriteLine(new string('=', 100));
#endif

#if DEBUG
      fitness.ToArray().Select((Fitness, Index) => (Fitness, Index)).OrderByDescending(_ => _).ToList().ForEach(value =>
      {
        Debug.WriteLine(oldPopulation[value.Index]);
        Debug.WriteLine($"(Index: {value.Index}; Fitness: {value.Fitness}; RawFitness: {Fitness.Get(oldPopulation[value.Index], incidents)}; Penalty: {LossCoeff * Penalty(oldPopulation[value.Index])}; RawPenalty: {Penalty(oldPopulation[value.Index])}; Feasible: {isFeasible(oldPopulation[value.Index])})\n");
      });
#endif

      for (int i = 0; i < PopulationSize - 1; i += 2)
      {
        int parent1 = Select(fitness, fitnessSum);
        //Debug.WriteLine($"Select parent1: {parent1}");
        //Debug.WriteLine($"Select parent1 fitness: {fitness[parent1]}");
        int parent2 = Select(fitness, fitnessSum);
        //Debug.WriteLine($"Select parent2 fitness: {fitness[parent2]}");

        Crossover(parent1, parent2, i, i + 1, oldPopulation, newPopulation);

        Mutation(i, MutationP, newPopulation);
        Mutation(i + 1, MutationP, newPopulation);
      }

      Weights[] temp = oldPopulation;
      oldPopulation = newPopulation;
      newPopulation = temp;

#if DEBUG
      Debug.WriteLine("--------------------------------------------------");
#endif
    }

    int optimalIndex = fitness.ToArray().Select((_, Index) => (_, Index)).Max().Index;

#if STATS
    Debug.WriteLine("Optimal fitness: " + fitness[optimalIndex]);
#endif

    return new List<Weights> { optimalOverAll, oldPopulation[optimalIndex] };
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

  private void Crossover(int parent1, int parent2, int child1, int child2, Weights[] oldPopulation, Weights[] newPopulation)
  {
    int depotIndex = _random.Next(0, World.Depots.Length);
    int onDepotIndex = _random.Next(0, Constraints.MaxMedicTeamsOnDepotCount);

#if DEBUG_CROSSOVER
    Debug.WriteLine("ENTRY CROSSOVER");
    Debug.WriteLine($"depotIndex: {depotIndex}");
    Debug.WriteLine($"onDepotIndex: {onDepotIndex}");
    Debug.WriteLine("parent 1");
    Debug.WriteLine(oldPopulation[parent1]);
    Debug.WriteLine("parent 2");
    Debug.WriteLine(oldPopulation[parent2]);
#endif

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

#if DEBUG_CROSSOVER
    Debug.WriteLine("prvni pulka child1");
    Debug.WriteLine(newPopulation[child1]);
#endif

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

#if DEBUG_CROSSOVER
    Debug.WriteLine("depotIndex child1");
    Debug.WriteLine(newPopulation[child1]);
#endif

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

#if DEBUG_CROSSOVER
    Debug.WriteLine("child 1");
    Debug.WriteLine(newPopulation[child1]);

    Debug.WriteLine("child 2");
    Debug.WriteLine(newPopulation[child2]);

    Debug.WriteLine("EXIT CROSSOVER");
    Debug.Flush();
#endif
  }

  /// Swaps all shifts on two randomly selected depots, including ambulances
  private void Mutation(int index, double mutationP, Weights[] newPopulation)
  {
    if (_random.NextDouble() < mutationP)
    {
#if DEBUG
      Debug.WriteLine("MUTATION!");
      Debug.WriteLine("Before mutation: ");
      Debug.WriteLine(newPopulation[index]);
#endif

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

#if DEBUG
      Debug.WriteLine("After mutation: ");
      Debug.WriteLine(newPopulation[index]);
#endif
    }
  }

  private double Penalty(Weights weights)
  {
    if (isFeasible(weights)) return 0;

    double medicTeamViolationSum = 0;
    double ambulanceViolationSum = 0;

    for (int depotIndex = 0; depotIndex < World.Depots.Length; ++depotIndex)
    {
      medicTeamViolationSum += Math.Max(0, weights.MedicTeamsPerDepotCount[depotIndex] - Constraints.MaxMedicTeamsOnDepotCount) * weights.MedicTeamsPerDepotCount[depotIndex];
      ambulanceViolationSum += Math.Max(0, weights.AmbulancesPerDepotCount[depotIndex] - Constraints.MaxAmbulancesOnDepotCount) * weights.AmbulancesPerDepotCount[depotIndex];
    }

    return (medicTeamViolationSum / World.Depots.Length + ambulanceViolationSum / World.Depots.Length) / 2;
  }

  private bool isFeasible(Weights weights)
  {
    return weights.AllAllocatedAmbulancesCount < Constraints.AvailableAmbulancesCount && weights.AllAllocatedMedicTeamsCount < Constraints.AvailableMedicTeamsCount;
  }

  private static World DoubleAmbulanceAndTeamsCapacity(World world)
  {
    return new World()
    {
      Depots = world.Depots,
      Hospitals = world.Hospitals,
      DistanceCalculator = world.DistanceCalculator,
      GoldenTimeSec = world.GoldenTimeSec,
      AvailableMedicTeams = Enumerable.Range(0, 2 * world.AvailableMedicTeams.Length).Select(_ => new MedicTeam()).ToImmutableArray(),
      AvailableAmbulances = Enumerable.Range(0, 2 * world.AvailableAmbulances.Length).Select(_ => new Ambulance()).ToImmutableArray(),
    };
  }
}
