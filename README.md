# the-godfather

[![Build Status](https://ci.appveyor.com/api/projects/status/axn087nlq6ha783x?svg=true)](https://ci.appveyor.com/project/ivan-ristovic/the-godfather)
[![Issues](https://img.shields.io/github/issues/ivan-ristovic/the-godfather.svg)](https://github.com/ivan-ristovic/the-godfather/issues)
[![Discord Server](https://img.shields.io/discord/794671727291531274.svg?label=discord)](https://discord.gg/z7KZGQQxRz)
[![Stable release](https://img.shields.io/github/release/ivan-ristovic/the-godfather.svg?label=stable)](https://github.com/ivan-ristovic/the-godfather/releases)
[![Latest release](https://img.shields.io/github/tag-pre/ivan-ristovic/the-godfather.svg?label=latest)](https://github.com/ivan-ristovic/the-godfather/releases)

Just another general-purpose Discord bot developed with the goal to remove all other bots from the guild and create one that will do everything as efficiently as possible but keeping the simplicity in mind.

> *Discord bots have grown very rapidly over the past few years and due to that growth it usually becomes hard to use them because of the unintuitive command system or due to performance issues. I have had a scenario where we had ten bots in the guild, because every bot did a unique job. Managing many bots and permissions for those bots quickly became overwhelming. Apart from that, only a handful of bots provided a customizable protection system against common destructive actions on Discord, yet it was still not enough - either the performance was poor due to it being a public bot instance, or the system was not customizable enough - that is if the system worked well to begin with. So, I have decided to create TheGodfather - one bot that will oversee and be in charge of everything.*
> 
> *TheGodfather became a side project for me, and it developed quite quickly. I have always intended it to be a private bot, however I realised over time that there are surely people like myself frustrated of public bot instances and dozens of narrow-purpose bots with web-based management interfaces. So, after a long time, even though this project was open sourced from the start, I decided to "open" the bot to the public - make it easier to setup and use for people who do not have programming background.*
>
> *TheGodfather is powered by the community that uses it - I do not have any financial gain from it. For that reason, the bot is meant to be self-hosted, but that might change in future if there are enough contributions from the community.*

Features:
  - Categorized and intuitive command group system.
  - Complete administration of the guild, channels, users, emojis, roles, integrations etc. via categorized commands.
  - Automatic guild administration: antispam, antiraid, ratelimit, instant join-leave protection, forbidden names and many more! All of them are customizable to suit your needs.
  - Eases your pain with auto-assignable roles and self-assignable roles.
  - Real-time guild backup with exemptions of specified channels.
  - Activity logs for any changes made to the guild, channels, members etc. It is possible to exempt entities (channels, users) that you do not want to be logged.
  - Message filtering either by raw string matching or regular expression matching, automatic gore websites filter, IP logging websites filter, invite filter, etc.
  - Customizable textual or emoji reactions which can be triggered by raw text or regular expression matching.
  - Customizable guild content: memes (along with meme generator), ranks, birthdays, currency, currency items and many more!
  - Searches of online services: YouTube, Wikipedia, Imgur, reddit, Steam, IMDb, OpenWeather etc.
  - RSS feed subscriptions (includes YouTube, reddit and many more) - automatic notifications when new content is released.
  - Many games to pass your time: Quiz, Tic-Tac-Toe, Hangman, Connect4, Othello, Caro and many more! 
  - Ranking/Experience system with guild-specific rank names.
  - Currency management and gambling games: Poker, BlackJack, Slots etc.
  - Interactive text or reaction polls.
  - Reminders in DM and guild channels.
  - Music playback and queue management commands supporting YouTube, Twitch, Soundcloud and many more.
  - Starboard with customizable star emoji and reaction threshold.
  - Chicken training, battles and wars!
  - Guild-specific culture setting (e.g. language, timezone). Currently supported languages/cultures: `en-UK` (default), `en-US`.


Written in C# using [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus).

---

TheGodfather listens for commands inside guilds and in DM. However, some commands cannot be invoked in DM and vice versa.

The commands are invoked by sending a message starting with a bot "prefix" or by mentioning the bot at the start of the message. The default prefix for the bot is ``!``, however you can change it using ``prefix`` command (guild specific). 

For example, valid command calls are: 
```
!ping
@TheGodfather ping
```


## Command list

Command list is available at the [Documentation](Documentation/README.md) directory.

**Note:** It is advised to read the explanation below in order to use TheGodfather in his full potential.


## Command naming system

Commands are divided into command groups for easier navigation and due to large amount of commands. 

For example, command group ``user`` contains commands which are used for administrative tasks on Discord users. Some subcommands of this group are ``kick`` , ``ban`` etc. For example, in order to call the ``kick`` command, one should always provide the "parent" group name first and then the actual command name, in this case: 
```
!user kick @Someone
```

The same applies if the command has multiple parent groups, such as:
```
!config antispam exempt @Someone
```

Aliases are synonyms for a command. Aliases are usually shorter than regular names and are meant for faster invocation of the commands. Some people like it short and some people like it descriptive - with this, everyone is covered. Here is an example, for the commands above:
```
!u k @Someone
!cfg as ex @Someone
```


## Command arguments

Almost all commands require additional information apart from their names, that information is from here on called **command arguments**.

For example, the ``kick`` command requires a Discord member as an argument, so the bot can know who to kick from the guild. Command arguments are strictly typed - meaning that you cannot pass anything other than a valid Discord member to a ``kick`` command.

A command argument can have exactly one of the following types (only the the most common types have been listed): 
  - ``int`` : A single integer in range [-2147483648, 2147483647]. Valid examples: ``25`` , ``-64`` , ``123456789``.
  - ``long`` : A single long integer with greater range (approx. 18 digits). Valid examples: ``25`` , ``-64`` , ``123456789123``.
  - ``double`` : A floating point number, can also be an integer. Valid examples: ``5.64`` , ``-3.2`` , ``5`` , ``123456.5646``.
  - ``string`` : Unicode character sequence without spaces. If you wish to include spaces, then surround the sequence with quotation marks (`"`). Valid examples: ``testtest`` , ``T3S7``, ``"quotes"`` , ``"I need quotes for spaces!"``
  - ``string...`` : Unicode characrer sequence which includes spaces. Since this is a very general argument type, it will always come last in argument queue. Valid examples: ``This is a text so I do not need quotes``.
  - ``boolean`` : A truth value, either ``true`` or ``false`` (can also be converted from ``yes`` or ``no`` in various forms, see: [CustomBoolConverter](TheGodfather/Common/Converters/CustomBoolConverter.cs)). Valid examples: ``true`` , ``yes`` , ``no`` , ``0``.
  - ``user`` : Discord user - given by mention, username or UID (User ID). Valid examples: ``@Someone`` , ``Someone`` , ``123456789123456``.
  - ``member`` : Discord guild member - given by mention, username or UID (User ID). Valid examples: ``@Someone`` , ``Someone`` , ``123456789123456``.
  - ``channel`` : Discord channel - given by mention, channel name or CID (Channel ID). Valid examples: ``#channel`` , ``MyChannel`` , ``123456789123456``.
  - ``role`` : Discord role - given by mention, role name or RID (Role ID). Valid examples: ``@Admins`` , ``Admins`` , ``123456789123456``.
  - ``emoji`` : Discord emoji, either in Unicode or Discord representation (using ``:``). Valid examples: ``😂`` , ``:joy:``.
  - ``message`` : Discord message ID. Valid examples: ``123456789123456``.
  - ``Uri`` : A sequence of characters representing a URL. The protocol for most commands must be either ``HTTP`` or ``HTTPS``. Valid examples: ``http://google.com``.
  - ``TimeSpan`` : A time span in form ``DDdHHhMMmSSs`` Valid examples: ``3d5m30s``, ``1d``, ``10s`` etc. 


**Note:** Discord entity IDs can only be seen in the Discord client by enabling the ``Developer appearance`` option in Discord settings.

Arguments can be marked as ``(optional)`` in the documentation. If this is the case, you can omit that argument in your command call.

For example, the ``user kick`` command described above also accepts a ``string...`` argument after the ``user`` , which corresponds to a reason for the action. However, since it is marked as optional, both of the following invocation attempts will succeed:
```
!user kick @Someone
!user kick @Someone I have kicked him because I can!
```


## Command overloads

Each command can be invoked in various ways, each of those ways being called an **overload** in the documentation. 

For example, let's consider the ``bank transfer`` command. The logic of this command is to transfer currency from your account to another user's account. 
One way to use it is to provide a ``user`` to pass the currency to and an ``int`` which corresponds to the amount of currency to transfer. 
The ordering of these arguments can sometimes be hard to remember, especially if there are more than two arguments. The purpose of an overload is to give alternate ways of invoking the same command.
In this example, another way to use the ``bank transfer`` command is to pass the amount first and the user second.
This way, the ordering of the arguments does not matter and therefore does not need to be remembered - both invocation attempts will succeed. This works as long as there is a clear distinction between the argument types (e.g. if a command has two arguments of the same type, overloading is not possible).

**Note:** ``string...`` argument always comes last in queue because it captures raw text until the end of the message. Therefore it cannot come before any other argument.

**Note:** It is always preferred to surround arguments of type ``string`` with quotes, although not nececary. This prevents the accidental misinterpretation of the second word in a given string as a different argument. Also note that, in case of ``string...`` type, the quotes will also be captured.

