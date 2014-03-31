using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using TShockAPI;
using Newtonsoft.Json;

namespace PvPCommands
{
    public class VSPlayer
    {
        /// <summary>
        /// Gets the TSPlayer's Index.
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Gets the TSPlayer's UserID.
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// Gets or sets the player's TSPlayer.
        /// </summary>
        public TSPlayer TSPlayer { get; set; }
        /// <summary>
        /// Gets or sets the player's list of commands.
        /// </summary>
        public List<VSCommand> VSCommands { get; set; }
        /// <summary>
        /// Gets or sets the player cooldowns.
        /// </summary>
        public Dictionary<string, int> Cooldowns { get; set; }
        /// <summary>
        /// Gets of sets timers related to the user's state. Key is Type, value is Count.
        /// </summary>
        //public List<int> State { get; set; }
        public Dictionary<States, int> State { get; set; }

        // Can add either by Player Index or Name. If name, make sure it matches exactly 1 player
        public VSPlayer(int index)
        {
            this.Index = index;
            this.TSPlayer = TShock.Players[index];
            this.UserID = TShock.Players[index].UserID;
            this.VSCommands = new List<VSCommand>();
            this.Cooldowns = new Dictionary<string, int>();
            //this.State = new List<int>();
            this.State = new Dictionary<States, int>();
        }
        public VSPlayer(string name)
        {
            this.Index = TShock.Utils.FindPlayer(name)[0].Index;
            this.TSPlayer = TShock.Utils.FindPlayer(name)[0];
            this.UserID = TShock.Utils.FindPlayer(name)[0].UserID;
            this.VSCommands = new List<VSCommand>();
            this.Cooldowns = new Dictionary<string, int>();
            //this.State = new List<int>();
            this.State = new Dictionary<States, int>();
        }

        public void DamagePlayer(double damage)
        {
            if (damage < 0)
            {
                TSPlayer.Heal(VSSystem.Equalize(damage));
            }
            else
            {
                TSPlayer.DamagePlayer(VSSystem.Equalize(damage, TSPlayer));
            }
        }
        public void DamagePlayer(double damage, VSCommand cmd)
        {
            if (damage < 0)
            {
                TSPlayer.Heal(VSSystem.Equalize(damage));
            }
            else
            {
                if (cmd.Offensive)
                {
                    if (State.ContainsKey(States.BARRIER))
                    {
                        Message msg = new Message("Barrier has been disposed!", false, "", false, "", Color.Indigo);
                        RemoveState(States.BARRIER);
                        TSPlayer.SendMessage(msg.Text, msg.Color);
                        return;
                    }
                    else if (State.ContainsKey(States.SHIELD))
                    {
                        damage *= 0.8;
                    }
                    else if (State.ContainsKey(States.LOCUS))
                    {
                        damage *= 0.5;
                    }
                }
                TSPlayer.DamagePlayer(VSSystem.Equalize(damage, TSPlayer));
            }
        }
        public void DamagePlayer(double damage, VSCommand cmd, VSPlayer user)
        {
            if (damage < 0)
            {
                TSPlayer.Heal(VSSystem.Equalize(damage));
            }
            else
            {
                if (cmd.Offensive)
                {
                    if (State.ContainsKey(States.BARRIER))
                    {
                        Message msg = new Message("Barrier has been disposed!", false, "", false, "", Color.Indigo);
                        RemoveState(States.BARRIER);
                        TSPlayer.SendInfoMessage(msg.Text, msg.Color);
                        return;
                    }
                    if (State.ContainsKey(States.SHIELD))
                    {
                        damage *= 0.8;
                    }
                    if (State.ContainsKey(States.LOCUS))
                    {
                        damage *= 0.5;
                    }
                }
                if (user.State.ContainsKey(States.BOOST))
                {
                    damage *= 1.2;
                }
                if (user.State.ContainsKey(States.LOCUS))
                {
                    damage *= 1.5;
                }
                TSPlayer.DamagePlayer(VSSystem.Equalize(damage, TSPlayer));
            }
        }

        public void SetState(States state = States.NULL, int count = 0)
        {
            if (state.Equals(States.NULL))
            {
                State.Clear();
            }
            else
            {
                State.Add(state, count);
            }
        }
        public void RemoveState(States state)
        {
            if (State.Keys.Contains(state))
            {
                State.Remove(state);
            }
        }
        public string StateName(States state)
        {
            if (state.Equals(States.BOOST))
                return "Boost";
            else if (state.Equals(States.SHIELD))
                return "Shield";
            else if (state.Equals(States.BARRIER))
                return "Barrier";
            else if (state.Equals(States.LOCUS))
                return "Locus";
            else
                return "None";
        }
    }

