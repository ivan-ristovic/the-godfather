# Command list

### 8ball
*An almighty ball which knows answer to everything.*


**Arguments:**

`[string...]` : *A question for the almighty ball.*

---

### admin botavatar
*Set bot avatar.*

**Aliases:**

*setbotavatar, setavatar*


**Arguments:**

`[string]` : *URL.*

---

### admin botname
*Set bot name.*

**Aliases:**

*setbotname, setname*


**Arguments:**

`[string...]` : *New name.*

---

### admin clearlog
*Clear application logs.*

**Aliases:**

*clearlogs, deletelogs, deletelog*


**Arguments:**

---

### admin dbquery
*Clear application logs.*

**Aliases:**

*sql, dbq, q*


**Arguments:**

`[string...]` : *SQL Query.*

---

### admin eval
*Evaluates a snippet of C# code, in context.*

**Aliases:**

*compile, run, e, c, r*


**Arguments:**

`[string...]` : *Code to evaluate.*

---

### admin generatecommands
*Generates a command-list.*

**Aliases:**

*cmdlist, gencmdlist, gencmds*


**Arguments:**

(optional) `[string...]` : *File path.* (def: `None`)

---

### admin leaveguilds
*Leave guilds given as IDs.*


**Arguments:**

`[unsigned long...]` : *Guild ID list.*

---

### admin sendmessage
*Sends a message to a user or channel.*

**Aliases:**

*send*


**Arguments:**

`[string]` : *u/c (for user or channel.)*

`[unsigned long]` : *User/Channel ID.*

`[string...]` : *Message.*

---

### admin shutdown
*Triggers the dying in the vineyard scene.*

**Aliases:**

*disable, poweroff, exit, quit*


**Arguments:**

---

## admin status
*Bot status manipulation.*


### admin sudo
*Executes a command as another user.*

**Aliases:**

*execas, as*


**Arguments:**

`[member]` : *Member to execute as.*

`[string...]` : *Command text to execute.*

---

### admin toggleignore
*Toggle bot's reaction to commands.*

**Aliases:**

*ti*


**Arguments:**

---

## bank
*Bank manipulation.*

**Aliases:**

*$, $$, $$$*


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

---

### bank grant
*Magically give funds to a user.*

**Aliases:**

*give*


**Arguments:**

`[user]` : *User.*

`[int]` : *Amount.*

---

### bank register
*Create an account in WM bank.*

**Aliases:**

*r, signup, activate*


**Arguments:**

---

### bank status
*View account balance for user.*

**Aliases:**

*s, balance*


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

---

### bank top
*Print the richest users.*

**Aliases:**

*leaderboard*


**Arguments:**

---

### bank transfer
*Transfer funds from one account to another.*

**Aliases:**

*lend*


**Arguments:**

`[user]` : *User to send credits to.*

`[int]` : *Amount.*

---

### cards draw
*Draw cards from the top of the deck.*

**Aliases:**

*take*


**Arguments:**

(optional) `[int]` : *Amount.* (def: `1`)

---

### cards reset
*Opens a brand new card deck.*

**Aliases:**

*new, opennew*


**Arguments:**

---

### cards shuffle
*Shuffle current deck.*

**Aliases:**

*s, sh, mix*


**Arguments:**

---

### channel createcategory
*Create new channel category.*

**Aliases:**

*createcat, createc, ccat, cc, +cat, +c, +category*


**Arguments:**

`[string...]` : *Name.*

---

### channel createtext
*Create new txt channel.*

**Aliases:**

*createtxt, createt, ctxt, ct, +, +t, +txt*


**Overload 2:**

`[string]` : *Name.*

(optional) `[channel]` : *Parent category.* (def: `None`)

(optional) `[boolean]` : *NSFW?* (def: `False`)

---

**Overload 1:**

`[string]` : *Name.*

(optional) `[boolean]` : *NSFW?* (def: `False`)

(optional) `[channel]` : *Parent category.* (def: `None`)

---

**Overload 0:**

`[channel]` : *Parent category.*

`[string]` : *Name.*

(optional) `[boolean]` : *NSFW?* (def: `False`)

---

