# the-godfather

Just another Discord bot. Written using DSharpPlus.

[Project website](https://ivan-ristovic.github.io/the-godfather/)

---

# Command list

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
* ``bool`` : ``true`` or ``false``
* ``text`` : Some Unicode text, can include spaces. Can be surrounded with ``"``
* ``user`` : Discord user, given by ``@mention``, ``Username`` or UID (User ID)
* ``channel`` : Discord channel, given by ``name``, ``#name`` or CID (Channel ID)
* ``role`` : An existing role, given with ``@mentionrole`` or ``Role name``
* ``emoji`` : Emoji, either Unicode or Discord representation (using ``:``)


## Command table

*Note: Required permissions are permissions required for both bot and user to run the command (if not specified otherwise in table)*
<br><br>

## Main commands

| Command group (with synonyms) | Command name (with synonyms) | Required Permissions | Command arguments | Command Description | Example of use |
|---|---|---|---|---|---|
|   |   |   |   |   |   |
|   | ``8ball`` |   | ``[text] Question`` | Get an answer to your question from an almighty magic ball. | ``!8ball Are mirrors real?`` |
|   | ``embed`` | Attach files (user) | ``[string] URL`` | Embeds image given as URL and sends a embed frame. | ``!embed https://img.memecdn.com/feelium_o_1007518.jpg``  |
|   | ``greet``<br>``hello``<br>``hi``<br>``halo``<br>``hey``<br>``howdy``<br>``sup`` |  |  | Greets a user and starts a conversation | ``!greet`` |
|   | ``invite``<br>``getinvite`` | Create instant invite |  | Get an instant invite link for the current channel. | ``!invite`` |
|   | ``leave`` | Kick members (user) |   | Makes Godfather leave the server. | ``!leave`` |
|   | ``leet`` |   | ``[text] Text`` | Wr1t3s m3ss@g3 1n 1337sp34k. | ``!leet This is so cool`` |
|   | ``penis``<br>``size``<br>``length``<br>``manhood``<br>``dick`` |   | ``(optional) [user] User (def: sender)`` | An accurate measurement of ``User``'s manhood. | ``!penis @Someone`` |
|   | ``ping`` |   |   | Ping the bot. | ``!ping`` |
|   | ``poll``<br>``vote`` |   | ``[text] Question`` | Starts a poll in the channel. The bot will ask for poll options, which you give separated with ``;``, for example: ``option1 ; another option ; option3`` | ``!poll "Do you want to kick Godfather?"`` |
|   | ``pollr``<br>``vote`` |   | ``[emoji] Reactions (min 2)`` | Starts a poll with reactions in the channel. | ``!pollr :smile: :angry:`` |
|   | ``prefix``<br>``setprefix`` | Administrator (user) | ``(optional) [string] New prefix (maxlen: 10)`` | If invoked without arguments, gives current prefix for this guild, otherwise sets the prefix to ``New prefix``. If for example ``New prefix`` is ``;``, all commands in this guild from that point must be invoked using ``;`` (``;greet``). | ``!prefix``<br><br>``!prefix .`` |
|   | ``rate``<br>``score``<br>``graph`` |   | ``[user] User`` | An accurate graphical representatin of ``User``'s humanity. | ``!rate @Someone`` |
|   | ``remind`` |   | ``[int] Time to wait before repeat (in seconds)``<br><br>``[text] What to repeat`` | Repeat given text after given time. | ``!remind 3600 I was told to remind you to do this`` |
|   | ``report`` |   | ``[text] Report message`` | Send message to owner (hopefully about a bug, I can see it being abused) | ``!report Your bot sucks!`` |
|   | ``say``  |   | ``[text] What to say`` | Make Godfather say something! | ``!say Luke, I am your father!`` |
|   | ``tts``  |   | ``[text] What to say`` | Make Godfather say something out loud (using tts)! | ``!tts Luke, I am your father!`` |
|   | ``zugify`` |   | ``[text] Text`` | Requested by Zugi. It is so stupid it isn't worth describing... | ``!zugify Some text`` |
   |   |   |   |   |   |
| ``emojireaction``<br>``emojireactions``<br>``ereact``<br>``emojir``<br>``er`` |   | Manage Guild (user) | ``[emoji] Emoji``<br><br>``[text] Triggers`` | Add a new automatic emoji reaction to a word from trigger list. Whenever someone sends a message containing a trigger word, bot will react to it with ``Emoji``. Trigger list is made of words separated with whitespace. | ``!emojir :smile: laughing`` |
| ``emojireaction``<br>``emojireactions``<br>``ereact``<br>``emojir``<br>``er`` | ``add``<br>``+``<br>``new`` | Manage Guild (user) | ``[emoji] Emoji``<br><br>``[text] Triggers`` | Same as above command. | ``!emojir add :smile: laughing`` |
| ``emojireaction``<br>``emojireactions``<br>``ereact``<br>``emojir``<br>``er`` | ``clear``<br>``c``<br>``da`` | Administrator (user) |  | Delete all reactions for the current guild. | ``!emojir clear`` |
| ``emojireaction``<br>``emojireactions``<br>``ereact``<br>``emojir``<br>``er`` | ``delete``<br>``-``<br>``remove``<br>``del``<br>``rm`` | Manage Guild (user) | ``[text] Triggers`` | Remove trigger word (can be a list of words separated by whitespaces) from guild emoji reaction list. Use ``!reaction list`` to view reactions. | ``!emojir delete smile hehe blabla`` |
| ``emojireaction``<br>``emojireactions``<br>``ereact``<br>``emojir``<br>``er`` | ``list``<br>``ls``<br>``l`` |  | ``(optional) [int] Page (def: 1)`` | List guild reactions on page ``Page``. | ``!emojir list 3`` |
|   |   |   |   |   |   |
| ``filter``<br>``filters``<br>``f`` | ``add``<br>``+``<br>``new`` | Manage Guild (user) | ``[text] Trigger`` | Add a new filter to filter guild list. Whenever someone sends a message containing ``Trigger``, bot will delete it. Triggers can also be regular expressions (case ignored). | ``!filter add fuck`` |
| ``filter``<br>``filters``<br>``f`` | ``clear``<br>``ca``<br>``c`` | Administrator (user) |  | Delete all filters for the current guild. | ``!filter clear`` |
| ``filter``<br>``filters``<br>``f`` | ``delete``<br>``-``<br>``remove``<br>``del``<br>``rm`` | Manage Guild (user) | ``[text] Trigger`` | Remove given filter from guild filter list. Use ``!filter list`` to view filters. | ``!filter delete fuck`` |
| ``filter``<br>``filters``<br>``f`` | ``list`` | ``ls``<br>``l`` | ``(optional) [int] Page (def: 1)`` | List guild filters on page ``Page``. | ``!filter list 3`` |
|   |   |   |   |   |   |
| ``insult``<br>``burn``<br>``insults`` |   |   | ``(optional) [user] User (def: sender)`` | Insult ``User``. | ``!insult``<br><br>``!insult @Someone`` |
| ``insult``<br>``burn``<br>``insults`` | ``add``<br>``+``<br>``new`` | Owner Only | ``[text] Insult`` | Add a new insult to global insult list. You can use ``%user%`` in your insult text as a replacement for the user mention who will be insulted. | ``!insult add Your age is greater than your IQ, %user%!`` |
| ``insult``<br>``burn``<br>``insults`` | ``clear``<br>``clearall`` | Owner Only |  | Delete all insults. | ``!insult clear`` |
| ``insult``<br>``burn``<br>``insults`` | ``delete``<br>``-``<br>``remove``<br>``del``<br>``rm`` | Owner Only | ``[int] Index`` | Remove insult with a given index from list. Use ``!insults list`` to view indexes. | ``!insult delete 5`` |
| ``insult``<br>``burn``<br>``insults`` | ``list`` |  | ``(optional) [int] Page (def: 1)`` | List insults on page ``Page``. | ``!insult list 3`` |
|   |   |   |   |   |   |
| ``meme``<br>``memes``<br>``mm`` |   |   | ``(optional) [text] Meme name`` | Send a meme with name ``Meme name``. If name isn't given, sends random one. | ``!meme``<br><br>``!meme fap`` |
| ``meme``<br>``memes``<br>``mm`` | ``add``<br>``+``<br>``new``<br>``a`` | Owner Only | ``[text] Name``<br><br>``[string] URL`` | Add a new meme to global meme list. | ``!meme add MyMeme http://url.png`` |
| ``meme``<br>``memes``<br>``mm`` | ``create``<br>``maker``<br>``make``<br>``c``<br>``m`` |  | ``[string] Template``<br><br>``[string] Top Text``<br><br>``[string] Bottom Text`` | Creates a new meme with ``Template`` as background and ``Top Text`` and ``Bottom Text`` on it. Use ``!meme template list`` to view all templates. | ``!meme create successkid "I tried to create a meme" "I succeeded!"`` |
| ``meme``<br>``memes``<br>``mm`` | ``delete``<br>``-``<br>``remove``<br>``del``<br>``rm``<br>``d`` | Owner Only | ``[text] Name`` | Remove meme with a given name from the list. Use ``!meme list`` to view all memes. | ``!meme delete fap`` |
| ``meme``<br>``memes``<br>``mm`` | ``list``<br>``ls``<br>``l`` |  | ``(optional) [int] Page (def: 1)`` | List memes on page ``Page``. | ``!meme list 3`` |
| ``meme templates``<br>``meme template``<br>``meme t`` | ``list``<br>``ls``<br>``l`` |  | ``(optional) [int] Page (def: 1)`` | List templates on page ``Page``. | ``!meme templates list 4`` |
| ``meme templates``<br>``meme template``<br>``meme t`` | ``add``<br>``+``<br>``new`` | Owner Only | ``[string] Template name``<br><br>``[string] URL`` | Add a new meme template to global meme template list. | ``!meme template add somename http://url.png`` |
| ``meme templates``<br>``meme template``<br>``meme t`` | ``delete``<br>``-``<br>``remove``<br>``del``<br>``rm``<br>``d`` | Owner Only | ``[text] Name``<br><br>``[string] URL`` | Add a new meme template to global meme template list. | ``!meme template add somename http://url.png`` |
| ``meme templates``<br>``meme template``<br>``meme t`` | ``preview``<br>``p``<br>``pr``<br>``view`` |   | ``[string] Template name`` | Preview a meme template. | ``!meme template preview 1stworld`` |
|   |   |   |   |   |   |
| ``random``<br>``rand``<br>``rnd`` | ``cat`` |   |   | Send a random cat image. | ``!random cat`` |
| ``random``<br>``rand``<br>``rnd`` | ``dog`` |   |   | Send a random dog image. | ``!random dog`` |
| ``random``<br>``rand``<br>``rnd`` | ``choose``<br>``select`` |   | ``[text] List`` | Randomly select one option from given list. Separate options with a ``,``. | ``!random choose red, dark green, blue`` |
| ``random``<br>``rand``<br>``rnd`` | ``raffle`` |   | ``(optional) [role] Role (def: @everyone)`` | Randomly select one online user from given role. | ``!random raffle @Admins`` |
|   |   |   |   |   |   |
| ``rank``<br>``ranks``<br>``ranking`` |   |   |   | Show user rank. One message gives 1XP. | ``!rank`` |
| ``rank``<br>``ranks``<br>``ranking`` | ``list``<br>``levels`` |   |   | Show all ranks and XP needed. | ``!rank list`` |
| ``rank``<br>``ranks``<br>``ranking`` | ``top`` |  |   | Print global rank leaderboard. | ``!rank top`` |
|   |   |   |   |   |   |
| ``textreaction``<br>``textreactions``<br>``treact``<br>``txtr``<br>``tr`` |  | Manage Guild (user) | ``[string] Trigger``<br><br>``[text] Response`` | Add a new trigger to guild text reaction list. Whenever someone sends a message ``Trigger``, bot will repond with ``Response``. You can also use ``%user%`` as a replacement for sender mention. | ``!txtr add "hi" Hey, %user%!`` |
| ``textreaction``<br>``textreactions``<br>``treact``<br>``txtr``<br>``tr`` | ``add``<br>``+``<br>``new`` | Manage Guild (user) | ``[string] Trigger``<br><br>``[text] Response`` | Same as above command. | ``!txtr add "hi" Hey, %user%!`` |
| ``textreaction``<br>``textreactions``<br>``treact``<br>``txtr``<br>``tr`` | ``clear``<br>``c``<br>``da`` | Administrator (user) |  | Delete all text reactions for the current guild. | ``!txtr clear`` |
| ``textreaction``<br>``textreactions``<br>``treact``<br>``txtr``<br>``tr`` | ``delete``<br>``-``<br>``remove``<br>``del``<br>``rm``<br>``d`` | Manage Guild (user) | ``[string] Trigger`` | Remove text reaction with a given trigger from guild text reaction list. Use ``!txtr list`` to view guild triggers. | ``!txtr delete hi`` |
| ``textreaction``<br>``textreactions``<br>``treact``<br>``txtr``<br>``tr`` | ``list``<br>``ls``<br>``l`` |  | ``(optional) [int] Page (def: 1)`` | List guild text reactions on page ``Page``. | ``!txtr list 3`` |
|   |   |   |   |   |   |

## Administration commands

| Command group (with synonyms) | Command name (with synonyms) | Required Permissions | Command arguments | Command Description | Example of use |
|---|---|---|---|---|---|
|   |   |   |   |   |   |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``createcategory``<br>``createc``<br>``+c``<br>``makec``<br>``newc``<br>``addc`` | Manage Channels | ``[text] Name`` | Create new channel category. | ``!channels +c My Category`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``createtext``<br>``createt``<br>``+``<br>``+t``<br>``maket``<br>``newt``<br>``addt`` | Manage Channels | ``[string] Name``<br><br>``(optional) [channel] Parent Channel (def: none)`` | Create new text channel. *Note: Discord does not allow spaces in text channel name.* | ``!channels +t spam``<br><br>``!channels +t spam public`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``createvoice``<br>``createv``<br>``+v``<br>``makev``<br>``newv``<br>``addv`` | Manage Channels | ``[string] Name``<br><br>``(optional) [channel] Parent Channel (def: none)`` | Create new voice channel. | ``!channel createvoice "My Voice Channel"``<br><br>``!channels +v "My Voice Channel" "My category"`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``delete``<br>``-``<br>``d``<br>``del``<br>``remove`` | Manage Channels | ``(optional) [channel] Channel/Category (def: current)``<br><br>``(optional) [text] Reason`` | Delete channel or category. If channel is not given as argument, deletes the current channel. | ``!channels delete``<br><br>``!channels delete #afkchannel`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``info``<br>``i``<br>``information`` | Manage Channels | ``(optional) [channel] Channel/Category (def: current)`` | Get channel information. | ``!channel info``<br><br>``!channel info #afkchannel`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``rename``<br>``r``<br>``name``<br>``setname`` | Manage Channels | ``[string] Name``<br><br>``(optional) [channel] Channel/Category (def: current)`` | Rename channel. If channel is not given as argument, renames the current channel. | ``!channel rename New Name``<br><br>``!channel rename "New Name" "Some Channel Name"`` |
| ``channel``<br>``channels``<br>``c``<br>``chn`` | ``settopic``<br>``t``<br>``sett``<br>``topic`` | Manage Channels | ``[string] Topic``<br><br>``(optional) [channel] Channel/Category (def: current)`` | Set a new channel topic. If channel is not given as argument, modifies the current channel. | ``!channel settopic Welcome to my channel!``<br><br>``!channel settopic "My topic" "Some Channel Name"`` |
|   |   |   |   |   |   |   |
| ``guild``<br>``server``<br>``g`` | ``info``<br>``i``<br>``information`` |  |  | Get guild information. | ``!guild info`` |
| ``guild``<br>``server``<br>``g`` | ``listmembers``<br>``memberlist``<br>``lm``<br>``members`` | Manage Guild | ``(optional) [int] page (def: 1)`` | Get guild member list. | ``!guild memberlist``<br><br>``!guild memberlist 3`` |
| ``guild``<br>``server``<br>``g`` | ``log``<br>``auditlog``<br>``viewlog``<br>``getlog``<br>``getlogs``<br>``logs`` | View Audit Log | ``(optional) [int] page (def: 1)`` | Get guild audit logs. | ``!guild log``<br><br>``!guild log 3`` |
| ``guild``<br>``server``<br>``g`` | ``bans``<br>``banlist``<br>``viewbans``<br>``viewbanlist``<br>``getbans``<br>``getbanlist`` | View Audit Log | ``(optional) [int] page (def: 1)`` | Get guild banlist. | ``!guild bans``<br><br>``!guild bans 3`` |
| ``guild``<br>``server``<br>``g`` | ``prune``<br>``p``<br>``clean`` | Administrator (user)<br><br>Kick Members (bot) | ``(optional) [int] page (def: 7)`` | Kick members who weren't active in given amount of days (1-7). | ``!guild prune``<br><br>``!guild prune 5`` |
| ``guild``<br>``server``<br>``g`` | ``rename``<br>``r``<br>``name``<br>``setname`` | Manage guild | ``[text] Name`` | Rename guild. | ``!guild rename New guild name`` |
| ``guild``<br>``server``<br>``g`` | ``seticon``<br>``si``<br>``icon`` | Manage guild | ``[string] URL`` | Set guild icon. | ``!guild seticon http://someimage.png`` |
| ``guild``<br>``server``<br>``g`` | ``getwelcomechannel``<br>``getwelcomec``<br>``getwc``<br>``getwelcome``<br>``welcomechannel``<br>``wc`` | Manage guild (user) |  | Get current welcome message channel for this guild. | ``!guild getwc`` |
| ``guild``<br>``server``<br>``g`` | ``getleavechannel``<br>``getleavec``<br>``getlc``<br>``getleave``<br>``leavechannel``<br>``lc`` | Manage guild (user) |  | Get current leave message channel for this guild. | ``!guild getlc`` |
| ``guild``<br>``server``<br>``g`` | ``setwelcomechannel``<br>``setwelcomec``<br>``setwc``<br>``setwelcome`` | Manage guild (user) | ``(optional) [channel] Channel`` | Set current welcome message channel for this guild. If not specified, the current channel is set. | ``!guild setwc``<br><br>``!guild setwc #welcome`` |
| ``guild``<br>``server``<br>``g`` | ``setleavechannel``<br>``setleavec``<br>``setwc``<br>``setleave`` | Manage guild (user) | ``(optional) [channel] Channel`` | Set current leave message channel for this guild. If not specified, the current channel is set. | ``!guild setlc``<br><br>``!guild setlc #general`` |
| ``guild``<br>``server``<br>``g`` | ``deletewelcomechannel``<br>``delwelcomec``<br>``delwc``<br>``deletewc``<br>``delwelcome``<br>``dwc`` | Manage guild (user) |  | Delete current welcome message channel for this guild. | ``!guild deletewc`` |
| ``guild``<br>``server``<br>``g`` | ``deleteleavechannel``<br>``delleavec``<br>``dellc``<br>``deletelc``<br>``delleave``<br>``dlc`` | Manage guild (user) |  | Delete current leave message channel for this guild. | ``!guild deletelc`` |
| ``g emoji``<br>``g emojis``<br>``g e`` |  |  |  | List guild emoji. | ``!guild emoji`` |
| ``g emoji``<br>``g emojis``<br>``g e`` | ``add``<br>``+``<br>``a``<br>``create`` | Manage emojis | ``[string] Name``<br><br>``[string] URL`` | Add a new guild emoji from URL. | ``!guild emoji add http://blabla.com/someemoji.img`` |
| ``g emoji``<br>``g emojis``<br>``g e`` | ``delete``<br>``-``<br>``del``<br>``d``<br>``remove`` | Manage emojis | ``[emoji] Emoji`` | Remove emoji from guild emoji list.<br>*Note: Bots can only remove emoji which they created!* | ``!guild emoji del :pepe:`` |
| ``g emoji``<br>``g emojis``<br>``g e`` | ``details``<br>``det`` |  | ``[emoji] Emoji`` | Get details for guild emoji. | ``!guild emoji details :pepe:`` |
| ``g emoji``<br>``g emojis``<br>``g e`` | ``list``<br>``print``<br>``show``<br>``print``<br>``l``<br>``p`` |  |  | List guild emoji. | ``!guild emoji list`` |
| ``g emoji``<br>``g emojis``<br>``g e`` | ``modify``<br>``edit``<br>``mod``<br>``e``<br>``m`` | Manage emojis | ``[emoji] Emoji``<br>``[string] New name`` | Modify guild emoji. | ``!guild emoji edit :pepe: pepenewname`` |
|   |   |   |   |   |   |   |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``delete``<br>``-``<br>``d``<br>``del``<br>``prune`` | Administrator (user)<br><br>Manage messages (bot) | ``[int] Amount (def: 5)`` | Delete ``Amount`` messages from the current channel. | ``!messages delete 100`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``deletefrom``<br>``-user``<br>``du``<br>``deluser``<br>``dfu`` | Administrator (user)<br><br>Manage messages (bot) | ``[user] User``<br><br>``[int] Amount (def: 5)`` | Delete ``Amount`` messages from ``User`` in the current channel. | ``!messages deletefrom @Someone 100`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``deleteregex``<br>``-regex``<br>``dr``<br>``delregex``<br>``dfr`` | Administrator (user)<br><br>Manage messages (bot) | ``[string] Regex``<br><br>``[int] Amount (def: 5)`` | Delete ``Amount`` messages that match pattern ``Regex`` in the current channel (case ignored) | ``!messages deleteregex s+p+a+m+ 100`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``listpinned``<br>``lp``<br>``listpins``<br>``listpin``<br>``pinned`` |  | ``[int] Amount (def: 1)`` | List ``Amount`` pinned messages. | ``!messages listpinned 5`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``pin``<br>``p`` | Manage Messages  |  | Pin last sent message (before ``pin`` command). | ``!messages pin`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``unpin``<br>``up`` | Manage Messages  | ``[int] Index (starting from 0)`` | Unpin pinned message with index ``Index`` in pinned message list. | ``!messages unpin 3`` |
| ``messages``<br>``m``<br>``msg``<br>``msgs`` | ``unpinall``<br>``upa`` | Manage Messages  |  | Unpin all pinned messages. | ``!messages unpinall`` |
|   |   |   |   |   |   |
| ``role``<br>``roles``<br>``r``<br>``rl`` |  |  |  | List all roles for this guild. | ``!roles`` |
| ``role``<br>``roles``<br>``r``<br>``rl`` | ``create``<br>``new``<br>``add``<br>``+`` | Manage Roles | ``[text] Name`` | Create new role with name ``Name`` | ``!role create My new role`` |
| ``role``<br>``roles``<br>``r``<br>``rl`` | ``delete``<br>``del``<br>``d``<br>``-``<br>``remove``<br>``rm`` | Manage Roles | ``[role] Role`` | Delete role ``Role``. | ``!role delete @role``<br><br>``!role delete Some Role`` |
| ``role``<br>``roles``<br>``r``<br>``rl`` | ``mentionall``<br>``@``<br>``ma`` | Mention everyone | ``[role] Role`` | Mention everyone from role ``Role``. | ``!role mentionall @role``<br><br>``!role mentionall Some Role`` |
| ``role``<br>``roles``<br>``r``<br>``rl`` | ``setcolor``<br>``clr``<br>``c``<br>``sc`` | Manage Roles | ``[role] Role``<br><br>``[string] Color (hex code)`` | Set ``Role`` color to ``Color``. | ``!role setcolor #800000 @role``<br><br>``!role setcolor #800000 Some Role`` |
| ``role``<br>``roles``<br>``r``<br>``rl`` | ``setname``<br>``rename``<br>``name``<br>``n`` | Manage Roles | ``[role] Role``<br><br>``[text] Name`` | Change ``Role`` name to ``Name``. | ``!role rename @somerole New Name``<br><br>``!role rename "Unmentionable role" Some new name`` |
| ``role``<br>``roles``<br>``r``<br>``rl`` | ``setmentionable``<br>``mentionable``<br>``m``<br>``setm`` | Manage Roles | ``[role] Role``<br><br>``[bool] Mentionable`` | Set ``Role`` to be mentionable or not. | ``!role mentionable @somerole false``<br><br>``!role mentionable "Unmentionable role" true`` |
| ``role``<br>``roles``<br>``r``<br>``rl`` | ``setvisible``<br>``separate``<br>``h``<br>``seth``<br>``hoist``<br>``sethoist`` | Manage Roles | ``[role] Role``<br><br>``[bool] Visible`` | Set ``Role`` to be visible (hoisted) or not. Visible roles appear separated in memberlist. | ``!role hoist @somerole false``<br><br>``!role hoist "Unmentionable role" true`` |
|   |   |   |   |   |   |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``addrole``<br>``+role``<br>``+r``<br>``ar`` | Manage Roles | ``[user] User``<br><br>``[role] Role`` | Give ``Role`` to ``User``. | ``!user addrole @SomeUser @admins``<br><br>``!user addrole @SomeUser "Unmentionable role"`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``avatar``<br>``a``<br>``pic`` |  | ``[user] User`` | Print ``User``'s avatar. | ``!user avatar @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``ban``<br>``b`` | Ban Members | ``[user] User``<br><br>``(optional) [text] Reason`` | Ban ``User``. | ``!user ban @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``banid``<br>``bid`` | Ban Members | ``[int] ID``<br><br>``(optional) [text] Reason`` | Ban user by ``ID``. | ``!user banid 235088799074484224`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``softban``<br>``sb`` | Ban Members | ``[user] User``<br><br>``(optional) [text] Reason`` | Ban ``User`` and unban him immediately (deletes his messages). | ``!user softban @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``tempban``<br>``tb`` | Ban Members | ``[user] User``<br><br>``[int] Amount of time units``<br><br>``[string] Time unit (s/m/h/d)``<br><br>``(optional) [text] Reason`` | Ban ``User`` and unban him after given time. | ``!user tempban 5 d @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``deafen``<br>``d``<br>``deaf`` | Deafen Members | ``[user] User``<br><br>``(optional) [text] Reason`` | Toggle ``User``'s voice deaf status. | ``!user deafen @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``info``<br>``i``<br>``information`` |  | ``(optional) [user] User (def: sender)`` | Get information about ``User``. | ``!user info``<br><br>``!user info @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``kick``<br>``k`` | Kick Members | ``[user] User``<br><br>``(optional) [text] Reason`` | Kick ``User``. | ``!user kick @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``listperms``<br>``permlist``<br>``perms``<br>``p`` |  | ``(optional) [user] User (def: sender)`` | List permissions for ``User``. | ``!user perms``<br><br>``!user perms @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``listroles``<br>``rolelist``<br>``roles``<br>``r`` |  | ``(optional) [user] User (def: sender)`` | List roles for ``User``. | ``!user roles``<br><br>``!user roles @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``mute``<br>``m`` | Mute Members | ``[user] User``<br><br>``(optional) [text] Reason`` | Mute ``User``. | ``!user mute @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``removerole``<br>``remrole``<br>``rmrole``<br>``-role``<br>``-r``<br>``rr`` | Manage Roles | ``[user] User``<br><br>``[role] Role`` | Remove ``Role`` from ``User``. | ``!user remrole @SomeUser @admins``<br><br>``!user remrole @SomeUser "Unmentionable role"`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``removeallroles``<br>``remallroles``<br>``rmallroles``<br>``-ra``<br>``-rall``<br>``-allr`` | Manage Roles | ``[user] User`` | Remove all roles for ``User``. | ``!user rmallroles @SomeUser`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``setname``<br>``nick``<br>``rename``<br>``name``<br>``newname`` | Manage Nicknames | ``[user] User``<br><br>``[text] New name`` | Change ``User``'s nickname to ``New name`` (for this server). | ``!user setname @SomeUser Some new name`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``unban``<br>``ub`` | Ban Members | ``[int] ID``<br><br>``(optional) [text] Reason`` | Unban user with given ID from the server. | ``!user unban 235088799074484224`` |
| ``user``<br>``users``<br>``u``<br>``usr`` | ``warn``<br>``w`` | Kick Members | ``[user] User``<br><br>``(optional) [text] Warning message`` | Send a warning message to ``User``. | ``!user warn @Troublemaker`` |
|   |   |   |   |   |   |

## Gambling commands

| Command group (with synonyms) | Command name (with synonyms) | Required Permissions | Command arguments | Command Description | Example of use |
|---|---|---|---|---|---|
|   |   |   |   |   |   |
| ``bank``<br>``$``<br>``$$``<br>``$$$`` |  |  | ``(optional) [user] User (def: sender)`` | Prints the account balance for the user (same as ``!bank balance``). | ``!bank`` |
| ``bank``<br>``$``<br>``$$``<br>``$$$`` | ``grant``<br>``give`` | Administrator | ``[user] User``<br><br>``[int] Amount`` | Add ``Amount`` credits to ``User``'s account. | ``!bank grant 100 @LuckyGuy`` |
| ``bank``<br>``$``<br>``$$``<br>``$$$`` | ``register``<br>``r``<br>``activate``<br>``signup`` |  |  | Opens an account for sender in WM bank. | ``!bank register`` |
| ``bank``<br>``$``<br>``$$``<br>``$$$`` | ``status``<br>``balance``<br>``s`` |  | ``(optional) [user] User (def: sender)`` | Prints the account balance for a user. | ``!bank balance``<br><br>``!bank balance @BillGates`` |
| ``bank``<br>``$``<br>``$$``<br>``$$$`` | ``top``<br>``leaderboard`` |  |  | Prints a list of richest users (globally). | ``!bank top`` |
| ``bank``<br>``$``<br>``$$``<br>``$$$`` | ``transfer``<br>``lend`` |  | ``[user] User``<br><br>``[int] Amount`` | Give ``Amount`` credits from your account to ``User``'s account. | ``!bank transfer @MyFriend 100`` |
|   |   |   |   |   |   |
| ``gamble``<br>``bet`` | ``coinflip``<br>``coin``<br>``flip`` |   | ``[int] Bid``<br><br>``[string] Heads/Tails`` | Bet on a coin flip outcome! Can be invoked without both arguments if you do not wish to bet. | ``!bet coinflip 5 heads`` |
| ``gamble``<br>``bet`` | ``roll``<br>``dice``<br>``die`` |   | ``[int] Bid``<br><br>``[int] Guess [1-6]`` | Bet on a dice roll outcome! Can be invoked without both arguments if you do not wish to bet. | ``!bet dice 50 6`` |
| ``gamble``<br>``bet`` | ``slot``<br>``slotmachine`` |   | ``[int] Bid (min: 5)`` | Bet on a slot machine outcome! | ``!bet slot 5`` |
|   |   |   |   |   |   |

## Game commands

| Command group (with synonyms) | Command name (with synonyms) | Required Permissions | Command arguments | Command Description | Example of use |
|---|---|---|---|---|---|
|   |   |   |   |   |   |
| ``cards``<br>``deck`` | ``draw``<br>``take`` |   | ``(optional) [int] Amount (def: 1)`` | Draw ``Amount`` of cards from the top of the deck. | ``!deck draw 5`` |
| ``cards``<br>``deck`` | ``reset``<br>``opennew``<br>``new`` |   |   | Open new deck of cards (unshuffled). | ``!deck new`` |
| ``cards``<br>``deck`` | ``shuffle``<br>``s``<br>``sh``<br>``mix`` |   |   | Shuffle current card deck. | ``!deck shuffle`` |
|   |   |   |   |   |   |
| ``games``<br>``game``<br>``gm`` | ``caro``<br>``c`` |   |   | Challenge friends to a "Caro" game! First who replies with ``me`` or ``i`` will join your game. Play by posting a pair of numbers from 1 to 10 corresponding to a column and row you wish to place your piece on (for example ``2 5``). | ``!game caro`` |
| ``games``<br>``game``<br>``gm`` | ``connectfour``<br>``connect4``<br>``chainfour``<br>``chain4``<br>``c4`` |   |   | Challenge friends to a "Connect Four" game! First who replies with ``me`` or ``i`` will join your game. Play by posting a number from 1 to 9 corresponding to a column you wish to place your piece on. | ``!game c4`` |
| ``games``<br>``game``<br>``gm`` | ``duel``<br>``fight``<br>``vs``<br>``d`` |   | ``[user] Opponent`` | Call ``Opponent`` to a death battle! Type ``hp`` while the duel is on to drink a health potion. | ``!game duel @TheRock`` |
| ``games``<br>``game``<br>``gm`` | ``hangman``<br>``d`` |   |   | Start a new hangman game! | ``!game hangman`` |
| ``games``<br>``game``<br>``gm`` | ``leaderboard``<br>``globalstats`` |   |   | Print global game leaderboard. | ``!game leaderboard`` |
| ``games``<br>``game``<br>``gm`` | ``rps``<br>``rockpaperscissors`` |   |   | Make Godfather play rock-paper-scissors! | ``!game rps`` |
| ``games``<br>``game``<br>``gm`` | ``stats`` |   | ``(optional) [user] User (def: sender)`` | Print game stats for given user. | ``!game stats`` |
| ``games``<br>``game``<br>``gm`` | ``tictactoe``<br>``ttt`` |   |   | Challenge friends to a "Tic-Tac-Toe" game! First who replies with ``me`` or ``i`` will join your game. Play by posting a number from 1 to 9 corresponding to field you wish to place your move on. | ``!game ttt`` |
| ``games``<br>``game``<br>``gm`` | ``typing``<br>``type``<br>``typerace``<br>``typingrace`` |   |   | Start a typing race game. | ``!game typerace`` |
| ``game nunchi``<br>``game n`` |  |   |   | Start a new game or join a pending Nunchi game. | ``!game nunchi`` |
| ``game nunchi``<br>``game n`` | ``rules``<br>``help`` |   |   | How to play? | ``!game nunchi rules`` |
| ``game quiz``<br>``game trivia``<br>``game q`` | ``countries``<br>``flags`` |   |   | Start a new countries quiz. | ``!game quiz countries`` |
| ``game race``<br>``game r`` |  |   |   | Start a new race or join a pending race! | ``!game race`` |
|   |   |   |   |   |   |

## Search commands

| Command group (with synonyms) | Command name (with synonyms) | Required Permissions | Command arguments | Command Description | Example of use |
|---|---|---|---|---|---|
|   |   |   |   |   |   |
| ``gif``<br>``giphy`` |   |   | ``[text] Query`` | Search GIPHY for ``Query`` and send a GIF result. | ``!gif deal with it`` |
| ``gif``<br>``giphy`` | ``random``<br>``r``<br>``rnd``<br>``rand`` |   |   | Send a random GIF from GIPHY. | ``!gif random`` |
| ``gif``<br>``giphy`` | ``trending``<br>``trend``<br>``tr``<br>``t`` |   | ``(optional) [int] Amount (def: 5)`` | Send ``Amount`` of trending GIFs. | ``!gif trending 5`` |
|   |   |   |   |   |   |
| ``imgur``<br>``i`` |   |   | ``[int] Amount``<br><br>``[text] Gallery`` | Search Imgur gallery group with name ``Gallery`` and send top ``Amount`` of result images for this day. | ``!imgur 5 aww`` |
| ``imgur``<br>``i`` | ``latest``<br>``l``<br>``new``<br>``newest`` |   | ``[int] Amount``<br><br>``[text] Gallery`` | Search Imgur gallery group with name ``Gallery`` and send ``Amount`` of newest posted images. | ``!imgur latest 5 aww`` |
| ``imgur``<br>``i`` | ``top``<br>``t`` |   | ``[string] Time Window``<br><br>``[int] Amount``<br><br>``[text] Gallery`` | Send ``Amount`` of top images in gallery group with name ``Gallery`` for given ``Time Window``. ``TimeWindow`` must be one of the following words: ``day/month/week/year/all``. | ``!imgur top month 10 aww`` |
|   |   |   |   |   |   |
| ``joke``<br>``jokes``<br>``j`` |   |   |   | Send a random joke. | ``!joke`` |
| ``joke``<br>``jokes``<br>``j`` | ``search``<br>``s`` |   | ``[text] Query`` | Search for the joke containing ``Query``. | ``!joke blonde`` |
| ``joke``<br>``jokes``<br>``j`` | ``yomomma``<br>``yourmom``<br>``yomama`` |   |   | Send a yomomma joke. | ``!joke yomomma`` |
|   |   |   |   |   |   |
| ``rss``<br>``feed`` |   |   | ``[text] Feed URL`` | Get feed from URL. | ``!feed http://somefeedurl.rss`` |
| ``rss``<br>``feed`` | ``subscribe``<br>``add``<br>``+``<br>``sub`` | Manage Guild (user) | ``[text] Feed URL``<br><br>``(optional) [text] Friendly Name`` | Subscribe to given feed URL. Bot will send messages whenever it detects updates. ``Friendly Name`` will be shown instead of feed URL (if set). | ``!feed subscribe http://somefeedurl.rss``<br><br>``!feed subscribe http://somefeedurl.rss My site RSS`` |
| ``rss``<br>``feed`` | ``unsubscribe``<br>``del``<br>``d``<br>``rm``<br>``-``<br>``unsub`` | Manage Guild (user) | ``[int] Feed index`` | Remove an existing feed subscription using the index (can be seen with ``!feed list``. | ``!feed unsubscribe 5`` |
| ``rss``<br>``feed`` | ``listsubs``<br>``list``<br>``ls`` |   |   | List all subscribed feeds. | ``!feed list`` |
| ``rss``<br>``feed`` | ``wm`` |   |   | Get latest topics from WM forum. | ``!feed wm`` |
| ``rss``<br>``feed`` | ``news`` |   |   | Get latest worldwide news. | ``!feed news`` |
| ``feed reddit``<br>``feed r`` |   |   | ``[string] Subreddit`` | Get latest topics from ``Subreddit``. | ``!feed reddit aww`` |
| ``feed reddit``<br>``feed r`` | ``subscribe``<br>``sub``<br>``add``<br>``a``<br>``+`` | Manage Guild (user) | ``[string] Subreddit`` | Subscribe to given subreddit. Bot will send messages when new post appears. | ``!feed reddit subscribe aww`` |
| ``feed reddit``<br>``feed r`` | ``unsubscribe``<br>``unsub``<br>``del``<br>``d``<br>``-``<br>``rm`` | Manage Guild (user) | ``[string] Subreddit`` | Unsubscribe from given subreddit. | ``!feed reddit unsubscribe aww`` |
| ``feed youtube``<br>``feed yt``<br>``feed y`` |   |   | ``[string] Channel URL``<br><br>``(optional) [text] Friendly Name``  | Get latest videos from YouTube channel. ``Friendly Name`` will be shown instead of feed URL (if set). | ``!feed youtube http://youtu.be/SomeChannel``<br><br>``!feed youtube http://youtu.be/SomeChannel My Videos`` |
| ``feed youtube``<br>``feed yt``<br>``feed y`` | ``subscribe``<br>``sub``<br>``add``<br>``a``<br>``+`` | Manage Guild (user) | ``[string] Channel URL`` | Subscribe to given YouTube channel. Bot will send messages when new video is uploaded. | ``!feed yt subscribe http://youtu.be/SomeChannel`` |
| ``feed youtube``<br>``feed yt``<br>``feed y`` | ``unsubscribe``<br>``unsub``<br>``del``<br>``d``<br>``-``<br>``rm`` | Manage Guild (user) | ``[string] Channel URL`` | Unsubscribe from given YouTube channel. | ``!feed yt unsubscribe http://youtu.be/SomeChannel`` |
|   |   |   |   |   |   |
| ``steam``<br>``st``<br>``s`` | ``profile``<br>``id`` |   | ``[int] Steam ID`` | Get Steam information for given ID. | ``!steam profile 76561198106475313`` |
|   |   |   |   |   |   |
| ``urbandict``<br>``urban``<br>``u`` |   |   | ``[text] Query`` | Search Urban Dictionary for a given query. | ``!urban Snowflake`` |
|   |   |   |   |   |   |
| ``youtube``<br>``y``<br>``yt`` |   |   | ``[text] Query`` | Search YouTube for a given query. | ``!yt Snowflake falling`` |
| ``youtube``<br>``y``<br>``yt`` | ``search``<br>``s`` |   | ``[int] Amount``<br><br>``[text] Query`` | Search YouTube for a given query. | ``!yt Snowflake falling`` |
| ``youtube``<br>``y``<br>``yt`` | ``searchvideo``<br>``searchv``<br>``sv`` |   | ``[text] Query`` | Search YouTube for a given query. Returns videos only | ``!yt searchv Snowflake falling`` |
| ``youtube``<br>``y``<br>``yt`` | ``searchchannel``<br>``searchc``<br>``sc`` |   | ``[text] Query`` | Search YouTube for a given query. Returns channels only | ``!yt searchc Snowflake falling`` |
| ``youtube``<br>``y``<br>``yt`` | ``searchplaylist``<br>``searchp``<br>``sp`` |   | ``[text] Query`` | Search YouTube for a given query. Returns playlists only | ``!yt searchp Snowflake falling`` |
|   |   |   |   |   |   |

## SWAT4 commands

*Note: Ports can be omitted if they are default. Examples:*
- ``!s4 q 13.95.232.189`` *If the joinport is not given, bot will use default 10480 joinport and 10481 queryport.*
- ``!s4 q 5.9.50.39:8480`` *If the joinport is given, bot will use given joinport and default query port (joinport + 1). In this case 8480 and 8481, respectively.*
<br>

| Command group (with synonyms) | Command name (with synonyms) | Required Permissions | Command arguments | Command Description | Example of use |
|---|---|---|---|---|---|
|   |   |   |   |   |   |
| ``swat4``<br>``swat``<br>``s4`` | ``serverlist`` |   |   | Player count for SWAT4 servers in database. | ``!s4 serverlist`` |
| ``swat4``<br>``swat``<br>``s4`` | ``query``<br>``info``<br>``q``<br>``i`` |   | ``[string] IP/Shortname``<br><br>``(optional) [int] Query port (def: joinport + 1)`` | Get info for SWAT4 server with given IP or short name (you can add these manually, popular servers are already added). | ``!s4 query 13.95.232.189:10480``<br><br>``!s4 query soh`` |
| ``swat4``<br>``swat``<br>``s4`` | ``settimeout`` | Owner Only | ``[int] Time (in ms)`` | Set spacecheck ping time. | ``!s4 settimeout 200`` |
| ``swat4``<br>``swat``<br>``s4`` | ``spacecheck``<br>``startcheck``<br>``checkspace`` |   | ``[string] IP/Shortname``<br><br>``(optional) [int] Query port (def: joinport + 1)`` | Start checking for space on SWAT4 server given with IP/Name. Bot will send a message when there is space on the server. One check per user at a time is allowed. | ``!s4 spacecheck 13.95.232.189:10480``<br><br>``!s4 spacecheck soh`` |
| ``swat4``<br>``swat``<br>``s4`` | ``stopcheck``<br>``checkstop`` |   |   | Stops your issued spacecheck. | ``!s4 stopcheck`` |
| ``s4 servers``<br>``s4 srv``<br>``s4 s`` | ``add``<br>``+``<br>``a`` | Administrator (user) | ``[string] Shortname``<br><br>``[string] IP``<br><br>``(optional) [int] Query port (def: joinport + 1)`` | Add a new server to database. If the server uses a non-default query port, add it after IP like in example. | ``!s4 servers + soh 13.95.232.189:10480``<br><br>``!s4 servers + soh 13.95.232.189:10480:10488`` |
| ``s4 servers``<br>``s4 srv``<br>``s4 s`` | ``delete``<br>``-``<br>``del``<br>``d`` | Administrator (user) | ``[string] Shortname`` | Remove a server from database. | ``!s4 servers - soh`` |
| ``s4 servers``<br>``s4 srv``<br>``s4 s`` | ``list``<br><br>``ls``<br><br>``l`` |   |  | ``(optional) [int] Page (def: 1)`` | List servers in database on page ``Page``. | ``!s4 servers list`` |
|   |   |   |   |   |   |

## Voice/Music commands (disabled for now)

| Command group (with synonyms) | Command name (with synonyms) | Required Permissions | Command arguments | Command Description | Example of use |
|---|---|---|---|---|---|
|   |   |   |   |   |   |
|  | ``connect`` | Use Voice | ``(optional) [channel] Channel`` | Connects bot to a given voice channel. If channel is not given, connects to the same voice channel the user is in. | ``!connect``<br><br>``!connect Music`` |
|  | ``disconnect`` |   |   | Disconnects bot from voice channel in the current guild. | ``!disconnect`` |
|  | ``stop``<br>``s`` |   |   | Stop voice playback. | ``!voice stop`` |
| ``play``<br>``music``<br>``p`` |  | Use Voice<br><br>Speak | ``[string] YouTube URL`` | Stream YouTube audio from given URL. | ``!play https://www.youtube.com/watch?v=c5OA1OdkKQI`` <br><br>``!play justin bieber baby`` |
| ``play``<br>``music``<br>``p`` | ``file``<br>``f`` | Use Voice<br><br>Speak | ``[text] File path`` | Stream a file from the server's filesystem. | ``!play file C://system32/idk.mp3`` |
|   |   |   |   |   |   |


## Owner commands

| Command group (with synonyms) | Command name (with synonyms) | Required Permissions | Command arguments | Command Description | Example of use |
|---|---|---|---|---|---|
|   |   |   |   |   |   |
| ``owner``<br>``o``<br>``admin`` | ``botavatar``<br>``setbotavatar``<br>``setavatar`` | Owner Only | ``[string] URL`` | Set Godfather's avatar. | ``!owner setavatar http://someimage.png`` |
| ``owner``<br>``o``<br>``admin`` | ``botname``<br>``setbotname``<br>``setname`` | Owner Only | ``[text] Name`` | Set Godfather's name. | ``!owner setname Vito Corleone`` |
| ``owner``<br>``o``<br>``admin`` | ``clearlog``<br>``clearlogs``<br>``deletelog``<br>``deletelogs`` | Owner Only |   | Clear all application logs. | ``!owner clearlog`` |
| ``owner``<br>``o``<br>``admin`` | ``dbquery``<br>``sql``<br>``dbq``<br>``q`` | Owner Only | ``[text] SQL Query`` | Execute given query on the bot database. | ``!owner dbquery SELECT * FROM gf.users;`` |
| ``owner``<br>``o``<br>``admin`` | ``eval``<br>``compile``<br>``run``<br>``e``<br>``c``<br>``r`` | Owner Only | ``[text] Code (surrounded in code block)`` | Compile and run given code, in context. | ``!owner eval ```return 42;``` `` |
| ``owner``<br>``o``<br>``admin`` | ``leaveguilds`` | Owner Only | ``[int] Guild IDs (separated by comma, if there are more)`` | Make Godfather leave guilds with given IDs. | ``!owner leaveguils 555555555555 1111111111111`` |
| ``owner``<br>``o``<br>``admin`` | ``sendmessage``<br>``send`` | Owner Only | ``[char] u/c (User or Channel)``<br><br>``[int] ID``<br><br>``[text] Message`` | Send a message to user or channel through the bot. | ``!owner send c 55555555555 Say hi to channel``<br><br>``!owner send u 55555555555 Say hi to user in DM`` |
| ``owner``<br>``o``<br>``admin`` | ``shutdown``<br>``disable``<br>``poweroff``<br>``exit``<br>``quit`` | Owner Only |   | Shutdown Godfather. | ``!owner shutdown`` |
| ``owner``<br>``o``<br>``admin`` | ``sudo``<br>``execas``<br>``as`` | Owner Only | ``[user] User``<br><br>``[text] Command`` | Execute ``Command`` as ``User``. | ``!owner execas @Someone !say test`` |
| ``owner``<br>``o``<br>``admin`` | ``toggleignore``<br>``ti`` | Owner Only |   | Toggle ignoring of commands (except this one). | ``!owner toggleignore`` |
| ``owner status`` | ``add``<br>``+``<br>``new`` | Owner Only | ``[string] Activity type``<br><br>``[text] Status`` | Add a new bot status to the list. Activity type can be: ``playing``, ``streaming``, ``watching`` and ``listening`` | ``!owner status add playing with Joky``<br><br>``!owner status add streaming PUBG`` |
| ``owner status`` | ``delete``<br>``-``<br>``remove`` | Owner Only | ``[text] Status`` | Remove playing status from the list. | ``!owner status delete Playing with Joky`` |
| ``owner status`` | ``list`` | Owner Only |   | List all statuses. | ``!owner status list`` |
|   |   |   |   |   |   |


---
