<div align="center">

# 21 Bullets

**Blackjack meets Russian roulette.**

A university game combining card decisions, a Russian-roulette mechanic and items across three levels.

![Unity](https://img.shields.io/badge/Unity-6000.0.52f1-292929?style=flat-square&logo=unity)
![CSharp](https://img.shields.io/badge/C%23-512bd4?style=flat-square)
![Project](https://img.shields.io/badge/Project-University-64748b?style=flat-square)

[My portfolio](https://jesuscaravaca.web.app/)

</div>

## The idea

21 Bullets mixes blackjack and Russian roulette, with items and three levels. It was developed as a university team project in Unity and C#.

## My contribution

I implemented the shooting mode and game items, and contributed to the **game design document (GDD)**.

## Open the project

Use **Unity 6000.0.52f1**, as recorded in `ProjectSettings/ProjectVersion.txt`.

```sh
git clone https://github.com/MagicNube/21Bullets.git
```

1. In Unity Hub, choose **Add project from disk**.
2. Select the nested `21 Bullets` folder, not the repository root. This is the folder containing `Assets`, `Packages` and `ProjectSettings`.
3. Wait for Unity to restore packages and import assets.
4. Open `Assets/Project/Escenas/01_SPLASH.unity` and enter Play mode.

The enabled build scenes start with `01_SPLASH`, followed by the menu and `03_MAIN`. `SampleScene` is disabled in the checked-in build settings. Use Unity's Build Profiles with the enabled scenes to create a standalone build.

## Project layout

```text
21 Bullets/
  Assets/Project/
    Escenas/       Splash, menu and gameplay
    Scripts/       Game implementation
    Prefabs/       Reusable game objects
    Arte/          Game art
    Audio/         Game audio
  Packages/        Unity dependencies
  ProjectSettings/ Editor version and build scenes
```

## Assets and licensing

The source tree includes third-party UI and other Unity assets. A repository-wide `LICENSE` and a consolidated attribution list have not yet been added; no blanket license is claimed here.
