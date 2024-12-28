# Use the .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution and project files
COPY *.sln .
COPY LMS.Api/*.csproj LMS.Api/
COPY LMS.Application/*.csproj LMS.Application/
COPY LMS.Common/*.csproj LMS.Common/
COPY LMS.Infrastructure/*.csproj LMS.Infrastructure/
COPY LMS.Domain/*.csproj LMS.Domain/
COPY LMS.Tests/*.csproj LMS.Tests/

# Restore the dependencies
RUN dotnet restore

# Copy the rest of the project files
COPY . .

# Build the application
RUN dotnet publish LMS.Api/LMS.Api.csproj -c Release -o /app/out

# Use the runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the certificate to the container
COPY ./certificates/aspnetcore-dev.pfx /https/aspnetcore-dev.pfx

# Copy the published output from the build stage
COPY --from=build /app/out .

# Expose ports for both HTTP and HTTPS
EXPOSE 80
EXPOSE 443

# Set environment variables for ASP.NET Core to use HTTPS
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="lmsapi2024"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/aspnetcore-dev.pfx"

# Set the environment variable for ASP.NET Core
ENV ASPNETCORE_ENVIRONMENT=Development

# Define the entry point for the container
ENTRYPOINT ["dotnet", "LMS.Api.dll"]
