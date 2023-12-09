using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using PeanutTools_AvatarCreatureGenerator;

public class AvatarCreatureGeneratorEditorWindow : EditorWindow {
    Vector2 scrollPosition;
    long lastBundleSize;

    [SerializeField]
    Animator selectedAnimator;

    [SerializeField]
    string absolutePathToGameDirectory = "";

    [SerializeField]
    long steamId = 0;

    [SerializeField]
    bool useThisAvatarForLocalToo = true;

    static string windowTitle = "AvatarCreature Generator";

    [MenuItem("PeanutTools/AvatarCreature Generator")]
    public static void ShowWindow() {
        var window = GetWindow<AvatarCreatureGeneratorEditorWindow>();
        window.titleContent = new GUIContent(windowTitle);
        window.minSize = new Vector2(400, 200);
    }

    void OnEnable() {
        Debug.Log($"AvatarCreatureGenerator :: Loading prefs");
        var json = EditorPrefs.GetString("AvatarCreatureGeneratorEditorWindow", JsonUtility.ToJson(this, false));
        JsonUtility.FromJsonOverwrite(json, this);
    }

    void OnDisable() {
        Debug.Log($"AvatarCreatureGenerator :: Saving prefs");
        var json = JsonUtility.ToJson(this, false);
        EditorPrefs.SetString("AvatarCreatureGeneratorEditorWindow", json);
    }

    void SetAbsolutePathToDefault() {
        // string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        // string defaultabsolutePathToGameDirectory = Path.Combine(roamingPath, "Thunderstore Mod Manager/DataFolder/LethalCompany/profiles/Default");

        string defaultabsolutePathToGameDirectory = Path.Combine("C:/Program Files (x86)/Steam/steamapps/common/Lethal Company");

        absolutePathToGameDirectory = defaultabsolutePathToGameDirectory;

        Debug.Log($"AvatarCreatureGenerator :: Set path to '{absolutePathToGameDirectory}'");
    }

    bool IsAllowedToGenerate() {
        return selectedAnimator != null && selectedAnimator.avatar != null && GetIsPathToGameDirectoryValid(absolutePathToGameDirectory) && steamId.ToString().Length == 17;
    }

    bool GetIsPathToGameDirectoryValid(string path) {
        return path != "" && path != null && Directory.Exists(path);
    }

    void OnGUI() {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        CustomGUI.LargeLabel("Avatar Creature Generator");
        CustomGUI.ItalicLabel("Generates an AvatarCreature from a Unity animator.");

        CustomGUI.SmallLineGap();
        
        GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
        guiStyle.fontSize = 16;
        guiStyle.normal.textColor = new Color(1.0f, 0.5f, 0.5f);
        GUILayout.Label("Remember to make a COPY of your original project", guiStyle);
        GUILayout.Label("This tool RENAMES your stuff", guiStyle);
        
        CustomGUI.SmallLineGap();

        CustomGUI.LargeLabel("Step 1: Insert your avatar");
        
        CustomGUI.SmallLineGap();

        selectedAnimator = (Animator)EditorGUILayout.ObjectField("Animator", (Animator)selectedAnimator, typeof(Animator));
        
        if (selectedAnimator != null && selectedAnimator.avatar == null) {
            CustomGUI.SmallLineGap();
            CustomGUI.RenderErrorMessage("ERROR: The selected animator is missing an avatar.");
        }

        CustomGUI.SmallLineGap();

        CustomGUI.LargeLabel("Step 2: Paste your 17 digit Steam ID");
        
        CustomGUI.SmallLineGap();

        steamId = CustomGUI.LongInput("Steam ID", steamId);
        CustomGUI.RenderLink("Click here to open steamidfinder.com", "https://www.steamidfinder.com");
        CustomGUI.ItalicLabel("and copy steamID64 (Dec)");

        if (steamId != 0 && steamId.ToString().Length != 17) {
            CustomGUI.SmallLineGap();
            CustomGUI.RenderErrorMessage("ERROR: Steam ID is not 17 digits");
        }

        CustomGUI.SmallLineGap();

        CustomGUI.LargeLabel("Step 3: Options");
        
        CustomGUI.SmallLineGap();

        useThisAvatarForLocalToo = CustomGUI.Checkbox("Use for local too", useThisAvatarForLocalToo);
        CustomGUI.ItalicLabel("Use your avatar for LAN games (steam ID '0').");
        CustomGUI.ItalicLabel("You should probably leave this enabled.");
        
        CustomGUI.SmallLineGap();

        CustomGUI.LargeLabel("Step 4: Insert path to Lethal Company install folder");
        
        CustomGUI.SmallLineGap();

        absolutePathToGameDirectory = EditorGUILayout.TextField("Install Folder", absolutePathToGameDirectory);
        // CustomGUI.ItalicLabel("Thunderstore => Lethal Company => Settings => Locations => Browse profile folder");
        CustomGUI.ItalicLabel("The folder that contains 'Lethal Company.exe'.");
        CustomGUI.ItalicLabel($"AssetBundles will output to {absolutePathToGameDirectory}/Avatars");

        GUILayout.BeginHorizontal();

        if (CustomGUI.StandardButton("Select Folder")) {
            string newAbsolutePathToGameDirectory = EditorUtility.OpenFolderPanel("Select LC install directory", absolutePathToGameDirectory, "");

            if (newAbsolutePathToGameDirectory != null) {
                absolutePathToGameDirectory = newAbsolutePathToGameDirectory;
            }
        }

        if (CustomGUI.StandardButton("Open")) {
            Debug.Log($"AvatarCreatureGenerator :: Opening '{absolutePathToGameDirectory}'");
            EditorUtility.RevealInFinder(absolutePathToGameDirectory);
        }

        if (CustomGUI.StandardButton("Set To Default")) {
            SetAbsolutePathToDefault();
        }
        
        GUILayout.EndHorizontal();

        if (absolutePathToGameDirectory != "" && !GetIsPathToGameDirectoryValid(absolutePathToGameDirectory)) {
            CustomGUI.SmallLineGap();
            CustomGUI.RenderErrorMessage("ERROR: Path does not exist");
        }

        CustomGUI.SmallLineGap();
            
        CustomGUI.LargeLabel("Step 5: Generate");
        
        CustomGUI.SmallLineGap();

        // EditorGUI.BeginDisabledGroup(!IsAllowedToGenerate());
        if (CustomGUI.PrimaryButton("Generate")) {
            if (EditorUtility.DisplayDialog("Confirm", "Are you sure this is a copy of your original project and you want to proceed?", "Yes", "No")) {
                Generate();
            }
        }
        // EditorGUI.EndDisabledGroup();

        if (lastBundleSize != null) {
            CustomGUI.SmallLineGap();

            CustomGUI.MediumLabel($"Last bundle size: {lastBundleSize / (1024 * 1024)} MB");
        }

        CustomGUI.SmallLineGap();
        
        CustomGUI.LargeLabel("How it works");
        
        CustomGUI.SmallLineGap();

        GUILayout.Label(
        "1. Finds every Renderer in the avatar (eg. SkinnedMeshRenderer, MeshRenderer)\n\n" +
        "2. Finds every Material\n\n" +
        "3. Finds the albedo, smoothness map, normal map and emission map and renames and formats them\n\n" +
        "4. Adds the Material to the assetbundle\n\n" +
        "5. Adds the Renderer's model (.fbx) to the assetbundle\n\n" +
        "6. Builds assetbundle then moves it to the correct spot");

        EditorGUILayout.EndScrollView();
    }

