# Client (3D Scene Generation Platform)

## How to use

To be able to generate 3D scenes, you must establish a connection with the server side of the project. See (server_repo_hyper_link) for detailed documentation on how to launch the server.

Current fonctionalities: generate 3D objects (command example: generate a 3D model of a cat), generate 3D scenes (command example: generate a sunny room with a couch, a coffee table near it and a cat sitting in the corner), modify 3D scene (command example: remove the cat from the scene, turn the cat into a dog, move the cat out of the room, add a cow in the middle of the room).
### Adding the project locally

1. Clone the git repository
2. Download Unity Hub
3. Add the project to Unity Hub and launch it
4. Enter the play-mode
5. Send messages via chat interface

### Using prebuilt executable

Executable file is available at #TODO

## 1. Fonctionalities/Modules

1. User Interface

A module providing a graphical terminal interface in Unity that allows the user to interact directly with the AI agent. It includes input handling (keyboard and microphone) and text message display. The user interface layer integrates elements such as an input field, a message history display, and status indicators.

Appearance and styles and related scripts are available under Assets/UI/Terminal and Assets/Scripts/Terminal respectively.

2. Websocket Client

A module implementing a WebSocket client that maintains a bidirectional communication channel over a TCP connection with the Python backend, enabling real-time exchange of messages, scene updates, and user commands. It calls internal methods to format user input into the appropriate messaging protocol (Protobuf) before sending it to the backend, or to render the agent’s responses in real time within the terminal window.

Module consists of two scripts: client.cs (script that allows to open/close websocket connection, receive and send messages) and message.cs (containts all incoming message processing logic).  

3. Redis Client

Redis is a in-memory database (cf https://redis.io/) used to store scene JSON data. On every scene change (creation or modification) current Unity scene is serialized into JSON format and written to the database with client id key.

Client.cs script allows to open/close redis connection and to write JSON scene data to the database.

4. SDK

A module defining all data structures used in communication with the Python backend, using Protocol Buffers (Protobuf) as the serialization protocol for efficient, language-independent data transfer. It provides three main categories of structures:

- message structures representing outgoing and incoming communication payloads. Each message type includes serialization and deserialization methods to convert all objects into their Protobuf form (to_proto() / from_proto()). Different message types cover a wide range of communication scenarios. It accounts for textual and vocal outgoing messages. For detailed incoming message types see (server_repo_hyper_link);
- scene structures that encompass all object models related to the Unity scene JSON representation. They encapsulate spatial and semantic information about scene elements such as objects, transformations, materials, lights, and skybox configurations, and are designed to match the format of Unity’s runtime scene graph, enabling direct scene generation after deserialization in Unity;
- patch structures that allow efficient scene modification. It accounts for the following modification: object addition, object modification (size/scale/shape/colour/position), object deletion, object regeneration (replace generated 3D mesh by a new one).

json_helpers.cs file contains useful functions for serialization/deserialization of these structures.

5. Scene management

A module allowing 3D object and scene rendering/modification in Unity, as well as scene serialization.

- importer.cs script contains code for scene creation/modification in Unity, all auxiliary functions using for these processes as well ass some test cases.
- exporter.cs scripts contains code for scene serialization.
- model_instantiator.cs contains code for a single 3D object instantiation in Unity.
