FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY /app ./

VOLUME ["/app/logs", "/app/certs", "/app/jobs"]
EXPOSE 80/tcp
EXPOSE 443/tcp
ENTRYPOINT ["dotnet", "BlazingQuartzApp.dll"]
