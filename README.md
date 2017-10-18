# the-godfather
Just another Discord bot. Written using DSharpPlus.

---

# Command list

Commands are separated into groups. For example, ``!user`` is a group of commands which allow manipulation of users and it has subcommands ``kick`` , ``ban`` etc. So if you want to call the ``kick`` command, you do it like ``!user kick ...``.

The default prefix for the bot is ``!``, however you can change that using ``!prefix`` command. Also you can trigger commands by mentioning the bot. For example:
``!greet`` is the same as ``@TheGodfather greet``

| Command group | Command name | Required Permissions | Arguments | Description | Example |
|---|---|---|---|---|---|
|   | ``embed`` | Attach files | ``[string] URL`` | Embeds image given as URL and sends a embed frame. | ``!embed https://img.memecdn.com/feelium_o_1007518.jpg``  |
|   | ``greet``<br>``hello``<br>``hi``<br>``halo``<br>``hey``<br>``howdy``<br>``sup`` |  |  | Greets a user and starts a conversation | ``!greet`` |
|   | ``invite``<br>``getinvite`` | Create instant invite |  | Get an instant invite link for the current channel. | ``!invite`` |
|   | ``leave`` | Kick members |   | Makes Godfather leave the server. | ``!leave`` |
|   | ``leet`` |   | ``[string] Text`` | Wr1t3s m3ss@g3 1n 1337sp34k. | ``!leet This is so cool`` |
|   | ``ping`` |   |   | Ping the bot. | ``!ping`` | 
|   | ``prefix``<br>``setprefix`` | Administrator | ``(optional) [string] New prefix`` | If invoked without arguments, gives current prefix for this channel, otherwise sets the prefix to ``New prefix``. If for example ``New prefix`` is ``;``, all commands in that channel from that point must be invoked using ``;``, for example ``;greet``. | ``!prefix`` <br> ``!prefix .`` |
|   | ``remind`` |  | ``[int] Time to wait before repeat (in seconds)``<br>``[string] What to repeat`` | Repeat given text after given time. | ``!repeat 3600 I was told to remind you to do something`` |
|   | ``report`` |   | ``[string] Report message`` | Send message to owner (hopefully about a bug, I can see it being abused) | ``!report Your bot sucks!`` | 
|  | ``say``  |   | ``[string] What to say`` | Make Godfather say something! | ``!say Luke, I am your father!`` |
|   | ``zugify`` |   | ``[string] Text`` | Requested by Zugi. It is so stupid it isn't worth describing... | ``!zugify This is some text`` 

---
