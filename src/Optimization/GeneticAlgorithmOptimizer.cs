// #define DEBUG
#define STATS

using System.Collections.Immutable;
using ESSP.DataModel;
using Simulating;

namespace Optimizing;

public class GeneticAlgorithmOptimizer : Optimizer
{
  public int GenerationSize { get; set; }
  public int Generations { get; set; }
  public double MutationP { get; set; }
  public float LossCoeff { get; set; }

  /// Is the objective function.
  /// Has to return nonnegative values (>= 0).
  public IObjectiveFunction Fitness => ObjectiveFunction;

  private readonly MoveGenerator moveGenerator;
  private readonly Move[] mutationMovesBuffer;

  public GeneticAlgorithmOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, int generationSize, int generations, double mutationP, float lossCoeff, IObjectiveFunction fitness = null, Random? random = null)
  : base(DoubleAmbulanceAndTeamsCapacity(world), constraints, shiftTimes, fitness ?? new GAStandardFitness(new Simulation(world, info: false), shiftTimes), random)
  {
    GenerationSize = generationSize;
    Generations = generations;
    MutationP = mutationP;
    LossCoeff = lossCoeff;

    moveGenerator = new MoveGenerator(null, null, null, null); //TODO: refactor move generator out of optimzer hieararchy
    mutationMovesBuffer = new Move[Enum.GetValues(typeof(MoveType)).Length];
  }

  //TODO: nahradit Math.Max ifem, pry by melo byt o dost rychlejsi pro doubly
  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    Weights optimalOverAll = default(Weights);
    double optimalOverAllFitness = double.MinValue;

    Weights[] oldGeneration = new Weights[GenerationSize];
    Weights[] newGeneration = new Weights[GenerationSize];
    Span<double> fitness = stackalloc double[GenerationSize];

    PopulateRandomly(oldGeneration);
    PopulateRandomly(newGeneration);

    double fitnessSum;
    for (int generation = 0; generation < Generations; ++generation)
    {
#if STATS
      double maxFitness = double.MinValue;
      double minFitness = double.MaxValue;
      double averageFitness = 0;
#endif

      fitnessSum = 0;

      for (int i = 0; i < GenerationSize; ++i)
      {
        fitness[i] = Math.Max(0, Fitness.Get(oldGeneration[i], incidents) - LossCoeff * Penalty(oldGeneration[i]));
        if (fitness[i] > optimalOverAllFitness)
        {
          optimalOverAllFitness = fitness[i];
          optimalOverAll = oldGeneration[i].Copy();
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
      averageFitness = fitnessSum / GenerationSize;
      //Debug.WriteLine(string.Join("\n", fitness.ToArray().OrderByDescending(_ => _)));
      Debug.WriteLine($"Generation: {generation}, MaxFitness: {maxFitness}, MinFitness: {minFitness}, AverageFitness: {averageFitness}");
      Debug.WriteLine(new string('=', 100));
#endif

#if DEBUG
      fitness.ToArray().Select((Fitness, Index) => (Fitness, Index)).OrderByDescending(_ => _).ToList().ForEach(value =>
      {
        Debug.WriteLine(oldGeneration[value.Index]);
        Debug.WriteLine($"(Index: {value.Index}; Fitness: {value.Fitness}; RawFitness: {Fitness.Get(oldGeneration[value.Index], incidents)}; Penalty: {LossCoeff * Penalty(oldGeneration[value.Index])}; RawPenalty: {Penalty(oldGeneration[value.Index])}; Feasible: {isFeasible(oldGeneration[value.Index])})\n");
      });
#endif

      for (int i = 0; i < GenerationSize - 1; i += 2)
      {
        int parent1 = Select(fitness, fitnessSum);
        int parent2 = Select(fitness, fitnessSum);

        Crossover(parent1, parent2, i, i + 1, oldGeneration, newGeneration);

        Mutation(i, MutationP, newGeneration);
        Mutation(i + 1, MutationP, newGeneration);
      }

      Weights[] temp = oldGeneration;
      oldGeneration = newGeneration;
      newGeneration = temp;

#if DEBUG
      Debug.WriteLine("--------------------------------------------------");
#endif

      Debug.Flush();
    }

    int optimalIndex = fitness.ToArray().Select((_, Index) => (_, Index)).Max().Index;

#if STATS
    Debug.WriteLine("Optimal fitness: " + fitness[optimalIndex]);
#endif

    return new List<Weights> { optimalOverAll, oldGeneration[optimalIndex] };
  }

  private void PopulateRandomly(Span<Weights> weights)
  {
    for (int i = 0; i < GenerationSize; ++i)
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

    while (partSum < rand && j != GenerationSize - 1)
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

  /// <summary>
  /// Makes a random move by <paramref name="mutationP"/> probability, if any are possible. For every medic team on depot - medic team move and for every depot - ambulance move.
  /// </summary>
  private void Mutation(int index, double mutationP, Weights[] newPopulation)
  {
    int length;
    for (int depotIndex = 0; depotIndex < World.Depots.Length; ++depotIndex)
    {
      for (int medicTeamIndex = 0; medicTeamIndex < Constraints.MaxMedicTeamsOnDepotCount; ++medicTeamIndex)
      {
        if (_random.NextDouble() < mutationP)
        {
          length = moveGenerator.GetAllMedicTeamMoves(newPopulation[index], depotIndex, medicTeamIndex, mutationMovesBuffer);
          if (length != 0) moveGenerator.ModifyMakeMove(newPopulation[index], mutationMovesBuffer[_random.Next(0, length)]);
        }
      }

      if (_random.NextDouble() < mutationP)
      {
        length = moveGenerator.GetAllAmbulanceMoves(newPopulation[index], depotIndex, mutationMovesBuffer);
        if (length != 0) moveGenerator.ModifyMakeMove(newPopulation[index], mutationMovesBuffer[_random.Next(0, length)]);
      }
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
