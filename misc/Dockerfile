FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
EXPOSE 2323
ENV ASPNETCORE_URLS=http://*:2323

WORKDIR /app
COPY . .

#ENTRYPOINT ["./EdgeMq.Api"]

ENTRYPOINT ["/bin/sh"]

