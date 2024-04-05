# Upgrade to v0.7.0 (.NET 8)
From [.NET 7 to 8 Migration](https://learn.microsoft.com/en-us/aspnet/core/migration/70-80?view=aspnetcore-8.0&tabs=visual-studio#update-docker-port):
> Default ASP.NET Core port configured in .NET container images has been updated from port 80 to 8080. 

If your BlazingQuartz docker container was configured to use http port 80, make sure you used environment variable `ASPNETCORE_HTTP_PORTS` or `ASPNETCORE_URLS` to configure the port to listen.

Example if you still want to use port 80:
  ```
  ASPNETCORE_HTTP_PORTS=80
  ```

  or
  
  ```
  ASPNETCORE_URLS=http://*:80/
  ```

References: [.NET 8 specify docker ports](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-8.0#specify-ports-only)