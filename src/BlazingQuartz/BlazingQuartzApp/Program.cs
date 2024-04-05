using BlazingQuartz;
using Quartz;
using Quartz.Logging;

var builder = WebApplication.CreateBuilder(args);

#region Configure Quartz3
// base configuration from appsettings.json
builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));

// if you are using persistent job store, you might want to alter some options
builder.Services.Configure<QuartzOptions>(options =>
{
    var jobStoreType = options["quartz.jobStore.type"];
    if ((jobStoreType ?? string.Empty) == "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz")
    {
        options.Scheduling.IgnoreDuplicates = true; // default: false
        options.Scheduling.OverWriteExistingData = true; // default: true
    }

    var dataSource = options["quartz.jobStore.dataSource"];
    if (!string.IsNullOrEmpty(dataSource))
    {
        var connectionStringName = options[$"quartz.dataSource.{dataSource}.connectionStringName"];
        if (!string.IsNullOrEmpty(connectionStringName))
        {
            var connStr = builder.Configuration.GetConnectionString(connectionStringName);
            options[$"quartz.dataSource.{dataSource}.connectionString"] = connStr;
        }
    }
});
// Add the required Quartz.NET services
builder.Services.AddQuartz();
// Add the Quartz.NET hosted service
builder.Services.AddQuartzHostedService(
    q => q.WaitForJobsToComplete = true);
#endregion Configure Quartz3

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazingQuartzUI(builder.Configuration.GetSection("BlazingQuartz"),
    connectionString: builder.Configuration.GetConnectionString("BlazingQuartzDb"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UsePathBase("/BlazingQuartzUI");
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseBlazingQuartzUI();
app.MapBlazorHub();
app.MapFallbackToPage("/BlazingQuartzUI/_Host");

app.Run();