### channel createvoice
*Create new voice channel.*

**Aliases:**

*createv, cvoice, cv, +voice, +v*


**Overload 2:**

`[string]` : *Name.*

(optional) `[channel]` : *Parent category.* (def: `None`)

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

---

**Overload 1:**

`[string]` : *Name.*

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

(optional) `[channel]` : *Parent category.* (def: `None`)

---

**Overload 0:**

`[channel]` : *Parent category.*

`[string]` : *Name.*

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

---

### channel delete
*Delete a given channel or category.*

**Aliases:**

*-, del, d, remove, rm*


**Overload 1:**

(optional) `[channel]` : *Channel to delete.* (def: `None`)

(optional) `[string...]` : *Reason.* (def: `None`)

---

**Overload 0:**

`[string...]` : *Reason.*

---

### channel info
*Get information about a given channel.*

**Aliases:**

*i, information*


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

---

### channel modify
*Modify a given voice channel. Set 0 if you wish to keep the value as it is.*

**Aliases:**

*edit, mod, m, e*


**Overload 1:**

`[channel]` : *Voice channel to edit*

(optional) `[int]` : *User limit.* (def: `0`)

(optional) `[int]` : *Bitrate.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

---

**Overload 0:**

(optional) `[int]` : *User limit.* (def: `0`)

(optional) `[int]` : *Bitrate.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

---

### channel rename
*Rename channel.*

**Aliases:**

*r, name, setname*


**Overload 2:**

`[string]` : *Reason.*

`[channel]` : *Channel to rename.*

`[string...]` : *New name.*

---

**Overload 1:**

`[channel]` : *Channel to rename.*

`[string...]` : *New name.*

---

**Overload 0:**

`[string...]` : *New name.*

---

### channel setparent
*Change the parent of the given channel.*

**Aliases:**

*setpar, par, parent*


**Overload 1:**

`[channel]` : *Child channel.*

`[channel]` : *Parent category.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

**Overload 0:**

`[channel]` : *Parent category.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

### channel setposition
*Change the position of the given channel in the guild channel list.*

**Aliases:**

*setpos, pos, position*


**Overload 2:**

`[channel]` : *Channel to reorder.*

`[int]` : *Position.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

**Overload 1:**

`[int]` : *Position.*

`[channel]` : *Channel to reorder.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

**Overload 0:**

`[int]` : *Position.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

### channel settopic
*Set channel topic.*

**Aliases:**

*t, topic, sett*


**Overload 2:**

`[string]` : *Reason.*

`[channel]` : *Channel.*

`[string...]` : *New topic.*

---

**Overload 1:**

`[channel]` : *Channel.*

`[string...]` : *New Topic.*

---

**Overload 0:**

`[string...]` : *New Topic.*

---

### connect
*Connects me to a voice channel.*


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

---

### disconnect
*Disconnects from voice channel.*


**Arguments:**

---

### embed
*Embed an image given as an URL.*


**Arguments:**

`[string]` : *Image URL.*

---

## emojireaction
*Emoji reaction handling.*

**Aliases:**

*ereact, er, emojir, emojireactions*


**Arguments:**

(optional) `[emoji]` : *Emoji to send.* (def: `None`)

`[string...]` : *Trigger word list.*

---

### emojireaction add
*Add emoji reactions to guild reaction list.*

**Aliases:**

*+, new*


**Arguments:**

`[emoji]` : *Emoji to send.*

`[string...]` : *Trigger word list.*

---

### emojireaction clear
*Delete all reactions for the current guild.*

**Aliases:**

*da, c*


**Arguments:**

---

### emojireaction delete
*Remove emoji reactions for given trigger words.*

**Aliases:**

*-, remove, del, rm, d*


**Arguments:**

`[string...]` : *Trigger word list.*

---

### emojireaction list
*Show all emoji reactions.*

**Aliases:**

*ls, l*


**Arguments:**

(optional) `[int]` : *Page.* (def: `1`)

---

### filter add
*Add filter to guild filter list.*

**Aliases:**

*+, new, a*


**Arguments:**

`[string...]` : *Filter. Can be a regex (case insensitive).*

---

### filter clear
*Delete all filters for the current guild.*

