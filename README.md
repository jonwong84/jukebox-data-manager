# jukebox-data-manager

This project provides the required workflows to provide song metadata to different applications, along with two separate API hosts that serve requests and responses. These APIs provides access points that allow a client application to manage metadata for music. It is responsible for the addition, upkeep, and deletion of metadata for individual songs, artists, and albums.

## Architectural Design

The Song Metadata API project is designed with five interconnecting layers: Client Applications -> API Hosts -> Manager Layer -> Access Layer -> Storage. It features a gRPC host and a REST host to serve the needs of different client applications.

Diagram for visual:

![Song metadata API architecture diagram showing a layered system: Clients layer with two client boxes labeled rRPC client generated stub consumer and REST client standard HTTP consumer; API hosts layer with rRPC host procedure-based endpoints and REST host resource-based endpoints; Business logic manager layer labeled Manager layer CRUD orchestration, validation, rules; Data logic access layer labeled Access layer Queries, writes, data mapping; and a SQL database at the bottom labeled SQL database. Arrows connect clients to hosts, hosts to manager, manager to access, and access to the database.](./docs/song_api_architecture_mid.svg)

## Local Setup

To run this project, you will need to do the following:

- Set up a SQL server.
  - If you are running Docker, you can run this Powershell command to quickly spin one up:
    ```
    docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Pass123" \
    -p 1433:1433 --name sql_server \
    -d mcr.microsoft.com/mssql/server:2022-latest
    ```
- Run the Entity Framework migrations to set up the database tables. The EF Core models and contexts live in the `jukebox-data-access` project.
  - To quickly set this up, navigate to the `jukebox-data-access` directory and run the following command to create the migration:
    ```
    dotnet ef migrations add InitialCreate --project src/Jukebox.DataAccess.Migrations/Jukebox.DataAccess.Migrations.csproj --startup-project src/Jukebox.DataAccess.Migrations/Jukebox.DataAccess.Migrations.csproj`
    ```
    And then run this command to apply the migration to the database:
    ```
    dotnet ef database update --project src/Jukebox.DataAccess.Migrations/Jukebox.DataAccess.Migrations.csproj --startup-project src/Jukebox.DataAccess.Migrations/Jukebox.DataAccess.Migrations.csproj
    ```
- Set a new environmental variable called `JUKEBOX_DB_CONNECTION_STRING` using your connection string to your SQL database.
  - For a quick setup, run this Powershell command:
  ```
  [System.Environment]::SetEnvironmentVariable("JUKEBOX_DB_CONNECTION_STRING", "Server=localhost,1433;Database=Jukebox;User Id=sa;Password=YourStrong@Pass123;TrustServerCertificate=True;", "User")
  ```
- In Visual Studio, run either `Jukebox.DataManager.Grpc` for the gRPC host, or `Jukebox.DataManager.Rest` for the REST host.

## Loal Deployment To Kubernetes Via Helm / Docker

- Follow the instructions in `HELM_DEPLOYMENT.md`.