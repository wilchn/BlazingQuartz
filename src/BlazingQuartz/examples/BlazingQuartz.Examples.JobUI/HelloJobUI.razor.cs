using System;
using System.Globalization;
using BlazingQuartz.Jobs.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BlazingQuartz.Examples.JobUI
{
    /// <summary>
    /// Custom UI for Hello Job
    /// </summary>
    public partial class HelloJobUI : ComponentBase, IJobUI
    {
        public string JobClass => "BlazingQuartz.Examples.JobUI.HelloJob";

        [Inject] public IDataMapValueResolver DataMapValueResolver { get; set; } = null!;

        [Parameter] public bool IsReadOnly { get; set; }
        [Parameter] public IDictionary<string, object> JobDataMap { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private DataMapValue DataMapMessage { get; set; } = new(DataMapValueType.InterpolatedString, 1);

        private string? ResolvedMessage { get; set; }
        private int DelayInMs { get; set; }

        protected override void OnInitialized()
        {
            if (JobDataMap.ContainsKey(HelloJob.PropertyMessage))
            {
                DataMapMessage = DataMapValue.Create(JobDataMap[HelloJob.PropertyMessage],
                    DataMapValueType.InterpolatedString, 1);
            }

            if (JobDataMap.ContainsKey(HelloJob.PropertyDelayInMs))
            {
                DelayInMs = Convert.ToInt32(JobDataMap[HelloJob.PropertyDelayInMs]);
            }
        }

        /// <summary>
        /// Handle action when message text field got updated
        /// </summary>
        /// <param name="message"></param>
        private void OnMessageChanged(string? message)
        {
            DataMapMessage.Value = message;
            ResolvedMessage = DataMapValueResolver.Resolve(DataMapMessage);
        }

        public Task<bool> ApplyChanges()
        {
            if (string.IsNullOrEmpty(DataMapMessage.Value))
            {
                JobDataMap.Remove(HelloJob.PropertyMessage);
            }
            else
            {
                JobDataMap[HelloJob.PropertyMessage] = DataMapMessage.ToString();
            }

            JobDataMap[HelloJob.PropertyDelayInMs] = DelayInMs.ToString();

            return Task.FromResult<bool>(true);
        }

        public Task ClearChanges()
        {
            JobDataMap.Remove(HelloJob.PropertyMessage);
            JobDataMap.Remove(HelloJob.PropertyDelayInMs);
            return Task.CompletedTask;
        }
    }
}

