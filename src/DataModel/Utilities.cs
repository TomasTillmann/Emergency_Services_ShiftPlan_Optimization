using System.Collections.Generic;
using System.Linq;
using Optimizing;

namespace MyExtensions;
using System;

public static class Utilities
{
  /// Fisher - Yates
  public static void Shuffle<T>(this T[] array, Random? random = null)
  {
    random ??= new Random();
    for (int i = array.Length - 1; i > 0; i--)
    {
      int j = random.Next(0, i + 1);
      Swap(ref array[i], ref array[j]);
    }
  }

  private static void Swap<T>(ref T a, ref T b)
  {
    (a, b) = (b, a);
  }
}

public static class BasicMovesGeneratorHelperExtensions
{
  /// <summary>
  /// </summary>
  /// <param name="moves">If produced by <see cref="MoveGeneratorBase"/>, the instance is shared, due to performance reasons.
  /// For correct enumeration, calling e.g. ToList() is not sufficient. You need to copy the shred instance for every one enumeration.</param>
  /// <returns></returns>
  public static List<MoveSequenceDuo> Enumerate(this IEnumerable<MoveSequenceDuo> moves, int bufferSize = 1)
  {
    return moves
      .Select(shared =>
      {
        MoveSequenceDuo x = MoveSequenceDuo.GetNewEmpty(bufferSize);
        shared.Normal.Count = shared.Count;
        shared.Inverse.Count = shared.Count;
        x.Normal.FillFrom(shared.Normal);
        x.Inverse.FillFrom(shared.Inverse);
        return x;
      }).ToList();
  }
}

