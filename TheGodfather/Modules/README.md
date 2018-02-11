# Command list

## 8ball
*An almighty ball which knows answer to everything.*


**Arguments:**

`[string...]` : *A question for the almighty ball.*

---

## bank
*Bank manipulation.*

**Aliases:**
`$, $$, $$$`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

---

### bank balance
*View account balance for given user. If the user is no given, checks sender's balance.*

**Aliases:**
`s, status, bal, money, credits`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

__**Examples:**__

`!bank balance @Someone`

---

### bank grant
*Magically give funds to a user.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`give`


**Overload 1:**

`[user]` : *User.*

`[int]` : *Amount.*

**Overload 0:**

`[int]` : *Amount.*

`[user]` : *User.*

__**Examples:**__

`!bank grant @Someone 1000`

`!bank grant 1000 @Someone`

---

### bank register
*Create an account in WM bank.*

**Aliases:**
`r, signup, activate`


__**Examples:**__

`!bank register`

---

### bank top
*Print the richest users.*

**Aliases:**
`leaderboard, elite`


__**Examples:**__

`!bank top`

---

### bank transfer
*Transfer funds from one account to another.*

**Aliases:**
`lend`


**Overload 1:**

`[user]` : *User to send credits to.*

`[int]` : *Amount.*

**Overload 0:**

`[int]` : *Amount.*

`[user]` : *User to send credits to.*

__**Examples:**__

`!bank transfer @Someone 40`

`!bank transfer 40 @Someone`

---

## cards
*Manipulate a deck of cards.*

**Aliases:**
`deck`


---

### cards draw
*Draw cards from the top of the deck. If amount of cards is not specified, draws one card.*

**Aliases:**
`take`


**Arguments:**

(optional) `[int]` : *Amount (in range [1-10]).* (def: `1`)

__**Examples:**__

`!deck draw 5`

---

### cards reset
*Opens a brand new card deck.*

**Aliases:**
`new, opennew, open`


__**Examples:**__

`!deck draw 5`

---

### cards shuffle
*Shuffles current deck.*

**Aliases:**
`s, sh, mix`


__**Examples:**__

`!deck shuffle`

---

### channel createcategory
*Create new channel category.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`createcat, createc, ccat, cc, +cat, +c, +category`


**Arguments:**

`[string...]` : *Name.*

__**Examples:**__

`!channel createcategory My New Category`

---

### channel createtext
*Create new text channel.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`createtxt, createt, ctxt, ct, +, +t, +txt`


**Overload 2:**

`[string]` : *Name.*

(optional) `[channel]` : *Parent category.* (def: `None`)

(optional) `[boolean]` : *NSFW?* (def: `False`)

**Overload 1:**

`[string]` : *Name.*

(optional) `[boolean]` : *NSFW?* (def: `False`)

(optional) `[channel]` : *Parent category.* (def: `None`)

**Overload 0:**

`[channel]` : *Parent category.*

`[string]` : *Name.*

(optional) `[boolean]` : *NSFW?* (def: `False`)

__**Examples:**__

`!channel createtext newtextchannel ParentCategory no`

`!channel createtext newtextchannel no`

`!channel createtext ParentCategory newtextchannel`

---

### channel createvoice
*Create new voice channel.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`createv, cvoice, cv, +voice, +v`


**Overload 2:**

`[string]` : *Name.*

(optional) `[channel]` : *Parent category.* (def: `None`)

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

**Overload 1:**

`[string]` : *Name.*

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

(optional) `[channel]` : *Parent category.* (def: `None`)

**Overload 0:**

`[channel]` : *Parent category.*

`[string]` : *Name.*

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

__**Examples:**__

`!channel createtext "My voice channel" ParentCategory 0 96000`

`!channel createtext "My voice channel" 10 96000`

`!channel createtext ParentCategory "My voice channel" 10 96000`

---

### channel delete
*Delete a given channel or category. If the channel isn't given, deletes the current one.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`-, del, d, remove, rm`


**Overload 1:**

