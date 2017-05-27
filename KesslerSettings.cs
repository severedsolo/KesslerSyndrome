using System;

namespace KesslerSyndrome
{
    class KesslerSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Kessler Syndrome Options"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Kessler Syndrome"; } }
        public override int SectionOrder { get { return 1; } }
        public override string DisplaySection { get { return Section; } }
        public override bool HasPresets { get { return false; } }

        [GameParameters.CustomIntParameterUI("Max Debris", minValue = 1, maxValue = 250, newGameOnly = false, toolTip = "How much debris is needed for a 100% chance of debris generating")]
        public int CloudChance = 100;
        [GameParameters.CustomParameterUI("Decay orbits (not working yet)?")]
        public bool orbitalDecay = false;
        [GameParameters.CustomFloatParameterUI("Height of exosphere (as percentage of atmosphere)", toolTip = "setting this to 200% at Kerbin would decay anything with a PE of less than 140km")]
        public float decayHeight = 2.0f;
        [GameParameters.CustomFloatParameterUI("Decay Percentage)", toolTip = "This doesn't do anything yet",maxValue = 1.0f)]
        public float decayPercent = 0.01f;
    }
}
