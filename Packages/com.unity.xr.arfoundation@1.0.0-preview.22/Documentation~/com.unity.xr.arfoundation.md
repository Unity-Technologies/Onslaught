# About AR Foundation

Use the AR Foundation package to add high-level functionality for working with augmented reality. Unity 2018.1 includes built-in multi-platform support for AR. These APIs are in the `UnityEngine.Experimental.XR` namespace, and consist of a number of `Subsystem`s, e.g., `XRPlaneSubsystem`. Several XR Subsystems comprise the low-level API for interacting with AR. The **AR Foundation** package wraps this low-level API into a cohesive whole and enhances it with additional utilities, such as AR session lifecycle management and the creation of `GameObject`s to represent detected features in the environment.

AR Foundation is a set of `MonoBehaviour`s for dealing with devices that support following concepts:
- Planar surface detection
- Point clouds, also known as feature points
- Reference points: an arbitrary position and orientation that the device tracks
- Light estimation: estimates for average color temperature and brightness in physical space.
- World tracking: tracking the device's position and orientation in physical space.

# Installing AR Foundation

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

In addition, you need to install at least one platform-specific AR package (*Window > Package Manager*):

 - ARKit XR Plugin
 - ARCore XR Plugin

# Glossary

| Term | Meaning |
|-|-|
| Tracking | The ability of the AR device to determine its relative position and orientation in the world. If the environment is too dark, for example, the device may "lose tracking", which means it can no longer accurately report its position. |
| Trackable | A real-world feature detected and/or tracked by the AR device, e.g., a planar surface. |
| Feature Point | A specific point in a point cloud. AR devices use a camera and image analysis to track specific points in the world which are used to build a map of its environment. These are usually high frequency elements, such as a knot in a wood-grain surface.|
| Session | Refers to an AR instance. |
| Session Space | The coordinate system relative to the beginning of the AR session. For example, session space (0, 0, 0) refers to the position at which the AR session was created. An AR device typically reports trackables and tracking information relative to its session origin.|

# Using AR Foundation

## Basic Setup

A basic AR scene hierarchy looks like this:

![alt text](images/simple_scene_graph.png "Scene graph")

Required components are explained below.

### ARSession
An AR scene should include an `ARSession` component. The AR Session controls the lifecycle of an AR experience, enabling or disabling AR on the target platform. The `ARSession` can be on any `GameObject`.

![alt text](images/ar_session.png "ARSession component")

An AR Session can be started, paused, resumed, and stopped entirely. Enabling or disabling the component will start/resume or pause the session, respectively. If a session is "paused", the system no longer tracks features in its environment, but if it is resumed at a later time, the system will attempt to recover and maintain previously detected features.

A session has the following options, each of which may be changed at runtime:

| Option | Meaning |
|-|-|
| Light Estimation | Whether to enable light estimation information. This can be used to correctly light virtual content in the scene. |
| Try to Install Update If Needed | Will try to install AR software if possible. |

**Note:** An AR session is a global construct. An `ARSession` component manages this global session, so multiple `ARSession` components will all try to manage the same global session.

### AR Subsystem Manager
Included in any AR Session is a static class called `ARSubsystemManager`.  This manager governs the lifecycles of multiple AR-related subsystems and provides higher level abstractions and utilities for managing and using the AR-related subsystems.

The AR Subsystem Manager also maintains the current state of the session.  The state of the current session can be queried through the `ARSubsystemManager.systemState` variable and compared against the values of the `ARSystemState` enumeration.  There is also a callback that can be subscribed to within the `ARSubsystemManager` named `systemStateChanged`.

A session can be in one of the following states and can change during runtime:

|State|Meaning|
|-|-|
|None|The AR System has not been initialized and availability is unknown.|
|Unsupported|AR is not supported on the current device.|
|CheckingAvailability|The system is checking the availability of AR on the current device.|
|NeedsInstall|AR is supported on the current device, but requires an additional install.|
|Installing|AR software is being installed|
|Ready|AR is supported and ready|
|SessionInitialized|An AR session is initializing (i.e., starting up).  This usually means AR is working but has not gathered enough information about the environment|
|SessionTracking|An AR session is running and is tracking (i.e., the device is able to determine its position and orientation in the world).|

### ARSessionOrigin

![alt text](images/ar_session_origin.png "AR Session Origin")

The purpose of the `ARSessionOrigin` is to transform trackable features (such as planar surfaces and feature points) into their final position, orientation, and scale in the Unity scene. Because AR devices provide their data in "session space", an unscaled space relative to the beginning of the AR session, the `ARSessionOrigin` performs the appropriate transformation into Unity space.

This concept is similar to the difference between "model" or "local" space and world space when working with other assets in Unity. For instance, if you import a house asset from a DCC tool, then the door will have a position relative to the modeler's origin. This is commonly called "model space" or "local space". When it is instantiated in Unity, it will also have a world space, relative to Unity's origin.

