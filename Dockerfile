FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ExpenseAnalyzer.Api/ExpenseAnalyzer.Api.csproj", "src/ExpenseAnalyzer.Api/"]
COPY ["src/ExpenseAnalyzer.Application/ExpenseAnalyzer.Application.csproj", "src/ExpenseAnalyzer.Application/"]
COPY ["src/ExpenseAnalyzer.Domain/ExpenseAnalyzer.Domain.csproj", "src/ExpenseAnalyzer.Domain/"]
COPY ["src/ExpenseAnalyzer.Infrastructure/ExpenseAnalyzer.Infrastructure.csproj", "src/ExpenseAnalyzer.Infrastructure/"]

RUN dotnet restore "src/ExpenseAnalyzer.Api/ExpenseAnalyzer.Api.csproj"

COPY . .
WORKDIR "/src/src/ExpenseAnalyzer.Api"
RUN dotnet publish "ExpenseAnalyzer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ExpenseAnalyzer.Api.dll"]