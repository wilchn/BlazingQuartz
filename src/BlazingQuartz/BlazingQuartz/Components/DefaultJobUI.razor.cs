using System;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazingQuartz.Components
{
    public partial class DefaultJobUI : ComponentBase
    {
        [Inject] private IDialogService DialogSvc { get; set; } = null!;

        [Parameter]
        [EditorRequired]
        public JobDetailModel JobDetail { get; set; } = new();

        [Parameter] public bool IsReadOnly { get; set; }

        async Task OnAddDataMap()
        {
            var options = new DialogOptions {
                CloseOnEscapeKey = true,
                DisableBackdropClick = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Small
            };
            var parameters = new DialogParameters { 
                ["JobDataMap"] = new Dictionary<string, object>(JobDetail.JobDataMap, StringComparer.OrdinalIgnoreCase)
            };

            var dialog = DialogSvc.Show<JobDataMapDialog>("Add Data Map", parameters, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                var dataMap = (DataMapItemModel)result.Data;
                if (dataMap.Key != null && dataMap.Value != null)
                    JobDetail.JobDataMap.Add(dataMap.Key, dataMap.Value);
                else
                {
                    // TODO print error message. Data map is null
                }
            }
        }

        async Task OnEditDataMap(KeyValuePair<string, object> item)
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                DisableBackdropClick = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Small
            };
            var parameters = new DialogParameters
            {
                ["JobDataMap"] = JobDetail.JobDataMap,
                ["DataMapItem"] = new DataMapItemModel(item),
                ["IsEditMode"] = true
            };

            var dialog = DialogSvc.Show<JobDataMapDialog>("Edit Data Map", parameters, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                var dataMap = (DataMapItemModel)result.Data;
                if (dataMap.Key != null && dataMap.Value != null)
                {
                    JobDetail.JobDataMap[dataMap.Key] = dataMap.Value;
                }
                else
                {
                    // TODO print error message. Data map is null
                }
            }
        }

        async Task OnCloneDataMap(KeyValuePair<string, object> item)
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                DisableBackdropClick = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Small
            };
            int index = 1;
            var key = item.Key + index++;
            
            while (JobDetail.JobDataMap.ContainsKey(key))
            {
                if (index == int.MaxValue)
                {
                    key = string.Empty;
                    break;
                }
                    
                key = item.Key + index++;
            }
            var clonedItem = new KeyValuePair<string, object>(key, item.Value);
            var parameters = new DialogParameters
            {
                ["JobDataMap"] = new Dictionary<string, object>(JobDetail.JobDataMap, StringComparer.OrdinalIgnoreCase),
                ["DataMapItem"] = new DataMapItemModel(clonedItem)
            };

            var dialog = DialogSvc.Show<JobDataMapDialog>("Add Data Map", parameters, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                var dataMap = (DataMapItemModel)result.Data;
                if (dataMap.Key != null && dataMap.Value != null)
                {
                    JobDetail.JobDataMap[dataMap.Key] = dataMap.Value;
                }
                else
                {
                    // TODO print error message. Data map is null
                }
            }
        }

        async Task OnDeleteDataMap(KeyValuePair<string, object> item)
        {
            bool? yes = await DialogSvc.ShowMessageBox(
                "Confirm Delete", 
                $"Do you want to delete '{item.Key}'?", 
                yesText:"Yes", cancelText:"No");

            if (yes == null || !yes.Value)
            {
                return;
            }

            JobDetail.JobDataMap.Remove(item);
        }
    }
}

