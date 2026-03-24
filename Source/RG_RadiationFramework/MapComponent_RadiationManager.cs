using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RG_RadiationFramework
{
    public sealed class MapComponent_RadiationManager : MapComponent
    {
        public MapComponent_RadiationManager(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % RadiationUtility.UpdateIntervalTicks != 0)
            {
                return;
            }

            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            float dayFraction = RadiationUtility.UpdateIntervalTicks / (float)GenDate.TicksPerDay;
            bool falloutActive = map.gameConditionManager.ConditionIsActive(RG_DefOf.RG_RadioactiveFallout);

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!RadiationUtility.IsRadiationRelevantPawn(pawn))
                {
                    continue;
                }

                RadiationUtility.GetOrCreateAccumulation(pawn);

                if (falloutActive)
                {
                    float amount = RadiationUtility.UnderOpenSky(pawn)
                        ? RadiationUtility.FalloutGainPerDayOpenSky * dayFraction
                        : RadiationUtility.FalloutGainPerDayRoofed * dayFraction;

                    RadiationUtility.AddRadiation(pawn, amount);
                }

                RadiationUtility.DecayRadiation(pawn, dayFraction);
            }

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn source = pawns[i];
                if (!RadiationUtility.IsRadiationRelevantPawn(source))
                {
                    continue;
                }

                for (int j = 0; j < pawns.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    RadiationUtility.ApplyEmitterRadiation(source, pawns[j], dayFraction);
                }
            }
        }
    }
}
