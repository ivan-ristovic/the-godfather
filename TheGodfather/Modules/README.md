# Command list

### 8ball
*An almighty ball which knows answer to everything.*


__**Arguments:**__

`[string...]` : *A question for the almighty ball.*

__**Examples:**__

---

## admin botavatar
*Set bot avatar.*

__**Owner-only.**__

__**Aliases:**__
*setbotavatar, setavatar*


__**Arguments:**__

`[string]` : *URL.*

__**Examples:**__

---

## admin botname
*Set bot name.*

__**Owner-only.**__

__**Aliases:**__
*setbotname, setname*


__**Arguments:**__

`[string...]` : *New name.*

__**Examples:**__

---

## admin clearlog
*Clear application logs.*

__**Owner-only.**__

__**Aliases:**__
*clearlogs, deletelogs, deletelog*


__**Arguments:**__

__**Examples:**__

---

## admin dbquery
*Clear application logs.*

__**Owner-only.**__

__**Aliases:**__
*sql, dbq, q*


__**Arguments:**__

`[string...]` : *SQL Query.*

__**Examples:**__

---

## admin eval
*Evaluates a snippet of C# code, in context.*

__**Owner-only.**__

__**Aliases:**__
*compile, run, e, c, r*


__**Arguments:**__

`[string...]` : *Code to evaluate.*

__**Examples:**__

---

## admin generatecommands
*Generates a command-list.*

__**Owner-only.**__

__**Aliases:**__
*cmdlist, gencmdlist, gencmds*


__**Arguments:**__

(optional) `[string...]` : *File path.* (def: `None`)

__**Examples:**__

---

## admin leaveguilds
*Leave guilds given as IDs.*

__**Owner-only.**__


__**Arguments:**__

`[unsigned long...]` : *Guild ID list.*

__**Examples:**__

---

## admin sendmessage
*Sends a message to a user or channel.*

__**Owner-only.**__

__**Aliases:**__
*send*


__**Arguments:**__

`[string]` : *u/c (for user or channel.)*

`[unsigned long]` : *User/Channel ID.*

`[string...]` : *Message.*

__**Examples:**__

---

## admin shutdown
*Triggers the dying in the vineyard scene.*

__**Owner-only.**__

__**Aliases:**__
*disable, poweroff, exit, quit*


__**Arguments:**__

__**Examples:**__

---

## admin status
*Bot status manipulation.*

__**Owner-only.**__


__**Examples:**__

---

## admin sudo
*Executes a command as another user.*

__**Owner-only.**__

__**Aliases:**__
*execas, as*


__**Arguments:**__

`[member]` : *Member to execute as.*

`[string...]` : *Command text to execute.*

__**Examples:**__

---

## admin toggleignore
*Toggle bot's reaction to commands.*

__**Owner-only.**__

__**Aliases:**__
*ti*


__**Arguments:**__

__**Examples:**__

---

## bank
*Bank manipulation.*

__**Aliases:**__
*$, $$, $$$*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

__**Examples:**__

---

## bank grant
*Magically give funds to a user.*

__**Requires user permissions:**__
*Administrator*

__**Aliases:**__
*give*


__**Arguments:**__

`[user]` : *User.*

`[int]` : *Amount.*

__**Examples:**__

---

## bank register
*Create an account in WM bank.*

__**Aliases:**__
*r, signup, activate*


__**Arguments:**__

__**Examples:**__

---

## bank status
*View account balance for user.*

__**Aliases:**__
*s, balance*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

__**Examples:**__

---

## bank top
*Print the richest users.*

__**Aliases:**__
*leaderboard*


__**Arguments:**__

__**Examples:**__

---

## bank transfer
*Transfer funds from one account to another.*

__**Aliases:**__
*lend*


__**Arguments:**__

`[user]` : *User to send credits to.*

`[int]` : *Amount.*

__**Examples:**__

---

## cards draw
*Draw cards from the top of the deck.*

__**Aliases:**__
*take*


__**Arguments:**__

(optional) `[int]` : *Amount.* (def: `1`)

__**Examples:**__

---

## cards reset
*Opens a brand new card deck.*

__**Aliases:**__
*new, opennew*


__**Arguments:**__

__**Examples:**__

---

## cards shuffle
*Shuffle current deck.*

__**Aliases:**__
*s, sh, mix*


__**Arguments:**__

__**Examples:**__

---

## channel createcategory
*Create new channel category.*

__**Requires permissions:**__
*Manage channels*

__**Aliases:**__
*createcat, createc, ccat, cc, +cat, +c, +category*


__**Arguments:**__

`[string...]` : *Name.*

__**Examples:**__

`!channel createcategory My New Category`

---

## channel createtext
*Create new text channel.*

__**Requires permissions:**__
*Manage channels*

__**Aliases:**__
*createtxt, createt, ctxt, ct, +, +t, +txt*


