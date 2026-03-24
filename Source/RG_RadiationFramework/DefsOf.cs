using RimWorld;
using Verse;

namespace RG_RadiationFramework
{
    [DefOf]
    public static class RG_DefOf
    {
        public static StatDef RG_AccumulatedRadiation;
        public static StatDef RG_RadiationProtection;
        public static StatDef RG_EmittedRadiation;
        public static StatDef RG_RadiationResistance;

        public static HediffDef RG_RadiationAccumulation;
        public static HediffDef RG_RadiationSickness;

        public static GameConditionDef RG_RadioactiveFallout;
        public static WeatherDef RG_RadioactiveRain;

        static RG_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RG_DefOf));
        }
    }
}
