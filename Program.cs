using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Burnout
{

    class Program
    {
        private static readonly ConcurrentStack<WorkItem> workItems = new ConcurrentStack<WorkItem>();

        private static double energy = 100;

        private static double recoveryRate = 1;

        private static int burnoutSaves = 2;

        private static int burnoutCounter = 0;

        private static int burnoutTrigger = 5;

        private static int workLoad = 10;


        public Program()
        {

        }

        private static void GenerateWork()
        {
            Console.WriteLine("Initializing.");
            Console.WriteLine("Starting Energy: (0-100.0)");
            double.TryParse(Console.ReadLine(), out energy);
            Console.WriteLine("Recovery Rate: (0-1.0)");
            double.TryParse(Console.ReadLine(), out recoveryRate);
            Console.WriteLine("Workload: (1-100)");
            int.TryParse(Console.ReadLine(), out workLoad);
            Console.WriteLine("Burnout Saves: (0-2)");
            int.TryParse(Console.ReadLine(), out burnoutSaves);
            Console.WriteLine("Burnout Trigger: (1-5)");
            int.TryParse(Console.ReadLine(), out burnoutTrigger);


            for (int i = 0; i < workLoad; i++)
            {
                var workAmount = new Random().Next(1, 10);
                workItems.Push(new WorkItem()
                {
                    Remaining = workAmount
                });
                
            }

            Console.WriteLine("Initial work: {0} tasks with {1} units of work remaining.", workItems.Count, workItems.Sum(x => x.Remaining));
        }

        private static void Main(string[] args)
        {
            GenerateWork();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            DoWork();
            stopWatch.Stop();
            Console.WriteLine("Worked for {0} seconds", stopWatch.ElapsedMilliseconds / 1000);
            Console.ReadLine();
        }

        private static bool HasWork()
        {
            WorkItem item;
            workItems.TryPeek(out item);

            return item != null && item.Remaining > 0;
        }

        private static void DoWork()
        {
            while (HasWork())
            {
                Console.WriteLine("Found work. Attempting to work.");

                while (energy > 0)
                {
                    WorkItem item;
                    workItems.TryPop(out item);

                    if (item == null)
                        break;

                    var effiency = energy / 100;

                    item.Remaining = item.Remaining - effiency;
                    if (item.Remaining > 0)
                    {
                        Console.WriteLine("Worked on a task, work remaining {0} (effiency: {1})",
                            item.Remaining.ToString("F"), effiency.ToString("F"));
                        workItems.Push(item);
                    }
                    else
                    {
                        Console.WriteLine("Task finished! Remaining tasks: {0}", workItems.Count);
                    }

                    energy -= (1 + effiency);
                    Console.WriteLine("Energy spent, current: {0}", energy.ToString("F"));
                }

                Thread.Sleep(1000);
                Console.WriteLine("Not enough energy. Recovering.");

                if (burnoutSaves > 0 && burnoutCounter == burnoutTrigger)
                {
                    Console.WriteLine("Burnout save triggered ({0} remaining)! Attempting to finish the work by working late.", burnoutSaves);
                    energy = 100;
                    recoveryRate = 0.5;
                    burnoutSaves -= 1;
                    burnoutCounter = 0;
                }

                var recoveryAmount = recoveryRate * 1;
                energy += recoveryAmount;

                burnoutCounter += 1;

                Console.WriteLine("Recovering energy by {0} (energy {1}", recoveryAmount.ToString("F"), energy.ToString("F"));
            }
        }
    }

    public class WorkItem
    {
        public double Remaining { get; set; }
    }
}
