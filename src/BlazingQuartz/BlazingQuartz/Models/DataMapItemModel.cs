using BlazingQuartz.Core;
using BlazingQuartz.Extensions;

namespace BlazingQuartz.Models;

public class DataMapItemModel
{
    public KeyValuePair<string, object>? OriginalKeyValue { get; private set; }
    public string? Key { get; set; }
    public object? Value { get; set; }
    public DataMapType? Type { get; set; }

    public DataMapItemModel(KeyValuePair<string, object>? kv = null)
    {
        OriginalKeyValue = kv;
        if (kv != null)
        {
            Key = kv.Value.Key;
            Value = kv.Value.Value;
            Type = kv.Value.GetDataMapType();
        }
    }

    public bool IsSameKeyAsOriginal()
    {
        return Key == OriginalKeyValue?.Key;
    }

    public void SetValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return;
            
        switch (Type)
        {
            case DataMapType.Bool:
                Value = Convert.ToBoolean(value);
                break;
            case DataMapType.String:
                Value = value;
                break;
            case DataMapType.Decimal:
                Value = Convert.ToDecimal(value);
                break;
            case DataMapType.Integer:
                Value = Convert.ToInt32(value);
                break;
            case DataMapType.Double:
                Value = Convert.ToDouble(value);
                break;
            case DataMapType.Float:
                Value = float.Parse(value ?? "0");
                break;
            case DataMapType.Short:
                Value = Convert.ToInt16(value);
                break;
            case DataMapType.Long:
                Value = Convert.ToInt64(value);
                break;
            case DataMapType.Char:
                Value = Convert.ToChar(value);
                break;
            default:
                break;
        }
    }
}