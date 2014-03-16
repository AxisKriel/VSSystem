TShock - Versus System
==============
Customizable PvP Commands, Damage Equalizer & PvP Check!

What does it do?
==============
Versus System (VSSystem) is a platform for developing special commands known as PvP Commands. By default (but configurable) those commands can only be used against players who've got their pvp status on. These commands can be given different damage, effects, messages upon activation (with custom colors!) and special PvP Command-only states such as a Barrier blocking the next command or a Boost boosting the commands' effectiveness. Cooldowns may be set to prevent command spamming, and a special damage Equalizer can be turned on to balance off the damage dealt using the target's max hp. A config file can be used to add new user-made commands to the mix. Eventually, the plugin will have a special program to alter the config with more ease, since the command format is quite complicated.

Permissions:
==============
vs.help - Allows the use of /vshelp

vs.list - Allows the use of /vslist

vs.reload - Allows the use of /vsreload

vs.commands.cmd - Replacing cmd with any PvP Command alias will allow the use of it. vs.commands.* will allow every command


Commands:
==============
**TSHOCK:**
/help <command> - Every command in this plugin has its own /help - including user-created (a simple usage guide in this case)

**VSSystem:**
/vshelp **[vscmd]** - Showns target PvP Command's description. Such includes Name, Type (Damage, Heal or Other) and Usage by default (if a new Description is given, you must place in the usage manually)
 
/vslist - Lists all currently available PvP Commands to the user, based off permissions.

/vsreload - Reloads VSConfig.json. It is recomended that you restart when adding new commands though. This should be used to reload configurations such as PvPOnly, Equalizer or edit already-existing command bits

Every PvP Command has its own alias. To execute them, you need to have the vs.commands.alias permission; you can then use /alias (where alias is the specific command alias). List of currently default commands:  strike, vsheal, stab, dedge, tickle, fsight, doom, chill, wish, hwish, drain, rake, boost, shield, barrier, locus

Changelog:
==============
Currently not writting a changelog. You can still use a commit-based changelog by checking the commits tab.

To Do:
==============
 - Make a config editor to add commands (thus making it simpler - a single command requires ~71 lines)
 - Improve stuff overall
 
 If you have any suggestion, make sure to comment on the Pulse section / plugin thread at TShock!