using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;

namespace PvPCommands
{
    public class VSConfig
    {
        public bool PvPOnly = true;
        public bool Cooldowns = true;
        public bool Equalizer = false;
        // Default Cooldown Formula: Damage|Heal Amount * 1.2 * (delayed ? 0.80 : 1) * (effect ? 1.3 : 1) * (heal ? 0.6 : 1) seconds.
        // Exceptions: Tickle, Silence, Healing Wish
        public int StabCooldown = 240;
        public int DedgeCooldown = 360;
        public int TickleCooldown = 300;
        public int FsightCooldown = 240;
        public int DrainCooldown = 156; // Kinda rounded here
        public int SilenceCooldown = 210;
        public int WishCooldown = 144;
        public int HwishCooldown = 300;
        public int ChillCooldown = 195;
        public int FlareCooldown = 250; // 80 + 4 * 20 = 160
        public int DoomCooldown = 384;
        public int RakeCooldown = 270;
        public int BoostCooldown = 120;
        public int ShieldCooldown = 120;
        public int BarrierCooldown = 150;
        public int LocusCooldown = 180;
        public List<VSCommand> Commands = new List<VSCommand>()
        {
            // Example Commands
            // Only Name is required for a command to work, but it won't really do anything.
            // Add Damage and a message to all as a minimum. Note that it is always safer to manually set Alias and Permission.
            new VSCommand("Smash")              // Constructor with Name. Auto-Alias / Auto-Permission will only work this way
            {
                Damage = 150,                   // Command Damage to target (negative heals player)
                Cooldown = 120,                 // Command Cooldown in seconds
                // Command Description - shown with /vshelp <command>. Each line has a string and a color.
                Description = new List<Message>
                {
                    new Message("Smashes target player, dealing pvp damage.", Color.Yellow),
                    new Message("Usage: /smash <player>", Color.Yellow)
                },
                // Message to send when the command is executed. You can choose from MsgAll, MsgPlayer and MsgSelf.
                // Syntax: new Message(" firstmessage ", whether to show user's name (true/false), " secondmessage ",
                // whether to show target's name (true/false), " lastmessage ", Color.ColorName)
                MsgAll = new Message("", true, " just smashed ", true, "!", Color.Brown)
            },
            new VSCommand("Knuckle")
            {
                Name = "Burning Knuckle",       // Custom Name; The name above will be used for auto-permission / alias
                Damage = 120,
                Cooldown = 180,
                // Description with different color (Firebrick)
                Description = new List<Message>
                {
                    new Message("Punches target player with flare fists, burning them for 15 seconds.", Color.Firebrick),
                    new Message("Usage: /knuckle <player>", Color.Yellow)
                },
                MsgAll = new Message("", true, " hits ", true, " with his Burning Knuckle!", Color.Firebrick),
                // This is when it gets interesting: You can add effects to commands. First arg is the type (string),
                // second arg is the parameter (duration, delayed damage, etc). Some effects require more than one parameter.
                // Check README.MD for a list of all available effects.
                Effect = new List<Effect>() { new Effect(EffectTypes.BUFF, 24, 15) }
            }
        };

        private static string savepath = Path.Combine(TShock.SavePath, "VSSystem");
        public static VSConfig config;

        private static void CreateConfig()
        {
            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }
            string filepath = Path.Combine(savepath, "VSConfig.json");
            try
            {
                using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (var sr = new StreamWriter(stream))
                    {
                        config = new VSConfig();
                        var configString = JsonConvert.SerializeObject(config, Formatting.Indented);
                        sr.Write(configString);
                    }
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Log.ConsoleError(ex.Message);
                config = new VSConfig();
            }
        }

        public static bool ReadVSConfig()
        {
            string filepath = Path.Combine(savepath, "VSConfig.json");
            try
            {
                if (File.Exists(filepath))
                {
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            var configString = sr.ReadToEnd();
                            config = JsonConvert.DeserializeObject<VSConfig>(configString);
                        }
                        stream.Close();
                    }
                    return true;
                }
                else
                {
                    Log.ConsoleError("[VS System] config not found. Creating new one...");
                    CreateConfig();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.ConsoleError(ex.Message);
            }
            return false;
        }
    }
}
