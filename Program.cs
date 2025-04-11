using System.Diagnostics;

#region Parallel.ForEach

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
    Thread.Sleep(1000);
    Console.WriteLine($"[Customer {customerId}] Purchase completed");
}

TimeSpan MeasureTime(Action action)
{
    var stopwatch = Stopwatch.StartNew();
    action();
    stopwatch.Stop();
    return stopwatch.Elapsed;
}

//Process();

#endregion

#region Task.WhenAll and Task.WhenAny
async Task UsingWhenAll()
{
    var task1 = Task.Delay(2000).ContinueWith(_ => "Task 1 done");
    var task2 = Task.Delay(3000).ContinueWith(_ => "Task 2 done");
    var task3 = Task.Delay(1000).ContinueWith(_ => "Task 3 done");

    var allResults = await Task.WhenAll(task1, task2, task3);

    foreach (var result in allResults)
    {
        Console.WriteLine(result);
    }

    Console.WriteLine("All tasks completed.");
}

async Task UsingWhenAny()
{
    var task1 = Task.Delay(2000).ContinueWith(_ => "Task 1 done");
    var task2 = Task.Delay(3000).ContinueWith(_ => "Task 2 done");
    var task3 = Task.Delay(1000).ContinueWith(_ => "Task 3 done");

    var firstFinished = await Task.WhenAny(task1, task2, task3);

    Console.WriteLine($"First completed: {firstFinished.Result}");

    Console.WriteLine("Other tasks may still be running...");
}

async Task UsingWhenAnyCancel()
{
    using var cts = new CancellationTokenSource();

    var task1 = SimulateApiCall("API 1", 3000, cts.Token);
    var task2 = SimulateApiCall("API 2", 1000, cts.Token);
    var task3 = SimulateApiCall("API 3", 2000, cts.Token);

    var tasks = new[] { task1, task2, task3 };

    var completed = await Task.WhenAny(tasks);

    Console.WriteLine($"✅ First completed: {await completed}");

    cts.Cancel();

    try
    {
        await Task.WhenAll(tasks);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("🚫 Remaining tasks were cancelled.");
    }
}

async Task<string> SimulateApiCall(string name, int delay, CancellationToken token)
{
    Console.WriteLine($"🔄 {name} started...");

    try
    {
        await Task.Delay(delay, token);
        return $"{name} finished!";
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine($"❌ {name} was cancelled.");
        throw;
    }
}

//await UsingWhenAll();
//await UsingWhenAny();
//await UsingWhenAnyCancel();
#endregion

#region Task.Run
async Task<string> ProcessDataAsync()
{
    return await Task.Run(() =>
    {
        // // Simulates heavy work
        Thread.Sleep(20);
        return "Processing completed!";
    });
}

//await ProcessDataAsync();
#endregion
