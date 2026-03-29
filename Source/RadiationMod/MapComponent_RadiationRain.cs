using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RadiationMod
{
    public class MapComponent_RadiationRain : MapComponent
    {
        private Dictionary<int, float> accumulatedRadiation = new Dictionary<int, float>();

        public const float MaxRadiation = 100f;

        private const int TickInterval   = 120;
        private const int HediffInterval = 300;
        private const float BaseRadPerInterval = 0.2f;

        public MapComponent_RadiationRain(Map map) : base(map) { }

        // ── API ──────────────────────────────────────────────────────────────────

        public float GetAccumulatedRadiation(Pawn pawn)
        {
            if (pawn == null) return 0f;
            accumulatedRadiation.TryGetValue(pawn.thingIDNumber, out float val);
            return val;
        }

        public void SetAccumulatedRadiation(Pawn pawn, float value)
        {
            if (pawn == null) return;
            accumulatedRadiation[pawn.thingIDNumber] =
                UnityEngine.Mathf.Clamp(value, 0f, MaxRadiation);
            UpdateHediff(pawn);
        }

        /// <summary>Средняя накопленная радиация среди пешек с ненулевым значением.</summary>
        public float GetAverageRadiation()
        {
            if (accumulatedRadiation.Count == 0) return 0f;
            float sum = 0f; int count = 0;
            foreach (var kv in accumulatedRadiation)
                if (kv.Value > 0.01f) { sum += kv.Value; count++; }
            return count > 0 ? sum / count : 0f;
        }

        // ── Тик ─────────────────────────────────────────────────────────────────

        public override void MapComponentTick()
        {
            int tick = Find.TickManager.TicksGame;

            // Начисляем радиацию от дождя
            if (tick % TickInterval == 0 && IsRadioactiveRainActive())
            {
                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn.Dead || pawn.RaceProps.IsMechanoid) continue;
                    if (pawn.RaceProps.IsAnomalyEntity) continue;
                    if (map.roofGrid.Roofed(pawn.Position)) continue;

                    float resistance = GetResistance(pawn);
                    float gain = BaseRadPerInterval * (1f - resistance);
                    if (gain <= 0f) continue;

                    float current = GetAccumulatedRadiation(pawn);
                    accumulatedRadiation[pawn.thingIDNumber] =
                        UnityEngine.Mathf.Clamp(current + gain, 0f, MaxRadiation);
                }
            }

            // Обновляем hediff + выведение радиации генами
            if (tick % HediffInterval == 0)
            {
                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn.Dead || pawn.RaceProps.IsMechanoid) continue;
                    if (pawn.RaceProps.IsAnomalyEntity) continue;

                    // Ген быстрого выведения: каждые 5 сек убираем 1.5 ед.
                    if (GeneRadiationEffects.HasFastDecay(pawn))
                    {
                        float cur = GetAccumulatedRadiation(pawn);
                        if (cur > 0f)
                            accumulatedRadiation[pawn.thingIDNumber] =
                                UnityEngine.Mathf.Max(0f, cur - GeneRadiationEffects.FastDecayRate);
                    }

                    UpdateHediff(pawn);
                }
            }
        }

        // ── Hediff ───────────────────────────────────────────────────────────────

        private void UpdateHediff(Pawn pawn)
        {
            if (pawn?.health == null) return;

            float radiation = GetAccumulatedRadiation(pawn);
            float severity  = radiation / MaxRadiation;

            // Ген иммунитета к болезни: severity видимая 80% меньше
            if (GeneRadiationEffects.HasSicknessImmunity(pawn))
                severity *= GeneRadiationEffects.SicknessImmunityFactor;

            HediffDef def = DefDatabase<HediffDef>.GetNamedSilentFail("RAD_RadiationSickness");
            if (def == null) return;

            if (severity <= 0.01f)
            {
                Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(def);
                if (existing != null) pawn.health.RemoveHediff(existing);
                return;
            }

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(def, pawn);
                hediff.Severity = severity;
                pawn.health.AddHediff(hediff);
            }
            else
            {
                hediff.Severity = severity;
            }
        }

        // ── Resistance ───────────────────────────────────────────────────────────

        private static float GetResistance(Pawn pawn)
        {
            StatDef stat = DefDatabase<StatDef>.GetNamedSilentFail("RAD_RadiationResistance");
            if (stat == null) return 0f;
            float raw = pawn.GetStatValue(stat);
            return UnityEngine.Mathf.Clamp(raw / 100f, 0f, 0.9f);
        }

        // ── Сохранение ───────────────────────────────────────────────────────────

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(
                ref accumulatedRadiation, "accumulatedRadiation",
                LookMode.Value, LookMode.Value);
            if (accumulatedRadiation == null)
                accumulatedRadiation = new Dictionary<int, float>();
        }

        // ── Хелперы ──────────────────────────────────────────────────────────────

        private bool IsRadioactiveRainActive()
        {
            GameConditionDef def = DefDatabase<GameConditionDef>
                .GetNamedSilentFail("RAD_RadioactiveRain");
            return def != null && map.GameConditionManager.ConditionIsActive(def);
        }

        public static MapComponent_RadiationRain GetForPawn(Pawn pawn)
        {
            if (pawn?.Map == null) return null;
            return pawn.Map.GetComponent<MapComponent_RadiationRain>();
        }
    }
}
