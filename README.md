# Blink3

[![Build](https://github.com/EpicOfficer/Blink3/actions/workflows/build.yml/badge.svg)](https://github.com/EpicOfficer/Blink3/actions/workflows/build.yml)
[![Publish Docker Images](https://github.com/EpicOfficer/Blink3/actions/workflows/docker-image.yml/badge.svg)](https://github.com/EpicOfficer/Blink3/actions/workflows/docker-image.yml)

Blink3 is a .NET 8 project implemented with a multilayered architecture. This repository holds a Discord Bot solution
that comes with an API, a Blazor front-end, and Discord Activity support. Multiple features, like a global to-do list
and customizable wordle, are provided as well.

## Project Structure

The application is structured into multiple layers as follows:

1. `Blink3.API`: A .NET 8 REST API that contains endpoints for the `Blink3.Web` and `Blink3.Activity` presentation
   layers, and includes a Discord OAuth implementation.
2. `Blink3.Web`: A Blazor WASM front-end that supports login through the API utilizing Discord OAuth, and allows users
   to manage the bot.
3. `Blink3.Activity`: A Work-in-Progress presentation layer which will integrate with Discord Activity.
4. `Blink3.Bot`: The main presentation layer, developed in .Net 8 using Discord.Net, responsible for running the discord
   bot.
5. `Blink3.DataAccess`: A layer consisting of data access repositories and an EF Core code-first DbContext.
6. `Blink3.Core`: A shared library that contains common business logic interfaces, enums, and models.

## Features

Currently, the bot provides several commands, including:

- `/temp`: Base command to create and manage temporary Voice Channels.
- `/define`: Get the definition of an English word.
- `/wordle`: Start a new wordle game in the current channel.
- `/guess`: Attempt to guess the wordle.
- `/leaderboard`: Displays a global wordle points leaderboard.
- `/todo`: Base command for managing your to-do list.
- `/config`: Allows guild admins to change bot settings for their guild.

## Build & Run

Blink3 includes multiple Docker images for each of the solution's parts and Docker Compose files for orchestration. The
Docker Compose files included are:

- `docker-compose.yml` - The base Docker Compose file.
- `docker-compose.development.yml` - Overwrites the base configuration for local development. References the images
  built in the local solution.

To build and run the application using Docker Compose, you can execute the following commands:

For local development:

    docker-compose -f docker-compose.yml -f docker-compose.development.yml up --build 

For production:

    docker-compose up -d

An `.env.example` file has been included in the repository. Rename or copy this file to `.env` and replace the sample
values with your actual environment values to set up your environment.

Please note that the development Compose file references the Docker build images in the solution, while the production
one uses the images from the GitHub Container Registry.