**Aliases:**

*c, da*


**Arguments:**

---

### filter delete
*Remove filter from guild filter list.*

**Aliases:**

*-, remove, del*


**Arguments:**

`[string...]` : *Filter to remove.*

---

### filter list
*Show all filters for this guild.*

**Aliases:**

*ls, l*


**Arguments:**

(optional) `[int]` : *Page* (def: `1`)

---

### gamble coinflip
*Flips a coin.*

**Aliases:**

*coin, flip*


**Arguments:**

(optional) `[int]` : *Bid.* (def: `0`)

(optional) `[string]` : *Heads/Tails (h/t).* (def: `None`)

---

### gamble roll
*Rolls a dice.*

**Aliases:**

*dice, die*


**Arguments:**

(optional) `[int]` : *Bid.* (def: `0`)

(optional) `[int]` : *Number guess.* (def: `0`)

---

### gamble slot
*Roll a slot machine.*

**Aliases:**

*slotmachine*


**Arguments:**

(optional) `[int]` : *Bid.* (def: `5`)

---

### games caro
*Starts a caro game.*

**Aliases:**

*c*


**Arguments:**

---

### games connectfour
*Starts a "Connect4" game. Play by posting a number from 1 to 9 corresponding to the column you wish to place your move on.*

**Aliases:**

*connect4, chain4, chainfour, c4*


**Arguments:**

---

### games duel
*Starts a duel which I will commentate.*

**Aliases:**

*fight, vs, d*


**Arguments:**

`[user]` : *Who to fight with?*

---

### games hangman
*Starts a hangman game.*

**Aliases:**

*h, hang*


**Arguments:**

---

### games leaderboard
*Starts a hangman game.*

**Aliases:**

*globalstats*


**Arguments:**

---

## games nunchi
*Nunchi game commands*

**Aliases:**

*n*


**Arguments:**

---

## games quiz
*Start a quiz!*

**Aliases:**

*trivia, q*


## games race
*Racing!*

**Aliases:**

*r*


**Arguments:**

---

### games rps
*Rock, paper, scissors game.*

**Aliases:**

*rockpaperscissors*


**Arguments:**

---

### games stats
*Print game stats for given user.*


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

---

### games tictactoe
*Starts a game of tic-tac-toe. Play by posting a number from 1 to 9 corresponding to field you wish to place your move on.*

**Aliases:**

*ttt*


**Arguments:**

---

### games typing
*Typing race.*

**Aliases:**

*type, typerace, typingrace*


**Arguments:**

---

## gif
*GIPHY commands.*

**Aliases:**

*giphy*


**Arguments:**

`[string...]` : *Query.*

---

### gif random
*Return a random GIF.*

**Aliases:**

*r, rand, rnd*


**Arguments:**

---

### gif trending
*Return an amount of trending GIFs.*

**Aliases:**

*t, tr, trend*


**Arguments:**

(optional) `[int]` : *Number of results (1-10).* (def: `5`)

---

### greet
*Greets a user and starts a conversation.*

**Aliases:**

*hello, hi, halo, hey, howdy, sup*


**Arguments:**

---

### guild bans
*Get guild ban list.*

**Aliases:**

*banlist, viewbanlist, getbanlist, getbans, viewbans*


**Arguments:**

---

### guild deleteleavechannel
*Remove leave message channel for this guild.*

**Aliases:**

*delleavec, dellc, delleave, dlc*


**Arguments:**

---

### guild deletewelcomechannel
*Remove welcome message channel for this guild.*

**Aliases:**

*delwelcomec, delwc, delwelcome, dwc, deletewc*


**Arguments:**

---

## guild emoji
*Manipulate guild emoji.*

**Aliases:**

*emojis, e*


**Arguments:**

---

### guild getleavechannel
*Get current leave message channel for this guild.*

**Aliases:**

*getleavec, getlc, getleave, leavechannel, lc*


**Arguments:**

---

### guild getwelcomechannel
*Get current welcome message channel for this guild.*

**Aliases:**

*getwelcomec, getwc, getwelcome, welcomechannel, wc*


**Arguments:**

---

### guild info
*Get guild information.*

