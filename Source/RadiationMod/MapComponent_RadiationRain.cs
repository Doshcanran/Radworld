using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RadiationMod
{
    public class MapComponent_RadiationRain : MapComponent
    {
        private Dictionary<int, float> accumulatedRadiation = new Dictionary<int, float>();

        /// <summary>100 единиц радиации = смерть (severity 1.0).</summary>
        public const float MaxRadiation = 100f;

        private const int TickInterval    = 120;   // начисляем каждые ~2 сек
        private const int HediffInterval  = 300;   // обновляем hediff каждые ~5 сек
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

        // ── Тик ─────────────────────────────────────────────────────────────────

        public override void MapComponentTick()
        {
            int tick = Find.TickManager.TicksGame;

            if (tick % TickInterval == 0 && IsRadioactiveRainActive())
            {
                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn.Dead) continue;
                    if (pawn.RaceProps.IsMechanoid) continue;
                    if (map.roofGrid.Roofed(pawn.Position)) continue;

                    float resistance = GetResistance(pawn);   // 0..0.9
                    float gain = BaseRadPerInterval * (1f - resistance);
                    if (gain <= 0f) continue;

                    float current = GetAccumulatedRadiation(pawn);
                    accumulatedRadiation[pawn.thingIDNumber] =
                        UnityEngine.Mathf.Clamp(current + gain, 0f, MaxRadiation);
                }
            }

            if (tick % HediffInterval == 0)
            {
                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn.Dead) continue;
                    if (pawn.RaceProps.IsMechanoid) continue;
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

        /// <summary>
        /// Возвращает сопротивление радиации в диапазоне [0..0.9].
        /// Берёт значение стата RAD_RadiationResistance (0–90%) и нормализует.
        /// StatWorker уже считает покрытие одеждой.
        /// </summary>
        private static float GetResistance(Pawn pawn)
        {
            StatDef stat = DefDatabase<StatDef>.GetNamedSilentFail("RAD_RadiationResistance");
            if (stat == null) return 0f;
            // Стат хранится в процентах (0–90), нормализуем в [0..0.9]
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
            if (def == null) return false;
            return map.GameConditionManager.ConditionIsActive(def);
        }

        public static MapComponent_RadiationRain GetForPawn(Pawn pawn)
        {
            if (pawn?.Map == null) return null;
            return pawn.Map.GetComponent<MapComponent_RadiationRain>();
        }
    }
}
