using OAST_MM1.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace OAST_MM1.Queues
{
    public class StandardMM1Queue
    {
        private const int MI = 8;
        private double LAMBDA = 6;
        private double timeBetweenIncidents = 0;
        private double simulationTime = 0;
        private double maxSimulationTime = 100;
        private double totalServiceTime = 0;
        private int lastIncidentIndex = 0;
        private int numberOfServedIncidents = 0;

        private Random random = new Random();
        private IncidentsList incidentsList = new IncidentsList();
        private List<Incident> incidentsQueue = new List<Incident>();
        private List<double> timesList = new List<double>();
        private List<int> numberOfIncidentsInQueue = new List<int>();

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

        public void StartSimulation()
        {
            // Symulację zaczynamy od dodania pierwszego zdarzenia dla czasu przyjścia równego 0
            while (simulationTime <= maxSimulationTime)
            {
                var newIncident = new Incident()
                {
                    arrivalTime = timeBetweenIncidents,
                    //startServiceTime = Math.Max(finishTimeOfLastIncident, timeBetweenIncidents),
                    serviceTime = GenerateServiceTime(),
                    nextTime = GenerateArrivalTime(),
                    incidentType = IncidentType.Arrival
                };

                incidentsList.PutIncident(newIncident);

                lastIncidentIndex = incidentsList.Incidents.Count - 1;  // zmienna przechowująca indeks ostatniego elementu na liście
                // Czas przyjścia następnego pakietu to czas przyjścia poprzedniego + losowy czas nextTime (po jakim czasie przyjdzie następny)
                timeBetweenIncidents += incidentsList.Incidents[lastIncidentIndex].nextTime;

                foreach (var incident in incidentsList.Incidents)
                {
                    // Jeżeli czas przyjścia nowego zdarzenia jest mniejszy niż aktualny i to zdarzenie jeszcze nie jest wpisane do kolejki czas symulacji to dodaj to zdarzenie do kolejki
                    if (incident.arrivalTime < simulationTime && !incidentsQueue.Contains(incident))   
                    {
                        incidentsQueue.Add(incident);
                    }
                }

                numberOfIncidentsInQueue.Add(incidentsQueue.Count);     // Lista przechowująca ilość zdarzeń w kolejce dla każdej iteracji

                timesList.Add(simulationTime);      // Dodaj aktualny czas do listy czasów

                if (incidentsList.Incidents[0].arrivalTime <= simulationTime)   // Jeżeli pakiet dociera i aktualny czas symulacji jest większy lub równy to zostaje obsłużony
                {
                    var incidentToServe = incidentsList.GetIncident();          // Pobieramy zdarzenie z listy
                    incidentsQueue.Remove(incidentToServe);                     // Usuwamy z kolejki, jeżeli się na niej znajduje
                    numberOfServedIncidents++;

                    totalServiceTime += incidentToServe.serviceTime;            // całkowity czas obsługi zdarzeń

                    simulationTime += incidentToServe.serviceTime;              // aktualny czas symulacji
                }
                else
                {
                    simulationTime = incidentsList.Incidents[0].arrivalTime;    // aktualny czas symulacji
                }
            }
        }
    }
}
