# Command list

## 8ball
*An almighty ball which knows the answer to any question you ask. Alright, it's random answer, so what?*

**Aliases:**
`8b`


**Arguments:**

`[string...]` : *A question for the almighty ball.*

**Examples:**

```
!8ball Am I gay?
```
---

## Group: bank
*Bank manipulation. If invoked alone, prints out your bank balance.*

**Aliases:**
`$, $$, $$$`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!bank
```
---

### bank balance
*View account balance for given user. If the user is not given, checks sender's balance.*

**Aliases:**
`s, status, bal, money, credits`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!bank balance @Someone
```
---

### bank grant
*Magically give funds to some user.*

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

**Examples:**

```
!bank grant @Someone 1000
!bank grant 1000 @Someone
```
---

### bank register
*Create an account for you in WM bank.*

**Aliases:**
`r, signup, activate`


**Examples:**

```
!bank register
```
---

### bank top
*Print the richest users.*

**Aliases:**
`leaderboard, elite`


**Examples:**

```
!bank top
```
---

### bank transfer
*Transfer funds from your account to another one.*

**Aliases:**
`lend`


**Overload 1:**

`[user]` : *User to send credits to.*

`[int]` : *Amount.*

**Overload 0:**

`[int]` : *Amount.*

`[user]` : *User to send credits to.*

**Examples:**

```
!bank transfer @Someone 40
!bank transfer 40 @Someone
```
---

## cancelvote
*Vote for an option in the current running poll.*

**Aliases:**
`cvote, resetvote`


**Examples:**

```
!vote 1
```
---

## Group: cards
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

**Examples:**

```
!deck draw 5
```
---

### cards reset
*Opens a brand new card deck.*

**Aliases:**
`new, opennew, open`


**Examples:**

```
!deck reset
```
---

### cards shuffle
*Shuffles current deck.*

**Aliases:**
`s, sh, mix`


**Examples:**

```
!deck shuffle
```
---

### channel createcategory
*Create new channel category.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`createcat, createc, ccat, cc, +cat, +c, +category`


**Arguments:**

`[string...]` : *Name.*

**Examples:**

```
!channel createcategory My New Category
```
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

**Examples:**

```
!channel createtext newtextchannel ParentCategory no
!channel createtext newtextchannel no
!channel createtext ParentCategory newtextchannel
```
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

**Examples:**

```
!channel createtext "My voice channel" ParentCategory 0 96000
!channel createtext "My voice channel" 10 96000
!channel createtext ParentCategory "My voice channel" 10 96000
```
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

**Examples:**

```
!channel delete
!channel delete "My voice channel"
!channel delete "My voice channel" Because I can!
```
---

### channel info
*Get information about a given channel. If the channel isn't given, uses the current one.*

**Requires permissions:**
`Read messages`

**Aliases:**
`i, information`


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

**Examples:**

```
!channel info
!channel info "My voice channel"
```
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

**Examples:**

```
!channel modify "My voice channel" 20 96000 Some reason
```
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

**Examples:**

```
!channel rename New name for this channel
!channel rename "My voice channel" "My old voice channel"
!channel rename "My reason" "My voice channel" "My old voice channel"
```
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

**Examples:**

```
!channel setparent "My channel" ParentCategory
!channel setparent ParentCategory I set a new parent for this channel!
```
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

**Examples:**

```
!channel setposition 4
!channel setposition "My channel" 1
!channel setposition "My channel" 4 I changed position :)
```
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

**Examples:**

```
!channel settopic New channel topic
!channel settopic "My channel" New channel topic
```
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

**Examples:**

```
!channel viewperms @Someone
!channel viewperms Admins
!channel viewperms #private everyone
!channel viewperms everyone #private
```
---

## coinflip
*Flip a coin.*

**Aliases:**
`coin, flip`


**Examples:**

```
!coinflip
```
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
*Roll a dice.*

**Aliases:**
`die, roll`


**Examples:**

```
!dice
```
---

## disconnect
*Disconnects from voice channel.*

**Owner-only.**


---

## Group: emoji
*Manipulate guild emoji. Standalone call lists all guild emoji.*

**Aliases:**
`emojis, e`


**Examples:**

```
!emoji
```
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

**Examples:**

```
!emoji add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg
```
---

### emoji delete
*Remove guild emoji. Note: bots can only delete emojis they created.*

**Requires permissions:**
`Manage emoji`

**Aliases:**
`remove, del, -, d`


**Arguments:**

`[emoji]` : *Emoji to delete.*

**Examples:**

```
!emoji delete pepe
```
---

### emoji details
*Get details for guild emoji.*

**Aliases:**
`det`


**Arguments:**

`[emoji]` : *Emoji.*

**Examples:**

```
!emoji details pepe
```
---

### emoji list
*View guild emojis.*

**Aliases:**
`print, show, l, p, ls`


**Examples:**

```
!emoji list
```
---

### emoji modify
*Edit name of an existing guild emoji.*

**Requires permissions:**
`Manage emoji`

**Aliases:**
`edit, mod, e, m, rename`


**Overload 1:**

`[emoji]` : *Emoji.*

`[string]` : *Name.*

**Overload 0:**

`[string]` : *Name.*

`[emoji]` : *Emoji.*

**Examples:**

```
!emoji modify :pepe: newname
!emoji modify newname :pepe:
```
---

## Group: emojireaction
*Orders a bot to react with given emoji to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new emoji reaction to a given trigger word list. Note: Trigger words can be regular expressions (use ``emojireaction addregex`` command).*

**Aliases:**
`ereact, er, emojir, emojireactions`


**Arguments:**

`[emoji]` : *Emoji to send.*

`[string...]` : *Trigger word list.*

**Examples:**

```
!emojireaction :smile: haha laughing
```
---

### emojireaction add
*Add emoji reaction to guild reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new, a`


**Overload 1:**

`[emoji]` : *Emoji to send.*

`[string...]` : *Trigger word list (case-insensitive).*

**Overload 0:**

`[string]` : *Trigger word (case-insensitive).*

`[emoji]` : *Emoji to send.*

**Examples:**

```
!emojireaction add :smile: haha
!emojireaction add haha :smile:
```
---

### emojireaction addregex
*Add emoji reaction triggered by a regex to guild reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+r, +regex, +regexp, +rgx, newregex, addrgx`


