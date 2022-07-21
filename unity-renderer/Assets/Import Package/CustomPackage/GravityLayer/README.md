Gravity SDK is a tool for authorizing users who own wearables from Gravity Layer and loading assets into a scene in Unity.

# Installation

Copy the files from this repository into a separate folder in your Unity project. For instance, into `Assets/Plugins/GravityLayer/CoreAsync`.

Alternatively you can download a Unity package file from [Releases](https://github.com/Gravity-Studio-Digital-Wear/UnitySDK-CoreAsync/releases). To install the package
1. Go to “Assets / Import Package / Custom Package”
2. Select `gravitylayer-unitysdk-coreasync.unitypackage`
3. Click “Import”

# Usage

A single entry point for Gravity SDK is the class `GravityLayerEntryPoint`.

You can connect to Gravity API by calling `GravityLayerEntryPoint.GLayerConnection.EstablishConnection()`

You can fetch user’s wearables by calling `GravityLayerEntryPoint.Wardrobe.FetchWearables()`

For more details see our [documentation](https://gravity-studio-digital-wear.github.io/gravity-docs/).