    /// <summary>
    /// PvP Command constructor
    /// </summary>
    public class VSCommand
    {
        /// <summary>
        /// Gets or sets the command alias for parsing and syntax purposes.
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// Gets or sets the command's display name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the command's damage. Negative values heal the target instead.
        /// </summary>
        public double Damage { get; set; }
        /// <summary>
        /// Gets or sets the command's damage dealth to the user. Negative Values heal the user.
        /// </summary>
        public double DamageToSelf { get; set; }
        /// <summary>
        /// Gets or sets whenever the command is offensive or not. Non-offensive commands bypass shields and ignore damage reduction.
        /// </summary>
        public bool Offensive { get; set; }
        /// <summary>
        /// Gets or sets the command's description. Every message is counted as a new line.
        /// </summary>
        public List<Message> Description { get; set; }
        /// <summary>
        /// Gets or sets an additional effects to the command.
        /// </summary>
        public List<Effect> Effect { get; set; }
        /// <summary>
        /// Gets or Sets whenever the command can be used on its user. Default 0, 1 is self-use, 2 is cannot be used on self.
        /// </summary>
        public int UseSelf { get; set; }
        /// <summary>
        /// Gets or sets the command's cooldown. Will have no effect if config.Cooldowns is set to false.
        /// </summary>
        public int Cooldown { get; set; }
        /// <summary>
        /// Gets or sets the command's time left, set to Cooldown when a command is used, and drops every second.
        /// </summary>
        public int Counter { get; set; }
        /// <summary>
        /// Gets or sets a timer which can be used to give the command special delayed effects.
        /// </summary>
        public Timer Timer { get; set; }
        /// <summary>
        /// Gets or sets the Message to send to the command's user upon activation.
        /// </summary>
        public Message MsgSelf { get; set; }
        /// <summary>
        /// Gets or sets the Message to send to the command's target upon activation.
        /// </summary>
        public Message MsgPlayer { get; set; }
        /// <summary>
        /// Gets or sets the Message to broadcast upon command activation.
        /// </summary>
        public Message MsgAll { get; set; }
        /// <summary>
        /// Gets or sets the VSCommand permission. Should be set to mimic the ChatCommand permission.
        /// </summary>
        public string Permission { get; set; }
        /// <summary>
        /// Gets or sets whether the command should bypass pvp permissions
        /// </summary>
        public bool BypassPvp { get; set; }
        /// <summary>
        /// Gets or sets the user of the command.
        /// </summary>
        public virtual VSPlayer User { get; set; }
        /// <summary>
        /// Gets or sets the target of the command.
        /// </summary>
        public virtual TSPlayer Target { get; set; }

        // If name is given a value, alias and permission are automatically set. Still better to manually set those
        public VSCommand(string name = "")
        {
            this.Name = name;
            this.Alias = name.ToLower();
            this.Damage = 0;
            this.DamageToSelf = 0;
            this.Offensive = true;
            this.Description = new List<Message> { new Message(string.Format("Usage: /{0} <player>", Alias), Color.Yellow) };
            this.Effect = null;
            this.UseSelf = 0;
            this.Cooldown = 0;
            this.Counter = 0;
            this.Timer = new Timer(1);
            this.Timer.Elapsed += Timer_Elapsed;
            this.MsgSelf = null;
            this.MsgPlayer = null;
            this.MsgAll = null;
            this.Permission = name.ToLower();
            this.BypassPvp = false;
            this.User = null;
            this.Target = null;
        }

        public bool CanRun(TSPlayer ply)
        {
            if (ply.Group.HasPermission("vs.commands." + Permission))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Timers

        public void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timer.Stop();
        }

        public void Timer_Tickle(object sender, ElapsedEventArgs e)
        {
            Message msg = new Message("", Target, " is no longer being tickled.", null, "", new Color(243, 112, 234));
            TSPlayer.All.SendMessage(msg.Text, msg.Color);
            Timer.Stop();
        }

        public void Timer_FSight(object sender, ElapsedEventArgs e)
        {
            Message msg = new Message("You received ", User.TSPlayer, "'s Future Sight!", null, "", Color.MediumPurple);
            VSSystem.VSPlayers[Target.UserID].DamagePlayer(250, this, User);
            Target.SendMessage(msg.Text, msg.Color);
            Timer.Stop();
        }

        public void Timer_Doom(object sender, ElapsedEventArgs e)
        {
            Message msg = new Message("", Target, " received his fated Doom!", null, "", Color.Orchid);
            VSSystem.VSPlayers[Target.UserID].DamagePlayer(400, this, User);
            TSPlayer.All.SendMessage(msg.Text, msg.Color);
            Timer.Stop();
        }

        public void Timer_Wish(object sender, ElapsedEventArgs e)
        {
            Message msg = new Message("", Target, " received ", User.TSPlayer, "'s Wish!", Color.RoyalBlue);
            VSSystem.VSPlayers[Target.UserID].DamagePlayer(-200);
            TSPlayer.All.SendMessage(msg.Text, msg.Color);
            Timer.Stop();
        }

        public void Timer_HWish(object sender, ElapsedEventArgs e)
        {
            Message msg = new Message("", Target, " received ", User.TSPlayer, "'s Healing Wish!", Color.SeaGreen);
            VSSystem.VSPlayers[Target.UserID].DamagePlayer(-(double)Target.FirstMaxHP);
            Target.TPlayer.ManaEffect(Target.FirstMaxMP);
            TSPlayer.All.SendMessage(msg.Text, msg.Color);
            Timer.Stop();
        }

