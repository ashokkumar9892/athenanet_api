using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
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
            GetToken();
            //string practiceid = "24451";
            //string token = "YQWa3E7k5xrDCKm2bqREtKbQ9yud";
            //GetDepartment(practiceid, token);

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
        public string GetToken1212()
        {

            var client = new RestClient("https://api.preview.platform.athenahealth.com/oauth2/token");
            client.Authenticator = new HttpBasicAuthenticator("0oa6bq032dQ8DcrwD297", "Vd5EJTpx4QD92cmTW3B_7uwPbYt2JEHAg8jfVTA6");
            var request = new RestRequest("api/oauth2/token", Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{ \"grant_type\":\"client_credentials\" }",
            ParameterType.RequestBody);
            var responseJson = client.Execute(request).Content;
            var token = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)["access_token"].ToString();
            if (token.Length == 0)
            {
               // throw new AuthenticationException("API authentication failed.");
            }
            return token;

            //String id = "0oa6bq032dQ8DcrwD297";
            //String secret = "Vd5EJTpx4QD92cmTW3B_7uwPbYt2JEHAg8jfVTA6";

            //var client = new RestClient("https://api.preview.platform.athenahealth.com/oauth2/token");
            //var request = new RestRequest(Method.POST);
            //request.AddHeader("cache-control", "no-cache");
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            //request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&scope=all&client_id=" + id + "&client_secret=" + secret, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);

            //dynamic resp = JObject.Parse(response.Content);
            //String token = resp.access_token;


            //string baseAddress = @"https://api.preview.platform.athenahealth.com/oauth2/token";

            //string grant_type = "client_credentials";
            //string client_id = "0oa6bq032dQ8DcrwD297";
            //string client_secret = "Vd5EJTpx4QD92cmTW3B_7uwPbYt2JEHAg8jfVTA6";
            //string client_scope = "athena/service/Athenanet.MDP.*";

            //var form = new Dictionary<string, string>
            //    {
            //        {"grant_type", grant_type},
            //        {"client_id", client_id},
            //        {"client_secret", client_secret},
            //        {"client_scope", client_scope}
            //    };

            //HttpResponseMessage tokenResponse = await client.PostAsync(baseAddress, new FormUrlEncodedContent(form));
            //var jsonContent = await tokenResponse.Content.ReadAsStringAsync();
            ////Token tok = JsonConvert.DeserializeObject<Token>(jsonContent);
            return "";
        }

        private  string GetToken()
        {
            string wClientId = "0oa6bq032dQ8DcrwD297";
            string wClientSecretKey = "Vd5EJTpx4QD92cmTW3B_7uwPbYt2JEHAg8jfVTA6";
            string wAccessToken;

            //--------------------------- Approch-1 to get token using HttpClient -------------------------------------------------------------------------------------
            HttpResponseMessage responseMessage;
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.preview.platform.athenahealth.com/oauth2/token");
                HttpContent httpContent = new FormUrlEncodedContent(
                        new[]
                        {
                                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        });
                tokenRequest.Content = httpContent;
                tokenRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(wClientId + ":" + wClientSecretKey)));
                responseMessage = client.SendAsync(tokenRequest).Result;
            }
            string ResponseJSON = responseMessage.Content.ReadAsStringAsync().Result;


            //--------------------------- Approch-2 to get token using HttpWebRequest and deserialize json object into ResponseModel class -------------------------------------------------------------------------------------


            byte[] byte1 = Encoding.ASCII.GetBytes("grant_type=client_credentials");

            HttpWebRequest oRequest = WebRequest.Create("https://localhost:1001/oauth/token") as HttpWebRequest;
            oRequest.Accept = "application/json";
            oRequest.Method = "POST";
            oRequest.ContentType = "application/x-www-form-urlencoded";
            oRequest.ContentLength = byte1.Length;
            oRequest.KeepAlive = false;
            oRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(wClientId + ":" + wClientSecretKey)));
            Stream newStream = oRequest.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);

            WebResponse oResponse = oRequest.GetResponse();

            using (var reader = new StreamReader(oResponse.GetResponseStream(), Encoding.UTF8))
            {
                var oJsonReponse = reader.ReadToEnd();
                ResponseModel oModel = JsonConvert.DeserializeObject<ResponseModel>(oJsonReponse);
                wAccessToken = oModel.access_token;
            }

            return wAccessToken;
        }
    
    public async Task<IActionResult> GetDepartment(string practiceid, string token)
        {
            try
            {
                //client.BaseAddress = new Uri("https://api.preview.platform.athenahealth.com/v1/"+practiceid+"/departments");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Bearer", token);

                var stringTask = client.GetStringAsync("https://api.preview.platform.athenahealth.com/v1/"+practiceid+"/departments");

                var msg = await stringTask;
                return Ok(msg);

            }
            catch (Exception ex)
            {
                return Ok("Failed");
            }
           
        }
    }

    public class ResponseModel
    {
        public string scope { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string access_token { get; set; }
    }

    internal class Token
    {
        [Newtonsoft.Json.JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
