using GameNetcodeStuff;
using HarmonyLib;
using System.Diagnostics;
using System.Numerics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CreatureModels.Patches
{
    [HarmonyPatch]
    internal class PlayerObjects
    {
        public static void InitModels()
        {
            var localPlayer = GameNetworkManager.Instance.localPlayerController;
            Debug.Log(localPlayer.gameObject.name);
            var players = UnityEngine.Object.FindObjectsOfType<PlayerControllerB>();
            foreach (var player in players)
            {
                if (player == localPlayer) continue;
                player.gameObject.AddComponent<CreatureController>();
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "SpawnPlayerAnimation")]
        [HarmonyPostfix]
        public static void InitModel(ref PlayerControllerB __instance)
        {
            InitModels();
        }

        
        //Outfits
        [HarmonyPatch(typeof(UnlockableSuit))]
        internal class UnlockableSuitPatch
        {
            [HarmonyPatch("SwitchSuitForPlayer")]
            [HarmonyPrefix]
            static void SwitchSuitForPlayerPatch(ref UnlockableSuit __instance, PlayerControllerB player, int suitID, bool playAudio = true)
            {
                string texName = StartOfRound.Instance.unlockablesList.unlockables[suitID].unlockableName; // ex: "Green suit"
                Texture tex = (Texture)BundleLoader.assets["ASSETS/" + texName + ".PNG"];

                SkinnedMeshRenderer[] meshes = player.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (SkinnedMeshRenderer mesh in meshes)
                {
                    mesh.materials[0].SetTexture("_BaseColorMap", tex);
                }
            }
        }


        // Hides the vanilla player
        [HarmonyPatch(typeof(PlayerControllerB), "DisablePlayerModel")]
        [HarmonyPostfix] 
        public static void DisablePlayerModel(ref PlayerControllerB __instance, GameObject playerObject)
        {
            var localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (playerObject == localPlayer) return;
            playerObject.gameObject.GetComponentInChildren<LODGroup>().enabled = false;
            var meshes = playerObject.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var LODmesh in meshes)
            {
                if (LODmesh.name == "Body") continue;
                LODmesh.enabled = false;
            }

        }
    }
}
