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

    Animator selectedAnimator;
    string absolutePathToGameDirectory = "";
    int steamId = 0;
    bool useThisAvatarForLocalToo = true;
    long lastBundleSize;

    static string windowTitle = "AvatarCreature Generator";

    [MenuItem("PeanutTools/AvatarCreature Generator")]
    public static void ShowWindow() {
        var window = GetWindow<AvatarCreatureGeneratorEditorWindow>();
        window.titleContent = new GUIContent(windowTitle);
        window.minSize = new Vector2(400, 200);
    }

    void Start() {
        SetAbsolutePathToDefault();
    }

    void SetAbsolutePathToDefault() {
        // string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        // string defaultabsolutePathToGameDirectory = Path.Combine(roamingPath, "Thunderstore Mod Manager/DataFolder/LethalCompany/profiles/Default");

        string defaultabsolutePathToGameDirectory = Path.Combine("E:/SteamLibrary/steamapps/common/Lethal Company");

        absolutePathToGameDirectory = defaultabsolutePathToGameDirectory;

        Debug.Log($"AvatarCreatureGenerator :: Set path to '{absolutePathToGameDirectory}'");
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

        selectedAnimator = (Animator)EditorGUILayout.ObjectField("Animator", (Animator)selectedAnimator, typeof(Animator));
        
        if (selectedAnimator != null && selectedAnimator.avatar == null) {
            CustomGUI.RenderWarningMessage("The selected animator is missing an avatar.");
        }

        CustomGUI.SmallLineGap();

        steamId = CustomGUI.IntInput("Steam ID", steamId);
        CustomGUI.ItalicLabel("Go to steamidfinder.com and copy the 'steamID64 (Dec)' value (all numbers)");
        CustomGUI.RenderLink("Click here to open steamidfinder.com", "https://www.steamidfinder.com");

        CustomGUI.SmallLineGap();

        useThisAvatarForLocalToo = CustomGUI.Checkbox("Use for local too", useThisAvatarForLocalToo);
        CustomGUI.ItalicLabel("LAN games use Steam ID '0'");
        
        CustomGUI.SmallLineGap();

        absolutePathToGameDirectory = EditorGUILayout.TextField("LC Install Path", absolutePathToGameDirectory);
        // CustomGUI.ItalicLabel("Thunderstore => Lethal Company => Settings => Locations => Browse profile folder");
        CustomGUI.ItalicLabel("The install path to Lethal Company. Avatars will be placed into a folder named 'Avatars' inside it.");

        if (CustomGUI.StandardButton("Select Folder")) {
            string newabsolutePathToGameDirectory = EditorUtility.OpenFolderPanel("Select LC install directory", absolutePathToGameDirectory, "");

            if (newabsolutePathToGameDirectory != null) {
                absolutePathToGameDirectory = newabsolutePathToGameDirectory;
            }
        }

        if (CustomGUI.StandardButton("Set To Default")) {
            SetAbsolutePathToDefault();
        }
            
        CustomGUI.SmallLineGap();

        if (GUILayout.Button("Generate", GUILayout.Width(300), GUILayout.Height(25))) {
            if (EditorUtility.DisplayDialog("Confirm", "Are you sure this is a copy of your original project and you want to proceed?", "Yes", "No")) {
                Generate();
            }
        }

        CustomGUI.SmallLineGap();

        // if (lastBundleSize != null) {
        //     CustomGUI.LargeLabel($"Last bundle size: {lastBundleSize / (1024 * 1024)} MB");
        //     CustomGUI.ItalicLabel("You need to manually share these bundles with your friends. Keep them small - below 10mb.");
        //     CustomGUI.ItalicLabel("Reduce texture size to 512x512.");
        // }

        EditorGUILayout.EndScrollView();
    }

    void AddToBundleIfNotAlready(List<string> currentAssets, string pathToAsset) {
        if (pathToAsset != "" && !currentAssets.Contains(pathToAsset)) {
            currentAssets.Add(pathToAsset);
        }
    }

    void Generate() {
        if (selectedAnimator == null || absolutePathToGameDirectory == "" || steamId == 0) {
            Debug.LogWarning("AvatarCreatureGenerator :: Missing something");
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
                    string newFilename = "model.fbx";
                    string pathToNewModelFile = Path.Combine(dir, newFilename);

                    Debug.Log($"AvatarCreatureGenerator :: Renaming FBX '{pathToModelFile}' to '{pathToNewModelFile}'");

                    string result = AssetDatabase.RenameAsset(pathToModelFile, newFilename);

                    if (result != "") {
                        throw new System.Exception($"Failed to rename");
                    }
                    
                    AssetDatabase.Refresh();

                    AddToBundleIfNotAlready(assetsToAdd, pathToNewModelFile);

                    Debug.Log($"AvatarCreatureGenerator :: Added renderer '{rendererName}' ('{pathToNewModelFile}') to bundle");
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

                    AddToBundleIfNotAlready(assetsToAdd, AssetDatabase.GetAssetPath(material.GetTexture("_MainTex")));
                    AddToBundleIfNotAlready(assetsToAdd, AssetDatabase.GetAssetPath(material.GetTexture("_GlossMap")));
                    AddToBundleIfNotAlready(assetsToAdd, AssetDatabase.GetAssetPath(material.GetTexture("_EmissionMap")));
                    AddToBundleIfNotAlready(assetsToAdd, AssetDatabase.GetAssetPath(material.GetTexture("_BumpMap")));

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

        lastBundleSize = Utils.GetSizeOfAssetBundle(pathToAssetBundle);

        string pathToDestBundle = Path.Combine(absolutePathToGameDirectory, "Avatars", $"{assetBundleName}.assetbundle");

        Utils.CopyFileAndOverwrite(pathToAssetBundle, pathToDestBundle);

        if (useThisAvatarForLocalToo) {
            string pathToLocalBundle = Path.Combine(absolutePathToGameDirectory, "Avatars", $"0.assetbundle");

            Utils.CopyFileAndOverwrite(pathToAssetBundle, pathToLocalBundle);
        }

        EditorUtility.DisplayDialog("Success", $"Outputted to '{pathToDestBundle}'", "OK"); //  ({lastBundleSize / (1024 * 1024)} MB)
    }
}