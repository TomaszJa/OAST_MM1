using System;
using System.Collections.Generic;
using System.Text;

namespace OAST_MM1.Objects
{
    public class Incident
    {
        public IncidentType incidentType = IncidentType.Arrival;
        public double arrivalTime;
        public double startServiceTime;
        public double serviceTime;
        public double nextTime;
        public double finishTime;
    }
}
