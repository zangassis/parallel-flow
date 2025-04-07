using System.Diagnostics;

void Process()
{
    var customers = new int[10];
    for (int i = 0; i < customers.Length; i++)
        customers[i] = i + 1;

    Console.WriteLine("*== Sequential Execution ==*");
    var sequentialTime = MeasureTime(() =>
    {
        foreach (var customer in customers)
        {
            ProcessPurchase(customer);
        }
    });
    Console.WriteLine($"Total time (sequential): {sequentialTime.TotalSeconds:F2} seconds\n");

    Console.WriteLine("*== Parallel Execution ==*");
    var parallelTime = MeasureTime(() =>
    {
        Parallel.ForEach(
            customers,
            customer =>
            {
                ProcessPurchase(customer);
            }
        );
    });
    Console.WriteLine($"Total time (parallel): {parallelTime.TotalSeconds:F2} seconds");
}

void ProcessPurchase(int customerId)
{
    Console.WriteLine($"[Customer {customerId}] Purchase started...");
    Thread.Sleep(1000); // simulating a long-running task
    Console.WriteLine($"[Customer {customerId}] Purchase completed");
}

TimeSpan MeasureTime(Action action)
{
    var stopwatch = Stopwatch.StartNew();
    action();
    stopwatch.Stop();
    return stopwatch.Elapsed;
}

Process();