Likewise, trackables produced by an AR device, such as planes, are provided in "session space", relative to the device's coordinate system. When instantiated in Unity as `GameObject`s, they also have a world space. In order to instantiate them in the correct place, Unity needs to know where the session origin should be in the Unity scene.

The `ARSessionOrigin` also allows you to scale virtual content and apply an offset to the camera. If you are scaling or offseting the `ARSessionOrigin`, then its `Camera` should be a child of the `ARSessionOrigin`. Since the `Camera` is driven by the session, the `Camera` and detected trackables will move together in this setup.

#### Scale

To apply scale to the `ARSessionOrigin`, simply set its `transform`'s scale. This has the effect of scaling all the data coming from the device, including the `AR Camera`'s position and any detected trackables. Larger values will make AR content appear smaller, so, for example, a scale of 10 would make your content appear 10 times smaller, while 0.1 would make your content appear 10 times larger.

### TrackedPoseDriver

Parented to the `ARSessionOrigin`'s' `GameObject` should be (at least) one camera, which will be used to render any trackables you wish to visualize. The camera should also have a `TrackedPoseDriver` component on it, which will drive the camera's local position and rotation according to the device's tracking information. This setup allows the camera's local space to match the AR "session space".

![alt text](images/tracked_pose_driver.png "TrackedPoseDriver")

The `Camera` must be a child of the `ARSessionOrigin`.

### ARCameraBackground

If you want to render the device's color camera texture to the background, you need to add an `ARCameraBackground` component to a camera. This will subscribe to AR camera events and blit the camera texture to the screen. This is not required, but common for AR apps.

![alt text](images/ar_background_renderer.png "ARCameraBackground")

The `Custom Material` property is optional, and typically you do not need to set it. The platform-specific packages provided by Unity (e.g., `ARCore` and `ARKit`) provide their own shaders for background rendering.

If `Use Custom Material` is `true`, then the `ARCameraBackground` will use the `Material` you specify for background rendering.

If you have exactly one `ARSessionOrigin`, then you can simply add the `ARCameraBackground` to that camera. If you have multiple `ARSessionOrigin`s (to selectively render different content at different scales, for instance), you should use separate cameras for each `ARSessionOrigin` and a separate, single camera for the `ARCameraBackground`.

#### Copying the Camera Texture to a Render Texture

