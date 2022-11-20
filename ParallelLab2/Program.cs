// Author: Ignas Bradauskas
// Description: Uses Eventcount algo to sync parallel consumption of fibonacci values from an array.
// Requirements: .Net 6
using ParallelLab2;


var limit = 100;
long [] buffer = new long[limit];
var tasks = new List<Task>();
tasks.Add(Task.Run(() => ConsumeFibonacci()));
tasks.Add(Task.Run(() => ProduceFibonacci()));
await Task.WhenAll(tasks);

void ProduceFibonacci()
{
    int a = 0, b = 1, c = 0;
    buffer[0] = a;
    EventCount.Advance();
    buffer[1] = b;
    EventCount.Advance();
    for (int i = 2; i < limit; i++)
    {
        c = a + b;
        buffer[i] = c;
        EventCount.Advance();
        Thread.Sleep(100);
        a = b;
        b = c;
    }
}

void ConsumeFibonacci()
{
    for (int i = 0; i < limit - 10; i += 10)
    {
        EventCount.Await(i + 10);

        for (int j = i; j < i + 10; j++)
        {
            Console.Write($"{buffer[j]}");
            Console.WriteLine();
        }
    }
}