# the-godfather

Just another Discord bot. Written using DSharpPlus.

[Project website](https://ivan-ristovic.github.io/the-godfather/)

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
* ``boolean`` : ``true`` or ``false`` (can be converted from ``yes`` or ``no`` in various forms, see: [AugmentedBoolConverter](TheGodfather/Extensions/AugmentedBoolConverter.cs))
* ``user`` : Discord user, given by ``@mention``, ``Username`` or UID (User ID)
* ``channel`` : Discord channel, given by ``name``, ``#name`` or CID (Channel ID)
* ``role`` : An existing role, given with ``@mentionrole`` or ``Role name``
* ``emoji`` : Emoji, either Unicode or Discord representation (using ``:``)
* ``id`` : ID of Discord entity (could be a message, user, channel etc.). Only seen when the ``Developer appearance`` option is enabled in the Discord client options.

For example, in ``!say Some text``, the command is ``say`` and argument is ``Some text``. The type of this argument is ``string...``.
Commands receive **only** arguments of the specified type, so for example if command expects an ``int``, passing ``text`` to it will cause an error.

## Command overload explanation:

Each command can be invoked in various ways, each of them being called an **overload**. For example, the ``!kick`` command takes two arguments, ``user`` and ``string...`` which correspond to user and reason for kick, respectively. 
There are two overloads for the ``!kick`` command. One takes ``user`` as the first argument and ``string...`` as the second one. The other overload takes a ``string`` and a ``user``. The reason why this is done is so that it doesn't matter if you provide the user first or the reason first, as long as you provide the user. So both of these calling attempts will work:

``!kick @User My reason for doing this is because I am a terrible person.``
``!kick "My reason for doing this is because I am a terrible person." @User``

Note that in the second overload the reason is a ``string`` and not ``string...`` like it is in the first overload. This is because ``string...`` captures the remaining text of the command invocation message and looks at it like plain text, which in this case would capture the ``user`` as well.

~~``!kick My reason for doing this is because I am a terrible person @User``~~ Invalid incovation

The reason why it fails is because it will look the entire ``My reason for doing this is because I am a terrible person @User`` as a reason for kick and would miss the ``user`` argument.

Furthermore, the reason in the first overload is marked as ``(optional)`` in the documentation. When the argument is marked as optional, you can omit it.

``!kick @User``

In the second overload though, the reason is mandatory since it preceeds ``user``. That doesn't mean that you HAVE to write a reason, for instance:

``!kick "" @user``

## Automatically generated command list

[Auto-generated command list.](TheGodfather/Modules/README.md)