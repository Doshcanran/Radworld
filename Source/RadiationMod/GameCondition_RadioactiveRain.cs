using RimWorld;
using Verse;

namespace RadiationMod
{
    /// <summary>
    /// Кастомный класс условия — используется ТОЛЬКО когда dll скомпилирована.
    /// Отвечает за визуал неба. Логика накопления радиации живёт в MapComponent_RadiationRain.
    /// </summary>
    public class GameCondition_RadioactiveRain : GameCondition
    {
        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, 3000f);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            // Сигнатуры из Assembly-CSharp.dll RimWorld 1.6:
            //   SkyColorSet(Color sky, Color shadow, Color overlay, float saturation)
            //   SkyTarget(float glow, SkyColorSet colors, float lightsourceShineSize, float lightsourceShineIntensity)
            SkyColorSet colors = new SkyColorSet(
                new ColorInt(160, 185, 140).ToColor,
                new ColorInt(100, 120,  90).ToColor,
                new ColorInt(170, 200, 150).ToColor,
                0.75f
            );
            return new SkyTarget(0.7f, colors, 1f, 1f);
        }
    }
}
