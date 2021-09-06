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
           
        }

        private async Task<string> GetToken()
        {

            var result = "";
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
                        result = r["access_token"].Value<string>();

                        if (result != "")
                        {
                            string practiceid = "24451";
                            GetDepartment(practiceid, result);
                        }

                        return result;
                    }
                    else
                    {
                        return result;
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
        public async Task<IActionResult> GetDepartment(string practiceid, string token)
        {
            try
            {
                //client.BaseAddress = new Uri("https://api.preview.platform.athenahealth.com/v1/"+practiceid+"/departments");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Bearer", token);

                var stringTask = client.GetStringAsync("https://api.preview.platform.athenahealth.com/v1/" + practiceid + "/departments");

                var msg = await stringTask;
                return Ok(msg);

            }
            catch (Exception ex)
            {
                return Ok("Failed");
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
