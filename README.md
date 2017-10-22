# the-godfather
Just another Discord bot. Written using DSharpPlus.

[Project website](https://ivan-ristovic.github.io/the-godfather/)

---

# Command list

Commands are separated into groups. For example, ``!user`` is a group of commands which allow manipulation of users and it has subcommands ``kick`` , ``ban`` etc. So if you want to call the ``kick`` command, you do it like ``!user kick ...``.

The default prefix for the bot is ``!``, however you can change that using ``!prefix`` command. Also you can trigger commands by mentioning the bot. For example:
``!greet`` is the same as ``@TheGodfather greet``


#### Type reference:
* ``int`` : Integer (positive or negative number)
* ``double`` : Decimal point number (positive or negative), can also be integer
* ``string`` : Word consisting of Unicode characters WITHOUT spaces. If you want to include spaces, then surround it with ``"``
* ``text`` : Some Unicode text, can include spaces
* ``user`` : Discord user, given by ``@mention``
* ``channel`` : Discord channel, given by ``name`` or ``#name``



| Command group (with synonyms) | Command name (with synonyms) | Required Permissions | Command arguments | Command Description | Example of use |
|---|---|---|---|---|---|
|   | ``embed`` | Attach files | ``[string] URL`` | Embeds image given as URL and sends a embed frame. | ``!embed https://img.memecdn.com/feelium_o_1007518.jpg``  |
|   | ``greet``<br>``hello``<br>``hi``<br>``halo``<br>``hey``<br>``howdy``<br>``sup`` |  |  | Greets a user and starts a conversation | ``!greet`` |
|   | ``invite``<br>``getinvite`` | Create instant invite |  | Get an instant invite link for the current channel. | ``!invite`` |
|   | ``leave`` | Kick members |   | Makes Godfather leave the server. | ``!leave`` |
|   | ``leet`` |   | ``[text] Text`` | Wr1t3s m3ss@g3 1n 1337sp34k. | ``!leet This is so cool`` |
|   | ``ping`` |   |   | Ping the bot. | ``!ping`` |
|   | ``prefix``<br>``setprefix`` | Administrator | ``(optional) [string] New prefix`` | If invoked without arguments, gives current prefix for this channel, otherwise sets the prefix to ``New prefix``. If for example ``New prefix`` is ``;``, all commands in that channel from that point must be invoked using ``;``, for example ``;greet``. | ``!prefix`` <br> ``!prefix .`` |
|   | ``remind`` |  | ``[int] Time to wait before repeat (in seconds)``<br><br>``[text] What to repeat`` | Repeat given text after given time. | ``!repeat 3600 I was told to remind you to do something`` |
|   | ``report`` |   | ``[text] Report message`` | Send message to owner (hopefully about a bug, I can see it being abused) | ``!report Your bot sucks!`` |
|  | ``say``  |   | ``[text] What to say`` | Make Godfather say something! | ``!say Luke, I am your father!`` |
|   | ``zugify`` |   | ``[text] Text`` | Requested by Zugi. It is so stupid it isn't worth describing... | ``!zugify Some text`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``createcategory``<br>``createc``<br>``+c``<br>``makec``<br>``newc``<br>``addc`` | Manage Channels | ``[text] Name`` | Create new channel category. | ``!channel createcategory My Category`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``createtext``<br>``createt``<br>``+``<br>``+t``<br>``maket``<br>``newt``<br>``addt`` | Manage Channels | ``[string] Name`` | Create new text channel. *Note: Discord does not allow spaces in text channel name.* | ``!channel createtext spam`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``createvoice``<br>``createv``<br>``+v``<br>``makev``<br>``newv``<br>``addv`` | Manage Channels | ``[text] Name`` | Create new voice channel. | ``!channel createvoice My Voice Channel`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``delete``<br>``-``<br>``d``<br>``del``<br>``remove`` | Manage Channels | ``(optional) [channel] Channel/Category`` | Delete channel or category. If channel is not given as argument, deletes the current channel. | ``!channel delete``<br>``!channel delete #afkchannel`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``info``<br>``i``<br>``information`` | Manage Channels | ``(optional) [channel] Channel/Category`` | Get channel information. | ``!channel info``<br>``!channel info #afkchannel`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``rename``<br>``r``<br>``name``<br>``setname`` | Manage Channels | ``[string] Name``<br><br>``(optional) [channel] Channel/Category`` | Rename channel. If channel is not given as argument, renames the current channel. | ``!channel rename New Name``<br>``!channel rename "New Name" "Some Channel Name"`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``settopic``<br>``t``<br>``sett``<br>``topic`` | Manage Channels | ``[string] Topic``<br><br>``(optional) [channel] Channel/Category`` | Set a new channel topic. If channel is not given as argument, modifies the current channel. | ``!channel settopic Welcome to my channel!``<br>``!channel settopic "My topic" "Some Channel Name"`` |


**(Command list is incomplete)**

---
