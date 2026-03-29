using RimWorld;
using Verse;

namespace RadiationMod
{
    /// <summary>
    /// StatPart для стата RAD_RadiationResistance.
    /// Существа DLC Аномалия получают +100 к сопротивлению радиации
    /// (т.е. полный иммунитет, так как макс стата = 90).
    /// Работает через RaceProperties.IsAnomalyEntity.
    /// </summary>
    public class StatPart_AnomalyEntityResistance : StatPart
    {
        // Значение добавляемое к стату (100 = гарантированный иммунитет)
        private const float ResistanceBonus = 100f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!IsAnomalyEntity(req)) return;
            val += ResistanceBonus;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!IsAnomalyEntity(req)) return null;
            return $"Anomaly entity (immune to radiation): +{ResistanceBonus}%";
        }

        private static bool IsAnomalyEntity(StatRequest req)
        {
            if (!req.HasThing) return false;
            if (req.Thing is not Pawn pawn) return false;
            return pawn.RaceProps?.IsAnomalyEntity ?? false;
        }
    }
}
