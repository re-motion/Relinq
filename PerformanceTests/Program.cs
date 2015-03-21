using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Remotion.Linq.Collections;
using Remotion.Linq.Parsing.Structure;

namespace Remotion.Linq.PerformanceTests
{
  internal class Program
  {
    private static int Main (string[] args)
    {
      if (args.Length != 1)
      {
        Console.WriteLine ("Test type must be specified via commandline argument. E.g. \"Remotion.Linq.PerformanceTests.exe QueryParser\"");
        return -1;
      }

      var testType = args[0];
      switch (testType)
      {
        case "QueryParser":
          CreateDefaultQueryParser();
          return 0;

        case "NodeTypeProvider":
          CreateDefaultNodeTypeProvider();
          return 0;

        default:
          Console.WriteLine ("Valid test types are: 'QueryParser'");
        return -1;
      }
      //Only enable one of the methods.

      //CreateDefaultNodeProvider();

      //MethodsViaList();

      //MethodsViaLookup();
    }

    private static void CreateDefaultQueryParser ()
    {
      Console.WriteLine ("Creating QueryParser...");
      var stopwatch = Stopwatch.StartNew();
      QueryParser.CreateDefault();
      stopwatch.Stop();
      Console.WriteLine ("Time taken: {0}ms, reference time: ~77ms", stopwatch.ElapsedMilliseconds);
    }

    private static void CreateDefaultNodeTypeProvider ()
    {
      Console.WriteLine ("Creating NodeTypeProvider...");
      var stopwatch = Stopwatch.StartNew();
      ExpressionTreeParser.CreateDefaultNodeTypeProvider();
      stopwatch.Stop();
      Console.WriteLine ("Time taken: {0}ms, reference time: ~72ms", stopwatch.ElapsedMilliseconds);
    }

    private static void MethodsViaList ()
    {
      var stopwatch1 = Stopwatch.StartNew();
      var enumerable = typeof (Enumerable).GetMethods().ToLookup (mi => mi.Name);
      var queryable = typeof (Enumerable).GetMethods().ToLookup (mi => mi.Name);
      stopwatch1.Stop();
      Console.WriteLine ("Time taken method reads: {0}µs", stopwatch1.Elapsed.TotalMilliseconds * 1000);

      var result = new List<MethodInfo>();
      var stopwatch2 = Stopwatch.StartNew();
      foreach (MethodInfo methodInfo in enumerable.Concat (queryable))
      {
        if (methodInfo.Name == "Test")
          result.Add (methodInfo);
      }
      stopwatch2.Stop();
      Console.WriteLine ("Time taken single run: {0}µs", stopwatch2.Elapsed.TotalMilliseconds * 1000);
    }

    private static void MethodsViaLookup ()
    {
      var stopwatch1 = Stopwatch.StartNew();
      var enumerable = typeof (Enumerable).GetMethods().ToLookup (mi => mi.Name);
      var queryable = typeof (Enumerable).GetMethods().ToLookup (mi => mi.Name);
      stopwatch1.Stop();
      Console.WriteLine ("Time taken method reads: {0}µs", stopwatch1.Elapsed.TotalMilliseconds * 1000);

      var result = new List<MethodInfo>();
      var stopwatch2 = Stopwatch.StartNew();
      for (int i = 0; i < 50; i++)
      {
        foreach (MethodInfo methodInfo in enumerable["Test"])
          result.Add (methodInfo);
        foreach (MethodInfo methodInfo in queryable["Test"])
          result.Add (methodInfo);
      }
      stopwatch2.Stop();
      Console.WriteLine ("Time taken 50 runs: {0}µs", stopwatch2.Elapsed.TotalMilliseconds * 1000);
    }
  }
}