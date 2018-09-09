# the-godfather

[![Build Status](https://ci.appveyor.com/api/projects/status/axn087nlq6ha783x?svg=true)](https://ci.appveyor.com/project/ivan-ristovic/the-godfather)
[![Discord Server](https://img.shields.io/discord/379378609942560770.svg?label=discord)](https://discord.me/worldmafia)

Just another general-purpose Discord bot. Developed with the goal to remove all other bots from the guild and create one that will do everything.

Features:
- Full administration of the guild, channels, users, emoji etc.
- Protection commands: antispam, antiraid and many more!
- Offers activity logging - will log any changes to guild / channels / members etc. It is possible to exempt entities (channels, users) that you do not want to be logged
- Message filtering: user-defined filters either by raw match or regex match, gorefilter, IP logging websites filter, invite filter, etc.
- Customizable textual or emoji reactions for each guild which can be triggered by raw text or regex matching
- Customizable guild content: memes, ranks, birthdays, currency items and many more!
- Searches of online services: YouTube, Imgur, reddit, Steam, IMDb, OpenWeather etc.
- RSS feed subscriptions (includes YouTube, reddit and much more) and automatic notifications on the new content release.
- Many games to pass your time: Quizzes, Tic-Tac-Toe, Connect4, Othello, Caro etc. 
- Currency commands and games: Poker, BlackJack, Slots etc.
- Eases your pain with auto-assigned roles and self-assignable roles.
- Interactive polls and reminders
- SWAT4 server queries and player database

Upcoming features:
- Music playback (currently in beta)
- Interactive NLP mode (currently in alpha) - explain what you want and it will be done!


Written in C# using [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus).

---

TheGodfather only listens for commands inside the guilds, and **nowhere** else.

The commands are invoked by sending a message starting with a "prefix" or by mentioning the bot at the start of the message.

The default prefix for the bot is ``!``, however you can change it using ``prefix`` command (affects just the guild in which it is invoked). 

For example, valid command calls are: 
```
!ping
@TheGodfather ping
```


## Command list

Command list is available at the [Documentation](Documentation/README.md) directory.

**Note:** It is advised to read the explanation below in order to use TheGodfather in his full potential.


## Command groups

Commands are divided into command groups for easier navigation and partially due to large amount of commands. 

For example, command group ``user`` contains commands which are used for administrative tasks on Discord users. Some subcommands of this group are ``kick`` , ``ban`` etc. 
In order to call the ``kick`` command for example, one should always provide the command group name and then the actual command name, in this case: 
```
!user kick @Someone
```


## Command arguments

Some commands require additional information, from here on called **command arguments**.

For example, the ``kick`` command requires a user as an argument, so the bot can know who to kick from the guild.

Commands that require arguments also specify the type of the arguments that they accept. 

For example, you need to pass a valid Discord user to ``kick`` command (not some random gibberish).

Arguments can be exactly one of the following types: 
* ``int`` : Integer (a single whole number) in range [-2147483648, 2147483647]. Valid examples: ``25`` , ``-64`` , ``123456789``.
* ``long`` : Integer (a single whole number) with greater range (approx. 18 digits). Valid examples: ``25`` , ``-64`` , ``123456789123``.
* ``double`` : Floating point number, can also be an integer. Valid examples: ``5.64`` , ``-3.2`` , ``5`` , ``123456.5646``.
* ``string`` : Unicode characrer sequence without spaces. If you want to include spaces, then surround the sequence with quotes. Valid examples: ``testtest`` , ``T3S7`` , ``"I need quotes for spaces!"``
* ``string...`` : Unicode characrer sequence, can include spaces. Since this is a very general argument type, it will always come last in argument queue. Valid examples: ``This is a text so I do not need quotes``.
* ``boolean`` : A truth value, either ``true`` or ``false`` (can also be converted from ``yes`` or ``no`` in various forms, see: [CustomBoolConverter](TheGodfather/Common/Converters/CustomBoolConverter.cs)). Valid examples: ``true`` , ``yes`` , ``no`` , ``0``.
* ``user`` : Discord user - given by mention, username or UID (User ID). Valid examples: ``@Someone`` , ``Someone`` , ``123456789123456``.
* ``message`` : Discord message ID. Valid examples: ``123456789123456``.
* ``channel`` : Discord channel - given by mention, channel name or CID (Channel ID). Valid examples: ``#channel`` , ``MyChannel`` , ``123456789123456``.
* ``role`` : Discord role - given by mention, role name or RID (Role ID). Valid examples: ``@Admins`` , ``Admins`` , ``123456789123456``.
* ``emoji`` : Discord emoji, either in Unicode or Discord representation (using ``:``). Valid examples: ``😂`` , ``:joy:``.
* ``Uri`` : A sequence of characters representing a URL. The protocol for most commands must be either ``HTTP`` or ``HTTPS``. Valid examples: ``http://google.com``.
* ``id`` : ID of a Discord entity (could be a message, user, channel, role etc.).
* ``color`` : A hexadecimal or RGB color representation. Valid examples: ``FF0000`` , ``(255, 0, 0)``.
* ``time span`` : A time span in form ``DDd HHh MMm SSs`` Valid examples: ``3d 5m 30s`` etc. 
* ``IPAddress`` : An IPv4 address. Valid examples: ``123.123.123.123`` etc. 
* ``CustomIPFormat`` : Combined IPv4 range and optional endpoint port. Valid examples: ``123.123.123.123:12345`` , ``123.123.*`` etc. 
* ``PunishmentActionType`` : One of the following values: Kick, Ban, Mute, TemporaryBan, TemporaryMute. 


**Note:** Discord entity IDs can only be seen in the Discord client by enabling the ``Developer appearance`` option in Discord settings.

Arguments can be marked as ``(optional)`` in the documentation. If this is the case, you can omit that argument in your command call.

For example, the aforementioned ``kick`` command also accepts a ``string...`` argument after the ``user`` , which corresponds to a reason for the kick. However, since it is marked as optional, both of the following invocation attempts will succeed:
```
!user kick @Someone
!user kick @Someone I have kicked him because I can!
```


## Command aliases

Aliases are the synonyms for a command.
Aliases are usually shorter than regular names and are meant for faster invocation of the commands. Some people like it short and some people like it descriptive.

For example, the ``user`` command group has an alias ``u`` and the ``kick`` command has an alias ``k``. So all of the following command calls are actually the one and the same:
```
!user kick @Someone
!user k @Someone
!u k @Someone
```


## Command overloads

Each command can be invoked in various ways, each of those ways being called an **overload** in the documentation. 

For example, let's consider the ``bank transfer`` command. The logic of this command is to transfer currency from your account to another user's account. 
One way to use it is to provide a ``user`` to pass the currency to and an ``int`` which corresponds to the amount of credits to transfer. 
The ordering of these arguments can sometimes be hard to remember. This is where overloads come in. The purpose of an overload is to give alternate ways of invoking the same command.
In this example, another way to use the ``bank transfer`` command is to pass the amount first and the user second.
This way, the ordering of the arguments does not matter and therefore does not need to be remembered.

**Note:** ``string...`` argument always comes last in queue because it captures raw text until the end of the message.

**Note:** It is always preferred to surround arguments of type ``string`` with quotes. 
This eliminates the misinterpretation in case two strings are required as arguments (if quotes are not used, the space will be seen as a separator and the passed text will be interpreted as multiple strings, which is not usually a behaviour that the user expects). However, note that if the argument type is ``string...`` , the quotes will be captured as well.