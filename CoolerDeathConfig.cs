using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CoolerDeath
{
    public class CoolerDeathConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        [LabelKey("$Mods.CoolerDeath.Config.ShowQuotes.Label")]
        [TooltipKey("$Mods.CoolerDeath.Config.ShowQuotes.Tooltip")]
        public bool ShowQuotes { get; set; }

        [DefaultValue(true)]
        [LabelKey("$Mods.CoolerDeath.Config.ShowCoinLoss.Label")]
        [TooltipKey("$Mods.CoolerDeath.Config.ShowCoinLoss.Tooltip")]
        public bool ShowCoinLoss { get; set; }

        [DefaultValue(true)]
        [LabelKey("$Mods.CoolerDeath.Config.ShowRespawnTimer.Label")]
        [TooltipKey("$Mods.CoolerDeath.Config.ShowRespawnTimer.Tooltip")]
        public bool ShowRespawnTimer { get; set; }

        [DefaultValue(true)]
        [LabelKey("$Mods.CoolerDeath.Config.DarkBackground.Label")]
        [TooltipKey("$Mods.CoolerDeath.Config.DarkBackground.Tooltip")]
        public bool DarkBackground { get; set; }

        [DefaultValue(0.6f)]
        [Range(0.1f, 1f)]
        [LabelKey("$Mods.CoolerDeath.Config.BackgroundDarkness.Label")]
        [TooltipKey("$Mods.CoolerDeath.Config.BackgroundDarkness.Tooltip")]
        public float BackgroundDarkness { get; set; }

        [DefaultValue(true)]
        [LabelKey("$Mods.CoolerDeath.Config.EnableFadeEffects.Label")]
        [TooltipKey("$Mods.CoolerDeath.Config.EnableFadeEffects.Tooltip")]
        public bool EnableFadeEffects { get; set; }
    }
}
