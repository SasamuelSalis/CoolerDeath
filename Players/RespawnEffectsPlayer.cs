using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace CoolerDeath.Content.Players
{
    public class RespawnEffectsPlayer : ModPlayer
    {
        private bool playingEffect = false;
        private int timer = 0;

        public override void OnRespawn()
        {
            playingEffect = true;
            timer = 0;
        }

        public override void PostUpdate()
        {
            if (ModContent.GetInstance<CoolerDeathConfig>().EnableRespawnEffects == true)
            {

                if (!playingEffect)
                    return;

                timer++;

                // End after 30 ticks (1/2 second)
                if (timer > 30)
                {
                    playingEffect = false;
                    return;
                }

                // Move halo slightly upward above the player’s head
                Vector2 pos = Player.Center + new Vector2(0, -34f);

                // Smooth 0→1→0 wing flap
                float flap = (float)Math.Sin((timer / 30f) * Math.PI);

                // ============================
                // 1. OVAL HALO (fixed)
                // ============================
                int haloPoints = 24;

                for (int i = 0; i < haloPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / haloPoints;

                    Vector2 offset =
                        new Vector2(
                            (float)Math.Cos(angle) * 18f,  // wide oval
                            (float)Math.Sin(angle) * 6f    // short oval
                        );

                    Dust d = Dust.NewDustPerfect(pos + offset, DustID.YellowStarDust);
                    d.noGravity = true;
                    d.scale = 1.3f + flap * 0.3f;      // pulse with flap
                    d.fadeIn = 0.5f;
                    d.alpha = 40 + timer * 4;          // fade-out
                    d.velocity *= 0.1f;
                }

                // ============================
                // 2. WINGS :P
                // ============================
                float wingSpread = 12f + flap * 14f; // wings open wider during flap

                for (int side = -1; side <= 1; side += 2) // left/right
                {
                    for (float t = 0; t <= 1f; t += 0.06f)
                    {
                        float x = side * (24f + (float)Math.Sin(t * Math.PI) * wingSpread);

                        // ↓↓↓ LOWERED wings here (from -5f to -2f)
                        float y = 0 + t * 40f;

                        Dust d = Dust.NewDustPerfect(pos + new Vector2(x, y), DustID.WhiteTorch);
                        d.noGravity = true;
                        d.scale = 1.4f + flap * 0.3f;
                        d.alpha = 40 + timer * 5;
                        d.fadeIn = 1f;
                        d.velocity *= 0f;
                    }
                }

                // ============================
                // 3. Sparkle flash
                // ============================
                if (timer < 6)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame);
                        d.velocity *= 0.3f;
                        d.noGravity = true;
                        d.scale = 1.4f;
                    }
                }

                // Golden glow
                Lighting.AddLight(Player.Center, 1.6f, 1.4f, 1.0f);
            }
        }
    }
}