**Overload 1:**

`[emoji]` : *Emoji to send.*

`[string...]` : *Trigger word list (case-insensitive).*

**Overload 0:**

`[string]` : *Trigger word (case-insensitive).*

`[emoji]` : *Emoji to send.*

**Examples:**

```
!emojireaction addregex :smile: (ha)+
!emojireaction addregex (ha)+ :smile:
```
---

### emojireaction clear
*Delete all reactions for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c, ca, cl, clearall`


**Examples:**

```
!emojireactions clear
```
---

### emojireaction delete
*Remove emoji reactions for given trigger words.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, del, rm, d`


**Overload 2:**

`[emoji]` : *Emoji to remove reactions for.*

**Overload 1:**

`[int...]` : *IDs of the reactions to remove.*

**Overload 0:**

`[string...]` : *Trigger words to remove.*

**Examples:**

```
!emojireaction delete haha sometrigger
!emojireaction delete 5
!emojireaction delete 5 4
!emojireaction delete :joy:
```
---

### emojireaction list
*Show all emoji reactions for this guild.*

**Aliases:**
`ls, l, view`


**Examples:**

```
!emojireaction list
```
---

## Group: filter
*Message filtering commands. If invoked without subcommand, adds a new filter for the given word list. Words can be regular expressions.*

**Aliases:**
`f, filters`


**Arguments:**

`[string...]` : *Filter list. Filter is a regular expression (case insensitive).*

**Examples:**

```
!filter fuck fk f+u+c+k+
```
---

### filter add
*Add filter to guild filter list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new, a`


**Arguments:**

`[string...]` : *Filter list. Filter is a regular expression (case insensitive).*

**Examples:**

```
!filter add fuck f+u+c+k+
```
---

### filter clear
*Delete all filters for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c, ca, cl, clearall`


**Examples:**

```
!filter clear
```
---

### filter delete
*Remove filters from guild filter list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, del, rm, rem, d`


**Arguments:**

`[string...]` : *Filters to remove.*

**Examples:**

```
!filter delete fuck f+u+c+k+
```
---

### filter list
*Show all filters for this guild.*

**Aliases:**
`ls, l`


**Examples:**

```
!filter list
```
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

**Examples:**

```
!bet coinflip 10 heads
!bet coinflip tails 20
```
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

**Examples:**

```
!dice 50 six
!dice three 10
```
---

### gamble slot
*Roll a slot machine.*

**Aliases:**
`slotmachine`


**Arguments:**

(optional) `[int]` : *Bid.* (def: `5`)

**Examples:**

```
!gamble slot 20
```
---

## Group: game animalrace
*Start a new animal race!*

**Aliases:**
`r, race, ar`


**Examples:**

```
!game animalrace
```
---

### game animalrace join
*Join an existing animal race game.*

**Aliases:**
`+, compete, enter, j`


**Examples:**

```
!game animalrace join
```
---

## Group: game caro
*Starts a "Caro" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.*

**Aliases:**
`c, gomoku, gobang`


**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```
!game caro
!game caro 10s
```
---

### game caro rules
*Explain the Caro game rules.*

**Aliases:**
`help, h, ruling, rule`


**Examples:**

```
!game caro rules
```
---

## Group: game connect4
*Starts a "Connect 4" game. Play a move by writing a number from 1 to 9 corresponding to the column where you wish to insert your piece. You can also specify a time window in which player must submit their move.*

**Aliases:**
`connectfour, chain4, chainfour, c4, fourinarow, fourinaline, 4row, 4line, cfour`


**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```
!game connect4
!game connect4 10s
```
---

### game connect4 rules
*Explain the Connect4 game rules.*

**Aliases:**
`help, h, ruling, rule`


**Examples:**

```
!game connect4 rules
```
---

### game duel
*Starts a duel which I will commentate.*

**Aliases:**
`fight, vs, d`


**Arguments:**

`[user]` : *Who to fight with?*

**Examples:**

```
!game duel @Someone
```
---

### game hangman
*Starts a hangman game.*

**Aliases:**
`h, hang`


**Examples:**

```
!game hangman
```
---

### game leaderboard
*View the global game leaderboard.*

**Aliases:**
`globalstats`


**Examples:**

```
!game leaderboard
```
---

## Group: game numberrace
*Number racing game commands.*

**Aliases:**
`nr, n, nunchi, numbers, numbersrace`


**Examples:**

```
!game numberrace
```
---

### game numberrace join
*Join an existing number race game.*

**Aliases:**
`+, compete, j, enter`


**Examples:**

```
!game numberrace join
```
---

### game numberrace rules
*Explain the number race rules.*

**Aliases:**
`help, h, ruling, rule`


**Examples:**

```
!game numberrace rules
```
---

## Group: game othello
*Starts an "Othello" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.*

**Aliases:**
`reversi, oth, rev`


**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```
!game othello
!game othello 10s
```
---

### game othello rules
*Explain the Othello game rules.*

**Aliases:**
`help, h, ruling, rule`


**Examples:**

```
!game othello rules
```
---

## Group: game quiz
*List all available quiz categories.*

**Aliases:**
`trivia, q`


**Examples:**

```
!game quiz 
```
---

### game quiz countries
*Country flags guessing quiz.*

**Aliases:**
`flags`


**Examples:**

```
!game quiz countries
```
---

### game rps
*Rock, paper, scissors game against TheGodfather*

**Aliases:**
`rockpaperscissors`


**Arguments:**

`[string]` : *rock/paper/scissors*

**Examples:**

```
!game rps scissors
```
---

### game stats
*Print game stats for given user.*

**Aliases:**
`s, st`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!game stats
!game stats @Someone
```
---

## Group: game tictactoe
*Starts a "Tic-Tac-Toe" game. Play a move by writing a number from 1 to 9 corresponding to the field where you wish to play. You can also specify a time window in which player must submit their move.*

**Aliases:**
`ttt`


**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```
!game tictactoe
!game tictactoe 10s
```
---

### game tictactoe rules
*Explain the Tic-Tac-Toe game rules.*

