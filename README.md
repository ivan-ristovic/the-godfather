# the-godfather
Just another Discord bot. Written using DSharpPlus.

[Project website](https://ivan-ristovic.github.io/the-godfather/)

---

# Command list

Commands are separated into groups. For example, ``!user`` is a group of commands which allow manipulation of users and it has subcommands ``kick`` , ``ban`` etc. So if you want to call the ``kick`` command, you do it like ``!user kick ...``.

The default prefix for the bot is ``!``, however you can change that using ``!prefix`` command. Also you can trigger commands by mentioning the bot. For example:
``!greet`` is the same as ``@TheGodfather greet``


### Argument type explanation:
Each command receives arguments. For example, in ``!say Some text``, the command is ``say`` and argument is ``Some text``. The type of this argument is ``text``.
Commands receive **only** arguments of the specified type, so for example if command expects an ``int``, passing ``text`` to it will cause an error.

Commands use the following types:
* ``int`` : Integer (positive or negative number)
* ``double`` : Decimal point number (positive or negative), can also be integer
* ``string`` : Word consisting of Unicode characters WITHOUT spaces. If you want to include spaces, then surround it with ``"``
* ``text`` : Some Unicode text, can include spaces
* ``user`` : Discord user, given by ``@mention``
* ``channel`` : Discord channel, given by ``name`` or ``#name``
* ``emoji`` : Emoji, either Unicode or Discord representation


### Command table

*Note: Required permissions are permissions required for both bot and user to run the command (if not specified otherwise in table)*
<br><br>

| Command group (with synonyms) | Command name (with synonyms) | Required Permissions | Command arguments | Command Description | Example of use |
|---|---|---|---|---|---|
|   | ``embed`` | Attach files (user) | ``[string] URL`` | Embeds image given as URL and sends a embed frame. | ``!embed https://img.memecdn.com/feelium_o_1007518.jpg``  |
|   | ``greet``<br>``hello``<br>``hi``<br>``halo``<br>``hey``<br>``howdy``<br>``sup`` |  |  | Greets a user and starts a conversation | ``!greet`` |
|   | ``invite``<br>``getinvite`` | Create instant invite |  | Get an instant invite link for the current channel. | ``!invite`` |
|   | ``leave`` | Kick members (user) |   | Makes Godfather leave the server. | ``!leave`` |
|   | ``leet`` |   | ``[text] Text`` | Wr1t3s m3ss@g3 1n 1337sp34k. | ``!leet This is so cool`` |
|   | ``ping`` |   |   | Ping the bot. | ``!ping`` |
|   | ``prefix``<br>``setprefix`` | Administrator (user) | ``(optional) [string] New prefix`` | If invoked without arguments, gives current prefix for this channel, otherwise sets the prefix to ``New prefix``. If for example ``New prefix`` is ``;``, all commands in that channel from that point must be invoked using ``;``, for example ``;greet``. | ``!prefix``<br><br>``!prefix .`` |
|   | ``remind`` |  | ``[int] Time to wait before repeat (in seconds)``<br><br>``[text] What to repeat`` | Repeat given text after given time. | ``!repeat 3600 I was told to remind you to do something`` |
|   | ``report`` |   | ``[text] Report message`` | Send message to owner (hopefully about a bug, I can see it being abused) | ``!report Your bot sucks!`` |
|  | ``say``  |   | ``[text] What to say`` | Make Godfather say something! | ``!say Luke, I am your father!`` |
|   | ``zugify`` |   | ``[text] Text`` | Requested by Zugi. It is so stupid it isn't worth describing... | ``!zugify Some text`` |
|   |   |   |   |   |   |   |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``createcategory``<br>``createc``<br>``+c``<br>``makec``<br>``newc``<br>``addc`` | Manage Channels | ``[text] Name`` | Create new channel category. | ``!channel createcategory My Category`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``createtext``<br>``createt``<br>``+``<br>``+t``<br>``maket``<br>``newt``<br>``addt`` | Manage Channels | ``[string] Name`` | Create new text channel. *Note: Discord does not allow spaces in text channel name.* | ``!channel createtext spam`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``createvoice``<br>``createv``<br>``+v``<br>``makev``<br>``newv``<br>``addv`` | Manage Channels | ``[text] Name`` | Create new voice channel. | ``!channel createvoice My Voice Channel`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``delete``<br>``-``<br>``d``<br>``del``<br>``remove`` | Manage Channels | ``(optional) [channel] Channel/Category`` | Delete channel or category. If channel is not given as argument, deletes the current channel. | ``!channel delete``<br><br>``!channel delete #afkchannel`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``info``<br>``i``<br>``information`` | Manage Channels | ``(optional) [channel] Channel/Category`` | Get channel information. | ``!channel info``<br><br>``!channel info #afkchannel`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``rename``<br>``r``<br>``name``<br>``setname`` | Manage Channels | ``[string] Name``<br><br>``(optional) [channel] Channel/Category`` | Rename channel. If channel is not given as argument, renames the current channel. | ``!channel rename New Name``<br><br>``!channel rename "New Name" "Some Channel Name"`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``settopic``<br>``t``<br>``sett``<br>``topic`` | Manage Channels | ``[string] Topic``<br><br>``(optional) [channel] Channel/Category`` | Set a new channel topic. If channel is not given as argument, modifies the current channel. | ``!channel settopic Welcome to my channel!``<br><br>``!channel settopic "My topic" "Some Channel Name"`` |
|   |   |   |   |   |   |   |
| ``guild``<br>``server``<br>``g`` | ``info``<br>``i``<br>``information`` |  |  | Get guild information. | ``!guild info`` |
| ``guild``<br>``server``<br>``g`` | ``listmembers``<br>``memberlist``<br>``lm``<br>``members`` | Manage Guild | ``(optional) [int] page (def: 1)`` | Get guild member list. | ``!guild memberlist``<br>``!guild memberlist 3`` |
| ``guild``<br>``server``<br>``g`` | ``log``<br>``auditlog``<br>``viewlog``<br>``getlog``<br>``getlogs``<br>``logs`` | View Audit Log | ``(optional) [int] page (def: 1)`` | Get guild audit logs. | ``!guild log``<br>``!guild log 3`` |
| ``guild``<br>``server``<br>``g`` | ``prune``<br>``p``<br>``clean`` | Administrator (user)<br><br>KickMembers (bot) | ``(optional) [int] page (def: 7)`` | Kick members who weren't active in given ammount of days (1-7). | ``!guild prune``<br>``!guild prune 5`` |
| ``guild``<br>``server``<br>``g`` | ``rename``<br>``r``<br>``name``<br>``setname`` | Manage guild | ``[text] Name`` | Rename guild. | ``!guild rename New guild name`` |
| ``guild``<br>``server``<br>``g`` | ``getwelcomechannel``<br>``getwelcomec``<br>``getwc``<br>``getwelcome``<br>``welcomechannel``<br>``wc`` | Manage guild (user) |  | Get current welcome message channel for this guild. | ``!guild getwc`` |
| ``guild``<br>``server``<br>``g`` | ``getleavechannel``<br>``getleavec``<br>``getlc``<br>``getleave``<br>``leavechannel``<br>``lc`` | Manage guild (user) |  | Get current leave message channel for this guild. | ``!guild getlc`` |
| ``guild``<br>``server``<br>``g`` | ``setwelcomechannel``<br>``setwelcomec``<br>``setwc``<br>``setwelcome`` | Manage guild (user) | ``(optional) [channel] Channel`` | Set current welcome message channel for this guild. If not specified, the current channel is set. | ``!guild setwc``<br>``!guild setwc #welcome`` |
| ``guild``<br>``server``<br>``g`` | ``setleavechannel``<br>``setleavec``<br>``setwc``<br>``setleave`` | Manage guild (user) | ``(optional) [channel] Channel`` | Set current leave message channel for this guild. If not specified, the current channel is set. | ``!guild setlc``<br>``!guild setlc #general`` |
| ``guild``<br>``server``<br>``g`` | ``deletewelcomechannel``<br>``delwelcomec``<br>``delwc``<br>``deletewc``<br>``delwelcome``<br>``dwc`` | Manage guild (user) |  | Delete current welcome message channel for this guild. | ``!guild deletewc`` |
| ``guild``<br>``server``<br>``g`` | ``deleteleavechannel``<br>``delleavec``<br>``dellc``<br>``deletelc``<br>``delleave``<br>``dlc`` | Manage guild (user) |  | Delete current leave message channel for this guild. | ``!guild deletelc`` |
| ``g emoji``<br>``g emojis``<br>``g e`` |  |  |  | List guild emoji. | ``!guild emoji`` |
| ``g emoji``<br>``g emojis``<br>``g e`` | ``add``<br>``+``<br>``a``<br>``create`` | Manage emojis | ``[string] Name``<br><br>``[string] URL`` | Add a new guild emoji from URL. | ``!guild emoji add http://blabla.com/someemoji.img`` |
| ``g emoji``<br>``g emojis``<br>``g e`` | ``delete``<br>``-``<br>``del``<br>``d``<br>``remove`` | Manage emojis | ``[emoji] Emoji`` | Remove emoji from guild emoji list.<br>*Note: Bots can only remove emoji which they created!* | ``!guild emoji del :pepe:`` |
| ``g emoji``<br>``g emojis``<br>``g e`` | ``list``<br>``print``<br>``show``<br>``print``<br>``l``<br>``p`` |  |  | List guild emoji. | ``!guild emoji list`` |
| ``g emoji``<br>``g emojis``<br>``g e`` | ``modify``<br>``edit``<br>``mod``<br>``e``<br>``m`` | Manage emojis | ``[emoji] Emoji``<br>``[string] New name`` | Modify guild emoji. | ``!guild emoji edit :pepe: pepenewname`` |
|   |   |   |   |   |   |   |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``delete``<br>``-``<br>``d``<br>``del``<br>``prune`` | Administrator (user)<br><br>Manage messages (bot) | ``[int] Ammount (def: 5)`` | Delete ``Ammount`` messages from the current channel. | ``!messages delete 100`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``deletefrom``<br>``-user``<br>``du``<br>``deluser``<br>``dfu`` | Administrator (user)<br><br>Manage messages (bot) | ``[user] User``<br><br>``[int] Ammount (def: 5)`` | Delete ``Ammount`` messages from ``User`` in the current channel. | ``!messages deletefrom @Someone 100`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``listpinned``<br>``lp``<br>``listpins``<br>``listpin``<br>``pinned`` |  | ``[int] Ammount (def: 1)`` | List ``Ammount`` pinned messages. | ``!messages listpinned 5`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``pin``<br>``p`` | Manage Messages  |  | Pin last sent message (before ``pin`` command). | ``!messages pin`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``unpin``<br>``up`` | Manage Messages  | ``[int] Index (starting from 0)`` | Unpin pinned message with index ``Index`` in pinned message list. | ``!messages unpin 3`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``unpinall``<br>``upa`` | Manage Messages  |  | Unpin all pinned messages. | ``!messages unpinall`` |
|   |   |   |   |   |   |


**(Command list is incomplete)**


---
