
namespace BlazingQuartz.Services
{
    public interface IJobUIProvider
    {
        Type GetJobUIType(string? jobTypeFullName);
    }
}