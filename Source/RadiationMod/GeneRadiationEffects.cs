using RimWorld;
using Verse;

namespace RadiationMod
{
    /// <summary>
    /// Утилиты для проверки радиационных генов у пешки.
    /// </summary>
    public static class GeneRadiationEffects
    {
        /// <summary>Выведение радиации в ед. каждые HediffInterval тиков.</summary>
        public const float FastDecayRate = 1.5f;

        /// <summary>Множитель severity болезни при иммунитете (0.2 = 80% снижения).</summary>
        public const float SicknessImmunityFactor = 0.2f;

        public static bool HasFastDecay(Pawn pawn)
            => HasGene(pawn, "RAD_FastRadDecay");

        public static bool HasSicknessImmunity(Pawn pawn)
            => HasGene(pawn, "RAD_RadSicknessImmunity");

        private static bool HasGene(Pawn pawn, string defName)
        {
            if (pawn?.genes == null) return false;
            GeneDef def = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
            if (def == null) return false;
            return pawn.genes.HasActiveGene(def);
        }
    }
}