**Aliases:**
`help, h, ruling, rule`


**Examples:**

```
!game tictactoe rules
```
---

### game typingrace
*Typing race.*

**Aliases:**
`type, typerace, typing`


**Examples:**

```
!game typingrace
```
---

## Group: gif
*GIPHY commands. If invoked without a subcommand, searches GIPHY with given query.*

**Aliases:**
`giphy`


**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!gif wat
```
---

### gif random
*Return a random GIF.*

**Aliases:**
`r, rand, rnd`


**Examples:**

```
!gif random
```
---

### gif trending
*Return an amount of trending GIFs.*

**Aliases:**
`t, tr, trend`


**Arguments:**

(optional) `[int]` : *Number of results (1-10).* (def: `5`)

**Examples:**

```
!gif trending 3
!gif trending
```
---

## giveme
*Grants you a role from this guild's self-assignable roles list.*

**Requires bot permissions:**
`Manage roles`

**Aliases:**
`giverole, gimme, grantme`


**Arguments:**

`[role]` : *Role to grant.*

**Examples:**

```
!giveme @Announcements
```
---

### guild bans
*Get guild ban list.*

**Requires permissions:**
`View audit log`

**Aliases:**
`banlist, viewbanlist, getbanlist, getbans, viewbans`


**Examples:**

```
!guild banlist
```
---

### guild deleteleavechannel
*Remove leave message channel for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`delleavec, dellc, delleave, dlc`


**Examples:**

```
!guild deletewelcomechannel
```
---

### guild deleteleavemessage
*Remove leave message for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`delleavem, dellm, delleavemsg, dlm, deletelm, dwlsg`


**Examples:**

```
!guild deleteleavemessage
```
---

### guild deletewelcomechannel
*Remove welcome message channel for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`delwelcomec, delwc, delwelcome, dwc, deletewc`


**Examples:**

```
!guild deletewelcomechannel
```
---

### guild deletewelcomemessage
*Remove welcome message for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`delwelcomem, delwm, delwelcomemsg, dwm, deletewm, dwmsg`


**Examples:**

```
!guild deletewelcomemessage
```
---

### guild getleavechannel
*Get current leave message channel for this guild.*

**Aliases:**
`getleavec, getlc, leavechannel, lc`


**Examples:**

```
!guild getleavechannel
```
---

### guild getleavemessage
*Get current leave message for this guild.*

**Aliases:**
`getleavem, getlm, leavemessage, lm, leavemsg, lmsg`


**Examples:**

```
!guild getwelcomemessage
```
---

### guild getwelcomechannel
*Get current welcome message channel for this guild.*

**Aliases:**
`getwelcomec, getwc, welcomechannel, wc`


**Examples:**

```
!guild getwelcomechannel
```
---

### guild getwelcomemessage
*Get current welcome message for this guild.*

**Aliases:**
`getwelcomem, getwm, welcomemessage, wm, welcomemsg, wmsg`


**Examples:**

```
!guild getwelcomemessage
```
---

### guild info
*Get guild information.*

**Aliases:**
`i, information`


**Examples:**

```
!guild info
```
---

### guild listmembers
*Get guild member list.*

**Aliases:**
`memberlist, lm, members`


**Examples:**

```
!guild memberlist
```
---

### guild log
*Get audit logs.*

**Requires permissions:**
`View audit log`

**Aliases:**
`auditlog, viewlog, getlog, getlogs, logs`


**Examples:**

```
!guild logs
```
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

**Examples:**

```
!guild prune 5 Kicking inactives..
```
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

**Examples:**

```
!guild rename New guild name
!guild rename "Reason for renaming" New guild name
```
---

### guild seticon
*Change icon of the guild.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`icon, si`


**Arguments:**

`[string]` : *New icon URL.*

**Examples:**

```
!guild seticon http://imgur.com/someimage.png
```
---

### guild setleavechannel
*Set leave message channel for this guild. If the channel isn't given, uses the current one.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`leavec, setlc, setleave`


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

**Examples:**

```
!guild setleavechannel
!guild setleavechannel #bb
```
---

### guild setleavemessage
*Set leave message for this guild. Any occurances of ``%user%`` inside the string will be replaced with newly joined user mention. Invoking command without a message will reset the current leave message to a default one.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setlm, setleavem, setleavemsg, setlmsg`


**Arguments:**

(optional) `[string...]` : *Message.* (def: `None`)

**Examples:**

```
!guild setleavemessage
!guild setleavemessage Bye, %user%!
```
---

### guild setwelcomechannel
*Set welcome message channel for this guild. If the channel isn't given, uses the current one.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setwc, setwelcomec, setwelcome`


**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

**Examples:**

```
!guild setwelcomechannel
!guild setwelcomechannel #welcome
```
---

