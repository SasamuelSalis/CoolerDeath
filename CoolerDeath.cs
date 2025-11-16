using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace CoolerDeath
{
    public class CoolerDeathPlayer : ModPlayer
    {
        public PlayerDeathReason lastDeathReason;
        public string lostCoinsString;
        public bool deathActive;
        public string deathQuote;
        public string deathCause;

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            var config = ModContent.GetInstance<CoolerDeathConfig>();

            lastDeathReason = damageSource;
            lostCoinsString = Player.lostCoinString;
            deathActive = true;

            if (config.ShowQuotes)
            {
                int quoteNumber = Main.rand.Next(0, 84); // 0 to 83 inclusive
                deathQuote = Language.GetTextValue($"Mods.CoolerDeath.UI.Quotes.{quoteNumber}");
            }
            else
                deathQuote = "";

            deathCause = GetDeathCause(damageSource, Player);
        }

        public override void PostUpdate()
        {
            if (deathActive && !Player.dead)
            {
                deathActive = false;
            }
        }

        private static string GetDeathCause(PlayerDeathReason reason, Player player)
        {
            if (reason == null)
                return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Default");

            // Check for custom death reason first
            if (reason.CustomReason != null && !string.IsNullOrEmpty(reason.CustomReason.ToString()))
                return reason.CustomReason.ToString();

            // Try to get the causing entity
            if (reason.TryGetCausingEntity(out Entity entity))
            {
                switch (entity)
                {
                    case NPC npc:
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.SlainByNPC", npc.FullName);
                    case Projectile proj:
                        string projName = Lang.GetProjectileName(proj.type).Value;
                        if (!string.IsNullOrEmpty(projName) && projName != "")
                            return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.SlainByProjectile", projName);
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.SlainByDefault");
                    case Player otherPlayer:
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.SlainByPlayer", otherPlayer.name);
                }
            }

            // Check SourceOtherIndex for environmental deaths
            switch (reason.SourceOtherIndex)
            {
                case 0: return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Fell");
                case 1: return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Drowned");
                case 2: return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Lava");
                case 3:
                    // Check if player was suffocating (sand/slush/silt) at the MOMENT of death
                    if (player.suffocating)
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Suffocated");
                    return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Spikes");
                default:
                    // Check for common debuff deaths
                    if (player.onFire)
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Burned");
                    if (player.HasBuff(67)) // Burning buff from hot blocks like Hellstone
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Burning");
                    if (player.onFire2) // Cursed Inferno
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Cursed");
                    if (player.poisoned)
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Poisoned");
                    if (player.venom)
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Envenomed");
                    if (player.electrified)
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Electrocuted");
                    if (player.frostBurn)
                        return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Froze");

                    string vanillaText = reason.GetDeathText(player.name).ToString();
                    if (vanillaText.Contains(" was "))
                    {
                        int index = vanillaText.IndexOf(" was ");
                        if (index > 0)
                            return "You were" + vanillaText.Substring(index + 4); // +4 to skip " was"
                    }
                    return Language.GetTextValue("Mods.CoolerDeath.DeathMessages.Default");
            }
        }
    }

    public class CoolerDeath : ModSystem
    {
        private float fade;
        private bool wasDeadLastFrame;

        public override void PostUpdateEverything()
        {
            var config = ModContent.GetInstance<CoolerDeathConfig>();
            Player player = Main.LocalPlayer;
            CoolerDeathPlayer cdPlayer = player.GetModPlayer<CoolerDeathPlayer>();

            if (player.dead && cdPlayer.deathActive)
            {
                if (config.EnableFadeEffects)
                    fade = MathHelper.Clamp(fade + 0.02f, 0f, 1f);
                else
                    fade = 1f; // Instant show if fade disabled
            }
            else
            {
                if (config.EnableFadeEffects)
                    fade = MathHelper.Clamp(fade - 0.05f, 0f, 1f);
                else
                    fade = 0f; // Instant hide if fade disabled
            }

            wasDeadLastFrame = player.dead;
        }

        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            int vanillaIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Death Text"));
            if (vanillaIndex != -1)
            {
                layers.RemoveAt(vanillaIndex);
                layers.Insert(vanillaIndex, new LegacyGameInterfaceLayer(
                    "CoolerDeath: Custom Death Screen",
                    delegate
                    {
                        DrawCoolDeath(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        private string FormatColoredCoins(long totalCoins)
        {
            if (totalCoins == 0)
                return Language.GetTextValue("Mods.CoolerDeath.UI.LostNoMoney");

            int platinum = (int)(totalCoins / 1000000);
            int gold = (int)((totalCoins % 1000000) / 10000);
            int silver = (int)((totalCoins % 10000) / 100);
            int copper = (int)(totalCoins % 100);

            List<string> parts = new List<string>();

            if (platinum > 0)
                parts.Add($"[i:{ItemID.PlatinumCoin}]{platinum}");
            if (gold > 0)
                parts.Add($"[i:{ItemID.GoldCoin}]{gold}");
            if (silver > 0)
                parts.Add($"[i:{ItemID.SilverCoin}]{silver}");
            if (copper > 0)
                parts.Add($"[i:{ItemID.CopperCoin}]{copper}");

            string coinsText = string.Join(" ", parts);
            return $"[c/AAAAAA:{Language.GetTextValue("Mods.CoolerDeath.UI.LostCoins")}] {coinsText}";
        }

        private void DrawColoredCoinText(SpriteBatch spriteBatch, string text, Vector2 position, float baseAlpha)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // Split the text by both color tags and item icons
            string[] parts = Regex.Split(text, @"(\[c\/[0-9A-Fa-f]{6}:[^\]]+\]|\[i:\d+\])");

            float currentX = position.X;
            Color defaultColor = Color.Lerp(Color.Transparent, Color.Gray, baseAlpha);

            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                // Check if this part is a color tag
                if (part.StartsWith("[c/") && part.EndsWith("]"))
                {
                    // Extract color and text from [c/FFFFFF:Text] format
                    int colorEnd = part.IndexOf(':', 3);
                    if (colorEnd > 0)
                    {
                        string colorHex = part.Substring(3, colorEnd - 3);
                        string coloredText = part.Substring(colorEnd + 1, part.Length - colorEnd - 2);

                        Color color = HexToColor(colorHex);
                        Color finalColor = Color.Lerp(Color.Transparent, color, baseAlpha);

                        Vector2 textSize = FontAssets.MouseText.Value.MeasureString(coloredText);
                        Utils.DrawBorderString(spriteBatch, coloredText, new Vector2(currentX, position.Y), finalColor, 1f, 0f, 0f);
                        currentX += textSize.X;
                    }
                }
                // Check if this part is an item icon
                else if (part.StartsWith("[i:") && part.EndsWith("]"))
                {
                    string itemIdStr = part.Substring(3, part.Length - 4);
                    if (int.TryParse(itemIdStr, out int itemId))
                    {
                        // Draw the item icon
                        Texture2D itemTexture = TextureAssets.Item[itemId].Value;
                        float iconScale = 0.75f; // Slightly smaller than normal
                        Rectangle sourceRect = itemTexture.Bounds;
                        Vector2 iconSize = new Vector2(sourceRect.Width * iconScale, sourceRect.Height * iconScale);

                        spriteBatch.Draw(itemTexture, new Vector2(currentX, position.Y - iconSize.Y * 0.5f + 8f),
                            sourceRect, Color.Lerp(Color.Transparent, Color.White, baseAlpha), 0f, Vector2.Zero, iconScale, SpriteEffects.None, 0f);

                        currentX += iconSize.X + 4f; // Small spacing after icon
                    }
                }
                else
                {
                    // Regular text (coin amounts)
                    Vector2 textSize = FontAssets.MouseText.Value.MeasureString(part);
                    Utils.DrawBorderString(spriteBatch, part, new Vector2(currentX, position.Y), defaultColor, 1f, 0f, 0f);
                    currentX += textSize.X;
                }
            }
        }

        private Color HexToColor(string hex)
        {
            if (hex.Length == 6)
            {
                return new Color(
                    Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                    Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                    Convert.ToInt32(hex.Substring(4, 2), 16) / 255f
                );
            }
            return Color.White;
        }

        private void DrawCoolDeath(SpriteBatch spriteBatch)
        {
            if (fade <= 0f)
                return;

            var config = ModContent.GetInstance<CoolerDeathConfig>();
            Player player = Main.LocalPlayer;
            CoolerDeathPlayer cdPlayer = player.GetModPlayer<CoolerDeathPlayer>();

            if (!cdPlayer.deathActive)
                return;

            // Dark background
            if (config.DarkBackground)
            {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    Color.Black * (fade * config.BackgroundDarkness));
            }

            Vector2 center = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);

            // Death cause (always shown)
            Utils.DrawBorderStringBig(spriteBatch, cdPlayer.deathCause, center,
                Color.Lerp(Color.Transparent, Color.Red, fade), 1f, 0.5f, 0.5f);

            // Quote (configurable)
            if (config.ShowQuotes && fade > 0.2f && !string.IsNullOrEmpty(cdPlayer.deathQuote))
            {
                float quoteFade = MathHelper.Clamp((fade - 0.2f) / 0.8f, 0f, 1f);
                Utils.DrawBorderString(spriteBatch, $"\"{cdPlayer.deathQuote}\"", center + new Vector2(0, 60f),
                    Color.Lerp(Color.Transparent, Color.White, quoteFade), 1f, 0.5f, 0.5f);
            }

            // Coins lost (configurable)
            if (config.ShowCoinLoss && fade > 0.4f)
            {
                string coinText = FormatColoredCoins(player.lostCoins);
                float coinFade = (fade - 0.4f) / 0.6f;

                if (player.lostCoins == 0)
                {
                    Utils.DrawBorderString(spriteBatch, coinText, center + new Vector2(0, 85f),
                        Color.Lerp(Color.Transparent, Color.Gray, coinFade), 1f, 0.5f, 0.5f);
                }
                else
                {
                    // Remove color and item tags to measure plain text width for centering
                    string plainText = Regex.Replace(coinText, @"\[c\/[0-9A-Fa-f]{6}:([^\]]+)\]", "$1");
                    plainText = Regex.Replace(plainText, @"\[i:\d+\]", "");
                    Vector2 textSize = FontAssets.MouseText.Value.MeasureString(plainText);
                    DrawColoredCoinText(spriteBatch, coinText, center + new Vector2(-textSize.X * 0.5f, 85f), coinFade);
                }
            }

            // Respawn timer (configurable)
            if (config.ShowRespawnTimer && player.respawnTimer > 0)
            {
                float seconds = 1f + player.respawnTimer / 60f;
                string respawnText = Language.GetTextValue("Mods.CoolerDeath.UI.Respawning", seconds.ToString("0"));
                Utils.DrawBorderString(spriteBatch, respawnText, center + new Vector2(0, 140f),
                    Color.Lerp(Color.Transparent, Color.Gray, fade), 1f, 0.5f, 0.5f);
            }
        }
    }
}