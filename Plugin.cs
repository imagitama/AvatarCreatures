using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace AvatarCreatures
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("verity.3rdperson", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Harmony _harmony;
        public static ConfigEntry<bool> configAlwaysRenderLocalPlayerModel;

        private void Awake()
        {

            configAlwaysRenderLocalPlayerModel = Config.Bind(
                "General",
                "AlwaysRenderLocalPlayerModel",
                false,
                "If to always render the local player model. Useful for testing with 3rd person mods."
            );

            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();

            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded");
        }
    }
}