### guild setwelcomemessage
*Set welcome message for this guild. Any occurances of ``%user%`` inside the string will be replaced with newly joined user mention. Invoking command without a message will reset the current welcome message to a default one.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setwm, setwelcomem, setwelcomemsg, setwmsg`


**Arguments:**

(optional) `[string...]` : *Message.* (def: `None`)

**Examples:**

```
!guild setwelcomemessage
!guild setwelcomemessage Welcome, %user%!
```
---

## help
*Displays command help.*


**Arguments:**

`[string...]` : *Command to provide help for.*

---

## Group: imgur
*Search imgur. Invoking without subcommand retrieves top ranked images from given subreddit.*

**Aliases:**
`img, im, i`


**Overload 1:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Overload 0:**

`[string]` : *Subreddit.*

(optional) `[int]` : *Number of images to print [1-10].* (def: `1`)

**Examples:**

```
!imgur aww
!imgur 10 aww
!imgur aww 10
```
---

### imgur latest
*Return latest images from given subreddit.*

**Aliases:**
`l, new, newest`


**Overload 1:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Overload 0:**

`[string]` : *Subreddit.*

`[int]` : *Number of images to print [1-10].*

**Examples:**

```
!imgur latest 5 aww
!imgur latest aww 5
```
---

### imgur top
*Return amount of top rated images in the given subreddit for given timespan.*

**Aliases:**
`t`


**Overload 3:**

`[TimeWindow]` : *Timespan in which to search (day/week/month/year/all).*

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Overload 2:**

`[TimeWindow]` : *Timespan in which to search (day/week/month/year/all).*

`[string]` : *Subreddit.*

(optional) `[int]` : *Number of images to print [1-10].* (def: `1`)

**Overload 1:**

`[int]` : *Number of images to print [1-10].*

`[TimeWindow]` : *Timespan in which to search (day/week/month/year/all).*

`[string...]` : *Subreddit.*

**Overload 0:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Examples:**

```
!imgur top day 10 aww
!imgur top 10 day aww
!imgur top 5 aww
!imgur top day aww
```
---

## Group: insult
*Insults manipulation. If invoked without subcommands, insults a given user.*

**Aliases:**
`burn, insults, ins, roast`


**Arguments:**

(optional) `[user]` : *User to insult.* (def: `None`)

**Examples:**

```
!insult @Someone
```
---

### insult add
*Add insult to list (use %user% instead of user mention).*

**Owner-only.**

**Aliases:**
`+, new, a`


**Arguments:**

`[string...]` : *Insult (must contain ``%user%``).*

**Examples:**

```
!insult add You are so dumb, %user%!
```
---

### insult clear
*Delete all insults.*

**Owner-only.**

**Aliases:**
`da, c, ca, cl, clearall`


**Examples:**

```
!insults clear
```
---

### insult delete
*Remove insult with a given index from list. (use command ``insults list`` to view insult indexes).*

**Owner-only.**

**Aliases:**
`-, remove, del, rm, rem, d`


**Arguments:**

`[int]` : *Index of the insult to remove.*

**Examples:**

```
!insult delete 2
```
---

### insult list
*Show all insults.*

**Aliases:**
`ls, l`


**Examples:**

```
!insult list
```
---

## invite
*Get an instant invite link for the current guild.*

**Requires permissions:**
`Create instant invites`

**Aliases:**
`getinvite`


**Examples:**

```
!invite
```
---

## Group: joke
*Group for searching jokes. If invoked without a subcommand, returns a random joke.*

**Aliases:**
`jokes, j`


**Examples:**

```
!joke
```
---

### joke search
*Search for the joke containing the given query.*

**Aliases:**
`s`


**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!joke search blonde
```
---

### joke yourmom
*Yo mama so...*

**Aliases:**
`mama, m, yomomma, yomom, yomoma, yomamma, yomama`


**Examples:**

```
!joke yourmom
```
---

## leave
*Makes Godfather leave the guild.*

**Requires user permissions:**
`Administrator`


**Examples:**

```
!leave
```
---

## leet
*Wr1t3s m3ss@g3 1n 1337sp34k.*

**Aliases:**
`l33t`


**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!leet Some sentence
```
---

## Group: meme
*Manipulate guild memes. When invoked without subcommands, returns a meme from this guild's meme list given by name, otherwise returns random one.*

**Aliases:**
`memes, mm`


**Arguments:**

(optional) `[string...]` : *Meme name.* (def: `None`)

**Examples:**

```
!meme
!meme SomeMemeNameWhichYouAdded
```
---

### meme add
*Add a new meme to the list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new, a`


**Arguments:**

`[string]` : *Short name (case insensitive).*

`[string]` : *URL.*

**Examples:**

```
!meme add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg
```
---

### meme clear
*Deletes all guild memes.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, ca, cl, clearall`


**Examples:**

```
!memes clear
```
---

### meme create
*Creates a new meme from blank template.*

**Requires permissions:**
`Use embeds`

**Aliases:**
`maker, c, make, m`


**Arguments:**

`[string]` : *Template.*

`[string]` : *Top Text.*

`[string]` : *Bottom Text.*

**Examples:**

```
!meme create 1stworld "Top text" "Bottom text"
```
---

### meme delete
*Deletes a meme from this guild's meme list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, del, remove, rm, d, rem`


**Arguments:**

`[string]` : *Short name (case insensitive).*

**Examples:**

```
!meme delete pepe
```
---

### meme list
*List all registered memes for this guild.*

**Aliases:**
`ls, l`


**Examples:**

```
!meme list
```
---

### meme templates
*Lists all available meme templates.*

**Aliases:**
`template, t`


**Examples:**

```
!meme templates
```
---

### message attachments
*View all message attachments. If the message is not provided, uses the last sent message before command invocation.*

**Aliases:**
`a, files, la`


**Arguments:**

(optional) `[unsigned long]` : *Message ID.* (def: `0`)

**Examples:**

```
!message attachments
!message attachments 408226948855234561
```
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

**Examples:**

```
!messages delete 10
!messages delete 10 Cleaning spam
```
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

**Examples:**

```
!messages deletefrom @Someone 10 Cleaning spam
!messages deletefrom 10 @Someone Cleaning spam
```
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

**Examples:**

```
!messages deletereactions 408226948855234561
```
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

**Examples:**

```
!messages deletefrom s+p+a+m+ 10 Cleaning spam
!messages deletefrom 10 s+p+a+m+ Cleaning spam
```
---

### message listpinned
*List pinned messages in this channel.*

**Aliases:**
`lp, listpins, listpin, pinned`


**Examples:**

```
!messages listpinned
```
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

**Examples:**

```
!messages modify 408226948855234561 modified text
```
---

### message pin
*Pins the message given by ID. If the message is not provided, pins the last sent message before command invocation.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`p`


**Arguments:**

(optional) `[unsigned long]` : *ID.* (def: `0`)

**Examples:**

```
!messages pin
!messages pin 408226948855234561
```
---

### message unpin
*Unpins the message at given index (starting from 1). If the index is not given, unpins the most recent one.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`up`


**Arguments:**

(optional) `[int]` : *Index (starting from 1).* (def: `1`)

**Examples:**

```
!messages unpin
!messages unpin 10
```
---

### message unpinall
*Unpins all pinned messages in this channel.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`upa`


**Examples:**

```
!messages unpinall
```
---

## news
*Get newest world news.*

**Aliases:**
`worldnews`


**Examples:**

```
!news
```
---

## Group: owner birthdays
*Birthday notifications management. If invoked without subcommand, lists all birthdays registered.*

**Owner-only.**

