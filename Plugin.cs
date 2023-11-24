using BepInEx;
using HarmonyLib;
using System.Reflection;
using LC_API;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System;
using System.Runtime.CompilerServices;

namespace CreatureModels 
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Harmony _harmony;
        private void Awake()
        {
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();
            Logger.LogInfo("HyenaNoises");
            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded");
        }
    }
}
