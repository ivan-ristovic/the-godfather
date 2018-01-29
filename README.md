# the-godfather

Just another Discord bot. Written using DSharpPlus.

[Project website](https://ivan-ristovic.github.io/the-godfather/)

---

# Command list

[Auto-generated command list.](TheGodfather/Modules/README.md)

Commands are separated into groups. For example, ``!user`` is a group of commands which allow manipulation of users and it has subcommands ``kick`` , ``ban`` etc. So, calling the ``kick`` command is done by typing ``!user kick ...``.

The default prefix for the bot is ``!``, however you can change that using ``!prefix`` command (affects just your guild). Also you can trigger commands by mentioning the bot. For example:
``!greet`` is the same as ``@TheGodfather greet``.


### Argument type explanation:
Each command receives arguments. For example, in ``!say Some text``, the command is ``say`` and argument is ``Some text``. The type of this argument is ``text``.
Commands receive **only** arguments of the specified type, so for example if command expects an ``int``, passing ``text`` to it will cause an error.

Commands use the following types:
* ``int`` : Integer (positive or negative number)
* ``double`` : Floating point number (positive or negative), can also be integer (for example 5.64)
* ``string`` : Word consisting of Unicode characters WITHOUT spaces. If you want to include spaces, then surround it with ``"``
* ``bool`` : ``true`` or ``false`` (can be converted from ``yes`` or ``no`` in various forms)
* ``text`` : Some Unicode text, can include spaces. Can be surrounded with ``"``
* ``user`` : Discord user, given by ``@mention``, ``Username`` or UID (User ID)
* ``channel`` : Discord channel, given by ``name``, ``#name`` or CID (Channel ID)
* ``role`` : An existing role, given with ``@mentionrole`` or ``Role name``
* ``emoji`` : Emoji, either Unicode or Discord representation (using ``:``)
* ``id`` : ID of Discord entity (could be a message, user, channel etc.). Only seen when the ``Developer appearance`` is enabled in the Discord client options.