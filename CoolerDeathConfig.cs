using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CoolerDeath
{
    public class CoolerDeathConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        public bool ShowQuotes { get; set; }

        [DefaultValue(true)]
        public bool ShowCoinLoss { get; set; }

        [DefaultValue(true)]
        public bool ShowRespawnTimer { get; set; }

        [DefaultValue(true)]
        public bool DarkBackground { get; set; }

        [DefaultValue(0.6f)]
        [Range(0.1f, 1f)]
        public float BackgroundDarkness { get; set; }

        [DefaultValue(true)]
        public bool EnableFadeEffects { get; set; }
    }
}