**Aliases:**
`birthday, bday, bd, bdays`


---

### owner birthdays add
*Add a birthday to the database. If date is not specified, uses the current date as a birthday date. If the channel is not specified, uses the current channel.*

**Aliases:**
`+, a`


**Overload 1:**

`[user]` : *Birthday boy/girl.*

(optional) `[string]` : *Birth date.* (def: `None`)

(optional) `[channel]` : *Channel to send a greeting message to.* (def: `None`)

**Overload 0:**

`[user]` : *Birthday boy/girl.*

(optional) `[channel]` : *Channel to send a greeting message to.* (def: `None`)

(optional) `[string]` : *Birth date.* (def: `None`)

**Examples:**

```
!owner birthday add @Someone
!owner birthday add @Someone #channel_to_send_message_to
!owner birthday add @Someone 15.2.1990
!owner birthday add @Someone #channel_to_send_message_to 15.2.1990
!owner birthday add @Someone 15.2.1990 #channel_to_send_message_to
```
---

### owner birthdays delete
*Remove status from running queue.*

**Aliases:**
`-, remove, rm, del`


**Arguments:**

`[user]` : *User whose birthday to remove.*

**Examples:**

```
!owner birthday delete @Someone
```
---

### owner birthdays list
*List all registered birthdays.*

**Aliases:**
`ls`


**Examples:**

```
!owner birthday list
```
---

## Group: owner blockedchannels
*Manipulate blocked channels. Bot will not listen for commands in blocked channels or react (either with text or emoji) to messages inside.*

**Owner-only.**

**Aliases:**
`bc, blockedc, blockchannel, bchannels, bchannel, bchn`


---

### owner blockedchannels add
*Add channel to blocked channels list.*

**Aliases:**
`+, a`


**Overload 2:**

`[channel...]` : *Channels to block.*

**Overload 1:**

`[string]` : *Reason (max 60 chars).*

`[channel...]` : *Channels to block.*

**Overload 0:**

`[channel]` : *Channels to block.*

`[string...]` : *Reason (max 60 chars).*

**Examples:**

```
!owner blockedchannels add #channel
!owner blockedchannels add #channel Some reason for blocking
!owner blockedchannels add 123123123123123
!owner blockedchannels add #channel 123123123123123
!owner blockedchannels add "This is some reason" #channel 123123123123123
```
---

### owner blockedchannels delete
*Remove channel from blocked channels list..*

**Aliases:**
`-, remove, rm, del`


**Arguments:**

`[channel...]` : *Channels to unblock.*

**Examples:**

```
!owner blockedchannels remove #channel
!owner blockedchannels remove 123123123123123
!owner blockedchannels remove @Someone 123123123123123
```
---

### owner blockedchannels list
*List all blocked channels.*

**Aliases:**
`ls`


**Examples:**

```
!owner blockedchannels list
```
---

## Group: owner blockedusers
*Manipulate blocked users. Bot will not allow blocked users to invoke commands and will not react (either with text or emoji) to their messages.*

**Owner-only.**

**Aliases:**
`bu, blockedu, blockuser, busers, buser, busr`


---

### owner blockedusers add
*Add users to blocked users list.*

**Aliases:**
`+, a`


**Overload 2:**

`[user...]` : *Users to block.*

**Overload 1:**

`[string]` : *Reason (max 60 chars).*

`[user...]` : *Users to block.*

**Overload 0:**

`[user]` : *Users to block.*

`[string...]` : *Reason (max 60 chars).*

**Examples:**

```
!owner blockedusers add @Someone
!owner blockedusers add @Someone Troublemaker and spammer
!owner blockedusers add 123123123123123
!owner blockedusers add @Someone 123123123123123
!owner blockedusers add "This is some reason" @Someone 123123123123123
```
---

### owner blockedusers delete
*Remove users from blocked users list..*

**Aliases:**
`-, remove, rm, del`


**Arguments:**

`[user...]` : *Users to unblock.*

**Examples:**

```
!owner blockedusers remove @Someone
!owner blockedusers remove 123123123123123
!owner blockedusers remove @Someone 123123123123123
```
---

### owner blockedusers list
*List all blocked users.*

**Aliases:**
`ls`


**Examples:**

```
!owner blockedusers list
```
---

### owner botavatar
*Set bot avatar.*

**Owner-only.**

**Aliases:**
`setbotavatar, setavatar`


**Arguments:**

`[string]` : *URL.*

**Examples:**

```
!owner botavatar http://someimage.png
```
---

### owner botname
*Set bot name.*

**Owner-only.**

**Aliases:**
`setbotname, setname`


**Arguments:**

`[string...]` : *New name.*

**Examples:**

```
!owner setname TheBotfather
```
---

### owner clearlog
*Clear application logs.*

**Owner-only.**

**Aliases:**
`clearlogs, deletelogs, deletelog`


**Examples:**

```
!owner clearlog
```
---

### owner dbquery
*Clear application logs.*

**Owner-only.**

**Aliases:**
`sql, dbq, q`


**Arguments:**

`[string...]` : *SQL Query.*

**Examples:**

```
!owner dbquery SELECT * FROM gf.msgcount;
```
---

### owner eval
*Evaluates a snippet of C# code, in context. Surround the code in the code block.*

**Owner-only.**

**Aliases:**
`compile, run, e, c, r`


**Arguments:**

`[string...]` : *Code to evaluate.*

**Examples:**

```
!owner eval ```await Context.RespondAsync("Hello!");```
```
---

### owner filelog
*Toggle writing to log file.*

**Owner-only.**

**Aliases:**
`setfl, fl, setfilelog`


**Arguments:**

(optional) `[boolean]` : *True/False* (def: `True`)

**Examples:**

```
!owner filelog yes
!owner filelog false
```
---

### owner generatecommandlist
*Generates a markdown command-list. You can also provide a file path for the output.*

**Owner-only.**

**Aliases:**
`cmdlist, gencmdlist, gencmds, gencmdslist`


**Arguments:**

(optional) `[string...]` : *File path.* (def: `None`)

**Examples:**

```
!owner generatecommandlist
!owner generatecommandlist Temp/blabla.md
```
---

### owner leaveguilds
*Leaves the given guilds.*