__**Overload 2:**__

`[string]` : *Name.*

(optional) `[channel]` : *Parent category.* (def: `None`)

(optional) `[boolean]` : *NSFW?* (def: `False`)

__**Overload 1:**__

`[string]` : *Name.*

(optional) `[boolean]` : *NSFW?* (def: `False`)

(optional) `[channel]` : *Parent category.* (def: `None`)

__**Overload 0:**__

`[channel]` : *Parent category.*

`[string]` : *Name.*

(optional) `[boolean]` : *NSFW?* (def: `False`)

__**Examples:**__

`!channel createtext newtextchannel ParentCategory no`

`!channel createtext newtextchannel no`

`!channel createtext ParentCategory newtextchannel`

---

## channel createvoice
*Create new voice channel.*

__**Requires permissions:**__
*Manage channels*

__**Aliases:**__
*createv, cvoice, cv, +voice, +v*


__**Overload 2:**__

`[string]` : *Name.*

(optional) `[channel]` : *Parent category.* (def: `None`)

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

__**Overload 1:**__

`[string]` : *Name.*

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

(optional) `[channel]` : *Parent category.* (def: `None`)

__**Overload 0:**__

`[channel]` : *Parent category.*

`[string]` : *Name.*

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

__**Examples:**__

`!channel createtext "My voice channel" ParentCategory 0 96000`

`!channel createtext "My voice channel" 10 96000`

`!channel createtext ParentCategory "My voice channel" 10 96000`

---

## channel delete
*Delete a given channel or category.*

__**Requires permissions:**__
*Manage channels*

__**Aliases:**__
*-, del, d, remove, rm*


__**Overload 1:**__

