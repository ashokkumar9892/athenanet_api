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

        Department department;
        Appointment appointment; 
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
        public async Task<ActionResult> Post([FromBody] IntakeRequest value)
        {
            department = new Department();
            appointment = new Appointment();
            string result = await GetToken(value);
            return Ok(result);
        }

        private async Task<string> GetToken(IntakeRequest value)
        {

            var token = "";
            string result = "";
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
                           string deptResult = await  GetDepartment(practiceid, token);
                            if (deptResult == "")
                            {

                                // Once get department then get open slots
                                string aptResult  = await GetOpenSlots(practiceid, department, token);

                                if (aptResult == "")
                                {
                                  string patientResult = await  CreatePatient(value, practiceid, department.departmentid, appointment.appointmentid, token);
                                    result = patientResult;
                                    return result;
                                }//Create Patient
                                else
                                {
                                    result = aptResult;
                                    return result;
                                }
                                
                            }
                            else
                            {
                                result = deptResult;
                                return result;
                            }
                        }

                        return result;
                    }
                    else
                    {
                        return token;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
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
        public async Task<string> GetDepartment(string practiceid, string token)
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
                department.departmentid = deptresult.departmentid;
                return "";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="practiceid"></param>
        /// <param name="departmentid"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> GetOpenSlots(string practiceid, Department dept, string token)
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
                    appointment = apptresult;

                }

                return "";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="practiceid"></param>
        /// <param name="dept"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> CreatePatient(IntakeRequest request, string practiceid, string deptId,int  appointmentid, string token)
        {
            Patient patientresult = new Patient();
            string results = "";
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
                        try
                        {
                            List<Patient> patient = JsonConvert.DeserializeObject<List<Patient>>(result.Result.ToString());

                            if (patient[0] != null)
                            {
                                //Boom Appt.
                                string confirmation = await BookAppointment(request, patient[0].patientid, deptId, appointmentid, token);
                                results = confirmation;
                            }
                        }catch(Exception ex)
                        {
                            results = result.Result.ToString();
                            return results;
                        }
                    }
                }

                return results;

            }
            catch (Exception ex)
            {
                return ex.Message;
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
        public async Task<string> BookAppointment(IntakeRequest request, string patientid, string deptId,int appointmentid, string token)
        {
            ApptConfirmation apptresult = new ApptConfirmation();
            string confirmationresult = "";
            try
            {
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("patientid", patientid));
                postData.Add(new KeyValuePair<string, string>("appointmenttypeid", "61"));
                postData.Add(new KeyValuePair<string, string>("appointmentid", appointmentid.ToString()));
                postData.Add(new KeyValuePair<string, string>("departmentid", deptId));
                //postData.Add(new KeyValuePair<string, string>("ignoreschedulablepermission", "true"));
                
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

                        HttpResponseMessage response = await httpClient.PutAsync(url, content);

                        var result = response.Content.ReadAsStringAsync();

                        List<ApptConfirmation> confirmation = JsonConvert.DeserializeObject<List<ApptConfirmation>>(result.Result.ToString());

                        if (confirmation != null)
                        {
                            ApptConfirmation confirm = new ApptConfirmation();
                            confirm = confirmation[0];
                            confirmationresult = "You confirmation Details :" + confirm.date + " Appointmentid: " + confirm.appointmentid + " Appointmentstatus: " + confirm.appointmentstatus; ;
                            return confirmationresult;

                            //Boom Appt.
                        }
                    }
                }

                return confirmationresult;

            }
            catch (Exception ex)
            {
                return ex.Message;
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
