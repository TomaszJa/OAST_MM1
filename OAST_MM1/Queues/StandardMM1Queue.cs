using OAST_MM1.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace OAST_MM1.Queues
{
    public class StandardMM1Queue
    {
        private const int MI = 8;
        private double LAMBDA = 0.25;
        private double timeBetweenIncidents;
        private double finishTimeOfLastIncident;

        private Random random = new Random();
        private IncidentsList incidentsList = new IncidentsList();

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
            var firstIncident = new Incident()
            {
                arrivalTime = 0,
                startServiceTime = 0,
                serviceTime = GenerateServiceTime(),
                nextTime = GenerateArrivalTime(),
                incidentType = IncidentType.Arrival
            };
            firstIncident.finishTime = firstIncident.serviceTime;

            incidentsList.PutIncident(firstIncident);
            var lastIncidentIndex = incidentsList.Incidents.Count - 1;
            timeBetweenIncidents = incidentsList.Incidents[lastIncidentIndex].nextTime;
            finishTimeOfLastIncident = incidentsList.Incidents[lastIncidentIndex].finishTime;

            int i = 0;
            while (i < 10)
            {
                i++;

                var newIncident = new Incident()
                {
                    arrivalTime = timeBetweenIncidents,
                    startServiceTime = Math.Max(finishTimeOfLastIncident, timeBetweenIncidents),
                    serviceTime = GenerateServiceTime(),
                    nextTime = GenerateArrivalTime() + timeBetweenIncidents,
                    incidentType = IncidentType.Arrival
                };
                newIncident.finishTime = newIncident.startServiceTime + newIncident.serviceTime;

                incidentsList.PutIncident(newIncident);

                // Czas przyjścia następnego pakietu to czas przyjścia poprzedniego + losowy czas nextTime (po jakim czasie przyjdzie następny)
                lastIncidentIndex = incidentsList.Incidents.Count - 1;
                timeBetweenIncidents = incidentsList.Incidents[lastIncidentIndex].nextTime;
                finishTimeOfLastIncident = incidentsList.Incidents[lastIncidentIndex].finishTime;

                Console.WriteLine(newIncident.arrivalTime);
            }
        }
    }
}
