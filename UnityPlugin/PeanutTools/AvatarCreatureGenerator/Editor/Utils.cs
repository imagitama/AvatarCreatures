using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace PeanutTools_AvatarCreatureGenerator {
    public class Utils {
        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        public static void MakeDirectoryIfNotExist(string folderPath) {
            if (!Directory.Exists(folderPath)) {
                Debug.Log($"AvatarCreatureGenerator :: Creating '{folderPath}'...");
                Directory.CreateDirectory(folderPath);
            } else {
                Debug.Log($"AvatarCreatureGenerator :: Already exists '{folderPath}'");
            }
        }

        public static void MoveFileAndOverwrite(string src, string dest) {
            try
            {
                Debug.Log($"AvatarCreatureGenerator :: Moving file from '{src}' to '{dest}'");

                if (File.Exists(dest)) {
                    File.Delete(dest);
                }

                File.Move(src, dest);
                
                Debug.Log("AvatarCreatureGenerator :: File moved successfully");
            }
            catch (IOException e)
            {
                Debug.LogError($"AvatarCreatureGenerator :: Error moving file: {e.Message}");
            }
        }

        public static void CopyFileAndOverwrite(string src, string dest) {
            try
            {
                Debug.Log($"AvatarCreatureGenerator :: Copying file from '{src}' to '{dest}'");

                if (File.Exists(dest)) {
                    File.Delete(dest);
                }

                File.Copy(src, dest);
                
                Debug.Log("AvatarCreatureGenerator :: File copied successfully");
            }
            catch (IOException e)
            {
                Debug.LogError($"AvatarCreatureGenerator :: Error copying file: {e.Message}");
            }
        }

        public static GameObject FindGameObjectByPath(string pathToGameObject) {
            GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (GameObject rootGameObject in rootGameObjects) {
                if (GetGameObjectPath(rootGameObject) == pathToGameObject) {
                    return rootGameObject;
                }

                Transform[] transforms = rootGameObject.GetComponentsInChildren<Transform>(true);
                
                foreach (Transform transform in transforms) {
                    if (GetGameObjectPath(transform.gameObject) == pathToGameObject) {
                        return transform.gameObject;
                    }
                }
            }

            return null;
        }

        // does NOT start with slash
        public static string GetRelativeGameObjectPath(GameObject objToFind, GameObject rootObj) {
            return GetGameObjectPath(objToFind).Replace(GetGameObjectPath(rootObj), "");
        }

        public static string GetPathRelativeToAssets(string path) {
            return Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "Assets");
        }

        public static string GetDirectoryPathRelativeToAssets(string path) {
            return GetPathRelativeToAssets(Directory.GetParent(path).FullName);
        }

        public static int StringToInt(string val) {
            return System.Int32.Parse(val);
        }

        public static Transform FindChild(Transform source, string pathToChild) {
            if (pathToChild.Length == 0) {
                return null;
            }

            if (pathToChild.Substring(0, 1) == "/") {
                if (pathToChild.Length == 1) {
                    return source;
                }

                pathToChild = pathToChild.Substring(1);
            }

            return source.Find(pathToChild);
        }

        public static Material CreateQuestMaterial(string originalMaterialPath, Material originalMaterial, bool useToonShader = false, bool placeQuestMaterialsInOwnDirectory = true) {
            Debug.Log($"Create quest material {originalMaterialPath}...");

            string pathToMaterialTemplate = "Assets/PeanutTools/AvatarCreatureGenerator/Materials/QuestTemplate" + (useToonShader ? "Toon" : "Standard") + ".mat";
            Material materialTemplate = GetMaterialAtPath(pathToMaterialTemplate);

            string pathToParentDir = Utils.GetDirectoryPathRelativeToAssets(originalMaterialPath);

            string pathToNewParentDir = pathToParentDir + "/" + (placeQuestMaterialsInOwnDirectory ? "Quest/" : "");

            string pathToDest = pathToNewParentDir + Path.GetFileName(originalMaterialPath).Replace(".mat", " Quest.mat");

            Material existingMaterial = LooselyGetMaterialAtPath(pathToDest);

            if (existingMaterial != null) {
                Debug.Log($"Existing material found, using...");
                return existingMaterial;
            }

            Debug.Log("Creating material...");

            if (placeQuestMaterialsInOwnDirectory) {
                bool exists = Directory.Exists(pathToNewParentDir);

                if(!exists) {
                    Debug.Log($"Creating directory for materials at {pathToNewParentDir}...");
                    Directory.CreateDirectory(pathToNewParentDir);
                }
            }

            bool result = AssetDatabase.CopyAsset(pathToMaterialTemplate, pathToDest);

            if (result == false) {
                throw new System.Exception("Failed to copy Quest material template!");
            }

            Material createdMaterial = GetMaterialAtPath(pathToDest);

            try {
                createdMaterial.CopyPropertiesFromMaterial(originalMaterial);
            } catch (System.Exception err) {
                // if props don't exist then it throws errors
                // ignore them
                Debug.Log(err);
            }

            return createdMaterial;
        }
            
        static Material GetMaterialAtPath(string pathToMaterial, bool ignoreNotFound = false) {
            Material loadedMaterial = (Material)AssetDatabase.LoadAssetAtPath(pathToMaterial, typeof(Material));

            if (loadedMaterial == null) {
                if (ignoreNotFound) {
                    return null;
                }
                throw new System.Exception("Failed to load material at path: " + pathToMaterial);
            }

            return loadedMaterial;
        }

        static Material LooselyGetMaterialAtPath(string pathToMaterial) {
            return GetMaterialAtPath(pathToMaterial, true);
        }

        public static void FocusGameObject(GameObject obj) {
            EditorGUIUtility.PingObject(obj);
        }

        public static long GetSizeOfAssetBundle(string pathToAssetBundle) {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(pathToAssetBundle);

            if (assetBundle != null)
            {
                string[] assetNames = assetBundle.GetAllAssetNames();

                long totalSize = 0;

                foreach (string assetName in assetNames)
                {
                    UnityEngine.Object asset = assetBundle.LoadAsset(assetName);
                    totalSize += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(asset);
                    Debug.Log($"{assetName} - {totalSize}");
                }

                assetBundle.Unload(true);

                return totalSize;
            }
            else
            {
                Debug.LogError($"Failed to load AssetBundle '{pathToAssetBundle}'");
                return -1;
            }
        }
    }
}