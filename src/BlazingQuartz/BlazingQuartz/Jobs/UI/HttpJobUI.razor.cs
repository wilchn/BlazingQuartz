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

        private DataMapValue DataMapUrl = new(DataMapValueType.InterpolatedString, 1);
        private DataMapValue DataMapHeaders = new(DataMapValueType.InterpolatedString, 1);
        private DataMapValue DataMapParameters = new DataMapValue(DataMapValueType.InterpolatedString, 1);
        private string? HttpAction { get; set; }
        private bool IgnoreSsl { get; set; }
        private int? TimeoutInSec { get; set; }

        protected override void OnInitialized()
        {
            if (JobDataMap.ContainsKey(HttpJob.PropertyRequestAction))
            {
                HttpAction = Convert.ToString(JobDataMap[HttpJob.PropertyRequestAction], CultureInfo.InvariantCulture);
            }
            if (JobDataMap.ContainsKey(HttpJob.PropertyRequestUrl))
            {
                DataMapUrl = DataMapValue.Create(JobDataMap[HttpJob.PropertyRequestUrl],
                    DataMapValueType.InterpolatedString, 1);
            }
            if (JobDataMap.ContainsKey(HttpJob.PropertyRequestHeaders))
            {
                DataMapHeaders = DataMapValue.Create(JobDataMap[HttpJob.PropertyRequestHeaders],
                    DataMapValueType.InterpolatedString, 1);
            }
            if (JobDataMap.ContainsKey(HttpJob.PropertyRequestParameters))
            {
                DataMapParameters = DataMapValue.Create(JobDataMap[HttpJob.PropertyRequestParameters],
                    DataMapValueType.InterpolatedString, 1);
            }
            if (JobDataMap.ContainsKey(HttpJob.PropertyIgnoreVerifySsl))
            {
                IgnoreSsl = Convert.ToBoolean(JobDataMap[HttpJob.PropertyIgnoreVerifySsl]);
            }
            if (JobDataMap.ContainsKey(HttpJob.PropertyRequestTimeoutInSec))
            {
                TimeoutInSec = Convert.ToInt32(JobDataMap[HttpJob.PropertyRequestTimeoutInSec]);
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

            if (DataMapUrl.Value == null)
            {
                JobDataMap.Remove(HttpJob.PropertyRequestUrl);
            }
            else
            {
                JobDataMap[HttpJob.PropertyRequestUrl] = DataMapUrl.ToString();
            }

            if (DataMapHeaders.Value == null)
            {
                JobDataMap.Remove(HttpJob.PropertyRequestHeaders);
            }
            else
            {
                JobDataMap[HttpJob.PropertyRequestHeaders] = DataMapHeaders.ToString();
            }

            if (DataMapParameters.Value == null)
            {
                JobDataMap.Remove(HttpJob.PropertyRequestParameters);
            }
            else
            {
                JobDataMap[HttpJob.PropertyRequestParameters] = DataMapParameters.ToString();
            }

            if (!IgnoreSsl)
            {
                JobDataMap.Remove(HttpJob.PropertyIgnoreVerifySsl);
            }
            else
            {
                JobDataMap[HttpJob.PropertyIgnoreVerifySsl] = IgnoreSsl.ToString();
            }

            if (!TimeoutInSec.HasValue)
            {
                JobDataMap.Remove(HttpJob.PropertyRequestTimeoutInSec);
            }
            else
            {
                JobDataMap[HttpJob.PropertyRequestTimeoutInSec] = TimeoutInSec.Value.ToString();
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

