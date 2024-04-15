using ESSP.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Model.Extensions;

public static class Extensions
{
  public static List<T> GetRandomSamples<T>(this IEnumerable<T> collection, int count, Random random = null)
  {
    random = random ?? new Random();
    return collection.OrderBy(x => random.Next()).Take(count).ToList();
  }
}
