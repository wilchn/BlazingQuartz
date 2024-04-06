FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY /app ./

VOLUME ["/app/logs", "/app/certs", "/app/jobs"]
EXPOSE 8080/tcp
EXPOSE 443/tcp
ENTRYPOINT ["dotnet", "BlazingQuartzApp.dll"]
