FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MacroMail/MacroMail.csproj", "MacroMail/"]
RUN dotnet restore "MacroMail/MacroMail.csproj"
COPY . .
WORKDIR "/src/MacroMail"
RUN dotnet build "MacroMail.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MacroMail.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MacroMail.dll"]