**Aliases:**

*i, information*


**Arguments:**

---

### guild listmembers
*Get guild member list.*

**Aliases:**

*memberlist, lm, members*


**Arguments:**

---

### guild log
*Get audit logs.*

**Aliases:**

*auditlog, viewlog, getlog, getlogs, logs*


**Arguments:**

---

### guild prune
*Kick guild members who weren't active in given amount of days (1-7).*

**Aliases:**

*p, clean*


**Arguments:**

(optional) `[int]` : *Days.* (def: `7`)

(optional) `[string...]` : *Reason.* (def: `None`)

---

### guild rename
*Rename guild.*

**Aliases:**

*r, name, setname*


**Arguments:**

`[string...]` : *New name.*

---

### guild seticon
*Change icon of the guild.*

**Aliases:**

*icon, si*


**Arguments:**

`[string]` : *New icon URL.*

---

### guild setleavechannel
*Set leave message channel for this guild.*

**Aliases:**

*leavec, setlc, setleave*


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

---

### guild setwelcomechannel
*Set welcome message channel for this guild.*

**Aliases:**

*setwc, setwelcomec, setwelcome*


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

---

### help
*Displays command help.*


**Arguments:**

`[string...]` : *Command to provide help for.*

---

## imgur
*Search imgur. Invoking without sub command searches top.*

**Aliases:**

*img, im, i*


**Arguments:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Query.*

---

### imgur latest
*Return latest images for query.*

**Aliases:**

*l, new, newest*


**Arguments:**

`[int]` : *Number of images to print [1-10].*

`[string]` : *Query.*

---

### imgur top
*Return most rated images for query.*

**Aliases:**

*t*


**Arguments:**

(optional) `[string]` : *Time window (day/month/week/year/all).* (def: `day`)

(optional) `[int]` : *Number of images to print [1-10].* (def: `1`)

(optional) `[string...]` : *Query.* (def: `None`)

---

## insult
*Burns a user!*

**Aliases:**

*burn, insults*


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

---

### insult add
*Add insult to list (Use % to code mention).*

**Aliases:**

*+, new*


**Arguments:**

`[string...]` : *Response.*

---

### insult clear
*Delete all insults.*

**Aliases:**

*clearall*


**Arguments:**

---

### insult delete
*Remove insult with a given index from list. (use ``!insults list`` to view indexes)*

**Aliases:**

*-, remove, del, rm*


**Arguments:**

`[int]` : *Index.*

---

### insult list
*Show all insults.*


**Arguments:**

(optional) `[int]` : *Page.* (def: `1`)

---

### invite
*Get an instant invite link for the current channel.*

**Aliases:**

*getinvite*


**Arguments:**

---

## joke
*Send a joke.*

**Aliases:**

*jokes, j*


**Arguments:**

---

### joke search
*Search for the joke containing the query.*

**Aliases:**

*s*


**Arguments:**

`[string...]` : *Query.*

---

### joke yourmom
*Yo mama so...*

**Aliases:**

*mama, m, yomomma, yomom, yomoma, yomamma, yomama*


**Arguments:**

---

### leave
*Makes Godfather leave the server.*


**Arguments:**

---

### leet
*Wr1t3s m3ss@g3 1n 1337sp34k.*


**Arguments:**

`[string...]` : *Text*

---

## meme
*Manipulate memes. When invoked without name, returns a random one.*

**Aliases:**

*memes, mm*


**Arguments:**

(optional) `[string...]` : *Meme name.* (def: `None`)

---

### meme add
*Add a new meme to the list.*

**Aliases:**

*+, new, a*


**Arguments:**

`[string]` : *Short name (case insensitive).*

`[string]` : *URL.*

---

### meme create
*Creates a new meme from blank template.*

**Aliases:**

*maker, c, make, m*


**Arguments:**

`[string]` : *Template.*

`[string]` : *Top Text.*

`[string]` : *Bottom Text.*

---

### meme delete
*Deletes a meme from list.*

**Aliases:**

*-, del, remove, rm, d*


**Arguments:**

`[string]` : *Short name (case insensitive).*

---

### meme list
*List all registered memes.*

**Aliases:**

