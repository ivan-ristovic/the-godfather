# Command list

### 8ball
*An almighty ball which knows answer to everything.*


__**Arguments:**__

`[string...]` : *A question for the almighty ball.*

---

## admin botavatar
*Set bot avatar.*

__**Owner-only.**__

__**Aliases:**__

*setbotavatar, setavatar*


__**Arguments:**__

`[string]` : *URL.*

---

## admin botname
*Set bot name.*

__**Owner-only.**__

__**Aliases:**__

*setbotname, setname*


__**Arguments:**__

`[string...]` : *New name.*

---

## admin clearlog
*Clear application logs.*

__**Owner-only.**__

__**Aliases:**__

*clearlogs, deletelogs, deletelog*


__**Arguments:**__

---

## admin dbquery
*Clear application logs.*

__**Owner-only.**__

__**Aliases:**__

*sql, dbq, q*


__**Arguments:**__

`[string...]` : *SQL Query.*

---

## admin eval
*Evaluates a snippet of C# code, in context.*

__**Owner-only.**__

__**Aliases:**__

*compile, run, e, c, r*


__**Arguments:**__

`[string...]` : *Code to evaluate.*

---

## admin generatecommands
*Generates a command-list.*

__**Owner-only.**__

__**Aliases:**__

*cmdlist, gencmdlist, gencmds*


__**Arguments:**__

(optional) `[string...]` : *File path.* (def: `None`)

---

## admin leaveguilds
*Leave guilds given as IDs.*

__**Owner-only.**__


__**Arguments:**__

`[unsigned long...]` : *Guild ID list.*

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

---

## admin shutdown
*Triggers the dying in the vineyard scene.*

__**Owner-only.**__

__**Aliases:**__

*disable, poweroff, exit, quit*


__**Arguments:**__

---

## admin status
*Bot status manipulation.*

__**Owner-only.**__


## admin sudo
*Executes a command as another user.*

__**Owner-only.**__

__**Aliases:**__

*execas, as*


__**Arguments:**__

`[member]` : *Member to execute as.*

`[string...]` : *Command text to execute.*

---

## admin toggleignore
*Toggle bot's reaction to commands.*

__**Owner-only.**__

__**Aliases:**__

*ti*


__**Arguments:**__

---

## bank
*Bank manipulation.*

__**Aliases:**__

*$, $$, $$$*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

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

---

## bank register
*Create an account in WM bank.*

__**Aliases:**__

*r, signup, activate*


__**Arguments:**__

---

## bank status
*View account balance for user.*

__**Aliases:**__

*s, balance*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

---

## bank top
*Print the richest users.*

__**Aliases:**__

*leaderboard*


__**Arguments:**__

---

## bank transfer
*Transfer funds from one account to another.*

__**Aliases:**__

*lend*


__**Arguments:**__

`[user]` : *User to send credits to.*

`[int]` : *Amount.*

---

## cards draw
*Draw cards from the top of the deck.*

__**Aliases:**__

*take*


__**Arguments:**__

(optional) `[int]` : *Amount.* (def: `1`)

---

## cards reset
*Opens a brand new card deck.*

__**Aliases:**__

*new, opennew*


__**Arguments:**__

---

## cards shuffle
*Shuffle current deck.*

__**Aliases:**__

*s, sh, mix*


__**Arguments:**__

---

## channel createcategory
*Create new channel category.*

__**Requires permissions:**__

*Manage channels*

__**Aliases:**__

*createcat, createc, ccat, cc, +cat, +c, +category*


__**Arguments:**__

`[string...]` : *Name.*

---

## channel createtext
*Create new txt channel.*

__**Requires permissions:**__

*Manage channels*

__**Aliases:**__

*createtxt, createt, ctxt, ct, +, +t, +txt*


__**Overload 2:**__

`[string]` : *Name.*

(optional) `[channel]` : *Parent category.* (def: `None`)

(optional) `[boolean]` : *NSFW?* (def: `False`)

---

__**Overload 1:**__

`[string]` : *Name.*

(optional) `[boolean]` : *NSFW?* (def: `False`)

(optional) `[channel]` : *Parent category.* (def: `None`)

---

__**Overload 0:**__

`[channel]` : *Parent category.*

`[string]` : *Name.*

(optional) `[boolean]` : *NSFW?* (def: `False`)

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

---

__**Overload 1:**__

`[string]` : *Name.*

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

