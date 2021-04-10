# Stream Deck for Unity

Enables Stream Deck integration within Unity.

**It should work with any Unity version as long it targets .NET 4.0+.**

## Getting Started

1. Download the _Unity_ plugin for Stream Deck on Stream Deck Store or [here](https://github.com/nicollasricas/streamdeck-unity/releases/latest).

2. Download the [Unity package](https://github.com/nicollasricas/unity-streamdeck/releases/latest) and [import](https://docs.unity3d.com/Manual/AssetPackages.html) in any Unity project you wish to use it.

If you have downloaded the plugin and imported the package into your project, you should see this message:

![Stream Deck Connected](https://user-images.githubusercontent.com/7860985/114270390-0f16ba00-99da-11eb-999b-fd90fb74cc95.png)

**If for some reason the live link lost the connection it will automatically reconnect.**

## Features

- Add components to objects
- Execute menus
- Paste components
- Pause/resume play mode
- Reset and rotate objects
- Select objects by name or tag
- Switch between scene and game view
- Toggle objects states

## Settings

To open the package settings go to the menu: **Tools > Stream Deck**.

![Settings](https://user-images.githubusercontent.com/7860985/114270056-3e2c2c00-99d8-11eb-8380-928377867142.png)

## Execute Menu

To execute a menu, you have to write the full path, spaces included. Submenus use a slash (/) as a divider.

> Window/Layouts/4 Split

![4 Split Layout Menu](https://user-images.githubusercontent.com/7860985/114270122-ab3fc180-99d8-11eb-80eb-341ef5182b3f.png)

![4 Split Layout Action](https://user-images.githubusercontent.com/7860985/114270198-15f0fd00-99d9-11eb-9898-510441127c8d.png)

## FAQ

- The live link is not connecting

  > Make sure you have installed the [stream deck plugin](https://github.com/nicollasricas/streamdeck-unity/releases/latest), imported the unity package into your project, the port you are using are available and the stream deck software is open. **If you have to change the port don't forget to change it on the [stream deck plugin](https://github.com/nicollasricas/streamdeck-unity)**.

- The default port is been used by another software

  > You can change the port on the [settings](#Settings) menu, don't forget to change it on the [stream deck plugin](https://github.com/nicollasricas/streamdeck-unity).

- Will this plugin be built into the compiled project??

  > No, the plugin is tagged as editor only so it will only work on the unity editor.

- I'm getting an `Multiple precompiled assemblies with the same name websocket-sharp.dll included or the current platform` error

  > This plugin requires the `Websocket-sharp` library and if you are getting this error, it means that another package in your project is using it. In this case, I recommend that you remove the one from this package (`Plugins/StreamDeck/Editor/websocket-sharp.dll`) since it's tagged as editor only and if your build depends on it better let the other win the conflict.
