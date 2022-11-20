namespace ParallelLab2
{
    public static class EventCount
    {
        public static int Count { get; set; } = 0;
        public static object MonitorLock = new object();
        public static object CountLock = new object();

        public static void Advance()
        {
            Monitor.Enter(MonitorLock);
            Count++; 
            Monitor.Pulse(MonitorLock);
            Monitor.Exit(MonitorLock);
        }

        public static void Await(int value)
        {
            Monitor.Enter(MonitorLock);
            Monitor.Wait(MonitorLock);

            if (Count < value)
            {
                Await(value);
            }

            Monitor.Exit(MonitorLock);
            return;
        }

        public static int Read() => Count;
    }
}
