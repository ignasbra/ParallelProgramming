// Author: Ignas Bradauskas
// Description: Counts occurances of "Fizz", for the game FizzBuzz https://en.wikipedia.org/wiki/Fizz_buzz, for a given iteration count.
// Usage: First parameter states if to use a mutex, second is the iteration count. 
// E.g. ParallelLab1.exe "false" "100000000"
// Requirements: .Net 6
using System.Diagnostics;

bool isLockEnabled = Convert.ToBoolean(Environment.GetCommandLineArgs()[1]);
int calculationIterations = Convert.ToInt32(Environment.GetCommandLineArgs()[2]);
var timer = new Stopwatch();

// Shared memory object.
int fizzCount = 0;

// Mutex memory.
object fizzCountLock = new();

timer.Start();
Parallel.For(1, calculationIterations, i =>
{
    var result = GetFizzBuzz(i);
    if (isLockEnabled)
    {
        // Mutex lock.
        lock (fizzCountLock)
        {
            // Critical section.
            fizzCount = result.Contains("Fizz") ? fizzCount + 1 : fizzCount;
        } 
    }
    else
    {
        fizzCount = result.Contains("Fizz") ? fizzCount + 1 : fizzCount;
    }
});
timer.Stop();
Console.WriteLine($"Parallel: {fizzCount}, Time elapsed: {timer.Elapsed}, Lock enabled: {isLockEnabled}");

fizzCount = 0;
timer.Start();
for (int i = 1; i < calculationIterations; i++)
{
    var result = GetFizzBuzz(i);
    fizzCount = result.Contains("Fizz") ? fizzCount + 1 : fizzCount;
}
timer.Stop();
Console.WriteLine($"Sequential: {fizzCount}, Time elapsed: {timer.Elapsed}");

string GetFizzBuzz(int i)
{
    if (i % 3 == 0 && i % 5 == 0) return "FizzBuzz";

    if (i % 3 == 0) return "Fizz";

    if (i % 5 == 0) return "Buzz";

    return i.ToString();
}