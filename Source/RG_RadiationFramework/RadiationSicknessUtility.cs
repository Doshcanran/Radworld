using RimWorld;
using Verse;

namespace RG_RadiationFramework
{
    public static class RadiationSicknessUtility
    {
        public static void EnsureRadiationSickness(Pawn pawn, float radiation)
        {
            if (pawn == null || pawn.Dead || pawn.RaceProps == null || pawn.RaceProps.IsMechanoid)
            {
                return;
            }

            if (radiation < 0.15f)
            {
                return;
            }

            HediffDef def = RG_DefOf.RG_RadiationSickness;
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def);

            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(def, pawn);
                hediff.Severity = 0.01f;
                pawn.health.AddHediff(hediff);
            }

            float targetSeverity = radiation;
            if (hediff.Severity < targetSeverity)
            {
                hediff.Severity = targetSeverity;
            }

            if (hediff.Severity >= 1.0f && !pawn.Dead)
            {
                DamageInfo dinfo = new DamageInfo(DamageDefOf.Rotting, 9999f);
                pawn.Kill(dinfo);
            }
        }
    }
}
