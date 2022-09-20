using System;
using System.Globalization;
using BlazingQuartz.Jobs.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BlazingQuartz.Examples.JobUI
{
    public partial class HelloJobUI : ComponentBase, IJobUI
    {
        public string JobClass => "BlazingQuartz.Examples.JobUI.HelloJob";

        [Parameter] public bool IsReadOnly { get; set; }
        [Parameter] public IDictionary<string, object> JobDataMap { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private string? Message { get; set; }

        protected override void OnInitialized()
        {
            if (JobDataMap.ContainsKey(HelloJob.PropertyMessage))
            {
                Message = Convert.ToString(JobDataMap[HelloJob.PropertyMessage], CultureInfo.InvariantCulture);
            }
        }

        public Task<bool> ApplyChanges()
        {
            if (string.IsNullOrEmpty(Message))
            {
                JobDataMap.Remove(HelloJob.PropertyMessage);
            }
            else
            {
                JobDataMap[HelloJob.PropertyMessage] = Message;
            }

            return Task.FromResult<bool>(true);
        }

        public Task ClearChanges()
        {
            JobDataMap.Remove(HelloJob.PropertyMessage);
            return Task.CompletedTask;
        }
    }
}

