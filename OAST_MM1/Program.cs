using OAST_MM1.Objects;
using OAST_MM1.Queues;
using System;

namespace OAST_MM1
{
    class Program
    {
        static void Main(string[] args)
        {
            double lambda = 0.5;
            double time = 15000;

            Console.WriteLine("Podaj lambdę");
            lambda = Double.Parse(Console.ReadLine());

            for (int i = 0; i < 11; i++)
            {
                Console.WriteLine($"Lambda: {lambda}");
                var simulation = new StandardMM1Queue(lambda, time);

                simulation.StartSimulation(20);
                lambda += 0.1;
                lambda = Math.Round(lambda, 1);
            }
        }
    }
}