*ls, l*


**Arguments:**

(optional) `[int]` : *Page.* (def: `1`)

---

## meme templates
*Manipulate meme templates.*

**Aliases:**

*template, t*


**Arguments:**

---

### message attachments
*Print all message attachments.*

**Aliases:**

*a, files, la*


**Arguments:**

(optional) `[unsigned long]` : *Message ID.* (def: `0`)

---

### message delete
*Deletes the specified amount of most-recent messages from the channel.*

**Aliases:**

*-, prune, del, d*


**Arguments:**

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

---

### message deletefrom
*Deletes given amount of most-recent messages from given user.*

**Aliases:**

*-user, -u, deluser, du, dfu, delfrom*


**Arguments:**

`[user]` : *User.*

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

---

### message deletereactions
*Deletes all reactions from the given message.*

**Aliases:**

*-reactions, -r, delreactions, dr*


**Arguments:**

(optional) `[unsigned long]` : *ID.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

---

### message deleteregex
*Deletes given amount of most-recent messages that match a given regular expression.*

**Aliases:**

*-regex, -rx, delregex, drx*


**Arguments:**

`[string]` : *Pattern (Regex).*

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

---

### message listpinned
*List latest amount of pinned messages.*

**Aliases:**

*lp, listpins, listpin, pinned*


**Arguments:**

---

### message modify
*Modify the given message.*

**Aliases:**

*edit, mod, e, m*


**Arguments:**

`[unsigned long]` : *Message ID.*

`[string...]` : *New content.*

---

### message pin
*Pins the last sent message. If the ID is given, pins that message.*

**Aliases:**

*p*


**Arguments:**

(optional) `[unsigned long]` : *ID.* (def: `0`)

---

### message unpin
*Unpins the message at given index (starting from 0).*

**Aliases:**

*up*


**Arguments:**

(optional) `[int]` : *Index (starting from 1).* (def: `1`)

---

### message unpinall
*Unpins all pinned messages.*

**Aliases:**

*upa*


**Arguments:**

---

### penis
*An accurate size of the user's manhood.*

**Aliases:**

*size, length, manhood, dick*


**Arguments:**

`[user]` : *Who to measure*

---

### ping
*Ping the bot.*


**Arguments:**

---

## play
*Plays a mp3 file from URL or server filesystem.*

**Aliases:**

*music, p*


**Arguments:**

`[string...]` : *URL or YouTube search query.*

---

### play file
*Plays an audio file from server filesystem.*

**Aliases:**

*f*


**Arguments:**

`[string...]` : *Full path to the file to play.*

---

### poll
*Starts a poll in the channel.*

**Aliases:**

*vote*


**Arguments:**

`[string...]` : *Question.*

---

### pollr
*Starts a poll with reactions in the channel.*

**Aliases:**

*voter*


**Arguments:**

`[emoji...]` : *Options*

---

### prefix
*Get current guild prefix, or change it.*

**Aliases:**

*setprefix*


**Arguments:**

(optional) `[string]` : *Prefix to set.* (def: `None`)

---

### random cat
*Get a random cat image.*


**Arguments:**

---

### random choose
*!choose option1, option2, option3...*

**Aliases:**

*select*


**Arguments:**

`[string...]` : *Option list (separated with a comma).*

---

### random dog
*Get a random dog image.*


**Arguments:**

---

### random raffle
*Choose a user from the online members list belonging to a given role.*


**Arguments:**

(optional) `[role]` : *Role.* (def: `None`)

---

## rank
*User ranking commands.*

**Aliases:**

*ranks, ranking*


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

---

### rank list
*Print all available ranks.*

**Aliases:**

*levels*


**Arguments:**

---

### rank top
*Get rank leaderboard.*


**Arguments:**

---

### rate
*An accurate graph of a user's humanity.*

**Aliases:**

*score, graph*


**Arguments:**

`[user]` : *Who to measure.*

---

### remind
*Resend a message after some time.*


**Arguments:**

`[int]` : *Time to wait before repeat (in seconds).*

`[string...]` : *What to repeat.*

---