(optional) `[channel]` : *Channel to delete.* (def: `None`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Overload 0:**__

`[string...]` : *Reason.*

__**Examples:**__

`!channel delete`

`!channel delete "My voice channel"`

`!channel delete "My voice channel" Because I can!`

---

## channel info
*Get information about a given channel.*

__**Requires permissions:**__
*Read messages*

__**Aliases:**__
*i, information*


__**Arguments:**__

(optional) `[channel]` : *Channel.* (def: `None`)

__**Examples:**__

`!channel info`

`!channel info "My voice channel"`

---

## channel modify
*Modify a given voice channel. Set 0 if you wish to keep the value as it is.*

__**Requires permissions:**__
*Manage channels*

__**Aliases:**__
*edit, mod, m, e*


__**Overload 1:**__

`[channel]` : *Voice channel to edit*

(optional) `[int]` : *User limit.* (def: `0`)

(optional) `[int]` : *Bitrate.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Overload 0:**__

(optional) `[int]` : *User limit.* (def: `0`)

(optional) `[int]` : *Bitrate.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!channel modify "My voice channel" 20 96000 Some reason`

---

## channel rename
*Rename channel.*

__**Requires permissions:**__
*Manage channels*

__**Aliases:**__
*r, name, setname*


__**Overload 2:**__

`[string]` : *Reason.*

`[channel]` : *Channel to rename.*

`[string...]` : *New name.*

__**Overload 1:**__

`[channel]` : *Channel to rename.*

`[string...]` : *New name.*

__**Overload 0:**__

`[string...]` : *New name.*

__**Examples:**__

`!channel rename New name for this channel`

`!channel rename "My voice channel" "My old voice channel"`

`!channel rename "My reason" "My voice channel" "My old voice channel"`

---

## channel setparent
*Change the parent of the given channel.*

__**Requires permissions:**__
*Manage channels*

__**Aliases:**__
*setpar, par, parent*


__**Overload 1:**__

`[channel]` : *Child channel.*

`[channel]` : *Parent category.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Overload 0:**__

`[channel]` : *Parent category.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!channel setparent "My channel" ParentCategory`

`!channel setparent ParentCategory I set a new parent for this channel!`

---

## channel setposition
*Change the position of the given channel in the guild channel list.*

__**Requires permissions:**__
*Manage channels*

__**Aliases:**__
*setpos, pos, position*


__**Overload 2:**__

`[channel]` : *Channel to reorder.*

`[int]` : *Position.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Overload 1:**__

`[int]` : *Position.*

`[channel]` : *Channel to reorder.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Overload 0:**__

`[int]` : *Position.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!channel setposition 4`

`!channel setposition "My channel" 1`

`!channel setposition "My channel" 4 I changed position :)`

---

## channel settopic
*Set channel topic.*

__**Requires permissions:**__
*Manage channels*

__**Aliases:**__
*t, topic, sett*


__**Overload 2:**__

`[string]` : *Reason.*

`[channel]` : *Channel.*

`[string...]` : *New topic.*

__**Overload 1:**__

`[channel]` : *Channel.*

`[string...]` : *New Topic.*

__**Overload 0:**__

`[string...]` : *New Topic.*

__**Examples:**__

`!channel settopic New channel topic`

`!channel settopic "My channel" New channel topic`

---

### connect
*Connects me to a voice channel.*

__**Owner-only.**__

__**Requires permissions:**__
*Use voice chat*


__**Arguments:**__

(optional) `[channel]` : *Channel.* (def: `None`)

__**Examples:**__

---

### disconnect
*Disconnects from voice channel.*

__**Owner-only.**__


__**Arguments:**__

__**Examples:**__

---

### embed
*Embed an image given as an URL.*

__**Requires permissions:**__
*Attach files*


__**Arguments:**__

`[string]` : *Image URL.*

__**Examples:**__

---

## emojireaction
*Emoji reaction handling.*

__**Aliases:**__
*ereact, er, emojir, emojireactions*


__**Arguments:**__

(optional) `[emoji]` : *Emoji to send.* (def: `None`)

`[string...]` : *Trigger word list.*

__**Examples:**__

---

## emojireaction add
*Add emoji reactions to guild reaction list.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*+, new*


__**Arguments:**__

`[emoji]` : *Emoji to send.*

`[string...]` : *Trigger word list.*

__**Examples:**__

---

## emojireaction clear
*Delete all reactions for the current guild.*

__**Requires user permissions:**__
*Administrator*

__**Aliases:**__
*da, c*


__**Arguments:**__

__**Examples:**__

---

## emojireaction delete
*Remove emoji reactions for given trigger words.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*-, remove, del, rm, d*


__**Arguments:**__

`[string...]` : *Trigger word list.*

__**Examples:**__

---

## emojireaction list
*Show all emoji reactions.*

__**Aliases:**__
*ls, l*


__**Arguments:**__

(optional) `[int]` : *Page.* (def: `1`)

__**Examples:**__

---

## filter add
*Add filter to guild filter list.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*+, new, a*


__**Arguments:**__

`[string...]` : *Filter. Can be a regex (case insensitive).*

__**Examples:**__

---

## filter clear
*Delete all filters for the current guild.*

__**Requires user permissions:**__
*Administrator*

__**Aliases:**__
*c, da*


__**Arguments:**__

__**Examples:**__

---

## filter delete
*Remove filter from guild filter list.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*-, remove, del*


__**Arguments:**__

`[string...]` : *Filter to remove.*

__**Examples:**__

---

## filter list
*Show all filters for this guild.*

__**Aliases:**__
*ls, l*


__**Arguments:**__

(optional) `[int]` : *Page* (def: `1`)

__**Examples:**__

---

## gamble coinflip
*Flips a coin.*

__**Aliases:**__
*coin, flip*


__**Arguments:**__

(optional) `[int]` : *Bid.* (def: `0`)

(optional) `[string]` : *Heads/Tails (h/t).* (def: `None`)

__**Examples:**__

---

## gamble roll
*Rolls a dice.*

__**Aliases:**__
*dice, die*


__**Arguments:**__

(optional) `[int]` : *Bid.* (def: `0`)

(optional) `[int]` : *Number guess.* (def: `0`)

__**Examples:**__

---

## gamble slot
*Roll a slot machine.*

__**Aliases:**__
*slotmachine*


__**Arguments:**__

(optional) `[int]` : *Bid.* (def: `5`)

__**Examples:**__

---

## games caro
*Starts a caro game.*

__**Aliases:**__
*c*


__**Arguments:**__

__**Examples:**__

---

## games connectfour
*Starts a "Connect4" game. Play by posting a number from 1 to 9 corresponding to the column you wish to place your move on.*

__**Aliases:**__
*connect4, chain4, chainfour, c4*


__**Arguments:**__

__**Examples:**__

---

## games duel
*Starts a duel which I will commentate.*

__**Aliases:**__
*fight, vs, d*


__**Arguments:**__

`[user]` : *Who to fight with?*

__**Examples:**__

---

## games hangman
*Starts a hangman game.*

__**Aliases:**__
*h, hang*


__**Arguments:**__

__**Examples:**__

---

## games leaderboard
*Starts a hangman game.*

__**Aliases:**__
*globalstats*


__**Arguments:**__

__**Examples:**__

---

## games nunchi
*Nunchi game commands*

__**Aliases:**__
*n*


__**Arguments:**__

__**Examples:**__

---

## games quiz
*Start a quiz!*

__**Aliases:**__
*trivia, q*


__**Examples:**__

---

## games race
*Racing!*

__**Aliases:**__
*r*


__**Arguments:**__

__**Examples:**__

---

## games rps
*Rock, paper, scissors game.*

__**Aliases:**__
*rockpaperscissors*


__**Arguments:**__

__**Examples:**__

---

## games stats
*Print game stats for given user.*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

__**Examples:**__

---

## games tictactoe
*Starts a game of tic-tac-toe. Play by posting a number from 1 to 9 corresponding to field you wish to place your move on.*

__**Aliases:**__
*ttt*


__**Arguments:**__

__**Examples:**__

---

## games typing
*Typing race.*

__**Aliases:**__
*type, typerace, typingrace*


__**Arguments:**__

__**Examples:**__

---

## gif
*GIPHY commands.*

__**Aliases:**__
*giphy*


__**Arguments:**__

`[string...]` : *Query.*

__**Examples:**__

---

## gif random
*Return a random GIF.*

__**Aliases:**__
*r, rand, rnd*


__**Arguments:**__

__**Examples:**__

---

## gif trending
*Return an amount of trending GIFs.*

__**Aliases:**__
*t, tr, trend*


__**Arguments:**__

(optional) `[int]` : *Number of results (1-10).* (def: `5`)

__**Examples:**__

---

### greet
*Greets a user and starts a conversation.*

__**Aliases:**__
*hello, hi, halo, hey, howdy, sup*


__**Arguments:**__

__**Examples:**__

---

## guild bans
*Get guild ban list.*

__**Requires permissions:**__
*View audit log*

__**Aliases:**__
*banlist, viewbanlist, getbanlist, getbans, viewbans*


__**Arguments:**__

__**Examples:**__

---

## guild deleteleavechannel
*Remove leave message channel for this guild.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*delleavec, dellc, delleave, dlc*


__**Arguments:**__

__**Examples:**__

---

## guild deletewelcomechannel
*Remove welcome message channel for this guild.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*delwelcomec, delwc, delwelcome, dwc, deletewc*


__**Arguments:**__

__**Examples:**__

---

## guild emoji
*Manipulate guild emoji.*

__**Aliases:**__
*emojis, e*


__**Arguments:**__

__**Examples:**__

---

## guild getleavechannel
*Get current leave message channel for this guild.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*getleavec, getlc, getleave, leavechannel, lc*


__**Arguments:**__

__**Examples:**__

---

## guild getwelcomechannel
*Get current welcome message channel for this guild.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*getwelcomec, getwc, getwelcome, welcomechannel, wc*


__**Arguments:**__

__**Examples:**__

---

## guild info
*Get guild information.*

__**Aliases:**__
*i, information*


__**Arguments:**__

__**Examples:**__

---

## guild listmembers
*Get guild member list.*

__**Aliases:**__
*memberlist, lm, members*


__**Arguments:**__

__**Examples:**__

---

## guild log
*Get audit logs.*

__**Requires permissions:**__
*View audit log*

__**Aliases:**__
*auditlog, viewlog, getlog, getlogs, logs*


__**Arguments:**__

__**Examples:**__

---

## guild prune
*Kick guild members who weren't active in given amount of days (1-7).*

__**Requires permissions:**__
*Kick members*

__**Requires user permissions:**__
*Administrator*

__**Aliases:**__
*p, clean*


__**Arguments:**__

(optional) `[int]` : *Days.* (def: `7`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## guild rename
*Rename guild.*

__**Requires permissions:**__
*Manage guild*

__**Aliases:**__
*r, name, setname*


__**Arguments:**__

`[string...]` : *New name.*

__**Examples:**__

---

## guild seticon
*Change icon of the guild.*

__**Requires permissions:**__
*Manage guild*

__**Aliases:**__
*icon, si*


__**Arguments:**__

`[string]` : *New icon URL.*

__**Examples:**__

---

## guild setleavechannel
*Set leave message channel for this guild.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*leavec, setlc, setleave*


__**Arguments:**__

(optional) `[channel]` : *Channel.* (def: `None`)

__**Examples:**__

---

## guild setwelcomechannel
*Set welcome message channel for this guild.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*setwc, setwelcomec, setwelcome*


__**Arguments:**__

(optional) `[channel]` : *Channel.* (def: `None`)

__**Examples:**__

---

### help
*Displays command help.*


__**Arguments:**__

`[string...]` : *Command to provide help for.*

__**Examples:**__

---

## imgur
*Search imgur. Invoking without sub command searches top.*

__**Aliases:**__
*img, im, i*


__**Arguments:**__

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Query.*

__**Examples:**__

---

## imgur latest
*Return latest images for query.*

__**Aliases:**__
*l, new, newest*


__**Arguments:**__

`[int]` : *Number of images to print [1-10].*

`[string]` : *Query.*

__**Examples:**__

---

## imgur top
*Return most rated images for query.*

__**Aliases:**__
*t*


__**Arguments:**__

(optional) `[string]` : *Time window (day/month/week/year/all).* (def: `day`)

(optional) `[int]` : *Number of images to print [1-10].* (def: `1`)

(optional) `[string...]` : *Query.* (def: `None`)

__**Examples:**__

---

## insult
*Burns a user!*

__**Aliases:**__
*burn, insults*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

__**Examples:**__

---

## insult add
*Add insult to list (Use % to code mention).*

__**Owner-only.**__

__**Aliases:**__
*+, new*


__**Arguments:**__

`[string...]` : *Response.*

__**Examples:**__

---

## insult clear
*Delete all insults.*

__**Owner-only.**__

__**Aliases:**__
*clearall*


__**Arguments:**__

__**Examples:**__

---

## insult delete
*Remove insult with a given index from list. (use ``!insults list`` to view indexes)*

__**Owner-only.**__

__**Aliases:**__
*-, remove, del, rm*


__**Arguments:**__

`[int]` : *Index.*

__**Examples:**__

---

## insult list
*Show all insults.*


__**Arguments:**__

(optional) `[int]` : *Page.* (def: `1`)

__**Examples:**__

---

### invite
*Get an instant invite link for the current channel.*

__**Requires permissions:**__
*Create instant invites*

__**Aliases:**__
*getinvite*


__**Arguments:**__

__**Examples:**__

---

## joke
*Send a joke.*

__**Aliases:**__
*jokes, j*


__**Arguments:**__

__**Examples:**__

---

## joke search
*Search for the joke containing the query.*

__**Aliases:**__
*s*


__**Arguments:**__

`[string...]` : *Query.*

__**Examples:**__

---

## joke yourmom
*Yo mama so...*

__**Aliases:**__
*mama, m, yomomma, yomom, yomoma, yomamma, yomama*


__**Arguments:**__

__**Examples:**__

---

### leave
*Makes Godfather leave the server.*

__**Requires user permissions:**__
*Kick members*


__**Arguments:**__

__**Examples:**__

---

### leet
*Wr1t3s m3ss@g3 1n 1337sp34k.*


__**Arguments:**__

`[string...]` : *Text*

__**Examples:**__

---

## meme
*Manipulate memes. When invoked without name, returns a random one.*

__**Aliases:**__
*memes, mm*


__**Arguments:**__

(optional) `[string...]` : *Meme name.* (def: `None`)

__**Examples:**__

---

## meme add
*Add a new meme to the list.*

__**Owner-only.**__

__**Aliases:**__
*+, new, a*


__**Arguments:**__

`[string]` : *Short name (case insensitive).*

`[string]` : *URL.*

__**Examples:**__

---

## meme create
*Creates a new meme from blank template.*

__**Aliases:**__
*maker, c, make, m*


__**Arguments:**__

`[string]` : *Template.*

`[string]` : *Top Text.*

`[string]` : *Bottom Text.*

__**Examples:**__

---

## meme delete
*Deletes a meme from list.*

__**Owner-only.**__

__**Aliases:**__
*-, del, remove, rm, d*


__**Arguments:**__

`[string]` : *Short name (case insensitive).*

__**Examples:**__

---

## meme list
*List all registered memes.*

__**Aliases:**__
*ls, l*


__**Arguments:**__

(optional) `[int]` : *Page.* (def: `1`)

__**Examples:**__

---

## meme templates
*Manipulate meme templates.*

__**Aliases:**__
*template, t*


__**Arguments:**__

__**Examples:**__

---

## message attachments
*Print all message attachments.*

__**Aliases:**__
*a, files, la*


__**Arguments:**__

(optional) `[unsigned long]` : *Message ID.* (def: `0`)

__**Examples:**__

---

## message delete
*Deletes the specified amount of most-recent messages from the channel.*

__**Requires permissions:**__
*Manage messages*

__**Requires user permissions:**__
*Administrator*

__**Aliases:**__
*-, prune, del, d*


__**Arguments:**__

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## message deletefrom
*Deletes given amount of most-recent messages from given user.*

__**Requires permissions:**__
*Manage messages*

__**Requires user permissions:**__
*Administrator*

__**Aliases:**__
*-user, -u, deluser, du, dfu, delfrom*


__**Arguments:**__

`[user]` : *User.*

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## message deletereactions
*Deletes all reactions from the given message.*

__**Requires permissions:**__
*Manage messages*

__**Requires user permissions:**__
*Administrator*

__**Aliases:**__
*-reactions, -r, delreactions, dr*


__**Arguments:**__

(optional) `[unsigned long]` : *ID.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## message deleteregex
*Deletes given amount of most-recent messages that match a given regular expression.*

__**Requires permissions:**__
*Manage messages*

__**Requires user permissions:**__
*Administrator*

__**Aliases:**__
*-regex, -rx, delregex, drx*


__**Arguments:**__

`[string]` : *Pattern (Regex).*

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## message listpinned
*List latest amount of pinned messages.*

__**Aliases:**__
*lp, listpins, listpin, pinned*


__**Arguments:**__

__**Examples:**__

---

## message modify
*Modify the given message.*

__**Requires permissions:**__
*Manage messages*

__**Aliases:**__
*edit, mod, e, m*


__**Arguments:**__

`[unsigned long]` : *Message ID.*

`[string...]` : *New content.*

__**Examples:**__

---

## message pin
*Pins the last sent message. If the ID is given, pins that message.*

__**Requires permissions:**__
*Manage messages*

__**Aliases:**__
*p*


__**Arguments:**__

(optional) `[unsigned long]` : *ID.* (def: `0`)

__**Examples:**__

---

## message unpin
*Unpins the message at given index (starting from 0).*

__**Requires permissions:**__
*Manage messages*

__**Aliases:**__
*up*


__**Arguments:**__

(optional) `[int]` : *Index (starting from 1).* (def: `1`)

__**Examples:**__

---

## message unpinall
*Unpins all pinned messages.*

__**Requires permissions:**__
*Manage messages*

__**Aliases:**__
*upa*


__**Arguments:**__

__**Examples:**__

---

### penis
*An accurate size of the user's manhood.*

__**Aliases:**__
*size, length, manhood, dick*


__**Arguments:**__

`[user]` : *Who to measure*

__**Examples:**__

---

### ping
*Ping the bot.*


__**Arguments:**__

__**Examples:**__

---

## play
*Plays a mp3 file from URL or server filesystem.*

__**Owner-only.**__

__**Aliases:**__
*music, p*


__**Arguments:**__

`[string...]` : *URL or YouTube search query.*

__**Examples:**__

---

## play file
*Plays an audio file from server filesystem.*

__**Owner-only.**__

__**Requires permissions:**__
*Speak, Use voice chat*

__**Aliases:**__
*f*


__**Arguments:**__

`[string...]` : *Full path to the file to play.*

__**Examples:**__

---

### poll
*Starts a poll in the channel.*

__**Aliases:**__
*vote*


__**Arguments:**__

`[string...]` : *Question.*

__**Examples:**__

---

### pollr
*Starts a poll with reactions in the channel.*

__**Aliases:**__
*voter*


__**Arguments:**__

`[emoji...]` : *Options*

__**Examples:**__

---

### prefix
*Get current guild prefix, or change it.*

__**Requires user permissions:**__
*Administrator*

__**Aliases:**__
*setprefix*


__**Arguments:**__

(optional) `[string]` : *Prefix to set.* (def: `None`)

__**Examples:**__

---

## random cat
*Get a random cat image.*


__**Arguments:**__

__**Examples:**__

---

## random choose
*!choose option1, option2, option3...*

__**Aliases:**__
*select*


__**Arguments:**__

`[string...]` : *Option list (separated with a comma).*

__**Examples:**__

---

## random dog
*Get a random dog image.*


__**Arguments:**__

__**Examples:**__

---

## random raffle
*Choose a user from the online members list belonging to a given role.*


__**Arguments:**__

(optional) `[role]` : *Role.* (def: `None`)

__**Examples:**__

---

## rank
*User ranking commands.*

__**Aliases:**__
*ranks, ranking*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

__**Examples:**__

---

## rank list
*Print all available ranks.*

__**Aliases:**__
*levels*


__**Arguments:**__

__**Examples:**__

---

## rank top
*Get rank leaderboard.*


__**Arguments:**__

__**Examples:**__

---

### rate
*An accurate graph of a user's humanity.*

__**Aliases:**__
*score, graph*


__**Arguments:**__

`[user]` : *Who to measure.*

__**Examples:**__

---

### remind
*Resend a message after some time.*


__**Arguments:**__

`[int]` : *Time to wait before repeat (in seconds).*

`[string...]` : *What to repeat.*

__**Examples:**__

---

### report
*Send a report message to owner about a bug (please don't abuse... please).*


__**Arguments:**__

`[string...]` : *Text.*

__**Examples:**__

---

## roles
*Miscellaneous role control commands.*

__**Aliases:**__
*role, r, rl*


__**Arguments:**__

__**Examples:**__

---

## roles create
*Create a new role.*

__**Requires permissions:**__
*Manage roles*

__**Aliases:**__
*new, add, +*


__**Arguments:**__

`[string...]` : *Role.*

__**Examples:**__

---

## roles delete
*Create a new role.*

__**Requires permissions:**__
*Manage roles*

__**Aliases:**__
*del, remove, d, -, rm*


__**Arguments:**__

`[role]` : *Role.*

__**Examples:**__

---

## roles mentionall
*Mention all users from given role.*

__**Requires permissions:**__
*Mention everyone*

__**Aliases:**__
*mention, @, ma*


__**Arguments:**__

`[role]` : *Role.*

__**Examples:**__

---

## roles setcolor
*Set a color for the role.*

__**Requires permissions:**__
*Manage roles*

__**Aliases:**__
*clr, c, sc*


__**Arguments:**__

`[role]` : *Role.*

`[string]` : *Color.*

__**Examples:**__

---

## roles setmentionable
*Set role mentionable var.*

__**Requires permissions:**__
*Manage roles*

__**Aliases:**__
*mentionable, m, setm*


__**Arguments:**__

`[role]` : *Role.*

`[boolean]` : *[true/false]*

__**Examples:**__

---

## roles setname
*Set a name for the role.*

__**Requires permissions:**__
*Manage roles*

__**Aliases:**__
*name, rename, n*


__**Arguments:**__

`[role]` : *Role.*

`[string...]` : *New name.*

__**Examples:**__

---

## roles setvisible
*Set role hoist var (visibility in online list.*

__**Requires permissions:**__
*Manage roles*

__**Aliases:**__
*separate, h, seth, hoist, sethoist*


__**Arguments:**__

`[role]` : *Role.*

`[boolean]` : *[true/false]*

__**Examples:**__

---

## rss
*RSS feed operations.*

__**Aliases:**__
*feed*


__**Arguments:**__

`[string...]` : *URL.*

__**Examples:**__

---

## rss listsubs
*Get feed list for the current channel.*

__**Aliases:**__
*ls, list*


__**Arguments:**__

__**Examples:**__

---

## rss news
*Get newest world news.*


__**Arguments:**__

__**Examples:**__

---

## rss reddit
*Reddit feed manipulation.*

__**Aliases:**__
*r*


__**Arguments:**__

(optional) `[string]` : *Subreddit.* (def: `all`)

__**Examples:**__

---

## rss subscribe
*Subscribe to given url.*

__**Requires permissions:**__
*Manage guild*

__**Aliases:**__
*sub, add, +*


__**Arguments:**__

`[string...]` : *URL.*

(optional) `[string]` : *Friendly name.* (def: `None`)

__**Examples:**__

---

## rss unsubscribe
*Remove an existing feed subscription.*

__**Requires permissions:**__
*Manage guild*

__**Aliases:**__
*del, d, rm, -, unsub*


__**Arguments:**__

`[int]` : *ID.*

__**Examples:**__

---

## rss wm
*Get newest topics from WM forum.*


__**Arguments:**__

__**Examples:**__

---

## rss youtube
*Youtube feed manipulation.*

__**Aliases:**__
*yt, y*


__**Arguments:**__

`[string]` : *Channel URL.*

__**Examples:**__

---

### say
*Repeats after you.*

__**Aliases:**__
*repeat*


__**Arguments:**__

`[string...]` : *Text.*

__**Examples:**__

---

## steam profile
*Get Steam user information from ID.*

__**Aliases:**__
*id*


__**Arguments:**__

`[unsigned long]` : *ID.*

__**Examples:**__

---

### stop
*Stops current voice playback.*

__**Owner-only.**__


__**Arguments:**__

__**Examples:**__

---

## swat query
*Return server information.*

__**Aliases:**__
*q, info, i*


__**Arguments:**__

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

__**Examples:**__

---

## swat serverlist
*Print the serverlist with current player numbers.*


__**Arguments:**__

__**Examples:**__

---

## swat servers
*SWAT4 serverlist manipulation commands.*

__**Aliases:**__
*s, srv*


__**Examples:**__

---

## swat settimeout
*Set checking timeout.*

__**Owner-only.**__


__**Arguments:**__

`[int]` : *Timeout (in ms).*

__**Examples:**__

---

## swat startcheck
*Notifies of free space in server.*

__**Aliases:**__
*checkspace, spacecheck*


__**Arguments:**__

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

__**Examples:**__

---

## swat stopcheck
*Stops space checking.*

__**Aliases:**__
*checkstop*


__**Arguments:**__

__**Examples:**__

---

## textreaction
*Text reaction handling.*

__**Aliases:**__
*treact, tr, txtr, textreactions*


__**Arguments:**__

`[string]` : *Trigger (case sensitive).*

`[string...]` : *Response.*

__**Examples:**__

---

## textreaction add
*Add text reaction to guild text reaction list.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*+, new*


__**Arguments:**__

`[string]` : *Trigger (case sensitive).*

`[string...]` : *Response.*

__**Examples:**__

---

## textreaction clear
*Delete all text reactions for the current guild.*

__**Requires user permissions:**__
*Administrator*

__**Aliases:**__
*c, da*


__**Arguments:**__

__**Examples:**__

---

## textreaction delete
*Remove text reaction from guild text reaction list.*

__**Requires user permissions:**__
*Manage guild*

__**Aliases:**__
*-, remove, del, rm, d*


__**Arguments:**__

`[string...]` : *Trigger words to remove.*

__**Examples:**__

---

## textreaction list
*Show all text reactions for the guild. Each page has 10 text reactions.*

__**Aliases:**__
*ls, l*


__**Arguments:**__

(optional) `[int]` : *Page.* (def: `1`)

__**Examples:**__

---

### tts
*Repeats after you but uses tts.*


__**Arguments:**__

`[string...]` : *Text.*

__**Examples:**__

---

### urbandict
*Search Urban Dictionary for a query.*

__**Aliases:**__
*ud, urban*


__**Arguments:**__

`[string...]` : *Query.*

__**Examples:**__

---

## user addrole
*Add a role to user.*

__**Requires permissions:**__
*Manage roles*

__**Aliases:**__
*+role, +r, ar*


__**Arguments:**__

`[member]` : *User.*

`[role]` : *Role.*

__**Examples:**__

---

## user avatar
*Get avatar from user.*

__**Aliases:**__
*a, pic*


__**Arguments:**__

`[user]` : *User.*

__**Examples:**__

---

## user ban
*Bans the user from the server.*

__**Requires permissions:**__
*Ban members*

__**Aliases:**__
*b*


__**Arguments:**__

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## user banid
*Bans the ID from the server.*

__**Requires permissions:**__
*Ban members*

__**Aliases:**__
*bid*


__**Arguments:**__

`[unsigned long]` : *ID.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## user deafen
*Toggle user's voice deafen state.*

__**Requires permissions:**__
*Deafen voice chat members*

__**Aliases:**__
*deaf, d*


__**Arguments:**__

`[member]` : *User*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## user info
*Print the user information.*

__**Aliases:**__
*i, information*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

__**Examples:**__

---

## user kick
*Kicks the user from server.*

__**Requires permissions:**__
*Kick members*

__**Aliases:**__
*k*


__**Arguments:**__

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## user listperms
*List user permissions.*

__**Aliases:**__
*permlist, perms, p*


__**Arguments:**__

(optional) `[member]` : *User.* (def: `None`)

__**Examples:**__

---

## user listroles
*List user permissions.*

__**Aliases:**__
*rolelist, roles, r*


__**Arguments:**__

(optional) `[member]` : *User.* (def: `None`)

__**Examples:**__

---

## user mute
*Toggle user mute.*

__**Requires permissions:**__
*Mute voice chat members*

__**Aliases:**__
*m*


__**Arguments:**__

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## user removeallroles
*Revoke all roles from user.*

__**Requires permissions:**__
*Manage roles*

__**Aliases:**__
*remallroles, -ra, -rall, -allr*


__**Arguments:**__

`[member]` : *User.*

__**Examples:**__

---

## user removerole
*Revoke a role from user.*

__**Requires permissions:**__
*Manage roles*

__**Aliases:**__
*remrole, rmrole, rr, -role, -r*


__**Arguments:**__

`[member]` : *User.*

`[role]` : *Role.*

__**Examples:**__

---

## user setname
*Gives someone a new nickname.*

__**Requires permissions:**__
*Manage nicknames*

__**Aliases:**__
*nick, newname, name, rename*


__**Arguments:**__

`[member]` : *User.*

(optional) `[string...]` : *New name.* (def: `None`)

__**Examples:**__

---

## user softban
*Bans the user from the server and then unbans him immediately.*

__**Requires permissions:**__
*Ban members*

__**Aliases:**__
*sb*


__**Arguments:**__

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## user tempban
*Temporarily ans the user from the server and then unbans him after given time.*

__**Requires permissions:**__
*Ban members*

__**Aliases:**__
*tb*


__**Arguments:**__

`[int]` : *Amount of time units.*

`[string]` : *Time unit.*

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## user unban
*Unbans the user from the server.*

__**Requires permissions:**__
*Ban members*

__**Aliases:**__
*ub*


__**Arguments:**__

`[unsigned long]` : *ID.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

---

## user warn
*Warn a user.*

__**Requires permissions:**__
*Kick members*

__**Aliases:**__
*w*


__**Arguments:**__

`[member]` : *User.*

(optional) `[string...]` : *Message.* (def: `None`)

__**Examples:**__

---

## youtube
*Youtube search commands.*

__**Aliases:**__
*y, yt*


__**Arguments:**__

`[string...]` : *Search query.*

__**Examples:**__

---

## youtube search
*Advanced youtube search.*

__**Aliases:**__
*s*


__**Arguments:**__

`[int]` : *Amount of results. [1-10]*

`[string...]` : *Search query.*

__**Examples:**__

---

## youtube searchc
*Advanced youtube search for channels only.*

__**Aliases:**__
*sc, searchchannel*


__**Arguments:**__

`[string...]` : *Search query.*

__**Examples:**__

---

## youtube searchp
*Advanced youtube search for playlists only.*

__**Aliases:**__
*sp, searchplaylist*


__**Arguments:**__

`[string...]` : *Search query.*

__**Examples:**__

---

## youtube searchv
*Advanced youtube search for videos only.*

__**Aliases:**__
*sv, searchvideo*


__**Arguments:**__

`[string...]` : *Search query.*

__**Examples:**__

---

### zugify
*I don't even...*

__**Aliases:**__
*z*


__**Arguments:**__

`[string...]` : *Text.*

__**Examples:**__

---

