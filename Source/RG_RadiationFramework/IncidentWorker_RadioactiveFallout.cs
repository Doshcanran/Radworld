using RimWorld;
using Verse;

namespace RG_RadiationFramework
{
    public class IncidentWorker_RadioactiveFallout : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = parms.target as Map;
            return map != null;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            if (map == null)
            {
                return false;
            }

            GameCondition gameCondition = GameConditionMaker.MakeCondition(
                RG_DefOf.RG_RadioactiveFallout,
                60000);

            map.gameConditionManager.RegisterCondition(gameCondition);

            LookTargets lookTargets = new TargetInfo(map.Center, map);
            SendStandardLetter(def.letterLabel, def.letterText, def.letterDef, parms, lookTargets);
            return true;
        }
    }
}
