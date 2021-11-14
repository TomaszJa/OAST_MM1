using OAST_MM1.Queues;
using System;
using System.Diagnostics;
using System.IO;

namespace OAST_MM1
{
    public static class Simulation
    {
        public static void Run()
        {
            double lambda = 0.5;
            double mi = 8;
            double time = 1000;

            try
            {
                PrepareFiles();
                double maxMi, maxLambda;
                int iterations;
                PrepareInput(out lambda, out mi, out time, out maxMi, out maxLambda, out iterations);

                while (true)
                {
                    if (lambda < maxLambda)
                    {
                        lambda += 0.1;
                        lambda = Math.Round(lambda, 1);
                    }
                    if (mi < maxMi)
                    {
                        mi += 0.1;
                        mi = Math.Round(mi, 1);
                    }
                    Console.WriteLine($"Lambda: {lambda}");
                    var simulation = new StandardMM1Queue(lambda, time, mi);

                    simulation.StartSimulation(iterations);

                    if (mi == maxMi && lambda == maxLambda)
                    {
                        break;
                    }
                }
                Process.Start("Notepad.exe", "./Wyniki/Wyniki.txt");
                Process.Start("Notepad.exe", "./Wyniki/Wyniki_Excel.txt");
                Process.Start("Notepad.exe", "./Wyniki/Delays.txt");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error", e);
            }
        }

        private static void PrepareInput(out double lambda, out double mi, out double time, out double maxMi, out double maxLambda, out int iterations)
        {
            Console.WriteLine("Podaj mi początkowe");
            mi = Double.Parse(Console.ReadLine()) - 0.1;

            Console.WriteLine("Podaj mi końcowe");
            maxMi = Double.Parse(Console.ReadLine());
            Console.WriteLine("Podaj lambdę początkową");
            lambda = Double.Parse(Console.ReadLine()) - 0.1;

            Console.WriteLine("Podaj lambdę końcową");
            maxLambda = Double.Parse(Console.ReadLine());
            Console.WriteLine("Podaj czas symulacji");
            time = Double.Parse(Console.ReadLine());
            while (time <= 120)
            {
                Console.WriteLine("Czas musi być większy od 120 ze względu na rozbieg symulatora");
                time = Double.Parse(Console.ReadLine());
            }

            Console.WriteLine("Podaj liczbę iteracji dla każdej lambdy");
            iterations = int.Parse(Console.ReadLine());
        }

        private static void PrepareFiles()
        {
            using (StreamWriter sw = File.CreateText("./Wyniki/Wyniki.txt"))
            {
                ;
            }

            using (StreamWriter sw = File.CreateText("./Wyniki/Wyniki_Excel.txt"))
            {
                ;
            }

            if (!Directory.Exists("./Wyniki"))
            {
                Directory.CreateDirectory("./Wyniki");
            }
        }
    }
}
