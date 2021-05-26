using OAST_MM1.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OAST_MM1.Queues
{
    public class StandardMM1Queue
    {
        private const int MI = 8;
        private double LAMBDA = 0.5;
        private double timeBetweenIncidents = 0;
        private double simulationTime = 0;
        private double maxSimulationTime = 50000;
        private double totalServiceTime = 0;
        private int lastIncidentIndex = 0;
        private int numberOfServedIncidents = 0;

        private Random random = new Random();
        private IncidentsList incidentsList = new IncidentsList();
        private List<double> timesOfServiceList = new List<double>();

        public double estimatedDelay;
        public int allServedIncidents;
        public double maxDelay;
        public double minDelay;

        public StandardMM1Queue(double _lambda, double _simulationTime)
        {
            LAMBDA = _lambda;
            maxSimulationTime = _simulationTime;
        }

        private double GenerateServiceTime()
        {
            var u = random.NextDouble();
            var serviceTime = -Math.Log(1 - u) / MI;
            return serviceTime;
        }

        private double GenerateArrivalTime()
        {
            var u = random.NextDouble();
            var arrivalTime = -Math.Log(1 - u) / LAMBDA;
            return arrivalTime;
        }

        public void StartSimulation(int numberOfIterations)
        {
            var calculatedDelay = 0.0;
            var minDelay = Double.MaxValue;
            var maxDelay = Double.MinValue;

            for (int i = 0; i < numberOfIterations; i++)
            {
                totalServiceTime = 0;
                simulationTime = 0;
                numberOfServedIncidents = 0;
                incidentsList = new IncidentsList();
                timesOfServiceList = new List<double>();
                timeBetweenIncidents = 0;

                var newIncident = new Incident()
                {
                    arrivalTime = timeBetweenIncidents,
                    serviceTime = GenerateServiceTime(),
                    nextTime = GenerateArrivalTime(),
                    incidentType = IncidentType.Arrival
                };

                incidentsList.PutIncident(newIncident);

                // Symulację zaczynamy od dodania pierwszego zdarzenia dla czasu przyjścia równego 0
                while (simulationTime <= maxSimulationTime)
                {
                    

                    //lastIncidentIndex = incidentsList.Incidents.Count - 1;  // zmienna przechowująca indeks ostatniego elementu na liście

                    //
                    //timeBetweenIncidents += incidentsList.Incidents[lastIncidentIndex].nextTime;

                    if (incidentsList.Incidents[0].arrivalTime <= simulationTime)   // Jeżeli pakiet dociera i aktualny czas symulacji jest większy lub równy to zostaje obsłużony
                    {
                        var incidentToServe = incidentsList.GetIncident();          // Pobieramy zdarzenie z listy

                        simulationTime += incidentToServe.serviceTime;              // aktualny czas symulacji

                        if (simulationTime >= 120)                                  // Rozpęd 120 sekund
                        {
                            totalServiceTime += simulationTime - incidentToServe.arrivalTime;          // całkowity czas obsługi zdarzeń
                            numberOfServedIncidents++;

                            timesOfServiceList.Add(simulationTime - incidentToServe.arrivalTime);      // Dodaj aktualny czas do listy czasów
                        }

                        // Czas przyjścia następnego pakietu to czas przyjścia poprzedniego + losowy czas nextTime (po jakim czasie przyjdzie następny)
                        timeBetweenIncidents += incidentToServe.nextTime;   

                        newIncident = new Incident()
                        {
                            arrivalTime = timeBetweenIncidents,
                            serviceTime = GenerateServiceTime(),
                            nextTime = GenerateArrivalTime(),
                            incidentType = IncidentType.Arrival
                        };

                        incidentsList.PutIncident(newIncident);     // Jako, że obsłużyliśmy zdarzenie to dodajemy kolejne
                    }
                    else
                    {
                        simulationTime = incidentsList.Incidents[0].arrivalTime;    // aktualny czas symulacji
                    }
                }
                calculatedDelay += totalServiceTime / numberOfServedIncidents;       // dodajemy do zmiennej obliczone opóźnienie
                allServedIncidents += numberOfServedIncidents;
                var delayValues = CalculateEdgeServiceValues(timesOfServiceList);

                if (minDelay > delayValues[0])
                {
                    minDelay = delayValues[0];
                }
                if (maxDelay < delayValues[1])
                {
                    maxDelay = delayValues[1];
                }
            }

            estimatedDelay = calculatedDelay / numberOfIterations;          // Wyestymowane opóźnienie na podstawie ilości iteracji
            allServedIncidents = allServedIncidents / numberOfIterations;   // Ile średnio było obsłużonych pakietów
            var ET = 1 / (MI - LAMBDA);                                     // Obliczone teoretyczne opóźnienie

            WriteToFile(minDelay, maxDelay, ET, numberOfIterations);
        }

        private void WriteToFile(double minDelay, double maxDelay, double ET, int numberOfIterations)
        {
            var path = $"Wyniki.txt";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine($"Mi: {MI} || Lambda: {LAMBDA} || Simulation Time: {maxSimulationTime} || Number of iterations: {numberOfIterations}");
                    sw.WriteLine($"Theoritical delay: {ET}");
                    sw.WriteLine($"Estimated delay: {estimatedDelay}");
                    sw.WriteLine($"Max delay: {maxDelay}");
                    sw.WriteLine($"Min delay: {minDelay}");
                    sw.WriteLine($"Estimated number of packets served: {allServedIncidents}\n");
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine($"Mi: {MI} || Lambda: {LAMBDA} || Simulation Time: {maxSimulationTime} || Number of iterations: {numberOfIterations}");
                    sw.WriteLine($"Theoritical delay: {ET}");
                    sw.WriteLine($"Estimated delay: {estimatedDelay}");
                    sw.WriteLine($"Max delay: {maxDelay}");
                    sw.WriteLine($"Min delay: {minDelay}");
                    sw.WriteLine($"Estimated number of packets served: {allServedIncidents}\n");
                }
            }
            Console.WriteLine($"ET: {ET}\nx: {estimatedDelay}");
        }

        private double[] CalculateEdgeServiceValues(List<double> timesOfService)
        {
            double minDelay = Double.MaxValue;
            double maxDelay = Double.MinValue;

            foreach (var time in timesOfService)
            {
                if (minDelay > time)
                {
                    minDelay = time;
                }
                if (maxDelay < time)
                {
                    maxDelay = time;
                }
            }

            double[] results = new double[2];

            results[0] = minDelay;
            results[1] = maxDelay;

            return results;
        }
    }
}
