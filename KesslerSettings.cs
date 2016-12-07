﻿namespace KesslerSyndrome
{
    class KesslerSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Kessler Syndrome Options"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Kessler Syndrome"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        [GameParameters.CustomIntParameterUI("Chance of Debris Strike", newGameOnly = false, toolTip = "Chance of debris destroying a part when a debris cloud passes by")]
        public int DebrisStrikeChance = 20;

        [GameParameters.CustomIntParameterUI("Max Debris", minValue = 1, maxValue = 250, newGameOnly = false, toolTip = "How much debris is needed for a 100% chance of debris generating")]
        public int CloudChance = 100;
    }
}
