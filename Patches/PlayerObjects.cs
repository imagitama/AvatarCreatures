using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using static CreatureModels.AvatarCreature;

namespace CreatureModels.Patches
{
    [HarmonyPatch]
    internal class PlayerObjects
    {
        public static void InitModels()
        {
            var localPlayer = GameNetworkManager.Instance.localPlayerController;
            var players = Object.FindObjectsOfType<PlayerControllerB>();

            // dev only flag for forcing the player's local model
            // note if set to false the game will render the default model AND the custom one in 1st person mode
            bool ignoreLocalPlayer = true;

            foreach (var player in players)
            {
                if (player == localPlayer && ignoreLocalPlayer == true)
                {
                    Debug.Log($"Ignoring local player (steam ID {player.playerSteamId})");
                }
                else
                {
                    var Creature = player.gameObject.GetComponentsInChildren<CreatureController>();
                    if (Creature.Length > 0)
                    {
                        Debug.Log($"Steam ID {player.playerSteamId} already has creature, skipping");
                        continue;
                    }

                    Debug.Log($"Adding creature to steam ID {player.playerSteamId}");

                    player.gameObject.AddComponent<CreatureController>();
                }
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "SpawnPlayerAnimation")]
        [HarmonyPostfix]
        public static void InitModel(ref PlayerControllerB __instance)
        {
            InitModels();
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

    //Outfits
    [HarmonyPatch(typeof(UnlockableSuit))]
    internal class UnlockableSuitPatch
    {
        [HarmonyPatch("SwitchSuitForPlayer")]
        [HarmonyPrefix]
        static void SwitchSuitForPlayerPatch(PlayerControllerB player, int suitID, bool playAudio = true)
        {
/*            Texture tex;
            switch (suitID)
            {
 *//*               case 0:
                    tex = LethalCreature.CreatureController.TexBase01;
                    break;
                case 1:
                    tex = LethalCreature.CreatureController.TexBase02;
                    break;
                case 2:
                    tex = LethalCreature.CreatureController.TexBase03;
                    break;
                case 3:
                    tex = LethalCreature.CreatureController.TexBase04;
                    break;*//*
                default:
                    tex = AvatarCreature.CreatureController.albedoTexture;
                    break;
            }

            SkinnedMeshRenderer[] meshes = player.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            Debug.Log("Looking for meshes...");
            foreach (SkinnedMeshRenderer mesh in meshes)
            {
                Debug.Log("Found a mesh: " + mesh.name);
                mesh.materials[0].SetTexture("_BaseColorMap", tex);
            }*/
        }
    }

}