### report
*Send a report message to owner about a bug (please don't abuse... please).*


**Arguments:**

`[string...]` : *Text.*

---

## roles
*Miscellaneous role control commands.*

**Aliases:**

*role, r, rl*


**Arguments:**

---

### roles create
*Create a new role.*

**Aliases:**

*new, add, +*


**Arguments:**

`[string...]` : *Role.*

---

### roles delete
*Create a new role.*

**Aliases:**

*del, remove, d, -, rm*


**Arguments:**

`[role]` : *Role.*

---

### roles mentionall
*Mention all users from given role.*

**Aliases:**

*mention, @, ma*


**Arguments:**

`[role]` : *Role.*

---

### roles setcolor
*Set a color for the role.*

**Aliases:**

*clr, c, sc*


**Arguments:**

`[role]` : *Role.*

`[string]` : *Color.*

---

### roles setmentionable
*Set role mentionable var.*

**Aliases:**

*mentionable, m, setm*


**Arguments:**

`[role]` : *Role.*

`[boolean]` : *[true/false]*

---

### roles setname
*Set a name for the role.*

**Aliases:**

*name, rename, n*


**Arguments:**

`[role]` : *Role.*

`[string...]` : *New name.*

---

### roles setvisible
*Set role hoist var (visibility in online list.*

**Aliases:**

*separate, h, seth, hoist, sethoist*


**Arguments:**

`[role]` : *Role.*

`[boolean]` : *[true/false]*

---

## rss
*RSS feed operations.*

**Aliases:**

*feed*


**Arguments:**

`[string...]` : *URL.*

---

### rss listsubs
*Get feed list for the current channel.*

**Aliases:**

*ls, list*


**Arguments:**

---

### rss news
*Get newest world news.*


**Arguments:**

---

## rss reddit
*Reddit feed manipulation.*

**Aliases:**

*r*


**Arguments:**

(optional) `[string]` : *Subreddit.* (def: `all`)

---

### rss subscribe
*Subscribe to given url.*

**Aliases:**

*sub, add, +*


**Arguments:**

`[string...]` : *URL.*

(optional) `[string]` : *Friendly name.* (def: `None`)

---

### rss unsubscribe
*Remove an existing feed subscription.*

**Aliases:**

*del, d, rm, -, unsub*


**Arguments:**

`[int]` : *ID.*

---

### rss wm
*Get newest topics from WM forum.*


**Arguments:**

---

## rss youtube
*Youtube feed manipulation.*

**Aliases:**

*yt, y*


**Arguments:**

`[string]` : *Channel URL.*

---

### say
*Repeats after you.*

**Aliases:**

*repeat*


**Arguments:**

`[string...]` : *Text.*

---

### steam profile
*Get Steam user information from ID.*

**Aliases:**

*id*


**Arguments:**

`[unsigned long]` : *ID.*

---

### stop
*Stops current voice playback.*


**Arguments:**

---

### swat query
*Return server information.*

**Aliases:**

*q, info, i*


**Arguments:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

---

### swat serverlist
*Print the serverlist with current player numbers.*


**Arguments:**

---

## swat servers
*SWAT4 serverlist manipulation commands.*

**Aliases:**

*s, srv*


### swat settimeout
*Set checking timeout.*


**Arguments:**

`[int]` : *Timeout (in ms).*

---

### swat startcheck
*Notifies of free space in server.*

**Aliases:**

*checkspace, spacecheck*


**Arguments:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

---

### swat stopcheck
*Stops space checking.*

**Aliases:**

*checkstop*


**Arguments:**

---

## textreaction
*Text reaction handling.*

**Aliases:**

*treact, tr, txtr, textreactions*


**Arguments:**

`[string]` : *Trigger (case sensitive).*

`[string...]` : *Response.*

---

### textreaction add
*Add text reaction to guild text reaction list.*

**Aliases:**

*+, new*


**Arguments:**

`[string]` : *Trigger (case sensitive).*

`[string...]` : *Response.*

---

### textreaction clear
*Delete all text reactions for the current guild.*

**Aliases:**

*c, da*


**Arguments:**

---

### textreaction delete
*Remove text reaction from guild text reaction list.*

**Aliases:**

*-, remove, del, rm, d*


**Arguments:**

`[string...]` : *Trigger words to remove.*

---

### textreaction list
*Show all text reactions for the guild. Each page has 10 text reactions.*

**Aliases:**

*ls, l*


**Arguments:**

(optional) `[int]` : *Page.* (def: `1`)

---

### tts
*Repeats after you but uses tts.*


**Arguments:**

`[string...]` : *Text.*

---

### urbandict
*Search Urban Dictionary for a query.*

**Aliases:**

*ud, urban*


**Arguments:**

`[string...]` : *Query.*

---

### user addrole
*Add a role to user.*

**Aliases:**

*+role, +r, ar*


**Arguments:**

`[member]` : *User.*

`[role]` : *Role.*

---

### user avatar
*Get avatar from user.*

**Aliases:**

*a, pic*


**Arguments:**

`[user]` : *User.*

---

### user ban
*Bans the user from the server.*

**Aliases:**

*b*


**Arguments:**

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

### user banid
*Bans the ID from the server.*

**Aliases:**

*bid*


**Arguments:**

`[unsigned long]` : *ID.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

### user deafen
*Toggle user's voice deafen state.*

**Aliases:**

*deaf, d*


**Arguments:**

`[member]` : *User*

(optional) `[string...]` : *Reason.* (def: `None`)

---

### user info
*Print the user information.*

**Aliases:**

*i, information*


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

---

### user kick
*Kicks the user from server.*

**Aliases:**

*k*


**Arguments:**

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

### user listperms
*List user permissions.*

**Aliases:**

*permlist, perms, p*


**Arguments:**

(optional) `[member]` : *User.* (def: `None`)

---

### user listroles
*List user permissions.*

**Aliases:**

*rolelist, roles, r*


**Arguments:**

(optional) `[member]` : *User.* (def: `None`)

---

### user mute
*Toggle user mute.*

**Aliases:**

*m*


**Arguments:**

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

### user removeallroles
*Revoke all roles from user.*

**Aliases:**

*remallroles, -ra, -rall, -allr*


**Arguments:**

`[member]` : *User.*

---

### user removerole
*Revoke a role from user.*

**Aliases:**

*remrole, rmrole, rr, -role, -r*


**Arguments:**

`[member]` : *User.*

`[role]` : *Role.*

---

### user setname
*Gives someone a new nickname.*

**Aliases:**

*nick, newname, name, rename*


**Arguments:**

`[member]` : *User.*

(optional) `[string...]` : *New name.* (def: `None`)

---

### user softban
*Bans the user from the server and then unbans him immediately.*

**Aliases:**

*sb*


**Arguments:**

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

### user tempban
*Temporarily ans the user from the server and then unbans him after given time.*

**Aliases:**

*tb*


**Arguments:**

`[int]` : *Amount of time units.*

`[string]` : *Time unit.*

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

### user unban
*Unbans the user from the server.*

**Aliases:**

*ub*


**Arguments:**

`[unsigned long]` : *ID.*

(optional) `[string...]` : *Reason.* (def: `None`)

---

### user warn
*Warn a user.*

**Aliases:**

*w*


**Arguments:**

`[member]` : *User.*

(optional) `[string...]` : *Message.* (def: `None`)

---

## youtube
*Youtube search commands.*

**Aliases:**

*y, yt*


**Arguments:**

`[string...]` : *Search query.*

---

### youtube search
*Advanced youtube search.*

**Aliases:**

*s*


**Arguments:**

`[int]` : *Amount of results. [1-10]*

`[string...]` : *Search query.*

---

### youtube searchc
*Advanced youtube search for channels only.*

**Aliases:**

*sc, searchchannel*


**Arguments:**

`[string...]` : *Search query.*

---

### youtube searchp
*Advanced youtube search for playlists only.*

**Aliases:**

*sp, searchplaylist*


**Arguments:**

`[string...]` : *Search query.*

---

### youtube searchv
*Advanced youtube search for videos only.*

**Aliases:**

*sv, searchvideo*


**Arguments:**

`[string...]` : *Search query.*

---

### zugify
*I don't even...*

**Aliases:**

*z*


**Arguments:**

`[string...]` : *Text.*

---

