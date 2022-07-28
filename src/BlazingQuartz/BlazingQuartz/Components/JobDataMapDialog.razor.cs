using System;
using BlazingQuartz.Core;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;
using Microsoft.AspNetCore.Components;
using BlazingQuartz.Extensions;
using MudBlazor;
using BlazingQuartz.Models;

namespace BlazingQuartz.Components;

public partial class JobDataMapDialog : ComponentBase
{
    [Inject]
    private IDialogService DialogSvc { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    
    [Parameter]
    [EditorRequired]
    public IDictionary<string, object> JobDataMap { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    [Parameter]
    public DataMapItemModel DataMapItem { get; set; } = new();

    [Parameter]
    public bool IsEditMode { get; set; } = false;

    private string? Value { get; set; }
    private MudForm _form = null!;
    private bool _isValid;

    /// <summary>
    /// DataMapType -> Description
    /// </summary>
    private IDictionary<DataMapType, string> AvailableDataMapTypes = new Dictionary<DataMapType, string>();

    protected override void OnInitialized()
    {
        // initialize available data types
        foreach(var mapType in Enum.GetValues<DataMapType>())
        {
            if (mapType == DataMapType.Object)
            {
                continue;
            }
            AvailableDataMapTypes.Add(mapType, mapType.ToString());
        }
        var currentMapType = DataMapItem.OriginalKeyValue?.GetDataMapType();
        if (currentMapType != null && currentMapType == DataMapType.Object)
        {
            AvailableDataMapTypes.Add(currentMapType.Value, 
                DataMapItem.OriginalKeyValue?.GetDataMapTypeDescription() ?? string.Empty);
        }

        Value = DataMapItem.Value?.ToString();
    }
    
    private string? ValidateKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "Key is required";

        // validate then add to dictionary
        if (!DataMapItem.IsSameKeyAsOriginal() &&
            JobDataMap.ContainsKey(key))
        {
            return "This key was already defined";
        }

        return null;
    }

    private async Task OnSave()
    {
        await _form.Validate();
        
        if (!_isValid)
            return;

        try
        {
            DataMapItem.SetValue(Value);
        }
        catch (Exception ex)
        {
            await DialogSvc.ShowMessageBox(
                "Error", 
                $"Invalid value. {ex.Message}");
            return;
        }

        MudDialog.Close(DialogResult.Ok(DataMapItem));
    }

    void OnCancel() => MudDialog.Cancel();

}