using System;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        int numberOfThreads = 50;
        int commonDuration = 10;
        //int numberOfThreads = 20;
        //int commonDuration = 2;
        Console.WriteLine($"Common duration for all threads: {commonDuration} seconds");

        CountdownEvent startSignal = new CountdownEvent(1);
        NumberCalculator[] calculators = new NumberCalculator[numberOfThreads];

        for (int i = 0; i < numberOfThreads; i++)
        {
            int individualStep = i + 1; 
            calculators[i] = new NumberCalculator(i, individualStep, startSignal);
            calculators[i].Start();
        }
        ThreadController controller = new ThreadController(commonDuration, calculators);
        Thread controllerThread = new Thread(new ThreadStart(controller.Run));

        Console.WriteLine(">>> Start <<<");
        controllerThread.Start();
        startSignal.Signal(); 
        controllerThread.Join();
        Console.WriteLine(">>> All threads completed <<<");
    }
}

class NumberCalculator
{
    private int threadId;
    private int increment;
    private CountdownEvent startSignal;
    private Thread thread;
    private DateTime startTime;
    private volatile bool stopped = false;

    public NumberCalculator(int threadId, int increment, CountdownEvent startSignal)
    {
        this.threadId = threadId;
        this.increment = increment;
        this.startSignal = startSignal;
        this.thread = new Thread(new ThreadStart(Run));
    }

    public void Start() => thread.Start();
    public void Stop()
    {
        stopped = true;
    }

    public void Run()
    {
        startSignal.Wait(); 
        startTime = DateTime.Now;
        long totalSum = 0;
        long termsCount = 0;
        long currentNumber = 0;

        while (!stopped) 
        {
            totalSum += currentNumber;
            termsCount++;
            currentNumber += increment;
            Thread.Sleep(10);
        }
        TimeSpan duration = DateTime.Now - startTime;
        Console.WriteLine($"[Thread {threadId}] Step: {increment}, " +
                         $"Duration: {duration.TotalSeconds:F3} sec, " +
                         $"Sum: {totalSum}, Terms: {termsCount}");
    }
}
class ThreadController
{
    private int duration;
    private NumberCalculator[] calculators;
    public ThreadController(int duration, NumberCalculator[] calculators)
    {
        this.duration = duration;
        this.calculators = calculators;
    }
    public void Run()
    {
        DateTime startTime = DateTime.Now;
        Thread.Sleep(duration * 1000);
        foreach (var calculator in calculators)
        {
            calculator.Stop();
        }
        TimeSpan actualDuration = DateTime.Now - startTime;
        Console.WriteLine($"All threads stopped after {actualDuration.TotalSeconds:F3} seconds");
    }

}

