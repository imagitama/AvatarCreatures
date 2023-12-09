using GameNetcodeStuff;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using System.IO;
using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.UIElements.UIR.Allocator2D;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEngine.Profiling;

namespace CreatureModels
{
    public class AvatarCreature
    {
        public class CreatureController : MonoBehaviour
        {
            GameObject currentAvatar = null;
            Transform spineBoneForRotation = null;
            Transform chestBone = null;

            void Start()
            {
                var playerController = gameObject.GetComponent<PlayerControllerB>();

                var rig = gameObject.transform.Find("ScavengerModel").Find("metarig");
                spineBoneForRotation = rig.Find("spine").Find("spine.001").Find("spine.002").Find("spine.003");

                if (spineBoneForRotation == null)
                {
                    throw new Exception("ScavengerModel is missing a spine bone");
                }

                // seems to be 0 when local
                var steamId = playerController.playerSteamId;

                Debug.Log($"Loading model for player '{steamId}'...");

                gameObject.GetComponentInChildren<LODGroup>().enabled = false;
                var meshes = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var LODmesh in meshes)
                {
                    LODmesh.enabled = false;
                }

                //Assetbundle Commune//========
                string pathToAvatars = Path.Combine(BepInEx.Paths.GameRootPath, "Avatars");
                string pathToAvatar = Path.Combine(pathToAvatars, steamId.ToString());
                string pathToAssetBundle = $"{pathToAvatar}.assetbundle";

                AssetBundle assetBundle = AssetBundle.LoadFromFile(pathToAssetBundle);

                if (assetBundle == null)
                {
                    throw new Exception($"Error loading asset bundle '{pathToAssetBundle}' - null");
                }

                GameObject modelFbx = assetBundle.LoadAsset<GameObject>("model.fbx");

                if (modelFbx == null)
                {
                    throw new Exception($"Model fbx not found inside bundle");
                }

                RuntimeAnimatorController animController = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<RuntimeAnimatorController>("assets/crittercontrol.controller");

                if (animController == null)
                {
                    animController = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<RuntimeAnimatorController>("assets/assets/crittercontrol.controller");

                    if (animController == null)
                    {
                        throw new Exception("Could not find controller in bundle");
                    }
                }

                Debug.Log("Creating avatar");

                //=================
                //Scaling//========
                var newAvatar = Instantiate(modelFbx);
                newAvatar.transform.localScale = new Vector3(1, 1, 1);

                Debug.Log("Parenting avatar");

                var spine = rig.Find("spine").Find("spine.001");
                newAvatar.transform.SetParent(spine);

                newAvatar.transform.localPosition = new Vector3(0, 0f, 0);
                newAvatar.transform.localEulerAngles = Vector3.zero;

                Debug.Log("Updating material");

                var LOD1 = gameObject.GetComponent<PlayerControllerB>().thisPlayerModel;
                var goodShader = LOD1.material.shader;
                var renderers = newAvatar.GetComponentsInChildren<Renderer>();

                //=================
                //Materials and Textures//========

                foreach (Renderer renderer in renderers)
                {
                    string rendererName = renderer.gameObject.name;

                    Debug.Log($"Updating {renderer.materials.Length} materials for renderer '{rendererName}'");

                    int idx = 0;

                    foreach (Material material in renderer.materials)
                    {
                        Debug.Log($"Applying textures to material #{idx} - '{material.name}'");

                        material.shader = goodShader;

                        material.EnableKeyword("_EMISSION");
                        material.EnableKeyword("_SPECGLOSSMAP");
                        material.EnableKeyword("_NORMALMAP");

                        string albedoName = $"{rendererName}_{idx}_Albedo.png";

                        Texture albedoTexture = assetBundle.LoadAsset<Texture>(albedoName);
                        Texture smoothnessTexture = assetBundle.LoadAsset<Texture>($"{rendererName}_{idx}_Smoothness.png");
                        Texture emissionTexture = assetBundle.LoadAsset<Texture>($"{rendererName}_{idx}_Emission.png");
                        Texture normalTexture = assetBundle.LoadAsset<Texture>($"{rendererName}_{idx}_Normal.png");

                        if (albedoTexture == null)
                        {
                            throw new Exception($"Could not find asset '{albedoName}' in asset bundle");
                        }

                        material.SetTexture("_BaseColorMap", albedoTexture);
                        material.SetTexture("_SpecularColorMap", smoothnessTexture);
                        material.SetFloat("_Smoothness", .30f);
                        material.SetTexture("_EmissiveColorMap", emissionTexture);
                        material.SetTexture("_BumpMap", normalTexture);

                        // trust the user made their eyes only have emission
                        if (emissionTexture != null) {
                            material.SetColor("_EmissiveColor", Color.white);
                        }

                        HDMaterial.ValidateMaterial(material);

                        idx++;
                    }
                }

