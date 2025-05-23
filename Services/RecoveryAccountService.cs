using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using DirectorySite.Data;
using DirectorySite.Models;

namespace DirectorySite.Services
{
    public class RecoveryAccountService(ILogger<RecoveryAccountService> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        private readonly ILogger<RecoveryAccountService> logger = logger;
        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

        public Dictionary<string,string> AccountRecoveryOrderTypes = new Dictionary<string, string>{
            {"createdAt", "Fecha de Registro"},
            {"curp", "Curp"},
            {"contactEmail", "Correo"},
            {"name", "Nombre"}
        };

        private string authToken = string.Empty;


        /// <summary>
        /// return the las account recovery requests
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"> Fail at attempt to get the auth token or id invalid</exception>
        /// <exception cref="ArgumentException">The request is invalid</exception>
        public async Task<RecoveryAccountPaginatorResponse> GetRequest(
            string orderBy = "createdAt",
            bool ascending = false,
            bool excludeConcluded = false,
            bool excludeDeleted = false,
            bool excludePending = false,
            int take = 25,
            int offset = 0
        )
        {
            // * load the authToken if is not loaded
            if(string.IsNullOrEmpty(authToken)){
                RetriveAuthToken();
            }

            // * prepare the parameters
            IEnumerable<string> queryParams = [
                string.Format("take={0}", take),
                string.Format("offset={0}", offset),
                string.Format("orderBy={0}", orderBy),
                string.Format("ascending={0}", ascending),
                string.Format("excludeConcluded={0}", excludeConcluded),
                string.Format("excludeDeleted={0}", excludeDeleted),
                string.Format("excludePending={0}", excludePending),
            ];


            // * prepare the request
            using var httpClient = httpClientFactory.CreateClient("DirectoryAPI");
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(httpClient.BaseAddress!, $"/api/accountRecovery?" + string.Join("&", queryParams)),
                Method = HttpMethod.Get
            };
            httpRequest.Headers.Add("Authorization", $"Bearer {authToken}");
            
            // * send the request
            RecoveryAccountPaginatorResponse? response;
            try
            {
                var httpResponse = await httpClient.SendAsync(httpRequest);
                httpResponse.EnsureSuccessStatusCode();
                response = await httpResponse.Content.ReadFromJsonAsync<RecoveryAccountPaginatorResponse>();
                return response!;
            }
            catch(HttpRequestException httpex)
            {
                this.logger.LogError(httpex, "Fail at get the preregister records: {message}", httpex.Message);
                throw httpex.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => new UnauthorizedAccessException(),
                    HttpStatusCode.BadRequest => new ArgumentException(),
                    _ => new Exception(),
                };
            }
        }

        /// <summary>
        /// return the recovery requests info with files
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"> Fail at attempt to get the auth token or id invalid</exception>
        /// <exception cref="ArgumentException">The request is invalid</exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<RecoveryAccountResponse> GetRequestById(string requestId)
        {
            // * load the authToken if is not loaded
            if(string.IsNullOrEmpty(authToken)){
                RetriveAuthToken();
            }

            // * prepare the request
            using var httpClient = httpClientFactory.CreateClient("DirectoryAPI");
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(httpClient.BaseAddress!, $"/api/accountRecovery/{requestId}"),
                Method = HttpMethod.Get
            };
            httpRequest.Headers.Add("Authorization", $"Bearer {authToken}");
            
            // * send the request
            try
            {
                var httpResponse = await httpClient.SendAsync(httpRequest);
                httpResponse.EnsureSuccessStatusCode();
                return await httpResponse.Content.ReadFromJsonAsync<RecoveryAccountResponse>()
                    ?? throw new KeyNotFoundException("The recovery request was not found");
            }
            catch(HttpRequestException httpex)
            {
                this.logger.LogError(httpex, "Fail at get the preregister records: {message}", httpex.Message);
                throw httpex.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => new UnauthorizedAccessException(),
                    HttpStatusCode.BadRequest => new ArgumentException(),
                    HttpStatusCode.NotFound => new KeyNotFoundException("The recovery request was not found"),
                    _ => new Exception(),
                };
            }
        }

        /// <summary>
        /// return the recovery requests info with files 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"> Fail at attempt to get the auth token or id invalid</exception>
        /// <exception cref="ArgumentException">The request is invalid</exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<int> UpdateTheRequest(string requestId, string comments, bool notifyEmail, int templateId)
        {
            // * load the authToken if is not loaded
            if(string.IsNullOrEmpty(authToken)){
                RetriveAuthToken();
            }

            // * prepare the payload
            var payloadRequest = JsonConvert.SerializeObject( new {
                ResponseComments = comments,
                NotifyEmail = notifyEmail ? 1 :0,
                TemplateId = templateId
            });

            // * prepare the request
            using var httpClient = httpClientFactory.CreateClient("DirectoryAPI");
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(httpClient.BaseAddress!, $"/api/accountRecovery/{requestId}"),
                Method = HttpMethod.Patch,
                Content = new StringContent(payloadRequest, Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Add("Authorization", $"Bearer {authToken}");
            
            // * send the request
            try
            {
                var httpResponse = await httpClient.SendAsync(httpRequest);
                httpResponse.EnsureSuccessStatusCode();
                return 1;
            }
            catch(HttpRequestException httpex)
            {
                this.logger.LogError(httpex, "Fail at get the preregister records: {message}", httpex.Message);
                throw httpex.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => new UnauthorizedAccessException(),
                    HttpStatusCode.BadRequest => new ArgumentException(),
                    HttpStatusCode.NotFound => new KeyNotFoundException("The recovery request was not found"),
                    _ => new Exception(),
                };
            }
        }

        public async Task<IEnumerable<RecoveryAccountTemplate>> GetTemplates()
        {
            var responseList = new List<RecoveryAccountTemplate>();

            // * load the authToken if is not loaded
            if(string.IsNullOrEmpty(authToken)){
                RetriveAuthToken();
            }
            
            // * prepare the request
            using var httpClient = httpClientFactory.CreateClient("DirectoryAPI");
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(httpClient.BaseAddress!, "/api/AccountRecovery/templates"),
                Method = HttpMethod.Get
            };
            httpRequest.Headers.Add("Authorization", $"Bearer {authToken}");
            
            // * send the request
            try
            {
                var httpResponse = await httpClient.SendAsync(httpRequest);
                httpResponse.EnsureSuccessStatusCode();
                
                responseList = await httpResponse.Content.ReadFromJsonAsync<List<RecoveryAccountTemplate>>()
                    ?? throw new KeyNotFoundException("The recovery request was not found");
            }
            catch(HttpRequestException httpex)
            {
                this.logger.LogError(httpex, "Fail at get the preregister records: {message}", httpex.Message);
                throw httpex.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => new UnauthorizedAccessException(),
                    HttpStatusCode.BadRequest => new ArgumentException(),
                    HttpStatusCode.NotFound => new KeyNotFoundException("The recovery request was not found"),
                    _ => new Exception(),
                };
            }

            return responseList;
        }

        public async Task<IEnumerable<RecoveryAccountResponse>> GetRequestFromAPerson(Guid personId, int take = 5,int offset = 0)
        {
            // * load the authToken if is not loaded
            if(string.IsNullOrEmpty(authToken)){
                RetriveAuthToken();
            }

            // * prepare the parameters
            IEnumerable<string> queryParams = [
                string.Format("take={0}", take),
                string.Format("offset={0}", offset)
            ];


            // * prepare the request
            using var httpClient = httpClientFactory.CreateClient("DirectoryAPI");
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(httpClient.BaseAddress!, $"/api/accountRecovery/people/{personId}?" + string.Join("&", queryParams)),
                Method = HttpMethod.Get
            };
            httpRequest.Headers.Add("Authorization", $"Bearer {authToken}");
            
            // * send the request
            PagedResponse<RecoveryAccountResponse>? response;
            try
            {
                var httpResponse = await httpClient.SendAsync(httpRequest);
                httpResponse.EnsureSuccessStatusCode();
                response = await httpResponse.Content.ReadFromJsonAsync<PagedResponse<RecoveryAccountResponse>>();
                return response?.Items ?? [];
            }
            catch(HttpRequestException httpex)
            {
                this.logger.LogError(httpex, "Fail at get the preregister records: {message}", httpex.Message);
                throw httpex.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => new UnauthorizedAccessException(),
                    HttpStatusCode.BadRequest => new ArgumentException(),
                    _ => new Exception(),
                };
            }
        }

        #region private methods

        /// <summary>
        /// load the auth token
        /// </summary>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void RetriveAuthToken()
        {
            try
            {
                this.authToken = httpContextAccessor.HttpContext!.Session.GetString("JWTToken")!;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error at attempting to retrive the auth token: {message}", ex.Message);
                throw new UnauthorizedAccessException();
            }
        }
        #endregion

    }
}