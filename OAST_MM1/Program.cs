using OAST_MM1.Queues;
using System;

namespace OAST_MM1
{
    class Program
    {
        static void Main(string[] args)
        {
            double lambda = 0.5;
            double time = 1000;

            Console.WriteLine("Podaj lambdę początkową");
            lambda = Double.Parse(Console.ReadLine());

            Console.WriteLine("Podaj lambdę końcową");
            double maxLambda = Double.Parse(Console.ReadLine());

            Console.WriteLine("Podaj czas symulacji");
            time = Double.Parse(Console.ReadLine());

            Console.WriteLine("Podaj liczbę iteracji dla każdej lambdy");
            int iterations = int.Parse(Console.ReadLine());

            while (lambda <= maxLambda)
            {
                Console.WriteLine($"Lambda: {lambda}");
                var simulation = new StandardMM1Queue(lambda, time);

                simulation.StartSimulation(iterations);
                lambda += 0.1;
                lambda = Math.Round(lambda, 1);
            }
        }
    }
}
