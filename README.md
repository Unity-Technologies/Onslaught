# Onslaught
A cross-platform XR sample game with no third-party packages.

This project is made as a part of "Scenario Test Week" - a time during the beta period of our editor release cycle when we do exploratory testing by making games.  As such, releases will target beta versions of the editor and use experimental APIs. Your mileage may vary using this project with a different version of the editor than the one the project targets.

This game can be deployed to...
Standalone (PC): OpenVR, Oculus, or Windows Mixed Reality Immersive Headsets
Android: Oculus, Daydream, ARCore
iOS: ARKit

To compile for VR, set up the ProjectSetting->Player->XRSettings to your target VR SDK(s) and build the scene Assets/Scenes/VR.unity to the corresponding platform.

To compile for AR, disable VR support and build the scene Assets/Scenes/AR.unity to the corresponding platform.  Do NOT enable XRSettings->ARCoreSupport when targeting AR.  This project uses ARFoundation for AR support, which does not use that settings path.
