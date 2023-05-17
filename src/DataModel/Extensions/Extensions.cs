using ESSP.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Extensions;

public static class Extensions
{
    /// <summary>
    /// returns subset for with minimal value, with at least one element, if this Enumerable is not empty.
    /// </summary>
    /// <typeparam name="MinValueType"></typeparam>
    /// <typeparam name="BaseType"></typeparam>
    /// <param name="stuff"></param>
    /// <param name="dataTransform"></param>
    /// <param name="infinity"></param>
    /// <returns></returns>
    public static List<BaseType> FindMinSubset<MinValueType, BaseType>(this IEnumerable<BaseType> stuff, Func<BaseType, MinValueType> dataTransform) where MinValueType : IComparable<MinValueType>
    {
        List<BaseType> result = new();
        if (!stuff.Any())
        {
            return result;
        }

        MinValueType minValue = dataTransform(stuff.First());

        foreach (BaseType s in stuff)
        {
            MinValueType value = dataTransform(s);
            // value < minValue
            if (value.CompareTo(minValue) == -1)
            {
                minValue = value;
                result.Clear();
                result.Add(s);
            }
            // value == minValue
            else if (value.CompareTo(minValue) == 0)
            {
                result.Add(s);
            }
        }

        return result;
    }

    public static List<BaseType> FindMaxSubset<MaxValueType, BaseType>(this IEnumerable<BaseType> stuff, Func<BaseType, MaxValueType> dataTransform) where MaxValueType : IComparable<MaxValueType>
    {
        List<BaseType> result = new();
        if (!stuff.Any())
        {
            return result;
        }

        MaxValueType maxValue = dataTransform(stuff.First());

        foreach (BaseType s in stuff)
        {
            MaxValueType value = dataTransform(s);
            // value > maxValue
            if (value.CompareTo(maxValue) == 1)
            {
                maxValue = value;
                result.Clear();
                result.Add(s);
            }
            // value == maxValue
            else if (value.CompareTo(maxValue) == 0)
            {
                result.Add(s);
            }
        }

        return result;
    }

    public static string Visualize<T>(this IEnumerable<T> enumerable, string separator = ", ", int indent = 0, Func<T, string> toString = null)
    {
        string str = "";
        string indentStr = new('\t', indent);

        foreach (T item in enumerable)
        {
            str += indentStr + (toString is null ? item?.ToString() : toString(item)) + separator;
        }

        // remove the last separator
        if(str.Length > 0)
        {
            str.Substring(0, str.Length - separator.Length);
        }

        return str;
    }

    public static T GetRandom<T>(this IList<T> collection, Random random = null)
    {
        random = random ?? new Random();
        return collection[random.Next(0, collection.Count - 1)];
    }

    public static List<T> GetRangeRandom<T>(this List<T> collection, Random random = null, int minCount = 0, int maxCount = int.MaxValue)
    {
        if(collection.Count == 0 || collection.Count - minCount < 0)
        {
            return new List<T>();
        }

        random = random ?? new Random();
        int start = random.Next(0, collection.Count - minCount);
        int count = random.Next(minCount, Math.Min(collection.Count - start, maxCount));

        return collection.GetRange(start, count);
    }

    public static void ModifyToLargest(this ShiftPlan shiftPlan, Constraints constraints)
    {
        Seconds largestDuration = constraints.AllowedShiftDurations.FindMaxSubset(_ => _).First();

        foreach (Shift shift in shiftPlan.Shifts)
        {
            shift.Work = Interval.GetByStartAndDuration(0.ToSeconds(), largestDuration);
        }
    }
}
