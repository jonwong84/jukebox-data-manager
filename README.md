# jukebox-data-manager

This project provides the required workflows to provide song metadata to different applications, along with API hosts that serve requests and responses. These APIs provides access points that allow a client application to manage metadata for music. It is responsible for the addition, upkeep, and deletion of metadata for individual songs, artists, and albums.

## Architectural Design

The Song Metadata API project is designed with five interconnecting layers: Client Applications -> API Hosts -> Manager Layer -> Access Layer -> Storage. Diagram for visual:

![Song metadata API architecture diagram showing a layered system: Clients layer with two client boxes labeled rRPC client generated stub consumer and REST client standard HTTP consumer; API hosts layer with rRPC host procedure-based endpoints and REST host resource-based endpoints; Business logic manager layer labeled Manager layer CRUD orchestration, validation, rules; Data logic access layer labeled Access layer Queries, writes, data mapping; and a SQL database at the bottom labeled SQL database. Arrows connect clients to hosts, hosts to manager, manager to access, and access to the database.](./docs/song_api_architecture_mid.svg)

## Features

This project has the following features:

### Dual gRPC And REST Hosts

This project features two different API hosts that share the same managers that handle business logic.