(optional) `[channel]` : *Parent category.* (def: `None`)

---

__**Overload 0:**__

`[channel]` : *Parent category.*

`[string]` : *Name.*

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

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

---

__**Overload 0:**__

`[string...]` : *Reason.*

---

## channel info
*Get information about a given channel.*

__**Requires permissions:**__

*Read messages*

__**Aliases:**__

*i, information*


__**Arguments:**__

(optional) `[channel]` : *Channel.* (def: `None`)

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

---

__**Overload 0:**__

(optional) `[int]` : *User limit.* (def: `0`)

(optional) `[int]` : *Bitrate.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

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

---

__**Overload 1:**__

`[channel]` : *Channel to rename.*

`[string...]` : *New name.*

---

__**Overload 0:**__

`[string...]` : *New name.*

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

---

__**Overload 0:**__

`[channel]` : *Parent category.*

(optional) `[string...]` : *Reason.* (def: `None`)

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

---

__**Overload 1:**__

`[int]` : *Position.*

`[channel]` : *Channel to reorder.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

__**Overload 0:**__

`[int]` : *Position.*

(optional) `[string...]` : *Reason.* (def: `None`)

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

---

__**Overload 1:**__

`[channel]` : *Channel.*

`[string...]` : *New Topic.*

---

__**Overload 0:**__

`[string...]` : *New Topic.*

---

### connect
*Connects me to a voice channel.*

__**Owner-only.**__

__**Requires permissions:**__

*Use voice chat*


__**Arguments:**__

(optional) `[channel]` : *Channel.* (def: `None`)

---

### disconnect
*Disconnects from voice channel.*

__**Owner-only.**__


__**Arguments:**__

---

### embed
*Embed an image given as an URL.*

__**Requires permissions:**__

*Attach files*


__**Arguments:**__

`[string]` : *Image URL.*

---

## emojireaction
*Emoji reaction handling.*

__**Aliases:**__

*ereact, er, emojir, emojireactions*


__**Arguments:**__

(optional) `[emoji]` : *Emoji to send.* (def: `None`)

`[string...]` : *Trigger word list.*

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

---

## emojireaction clear
*Delete all reactions for the current guild.*

__**Requires user permissions:**__

*Administrator*

__**Aliases:**__

*da, c*


__**Arguments:**__

---

## emojireaction delete
*Remove emoji reactions for given trigger words.*

__**Requires user permissions:**__

*Manage guild*

__**Aliases:**__

*-, remove, del, rm, d*


__**Arguments:**__

`[string...]` : *Trigger word list.*

---

## emojireaction list
*Show all emoji reactions.*

__**Aliases:**__

*ls, l*


__**Arguments:**__

(optional) `[int]` : *Page.* (def: `1`)

---

## filter add
*Add filter to guild filter list.*

__**Requires user permissions:**__

*Manage guild*

__**Aliases:**__

*+, new, a*


__**Arguments:**__

`[string...]` : *Filter. Can be a regex (case insensitive).*

---

## filter clear
*Delete all filters for the current guild.*

__**Requires user permissions:**__

*Administrator*

__**Aliases:**__

*c, da*


__**Arguments:**__

---

## filter delete
*Remove filter from guild filter list.*

__**Requires user permissions:**__

*Manage guild*

__**Aliases:**__

*-, remove, del*


__**Arguments:**__

`[string...]` : *Filter to remove.*

---

## filter list
*Show all filters for this guild.*

__**Aliases:**__

*ls, l*


__**Arguments:**__

(optional) `[int]` : *Page* (def: `1`)

---

## gamble coinflip
*Flips a coin.*

__**Aliases:**__

*coin, flip*


__**Arguments:**__

(optional) `[int]` : *Bid.* (def: `0`)

(optional) `[string]` : *Heads/Tails (h/t).* (def: `None`)

---

## gamble roll
*Rolls a dice.*

__**Aliases:**__

*dice, die*


__**Arguments:**__

(optional) `[int]` : *Bid.* (def: `0`)

(optional) `[int]` : *Number guess.* (def: `0`)

---

## gamble slot
*Roll a slot machine.*

__**Aliases:**__

*slotmachine*


__**Arguments:**__

(optional) `[int]` : *Bid.* (def: `5`)

---

## games caro
*Starts a caro game.*

__**Aliases:**__

*c*


__**Arguments:**__

---

## games connectfour
*Starts a "Connect4" game. Play by posting a number from 1 to 9 corresponding to the column you wish to place your move on.*

