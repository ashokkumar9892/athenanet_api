using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athenanet_api.Model
{
    public class Department
    {
        public bool medicationhistoryconsent { get; set; }
        public int timezoneoffset { get; set; }
        public bool ishospitaldepartment { get; set; }
        public string state { get; set; }
        public string portalurl { get; set; }
        public string city { get; set; }
        public bool placeofservicefacility { get; set; }
        public bool servicedepartment { get; set; }
        public string fax { get; set; }
        public bool doesnotobservedst { get; set; }
        public string departmentid { get; set; }
        public string address { get; set; }
        public string placeofservicetypeid { get; set; }
        public string clinicals { get; set; }
        public int timezone { get; set; }
        public string patientdepartmentname { get; set; }
        public string chartsharinggroupid { get; set; }
        public string name { get; set; }
        public string placeofservicetypename { get; set; }
        public string phone { get; set; }
        public string clinicalproviderfax { get; set; }
        public List<object> ecommercecreditcardtypes { get; set; }
        public string zip { get; set; }
        public string timezonename { get; set; }
    }

    public class DepartmentRoot
    {
        public int totalcount { get; set; }
        public List<Department> departments { get; set; }
    }

}