        public void Timer_Rake(object sender, ElapsedEventArgs e)
        {
            Message msg = new Message("Your bleeding has stopped.", null, "", null, "", Color.Salmon);
            User.TSPlayer.SendMessage(msg.Text, msg.Color);
            Timer.Stop();
        }
        #endregion
    }

    /// <summary>
    /// Handles messages for correct formatting
    /// </summary>
    public class Message
    {
        public string Text { get; set; }
        public Color Color { get; set; }
        public string InitMsg { get; set; }
        public string MidMsg { get; set; }
        public string EndMsg { get; set; }
        public bool UsePlr1 { get; set; }
        public bool UsePlr2 { get; set; }

        public Message(string text, Color color)
        {
            this.Text = text;
            this.Color = color;
        }
        [JsonConstructor]
        public Message(string initmsg, bool useplr1, string midmsg, bool useplr2, string endmsg, Color color)
        {
            this.InitMsg = initmsg;
            this.MidMsg = midmsg;
            this.EndMsg = endmsg;
            this.Color = color;
            this.UsePlr1 = useplr1;
            this.UsePlr2 = useplr2;
        }
        // Format is "Message" + User name (show?true/false) + "Message" + Target name (show?true/false) + "Message".
        // If inverted is true, User and Target name are shown in the opposite order (Target first, User last)
        public Message(string msg1, TSPlayer ply1, string msg2, TSPlayer ply2, string msg3, Color color, bool inverted = false)
        {
            this.Text = string.Format("[PvP] {0}{1}{2}{3}{4}{5}{6}", inverted ? "" : msg1, inverted ? "" : (ply1 == null ? "" : ply1.Name), msg2, ply2 == null ? "" : ply2.Name, msg3, inverted ? (ply1 == null ? "" : ply1.Name) : "", inverted ? msg1 : "");
            this.Color = color;
        }
    }

    /// <summary>
    /// VSPlayer states used to affect damage properties
    /// </summary>
    public enum States
    {
        NULL,
        BOOST,
        SHIELD,
        BARRIER,
        LOCUS,
        NONE = 100
    }
    /// <summary>
    /// Provides additional effects such as buffs and states
    /// </summary>
    public class Effect
    {
        public EffectTypes Type { get; set; }
        public int Parameter { get; set; }
        public int Parameter2 { get; set; }
        public int Who { get; set; }
        public States SetState { get; set; }

        public Effect(EffectTypes type, int parameter, int parameter2 = 0, int Who = 1, States setstate = States.NONE)
        {
            this.Type = type;
            this.Parameter = parameter;
            this.Parameter2 = parameter2;
            this.Who = Who;
            this.SetState = setstate;
        }

        // Event for the effect. Parameter function vary based on the type. Who is 0 (User) and 1 (target)
        public static void Event(VSPlayer User, TSPlayer Target, EffectTypes type, int Parameter, int Parameter2 = 0,
            int Who = 1, States setstate = States.NONE)
        {
            if (type.Equals(EffectTypes.BUFF))
            {
                try
                {
                    if (Who == 0)
                        User.TSPlayer.SetBuff(Parameter, Parameter2 * 60, false);
                    else
                        Target.SetBuff(Parameter, Parameter2 * 60, false);
                }
                catch (Exception)
                {
                    Log.ConsoleError("[VSSystem] A command has returned an error at Effect parameter: Invalid buff ID");
                }
            }
            else if (type.Equals(EffectTypes.HEALSELF))
            {
                if (Who == 0)
                    User.TSPlayer.Heal(Parameter);
                else
                    Target.Heal(Parameter);
            }
            else if (type.Equals(EffectTypes.TICKLE))
            {
                if (Who == 0)
                {
                    if (VSSystem.ItemCheck(TShock.Utils.GetItemByName("Hand Warmer")[0].netID, User.TSPlayer))
                    {
                        User.TSPlayer.DamagePlayer(10000);
                        User.TSPlayer.TPlayer.KillMe(10000, 0, true, " was tickled to death for having Hand Warmer.");
                    }
                    else
                        User.TSPlayer.SetBuff(47, Parameter * 60, false);
                }
                else
                {
                    if (VSSystem.ItemCheck(TShock.Utils.GetItemByName("Hand Warmer")[0].netID, Target))
                    {
                        Target.DamagePlayer(10000);
                        Target.TPlayer.KillMe(10000, 0, true, " was tickled to death for having Hand Warmer.");
                    }
                    else
                        Target.SetBuff(47, Parameter * 60, false);
                }
            }
            else if (type.Equals(EffectTypes.STATE) && setstate != States.NONE)
            {
                if (Who == 0)
                    User.SetState(setstate, Parameter);
                else
                    VSSystem.VSPlayers[Target.UserID].SetState(setstate, Parameter);
            }
        }
    }
    /// <summary>
    /// Enumeration for effect types
    /// </summary>
    public enum EffectTypes
    {
        BUFF,
        HEALSELF,
        TICKLE,
        STATE
    }
}