__**Aliases:**__

*connect4, chain4, chainfour, c4*


__**Arguments:**__

---

## games duel
*Starts a duel which I will commentate.*

__**Aliases:**__

*fight, vs, d*


__**Arguments:**__

`[user]` : *Who to fight with?*

---

## games hangman
*Starts a hangman game.*

__**Aliases:**__

*h, hang*


__**Arguments:**__

---

## games leaderboard
*Starts a hangman game.*

__**Aliases:**__

*globalstats*


__**Arguments:**__

---

## games nunchi
*Nunchi game commands*

__**Aliases:**__

*n*


__**Arguments:**__

---

## games quiz
*Start a quiz!*

__**Aliases:**__

*trivia, q*


## games race
*Racing!*

__**Aliases:**__

*r*


__**Arguments:**__

---

## games rps
*Rock, paper, scissors game.*

__**Aliases:**__

*rockpaperscissors*


__**Arguments:**__

---

## games stats
*Print game stats for given user.*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

---

## games tictactoe
*Starts a game of tic-tac-toe. Play by posting a number from 1 to 9 corresponding to field you wish to place your move on.*

__**Aliases:**__

*ttt*


__**Arguments:**__

---

## games typing
*Typing race.*

__**Aliases:**__

*type, typerace, typingrace*


__**Arguments:**__

---

## gif
*GIPHY commands.*

__**Aliases:**__

*giphy*


__**Arguments:**__

`[string...]` : *Query.*

---

## gif random
*Return a random GIF.*

__**Aliases:**__

*r, rand, rnd*


__**Arguments:**__

---

## gif trending
*Return an amount of trending GIFs.*

__**Aliases:**__

*t, tr, trend*


__**Arguments:**__

(optional) `[int]` : *Number of results (1-10).* (def: `5`)

---

### greet
*Greets a user and starts a conversation.*

__**Aliases:**__

*hello, hi, halo, hey, howdy, sup*


__**Arguments:**__

---

## guild bans
*Get guild ban list.*

__**Requires permissions:**__

*View audit log*

__**Aliases:**__

*banlist, viewbanlist, getbanlist, getbans, viewbans*


__**Arguments:**__

---

## guild deleteleavechannel
*Remove leave message channel for this guild.*

__**Requires user permissions:**__

*Manage guild*

__**Aliases:**__

*delleavec, dellc, delleave, dlc*


__**Arguments:**__

---

## guild deletewelcomechannel
*Remove welcome message channel for this guild.*

__**Requires user permissions:**__

*Manage guild*

__**Aliases:**__

*delwelcomec, delwc, delwelcome, dwc, deletewc*


__**Arguments:**__

---

## guild emoji
*Manipulate guild emoji.*

__**Aliases:**__

*emojis, e*


__**Arguments:**__

---

## guild getleavechannel
*Get current leave message channel for this guild.*

__**Requires user permissions:**__

*Manage guild*

__**Aliases:**__

*getleavec, getlc, getleave, leavechannel, lc*


__**Arguments:**__

---

## guild getwelcomechannel
*Get current welcome message channel for this guild.*

__**Requires user permissions:**__

*Manage guild*

__**Aliases:**__

*getwelcomec, getwc, getwelcome, welcomechannel, wc*


__**Arguments:**__

---

## guild info
*Get guild information.*

__**Aliases:**__

*i, information*


__**Arguments:**__

---

## guild listmembers
*Get guild member list.*

__**Aliases:**__

*memberlist, lm, members*


__**Arguments:**__

---

## guild log
*Get audit logs.*

__**Requires permissions:**__

*View audit log*

__**Aliases:**__

*auditlog, viewlog, getlog, getlogs, logs*


__**Arguments:**__

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

---

## guild rename
*Rename guild.*

__**Requires permissions:**__

*Manage guild*

__**Aliases:**__

*r, name, setname*


__**Arguments:**__

`[string...]` : *New name.*

---

## guild seticon
*Change icon of the guild.*

__**Requires permissions:**__

*Manage guild*

__**Aliases:**__

*icon, si*


__**Arguments:**__

`[string]` : *New icon URL.*

---

## guild setleavechannel
*Set leave message channel for this guild.*

__**Requires user permissions:**__

*Manage guild*

__**Aliases:**__

*leavec, setlc, setleave*


__**Arguments:**__

