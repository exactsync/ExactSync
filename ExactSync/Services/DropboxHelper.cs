using ExactSync.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace ExactSync.Services
{
    public static class DropboxHelper
    {
        public static string ApiEndpoint = "https://api.dropbox.com";
        public static string Delta = "/1/delta";

        public static async Task<DeltaModel> DeltaQuery(string accessToken, string cursor = null, string tokenType = "Bearer", HttpClient client = null)
        {
            // Null-coalescing operator
            var httpClient = client ?? new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);

            DeltaModel deltaModel = null;

            try
            {
                var resquest = new HttpRequestMessage(HttpMethod.Post, String.Concat(ApiEndpoint, Delta));

                if (!String.IsNullOrEmpty(cursor))
                {
                    var content = new FormUrlEncodedContent(
                        new Dictionary<string, string> 
                        {
                            { "cursor", cursor }
                        });
                    resquest.Content = content;
                }

                var response = await httpClient.SendAsync(resquest);
                var jsonData = await response.Content.ReadAsStringAsync();

                deltaModel = JsonConvert.DeserializeObject<DeltaModel>(jsonData);

                // Logging
                AuditLogService.Log(Level.Info, EventType.DropboxAPI, EventAction.RequestDelta, jsonData);
            }
            catch (Exception ex)
            {
                AuditLogService.Log(Level.Error, EventType.DropboxAPI, EventAction.RequestDelta, ex.Message);
            }
            finally
            {
                // Dispose local new instance only
                if (client == null)
                {
                    httpClient.Dispose();
                }
            }

            return deltaModel;
        }
    }
}