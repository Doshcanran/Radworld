using RimWorld;
using System;
using Verse;

namespace RG_RadiationFramework
{
    public static class RadiationUtility
    {
        public const float SicknessThreshold = 0.30f;
        public const float MaxAccumulation = 2.00f;
        public const float PassiveDecayPerDay = 0.010f;
        public const float FalloutGainPerDayOpenSky = 0.180f;
        public const float FalloutGainPerDayRoofed = 0.030f;
        public const float EmissionRadius = 4.9f;
        public const float EmissionFalloff = 0.12f;
        public const int UpdateIntervalTicks = 250;

        public static bool IsRadiationRelevantPawn(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.Destroyed)
            {
                return false;
            }

            if (pawn.RaceProps == null || !pawn.RaceProps.IsFlesh)
            {
                return false;
            }

            return !pawn.RaceProps.IsMechanoid;
        }

        public static Hediff GetOrCreateAccumulation(Pawn pawn)
        {
            Hediff existing = pawn.health?.hediffSet?.GetFirstHediffOfDef(RG_DefOf.RG_RadiationAccumulation);
            if (existing != null)
            {
                return existing;
            }

            if (pawn.health == null)
            {
                return null;
            }

            Hediff hediff = HediffMaker.MakeHediff(RG_DefOf.RG_RadiationAccumulation, pawn);
            hediff.Severity = 0f;
            pawn.health.AddHediff(hediff);
            return hediff;
        }

        public static Hediff GetRadiationSickness(Pawn pawn)
        {
            return pawn.health?.hediffSet?.GetFirstHediffOfDef(RG_DefOf.RG_RadiationSickness);
        }

        public static float GetAccumulatedRadiation(Pawn pawn)
        {
            return pawn.health?.hediffSet?.GetFirstHediffOfDef(RG_DefOf.RG_RadiationAccumulation)?.Severity ?? 0f;
        }

        public static void AddRadiation(Pawn pawn, float rawAmount)
        {
            if (!IsRadiationRelevantPawn(pawn) || rawAmount <= 0f)
            {
                return;
            }

            float protection = SafeStatValue(pawn, RG_DefOf.RG_RadiationProtection);
            float resistance = SafeStatValue(pawn, RG_DefOf.RG_RadiationResistance);
            float finalMultiplier = 1f - Clamp01(protection + resistance);

            if (finalMultiplier <= 0f)
            {
                return;
            }

            Hediff accumulation = GetOrCreateAccumulation(pawn);
            if (accumulation == null)
            {
                return;
            }

            accumulation.Severity = Clamp(accumulation.Severity + rawAmount * finalMultiplier, 0f, MaxAccumulation);
            SyncSicknessWithAccumulation(pawn, accumulation.Severity);
        }

        public static void DecayRadiation(Pawn pawn, float dayFraction)
        {
            if (!IsRadiationRelevantPawn(pawn))
            {
                return;
            }

            Hediff accumulation = pawn.health?.hediffSet?.GetFirstHediffOfDef(RG_DefOf.RG_RadiationAccumulation);
            if (accumulation == null || accumulation.Severity <= 0f)
            {
                return;
            }

            accumulation.Severity = Math.Max(0f, accumulation.Severity - PassiveDecayPerDay * dayFraction);
            SyncSicknessWithAccumulation(pawn, accumulation.Severity);
        }

        public static void SyncSicknessWithAccumulation(Pawn pawn, float accumulationSeverity)
        {
            if (!IsRadiationRelevantPawn(pawn))
            {
                return;
            }

            Hediff sickness = GetRadiationSickness(pawn);

            if (accumulationSeverity < SicknessThreshold)
            {
                if (sickness != null)
                {
                    sickness.Severity = Math.Max(0.01f, sickness.Severity - 0.02f);
                    if (sickness.Severity <= 0.011f)
                    {
                        pawn.health.RemoveHediff(sickness);
                    }
                }

                return;
            }

            float normalized = Clamp01((accumulationSeverity - SicknessThreshold) / (1.10f - SicknessThreshold));

            if (sickness == null)
            {
                sickness = HediffMaker.MakeHediff(RG_DefOf.RG_RadiationSickness, pawn);
                pawn.health.AddHediff(sickness);
            }

            sickness.Severity = Math.Max(0.01f, normalized);
        }

        public static float SafeStatValue(Pawn pawn, StatDef statDef)
        {
            if (pawn == null || statDef == null)
            {
                return 0f;
            }

            try
            {
                return pawn.GetStatValue(statDef, true);
            }
            catch
            {
                return statDef.defaultBaseValue;
            }
        }

        public static bool UnderOpenSky(Pawn pawn)
        {
            return pawn.Spawned && pawn.MapHeld != null && !pawn.PositionHeld.Roofed(pawn.MapHeld);
        }

        public static void ApplyEmitterRadiation(Pawn source, Pawn target, float dayFraction)
        {
            if (source == null || target == null || source == target)
            {
                return;
            }

            if (!IsRadiationRelevantPawn(source) || !IsRadiationRelevantPawn(target))
            {
                return;
            }

            if (source.MapHeld != target.MapHeld || source.PositionHeld.DistanceTo(target.PositionHeld) > EmissionRadius)
            {
                return;
            }

            float emitted = SafeStatValue(source, RG_DefOf.RG_EmittedRadiation);
            if (emitted <= 0f)
            {
                return;
            }

            float distance = Math.Max(1f, source.PositionHeld.DistanceTo(target.PositionHeld));
            float amount = emitted * Math.Max(0f, 1f - distance * EmissionFalloff) * dayFraction;
            AddRadiation(target, amount);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }
    }
}