**Owner-only.**

**Aliases:**
`leave, gtfo`


**Arguments:**

`[unsigned long...]` : *Guild ID list.*

**Examples:**

```
!owner leave 337570344149975050
!owner leave 337570344149975050 201315884709576708
```
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

**Examples:**

```
!owner send u 303463460233150464 Hi to user!
!owner send c 120233460278590414 Hi to channel!
```
---

### owner shutdown
*Triggers the dying in the vineyard scene (power off the bot).*

**Owner-only.**

**Aliases:**
`disable, poweroff, exit, quit`


**Overload 1:**

`[time span]` : *Time until shutdown.*

**Examples:**

```
!owner shutdown
```
---

### owner statuses add
*Add a status to running status queue.*

**Aliases:**
`+, a`


**Arguments:**

`[ActivityType]` : *Activity type (Playing/Watching/Streaming/ListeningTo).*

`[string...]` : *Status.*

**Examples:**

```
!owner status add Playing CS:GO
!owner status add Streaming on Twitch
```
---

### owner statuses delete
*Remove status from running queue.*

**Aliases:**
`-, remove, rm, del`


**Arguments:**

`[int]` : *Status ID.*

**Examples:**

```
!owner status delete 1
```
---

### owner statuses list
*List all bot statuses.*

**Aliases:**
`ls`


**Examples:**

```
!owner status list
```
---

### owner statuses set
*Set status to given string or status with given index in database. This sets rotation to false.*

**Aliases:**
`s`


**Overload 1:**

`[ActivityType]` : *Activity type (Playing/Watching/Streaming/ListeningTo).*

`[string...]` : *Status.*

**Overload 0:**

`[int]` : *Status ID.*

**Examples:**

```
!owner status set Playing with fire
!owner status set 5
```
---

### owner statuses setrotation
*Set automatic rotation of bot statuses.*

**Aliases:**
`sr, setr`


**Arguments:**

(optional) `[boolean]` : *True/False* (def: `True`)

**Examples:**

```
!owner status setrotation
!owner status setrotation false
```
---

### owner sudo
*Executes a command as another user.*

**Owner-only.**

**Aliases:**
`execas, as`


**Arguments:**

`[member]` : *Member to execute as.*

`[string...]` : *Command text to execute.*

**Examples:**

```
!owner sudo @Someone !rate
```
---

### owner toggleignore
*Toggle bot's reaction to commands.*

**Owner-only.**

**Aliases:**
`ti`


**Examples:**

```
!owner toggleignore
```
---

## penis
*An accurate measurement.*

**Aliases:**
`size, length, manhood, dick`


**Arguments:**

(optional) `[user]` : *Who to measure.* (def: `None`)

**Examples:**

```
!penis @Someone
```
---

## peniscompare
*Comparison of the results given by ``penis`` command.*

**Aliases:**
`sizecompare, comparesize, comparepenis, cmppenis, peniscmp, comppenis`


**Arguments:**

`[user]` : *User1.*

(optional) `[user]` : *User2 (def. sender).* (def: `None`)

**Examples:**

```
!peniscompare @Someone
!peniscompare @Someone @SomeoneElse
```
---

## ping
*Ping the bot.*


**Examples:**

```
!ping
```
---

## Group: play
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
*Starts a new poll in the current channel. You can provide also the time for the poll to run.*


**Overload 1:**

`[time span]` : *Time for poll to run.*

`[string...]` : *Question.*

**Overload 0:**

`[string...]` : *Question.*

**Examples:**

```
!poll Do you vote for User1 or User2?
!poll 5m Do you vote for User1 or User2?
```
---

## prefix
*Get current guild prefix, or change it.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`setprefix, pref, setpref`


**Arguments:**

(optional) `[string]` : *Prefix to set.* (def: `None`)

**Examples:**

```
!prefix
!prefix ;
```
---

### random cat
*Get a random cat image.*


**Examples:**

```
!random cat
```
---

### random choose
*Choose one of the provided options separated by comma.*

**Aliases:**
`select`


**Arguments:**

`[string...]` : *Option list (separated by comma).*

**Examples:**

```
!random choose option 1, option 2, option 3...
```
---

### random dog
*Get a random dog image.*


**Examples:**

```
!random dog
```
---

### random raffle
*Choose a user from the online members list belonging to a given role.*

**Aliases:**
`chooseuser`


**Arguments:**

(optional) `[role]` : *Role.* (def: `None`)

**Examples:**

```
!random raffle
!random raffle Admins
```
---

## Group: rank
*User ranking commands. If invoked without subcommands, prints sender's rank.*

**Aliases:**
`ranks, ranking, level`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!rank
!rank @Someone
```
---

### rank list
*Print all available ranks.*

**Aliases:**
`levels`


**Examples:**

```
!rank list
```
---

### rank top
*Get rank leaderboard.*


**Examples:**

```
!rank top
```
---

## rate
*Gives a rating chart for the user. If the user is not provided, rates sender.*

**Requires bot permissions:**
`Attach files`

**Aliases:**
`score, graph`


**Arguments:**

(optional) `[user]` : *Who to measure.* (def: `None`)

**Examples:**

```
!rate @Someone
```
---

## reactionspoll
*Starts a poll with reactions in the channel.*

**Aliases:**
`rpoll, pollr, voter`


**Overload 1:**

`[time span]` : *Time for poll to run.*

`[string...]` : *Question.*

**Overload 0:**

`[string...]` : *Question.*

**Examples:**

```
!rpoll :smile: :joy:
```
---

## Group: reddit
*Reddit commands.*

**Aliases:**
`r`


**Arguments:**

(optional) `[string]` : *Subreddit.* (def: `all`)

**Examples:**

```
!reddit aww
```
---

### reddit subscribe
*Add new feed for a subreddit.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`add, a, +, sub`


**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!reddit sub aww
```
---

### reddit unsubscribe
*Remove a subreddit feed using subreddit name or subscription ID (use command ``feed list`` to see IDs).*

