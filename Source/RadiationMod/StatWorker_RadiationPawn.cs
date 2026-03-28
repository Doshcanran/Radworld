using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RadiationMod
{
    /// <summary>
    /// Кастомный StatWorker для радиационных статов.
    ///
    /// Для RAD_RadiationResistance:
    ///   Значение = покрытие одеждой * 50%, зажатое в [0..90].
    ///   Логика: считаем уникальные группы тела покрытые одеждой / все группы тела.
    ///   Полностью одетая пешка → 100% покрытие → 50% защиты.
    ///   Максимум стата = 90 (задан в StatDef.maxValue).
    ///
    /// Для RAD_AccumulatedRadiation:
    ///   Берём значение из MapComponent_RadiationRain.
    /// </summary>
    public class StatWorker_RadiationPawn : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!req.HasThing || req.Thing is not Pawn pawn)
                return false;

            RaceProperties race = pawn.RaceProps;
            if (race.IsMechanoid) return stat.showOnMechanoids;
            if (race.Animal)     return stat.showOnAnimals;
            return stat.showOnHumanlikes;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (!req.HasThing || req.Thing is not Pawn pawn)
                return 0f;

            // ── Накопленная радиация ─────────────────────────────────────────────
            if (stat.defName == "RAD_AccumulatedRadiation")
            {
                var comp = MapComponent_RadiationRain.GetForPawn(pawn);
                return comp?.GetAccumulatedRadiation(pawn) ?? 0f;
            }

            // ── Сопротивление радиации ───────────────────────────────────────────
            if (stat.defName == "RAD_RadiationResistance")
            {
                return CalcRadiationResistance(pawn);
            }

            return base.GetValueUnfinalized(req, applyPostProcess);
        }

        /// <summary>
        /// Считает защиту от радиации на основе одетой одежды.
        ///
        /// Алгоритм:
        ///   1. Собираем все группы тела (BodyPartGroupDef) существа.
        ///   2. Для каждого одетого предмета одежды — добавляем в покрытые группы.
        ///   3. coverage = покрытые_группы / все_группы  (0..1)
        ///   4. resistance = coverage * 50f  (0..50%)
        ///   5. Зажимаем в [0..90] (maxValue из StatDef).
        ///
        /// Одежда без coverage (украшения, пояса без групп) не учитывается.
        /// </summary>
        private static float CalcRadiationResistance(Pawn pawn)
        {
            if (pawn.apparel == null || pawn.RaceProps?.body == null)
                return 0f;

            // Все группы тела расы
            var allGroups = new HashSet<BodyPartGroupDef>();
            foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
            {
                foreach (BodyPartGroupDef grp in part.groups)
                    allGroups.Add(grp);
            }

            if (allGroups.Count == 0)
                return 0f;

            // Группы покрытые одеждой
            var coveredGroups = new HashSet<BodyPartGroupDef>();
            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                foreach (BodyPartGroupDef grp in apparel.def.apparel.bodyPartGroups)
                    coveredGroups.Add(grp);
            }

            float coverage = (float)coveredGroups.Count / allGroups.Count;
            // Полное покрытие → 50% защиты; частичное — пропорционально
            float resistance = coverage * 50f;
            return UnityEngine.Mathf.Clamp(resistance, 0f, 90f);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            if (stat.defName != "RAD_RadiationResistance" || !req.HasThing || req.Thing is not Pawn pawn)
                return base.GetExplanationUnfinalized(req, numberSense);

            if (pawn.apparel == null)
                return "No apparel system.";

            int totalGroups = 0;
            var allGroups = new HashSet<BodyPartGroupDef>();
            if (pawn.RaceProps?.body != null)
            {
                foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
                    foreach (BodyPartGroupDef grp in part.groups)
                        allGroups.Add(grp);
                totalGroups = allGroups.Count;
            }

            var coveredGroups = new HashSet<BodyPartGroupDef>();
            int itemCount = 0;
            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                foreach (BodyPartGroupDef grp in apparel.def.apparel.bodyPartGroups)
                    coveredGroups.Add(grp);
                itemCount++;
            }

            float coverage = totalGroups > 0 ? (float)coveredGroups.Count / totalGroups : 0f;
            float resistance = UnityEngine.Mathf.Clamp(coverage * 50f, 0f, 90f);

            return $"Worn apparel: {itemCount} items\n"
                 + $"Body coverage: {coveredGroups.Count}/{totalGroups} groups ({coverage * 100f:F0}%)\n"
                 + $"Radiation resistance from clothing: {resistance:F0}%\n"
                 + "(Full coverage = 50% resistance, max 90%)";
        }
    }
}
