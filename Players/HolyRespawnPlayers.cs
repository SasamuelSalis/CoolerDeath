using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CoolerDeath.Players
{
    public class HolyRespawnPlayer : ModPlayer
    {
        public override void OnRespawn()
        {
            var configValue = ModContent.GetInstance<CoolerDeathConfig>().EnableRespawnEffects;
            Main.NewText($"Config Value: {configValue}");

            if (configValue)
            {
                // Burst of holy particles
                for (int i = 0; i < 45; i++)
                {
                    var dust = Dust.NewDustPerfect(
                        Player.Center,
                        Terraria.ID.DustID.GoldFlame,
                        new Vector2(
                            Main.rand.NextFloat(-3f, 3f),
                            Main.rand.NextFloat(-6f, -1f)
                        ),
                        150,
                        Color.White,
                        Main.rand.NextFloat(1f, 1.7f)
                    );
                    dust.noGravity = true;
                }

                // Floating sparkles
                for (int i = 0; i < 20; i++)
                {
                    var dust = Dust.NewDustPerfect(
                        Player.Center,
                        Terraria.ID.DustID.WhiteTorch,
                        new Vector2(
                            Main.rand.NextFloat(-1.3f, 1.3f),
                            Main.rand.NextFloat(-1f, 0f)
                        ),
                        100,
                        Color.Gold,
                        1.3f
                    );
                    dust.noGravity = false;
                }

                // Play holy respawn sound
                Terraria.Audio.SoundEngine.PlaySound(
                    new Terraria.Audio.SoundStyle("Terraria/Sounds/Item_30")
                );
            }
        }
    }
}