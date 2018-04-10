# Module: Administration

## Group: automaticroles
*Commands to manipulate automatically assigned roles (roles which get automatically granted to a user who enters the guild). If invoked without command, either lists or adds automatic role depending if argument is given.*

**Aliases:**
`ar`

**Overload 0:**

`[role...]` : *Roles to add.*

**Examples:**

```
!ar
```
---

### automaticroles add
*Add an automatic role (or roles) for this guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`a, +`

**Arguments:**

`[role...]` : *Roles to add.*

**Examples:**

```
!ar add @Notifications
!ar add @Notifications @Role1 @Role2
```
---

### automaticroles clear
*Delete all automatic roles for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c, ca, cl, clearall`

**Examples:**

```
!ar clear
```
---

### automaticroles delete
*Remove automatic role (or roles).*

**Requires user permissions:**
`Administrator`

**Aliases:**
`remove, del, -, d`

**Arguments:**

`[role...]` : *Roles to delete.*

**Examples:**

```
!ar delete @Notifications
!ar delete @Notifications @Role1 @Role2
```
---

### automaticroles list
*View all automatic roles in the current guild.*

**Aliases:**
`print, show, l, p`

**Examples:**

```
!ar list
```
---

## Group: channel
*Miscellaneous channel control commands. If invoked without subcommands, prints out channel information.*

**Aliases:**
`channels, c, chn`

**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

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

## Group: emoji
*Manipulate guild emoji. Standalone call lists all guild emoji or gives information about given emoji.*

**Aliases:**
`emojis, e`

**Overload 0:**

`[emoji]` : *Emoji to print information about.*

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

### emoji info
*Get information for given guild emoji.*

**Aliases:**
`details, information, i`

**Arguments:**

`[emoji]` : *Emoji.*

**Examples:**

```
!emoji info pepe
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

## Group: filter
*Message filtering commands. If invoked without subcommand, either lists all filters or adds a new filter for the given word list. Words can be regular expressions.*

**Aliases:**
`f, filters`

**Overload 0:**

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

## Group: guild
*Miscellaneous guild control commands. If invoked without subcommands, prints guild information.*

**Aliases:**
`server, g`

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

## Group: roles
*Miscellaneous role control commands.*

**Aliases:**
`role, rl`

**Overload 0:**

`[role]` : *Role.*

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

**Overload 1:**

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

## Group: selfassignableroles
*Commands to manipulate self-assignable roles. If invoked without subcommands, lists all self-assignable roles for this guild or adds a new self-assignable role depending of argument given.*

**Aliases:**
`sar`

**Overload 0:**

`[role...]` : *Roles to add.*

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

## Group: user
*Miscellaneous user control commands. If invoked without subcommands, prints out user information.*

**Aliases:**
`users, u, usr`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

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
`deaf, d, df, deafenon`

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
`udeaf, ud, udf, deafenoff`

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

