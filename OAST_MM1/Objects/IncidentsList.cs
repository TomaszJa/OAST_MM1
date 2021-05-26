using System.Collections.Generic;
using System.Linq;

namespace OAST_MM1.Objects
{
    public class IncidentsList
    {
        public List<Incident> Incidents = new List<Incident>();

        // Pobierz pierwszy incydent z listy
        public Incident GetIncident()
        {
            var incident = Incidents[0];
            Incidents.RemoveAt(0);
            return incident;
        }

        // Dodaj nowy incydent do listy i posortuj listę
        public void PutIncident(Incident newIncident)
        {
            Incidents.Add(newIncident);
            Incidents = SortIncidents(Incidents);
        }

        // Posortuj listę ze zdarzeniami na podstawie czasu przyjścia
        public static List<Incident> SortIncidents(List<Incident> incidents)
        {
            var sortedList = incidents.OrderBy(i => i.arrivalTime).ToList();
            return sortedList;
        }
    }
}
