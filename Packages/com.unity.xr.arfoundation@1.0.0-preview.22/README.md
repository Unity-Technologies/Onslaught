# AR Foundation

Use the AR Foundation package to add high-level functionality for working with augmented reality. Unity 2018.1 includes built-in multi-platform support for AR. These APIs are in the `UnityEngine.Experimental.XR` namespace, and consist of a number of `Subsystem`s, e.g., `XRPlaneSubsystem`. Several XR Subsystems comprise the low-level API for interacting with AR. The **AR Foundation** package wraps this low-level API into a cohesive whole and enhances it with additional utilities, such as AR session lifecycle management and the creation of `GameObject`s to represent detected features in the environment.

AR Foundation is a set of utilities for dealing with devices that support following concepts:
- Planar surface detection
- Point clouds, aka feature points
- Reference points: an arbitrary position and orientation that the device tracks
- Light estimation: estimates for average color temperature and brightness in physical space.
- World tracking: tracking the device's position and orientation in physical space.
- Face tracking: tracking the position and orientation of the face of a person.

## Installing AR Foundation

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

This package is `com.unity.xr.arfoundation`

## Documentation

* [Script API](Runtime/AR/)
* [Manual](Documentation/com.unity.xr.arfoundation.md)
