using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
/* source from : http://blog.bodurov.com/Performance-SortedList-SortedDictionary-Dictionary-Hashtable/
 */
namespace CollectionTestPerfomance
{
	class Program
	{
		private static int _searchIndex = 27;
		private const int NUMBER_INSERTED_KEYS = 50000;
		private const int NUMBER_TESTS = 8000;

		private static readonly string[] Letters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

		public static void Main(string[] args)
		{
			try
			{
				// TRY STARTS HERE ----------

				var listDictionary = new List<RunResult>();
				var listSortedDictionary = new List<RunResult>();
				var listHashtable = new List<RunResult>();
				var listSorderList = new List<RunResult>();
				var watch = Stopwatch.StartNew();

				for (var i = 0; i < NUMBER_TESTS; i++)
				{
					_searchIndex += 1;
					var rand = new Random();
					var randInt = rand.Next(0, 4);
					switch (randInt)
					{
						case 0:
							listDictionary.Add(
								Test("Dictionary", new Dictionary<string, string>(), i));
							break;
						case 1:
							listSortedDictionary.Add(
								Test("SortedDictionary",
								     new SortedDictionary<string, string>(), i));
							break;
						case 2:
							listHashtable.Add(
								Test("Hashtable", new Hashtable(), i));
							break;
						case 3:
							listSorderList.Add(
								Test("SortedList", new SortedList(), i));
							break;
					}
				}

				Console.Clear();
				Msg("Time taken (minutes): {0} or about {1} minutes and {2} seconds",
					watch.Elapsed.TotalMinutes,
					watch.Elapsed.Minutes,
					watch.Elapsed.Seconds);

				var resultDict = CalculateAvg(listDictionary);
				var resultSortDict = CalculateAvg(listSortedDictionary);
				var resultHash = CalculateAvg(listHashtable);
				var resultSortList = CalculateAvg(listSorderList);

				var min =
					new RunResult
					{
						MemoryUsed = Math.Min(Math.Min(Math.Min(resultDict.MemoryUsed, resultSortDict.MemoryUsed), resultHash.MemoryUsed), resultSortList.MemoryUsed),
						InsertTicks = Math.Min(Math.Min(Math.Min(resultDict.InsertTicks, resultSortDict.InsertTicks), resultHash.InsertTicks), resultSortList.InsertTicks),
						SearchTicks = Math.Min(Math.Min(Math.Min(resultDict.SearchTicks, resultSortDict.SearchTicks), resultHash.SearchTicks), resultSortList.SearchTicks),
						ForEachTicks = Math.Min(Math.Min(Math.Min(resultDict.ForEachTicks, resultSortDict.ForEachTicks), resultHash.ForEachTicks), resultSortList.ForEachTicks)
					};

				PrintResults(resultDict, listDictionary.Count, min, "Dictionary");
				PrintResults(resultSortDict, listDictionary.Count, min, "SortedDictionary");
				PrintResults(resultHash, listDictionary.Count, min, "Hashtable");
				PrintResults(resultSortList, listDictionary.Count, min, "SortedList");


			}
			catch (Exception ex)
			{
				Msg("{0}", ex);
			}
		}

		private static RunResult CalculateAvg(List<RunResult> list)
		{
			decimal sumMemory = 0;
			decimal sumInsert = 0;
			decimal sumSearch = 0;
			decimal sumForEach = 0;

			foreach (var curr in list)
			{
				sumMemory += curr.MemoryUsed;
				sumInsert += curr.InsertTicks;
				sumSearch += curr.SearchTicks;
				sumForEach += curr.ForEachTicks; 
			}

			return new RunResult
			{
				MemoryUsed = sumMemory / list.Count,
				InsertTicks = sumInsert / list.Count,
				SearchTicks = sumSearch / list.Count,
				ForEachTicks = sumForEach / list.Count,
			};

		}

		private static void PrintResults(RunResult result, int count, RunResult min, string name)
		{
			Msg("--------- Results for {0}", name);
			Msg("# Tests {0}", count);
			Msg("Memory Used    Insert Ticks    Search Ticks    ForEach Ticks");
			Msg("Average Values:");
			Msg("{0,11:N} {1,13:N} {2,14:N} {3,14:N}",
				result.MemoryUsed,
				result.InsertTicks,
				result.SearchTicks,
				result.ForEachTicks);
			Msg("Performance Coefficient:");
			Msg("{0,11:N} {1,13:N} {2,14:N} {3,14:N}",
				min.MemoryUsed / result.MemoryUsed,
				min.InsertTicks / result.InsertTicks,
				min.SearchTicks / result.SearchTicks,
				min.ForEachTicks / result.ForEachTicks);
			Msg("");
		}

		
		private static void Msg(string name, params object[] args)
		{
			Console.WriteLine(name, args);
		}

		private static RunResult Test(string name, IDictionary dict, int n)
		{
			Console.Clear();
			Msg("Currently executing test {1} of {2} for {0} object",
				name, n + 1, NUMBER_TESTS);
			var rr = new RunResult();
			Stopwatch watch;
			var rand = new Random();
			var memoryStart = GC.GetTotalMemory(true);
			long insertTicksSum = 0;
			for (var i = 0; i < NUMBER_INSERTED_KEYS; i++)
			{
				var key = GetRandomLetter(rand, i) + "_key" + i;
				var value = "value" + i;

				watch = Stopwatch.StartNew();
				dict.Add(key, value);
				watch.Stop();

				insertTicksSum += watch.ElapsedTicks;
			}
			rr.MemoryUsed = GC.GetTotalMemory(true) - memoryStart;


			rr.InsertTicks = insertTicksSum;

			watch = Stopwatch.StartNew();
			object searchResult = dict["C_key" + _searchIndex];
			watch.Stop();

			rr.SearchTicks = watch.ElapsedTicks;

			watch = Stopwatch.StartNew();
			foreach (var curr in dict) { }
			watch.Stop();

			rr.ForEachTicks = watch.ElapsedTicks;

			return rr;
		}

		private static string GetRandomLetter(Random rand, int i)
		{
			return i == _searchIndex ? "C" : Letters[rand.Next(0, 10)];
		}
	}

	public class RunResult
	{
		public decimal MemoryUsed;
		public decimal InsertTicks;
		public decimal SearchTicks;
		public decimal ForEachTicks;
	}
}