(optional) `[channel]` : *Channel.* (def: `None`)

---

## guild setwelcomechannel
*Set welcome message channel for this guild.*

__**Requires user permissions:**__

*Manage guild*

__**Aliases:**__

*setwc, setwelcomec, setwelcome*


__**Arguments:**__

(optional) `[channel]` : *Channel.* (def: `None`)

---

### help
*Displays command help.*


__**Arguments:**__

`[string...]` : *Command to provide help for.*

---

## imgur
*Search imgur. Invoking without sub command searches top.*

__**Aliases:**__

*img, im, i*


__**Arguments:**__

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Query.*

---

## imgur latest
*Return latest images for query.*

__**Aliases:**__

*l, new, newest*


__**Arguments:**__

`[int]` : *Number of images to print [1-10].*

`[string]` : *Query.*

---

## imgur top
*Return most rated images for query.*

__**Aliases:**__

*t*


__**Arguments:**__

(optional) `[string]` : *Time window (day/month/week/year/all).* (def: `day`)

(optional) `[int]` : *Number of images to print [1-10].* (def: `1`)

(optional) `[string...]` : *Query.* (def: `None`)

---

## insult
*Burns a user!*

__**Aliases:**__

*burn, insults*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

---

## insult add
*Add insult to list (Use % to code mention).*

__**Owner-only.**__

__**Aliases:**__

*+, new*


__**Arguments:**__

`[string...]` : *Response.*

---

## insult clear
*Delete all insults.*

__**Owner-only.**__

__**Aliases:**__

*clearall*


__**Arguments:**__

---

## insult delete
*Remove insult with a given index from list. (use ``!insults list`` to view indexes)*

__**Owner-only.**__

__**Aliases:**__

*-, remove, del, rm*


__**Arguments:**__

`[int]` : *Index.*

---

## insult list
*Show all insults.*


__**Arguments:**__

(optional) `[int]` : *Page.* (def: `1`)

---

### invite
*Get an instant invite link for the current channel.*

__**Requires permissions:**__

*Create instant invites*

__**Aliases:**__

*getinvite*


__**Arguments:**__

---

## joke
*Send a joke.*

__**Aliases:**__

*jokes, j*


__**Arguments:**__

---

## joke search
*Search for the joke containing the query.*

__**Aliases:**__

*s*


__**Arguments:**__

`[string...]` : *Query.*

---

## joke yourmom
*Yo mama so...*

__**Aliases:**__

*mama, m, yomomma, yomom, yomoma, yomamma, yomama*


__**Arguments:**__

---

### leave
*Makes Godfather leave the server.*

__**Requires user permissions:**__

*Kick members*


__**Arguments:**__

---

### leet
*Wr1t3s m3ss@g3 1n 1337sp34k.*


__**Arguments:**__

`[string...]` : *Text*

---

## meme
*Manipulate memes. When invoked without name, returns a random one.*

__**Aliases:**__

*memes, mm*


__**Arguments:**__

(optional) `[string...]` : *Meme name.* (def: `None`)

---

## meme add
*Add a new meme to the list.*

__**Owner-only.**__

__**Aliases:**__

*+, new, a*


__**Arguments:**__

`[string]` : *Short name (case insensitive).*

`[string]` : *URL.*

---

## meme create
*Creates a new meme from blank template.*

__**Aliases:**__

*maker, c, make, m*


__**Arguments:**__

`[string]` : *Template.*

`[string]` : *Top Text.*

`[string]` : *Bottom Text.*

---

## meme delete
*Deletes a meme from list.*

__**Owner-only.**__

__**Aliases:**__

*-, del, remove, rm, d*


__**Arguments:**__

`[string]` : *Short name (case insensitive).*

---

## meme list
*List all registered memes.*

__**Aliases:**__

*ls, l*


__**Arguments:**__

(optional) `[int]` : *Page.* (def: `1`)

---

## meme templates
*Manipulate meme templates.*

__**Aliases:**__

*template, t*


__**Arguments:**__

---

## message attachments
*Print all message attachments.*

__**Aliases:**__

*a, files, la*


__**Arguments:**__

(optional) `[unsigned long]` : *Message ID.* (def: `0`)

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

---

## message listpinned
*List latest amount of pinned messages.*

__**Aliases:**__

*lp, listpins, listpin, pinned*


__**Arguments:**__

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

---

## message pin
*Pins the last sent message. If the ID is given, pins that message.*

