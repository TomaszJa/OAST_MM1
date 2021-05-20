using OAST_MM1.Objects;
using OAST_MM1.Queues;
using System;

namespace OAST_MM1
{
    class Program
    {
        static void Main(string[] args)
        {
            var simulation = new StandardMM1Queue();

            simulation.StartSimulation();
        }
    }
}
