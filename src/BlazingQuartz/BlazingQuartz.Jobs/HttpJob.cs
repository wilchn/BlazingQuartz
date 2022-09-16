using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Quartz;
using static System.Net.Mime.MediaTypeNames;

namespace BlazingQuartz.Jobs
{
    public class HttpJob : IJob
    {
        public const string PropertyRequestAction = "requestAction";
        public const string PropertyRequestUrl = "requestUrl";
        public const string PropertyRequestParameters = "requestParams";
        public const string PropertyRequestHeaders = "requestHeaders";
        public const string PropertyIgnoreVerifySsl = "ignoreSsl";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpJob> _logger;

        public HttpJob(IHttpClientFactory httpClientFactory,
            ILogger<HttpJob> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var data = context.MergedJobDataMap;
                var url = data.GetString(PropertyRequestUrl);
                if (string.IsNullOrEmpty(url))
                {
                    _logger.LogWarning("[{runInstanceId}]. Cannot run HttpJob. No request url specified.",
                        context.FireInstanceId);
                    throw new JobExecutionException("No request url specified");
                }
                url = url.StartsWith("http") ? url : "http://" + url;

                var parameters = data.GetString(PropertyRequestParameters);
                var strHeaders = data.GetString(PropertyRequestHeaders);
                var headers = string.IsNullOrEmpty(strHeaders) ? null :
                    JsonSerializer.Deserialize<Dictionary<string, string>>(strHeaders.Trim());

                var strAction = data.GetString(PropertyRequestAction);
                HttpAction action;
                if (strAction == null)
                {
                    _logger.LogWarning("[{runInstanceId}]. Cannot run HttpJob. No http action specified.",
                        context.FireInstanceId);
                    throw new JobExecutionException("No http action specified");
                }
                action = Enum.Parse<HttpAction>(strAction);

                _logger.LogDebug("[{runInstanceId}]. Creating HttpClient...", context.FireInstanceId);
                HttpClient httpClient;
                if (data.GetBoolean(PropertyIgnoreVerifySsl))
                {
                    httpClient = _httpClientFactory.CreateClient(Constants.HttpClientIgnoreVerifySsl);
                    _logger.LogInformation("[{runInstanceId}]. Created ignore SSL validation HttpClient.",
                        context.FireInstanceId);
                }
                else
                {
                    httpClient = _httpClientFactory.CreateClient();
                    _logger.LogInformation("[{runInstanceId}]. Created HttpClient.",
                        context.FireInstanceId);
                }

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                HttpContent? reqParam = null;
                if (!string.IsNullOrEmpty(parameters))
                    reqParam = new StringContent(parameters, Encoding.UTF8, Application.Json);

                HttpResponseMessage response = new HttpResponseMessage();
                _logger.LogInformation("[{runInstanceId}]. Sending '{action}' request to specified url '{url}'.",
                    context.FireInstanceId, action, url);
                switch (action)
                {
                    case HttpAction.Get:
                        response = await httpClient.GetAsync(url, context.CancellationToken);
                        break;
                    case HttpAction.Post:
                        response = await httpClient.PostAsync(url, reqParam, context.CancellationToken);
                        break;
                    case HttpAction.Put:
                        response = await httpClient.PutAsync(url, reqParam, context.CancellationToken);
                        break;
                    case HttpAction.Delete:
                        response = await httpClient.DeleteAsync(url, context.CancellationToken);
                        break;
                }

                var result = await response.Content.ReadAsStringAsync(context.CancellationToken);
                _logger.LogInformation("[{runInstanceId}]. Response tatus code '{code}'.",
                    context.FireInstanceId, response.StatusCode);
                context.Result = result;
                context.SetIsSuccess(response.IsSuccessStatusCode);
                context.SetReturnCode((int)response.StatusCode);
                context.SetExecutionDetails($"Request: [{response.RequestMessage}]");
            }
            catch (JobExecutionException)
            {
                context.SetIsSuccess(false);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to run HttpJob. [{runInstanceId}]",
                    context.FireInstanceId);
                context.SetIsSuccess(false);
                throw new JobExecutionException("Failed to execute http job", ex);
            }
        }
    }
}