__**Requires permissions:**__

*Manage messages*

__**Aliases:**__

*p*


__**Arguments:**__

(optional) `[unsigned long]` : *ID.* (def: `0`)

---

## message unpin
*Unpins the message at given index (starting from 0).*

__**Requires permissions:**__

*Manage messages*

__**Aliases:**__

*up*


__**Arguments:**__

(optional) `[int]` : *Index (starting from 1).* (def: `1`)

---

## message unpinall
*Unpins all pinned messages.*

__**Requires permissions:**__

*Manage messages*

__**Aliases:**__

*upa*


__**Arguments:**__

---

### penis
*An accurate size of the user's manhood.*

__**Aliases:**__

*size, length, manhood, dick*


__**Arguments:**__

`[user]` : *Who to measure*

---

### ping
*Ping the bot.*


__**Arguments:**__

---

## play
*Plays a mp3 file from URL or server filesystem.*

__**Owner-only.**__

__**Aliases:**__

*music, p*


__**Arguments:**__

`[string...]` : *URL or YouTube search query.*

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

---

### poll
*Starts a poll in the channel.*

__**Aliases:**__

*vote*


__**Arguments:**__

`[string...]` : *Question.*

---

### pollr
*Starts a poll with reactions in the channel.*

__**Aliases:**__

*voter*


__**Arguments:**__

`[emoji...]` : *Options*

---

### prefix
*Get current guild prefix, or change it.*

__**Requires user permissions:**__

*Administrator*

__**Aliases:**__

*setprefix*


__**Arguments:**__

(optional) `[string]` : *Prefix to set.* (def: `None`)

---

## random cat
*Get a random cat image.*


__**Arguments:**__

---

## random choose
*!choose option1, option2, option3...*

__**Aliases:**__

*select*


__**Arguments:**__

`[string...]` : *Option list (separated with a comma).*

---

## random dog
*Get a random dog image.*


__**Arguments:**__

---

## random raffle
*Choose a user from the online members list belonging to a given role.*


__**Arguments:**__

(optional) `[role]` : *Role.* (def: `None`)

---

## rank
*User ranking commands.*

__**Aliases:**__

*ranks, ranking*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

---

## rank list
*Print all available ranks.*

__**Aliases:**__

*levels*


__**Arguments:**__

---

## rank top
*Get rank leaderboard.*


__**Arguments:**__

---

### rate
*An accurate graph of a user's humanity.*

__**Aliases:**__

*score, graph*


__**Arguments:**__

`[user]` : *Who to measure.*

---

### remind
*Resend a message after some time.*


__**Arguments:**__

`[int]` : *Time to wait before repeat (in seconds).*

`[string...]` : *What to repeat.*

---

