---
permalink: /ManualInstallation.html
title: Manual Installation
description: the flat-pack Kiea instructions, written in Kerbalese, unusally present
# layout: bare
tags: installation,directions,page,kerbal,ksp,zer0Kerbal,zedK
---

<!-- ManualInstallation.md v1.1.7.0
Surface Experiment Pack (SEP)
created: 01 Oct 2019
updated: 18 Apr 2022 -->

<!-- based upon work by Lisias -->

# Surface Experiment Pack (SEP)

[Home](./index.md)

An addon that adds science packages that must be constructed prior to use for Kerbal Space Program.

## Installation Instructions

### Using CurseForge/OverWolf app or CKAN

You should be all good! (check for latest version on CurseForge)

### If Downloaded from CurseForge/OverWolf manual download

To install, place the `SurfaceExperimentPack` folder inside your Kerbal Space Program's `GameData` folder:

* **REMOVE ANY OLD VERSIONS OF THE PRODUCT BEFORE INSTALLING**, including any other fork:
  * Delete `<KSP_ROOT>/GameData/SurfaceExperimentPack`
* Extract the package's `SurfaceExperimentPack/` folder into your KSP's GameData folder as follows:
  * `<PACKAGE>/SurfaceExperimentPack` --> `<KSP_ROOT>/GameData`
    * Overwrite any preexisting folder/file(s).
  * you should end up with `<KSP_ROOT>/GameData/SurfaceExperimentPack`

### If Downloaded from SpaceDock / GitHub / other

To install, place the `GameData` folder inside your Kerbal Space Program folder:

* **REMOVE ANY OLD VERSIONS OF THE PRODUCT BEFORE INSTALLING**, including any other fork:
  * Delete `<KSP_ROOT>/GameData/SurfaceExperimentPack`
* Extract the package's `GameData` folder into your KSP's root folder as follows:
  * `<PACKAGE>/GameData` --> `<KSP_ROOT>`
    * Overwrite any preexisting file.
  * you should end up with `<KSP_ROOT>/GameData/SurfaceExperimentPack`

## The following file layout must be present after installation

```markdown
<KSP_ROOT>
  + [GameData]
    + [SurfaceExperimentPack]
      + [Agencies]
        ...
      + [Compatibility]
        ...
      + [Flags]
        ...
      + [Localization]
        ...
      + [Parts]
        ...
      + [Plugins]
        + [PluginData]
          * Settings.cfg
          * ...
        * SurfaceExperimentPack.dll
        * SEPScience.dll
        * SEPScience.Unity.dll
        ...
      + [Resources]
        ...
      * #.#.#.#.htm
      * changelog.md
      * SimpleBSD-2.txt
      * readme.htm
      * SurfaceExperimentPack.version
    ...
    * [Module Manager][mm] or [Module Manager /L][mml]
  * KSP.log
  ...
```

### Dependencies

* [KIS][kis]
* [KAS][kas]
* *either*
  * [Module Manager][mm]
  * [Module Manager /L][mml]

[kas]: https://forum.kerbalspaceprogram.com/index.php?/topic/142594-*/ "Kerbal Attachment System (KAS)"
[kis: https://forum.kerbalspaceprogram.com/index.php?/topic/149848-*/ "Kerbal Inventory System (KIS)"
[mm]: https://forum.kerbalspaceprogram.com/index.php?/topic/50533-*/ "Module Manager"
[mml]: https://github.com/net-lisias-ksp/ModuleManager "Module Manager /L"
