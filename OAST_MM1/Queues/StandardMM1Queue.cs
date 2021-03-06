using OAST_MM1.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OAST_MM1.Queues
{
    public class StandardMM1Queue
    {
        private double MI = 8;
        private double LAMBDA = 0.5;
        private double timeBetweenIncidents = 0;
        private double simulationTime = 0;
        private double maxSimulationTime = 50000;
        private double totalServiceTime = 0;
        private double totalIdleTime = 0;
        private int numberOfServedIncidents = 0;
        private int numberOfIdleMoments = 0;
        private string path = "./Wyniki/Wyniki.txt";
        private string pathExcel = "./Wyniki/Wyniki_Excel.txt";
        private string pathDelays = "./Wyniki/Delays.txt";

        private Random random = new Random();
        private IncidentsList incidentsList = new IncidentsList();
        private List<double> timesOfServiceList = new List<double>();
        private List<double> delaysList = new List<double>();

        public double estimatedDelay;
        public int allServedIncidents;
        public int allIdleMoments;
        public double maxDelay;
        public double minDelay;
        public double confidence;
        public double deviation;
        public string delayValues;

        public StandardMM1Queue(double _lambda, double _simulationTime)
        {
            LAMBDA = _lambda;
            maxSimulationTime = _simulationTime;
        }

        public StandardMM1Queue(double _lambda, double _simulationTime, double _mi)
        {
            LAMBDA = _lambda;
            maxSimulationTime = _simulationTime;
            MI = _mi;
        }

        private double GenerateServiceTime()
        {
            double u = random.NextDouble();
            double serviceTime = -Math.Log(1 - u) / MI;
            return serviceTime;
        }

        private double GenerateArrivalTime()
        {
            double u = random.NextDouble();
            double arrivalTime = -Math.Log(1 - u) / LAMBDA;
            return arrivalTime;
        }

        public void StartSimulation(int numberOfIterations)
        {
            double allCalculatedDelays = 0.0;
            double minDelay = Double.MaxValue;
            double maxDelay = Double.MinValue;
            double allCalculatedIdleMoments = 0.0;

            int completed = (int)Math.Floor((double)(numberOfIterations / 40));
            int step = completed;

            for (int i = 0; i < numberOfIterations; i++)
            {
                if (completed != 0)
                    step = ProgressBar(numberOfIterations, completed, i, step);
                totalServiceTime = 0;
                totalIdleTime = 0;
                simulationTime = 0;
                numberOfServedIncidents = 0;
                numberOfIdleMoments = 0;
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
                    if (incidentsList.Incidents[0].arrivalTime <= simulationTime)   // Jeżeli pakiet dociera i aktualny czas symulacji jest większy lub równy to zostaje obsłużony
                    {
                        var incidentToServe = incidentsList.GetIncident();          // Pobieramy zdarzenie z listy

                        simulationTime += incidentToServe.serviceTime;              // aktualny czas symulacji

                        if (simulationTime >= 120)                                  // Rozpęd 120 sekund
                        {
                            //PacketServiceOAST(ref minDelay, ref maxDelay, incidentToServe);
                            PacketServiceMPI(ref minDelay, ref maxDelay, incidentToServe);
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
                        if (simulationTime >= 120)                                  // Rozpęd 120 sekund
                        {
                            totalIdleTime += incidentsList.Incidents[0].arrivalTime - simulationTime;  // czas idle to czas gdy system czeka na pakiet
                            numberOfIdleMoments++;
                        }
                        simulationTime = incidentsList.Incidents[0].arrivalTime;    // aktualny czas symulacji
                    }
                }
                allCalculatedDelays += totalServiceTime / numberOfServedIncidents; // dodajemy do zmiennej obliczone opóźnienie z tej próby
                allCalculatedIdleMoments += totalIdleTime / numberOfServedIncidents;    // średni czas idle na pakiet
                allServedIncidents += numberOfServedIncidents;
                allIdleMoments += numberOfIdleMoments;

                delaysList.Add(totalServiceTime / numberOfServedIncidents);             // Lista z opóźnieniami do liczenia przedziałów ufności
            }
            if (completed != 0)
                ProgressBar(numberOfIterations, completed, numberOfIterations, step);

            estimatedDelay = allCalculatedDelays / numberOfIterations;          // Wyestymowane opóźnienie na podstawie ilości iteracji
            allServedIncidents = allServedIncidents / numberOfIterations;   // Ile średnio było obsłużonych pakietów
            allIdleMoments = allIdleMoments / numberOfIterations;
            double ET = 1 / (MI - LAMBDA);                                     // Obliczone teoretyczne opóźnienie
            double estimatedIdle = allCalculatedIdleMoments / numberOfIterations;

            ConfidenceInterval(estimatedDelay, numberOfIterations);
            NormalDistribution();
            WriteToFile(minDelay, maxDelay, ET, numberOfIterations, estimatedIdle);
        }

        private void PacketServiceOAST(ref double minDelay, ref double maxDelay, Incident incidentToServe)
        {
            double serviceTime = simulationTime - incidentToServe.arrivalTime;
            totalServiceTime += serviceTime;                        // całkowity czas obsługi zdarzeń
            numberOfServedIncidents++;

            timesOfServiceList.Add(serviceTime);      // Dodaj aktualny czas do listy czasów

            if (minDelay > serviceTime)
            {
                minDelay = serviceTime;
            }
            if (maxDelay < serviceTime)
            {
                maxDelay = serviceTime;
            }
        }

        private void PacketServiceMPI(ref double minDelay, ref double maxDelay, Incident incidentToServe)
        {
            double serviceTime = incidentToServe.serviceTime;
            totalServiceTime += serviceTime;                        // całkowity czas obsługi zdarzeń
            numberOfServedIncidents++;

            timesOfServiceList.Add(serviceTime);      // Dodaj aktualny czas do listy czasów

            if (minDelay > serviceTime)
            {
                minDelay = serviceTime;
            }
            if (maxDelay < serviceTime)
            {
                maxDelay = serviceTime;
            }
        }

        private int ProgressBar(int numberOfIterations, int completed, int i, int x)
        {
            int progressBar = x / completed;
            if (i == 0)
            {
                Console.Clear();
                Console.WriteLine($"Lambda: {LAMBDA} MI: {MI} Number of iterations: {numberOfIterations} Czas symulacji: {maxSimulationTime}");
                Console.Write("Progress: [");
                for (int a = 0; a < 40; a++)
                {
                    Console.Write(" ");
                }
                Console.Write("]");
            }
            if (i == x)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"Lambda: {LAMBDA} MI: {MI} Number of iterations: {numberOfIterations} Czas symulacji: {maxSimulationTime}");
                Console.Write("Progress: [");
                for (int a = 0; a < progressBar; a++)
                {
                    Console.Write("|");
                }
                for (int a = 0; a < 40 - progressBar; a++)
                {
                    Console.Write(" ");
                }
                double percentage = (double)progressBar / 40;
                Console.Write($"] {Math.Truncate(percentage * 100)}%\n");
                x += completed;
            }
            return x;
        }

        // Funkcja do liczenia przedziału ufności
        private void ConfidenceInterval(double averageDelay, int numberOfIterations)
        {
            delayValues = "";
            double sumOfDelaysDifferences = 0.0;
            double u = 1.96;

            foreach (var delay in delaysList)
            {
                double x = Math.Pow(delay - averageDelay, 2);
                sumOfDelaysDifferences += x;
            }

            double y = sumOfDelaysDifferences / numberOfIterations;        // Odchylenie standardowe z próby przed pierwiastkiem
            deviation = Math.Sqrt(y);                                   // Odchylenie standardowe z próby

            confidence = (u * deviation) / Math.Sqrt(numberOfIterations);
        }

        // Funkcja do liczenia przedziałów do rozkładu normalnego.
        private void NormalDistribution()
        {
            delaysList.Sort();

            int numberOfValues = delaysList.Count;
            double maxValue = delaysList[numberOfValues - 1];
            double minValue = delaysList[0];

            double difference = maxValue - minValue;
            double step = difference / 20;

            double currentValue = minValue + step;
            double previousValue = minValue;

            // ilość argumentów x na wykresie to 20, bo tak sobie przyjąłęm
            int numberOfValuesPerIteration = numberOfValues / 20;

            for (int i = 0; i < 20; i++)
            {
                List<double> range = delaysList.Where(x => x <= currentValue && x >= minValue).ToList();
                int sum = range.Count;

                delayValues += $"{currentValue}\t{sum}\n";

                minValue += step;
                currentValue += step;
            }
        }

        private void WriteToFile(double minDelay, double maxDelay, double ET, int numberOfIterations, double estimatedIdle)
        {
            using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
            {
                sw.WriteLine($"Mi: {MI} || Lambda: {LAMBDA} || Simulation Time: {maxSimulationTime} || Number of iterations: {numberOfIterations}");
                sw.WriteLine($"Theoritical delay: {ET}");
                sw.WriteLine($"Estimated delay: {estimatedDelay}");
                sw.WriteLine($"Confidence: +/-{confidence}");
                sw.WriteLine($"Deviation: {deviation}");
                sw.WriteLine($"Max delay: {maxDelay}");
                sw.WriteLine($"Min delay: {minDelay}");
                sw.WriteLine($"Estimated number of packets served: {allServedIncidents}");
                sw.WriteLine($"Estimated number of idle moments: {allIdleMoments}");
                sw.WriteLine($"Estimated IdleTime: {estimatedIdle}\n");
            }

            using (StreamWriter sw = (File.Exists(pathExcel)) ? File.AppendText(pathExcel) : File.CreateText(pathExcel))
            {
                sw.WriteLine($"{MI}\t{LAMBDA}\t{estimatedIdle}\t{estimatedDelay}\t{allIdleMoments}\t{allServedIncidents}\t{LAMBDA/MI}\t{1 - LAMBDA/MI}");
            }

            using (StreamWriter sw = File.CreateText(pathDelays))
            {
                sw.WriteLine($"{delayValues}");
            }
            Console.WriteLine($"ET: {ET}\nx: {estimatedDelay}");
        }

    }
}