### report
*Send a report message to owner about a bug (please don't abuse... please).*


__**Arguments:**__

`[string...]` : *Text.*

---

## roles
*Miscellaneous role control commands.*

__**Aliases:**__

*role, r, rl*


__**Arguments:**__

---

## roles create
*Create a new role.*

__**Requires permissions:**__

*Manage roles*

__**Aliases:**__

*new, add, +*


__**Arguments:**__

`[string...]` : *Role.*

---

## roles delete
*Create a new role.*

__**Requires permissions:**__

*Manage roles*

__**Aliases:**__

*del, remove, d, -, rm*


__**Arguments:**__

`[role]` : *Role.*

---

## roles mentionall
*Mention all users from given role.*

__**Requires permissions:**__

*Mention everyone*

__**Aliases:**__

*mention, @, ma*


__**Arguments:**__

`[role]` : *Role.*

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

---

## rss
*RSS feed operations.*

__**Aliases:**__

*feed*


__**Arguments:**__

`[string...]` : *URL.*

---

## rss listsubs
*Get feed list for the current channel.*

__**Aliases:**__

*ls, list*


__**Arguments:**__

---

## rss news
*Get newest world news.*


__**Arguments:**__

---

## rss reddit
*Reddit feed manipulation.*

__**Aliases:**__

*r*


__**Arguments:**__

(optional) `[string]` : *Subreddit.* (def: `all`)

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

---

## rss unsubscribe
*Remove an existing feed subscription.*

__**Requires permissions:**__

*Manage guild*

__**Aliases:**__

*del, d, rm, -, unsub*


__**Arguments:**__

`[int]` : *ID.*

---

## rss wm
*Get newest topics from WM forum.*


__**Arguments:**__

---

## rss youtube
*Youtube feed manipulation.*

__**Aliases:**__

*yt, y*


__**Arguments:**__

`[string]` : *Channel URL.*

---

### say
*Repeats after you.*

__**Aliases:**__

*repeat*


__**Arguments:**__

`[string...]` : *Text.*

---

## steam profile
*Get Steam user information from ID.*

__**Aliases:**__

*id*


__**Arguments:**__

`[unsigned long]` : *ID.*

---

### stop
*Stops current voice playback.*

__**Owner-only.**__


__**Arguments:**__

---

## swat query
*Return server information.*

__**Aliases:**__

*q, info, i*


__**Arguments:**__

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

---

## swat serverlist
*Print the serverlist with current player numbers.*


__**Arguments:**__

---

## swat servers
*SWAT4 serverlist manipulation commands.*

__**Aliases:**__

*s, srv*


## swat settimeout
*Set checking timeout.*

__**Owner-only.**__


__**Arguments:**__

`[int]` : *Timeout (in ms).*

---

## swat startcheck
*Notifies of free space in server.*

__**Aliases:**__

*checkspace, spacecheck*


__**Arguments:**__

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

---

## swat stopcheck
*Stops space checking.*

__**Aliases:**__

*checkstop*


__**Arguments:**__

---

## textreaction
*Text reaction handling.*

__**Aliases:**__

*treact, tr, txtr, textreactions*


__**Arguments:**__

`[string]` : *Trigger (case sensitive).*

`[string...]` : *Response.*

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

---

## textreaction clear
*Delete all text reactions for the current guild.*

__**Requires user permissions:**__

*Administrator*

__**Aliases:**__

*c, da*


__**Arguments:**__

---

## textreaction delete
*Remove text reaction from guild text reaction list.*

__**Requires user permissions:**__

*Manage guild*

__**Aliases:**__

*-, remove, del, rm, d*


__**Arguments:**__

`[string...]` : *Trigger words to remove.*

---

## textreaction list
*Show all text reactions for the guild. Each page has 10 text reactions.*

__**Aliases:**__

*ls, l*


__**Arguments:**__

(optional) `[int]` : *Page.* (def: `1`)

---

### tts
*Repeats after you but uses tts.*


__**Arguments:**__

`[string...]` : *Text.*

---

### urbandict
*Search Urban Dictionary for a query.*

__**Aliases:**__

*ud, urban*


__**Arguments:**__

`[string...]` : *Query.*

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

---

## user avatar
*Get avatar from user.*

__**Aliases:**__

*a, pic*


__**Arguments:**__

`[user]` : *User.*

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

---

## user info
*Print the user information.*

__**Aliases:**__

*i, information*


__**Arguments:**__

(optional) `[user]` : *User.* (def: `None`)

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

---

## user listperms
*List user permissions.*

__**Aliases:**__

*permlist, perms, p*


__**Arguments:**__

(optional) `[member]` : *User.* (def: `None`)

---

## user listroles
*List user permissions.*

__**Aliases:**__

*rolelist, roles, r*


__**Arguments:**__

(optional) `[member]` : *User.* (def: `None`)

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

---

## user removeallroles
*Revoke all roles from user.*

__**Requires permissions:**__

*Manage roles*

__**Aliases:**__

*remallroles, -ra, -rall, -allr*


__**Arguments:**__

`[member]` : *User.*

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

---

## youtube
*Youtube search commands.*

__**Aliases:**__

*y, yt*


__**Arguments:**__

`[string...]` : *Search query.*

---

## youtube search
*Advanced youtube search.*

__**Aliases:**__

*s*


__**Arguments:**__

`[int]` : *Amount of results. [1-10]*

`[string...]` : *Search query.*

---

## youtube searchc
*Advanced youtube search for channels only.*

__**Aliases:**__

*sc, searchchannel*


__**Arguments:**__

`[string...]` : *Search query.*

---

## youtube searchp
*Advanced youtube search for playlists only.*

__**Aliases:**__

*sp, searchplaylist*


__**Arguments:**__

`[string...]` : *Search query.*

---

## youtube searchv
*Advanced youtube search for videos only.*

__**Aliases:**__

*sv, searchvideo*


__**Arguments:**__

`[string...]` : *Search query.*

---

### zugify
*I don't even...*

__**Aliases:**__

*z*


__**Arguments:**__

`[string...]` : *Text.*

---