                //=================
                //Dark magik IK voodoo//========

                // assumes you bundled your FBX with a humanoid avatar
                var animator = newAvatar.GetComponentInChildren<Animator>();

                Debug.Log("Storing chest bone");

                chestBone = animator.GetBoneTransform(HumanBodyBones.Chest);

                if (chestBone == null)
                {
                    throw new Exception("Avatar is missing a chest bone");
                }

                Debug.Log("Inserting controller");

                animator.runtimeAnimatorController = animController;

                Debug.Log("Setting up IK");

                var ikController = newAvatar.AddComponent<IKController>();
                var lthigh = rig.Find("spine").Find("thigh.L");
                var rthigh = rig.Find("spine").Find("thigh.R");
                var lshin = lthigh.Find("shin.L");
                var rshin = rthigh.Find("shin.R");
                var lfoot = lshin.Find("foot.L");
                var rfoot = rshin.Find("foot.R");

                var chest = rig.Find("spine").Find("spine.001").Find("spine.002").Find("spine.003");
                var lshoulder = chest.Find("shoulder.L");
                var rshoulder = chest.Find("shoulder.R");
                var lUpperArm = lshoulder.Find("arm.L_upper");
                var rUpperArm = rshoulder.Find("arm.R_upper");
                var lLowerArm = lUpperArm.Find("arm.L_lower");
                var rLowerArm = rUpperArm.Find("arm.R_lower");
                var lHand = lLowerArm.Find("hand.L");
                var rHand = rLowerArm.Find("hand.R");

                //=================
                //IK Offsets for limbs//=========
                GameObject lFootOffset = new("IK Offset");
                lFootOffset.transform.SetParent(lfoot, false); // X Y Z
                lFootOffset.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                GameObject rFootOffset = new("IK Offset");
                rFootOffset.transform.SetParent(rfoot, false); // X Y Z
                rFootOffset.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                GameObject lHandOffset = new("IK Offset");
                lHandOffset.transform.SetParent(lHand, false); // Z Y
                lHandOffset.transform.localPosition = new Vector3(0.05f, 0f, 0f);
                GameObject rHandOffset = new("IK Offset");
                rHandOffset.transform.SetParent(rHand, false); // Z Y
                rHandOffset.transform.localPosition = new Vector3(-0.05f, 0f, 0f);

                ikController.leftLegTarget = lFootOffset.transform;
                ikController.rightLegTarget = rFootOffset.transform;
                ikController.leftHandTarget = lHandOffset.transform;
                ikController.rightHandTarget = rHandOffset.transform;
                ikController.ikActive = true;

                Debug.Log("IK set up");

                assetBundle.Unload(false);

                Debug.Log("Avatar has been loaded");

                currentAvatar = newAvatar;
            }

            private void LateUpdate()
            {
                if (currentAvatar != null && chestBone != null && spineBoneForRotation != null)
                {
                    currentAvatar.transform.localPosition = new Vector3(0, -0.15f, 0);
                    // currentAvatar.transform.Find("Armature").Find("Hips").Find("Spine").Find("Chest").localEulerAngles = trans.localEulerAngles;
                    chestBone.localEulerAngles = spineBoneForRotation.localEulerAngles;
                }

            }
        }
    }

}

