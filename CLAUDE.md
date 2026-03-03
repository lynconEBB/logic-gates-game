# LogicGatesGame

A VR (and Desktop) puzzle game where the player is presented with a boolean logic expression (e.g. `A + A·B`) and must wire together logic gate nodes to reproduce the result at the final sink node.

## Project Layout

All game-specific code lives under `Assets/LogicGatesGame/`. Do not place project code elsewhere.

```
Assets/LogicGatesGame/
  Circuits/    # Saved circuit definitions
  Materials/   # Game materials (e.g. Emissive.mat)
  Models/      # 3D models for gates, nodes, wires
  Prefabs/     # Unity prefabs for gates, wires, sockets
  Scenes/      # Unity scenes
  Scripts/     # All C# game logic (namespace: LogicGatesGame.Scripts)
  Textures/    # Textures
```

## Architecture

### Logic Layer (pure C#, no Unity dependency)

`Node.cs` — base class and all node types. Nodes form a directed acyclic graph evaluated lazily via `ExecEvaluation()`.

| Class | Role |
|---|---|
| `Node` | Abstract base. Holds `inputs`/`outputs` lists, enforces slot limits, fires `OnEvaluated` event. |
| `SourceNode` | No inputs. Holds a `bool?` value set externally (the user-controlled input). |
| `SinkNode` | No outputs, 1 input. The final result node the player must satisfy. |
| `SimpleNode` | Pass-through, 1 input. |
| `AndNode` | N inputs → true only if all inputs are true. |
| `OrNode` | N inputs → true if any input is true. |
| `NotNode` | 1 input → inverts it. |

`bool?` (nullable bool) is used throughout: `null` means the value is undefined (disconnected or unset).

`CircuitController.cs` — Unity `MonoBehaviour` that owns the node graph. Responsibilities:
- Creates and removes nodes (`AddNode`, `RemoveNode`)
- Connects/disconnects nodes (`ConnectNodes`, `DisconnectNodes`)
- Propagates evaluation via BFS from the changed source (`EvaluateTree`)

### Unity / XR Layer

| Script | Role |
|---|---|
| `NodeComponent.cs` | `MonoBehaviour` bridge between a Unity GameObject and a logic `Node`. Registers the node with `CircuitController` on `Awake`. |
| `ConnectionSocket.cs` | Extends `XRProximityInteractor`. The physical socket on a gate that accepts a `WireConnection`. Handles connect/disconnect logic and visual feedback (outline color). |
| `GateSocket.cs` | Extends `XRSocketInteractor`. Used for snapping gate objects into slots. |
| `WireConnection.cs` | Extends `XRGrabInteractable`. One end of a physical wire. Knows its connected `Node` and its sibling `WireConnection` (the other end). Destroys itself if dropped without connecting. |
| `WireInteractable.cs` | Extends `XRSimpleInteractable`. Represents the full wire (both ends). Grabbing it destroys the wire and disconnects nodes. Updates `StateVisualizer` when a connection is made. |
| `WireSplineController.cs` | Manages the visual spline mesh of a wire between its two endpoints. |
| `SourceProvider.cs` | `MonoBehaviour` on an input node. Holds a `bool` value with `ToggleValue()`. Fires `OnValueChanged` which `NodeComponent` listens to in order to call `CircuitController.UpdateValue`. |
| `StateVisualizer.cs` | Changes a `MeshRenderer`'s material color based on `Node.OnEvaluated` (cyan = true, white = false, pale violet red = undefined). |
| `XRProximityInteractor.cs` | Custom XR interactor that activates within a radius rather than on direct grab. |
| `ConnectionInitializer.cs` | Helper to set up initial connections at scene start. |
| `CapsuleColliderFitter.cs` | Utility to fit a `CapsuleCollider` along a wire spline. |
| `TriggerContactHelper.cs` | Utility for trigger-based contact detection. |
| `SortingHelper.cs` | General sorting utility. |
| `XRToolkitUtils.cs` | XR Toolkit helper utilities. |

## Key Conventions

- Namespace: `LogicGatesGame.Scripts` for all scripts.
- The logic layer (`Node` and subclasses) must remain free of Unity dependencies.
- `bool?` (`null`) represents an undefined/unconnected signal throughout the entire evaluation chain.
- Evaluation is BFS-based and triggered only when a source value changes — not every frame.
- `NodeComponent` determines whether a node is an Output (source, player-controlled) or Input (sink, receives signal) via the `NodeType` enum.
- Wire connections are bidirectional at the XR layer: each `WireConnection` knows its sibling end, and `ConnectionSocket` resolves which side is input/output from `NodeComponent.Type`.

## Files — Do Not Edit

- `*.meta` files — managed by the Unity Editor.
- `.unity` scene files — edit only through the Unity Editor.
- `.prefab` files — edit only through the Unity Editor.
- `.asset` files — edit only through the Unity Editor.
- `.lighting` files — managed by Unity's lighting system.
- `.mat` files — edit only through the Unity Editor (exception: programmatic material property changes at runtime in C# are fine).
