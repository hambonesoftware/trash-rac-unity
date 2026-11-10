# Trash Rac

[Play Back Alley Rumble on GitHub Pages](https://hambonesoftware.github.io/trash-rac-unity/)

Milo’s Market stacks empty cartons by the dumpster for the morning recycling pickup. For one raccoon, those cartons are the only thing between him and dinner.

- Click or tap the board to aim and throw. Arrow keys aim and Enter throws.
- Hit a lower carton to send the cartons above it tumbling.
- Press Space or use the on-screen Hide button when the stockroom door lights up. Use the same control to peek out again.
- Workers add cartons and throw one at a spotted raccoon. Get behind the metal can before it lands. Three hits end the raid.

## Open in Unity

1. Install **Unity 6000.3.28f1** with **Web Build Support**.
2. Add this repository’s root as a project in Unity Hub.
3. Open `Assets/Scenes/BackAlley.unity` and press Play.

`AlleyArt.cs` and `AlleyScenery.cs` assemble the alley and props. `AlleyCharacters.cs` builds and poses the raccoon and stock clerk rigs. `AlleyEffects.cs` draws the comic bursts and cardboard scraps. `AlleyGame.cs` runs the round and tracks which cartons support each stack. `AlleyToon.shader` renders the lighting bands, ink outlines, and stockroom spill. The models and poses are generated in Unity and require no external art or animation downloads.

## Build the web edition

With the Unity editor closed, run the project’s batch entry point. This PowerShell example uses the default Unity Hub installation path.

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.3.28f1/Editor/Unity.exe' -batchmode -nographics -quit -projectPath (Get-Location).Path -buildTarget WebGL -executeMethod TrashRac.Editor.AlleyBuild.BuildWeb -logFile web-build.log
```

The build writes `Builds/Web`. Serve that directory over HTTP for local play. GitHub Pages publishes the root of the `gh-pages` branch. To update the live game, copy the contents of `Builds/Web` into that branch, including `.nojekyll`, and push it. The build uses Unity’s browser decompression fallback so Pages can serve it with ordinary static file headers.
