using HarmonyLib;
using Verse;

namespace RG_RadiationFramework
{
    public sealed class RadiationMod : Mod
    {
        public RadiationMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("openai.rg.radiationframework");
            harmony.PatchAll();
        }
    }
}
