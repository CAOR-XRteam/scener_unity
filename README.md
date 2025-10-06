# Client (3D Scene Generation Platform)

## Overview

This document outlines the client-side component of the 3D Scene Generation Platform, a system designed to enable users to interactively create and modify 3D environments. This client, built on Unity, provides a rich graphical interface for communicating with an AI agent hosted on the server.

## How to Use

To begin generating 3D scenes, you must first establish a connection with the project's server component. Please refer to the [server repository documentation](server_repo_hyper_link) for detailed instructions on launching the server.

### Core Functionalities

The platform offers robust capabilities for 3D scene manipulation:

*   **Generate 3D Objects:** Create individual 3D models based on textual descriptions.
    *   *Example: "generate a 3D model of a cat"*
*   **Generate 3D Scenes:** Construct complex 3D environments from high-level prompts.
    *   *Example: "generate a sunny room with a couch, a coffee table near it, and a cat sitting in the corner"*
*   **Modify 3D Scenes:** Dynamically alter existing scenes with specific commands.
    *   *Examples: "remove the cat from the scene", "turn the cat into a dog", "move the cat out of the room", "add a cow in the middle of the room"*
*   **Database Operations:** Manage the server's local database, which stores and reuses generated 3D assets (see [server repository documentation](server_repo_hyper_link)). You can delete individual assets or clear the entire database.
    *   **Warning:** Clearing the database is a permanent and irreversible action.
    *   *Examples: "delete asset `asset_id`", "clear the database"*

### Getting Started

#### Adding the Project Locally

1.  **Clone Repository:** Clone the project's Git repository to your local machine.
2.  **Download Unity Hub:** Ensure you have Unity Hub installed.
3.  **Add & Launch Project:** Add the cloned project to Unity Hub and launch it within the Unity Editor.
4.  **Enter Play Mode:** Start the Unity application in play-mode.
5.  **Interact:** Send commands via the integrated chat interface to begin scene generation.

#### Using Pre-built Executable

A pre-built executable file for the client will be available at: #TODO

## Technical Architecture

The client application is composed of several interconnected modules, each handling a specific aspect of user interaction, communication, and 3D scene management within Unity.

### 1. User Interface (UI)

This module provides a comprehensive graphical terminal interface within Unity, facilitating direct interaction with the AI agent. It encompasses:

*   **Input Handling:** Manages user input via keyboard and microphone.
*   **Message Display:** Renders a history of text messages exchanged with the agent.
*   **Status Indicators:** Presents real-time operational feedback to the user.

*Related assets and scripts are located under `Assets/UI/Terminal` and `Assets/Scripts/Terminal` respectively.*

### 2. WebSocket Client

The WebSocket Client module is responsible for maintaining a persistent, bidirectional communication channel over a TCP connection with the Python backend. This enables real-time exchange of user commands, scene updates, and agent responses.

*   It formats user input into the appropriate messaging protocol (Protocol Buffers) before transmission to the backend.
*   It renders the AI agent's responses in real-time within the terminal window.

*This module consists of two primary scripts: `client.cs` (manages WebSocket connection, message sending, and receiving) and `message.cs` (handles all incoming message processing logic).*  

### 3. Redis Client

Redis, an in-memory data structure store ([learn more](https://redis.io/)), is utilized to store serialized scene JSON data. Upon every scene modification or creation, the current Unity scene is serialized into JSON format and written to the Redis database, indexed by the client's unique ID.

*The `client.cs` script facilitates opening/closing Redis connections and writing JSON scene data to the database.*

### 4. Software Development Kit (SDK)

The SDK module defines all data structures critical for communication with the Python backend, leveraging Protocol Buffers (Protobuf) for efficient, language-independent data transfer. It categorizes structures into three main groups:

*   **Message Structures:** Represent outgoing and incoming communication payloads, including support for textual and vocal messages. Each message type provides `to_proto()` and `from_proto()` methods for Protobuf serialization and deserialization. For detailed incoming message types, please refer to the [server repository documentation](server_repo_hyper_link).
*   **Scene Structures:** Encompass object models for Unity scene JSON representation. These structures encapsulate spatial and semantic information (e.g., objects, transformations, materials, lights, skybox configurations) and are designed to align with Unity's runtime scene graph, enabling direct scene generation.
*   **Patch Structures:** Allow for efficient scene modifications, supporting operations such as:
    *   Object addition
    *   Object modification (size, scale, shape, color, position)
    *   Object deletion
    *   Object regeneration (replacing a generated 3D mesh with a new one)

*The `json_helpers.cs` file contains utility functions for the serialization and deserialization of these structures.*

### 5. Scene Management

This module is central to the creation, modification, and serialization of 3D objects and scenes within Unity.

*   **`importer.cs`:** Contains code for scene creation and modification in Unity, along with auxiliary functions and test cases for these processes.
*   **`exporter.cs`:** Manages the serialization of Unity scenes into a structured format.
*   **`model_instantiator.cs`:** Handles the instantiation of individual 3D objects within the Unity environment.
