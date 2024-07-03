namespace ESSP.DataModel;
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
    T temp = a;
    a = b;
    b = temp;
  }
}
