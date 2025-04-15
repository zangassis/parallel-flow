using System.Collections.Concurrent;
using System.Diagnostics;

#region 1 - Parallel.ForEach

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

#region 2 - Parallel.Invoke

void RunInvoke()
{
    Parallel.Invoke(
        () => PerformingAnAction("SendDataToA", 1000),
        () => PerformingAnAction("SendDataToB", 500),
        () => PerformingAnAction("SendDataToC", 2000)
    );

    Console.WriteLine("All tasks are finished");
}

void PerformingAnAction(string name, int delay)
{
    Console.WriteLine($"Starting {name} on thread {Thread.CurrentThread.ManagedThreadId}");
    Thread.Sleep(delay);
    Console.WriteLine($"Ending {name}");
}

//RunInvoke();
#endregion

#region 3 - Task.WhenAll and Task.WhenAny
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
        Console.WriteLine($"❌ {name} was cancelled");
        throw;
    }
}

//await UsingWhenAll();
//await UsingWhenAny();
//await UsingWhenAnyCancel();
#endregion

#region 4 - Task.Run
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

#region 5 - Parallel.ForEachAsync
async Task ExecuteAsync()
{
    var userIds = Enumerable.Range(1, 100).ToList();
    var processedUsers = new ConcurrentBag<string>();

    var parallelOptions = new ParallelOptions
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount
    };

    await Parallel.ForEachAsync(
        userIds,
        parallelOptions,
        async (userId, cancellationToken) =>
        {
            var userData = await FetchUserDataAsync(userId);
            var result = await ProcessUserDataAsync(userData);
            processedUsers.Add(result);
        }
    );

    // At this point processedUsers contains all the results
    Console.WriteLine($"Total processed users: {processedUsers.Count}");
}

async Task<string> FetchUserDataAsync(int userId)
{
    await Task.Delay(100); // Simulate I/O-bound async work
    return $"UserData-{userId}";
}

async Task<string> ProcessUserDataAsync(string userData)
{
    await Task.Delay(50); //simulate processing work
    return $"Processed-{userData}";
}

await ExecuteAsync();
#endregion
