using System;
using System.Globalization;
using BlazingQuartz.Jobs.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BlazingQuartz.Jobs.UI
{
    public partial class HttpJobUI : ComponentBase, IJobUI
    {
        const string JOB_CLASS = "BlazingQuartz.Jobs.HttpJob";

        public string JobClass => JOB_CLASS;

        [Parameter] public bool IsReadOnly { get; set; }
        [Parameter] public IDictionary<string, object> JobDataMap { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private string? Url { get; set; }
        private string? HttpAction { get; set; }
        private string? Headers { get; set; }
        private string? Parameters { get; set; }
        private bool IgnoreSsl { get; set; }

        protected override void OnInitialized()
        {
            if (JobDataMap.ContainsKey(HttpJob.PropertyRequestAction))
            {
                HttpAction = Convert.ToString(JobDataMap[HttpJob.PropertyRequestAction], CultureInfo.InvariantCulture);
            }
            if (JobDataMap.ContainsKey(HttpJob.PropertyRequestUrl))
            {
                Url = Convert.ToString(JobDataMap[HttpJob.PropertyRequestUrl], CultureInfo.InvariantCulture);
            }
            if (JobDataMap.ContainsKey(HttpJob.PropertyRequestHeaders))
            {
                Headers = Convert.ToString(JobDataMap[HttpJob.PropertyRequestHeaders], CultureInfo.InvariantCulture);
            }
            if (JobDataMap.ContainsKey(HttpJob.PropertyRequestParameters))
            {
                Parameters = Convert.ToString(JobDataMap[HttpJob.PropertyRequestParameters], CultureInfo.InvariantCulture);
            }
            if (JobDataMap.ContainsKey(HttpJob.PropertyIgnoreVerifySsl))
            {
                IgnoreSsl = Convert.ToBoolean(JobDataMap[HttpJob.PropertyIgnoreVerifySsl]);
            }
        }

        public Task<bool> ApplyChanges()
        {
            if (HttpAction == null)
            {
                JobDataMap.Remove(HttpJob.PropertyRequestAction);
            }
            else
            {
                JobDataMap[HttpJob.PropertyRequestAction] = HttpAction;
            }

            if (Url == null)
            {
                JobDataMap.Remove(HttpJob.PropertyRequestUrl);
            }
            else
            {
                JobDataMap[HttpJob.PropertyRequestUrl] = Url;
            }

            if (Headers == null)
            {
                JobDataMap.Remove(HttpJob.PropertyRequestHeaders);
            }
            else
            {
                JobDataMap[HttpJob.PropertyRequestHeaders] = Headers;
            }

            if (Parameters == null)
            {
                JobDataMap.Remove(HttpJob.PropertyRequestParameters);
            }
            else
            {
                JobDataMap[HttpJob.PropertyRequestParameters] = Parameters;
            }

            if (!IgnoreSsl)
            {
                JobDataMap.Remove(HttpJob.PropertyIgnoreVerifySsl);
            }
            else
            {
                JobDataMap[HttpJob.PropertyIgnoreVerifySsl] = IgnoreSsl.ToString();
            }

            return Task.FromResult<bool>(true);
        }

        public Task ClearChanges()
        {
            JobDataMap.Remove(HttpJob.PropertyRequestAction);
            JobDataMap.Remove(HttpJob.PropertyRequestUrl);
            JobDataMap.Remove(HttpJob.PropertyRequestHeaders);
            JobDataMap.Remove(HttpJob.PropertyRequestParameters);
            JobDataMap.Remove(HttpJob.PropertyIgnoreVerifySsl);

            return Task.CompletedTask;
        }
    }
}

