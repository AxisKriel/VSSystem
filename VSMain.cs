using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Newtonsoft.Json;

namespace PvPCommands
{
    [ApiVersion(1, 15)]
    public class VSSystem : TerrariaPlugin
    {
        #region PluginInfo
        public VSSystem(Main game) : base(game) { }

        Version version = new Version(1, 0, 0);
        public override Version Version
        {
            get { return version; }
        }

        public override string Name
        {
            get { return "Versus System"; }
        }

        public override string Author
        {
            get { return "Enerdy"; }
        }

        public override string Description
        {
            get { return "VSSystem - PvP Commands"; }
        }
        #endregion

        // Global Variables
        public static Timer UpdateCD = new Timer(1000);
        public static Dictionary<int, VSPlayer> VSPlayers = new Dictionary<int,VSPlayer>();
        public static List<VSCommand> PVPCommands = new List<VSCommand>();
        //public static string strVSCommands;

        public override void Initialize()
        {
            PlayerHooks.PlayerPostLogin += new PlayerHooks.PlayerPostLoginD(OnPlayerPostLogin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            Commands.ChatCommands.Add(new Command("vs.help", DoVSHelp, "vshelp") { HelpText = "Provides information about a PvP Command." });
            Commands.ChatCommands.Add(new Command("vs.list", DoVSList, "vslist") { HelpText = "Lists all of the currently available PvP Commands." });
            Commands.ChatCommands.Add(new Command("vs.reload", DoVSReload, "vsreload") { HelpText = "Reloads VSConfig.json." });
            //Commands.ChatCommands.Add(new Command("vs.commands.strike", DoStrike, "strike") { HelpText = "Usage: /strike <player> <amount>. Type /vshelp strike for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.heal", DoVSHeal, "vsheal") { HelpText = "Usage: /vsheal <player>. Type /vshelp vsheal for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.chill", DoCommand, "chill") { HelpText = "Usage: /chill <player>. Type /vshelp chill for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.stab", DoCommand, "stab") { HelpText = "Usage: /stab <player>. Type /vshelp stab for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.dedge", DoCommand, "dedge") { HelpText = "Usage: /dedge <player>. Type /vshelp dedge for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.tickle", DoCommand, "tickle") { HelpText = "Usage: /tickle <player>. Type /vshelp tickle for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.fsight", DoCommand, "fsight") { HelpText = "Usage: /fsight <player>. Type /vshelp fsight for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.doom", DoCommand, "doom") { HelpText = "Usage: /doom <player>. Type /vshelp doom for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.wish", DoCommand, "wish") { HelpText = "Usage: /wish <player>. Type /vshelp wish for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.hwish", DoCommand, "hwish") { HelpText = "Usage: /hwish <player>. Type /vshelp hwish for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.drain", DoCommand, "drain") { HelpText = "Usage: /drain <player>. Type /vshelp drain for more info", AllowServer = false });
            //Commands.ChatCommands.Add(new Command("vs.commands.rake", DoCommand, "rake") { HelpText = "Usage: /rake <player>. Type /vshelp rake for more info", AllowServer = false });

            VSConfig.ReadVSConfig();
            VSDatabase.SetupDB();
            BuildCommands();
            // Adding PvP Commands
            foreach (VSCommand cmd in PVPCommands)
            {
                Commands.ChatCommands.Add(new Command(cmd.Permission, DoCommand, cmd.Alias) { HelpText = string.Format("Usage: /{0} <player>. Type /vshelp {0} for more info", cmd.Alias.ToLower()), AllowServer = false });
            }
            UpdateCD.Elapsed += Update_Cooldowns;
            UpdateCD.Start();
        }

        private static void OnPlayerPostLogin(PlayerPostLoginEventArgs e)
        {
            int id = e.Player.UserID;
            if (VSDatabase.VSPlayerExists(id))
            {
                VSPlayers.Add(id, new VSPlayer(e.Player.Name));
                VSPlayers[id].Cooldowns = VSDatabase.GetCooldowns(id);
                LoadUserCooldowns(id);
                return;
            }
            AddToList(e.Player.Index, id);
        }

        private static void OnLeave(LeaveEventArgs e)
        {
            if (TShock.Players[e.Who] != null)
            {
                SaveToDb(VSPlayers[TShock.Players[e.Who].UserID]);
                VSPlayers.Remove(TShock.Players[e.Who].UserID);
            }
        }

        protected override void Dispose(bool disposing)
        {
            ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;
            UpdateCD.Elapsed -= Update_Cooldowns;
            SaveToDb();
            base.Dispose(disposing);
        }


        private static void BuildCommands()
        {
            #region oldcommands
            //VSCommand Stab = new VSCommand(1, "Stab", new[] { string.Format("Stabs target player. {0} base power.{1}", (config.Equalizer ? Equalize(200) : "200", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.StabCooldown) : "")), "Usage: /stab <player>" }, config.StabCooldown);
            //VSCommand DEdge = new VSCommand(2, "Double-Edged Strike", new[] { string.Format("Hits target player with a life-risking tackle. {0} base power, user takes 150 self-damage.{1}", (config.Equalizer ? Equalize(300) : "300", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.DedgeCooldown) : "")), "Usage: /dedge <player>" }, config.DedgeCooldown);
            //VSCommand Tickle = new VSCommand(3, "Tickle Freenzy", new[] { string.Format("Tickles target player, freezing them for 15 seconds.{0}", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.TickleCooldown) : "")), "Usage: /tickle <player>" }, config.StabCooldown);
            //VSCommand FSight = new VSCommand(4, "Future Sight", new[] { string.Format("Strikes target player after a 5 seconds delay. {0} base power.{1}", (config.Equalizer ? Equalize(250) : "250", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.FsightCooldown) : "")), "Usage: /fsight <player>" }, config.FsightCooldown);
            //VSCommand Drain = new VSCommand(5, "Drain", new[] { string.Format("Drains target player's health, healing the user for 80 health. {0} base power.{1}", (config.Equalizer ? Equalize(100) : "100", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.DrainCooldown) : "")), "Usage: /drain <player>" }, config.DrainCooldown);
            //VSCommand Silence = new VSCommand(6, "Silence!", new[] { string.Format("Silences target player for 8 seconds.{0}", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.SilenceCooldown) : "")), "Usage: /silence <player>" }, config.SilenceCooldown);
            //VSCommand Wish = new VSCommand(7, "Wish", new[] { string.Format("Wishes upon a star, healing target player or self for 200 health after 5 seconds.{0}", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.WishCooldown) : "")), "Usage: /wish [player] (leave blank for self)" }, config.WishCooldown);
            //VSCommand HWish = new VSCommand(8, "Healing Wish", new[] { string.Format("Sacrifices user to fully restore target ally or player after 5 seconds.{0}", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.HwishCooldown) : "")), "Usage: /hwish <player> (can't be used on self)" }, config.HwishCooldown);
            //VSCommand Chill = new VSCommand(9, "Chilling Glyph", new[] { string.Format("Enchanted glyph attack, slows target player for 7 seconds. {0} base power.{1}", (config.Equalizer ? Equalize(125) : "125", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.ChillCooldown) : "")), "Usage: /chill <player>" }, config.ChillCooldown);
            //VSCommand Flare = new VSCommand(10, "Flare Lash", new[] { string.Format("Lash that damages and burns target player for 20 seconds. {0} base power, 60 damage-over-time.{1}", (config.Equalizer ? Equalize(80) : "80", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.FlareCooldown) : "")), "Usage: /flare <player>" }, config.FlareCooldown);
            //VSCommand Doom = new VSCommand(11, "Doom Demise", new[] { string.Format("Dooms target player's fate, bombarding them to minimal health after 5 seconds. {0} base power.{1}", (config.Equalizer ? Equalize(400) : "400", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.DoomCooldown) : "")), "Usage: /doom <player>" }, config.DoomCooldown);
            //VSCommand Rake = new VSCommand(12, "Bloody Rake", new[] { string.Format("Rakes target player, hitting them 3 times. {0} + {1} + {2} base power.{3}", (config.Equalizer ? Equalize(100) : "100", (config.Equalizer ? Equalize(75) : "75", (config.Equalizer ? Equalize(50) : "50", (config.Cooldowns ? string.Format(" {0} seconds cooldown.", config.RakeCooldown) : "")), "Usage: /rake <player>" }, config.RakeCooldown);
            #endregion
            // VS Command Setup
            // Default Commands Count: 2 (Baseline), 10 (PvP)
            #region Strike
            VSCommand Strike = new VSCommand() // Baseline damage/heal command    
            {
                Name = "VS Strike",
                Alias = "strike",
                Permission = "strike",
                Description = new List<Message> { new Message("Damages or heals target [player] by [amount].", Color.Yellow), new Message("Usage: /strike <player> <amount>", Color.Yellow) },
                Cooldown = 15
            };
            Strike.Offensive = (Strike.Damage < 0 ? false : true);
            PVPCommands.Add(Strike);
            #endregion
            #region Heal
            VSCommand Heal = new VSCommand()
            {
                Name = "VS Heal",
                Alias = "vsheal",
                Permission = "heal",
                Damage = -100,
                MsgPlayer = new Message("You just got healed!", false, "", false, "", Color.Pink)
            };
            PVPCommands.Add(Heal);
            #endregion
            #region Stab
            VSCommand Stab = new VSCommand()
            {
                Name = "Stab",
                Alias = "stab",
                Description = new List<Message>() { new Message("Stabs target player, dealing pvp damage.", Color.Yellow), new Message("Usage: /stab <player>", Color.Yellow) },
                Permission = "stab",
                Damage = 200,
                Cooldown = VSConfig.config.StabCooldown
            };
            Stab.MsgAll = new Message("", false, "", true, " just got stabbed!", new Color(133, 96, 155));
            PVPCommands.Add(Stab);
            #endregion
            #region DEdge
            VSCommand DEdge = new VSCommand()
            {
                Name = "Double-Edge Slash",
                Alias = "dedge",
                Description = new List<Message>() { new Message("Hits target player with a reckoning force, dealing pvp damage. User takes 150 damage as recoil.", Color.Yellow), new Message("Usage: /dedge <player>", Color.Yellow) },
                Permission = "dedge",
                Damage = 300,
                DamageToSelf = 150,
                Cooldown = VSConfig.config.DedgeCooldown,
                MsgSelf = new Message("You have taken the recoil from the double-edged blade!", false, "", false, "", new Color(255, 255, 153))
            };
            DEdge.MsgAll = new Message("", true, " hits ", true, " with a life-risking strike!", new Color(255, 255, 153));
            PVPCommands.Add(DEdge);
            #endregion
            #region Tickle
            VSCommand Tickle = new VSCommand()
            {
                Name = "Tickle",
                Alias = "tickle",
                Description = new List<Message>() { new Message("Tickles target player, freezing them for 10 seconds.", Color.Yellow), new Message("Usage: /tickle <player>", Color.Yellow) },
                Permission = "tickle",
                BypassPvp = true,
                Offensive = false,
                Effect = new Effect("freeze", 10)
            };
            Tickle.MsgAll = new Message("", false, "", true, " is under a freezing attack of tickles!", new Color(243, 112, 234));
            Tickle.Timer.Interval = 10000;
            Tickle.Timer.Elapsed += Tickle.Timer_Tickle;
            PVPCommands.Add(Tickle);
            #endregion
            #region FSight
            VSCommand FSight = new VSCommand()
            {
                Name = "Future Sight",
                Alias = "fsight",
                Description = new List<Message>() { new Message("Casts a delayed blast, hitting target player for 250 damage. 5 seconds delay.", Color.Yellow), new Message("Usage: /fsight <player>", Color.Yellow) },
                Cooldown = VSConfig.config.FsightCooldown,
                Permission = "fsight"
            };
            FSight.MsgAll = new Message("", true, " foresaw ", true, "'s future!", Color.MediumPurple);
            FSight.Timer.Interval = 5000;
            FSight.Timer.Elapsed += FSight.Timer_FSight;
            PVPCommands.Add(FSight);
            #endregion
            #region Doom
            VSCommand Doom = new VSCommand()
            {
                Name = "Doom Desire",
                Alias = "doom",
                Description = new List<Message>() { new Message("Desires doom upon target player, dealing 400 true damage. 5 seconds delay.", Color.Yellow), new Message("Usage: /doom <player>", Color.Yellow) },
                Cooldown = VSConfig.config.DoomCooldown,
                Permission = "doom"
            };
            Doom.MsgAll = new Message("", true, " has doomed ", true, "'s fate!", Color.Orchid);
            Doom.Timer.Interval = 5000;
            Doom.Timer.Elapsed += Doom.Timer_Doom;
            PVPCommands.Add(Doom);
            #endregion
            #region Chill
            VSCommand Chill = new VSCommand()
            {
                Name = "Chilling Glyph",
                Alias = "chill",
                Permission = "chill",
                Damage = 125,
                Description = new List<Message>() { new Message("Casts an ice glyph on target player, chilling them for 7 seconds.", Color.Cyan), new Message("Usage: /chill <player>", Color.Yellow) },
                Effect = new Effect("chill", 7),
                MsgPlayer = new Message("You have been chilled!", false, "", false, "", Color.Cyan)
            };
            PVPCommands.Add(Chill);
            #endregion
            #region Wish
            VSCommand Wish = new VSCommand()
            {
                Name = "Wish",
                Alias = "wish",
                Description = new List<Message>() { new Message("Casts a wish on target player, healing them for 200 health. Can be self-cast. 5 seconds delay.", Color.Yellow), new Message("Usage: /wish <player>", Color.Yellow) },
                Permission = "wish",
                Cooldown = VSConfig.config.WishCooldown,
                Offensive = false
            };
            Wish.MsgAll = new Message("", true, " wished upon a star!", false, "", Color.RoyalBlue);
            Wish.Timer.Interval = 5000;
            Wish.Timer.Elapsed += Wish.Timer_Wish;
            PVPCommands.Add(Wish);
            #endregion
            #region HWish
            VSCommand HWish = new VSCommand()
            {
                Name = "Healing Wish",
                Alias = "hwish",
                UseSelf = false,
                Offensive = false,
                Description = new List<Message>() { new Message("Sacrifices user to fully restore target player after 5 seconds. Cannot be self-cast.", Color.Yellow), new Message("Usage: /hwish <player>", Color.Yellow) },
                Permission = "hwish",
                Cooldown = VSConfig.config.HwishCooldown,
                DamageToSelf = 10000
            };
            HWish.MsgAll = new Message("", true, " sacrificed himself for greater good!", false, "", Color.SeaGreen);
            HWish.MsgPlayer = new Message("You've been chosen by ", true, "!", false, "", Color.SeaGreen);
            HWish.Timer.Interval = 5000;
            HWish.Timer.Elapsed += HWish.Timer_HWish;
            PVPCommands.Add(HWish);
            #endregion
            #region Drain
            VSCommand Drain = new VSCommand()
            {
                Name = "Drain Life",
                Alias = "drain",
                Description = new List<Message>() { new Message("Steals target player's life energy, healing the user by 80.", Color.Yellow), new Message("Usage: /drain <player>", Color.Yellow) },
                Permission = "drain",
                Damage = 100,
                DamageToSelf = -80,
                Cooldown = VSConfig.config.DrainCooldown
            };
            Drain.MsgAll = new Message("", true, " has drained ", true, "'s life!", new Color(133, 96, 155));
            PVPCommands.Add(Drain);
            #endregion
            #region Rake
            VSCommand Rake = new VSCommand()
            {
                Name = "Rake",
                Alias = "rake",
                Description = new List<Message>() { new Message("Rakes target player, dealing initial damage plus extra bleeding damage over 4 seconds.", Color.Yellow), new Message("Usage: /rake <player>", Color.Yellow) },
                Permission = "rake",
                Damage = 100,
                Cooldown = VSConfig.config.RakeCooldown,
                MsgPlayer = new Message("You are now bleeding!", false, "", false, "", Color.Salmon)
            };
            Rake.MsgAll = new Message("", true, " just raked ", true, "!", Color.Salmon);
            Rake.Timer.Interval = 4000;
            Rake.Timer.Elapsed += Rake.Timer_Rake;
            PVPCommands.Add(Rake);
            #endregion
            #region Shield
            VSCommand Shield = new VSCommand()
            {
                Name = "Virtual Shield",
                Alias = "shield",
                Description = new List<Message>() { new Message("Activates a virtual shield, reducing damage taken by PvP Commands. Lasts 1 minute.", Color.DimGray), new Message("Usage: /shield <player>", Color.Yellow) },
                Permission = "shield",
                Offensive = false,
                BypassPvp = true,
                MsgSelf = new Message("Virtual Shield activated! Damage taken by PvP Commands will be reduced by 20%!", false, "", false, "", Color.DimGray),
                Effect = new Effect("userstate", 2)
            };
            PVPCommands.Add(Shield);
            #endregion
            // Adds Custom Commands
            foreach (VSCommand cmd in VSConfig.config.Commands)
            {
                PVPCommands.Add(cmd);
            }
        }

        private void Update_Cooldowns(object sender, ElapsedEventArgs e)
        {
            foreach (VSPlayer player in VSPlayers.Values)
            {
                foreach (VSCommand cmd in player.VSCommands)
                {
                    if (cmd.Counter > 0)
                    {
                        cmd.Counter--;
                    }
                    if (cmd == VSCommandByAlias(player, "rake") && cmd.Timer.Enabled)
                    {
                        VSPlayers[cmd.Target.UserID].DamagePlayer(30, cmd, cmd.User);
                    }

                }
            }
        }

        private void DoVSHelp(CommandArgs args)
        {
            TSPlayer ply = args.Player;
            if (args.Parameters.Count < 1)
            {
                ply.SendErrorMessage("Invalid syntax! Proper Syntax: /vshelp <command>");
                return;
            }
            VSCommand cmd = PVPCommandByAlias(args.Parameters[0]);
            if (args.Parameters[0].StartsWith("/"))
            {
                cmd = PVPCommandByAlias(args.Parameters[0].Substring(1));
            }
            if (args.Parameters.Count > 1 || cmd == null)
            {
                ply.SendErrorMessage("[PvP] Error: Invalid command entered! Type /vslist for a list of commands");
                return;
            }
            List<Message> print = new List<Message>()
            {
                new Message("Name: " + cmd.Name, Color.LimeGreen),
                new Message("Type: " + (cmd.Damage < 0 ? "Heal | " + Equalize(cmd.Damage) : (cmd.Damage == 0 ? "Other" : "Damage | " + cmd.Damage)), (cmd.Damage < 0 ? Color.SpringGreen : (cmd.Damage == 0 ? Color.White : Color.Red)))
            };
            print.AddRange(cmd.Description);
            if (VSConfig.config.Cooldowns)
            {
                if (cmd.Cooldown > 0)
                {
                    Message text = print[2];
                    print.RemoveAt(2);
                    print.Insert(2, new Message(text.Text + string.Format(" {0} seconds cooldown.", cmd.Cooldown), text.Color));
                }
            }

            ply.SendSuccessMessage(string.Format("/{0} vshelp:", cmd.Alias));
            foreach (Message line in print)
            {
                ply.SendMessage(line.Text, line.Color);
            }
        }

        private void DoVSList(CommandArgs args)
        {
            int pageNumber;
            if (args.Parameters.Count == 0 || int.TryParse(args.Parameters[0], out pageNumber))
            {
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 0, args.Player, out pageNumber))
                {
                    return;
                }
                IEnumerable<string> cmdAlias = from cmd in PVPCommands
                                               where cmd.CanRun(args.Player)
                                               select "/" + cmd.Alias;

                PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(cmdAlias),
                        new PaginationTools.Settings
                        {
                            HeaderFormat = "PVP Commands ({0}/{1}):",
                            FooterFormat = "Type /vslist {0} for more."
                        });
            }
            else
            {
                args.Player.SendErrorMessage("Invalid Syntax! Proper syntax: /vslist [page]");
            }
        }

        private void DoVSReload(CommandArgs args)
        {
            if (VSConfig.ReadVSConfig())
            {
                args.Player.SendSuccessMessage("[VSSystem] Config file reloaded successfully!");
            }
            else
            {
                args.Player.SendErrorMessage("[VSSystem] Config file reload failed! Check logs for details.");
            }
        }

        private static void VSExecute(VSPlayer user, VSCommand command, TSPlayer target)
        {
            // Sets command user and target
            command.User = user;
            command.Target = target;

            // Self-use Check & PvP Check
            if (!command.UseSelf && user.TSPlayer == target)
            {
                user.TSPlayer.SendErrorMessage("[PvP] Error: You cannot use this command on yourself!");
                return;
            }
            if (!PVPCheck(target) && !command.BypassPvp)
            {
                user.TSPlayer.SendErrorMessage("[PvP] Error: Target is not hostile!");
                return;
            }

            // Cooldown Check
            if (VSConfig.config.Cooldowns && !user.TSPlayer.Group.HasPermission("vs.ignore.cooldown"))
            {
                if (command.Counter > 0)
                {
                    user.TSPlayer.SendErrorMessage(string.Format("[PvP] Error: You must wait {0} seconds before using {1} again!", command.Counter, command.Name));
                    return;
                }
                else
                {
                    command.Counter = command.Cooldown;
                }
            }

            // Damage Check
            if (command.Damage != 0)
            {
                VSPlayers[target.UserID].DamagePlayer(command.Damage, command, user);
            }
            if (command.DamageToSelf != 0)
            {
                user.DamagePlayer(command.DamageToSelf, command, user);
            }

            // Effect Check
            if (command.Effect != null)
            {
                Effect.Event(user, target, command.Effect.Type, command.Effect.Parameter);
            }

            // Timer Check
            if (command.Timer.Interval != 1)
            {
                command.Timer.Start();
            }

            // Messages Check
            if (command.MsgAll != null)
            {
                Message msg = new Message(command.MsgAll.InitMsg, command.MsgAll.UsePlr1 ? command.User.TSPlayer : null, command.MsgAll.MidMsg, command.MsgAll.UsePlr2 ? command.Target : null, command.MsgAll.EndMsg, command.MsgAll.Color);
                TSPlayer.All.SendMessage(msg.Text, msg.Color);
            }
            if (command.MsgSelf != null)
            {
                Message msg = new Message(command.MsgSelf.InitMsg, command.MsgSelf.UsePlr1 ? command.User.TSPlayer : null, command.MsgSelf.MidMsg, command.MsgSelf.UsePlr2 ? command.Target : null, command.MsgSelf.EndMsg, command.MsgSelf.Color);
                user.TSPlayer.SendMessage(msg.Text, msg.Color);
            }
            if (command.MsgPlayer != null)
            {
                Message msg = new Message(command.MsgPlayer.InitMsg, command.MsgPlayer.UsePlr1 ? command.User.TSPlayer : null, command.MsgPlayer.MidMsg, command.MsgPlayer.UsePlr2 ? command.Target : null, command.MsgPlayer.EndMsg, command.MsgPlayer.Color);
                target.SendMessage(msg.Text, msg.Color);
            }
            return;
        }

        private static void DoCommand(CommandArgs args)
        {
            VSPlayer ply = VSPlayers[args.Player.UserID];
            VSCommand cmd = null;
            foreach (VSCommand cmdfound in ply.VSCommands)
            {
                if (args.Message.StartsWith(cmdfound.Alias))
                {
                    cmd = cmdfound;
                }
            }
            if (cmd == null)
            {
                ply.TSPlayer.SendErrorMessage("[PvP] Error: Invalid command entered! Type /vslist for a list of commands");
                return;
            }
            string query = "";
            if (args.Parameters.Count < 1)
            {
                ply.TSPlayer.SendErrorMessage(string.Format("Invalid syntax! Proper syntax: /{0} <player>", cmd.Alias));
                return;
            }
            else
            {
                query = string.Join(" ", args.Parameters);
            }
            var found = TShock.Utils.FindPlayer(query);
            if (found.Count < 1)
            {
                ply.TSPlayer.SendErrorMessage("[PvP] Error: No players matched!");
                return;
            }
            else if (found.Count > 1)
            {
                string list = string.Join(", ", found);
                ply.TSPlayer.SendErrorMessage("[PvP] Error: More than one player matched! Matches: " + list);
                return;
            }
            else
            {
                VSExecute(ply, cmd, found[0]);
                return;
            }
        }

        private void DoStrike(CommandArgs args)
        {
            VSPlayer ply = VSPlayers[args.Player.UserID];
            VSCommand cmd = VSCommandByAlias(ply, "strike");
            string query = "";
            if (args.Parameters.Count != 2)
            {
                ply.TSPlayer.SendErrorMessage(string.Format("Invalid syntax! Proper syntax: /{0} <player> <amount>", cmd.Alias));
                return;
            }
            else
            {
                query = args.Parameters[0];
            }
            var found = TShock.Utils.FindPlayer(query);
            if (found.Count < 1)
            {
                ply.TSPlayer.SendErrorMessage("[PvP] Error: No players matched!");
                return;
            }
            else if (found.Count > 1)
            {
                string list = string.Join(", ", found);
                ply.TSPlayer.SendErrorMessage("[PvP] Error: More than one player matched! Matches: " + list);
                return;
            }
            else
            {
                double damage = 0;
                if (!Double.TryParse(args.Parameters[1], out damage))
                {
                    ply.TSPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /strike <player> <amount>");
                    return;
                }
                cmd.Damage = damage;
                cmd.MsgPlayer = new Message(string.Format("[VS] Strike: You just got {0}!", (damage < 0 ? "healed" : "hit")), false, "", false, "", (damage < 0 ? Color.LimeGreen : Color.Red));
                VSExecute(ply, cmd, found[0]);
                return;
            }
        }

        private void DoVSHeal(CommandArgs args)
        {
            VSCommand cmd = PVPCommandByAlias("vsheal");
            TSPlayer ply = args.Player;
            string query = "";
            if (args.Parameters.Count < 1)
            {
                ply.SendErrorMessage(string.Format("Invalid syntax! Proper syntax: /{0} <player>", cmd.Alias));
                return;
            }
            else
            {
                query = string.Join(" ", args.Parameters);
            }
            var found = TShock.Utils.FindPlayer(query);
            if (found.Count < 1)
            {
                ply.SendErrorMessage("[PvP] Error: No players matched!");
                return;
            }
            else if (found.Count > 1)
            {
                string list = string.Join(", ", found);
                ply.SendErrorMessage("[PvP] Error: More than one player matched! Matches: " + list);
                return;
            }
            else
            {
                VSExecute(VSPlayers[ply.Index], cmd, found[0]);
                return;
            }
        }


        #region UTILS

        private static bool PVPCheck(TSPlayer plr)
        {
            if (VSConfig.config.PvPOnly)
            {
                if (plr.TPlayer.hostile)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static int Equalize(double damage)
        {
            if (damage < 0)
            {
                damage *= -1;
                return (int)damage;
            }
            return (int)damage;
        }
        public static int Equalize(double damage, TSPlayer target)
        {
            if (damage < 0)
            {
                damage *= -1;
                return (int)damage;
            }
            else if (!VSConfig.config.Equalizer)
            {
                return (int)damage;
            }
            else
            {
                double multiplier = (double)target.FirstMaxHP / 500;
                damage *= multiplier;
                return (int)damage;
            }
        }
        private static void AddToList(int Index, int UserID)
        {
            //if (PVPCommands.Count < 1)
            //{
            //    VSPlayers.Insert(UserID, new VSPlayer(UserID));
            //    BuildCommands();
            //    foreach (VSCommand cmd in PVPCommands)
            //    {
            //        VSPlayers[UserID].VSCommands.Add(cmd);
            //        strVSCommands += cmd.Alias + " ";
            //        VSPlayers[UserID].Cooldowns.Add(cmd.Alias, cmd.Counter);
            //        VSDatabase.AddCooldown(UserID, cmd.Alias, cmd.Counter);
            //    }
            //    strVSCommands.Trim();
            //    VSDatabase.AddVSPlayer(UserID, TShock.Players[UserID].Name, strVSCommands);
            //    return;
            //}
            VSPlayers.Add(UserID, new VSPlayer(Index));
            if (!VSDatabase.AddVSPlayer(UserID, TShock.Players[Index].Name))
            {
                Log.ConsoleError("[PvP Commands] failed to create VSPlayer for user " + TShock.Players[Index].Name);
                return;
            }
            VSPlayers[UserID].VSCommands.AddRange(PVPCommands);
            foreach (VSCommand cmd in VSPlayers[UserID].VSCommands)
            {
                VSPlayers[UserID].Cooldowns.Add(cmd.Alias, cmd.Counter);
                VSDatabase.AddCooldown(UserID, cmd.Alias, cmd.Counter);
            }
            
            return;
        }
        private static void SaveToDb(VSPlayer ply)
        {
            foreach (VSCommand cmd in ply.VSCommands)
            {
                VSDatabase.SaveCooldown(ply.UserID, cmd.Alias, cmd.Counter);
            }
        }
        private static void SaveToDb()
        {
            foreach (VSPlayer ply in VSPlayers.Values)
            {
                if (ply.TSPlayer.Active)
                {
                    foreach (VSCommand cmd in ply.VSCommands)
                    {
                        VSDatabase.SaveCooldown(ply.UserID, cmd.Alias, cmd.Counter);
                    }
                }
            }
        }
        private static void LoadUserCooldowns(int UserID)
        {
            VSPlayer ply = VSPlayers[UserID];
            if (ply.VSCommands.Count < 1)
            {
                ply.VSCommands.AddRange(PVPCommands);
            }
            foreach (var cooldown in ply.Cooldowns)
            {
                foreach (VSCommand cmd in ply.VSCommands)
                {
                    if (cmd.Alias == cooldown.Key)
                    {
                        cmd.Counter = cooldown.Value;
                    }
                }
            }
        }
        private void RemoveFromList(int id)
        {
            VSPlayers.Remove(id);
        }

        //public static VSPlayer VSPlayerById(int UserID)
        //{
        //    foreach (VSPlayer plr in VSPlayers)
        //    {
        //        if (plr.TSPlayer.UserID == UserID)
        //        {
        //            return plr;
        //        }
        //    }
        //    return null;
        //}
        //private static VSPlayer VSPlayerByUserID(int UserID)
        //{
        //    List<VSPlayer> pulled = VSDatabase.PullPlayers();
        //    foreach (VSPlayer player in pulled)
        //    {
        //        if (player.TSPlayer == null)
        //        {
        //            player.TSPlayer = TShock.Utils.FindPlayer(player.Index
        //        }
        //    }
        //    return null;
        //}
        private static VSCommand PVPCommandByAlias(string alias)
        {
            foreach (VSCommand cmd in PVPCommands)
            {
                if (cmd.Alias == alias.ToLower())
                {
                    return cmd;
                }
            }
            return null;
        }
        private static VSCommand VSCommandByAlias(VSPlayer player, string alias)
        {
            foreach (VSCommand cmd in player.VSCommands)
            {
                if (alias == cmd.Alias)
                {
                    return cmd;
                }
            }
            return null;
        }

        #endregion
    }
}