(optional) `[channel]` : *Channel to delete.* (def: `None`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[string...]` : *Reason.*

__**Examples:**__

`!channel delete`

`!channel delete "My voice channel"`

`!channel delete "My voice channel" Because I can!`

---

### channel info
*Get information about a given channel. If the channel isn't given, uses the current one.*

**Requires permissions:**
`Read messages`

**Aliases:**
`i, information`


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

__**Examples:**__

`!channel info`

`!channel info "My voice channel"`

---

### channel modify
*Modify a given voice channel. Set 0 if you wish to keep the value as it is.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`edit, mod, m, e`


**Overload 1:**

`[channel]` : *Voice channel to edit*

(optional) `[int]` : *User limit.* (def: `0`)

(optional) `[int]` : *Bitrate.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

(optional) `[int]` : *User limit.* (def: `0`)

(optional) `[int]` : *Bitrate.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!channel modify "My voice channel" 20 96000 Some reason`

---

### channel rename
*Rename channel. If the channel isn't given, uses the current one.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`r, name, setname`


**Overload 2:**

`[string]` : *Reason.*

`[channel]` : *Channel to rename.*

`[string...]` : *New name.*

**Overload 1:**

`[channel]` : *Channel to rename.*

`[string...]` : *New name.*

**Overload 0:**

`[string...]` : *New name.*

__**Examples:**__

`!channel rename New name for this channel`

`!channel rename "My voice channel" "My old voice channel"`

`!channel rename "My reason" "My voice channel" "My old voice channel"`

---

### channel setparent
*Change the parent of the given channel. If the channel isn't given, uses the current one.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`setpar, par, parent`


**Overload 1:**

`[channel]` : *Child channel.*

`[channel]` : *Parent category.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[channel]` : *Parent category.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!channel setparent "My channel" ParentCategory`

`!channel setparent ParentCategory I set a new parent for this channel!`

---

### channel setposition
*Change the position of the given channel in the guild channel list. If the channel isn't given, uses the current one.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`setpos, pos, position`


**Overload 2:**

`[channel]` : *Channel to reorder.*

`[int]` : *Position.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 1:**

`[int]` : *Position.*

`[channel]` : *Channel to reorder.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[int]` : *Position.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!channel setposition 4`

`!channel setposition "My channel" 1`

`!channel setposition "My channel" 4 I changed position :)`

---

### channel settopic
*Set channel topic. If the channel isn't given, uses the current one.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`t, topic, sett`


**Overload 2:**

`[string]` : *Reason.*

`[channel]` : *Channel.*

`[string...]` : *New topic.*

**Overload 1:**

`[channel]` : *Channel.*

`[string...]` : *New Topic.*

**Overload 0:**

`[string...]` : *New Topic.*

__**Examples:**__

`!channel settopic New channel topic`

`!channel settopic "My channel" New channel topic`

---

### channel viewperms
*View permissions for a member or role in the given channel. If the member is not given, lists the sender's permissions. If the channel is not given, uses current one.*

**Aliases:**
`tp, perms, permsfor, testperms, listperms`


**Overload 3:**

(optional) `[member]` : *Member.* (def: `None`)

(optional) `[channel]` : *Channel.* (def: `None`)

**Overload 2:**

`[channel]` : *Channel.*

(optional) `[member]` : *Member.* (def: `None`)

**Overload 1:**

`[role]` : *Role.*

(optional) `[channel]` : *Channel.* (def: `None`)

**Overload 0:**

`[channel]` : *Channel.*

`[role]` : *Role.*

__**Examples:**__

`!channel viewperms @Someone`

`!channel viewperms Admins`

`!channel viewperms #private everyone`

`!channel viewperms everyone #private`

---

## coinflip
*Throw a coin.*

**Aliases:**
`coin`


---

## connect
*Connects me to a voice channel.*

**Owner-only.**

**Requires permissions:**
`Use voice chat`


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

---

## dice
*Throw a coin.*

**Aliases:**
`die, roll`


---

## disconnect
*Disconnects from voice channel.*

**Owner-only.**


---

## embed
*Embed an image given as an URL.*

**Requires permissions:**
`Attach files`


**Arguments:**

`[string]` : *Image URL.*

---

## emoji
*Manipulate guild emoji. Standalone call lists all guild emoji.*

**Aliases:**
`emojis, e`


__**Examples:**__

`!emoji`

---

### emoji add
*Add emoji.*

**Requires permissions:**
`Manage emoji`

**Aliases:**
`create, a, +`


**Arguments:**

`[string]` : *Name.*

`[string]` : *URL.*

__**Examples:**__

`!emoji add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg`

---

### emoji delete
*Remove guild emoji. Note: bots can only delete emojis they created.*

**Requires permissions:**
`Manage emoji`

**Aliases:**
`remove, del, -, d`


**Arguments:**

`[emoji]` : *Emoji to delete.*

__**Examples:**__

`!emoji delete pepe`

---

### emoji details
*Get details for guild emoji.*

**Aliases:**
`det`


**Arguments:**

`[emoji]` : *Emoji.*

__**Examples:**__

`!emoji details pepe`

---

### emoji list
*View guild emojis.*

**Aliases:**
`print, show, l, p`


__**Examples:**__

`!emoji list`

---

### emoji modify
*Edit name of an existing guild emoji.*

**Requires permissions:**
`Manage emoji`

**Aliases:**
`edit, mod, e, m`


**Overload 1:**

`[emoji]` : *Emoji.*

`[string]` : *Name.*

**Overload 0:**

`[string]` : *Name.*

`[emoji]` : *Emoji.*

__**Examples:**__

`!emoji modify :pepe: newname`

`!emoji modify newname :pepe:`

---

## emojireaction
*Emoji reaction handling.*

**Aliases:**
`ereact, er, emojir, emojireactions`


**Arguments:**

(optional) `[emoji]` : *Emoji to send.* (def: `None`)

`[string...]` : *Trigger word list.*

---

### emojireaction add
*Add emoji reactions to guild reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new`


**Arguments:**

`[emoji]` : *Emoji to send.*

`[string...]` : *Trigger word list.*

---

### emojireaction clear
*Delete all reactions for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c`


---

### emojireaction delete
*Remove emoji reactions for given trigger words.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, del, rm, d`


**Arguments:**

`[string...]` : *Trigger word list.*

---

### emojireaction list
*Show all emoji reactions.*

**Aliases:**
`ls, l`


**Arguments:**

(optional) `[int]` : *Page.* (def: `1`)

---

### filter add
*Add filter to guild filter list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new, a`


**Arguments:**

`[string...]` : *Filter. Can be a regex (case insensitive).*

---

### filter clear
*Delete all filters for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`c, da`


---

### filter delete
*Remove filter from guild filter list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, del`


**Arguments:**

`[string...]` : *Filter to remove.*

---

### filter list
*Show all filters for this guild.*

**Aliases:**
`ls, l`


**Arguments:**

(optional) `[int]` : *Page* (def: `1`)

---

### gamble coinflip
*Flip a coin and bet on the outcome.*

**Aliases:**
`coin, flip`


**Overload 1:**

`[int]` : *Bid.*

`[string]` : *Heads/Tails (h/t).*

**Overload 0:**

`[string]` : *Heads/Tails (h/t).*

`[int]` : *Bid.*

__**Examples:**__

`!bet coinflip 10 heads`

`!bet coinflip tails 20`

---

### gamble dice
*Roll a dice and bet on the outcome.*

**Aliases:**
`roll, die`


**Overload 1:**

`[int]` : *Bid.*

`[string]` : *Number guess (has to be a word one-six).*

**Overload 0:**

`[string]` : *Number guess (has to be a word one-six).*

`[int]` : *Bid.*

__**Examples:**__

`!dice 50 six`

`!dice three 10`

---

### gamble slot
*Roll a slot machine.*

**Aliases:**
`slotmachine`


**Arguments:**

(optional) `[int]` : *Bid.* (def: `5`)

__**Examples:**__

`!gamble slot 20`

---

## game caro
*Starts a "Caro" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play.*

**Aliases:**
`c`


__**Examples:**__

`!game caro`

---

## game connect4
*Starts a "Connect 4" game. Play a move by writing a number from 1 to 9 corresponding to the column where you wish to insert your piece.*

**Aliases:**
`connectfour, chain4, chainfour, c4`


__**Examples:**__

`!game connect4`

---

### game duel
*Starts a duel which I will commentate.*

**Aliases:**
`fight, vs, d`


**Arguments:**

`[user]` : *Who to fight with?*

__**Examples:**__

`!game duel @Someone`

---

### game hangman
*Starts a hangman game.*

**Aliases:**
`h, hang`


__**Examples:**__

`!game hangman`

---

### game leaderboard
*View the global game leaderboard.*

**Aliases:**
`globalstats`


__**Examples:**__

`!game leaderboard`

---

## game nunchi
*Nunchi game commands*

**Aliases:**
`n`


---

### game nunchi join
*Join a nunchi game.*

**Aliases:**
`+, compete`


---

### game nunchi rules
*Explain the game.*

**Aliases:**
`help`


---

## game othello
*Starts an "Othello" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play.*

**Aliases:**
`reversi, oth, rev`


__**Examples:**__

`!game othello`

---

### game quiz countries
*Country flags quiz.*

**Aliases:**
`flags`


---

## game race
*Racing!*

**Aliases:**
`r`


---

### game race join
*Join a race.*

**Aliases:**
`+, compete`


---

### game rps
*Rock, paper, scissors game against TheGodfather*

**Aliases:**
`rockpaperscissors`


**Arguments:**

`[string]` : *rock/paper/scissors*

__**Examples:**__

`!game rps scissors`

---

### game stats
*Print game stats for given user.*

**Aliases:**
`s, st`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

__**Examples:**__

`!game stats`

`!game stats @Someone`

---

### game tictactoe
*Starts a game of tic-tac-toe. Play by posting a number from 1 to 9 corresponding to field you wish to place your move on.*

**Aliases:**
`ttt`


__**Examples:**__

`!game tictactoe`

---

## game tictactoe
*Starts a "Tic-Tac-Toe" game. Play a move by writing a number from 1 to 9 corresponding to the field where you wish to play.*

**Aliases:**
`ttt`


__**Examples:**__

`!game tictactoe`

---

### game typing
*Typing race.*

**Aliases:**
`type, typerace, typingrace`


---

## gif
*GIPHY commands.*

**Aliases:**
`giphy`


**Arguments:**

`[string...]` : *Query.*

---

### gif random
*Return a random GIF.*

**Aliases:**
`r, rand, rnd`


---

### gif trending
*Return an amount of trending GIFs.*

**Aliases:**
`t, tr, trend`


**Arguments:**

(optional) `[int]` : *Number of results (1-10).* (def: `5`)

---

## greet
*Greets a user and starts a conversation.*

**Aliases:**
`hello, hi, halo, hey, howdy, sup`


---

### guild bans
*Get guild ban list.*

**Requires permissions:**
`View audit log`

**Aliases:**
`banlist, viewbanlist, getbanlist, getbans, viewbans`


__**Examples:**__

`!guild banlist`

---

### guild deleteleavechannel
*Remove leave message channel for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`delleavec, dellc, delleave, dlc`


__**Examples:**__

`!guild deletewelcomechannel`

---

### guild deletewelcomechannel
*Remove welcome message channel for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`delwelcomec, delwc, delwelcome, dwc, deletewc`


__**Examples:**__

`!guild deletewelcomechannel`

---

### guild getleavechannel
*Get current leave message channel for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`getleavec, getlc, getleave, leavechannel, lc`


__**Examples:**__

`!guild getleavechannel`

---

### guild getwelcomechannel
*Get current welcome message channel for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`getwelcomec, getwc, getwelcome, welcomechannel, wc`


__**Examples:**__

`!guild getwelcomechannel`

---

### guild info
*Get guild information.*

**Aliases:**
`i, information`


__**Examples:**__

`!guild info`

---

### guild listmembers
*Get guild member list.*

**Aliases:**
`memberlist, lm, members`


__**Examples:**__

`!guild memberlist`

---

### guild log
*Get audit logs.*

**Requires permissions:**
`View audit log`

**Aliases:**
`auditlog, viewlog, getlog, getlogs, logs`


__**Examples:**__

`!guild logs`

---

### guild prune
*Kick guild members who weren't active in given amount of days (1-7).*

**Requires permissions:**
`Kick members`

**Requires user permissions:**
`Administrator`

**Aliases:**
`p, clean`


**Arguments:**

(optional) `[int]` : *Days.* (def: `7`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!guild prune 5 Kicking inactives..`

---

### guild rename
*Rename guild.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`r, name, setname`


**Overload 0:**

`[string]` : *Reason.*

`[string...]` : *New name.*

**Overload 0:**

`[string...]` : *New name.*

__**Examples:**__

`!guild rename New guild name`

`!guild rename "Reason for renaming" New guild name`

---

### guild seticon
*Change icon of the guild.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`icon, si`


**Arguments:**

`[string]` : *New icon URL.*

__**Examples:**__

`!guild seticon http://imgur.com/someimage.png`

---

### guild setleavechannel
*Set leave message channel for this guild. If the channel isn't given, uses the current one.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`leavec, setlc, setleave`


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

__**Examples:**__

`!guild setleavechannel`

`!guild setleavechannel #bb`

---

### guild setwelcomechannel
*Set welcome message channel for this guild. If the channel isn't given, uses the current one.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setwc, setwelcomec, setwelcome`


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

__**Examples:**__

`!guild setwelcomechannel`

`!guild setwelcomechannel #welcome`

---

## help
*Displays command help.*


**Arguments:**

`[string...]` : *Command to provide help for.*

---

## imgur
*Search imgur. Invoking without sub command searches top.*

**Aliases:**
`img, im, i`


**Arguments:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Query.*

---

### imgur latest
*Return latest images for query.*

**Aliases:**
`l, new, newest`


**Arguments:**

`[int]` : *Number of images to print [1-10].*

`[string]` : *Query.*

---

### imgur top
*Return most rated images for query.*

**Aliases:**
`t`


**Arguments:**

(optional) `[string]` : *Time window (day/month/week/year/all).* (def: `day`)

(optional) `[int]` : *Number of images to print [1-10].* (def: `1`)

(optional) `[string...]` : *Query.* (def: `None`)

---

## insult
*Burns a user!*

**Aliases:**
`burn, insults`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

---

### insult add
*Add insult to list (Use % to code mention).*

**Owner-only.**

**Aliases:**
`+, new`


**Arguments:**

`[string...]` : *Response.*

---

### insult clear
*Delete all insults.*

**Owner-only.**

**Aliases:**
`clearall`


---

### insult delete
*Remove insult with a given index from list. (use ``!insults list`` to view indexes)*

**Owner-only.**

**Aliases:**
`-, remove, del, rm`


**Arguments:**

`[int]` : *Index.*

---

### insult list
*Show all insults.*


**Arguments:**

(optional) `[int]` : *Page.* (def: `1`)

---

## invite
*Get an instant invite link for the current channel.*

**Requires permissions:**
`Create instant invites`

**Aliases:**
`getinvite`


---

## joke
*Send a joke.*

**Aliases:**
`jokes, j`


---

### joke search
*Search for the joke containing the query.*

**Aliases:**
`s`


**Arguments:**

`[string...]` : *Query.*

---

### joke yourmom
*Yo mama so...*

**Aliases:**
`mama, m, yomomma, yomom, yomoma, yomamma, yomama`


---

## leave
*Makes Godfather leave the server.*

**Requires user permissions:**
`Kick members`


---

## leet
*Wr1t3s m3ss@g3 1n 1337sp34k.*


**Arguments:**

`[string...]` : *Text*

---

## meme
*Manipulate memes. When invoked without name, returns a random one.*

**Aliases:**
`memes, mm`


**Arguments:**

(optional) `[string...]` : *Meme name.* (def: `None`)

---

### meme add
*Add a new meme to the list.*

**Owner-only.**

**Aliases:**
`+, new, a`


**Arguments:**

`[string]` : *Short name (case insensitive).*

`[string]` : *URL.*

---

### meme create
*Creates a new meme from blank template.*

**Aliases:**
`maker, c, make, m`


**Arguments:**

`[string]` : *Template.*

`[string]` : *Top Text.*

`[string]` : *Bottom Text.*

---

### meme delete
*Deletes a meme from list.*

**Owner-only.**

**Aliases:**
`-, del, remove, rm, d`


**Arguments:**

`[string]` : *Short name (case insensitive).*

---

### meme list
*List all registered memes.*

**Aliases:**
`ls, l`


**Arguments:**

(optional) `[int]` : *Page.* (def: `1`)

---

### message attachments
*View all message attachments. If the message is not provided, uses the last sent message before command invocation.*

**Aliases:**
`a, files, la`


**Arguments:**

(optional) `[unsigned long]` : *Message ID.* (def: `0`)

__**Examples:**__

`!message attachments`

`!message attachments 408226948855234561`

---

### message delete
*Deletes the specified amount of most-recent messages from the channel.*

**Requires permissions:**
`Manage messages`

**Requires user permissions:**
`Administrator`

**Aliases:**
`-, prune, del, d`


**Arguments:**

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!messages delete 10`

`!messages delete 10 Cleaning spam`

---

### message deletefrom
*Deletes given amount of most-recent messages from given user.*

**Requires permissions:**
`Manage messages`

**Requires user permissions:**
`Administrator`

**Aliases:**
`-user, -u, deluser, du, dfu, delfrom`


**Overload 1:**

`[user]` : *User.*

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[int]` : *Amount.*

`[user]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!messages deletefrom @Someone 10 Cleaning spam`

`!messages deletefrom 10 @Someone Cleaning spam`

---

### message deletereactions
*Deletes all reactions from the given message.*

**Requires permissions:**
`Manage messages`

**Requires user permissions:**
`Administrator`

**Aliases:**
`-reactions, -r, delreactions, dr`


**Arguments:**

(optional) `[unsigned long]` : *ID.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!messages deletereactions 408226948855234561`

---

### message deleteregex
*Deletes given amount of most-recent messages that match a given regular expression.*

**Requires permissions:**
`Manage messages`

**Requires user permissions:**
`Administrator`

**Aliases:**
`-regex, -rx, delregex, drx`


**Overload 1:**

`[string]` : *Pattern (Regex).*

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[int]` : *Amount.*

`[string]` : *Pattern (Regex).*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!messages deletefrom s+p+a+m+ 10 Cleaning spam`

`!messages deletefrom 10 s+p+a+m+ Cleaning spam`

---

### message listpinned
*List pinned messages in this channel.*

**Aliases:**
`lp, listpins, listpin, pinned`


__**Examples:**__

`!messages listpinned`

---

### message modify
*Modify the given message.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`edit, mod, e, m`


**Arguments:**

`[unsigned long]` : *Message ID.*

`[string...]` : *New content.*

__**Examples:**__

`!messages modify 408226948855234561 modified text`

---

### message pin
*Pins the message given by ID. If the message is not provided, pins the last sent message before command invocation.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`p`


**Arguments:**

(optional) `[unsigned long]` : *ID.* (def: `0`)

__**Examples:**__

`!messages pin`

`!messages pin 408226948855234561`

---

### message unpin
*Unpins the message at given index (starting from 1). If the index is not given, unpins the most recent one.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`up`


**Arguments:**

(optional) `[int]` : *Index (starting from 1).* (def: `1`)

__**Examples:**__

`!messages unpin`

`!messages unpin 10`

---

### message unpinall
*Unpins all pinned messages in this channel.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`upa`


__**Examples:**__

`!messages unpinall`

---

### owner botavatar
*Set bot avatar.*

**Owner-only.**

**Aliases:**
`setbotavatar, setavatar`


**Arguments:**

`[string]` : *URL.*

__**Examples:**__

`!owner botavatar http://someimage.png`

---

### owner botname
*Set bot name.*

**Owner-only.**

**Aliases:**
`setbotname, setname`


**Arguments:**

`[string...]` : *New name.*

__**Examples:**__

`!owner setname TheBotfather`

---

### owner clearlog
*Clear application logs.*

**Owner-only.**

**Aliases:**
`clearlogs, deletelogs, deletelog`


__**Examples:**__

`!owner clearlog`

---

### owner dbquery
*Clear application logs.*

**Owner-only.**

**Aliases:**
`sql, dbq, q`


**Arguments:**

`[string...]` : *SQL Query.*

__**Examples:**__

`!owner dbquery SELECT * FROM gf.msgcount;`

---

### owner eval
*Evaluates a snippet of C# code, in context. Surround the code in the code block.*

**Owner-only.**

**Aliases:**
`compile, run, e, c, r`


**Arguments:**

`[string...]` : *Code to evaluate.*

__**Examples:**__

`!owner eval ```await Context.RespondAsync("Hello!");````

---

### owner generatecommandlist
*Generates a markdown command-list. You can also provide a file path for the output.*

**Owner-only.**

**Aliases:**
`cmdlist, gencmdlist, gencmds, gencmdslist`


**Arguments:**

(optional) `[string...]` : *File path.* (def: `None`)

__**Examples:**__

`!owner generatecommandlist`

`!owner generatecommandlist Temp/blabla.md`

---

### owner leaveguilds
*Leaves the given guilds.*

**Owner-only.**

**Aliases:**
`leave, gtfo`


**Arguments:**

`[unsigned long...]` : *Guild ID list.*

__**Examples:**__

`!owner leave 337570344149975050`

`!owner leave 337570344149975050 201315884709576708`

---

### owner sendmessage
*Sends a message to a user or channel.*

**Owner-only.**

**Aliases:**
`send, s`


**Arguments:**

`[string]` : *u/c (for user or channel.)*

`[unsigned long]` : *User/Channel ID.*

`[string...]` : *Message.*

__**Examples:**__

`!owner send u 303463460233150464 Hi to user!`

`!owner send c 120233460278590414 Hi to channel!`

---

### owner shutdown
*Triggers the dying in the vineyard scene (power off the bot).*

**Owner-only.**

**Aliases:**
`disable, poweroff, exit, quit`


__**Examples:**__

`!owner shutdown`

---

### owner statuses add
*Add a status to running status queue.*

**Aliases:**
`+, a`


**Arguments:**

`[string]` : *Activity type.*

`[string...]` : *Status.*

__**Examples:**__

`!owner status add Playing CS:GO`

`!owner status add Streaming on Twitch`

---

### owner statuses delete
*Remove status from running queue.*

**Aliases:**
`-, remove, rm, del`


**Arguments:**

`[int]` : *Status ID.*

__**Examples:**__

`!owner status delete Playing CS:GO`

---

### owner statuses list
*List all bot statuses.*

**Aliases:**
`ls`


__**Examples:**__

`!owner status list`

---

### owner sudo
*Executes a command as another user.*

**Owner-only.**

**Aliases:**
`execas, as`


**Arguments:**

`[member]` : *Member to execute as.*

`[string...]` : *Command text to execute.*

__**Examples:**__

`!owner sudo @Someone !rate`

---

### owner toggleignore
*Toggle bot's reaction to commands.*

**Owner-only.**

**Aliases:**
`ti`


__**Examples:**__

`!owner toggleignore`

---

## penis
*An accurate size of the user's manhood.*

**Aliases:**
`size, length, manhood, dick`


**Arguments:**

`[user]` : *Who to measure*

---

## ping
*Ping the bot.*


---

## play
*Plays a mp3 file from URL or server filesystem.*

**Owner-only.**

**Aliases:**
`music, p`


**Arguments:**

`[string...]` : *URL or YouTube search query.*

---

### play file
*Plays an audio file from server filesystem.*

**Owner-only.**

**Requires permissions:**
`Speak, Use voice chat`

**Aliases:**
`f`


**Arguments:**

`[string...]` : *Full path to the file to play.*

---

## poll
*Starts a poll in the channel.*

**Aliases:**
`vote`


**Arguments:**

`[string...]` : *Question.*

---

## pollr
*Starts a poll with reactions in the channel.*

**Aliases:**
`voter`


**Arguments:**

`[emoji...]` : *Options*

---

## prefix
*Get current guild prefix, or change it.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`setprefix`


**Arguments:**

(optional) `[string]` : *Prefix to set.* (def: `None`)

---

### random cat
*Get a random cat image.*


---

### random choose
*!choose option1, option2, option3...*

**Aliases:**
`select`


**Arguments:**

`[string...]` : *Option list (separated with a comma).*

---

### random dog
*Get a random dog image.*


---

### random raffle
*Choose a user from the online members list belonging to a given role.*


**Arguments:**

(optional) `[role]` : *Role.* (def: `None`)

---

## rank
*User ranking commands.*

**Aliases:**
`ranks, ranking`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

---

### rank list
*Print all available ranks.*

**Aliases:**
`levels`


---

### rank top
*Get rank leaderboard.*


---

## rate
*An accurate graph of a user's humanity.*

**Aliases:**
`score, graph`


**Arguments:**

`[user]` : *Who to measure.*

---

## remind
*Resend a message after some time.*


**Arguments:**

`[int]` : *Time to wait before repeat (in seconds).*

`[string...]` : *What to repeat.*

---

## report
*Send a report message to owner about a bug (please don't abuse... please).*


**Arguments:**

`[string...]` : *Text.*

---

## roles
*Miscellaneous role control commands.*

**Aliases:**
`role, r, rl`


---

### roles create
*Create a new role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`new, add, +, c`


**Overload 2:**

`[string]` : *Name.*

(optional) `[color]` : *Color.* (def: `None`)

(optional) `[boolean]` : *Hoisted (visible in online list)?* (def: `False`)

(optional) `[boolean]` : *Mentionable?* (def: `False`)

**Overload 1:**

`[color]` : *Color.*

`[string...]` : *Name.*

**Overload 0:**

`[string...]` : *Name.*

__**Examples:**__

`!roles create "My role" #C77B0F no no`

`!roles create `

`!roles create #C77B0F My new role`

---

### roles delete
*Create a new role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`del, remove, d, -, rm`


**Arguments:**

`[role]` : *Role.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!role delete My role`

`!role delete @admins`

---

### roles info
*Get information about a given role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`i`


**Arguments:**

`[role]` : *Role.*

__**Examples:**__

`!role info Admins`

---

### roles mentionall
*Mention all users from given role.*

**Requires permissions:**
`Mention everyone`

**Aliases:**
`mention, @, ma`


**Arguments:**

`[role]` : *Role.*

__**Examples:**__

`!role mentionall Admins`

---

### roles setcolor
*Set a color for the role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`clr, c, sc, setc`


**Overload 1:**

`[role]` : *Role.*

`[color]` : *Color.*

**Overload 0:**

`[color]` : *Color.*

`[role]` : *Role.*

__**Examples:**__

`!role setcolor #FF0000 Admins`

`!role setcolor Admins #FF0000`

---

### roles setmentionable
*Set role mentionable var.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`mentionable, m, setm`


**Overload 1:**

`[role]` : *Role.*

(optional) `[boolean]` : *Mentionable?* (def: `True`)

**Overload 0:**

`[boolean]` : *Mentionable?*

`[role]` : *Role.*

__**Examples:**__

`!role setmentionable Admins`

`!role setmentionable Admins false`

`!role setmentionable false Admins`

---

### roles setname
*Set a name for the role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`name, rename, n`


**Overload 1:**

`[role]` : *Role.*

`[string...]` : *New name.*

**Overload 0:**

`[string]` : *New name.*

`[role]` : *Role.*

__**Examples:**__

`!role setname @Admins Administrators`

`!role setname Administrators @Admins`

---

### roles setvisible
*Set role hoisted var (visibility in online list).*

**Requires permissions:**
`Manage roles`

**Aliases:**
`separate, h, seth, hoist, sethoist`


**Overload 0:**

`[role]` : *Role.*

(optional) `[boolean]` : *Hoisted (visible in online list)?* (def: `False`)

**Overload 0:**

`[boolean]` : *Hoisted (visible in online list)?*

`[role]` : *Role.*

__**Examples:**__

`!role setvisible Admins`

`!role setvisible Admins false`

`!role setvisible false Admins`

---

## rss
*RSS feed operations.*

**Aliases:**
`feed`


**Arguments:**

`[string...]` : *URL.*

---

### rss listsubs
*Get feed list for the current channel.*

**Aliases:**
`ls, list`


---

### rss news
*Get newest world news.*


---

## rss reddit
*Reddit feed manipulation.*

**Aliases:**
`r`


**Arguments:**

(optional) `[string]` : *Subreddit.* (def: `all`)

---

### rss reddit subscribe
*Add new feed for a subreddit.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`add, a, +, sub`


**Arguments:**

`[string]` : *Subreddit.*

---

### rss reddit unsubscribe
*Remove a subreddit feed.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`


**Arguments:**

`[string]` : *Subreddit.*

---

### rss subscribe
*Subscribe to given url.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`sub, add, +`


**Arguments:**

`[string...]` : *URL.*

(optional) `[string]` : *Friendly name.* (def: `None`)

---

### rss unsubscribe
*Remove an existing feed subscription.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`


**Arguments:**

`[int]` : *ID.*

---

### rss wm
*Get newest topics from WM forum.*


---

## rss youtube
*Youtube feed manipulation.*

**Aliases:**
`yt, y`


**Arguments:**

`[string]` : *Channel URL.*

---

### rss youtube subscribe
*Add new feed for a YouTube channel.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`add, a, +, sub`


**Arguments:**

`[string]` : *Channel URL.*

(optional) `[string]` : *Friendly name.* (def: `None`)

---

### rss youtube unsubscribe
*Remove a YouTube channel feed.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`


**Arguments:**

`[string]` : *Channel URL.*

---

## say
*Repeats after you.*

**Aliases:**
`repeat`


**Arguments:**

`[string...]` : *Text.*

---

### steam profile
*Get Steam user information from ID.*

**Aliases:**
`id`


**Arguments:**

`[unsigned long]` : *ID.*

---

## stop
*Stops current voice playback.*

**Owner-only.**


---

### swat query
*Return server information.*

**Aliases:**
`q, info, i`


**Arguments:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

---

### swat serverlist
*Print the serverlist with current player numbers.*


---

### swat servers add
*Add a server to serverlist.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`+, a`


**Arguments:**

`[string]` : *Name.*

`[string]` : *IP.*

(optional) `[int]` : *Query port* (def: `10481`)

---

### swat servers delete
*Remove a server from serverlist.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`-, del, d`


**Arguments:**

`[string]` : *Name.*

---

### swat servers list
*List all registered servers.*

**Aliases:**
`ls, l`


**Arguments:**

(optional) `[int]` : *Page.* (def: `1`)

---

### swat settimeout
*Set checking timeout.*

**Owner-only.**


**Arguments:**

`[int]` : *Timeout (in ms).*

---

### swat startcheck
*Notifies of free space in server.*

**Aliases:**
`checkspace, spacecheck`


**Arguments:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

---

### swat stopcheck
*Stops space checking.*

**Aliases:**
`checkstop`


---

## textreaction
*Text reaction handling.*

**Aliases:**
`treact, tr, txtr, textreactions`


**Arguments:**

`[string]` : *Trigger (case sensitive).*

`[string...]` : *Response.*

---

### textreaction add
*Add text reaction to guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new`


**Arguments:**

`[string]` : *Trigger (case sensitive).*

`[string...]` : *Response.*

---

### textreaction clear
*Delete all text reactions for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`c, da`


---

### textreaction delete
*Remove text reaction from guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, del, rm, d`


**Arguments:**

`[string...]` : *Trigger words to remove.*

---

### textreaction list
*Show all text reactions for the guild. Each page has 10 text reactions.*

**Aliases:**
`ls, l`


**Arguments:**

(optional) `[int]` : *Page.* (def: `1`)

---

## tts
*Repeats after you but uses tts.*


**Arguments:**

`[string...]` : *Text.*

---

## urbandict
*Search Urban Dictionary for a query.*

**Aliases:**
`ud, urban`


**Arguments:**

`[string...]` : *Query.*

---

### user addrole
*Assign a role to a member.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`+role, +r, ar, addr, +roles, addroles, giverole, giveroles, grantrole, grantroles, gr`


**Overload 1:**

`[member]` : *Member.*

`[role...]` : *Role to grant.*

**Overload 0:**

`[role]` : *Role.*

`[member]` : *Member.*

__**Examples:**__

`!user addrole @User Admins`

`!user addrole Admins @User`

---

### user avatar
*Get avatar from user.*

**Aliases:**
`a, pic, profilepic`


**Arguments:**

`[user]` : *User.*

__**Examples:**__

`!user avatar @Someone`

---

### user ban
*Bans the user from the guild.*

**Requires permissions:**
`Ban members`

**Aliases:**
`b`


**Arguments:**

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user ban @Someone`

`!user ban @Someone Troublemaker`

---

### user banid
*Bans the ID from the server.*

**Requires permissions:**
`Ban members`

**Aliases:**
`bid`


**Arguments:**

`[unsigned long]` : *ID.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user banid 154956794490845232`

`!user banid 154558794490846232 Troublemaker`

---

### user deafen
*Deafen a member.*

**Requires permissions:**
`Deafen voice chat members`

**Aliases:**
`deaf, d, df`


**Arguments:**

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user deafen @Someone`

---

### user info
*Print the information about the given user. If the user is not given, uses the sender.*

**Aliases:**
`i, information`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

__**Examples:**__

`!user info @Someone`

---

### user kick
*Kicks the member from the guild.*

**Requires permissions:**
`Kick members`

**Aliases:**
`k`


**Arguments:**

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user kick @Someone`

`!user kick @Someone Troublemaker`

---

### user mute
*Mute a member.*

**Requires permissions:**
`Mute voice chat members`

**Aliases:**
`m`


**Arguments:**

`[member]` : *member.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user mute @Someone`

`!user mute @Someone Trashtalk`

---

### user removeallroles
*Revoke all roles from user.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`remallroles, -ra, -rall, -allr`


**Arguments:**

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user removeallroles @Someone`

---

### user removerole
*Revoke a role from member.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`remrole, rmrole, rr, -role, -r, removeroles, revokerole, revokeroles`


**Overload 2:**

`[member]` : *Member.*

`[role...]` : *Roles to revoke.*

**Overload 1:**

`[member]` : *Member.*

`[role]` : *Role.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[role]` : *Role.*

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user removerole @Someone Admins`

`!user removerole Admins @Someone`

---

### user setname
*Gives someone a new nickname.*

**Requires permissions:**
`Manage nicknames`

**Aliases:**
`nick, newname, name, rename`


**Arguments:**

`[member]` : *User.*

(optional) `[string...]` : *New name.* (def: `None`)

__**Examples:**__

`!user setname @Someone Newname`

---

### user softban
*Bans the member from the guild and then unbans him immediately.*

**Requires permissions:**
`Ban members`

**Aliases:**
`sb, sban`


**Arguments:**

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user sban @Someone`

`!user sban @Someone Troublemaker`

---

### user tempban
*Temporarily ans the user from the server and then unbans him after given timespan.*

**Requires permissions:**
`Ban members`

**Aliases:**
`tb, tban, tmpban, tmpb`


**Overload 1:**

`[time span]` : *Time span.*

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[member]` : *User.*

`[time span]` : *Time span.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user tempban @Someone 3h4m`

`!user tempban 5d @Someone Troublemaker`

`!user tempban @Someone 5h30m30s Troublemaker`

---

### user unban
*Unbans the user ID from the server.*

**Requires permissions:**
`Ban members`

**Aliases:**
`ub`


**Arguments:**

`[unsigned long]` : *ID.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user unban 154956794490845232`

---

### user undeafen
*Undeafen a member.*

**Requires permissions:**
`Deafen voice chat members`

**Aliases:**
`udeaf, ud, udf`


**Arguments:**

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user undeafen @Someone`

---

### user unmute
*Unmute a member.*

**Requires permissions:**
`Mute voice chat members`

**Aliases:**
`um`


**Arguments:**

`[member]` : *member.*

(optional) `[string...]` : *Reason.* (def: `None`)

__**Examples:**__

`!user unmute @Someone`

`!user unmute @Someone Some reason`

---

### user warn
*Warn a member in private message by sending a given warning text.*

**Requires permissions:**
`Kick members`

**Aliases:**
`w`


**Arguments:**

`[member]` : *Member.*

(optional) `[string...]` : *Warning message.* (def: `None`)

__**Examples:**__

`!user warn @Someone Stop spamming or kick!`

---

## youtube
*Youtube search commands.*

**Aliases:**
`y, yt`


**Arguments:**

`[string...]` : *Search query.*

---

### youtube search
*Advanced youtube search.*

**Aliases:**
`s`


**Arguments:**

`[int]` : *Amount of results. [1-10]*

`[string...]` : *Search query.*

---

### youtube searchc
*Advanced youtube search for channels only.*

**Aliases:**
`sc, searchchannel`


**Arguments:**

`[string...]` : *Search query.*

---

### youtube searchp
*Advanced youtube search for playlists only.*

**Aliases:**
`sp, searchplaylist`


**Arguments:**

`[string...]` : *Search query.*

---

### youtube searchv
*Advanced youtube search for videos only.*

**Aliases:**
`sv, searchvideo`


**Arguments:**

`[string...]` : *Search query.*

---

## zugify
*I don't even...*

**Aliases:**
`z`


**Arguments:**

`[string...]` : *Text.*

---

