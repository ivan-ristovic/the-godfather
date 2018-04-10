# the-godfather

Just another general-purpose Discord bot. Written in C# using [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus).

---

## Command groups explanation:

Commands are separated into groups. For example, ``!user`` is a group of commands which allow manipulation of users and it has subcommands ``kick`` , ``ban`` etc. So, calling the ``kick`` command is done by typing ``!user kick ...``.

The default prefix for the bot is ``!``, however you can change that using ``!prefix`` command (affects just your guild). Also you can trigger commands by mentioning the bot. For example:
``!greet`` is the same as ``@TheGodfather greet``.


## Argument type explanation:

Each command receives arguments which are some of the following types: 
* ``int`` : Integer (positive or negative number)
* ``double`` : Floating point number (positive or negative), can also be integer (for example 5.64)
* ``string`` : Word consisting of Unicode characters WITHOUT spaces. If you want to include spaces, then surround it with ``"``
* ``string...`` : Some Unicode text, can include spaces. Can be surrounded with ``"``
* ``boolean`` : ``true`` or ``false`` (can be converted from ``yes`` or ``no`` in various forms, see: [CustomBoolConverter](TheGodfather/Extensions/Converters/CustomBoolConverter.cs))
* ``user`` : Discord user, given by ``@mention``, ``Username`` or UID (User ID)
* ``channel`` : Discord channel, given by ``name``, ``#name`` or CID (Channel ID)
* ``role`` : An existing role, given with ``@mentionrole`` or ``Role name``
* ``emoji`` : Emoji, either Unicode or Discord representation (using ``:``)
* ``id`` : ID of Discord entity (could be a message, user, channel etc.). Only seen when the ``Developer appearance`` option is enabled in the Discord client options.
* ``color`` : A hex or rgb color representation, for example: ``FF0000`` or ``(255, 0, 0)`` for red
* ``time span`` : A time span, for example: ``3d 5m 30s`` etc. 

For example, in ``!say Some text``, the command is ``say`` and argument is ``Some text``. The type of this argument is ``string...``.
Commands receive **only** arguments of the specified type, so for example if command expects an ``int``, passing ``text`` to it will cause an error.

## Aliases explanation

Aliases are the synonyms for a command. For example, the ``user`` command group has an alias ``u``. This means that if you wish to call a command from that group, let's say ``!user kick``, you can also call it by using an alias: ``!u kick``.

## Command overloads explanation:

Each command can be invoked in various ways, each of those ways being called an **overload** in the documentation. 
For example, one way to call ``!user kick`` is by giving a ``user`` and a ``string...`` which correspond to user and reason for kick, respectively. 
Another way to call ``!user kick`` is by giving a ``string`` and a ``user``. The reason why they both exist is so that it doesn't matter if you provide the user first or the reason first, as long as you provide the user (since it is nececary). So both of these calling attempts will work:

```
!user kick @User My reason for doing this is because I am a terrible person.
!user kick "My reason for doing this is because I am a terrible person." @User
```

Note that in the second overload the reason is a ``string`` and not ``string...`` unlike in the first one. This is because ``string...`` captures the remaining text of the command invocation message and looks at it like plain text, which in this case would capture the ``user`` as well. 
So, the following command invocation attempt will cause an error:

~~```!user kick My reason for doing this is because I am a terrible person @User```~~

Arguments can be marked as ``(optional)`` in the documentation. When the argument is marked as optional, you can omit it.
For example, the reason for kick mentioned above, is optional, so you can do simply:

```!user kick @User```

In the second overload though, the reason is mandatory since it preceeds ``user`` which is also mandatory.

```!user kick "I must type a reason here... Also surround it with quotes because it's a string and not text." @user```

## Command list

Command list is available at the [Documentation](Documentation/README.md) directory.