
static class Program
{
  private static int _maxThreads;// max threads in the thread pool 16
  private static int _minThreads;  // min threads in the thread pool 16
  private static int _numberTasks;  // number of tasks to run
  private static int _cpuStress;  // number of Times to run stress the CPU
  private static int _consumeIterations; // number of iterations to stress the CPU
  private static int _maxParallelism;  // max parallelism for "Parallel.For"
  public static void Main(string[] args)
  {

    if (args.Length > 0)
    {
      if (args[0].ToLower() == "help" || args[0].ToLower() == "-help" || args[0].ToLower() == "--help" ||
          args[0].ToLower() == "/help" || args[0].ToLower() == "-h" || args[0].ToLower() == "/h" ||
          args[0].ToLower() == "/?")
      {
        WriteColor("ThreadTest.exe [maxThreads] [minThreads] [numberTasks] [cpuStress] [consumeIterations] [maxParallelism]", ConsoleColor.DarkMagenta);
        Console.WriteLine("    maxThreads --> max threads in the thread pool");
        Console.WriteLine("    minThreads --> min threads in the thread pool");
        Console.WriteLine("    numberTasks --> number of tasks to run");
        Console.WriteLine("    cpuStress --> number of Times to run stress the CPU");
        Console.WriteLine("    consumeIterations --> number of iterations to stress the CPU");
        Console.WriteLine("    maxParallelism --> max parallelism for \"Parallel.For\"");
        return;
      }
    }
    _maxThreads = Convert.ToInt16(args[0]); // max threads in the thread pool 16
    _minThreads = Convert.ToInt16(args[1]);  // min threads in the thread pool 16
    _numberTasks = Convert.ToInt16(args[2]);  // number of tasks to run
    _cpuStress = Convert.ToInt16(args[3]);  // number of Times to run stress the CPU
    _consumeIterations = Convert.ToInt32(args[4]); // number of iterations to stress the CPU
    _maxParallelism = Convert.ToInt16(args[5]); // max parallelism for "Parallel.For"
    Console.Title = "ThreadTest";
    if (ThreadPool.SetMaxThreads(_maxThreads, _maxThreads))
      WriteColor($"Max threads set to {_maxThreads}.", ConsoleColor.Green);
    else
      WriteColor("Error: Max threads not set.", ConsoleColor.White, ConsoleColor.DarkRed);

    if (ThreadPool.SetMinThreads(_minThreads, _minThreads))
      WriteColor($"Min threads set to {_minThreads}.", ConsoleColor.Green);
    else
      WriteColor("Error: Min threads not set.", ConsoleColor.White, ConsoleColor.DarkRed);

    WriteColor("\r\n*****************************", ConsoleColor.DarkCyan);
    WriteColor("* Serial For Loop in Thread *", ConsoleColor.DarkCyan);
    WriteColor("*****************************\r\n", ConsoleColor.DarkCyan);
    var elapsedTimeSerial = SerialProcessThread();

    Thread.Sleep(1000);

    WriteColor("\r\n*******************************", ConsoleColor.DarkCyan);
    WriteColor("* Parallel For Loop in Thread *", ConsoleColor.DarkCyan);
    WriteColor("*******************************\r\n", ConsoleColor.DarkCyan);
    var elapsedTimeParallel = ParallelProcessThread();

    Thread.Sleep(1000);


    WriteColor($"\r\nProcessTime(For) ElapsedTime: {elapsedTimeSerial.TotalMilliseconds}", elapsedTimeParallel > elapsedTimeSerial ? ConsoleColor.DarkYellow : ConsoleColor.Yellow);
    WriteColor($"ProcessTime (ParallelFor) ElapsedTime: {elapsedTimeParallel.TotalMilliseconds}", elapsedTimeParallel > elapsedTimeSerial ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
    Console.ForegroundColor = ConsoleColor.White;

    static TimeSpan SerialProcessThread()
    {

      var startTimeSerial = DateTime.Now;
      for (var task = 1; task < _numberTasks; task++)
      {
        ThreadPool.QueueUserWorkItem(new WaitCallback(TaskCallBack), task);
      }
      while (ThreadPool.PendingWorkItemCount > 0)
      {
        Thread.Sleep(10);
      }
      var endTimeSerial = DateTime.Now;
      return endTimeSerial - startTimeSerial;
    }

    static TimeSpan ParallelProcessThread()
    {
      var startTimeParallel = DateTime.Now;
      Parallel.For(1, _numberTasks, new ParallelOptions { MaxDegreeOfParallelism = _maxParallelism }, task =>
      {
        ThreadPool.QueueUserWorkItem(new WaitCallback(TaskCallBackParallel), task);
      });
      while (ThreadPool.PendingWorkItemCount > 0)
      {
        Thread.Sleep(10);
      }

      var endTimeParallel = DateTime.Now;
      return endTimeParallel - startTimeParallel;
    }

    static void TaskCallBack(object ThreadNumber)
    {
      var startTime = DateTime.Now;
      var rand = new Random();
      for (var i = 0; i < _cpuStress; i++)
      {
        _ = ConsumeCPU(_consumeIterations);
      }
      var elapsedTime = DateTime.Now - startTime;
      WriteColor($"Thread (serial) {ThreadNumber} is done. ElapsedTime: {elapsedTime.TotalMilliseconds} ");
    }

    static void TaskCallBackParallel(object ThreadNumber)
    {
      var startTime = DateTime.Now;
      var rand = new Random();
      Parallel.For(0, _cpuStress, new ParallelOptions { MaxDegreeOfParallelism = _maxParallelism }, i =>
      {
        _ = ConsumeCPU(_consumeIterations);
      });
      var elapsedTime = DateTime.Now - startTime;
      WriteColor($"Thread (Parallel) {ThreadNumber} is done. ElapsedTime: {elapsedTime.TotalMilliseconds} ");
    }


    static void WriteColor(string message, ConsoleColor foreColor = ConsoleColor.White, ConsoleColor backColor = ConsoleColor.Black)
    {
      Console.ForegroundColor = foreColor;
      Console.BackgroundColor = backColor;
      Console.WriteLine(message);
      Console.ForegroundColor = ConsoleColor.White;
      Console.BackgroundColor = ConsoleColor.Black;
    }

    static double ConsumeCPU(int iterations)
    {
      double pi = 0.0;
      double denominator = 1.0;

      for (int i = 0; i < iterations; i++)
      {
        if (i % 2 == 0)
          pi += 4.0 / denominator;
        else
          pi -= 4.0 / denominator;
        denominator += 2.0;
      }
      return pi;
    }
  }
}