using OAST_MM1.Queues;
using System;
using System.Diagnostics;

namespace OAST_MM1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                double lambda = 0.5;
                double mi = 8;
                double time = 1000;

                Console.WriteLine("Podaj mi");
                mi = Double.Parse(Console.ReadLine());

                Console.WriteLine("Podaj lambdę początkową");
                lambda = Double.Parse(Console.ReadLine());

                Console.WriteLine("Podaj lambdę końcową");
                double maxLambda = Double.Parse(Console.ReadLine());

                Console.WriteLine("Podaj czas symulacji");
                time = Double.Parse(Console.ReadLine());
                while(time <= 120)
                {
                    Console.WriteLine("Czas musi być większy od 120 ze względu na rozbieg symulatora");
                    time = Double.Parse(Console.ReadLine());
                }

                Console.WriteLine("Podaj liczbę iteracji dla każdej lambdy");
                int iterations = int.Parse(Console.ReadLine());

                while (lambda <= maxLambda)
                {
                    Console.WriteLine($"Lambda: {lambda}");
                    var simulation = new StandardMM1Queue(lambda, time, mi);

                    simulation.StartSimulation(iterations);
                    lambda += 0.1;
                    lambda = Math.Round(lambda, 1);
                }
                Process.Start("Notepad.exe",$"Wyniki2.txt");
                Process.Start("Notepad.exe", $"Delays.txt");
            }
            catch(Exception e)
            {
                Console.WriteLine("Error", e);
            }

        }
    }
}
