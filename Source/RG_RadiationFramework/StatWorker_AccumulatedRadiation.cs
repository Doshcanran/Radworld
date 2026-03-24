using RimWorld;
using Verse;

namespace RG_RadiationFramework
{
    public sealed class StatWorker_AccumulatedRadiation : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!req.HasThing || req.Thing is not Pawn pawn)
            {
                return false;
            }

            return RadiationUtility.IsRadiationRelevantPawn(pawn);
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (!req.HasThing || req.Thing is not Pawn pawn)
            {
                return 0f;
            }

            return RadiationUtility.GetAccumulatedRadiation(pawn);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            if (!req.HasThing || req.Thing is not Pawn pawn)
            {
                return "0";
            }

            return "Считывается из скрытого накопления радиации в организме.";
        }
    }
}
