using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athenanet_api.Model
{
    public class Appointment
    {
        public string date { get; set; }
        public int appointmentid { get; set; }
        public int departmentid { get; set; }
        public int localproviderid { get; set; }
        public string appointmenttype { get; set; }
        public int providerid { get; set; }
        public string starttime { get; set; }
        public int duration { get; set; }
        public int appointmenttypeid { get; set; }
        public List<string> reasonid { get; set; }
        public string patientappointmenttypename { get; set; }
    }

    public class AppointmentRoot
    {
        public string next { get; set; }
        public int totalcount { get; set; }
        public List<Appointment> appointments { get; set; }
    }
}
