using RimWorld;
using Verse;

namespace RG_RadiationFramework
{
    public sealed class HediffCompProperties_KillOnSeverity : HediffCompProperties
    {
        public float killAtSeverity = 1f;
        public string deathMessageKey = "RG_RadiationDeathMessage";

        public HediffCompProperties_KillOnSeverity()
        {
            compClass = typeof(HediffComp_KillOnSeverity);
        }
    }

    public sealed class HediffComp_KillOnSeverity : HediffComp
    {
        public HediffCompProperties_KillOnSeverity Props => (HediffCompProperties_KillOnSeverity)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn == null || Pawn.Dead || parent == null)
            {
                return;
            }

            if (parent.Severity >= Props.killAtSeverity)
            {
                var message = Props.deathMessageKey.Translate(Pawn.LabelShortCap);
                Pawn.Kill(null, parent);
                if (!message.NullOrEmpty())
                {
                    Messages.Message(message, Pawn, MessageTypeDefOf.NegativeHealthEvent);
                }
            }
        }
    }
}