    void AddToBundleIfNotAlready(List<string> currentAssets, string pathToAsset) {
        if (pathToAsset != "" && !currentAssets.Contains(pathToAsset)) {
            Debug.Log($"AvatarCreatureGenerator :: Asset '{pathToAsset}' not in bundle ({currentAssets.Count}), adding");
            currentAssets.Add(pathToAsset);
        }
    }

    void ProcessTexture(List<string> assetsToAdd, Material material, string textureNameInMaterial) {
        Texture texture = material.GetTexture(textureNameInMaterial);
        
        string pathToTextureFile = AssetDatabase.GetAssetPath(texture);

        TextureImporter textureImporter = AssetImporter.GetAtPath(pathToTextureFile) as TextureImporter;

        if (textureImporter != null) {
            if (textureImporter.maxTextureSize != 512) {
                Debug.Log($"AvatarCreatureGenerator :: Texture '{pathToTextureFile}' is not 512, resizing");

                textureImporter.maxTextureSize = 512;
                AssetDatabase.ImportAsset(pathToTextureFile);
            }
        }

        AddToBundleIfNotAlready(assetsToAdd, pathToTextureFile);
    }

    void Generate() {
        if (!IsAllowedToGenerate()) {
            Debug.LogWarning("AvatarCreatureGenerator :: Missing something");
            Debug.Log(selectedAnimator);
            Debug.Log(selectedAnimator.avatar);
            Debug.Log(absolutePathToGameDirectory);
            Debug.Log(steamId);
            return;
        }

        // TODO: Verify animator has an avatar set

        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
 
        string assetBundleName = $"{steamId}";

        buildMap[0].assetBundleName = assetBundleName;

        var renderers = selectedAnimator.gameObject.GetComponentsInChildren<Renderer>();

        var assetsToAdd = new List<string>();
        
        // NOTE: AssetDatabase.GetAssetPath always starts with Assets/
        
        foreach (Renderer renderer in renderers) {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(renderer.gameObject);

            string rendererName = renderer.gameObject.name;

            if (prefab == null) {
                Debug.Log($"AvatarCreatureGenerator :: Renderer '{rendererName}' is not a prefab - ignoring");
            } else {
                var pathToModelFile = AssetDatabase.GetAssetPath(prefab);

                if (pathToModelFile == "") {
                    Debug.Log($"AvatarCreatureGenerator :: Renderer '{rendererName}' does not exist in the project");
                } else if (!pathToModelFile.Contains(".fbx")) {
                    Debug.Log($"AvatarCreatureGenerator :: Renderer '{rendererName}' does not use a FBX (not supported)");
                } else {
                    string dir = Path.GetDirectoryName(pathToModelFile);

                    if (Path.GetFileName(pathToModelFile) != "model.fbx") {
                        string newFilename = "model.fbx";
                        string pathToNewModelFile = Path.Combine(dir, newFilename);
                        string absolutePathToNewModelFile = Path.GetFullPath(Path.Combine(Application.dataPath, "../", pathToNewModelFile));

                        Debug.Log($"AvatarCreatureGenerator :: Renaming FBX '{pathToModelFile}' to '{pathToNewModelFile}' ('{absolutePathToNewModelFile}')");

                        if (File.Exists(absolutePathToNewModelFile)) {
                            Debug.Log($"AvatarCreatureGenerator :: New FBX already exists, deleting...");
                            File.Delete(absolutePathToNewModelFile);
                        }

                        string result = AssetDatabase.RenameAsset(pathToModelFile, newFilename);

                        if (result != "") {
                            throw new System.Exception($"Failed to rename");
                        }
                        
                        AssetDatabase.Refresh();
                        
                        AddToBundleIfNotAlready(assetsToAdd, pathToNewModelFile);
                    } else {
                        AddToBundleIfNotAlready(assetsToAdd, pathToModelFile);
                    }
                }
            }

            int idx = 0;

            foreach (Material material in renderer.sharedMaterials) {
                Debug.Log($"AvatarCreatureGenerator :: Material #{idx} - '{material.name}'");

                string pathToMaterial = AssetDatabase.GetAssetPath(material);

                if (pathToMaterial == "") {
                    Debug.Log($"AvatarCreatureGenerator :: Material '{pathToMaterial}' does not exist in the project");
                } else {                    
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(material.GetTexture("_MainTex")), $"{rendererName}_{idx}_Albedo.png");
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(material.GetTexture("_GlossMap")), $"{rendererName}_{idx}_Smoothness.png");

                    string pathToAlbedo = AssetDatabase.GetAssetPath(material.GetTexture("_MainTex"));
                    string pathToEmission = AssetDatabase.GetAssetPath(material.GetTexture("_EmissionMap"));

                    if (pathToEmission == pathToAlbedo) {
                        string absolutePathToAlbedo = Path.GetFullPath(Path.Combine(Application.dataPath, "../", pathToAlbedo));
                        string absolutePathToNewEmission = Path.GetFullPath(Path.Combine(Application.dataPath, "../", pathToAlbedo, "../", $"{rendererName}_{idx}_Emission.png"));

                        Debug.Log($"AvatarCreatureGenerator :: Emission file '{pathToEmission}' is same as albedo '{pathToAlbedo}', making copy");

                        Utils.CopyFileAndOverwrite(absolutePathToAlbedo, absolutePathToNewEmission);
                    } else {
                        AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(material.GetTexture("_EmissionMap")), $"{rendererName}_{idx}_Emission.png");
                    }

                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(material.GetTexture("_BumpMap")), $"{rendererName}_{idx}_Normal.png");

