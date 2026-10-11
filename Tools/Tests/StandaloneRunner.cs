using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using WarWW3.Core;
using WarWW3.Tests;

internal static class StandaloneRunner
{
    static int Main(string[] args)
    {
        int passed = 0, failed = 0;
        foreach (var test in CoreChecks.Cases())
        {
            try { test.Value(); Console.WriteLine("PASS " + test.Key); passed++; }
            catch (Exception ex) { Console.WriteLine("FAIL " + test.Key + ": " + ex.Message); failed++; }
        }
        Console.WriteLine("Core checks: " + passed + " passed, " + failed + " failed.");
        // Sustained scenario: very high health prevents early victory from making this a no-op benchmark.
        var durable = new UnitStats("benchmark", 100000000, 0, 1, 6, 20, 1);
        var simulation = PrototypeScenario.Create(durable, durable);
        for (int i = 0; i < 100; i++) simulation.Step(true);
        GC.Collect(); long memoryBefore = GC.GetTotalMemory(true);
        var stopwatch = Stopwatch.StartNew(); const int ticks = 2000;
        for (int i = 0; i < ticks; i++) simulation.Step(true);
        stopwatch.Stop();
        string report = "{\n  \"scope\": \"C# domain only; excludes Unity rendering, physics, input, and GPU\",\n" +
            "  \"passed\": " + passed + ", \"failed\": " + failed + ",\n  \"units\": 14, \"measuredTicks\": " + ticks + ",\n" +
            "  \"elapsedMilliseconds\": " + stopwatch.ElapsedMilliseconds + ",\n  \"managedBytesBefore\": " + memoryBefore + ",\n" +
            "  \"managedBytesAfter\": " + GC.GetTotalMemory(false) + ",\n  \"processorCount\": " + Environment.ProcessorCount + ",\n" +
            "  \"runtime\": \"" + Environment.Version + "\",\n  \"outcome\": \"" + simulation.Outcome + "\"\n}\n";
        Directory.CreateDirectory("TestResults"); File.WriteAllText("TestResults/standalone-summary.json", report, new UTF8Encoding(false));
        Console.WriteLine(report); return failed == 0 ? 0 : 1;
    }
}
