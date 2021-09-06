using Athenanet_api.Model;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Athenanet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AthenanetController : ControllerBase
    {
        static HttpClient client = new HttpClient();
        string practiceid = "24451";
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] IntakeRequest value)
        {
            value.Firstname = "asdasd";
            value.Lastname = "asdasd";
            value.Dob = "01/01/1980";
            value.Email = "01/01/1980";
            value.Gurantoremail = "01/01/1980";
            value.Ssn = "123456789";

            GetToken(value);
           
        }

        private async Task<string> GetToken(IntakeRequest value)
        {

            var token = "";
            try
            {
                var _httpClient = new HttpClient();

                string requestURL = "https://api.preview.platform.athenahealth.com/oauth2/v1/token";

                var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                  { "client_id", "0oa6bq032dQ8DcrwD297" },
                  { "client_secret", "Vd5EJTpx4QD92cmTW3B_7uwPbYt2JEHAg8jfVTA6" },
                  { "grant_type", "client_credentials" },
                  { "scope", "athena/service/Athenanet.MDP.*" },
                 });


                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(requestURL))
                {
                    Content = content
                };


                using (var response = await _httpClient.SendAsync(httpRequestMessage))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseStream = response.Content.ReadAsStringAsync();
                        var r = JToken.Parse(responseStream.Result);
                        token = r["access_token"].Value<string>();

                        if (token != "")
                        {
                            Department _dept = GetDepartment(practiceid, token);

                            // Once get department then get open slots
                            Appointment appt=  GetOpenSlots(practiceid, _dept, token);

                            //Create Patient
                            CreatePatient(value, practiceid, _dept.departmentid, appt.appointmentid, token);
                        }

                        return token;
                    }
                    else
                    {
                        return token;
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="practiceid"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Department GetDepartment(string practiceid, string token)
        {
            Department deptresult = new Department();
            try
            {
                var url = "https://api.preview.platform.athenahealth.com/v1/" + practiceid + "/departments";

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                httpRequest.Accept = "application/json";
                httpRequest.Headers["Authorization"] = "Bearer "+ token;

               
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    DepartmentRoot depts = JsonConvert.DeserializeObject<DepartmentRoot>(result);
                    // we only need department which id is 1.
                    deptresult = depts.departments.Where(p => p.departmentid == "1").FirstOrDefault();

                }
                
                return deptresult;

            }
            catch (Exception ex)
            {
                return deptresult;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="practiceid"></param>
        /// <param name="departmentid"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Appointment GetOpenSlots(string practiceid, Department dept, string token)
        {
            Appointment apptresult = new Appointment();
            try
            {
                var url = "https://api.preview.platform.athenahealth.com/v1/" + practiceid + "/appointments/open?practiceid="+practiceid+"&departmentid="+ dept.departmentid + "&reasonid=-1";

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                httpRequest.Accept = "application/json";
                httpRequest.Headers["Authorization"] = "Bearer " + token;


                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    AppointmentRoot appt = JsonConvert.DeserializeObject<AppointmentRoot>(result);
                    // we only need department which id is 1.
                    apptresult = appt.appointments.FirstOrDefault();
                    //Take 1st appt/


                }

                return apptresult;

            }
            catch (Exception ex)
            {
                return apptresult;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="practiceid"></param>
        /// <param name="dept"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Patient> CreatePatient(IntakeRequest request, string practiceid, string deptId,int  appointmentid, string token)
        {
            Patient apptresult = new Patient();
            try
            {
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("firstname", request.Firstname));
                postData.Add(new KeyValuePair<string, string>("lastname", request.Lastname));
                postData.Add(new KeyValuePair<string, string>("departmentid", deptId));
                postData.Add(new KeyValuePair<string, string>("dob", request.Dob));
                postData.Add(new KeyValuePair<string, string>("email", request.Email));
                postData.Add(new KeyValuePair<string, string>("guarantoremail", request.Gurantoremail));
                postData.Add(new KeyValuePair<string, string>("ssn", request.Ssn));

                string url = "https://api.preview.platform.athenahealth.com/v1/"+practiceid+"/patients";
                using (var httpClient = new HttpClient())
                {
                    using (var content = new FormUrlEncodedContent(postData))
                    {
                        content.Headers.Clear();
                        //content.Headers.add(["Authorization"] = "Bearer " + token;
                        httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                        HttpResponseMessage response = await httpClient.PostAsync(url, content);

                        var result = response.Content.ReadAsStringAsync();

                        List<Patient> patient = JsonConvert.DeserializeObject<List<Patient>>(result.Result.ToString());

                        if (patient[0] != null)
                        {
                            //Boom Appt.
                            await BookAppointment(request, patient[0].patientid, deptId, appointmentid, token);
                        }
                    }
                }

                return apptresult;

            }
            catch (Exception ex)
            {
                return apptresult;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="practiceid"></param>
        /// <param name="deptId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Patient> BookAppointment(IntakeRequest request, string patientid, string deptId,int appointmentid, string token)
        {
            Patient apptresult = new Patient();
            try
            {
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("patientid", patientid));
                postData.Add(new KeyValuePair<string, string>("appointmenttypeid", "61"));
                postData.Add(new KeyValuePair<string, string>("appointmentid", appointmentid.ToString()));
                postData.Add(new KeyValuePair<string, string>("departmentid", deptId));
                postData.Add(new KeyValuePair<string, string>("ignoreschedulablepermission", "true"));
                
                string url = "https://api.preview.platform.athenahealth.com/v1/"+practiceid+"/appointments/"+ appointmentid;
                using (var httpClient = new HttpClient())
                {
                    using (var content = new FormUrlEncodedContent(postData))
                    {
                        content.Headers.Clear();
                        //content.Headers.add(["Authorization"] = "Bearer " + token;
                        httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                        HttpResponseMessage response = await httpClient.PostAsync(url, content);

                        var result = response.Content.ReadAsStringAsync();

                        Patient patient = JsonConvert.DeserializeObject<Patient>(result.ToString());

                        if (patient != null)
                        {
                            //Boom Appt.
                        }
                    }
                }

                return apptresult;

            }
            catch (Exception ex)
            {
                return apptresult;
            }

        }
    }



    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }


        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }


    }
}