**Requires permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`


**Overload 1:**

`[string]` : *Subreddit.*

**Overload 0:**

`[int]` : *Subscription ID.*

**Examples:**

```
!reddit unsub aww
!reddit unsub 12
```
---

## remind
*Resend a message after some time.*

**Requires user permissions:**
`Administrator`


**Overload 2:**

`[time span]` : *Time span until reminder.*

`[channel]` : *Channel to send message to.*

`[string...]` : *What to send?*

**Overload 1:**

`[channel]` : *Channel to send message to.*

`[time span]` : *Time span until reminder.*

`[string...]` : *What to send?*

**Overload 0:**

`[time span]` : *Time span until reminder.*

`[string...]` : *What to send?*

**Examples:**

```
!remind 1h Drink water!
```
---

## report
*Send a report message to owner about a bug (please don't abuse... please).*


**Arguments:**

`[string...]` : *Issue text.*

**Examples:**

```
!report Your bot sucks!
```
---

## Group: roles
*Miscellaneous role control commands.*

**Aliases:**
`role, rl`


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

**Examples:**

```
!roles create "My role" #C77B0F no no
!roles create 
!roles create #C77B0F My new role
```
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

**Examples:**

```
!role delete My role
!role delete @admins
```
---

### roles info
*Get information about a given role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`i`


**Arguments:**

`[role]` : *Role.*

**Examples:**

```
!role info Admins
```
---

### roles mentionall
*Mention all users from given role.*

**Requires permissions:**
`Mention everyone`

**Requires bot permissions:**
`Manage roles`

**Aliases:**
`mention, @, ma`


**Arguments:**

`[role]` : *Role.*

**Examples:**

```
!role mentionall Admins
```
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

**Examples:**

```
!role setcolor #FF0000 Admins
!role setcolor Admins #FF0000
```
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

**Examples:**

```
!role setmentionable Admins
!role setmentionable Admins false
!role setmentionable false Admins
```
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

**Examples:**

```
!role setname @Admins Administrators
!role setname Administrators @Admins
```
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

**Examples:**

```
!role setvisible Admins
!role setvisible Admins false
!role setvisible false Admins
```
---

## Group: rss
*Commands for RSS feed querying or subscribing. If invoked without subcommand, gives the latest topic from the given RSS URL.*

**Aliases:**
`feed`


**Arguments:**

`[string...]` : *RSS URL.*

**Examples:**

```
!rss https://news.google.com/news/rss/
```
---

### rss list
*Get feed list for the current channel.*

**Aliases:**
`ls, listsubs, listfeeds`


**Examples:**

```
!feed list
```
---

### rss subscribe
*Subscribe to given RSS feed URL. The bot will send a message when the latest topic is changed.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`sub, add, +`


**Arguments:**

`[string]` : *URL.*

(optional) `[string]` : *Friendly name.* (def: `None`)

**Examples:**

```
!rss subscribe https://news.google.com/news/rss/
!rss subscribe https://news.google.com/news/rss/ news
```
---

### rss unsubscribe
*Remove an existing feed subscription.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`


**Overload 1:**

`[int]` : *ID of the subscription.*

**Overload 0:**

`[string]` : *Name of the subscription.*

**Examples:**

```
!rss unsubscribe 1
```
---

### rss wm
*Get newest topics from WM forum.*


**Examples:**

```
!rss wm
```
---

## say
*Echo echo echo.*

**Aliases:**
`repeat`


**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!say I am gay.
```
---

## Group: selfassignableroles
*Commands to manipulate self-assignable roles. If invoked alone, lists all allowed self-assignable roles in this guild.*

**Aliases:**
`sar`


**Examples:**

```
!sar
```
---

### selfassignableroles add
*Add a self-assignable role (or roles) for this guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`a, +`


**Arguments:**

`[role...]` : *Roles to add.*

**Examples:**

```
!sar add @Notifications
!sar add @Notifications @Role1 @Role2
```
---

### selfassignableroles clear
*Delete all self-assignable roles for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c, ca, cl, clearall`


**Examples:**

```
!sar clear
```
---

### selfassignableroles delete
*Remove self-assignable role (or roles).*

**Requires user permissions:**
`Administrator`

**Aliases:**
`remove, del, -, d`


**Arguments:**

`[role...]` : *Roles to delete.*

**Examples:**

```
!sar delete @Notifications
!sar delete @Notifications @Role1 @Role2
```
---

### selfassignableroles list
*View all self-assignable roles in the current guild.*

**Aliases:**
`print, show, l, p`


**Examples:**

```
!sar list
```
---

### steam profile
*Get Steam user information for user based on his ID.*

**Aliases:**
`id, user`


**Arguments:**

`[unsigned long]` : *ID.*

---

## stop
*Stops current voice playback.*

**Owner-only.**


---

### swat ip
*Return IP of the registered server by name.*

**Aliases:**
`getip`


**Arguments:**

`[string]` : *Registered name.*

**Examples:**

```
!s4 ip wm
```
---

### swat query
*Return server information.*

**Aliases:**
`q, info, i`


**Arguments:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Examples:**

```
!s4 q 109.70.149.158
!s4 q 109.70.149.158:10480
!s4 q wm
```
---

### swat serverlist
*Print the serverlist with current player numbers.*


**Examples:**

```
!swat serverlist
```
---

### swat servers add
*Add a server to serverlist.*

**Owner-only.**

**Aliases:**
`+, a`


**Arguments:**

`[string]` : *Name.*

`[string]` : *IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Examples:**

```
!swat servers add 4u 109.70.149.158:10480
!swat servers add 4u 109.70.149.158:10480 10481
```
---

### swat servers delete
*Remove a server from serverlist.*

**Owner-only.**

**Aliases:**
`-, del, d`


**Arguments:**

`[string]` : *Name.*

**Examples:**

```
!swat servers delete 4u
```
---

### swat servers list
*List all registered servers.*

**Owner-only.**

**Aliases:**
`ls, l`


**Examples:**

```
!swat servers list
```
---

### swat settimeout
*Set checking timeout.*

**Owner-only.**


**Arguments:**

`[int]` : *Timeout (in ms).*

**Examples:**

```
!swat settimeout 500
```
---

### swat startcheck
*Start listening for space on a given server and notifies you when there is space.*

**Aliases:**
`checkspace, spacecheck`


**Arguments:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Examples:**