                    ProcessTexture(assetsToAdd, material, "_MainTex");
                    ProcessTexture(assetsToAdd, material, "_GlossMap");
                    ProcessTexture(assetsToAdd, material, "_EmissionMap");
                    ProcessTexture(assetsToAdd, material, "_BumpMap");

                    AssetDatabase.Refresh();

                    AddToBundleIfNotAlready(assetsToAdd, pathToMaterial);
                    
                    Debug.Log($"AvatarCreatureGenerator :: Added material '{pathToMaterial}' to bundle");
                }

                idx++;
            }
        }

        foreach (string pathToAsset in assetsToAdd) {
            Debug.Log($"AvatarCreatureGenerator :: Asset '{pathToAsset}'");
        }
 
        buildMap[0].assetNames = assetsToAdd.ToArray();

        string pathToAssetBundles = Path.GetFullPath(Path.Combine(Application.dataPath, "../AssetBundles"));

        string pathToAssetBundle = Path.Combine(pathToAssetBundles, assetBundleName);

        Debug.Log($"AvatarCreatureGenerator :: Building asset bundle to '{pathToAssetBundle}'");

        Utils.MakeDirectoryIfNotExist(pathToAssetBundles);
 
        BuildPipeline.BuildAssetBundles("AssetBundles", buildMap, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows);

        lastBundleSize = new System.IO.FileInfo(pathToAssetBundle).Length;

        string pathToDestBundle = Path.Combine(absolutePathToGameDirectory, "Avatars", $"{assetBundleName}.assetbundle");

        Utils.CopyFileAndOverwrite(pathToAssetBundle, pathToDestBundle);

        if (useThisAvatarForLocalToo) {
            string pathToLocalBundle = Path.Combine(absolutePathToGameDirectory, "Avatars", $"0.assetbundle");

            Utils.CopyFileAndOverwrite(pathToAssetBundle, pathToLocalBundle);
        }

        EditorUtility.DisplayDialog("Success", $"Outputted to '{pathToDestBundle}' ({lastBundleSize / (1024 * 1024)} MB)", "OK");
    }
}