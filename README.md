# AvatarCreatures

**YouTube guide for converting a VRC avatar into the mod: https://www.youtube.com/watch?v=FCsUxyJM4yo**

**Your avatar is only visible to other people - it does not replace your own hands and is not compatible with 3rd person mod**

A Lethal Company mod that lets you load custom avatars for players in your lobby.

**You only see other people's avatars not your own.** If you know how to hide the default hands please open an issue in my GitHub repo.

Each player must have the AssetBundle for each player to see them properly:

1. Use the Unity plugin to generate an AssetBundle for your own avatar in your `steamapps/common/Lethal Company/Avatars` folder
2. Share this AssetBundle with your friends (eg. `12345678912345678.assetbundle`) and make sure everyone's AssetBundle is in the same "Avatars" folder
3. Run the mod and you should see them

Forked from DarnHyena's [LethalCreatures mod](https://github.com/DarnHyena/LethalCreatures)

## 3rd person mods

This mod is not compatible with 3rd person mods.

You _can_ look at a broken version of yourself with 3rd person mods and enabling setting `AlwaysRenderLocalPlayerModel` and restarting the game.

## Development

### Unity plugin

1. Open Unity project

2. Copy everything in `UnityPlugin` to `Assets` folder inside Unity project

### Mod

1. Open solution in Visual Studio 2022
2. Change `.csproj` references:

   LC_API => `C:/Users/<username>/AppData/Roaming/Thunderstore Mod Manager/DataFolder/LethalCompany/profiles/Default/BepInEx/plugins/2018-LC_API/LC_API.dll`

   Assembly-CSharp/everything else => `steamapps/common/Lethal Company/Lethal Company_Data/Managed/<name>.dll`

3. Restore NuGet packages
4. Build project
5. Copy `LethalCreatures/bin/Debug/netstandard2.1/CreatureModels.dll` to `C:/Users/<username>/AppData/Roaming/Thunderstore Mod Manager/DataFolder/LethalCompany/profiles/Default/BepInEx/plugins/PeanutBuddha-AvatarCreature/CreatureModels.dll`
6. (optionally) From Unity use official plugin "AssetBundle Browser" to create a new bundle named "core" with these files (the mod uses another mod to load them from their own assetbundle):

   - `UnityAssets/CritterControl.controller`
   - `UnityAssets/CtritterIdle.anim`

7. Launch game

## Publish

NOTE: Include unity plugin with the mod to make it easier for people.

### Unity plugin

1. In Unity project export `PeanutTools/AvatarCreatureGenerator` inclusive as `PeanutTools_AvatarCreatureGenerator_X.X.X.unitypackage`

2. Copy into mod folder `C:/Users/<username>/AppData/Roaming/Thunderstore Mod Manager/DataFolder/LethalCompany/profiles/Default/BepInEx/plugins/PeanutBuddha-AvatarCreature`

### Mod

1. Ensure the manifest file is up to date

2. Run `./packageup.ps1` to copy everything into the plugin directory

3. Compress the plugin directory into `PeanutBuddha-AvatarCreature-X.X.X.zip`

4. Release
