using System;
using System.Diagnostics;
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
    }

    private static void CreateDefaultQueryParser ()
    {
      Console.WriteLine ("Creating QueryParser...");
      var stopwatch = Stopwatch.StartNew();
      QueryParser.CreateDefault();
      stopwatch.Stop();
      Console.WriteLine ("Time taken: {0}ms, reference time: ~36ms", stopwatch.ElapsedMilliseconds);
    }

    private static void CreateDefaultNodeTypeProvider ()
    {
      Console.WriteLine ("Creating NodeTypeProvider...");
      var stopwatch = Stopwatch.StartNew();
      ExpressionTreeParser.CreateDefaultNodeTypeProvider();
      stopwatch.Stop();
      Console.WriteLine ("Time taken: {0}ms, reference time: ~30ms", stopwatch.ElapsedMilliseconds);
    }
  }
}