```
!s4 startcheck 109.70.149.158
!s4 startcheck 109.70.149.158:10480
!swat startcheck wm
```
---

### swat stopcheck
*Stops space checking.*

**Aliases:**
`checkstop`


**Examples:**

```
!swat stopcheck
```
---

## Group: textreaction
*Orders a bot to react with given text to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new text reaction to a given trigger word. Note: Trigger words can be regular expressions (use ``textreaction addregex`` command). You can also use "%user%" inside response and the bot will replace it with mention for the user who triggers the reaction.*

**Aliases:**
`treact, tr, txtr, textreactions`


**Arguments:**

`[string]` : *Trigger string (case insensitive).*

`[string...]` : *Response.*

**Examples:**

```
!textreaction hi hello
!textreaction "hi" "Hello, %user%!"
```
---

### textreaction add
*Add a new text reaction to guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new, a`


**Arguments:**

`[string]` : *Trigger string (case insensitive).*

`[string...]` : *Response.*

**Examples:**

```
!textreaction add "hi" "Hello, %user%!"
```
---

### textreaction addregex
*Add a new text reaction triggered by a regex to guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+r, +regex, +regexp, +rgx, newregex, addrgx`


**Arguments:**

`[string]` : *Regex (case insensitive).*

`[string...]` : *Response.*

**Examples:**

```
!textreaction addregex "h(i|ey|ello|owdy)" "Hello, %user%!"
```
---

### textreaction clear
*Delete all text reactions for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c, ca, cl, clearall`


**Examples:**

```
!textreactions clear
```
---

### textreaction delete
*Remove text reaction from guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, del, rm, d`


**Overload 1:**

`[int...]` : *IDs of the reactions to remove.*

**Overload 0:**

`[string...]` : *Trigger words to remove.*

**Examples:**

```
!textreaction delete 5
!textreaction delete 5 8
!textreaction delete hi
```
---

### textreaction list
*Show all text reactions for the guild.*

**Aliases:**
`ls, l, view`


**Examples:**

```
!textreactions list
```
---

## tts
*Sends a tts message.*


**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!tts I am gay.
```
---

## Group: urbandict
*Urban Dictionary commands. If invoked without subcommand, searches Urban Dictionary for a given query.*

**Aliases:**
`ud, urban`


**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!urbandict blonde
```
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

**Examples:**

```
!user addrole @User Admins
!user addrole Admins @User
```
---

### user avatar
*Get avatar from user.*

**Aliases:**
`a, pic, profilepic`


**Arguments:**

`[user]` : *User.*

**Examples:**

```
!user avatar @Someone
```
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

**Examples:**

```
!user ban @Someone
!user ban @Someone Troublemaker
```
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

**Examples:**

```
!user banid 154956794490845232
!user banid 154558794490846232 Troublemaker
```
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

**Examples:**

```
!user deafen @Someone
```
---

### user info
*Print the information about the given user. If the user is not given, uses the sender.*

**Aliases:**
`i, information`


**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!user info @Someone
```
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

**Examples:**

```
!user kick @Someone
!user kick @Someone Troublemaker
```
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

**Examples:**

```
!user mute @Someone
!user mute @Someone Trashtalk
```
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

**Examples:**

```
!user removeallroles @Someone
```
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

**Examples:**

```
!user removerole @Someone Admins
!user removerole Admins @Someone
```
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

**Examples:**

```
!user setname @Someone Newname
```
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

**Examples:**

```
!user sban @Someone
!user sban @Someone Troublemaker
```
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

**Examples:**

```
!user tempban @Someone 3h4m
!user tempban 5d @Someone Troublemaker
!user tempban @Someone 5h30m30s Troublemaker
```
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

**Examples:**

```
!user unban 154956794490845232
```
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

**Examples:**

```
!user undeafen @Someone
```
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

**Examples:**

```
!user unmute @Someone
!user unmute @Someone Some reason
```
---

### user warn
*Warn a member in private message by sending a given warning text.*

**Requires user permissions:**
`Kick members`

**Aliases:**
`w`


**Arguments:**

`[member]` : *Member.*

(optional) `[string...]` : *Warning message.* (def: `None`)

**Examples:**

```
!user warn @Someone Stop spamming or kick!
```
---

## vote
*Vote for an option in the current running poll.*


**Arguments:**

`[int]` : *Option to vote for.*

**Examples:**

```
!vote 1
```
---

## weather
*Return weather information for given query.*

**Aliases:**
`w`


**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!weather london
```
---

## Group: youtube
*Youtube search commands. If invoked without subcommands, searches YouTube for given query.*

**Aliases:**
`y, yt, ytube`


**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube never gonna give you up
```
---

### youtube search
*Advanced youtube search.*

**Aliases:**
`s`


**Arguments:**

`[int]` : *Amount of results. [1-10]*

`[string...]` : *Search query.*

**Examples:**

```
!youtube search 5 rick astley
```
---

### youtube searchchannel
*Advanced youtube search for channels only.*

**Aliases:**
`sc, searchc`


**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube searchchannel 5 rick astley
```
---

### youtube searchp
*Advanced youtube search for playlists only.*

**Aliases:**
`sp, searchplaylist`


**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube searchplaylist 5 rick astley
```
---

### youtube searchvideo
*Advanced youtube search for videos only.*

**Aliases:**
`sv, searchv`


**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube searchvideo 5 rick astley
```
---

### youtube subscribe
*Add a new subscription for a YouTube channel.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`add, a, +, sub`


**Arguments:**

`[string]` : *Channel URL.*

(optional) `[string]` : *Friendly name.* (def: `None`)

**Examples:**

```
!youtube subscribe https://www.youtube.com/user/RickAstleyVEVO
!youtube subscribe https://www.youtube.com/user/RickAstleyVEVO rick
```
---

### youtube unsubscribe
*Remove a YouTube channel subscription.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`


**Arguments:**

`[string]` : *Channel URL or subscription name.*

**Examples:**

```
!youtube unsubscribe https://www.youtube.com/user/RickAstleyVEVO
!youtube unsubscribe rick
```
---

## zugify
*I don't even...*

**Aliases:**
`z`


**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!zugify Some random text
```
---

