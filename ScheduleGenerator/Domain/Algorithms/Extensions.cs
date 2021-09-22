﻿using System;
using System.Collections.Generic;

namespace Domain.Algorithms
{
    public static class ArrayExtensions
    {
        public static T[] Shuffled<T>(this T[] arr)
        {
            var rnd = new Random();
            var n = arr.Length;
            var shuffledArr = new T[n];
            for (var i = 0; i < n; ++i)
            {
                var j = rnd.Next(i + 1);
                shuffledArr[i] = shuffledArr[j];
                shuffledArr[j] = arr[i];
            }

            return shuffledArr;
        }
    }

    public static class DictionaryExtensions
    {
        public static void SafeAdd<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue value)
            where TKey : notnull
        {
            if (dict.ContainsKey(key))
                dict[key].Add(value);
            else
                dict.Add(key, new List<TValue> {value});
        }

        public static void SafeAdd<TKey1, TKey2, TValue>(this Dictionary<TKey1, Dictionary<TKey2, TValue>> dict,
            TKey1 key1, TKey2 key2, TValue value)
            where TKey1 : notnull
            where TKey2 : notnull
        {
            if (dict.ContainsKey(key1))
                dict[key1].Add(key2, value);
            else
                dict.Add(key1, new Dictionary<TKey2, TValue> {{key2, value}});
        }

        public static void SafeAdd<TKey1, TKey2, TValue>(
            this Dictionary<TKey1, Dictionary<TKey2, SortedSet<TValue>>> dict, TKey1 key1, TKey2 key2, TValue value)
            where TKey1 : notnull
            where TKey2 : notnull
        {
            if (!dict.ContainsKey(key1))
            {
                dict[key1] = new Dictionary<TKey2, SortedSet<TValue>>();
            }

            if (!dict[key1].ContainsKey(key2))
            {
                dict[key1][key2] = new SortedSet<TValue>();
            }

            dict[key1][key2].Add(value);
        }

        public static void SafeAdd<TKey1, TKey2, TKey3, TKey4, TValue>(
            this Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, Dictionary<TKey4, TValue>>>> dict,
            TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TValue value)
            where TKey1 : notnull
            where TKey2 : notnull
            where TKey3 : notnull
            where TKey4 : notnull
        {
            if (!dict.ContainsKey(key1))
            {
                dict[key1] = new Dictionary<TKey2, Dictionary<TKey3, Dictionary<TKey4, TValue>>>();
            }

            if (!dict[key1].ContainsKey(key2))
            {
                dict[key1][key2] = new Dictionary<TKey3, Dictionary<TKey4, TValue>>();
            }

            if (!dict[key1][key2].ContainsKey(key3))
            {
                dict[key1][key2][key3] = new Dictionary<TKey4, TValue>();
            }

            if (dict[key1][key2][key3].ContainsKey(key4))
                throw new FormatException($"Dictionary already contains key3: {key4}");
            dict[key1][key2][key3].Add(key4, value);
        }

        public static void SafeIncrement<TKey1, TKey2, TKey3>(
            this Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, int>>> dict,
            TKey1 key1, TKey2 key2, TKey3 key3)
            where TKey1 : notnull
            where TKey2 : notnull
            where TKey3 : notnull
        {
            if (!dict.ContainsKey(key1))
            {
                dict[key1] = new Dictionary<TKey2, Dictionary<TKey3, int>>();
            }

            if (!dict[key1].ContainsKey(key2))
            {
                dict[key1][key2] = new Dictionary<TKey3, int>();
            }

            if (!dict[key1][key2].ContainsKey(key3))
            {
                dict[key1][key2][key3] = 0;
            }

            dict[key1][key2][key3]++;
        }

        public static void SafeDecrement<TKey1, TKey2, TKey3>(
            this Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, int>>> dict,
            TKey1 key1, TKey2 key2, TKey3 key3)
            where TKey1 : notnull
            where TKey2 : notnull
            where TKey3 : notnull
        {
            if (!dict.ContainsKey(key1))
            {
                throw new FormatException($"Dictionary does not contains key1: {key1}");
            }

            if (!dict[key1].ContainsKey(key2))
            {
                throw new FormatException($"Dictionary does not contains key2: {key2}");
            }

            if (!dict[key1][key2].ContainsKey(key3))
            {
                throw new FormatException($"Dictionary does not contains key3: {key3}");
            }

            if (dict[key1][key2][key3] == 0)
                return;
            dict[key1][key2][key3]--;
        }
    }
}