The camera textures are likely [External Textures](https://docs.unity3d.com/ScriptReference/Texture2D.CreateExternalTexture.html) and may not last beyond a frame boundary. It can be useful to copy the camera image to a [Render Texture](https://docs.unity3d.com/Manual/class-RenderTexture.html) for persistence or further processing. This code will blit the camera image to a render texture of your choice:

```csharp
Graphics.Blit(null, m_MyRenderTexture, m_ARBackgroundCamera.material);
```

### Accessing the Camera Image on the CPU

See [Camera Image Manual Documentation](cpu-camera-image.md).

### AR Managers

The `ARSessionOrigin`'s' `GameObject` can also have a number of additional components on it, which manage the addition, updating, and removal of `GameObject`s to represent planes, point cloud, and reference points. These are the `ARPlaneManager`, `ARPointCloudManager`, and `ARReferencePointManager`, respectively:

![alt text](images/ar_session_origin_with_managers.png "AR Session Origin with Managers")

Each manager subscribes to relevant AR events, such as `planeAdded`, `planeUpdated`, and `planeRemoved`, and creates `GameObject`s for each detected trackable. The generated `GameObject`s are parented to a special `GameObject` under the `ARSessionOrigin`, accessible via `ARSessionOrigin.trackablesParent`.

Each manager also accepts an optional prefab, which will be instantiated to represent the trackable. If no prefab is specified, a `GameObject` is created with an appropriate trackable component:

| Manager                   | Trackable Component |
|---------------------------|---------------------|
| `ARPlaneManager`          | `ARPlane`           |
| `ARPointCloudManager`     | `ARPointCloud`      |
| `ARReferencePointManager` | `ARReferencePoint`  |

Each trackable component stores information about the trackable, but does not visualize it. Its `Transform` is updated by its manager whenever the AR device reports an update.

Most trackables have a session-unique `TrackableId` associated with them. You can access an individual trackable by id through its manager.

**Note** A point cloud does not have a `TrackableId` (nor do the individual points within the cloud), and only one `GameObject` is created to represent the entire point cloud.

### Visualizing Trackables

To do something with the generated trackables, subscribe to the appropriate event on that trackable's component.

An `ARPlane` has the following events:

| Event | Description |
|-|-|
| `updated` | Invoked when a plane is updated |
| `removed` | Invoked when a plane is about to be removed. <br/><br/> **Note:** By default, the `ARPlane` (and its `GameObject`) will be removed immediately. You can suppress this behavior by setting `ARPlane.destroyOnRemoval` to `false`. You may wish to do this if, for example, you want the plane to fade out over time. |
| `boundaryChanged` | Invoked when the boundary points change by more than `ARPlane.vertexChangedThreshold`. If you want to visualize the mesh, subscribe to this event. |

An `ARPointCloud` has the following event:

| Event | Description |
|-|-|
| `updated` | Invoked when the point cloud is updated. |

An `ARReferencePoint` has the following event:

| Event | Description |
|-|-|
| `updated` | Invoked when the reference point's `Pose` or `TrackingState` changes. |

#### ARPlaneMeshVisualizer

To visualize planes, you'll need to create a prefab or `GameObject` which includes a component that subscribes to `ARPlane`'s `updated` or `boundaryChanged` event. `ARFoundation` includes such a component: `ARPlaneMeshVisualizer`. This component will generate a `Mesh` from the boundary vertices and assign it to a `MeshCollider`, `MeshFilter`, and `LineRenderer`, if present.

There is a new menu item `GameObject > XR > AR Default Plane` which will create a new `GameObject` which you can use to create your prefab.

![alt text](images/ar_default_plane.png "AR Default Plane")

Once created, you should assign it to the `ARPlaneManager`'s `Plane Prefab` field. You can use it directly or create a prefab by dragging the `GameObject` into your Assets folder.  It is recommended to save the `AR Default Plane` as a prefab first, delete the `AR Default Plane` GameObject, and then use that in the prefab field as leaving the plane in your scene will leave a zero scale plane artifact in the scene.

![alt text](images/ar_default_plane_as_prefab.png "AR Default Plane Prefab")

There is a similar menu item for point cloud visualization.

These components are included for ease of use, but you may wish to create your own visualizers (or other logic) as you see fit.

## Raycasting

Also known as hit testing, raycasting allows you to determine where a ray (defined by an origin and direction) intersects with a trackable. The current raycast interface only tests against planes and points in the point cloud. The raycasting interface is similar to the one in the Physics module, but since AR trackables may not necessarily have a presence in the physics world, we provide a separate interface.

The raycasting interface is on the `ARSessionOrigin`. This is because only the `ARSessionOrigin` can transform the raycast results from session space to Unity world space. There are two raycasting methods on the `ARSessionOrigin`:

```csharp
public bool Raycast(Vector3 screenPoint, List<ARRaycastHit> hitResults, TrackableType trackableTypeMask = TrackableType.All);
public bool Raycast(Ray ray, List<ARRaycastHit> hitResults, TrackableType trackableTypeMask = TrackableType.All, float pointCloudRaycastAngleInDegrees = 5f);
```

The first method takes a two dimensional position on the screen. You can, for example, pass a touch position directly:

```csharp
rig.Raycast(Input.GetTouch(0).position, ...);
```

The second method takes an arbitrary `Ray` (a position and direction).

The following table summarizes the other parameters:

| Parameter | Description |
|-|-|
| `hitResults` | The results for both methods are stored in this `List`, which must not be `null` (this lets you reuse the same `List` object to avoid garbage-collected allocations). |
| `trackableTypeMask` | The type(s) of trackable(s) to hit test against. This is a flag, so multiple types can be bitwise OR'd together, e.g., `TrackableType.PlaneWithinPolygon` &#124; `FeaturePoint` |
| `pointCloudRaycastAngleInDegrees` | Because feature points have no volume, a cone is used when casting against feature points instead of an infinitely thin ray. This parameter controls the width of the cone. |

## Face Tracking

See [Face Tracking Documentation](face-tracking.md).

# Technical details
## Requirements

This version of AR Foundation is compatible with the following versions of the Unity Editor:

* 2018.1 and later (recommended)

## Known limitations

AR Foundation includes the following known limitations:

* No known issues

## Document revision history

|Date|Reason|
|---|---|
|November 15, 2018|Face Tracking added.|
|July 25, 2018|Update `ARCameraBackground` image and description following refactor.<br/>Add howto section for blitting the camera image to a render texture.|
|July 16, 2018|Additional explanation for `ARSessionOrigin`|
|June 14, 2018|Update `ARSessionOrigin` photos|
|June 12, 2018|Update `ARPlaneMeshVisualizer` and `ARPointCloudMeshVisualizer` with additional debug recommendations and standards.|
|June 7, 2018|Remove known issue.|
|June 6, 2018|Update ARSession image.|
|April 25, 2018|Updated docs and screen shots after package rename.|
|April 19, 2018|Updated screen shots and information after code changes. Added section on `ARBackgroundRenderer` component. |
|April 10, 2018|Document created.|
