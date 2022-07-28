using System;
namespace BlazingQuartz.Core.Models
{
    public class JobDetailModel
    {
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = "No Group";
        public string? Description { get; set; }
        public Type? JobClass { get; set; }
        public IDictionary<string, object> JobDataMap { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }
}

