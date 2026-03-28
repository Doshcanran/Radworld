using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RadiationMod
{
    /// <summary>
    /// Тикает каждые UraniumTickInterval тиков и начисляет радиацию пешкам
    /// которые находятся в радиусе 3 клеток от любого источника урана:
    ///   - ресурс на полу / в стеке (ThingDef: Uranium)
    ///   - строение из урана (stuffedWith Uranium или defName содержит Uranium)
    ///   - залежь руды (Mineable с продуктом Uranium)
    ///
    /// Добавляется на карту через тот же Harmony-патч что и MapComponent_RadiationRain.
    /// </summary>
    public class MapComponent_UraniumRadiation : MapComponent
    {
        // Радиус в клетках
        private const float UraniumRadius = 3f;

        // Раз в сколько тиков проверяем (~5 секунд игры)
        private const int UraniumTickInterval = 300;

        // Сколько радиации добавляем за один интервал при нахождении рядом с ураном
        // Масштаб: 0.05 ед./интервал = ~6 ед. радиации за игровой день
        private const float BaseRadPerInterval = 0.05f;

        // Набор defName-ов которые считаются источниками урана
        // Включает: ресурс, строения, руду
        private static readonly HashSet<string> UraniumDefNames = new HashSet<string>
        {
            "Uranium",              // стак ресурса
            "MineableUranium",      // залежь руды
        };

        public MapComponent_UraniumRadiation(Map map) : base(map) { }

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % UraniumTickInterval != 0)
                return;

            // Собираем все позиции урана на карте
            var uraniumPositions = GetUraniumPositions();
            if (uraniumPositions.Count == 0)
                return;

            // Для каждой пешки проверяем расстояние до ближайшего урана
            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.Dead) continue;
                if (pawn.RaceProps.IsMechanoid) continue;

                float closest = ClosestUraniumDist(pawn.Position, uraniumPositions);
                if (closest > UraniumRadius) continue;

                // Интенсивность убывает с расстоянием: 1.0 вплотную, ~0.1 на краю
                float intensity = 1f - (closest / UraniumRadius);
                float gain = BaseRadPerInterval * intensity;

                // Учитываем сопротивление пешки
                float resistance = GetResistance(pawn);
                gain *= (1f - resistance);

                if (gain <= 0f) continue;

                // Начисляем через MapComponent_RadiationRain
                var rainComp = map.GetComponent<MapComponent_RadiationRain>();
                if (rainComp == null) continue;

                float current = rainComp.GetAccumulatedRadiation(pawn);
                rainComp.SetAccumulatedRadiation(pawn, current + gain);
            }
        }

        // ── Поиск урана на карте ─────────────────────────────────────────────────

        private List<IntVec3> GetUraniumPositions()
        {
            var result = new List<IntVec3>();

            // 1. Ресурс в ThingGrid (стаки на полу, строения из урана как stuff)
            foreach (string defName in UraniumDefNames)
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def == null) continue;

                foreach (Thing thing in map.listerThings.ThingsOfDef(def))
                {
                    if (!result.Contains(thing.Position))
                        result.Add(thing.Position);
                }
            }

            // 2. Строения изготовленные из урана (stuff = Uranium)
            ThingDef uraniumDef = DefDatabase<ThingDef>.GetNamedSilentFail("Uranium");
            if (uraniumDef != null)
            {
                foreach (Building building in map.listerBuildings.allBuildingsColonist)
                {
                    if (building.Stuff == uraniumDef && !result.Contains(building.Position))
                        result.Add(building.Position);
                }

                // Также вражеские/нейтральные строения
                foreach (Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
                {
                    if (thing is Building b && b.Stuff == uraniumDef && !result.Contains(b.Position))
                        result.Add(b.Position);
                }
            }

            return result;
        }

        private static float ClosestUraniumDist(IntVec3 pos, List<IntVec3> positions)
        {
            float min = float.MaxValue;
            foreach (IntVec3 p in positions)
            {
                float d = pos.DistanceTo(p);
                if (d < min) min = d;
            }
            return min;
        }

        // ── Resistance ───────────────────────────────────────────────────────────

        private static float GetResistance(Pawn pawn)
        {
            StatDef stat = DefDatabase<StatDef>.GetNamedSilentFail("RAD_RadiationResistance");
            if (stat == null) return 0f;
            return UnityEngine.Mathf.Clamp(pawn.GetStatValue(stat) / 100f, 0f, 0.9f);
        }
    }
}
