# Module: Administration

## Group: automaticroles
<details><summary markdown='span'>Expand for additional information</summary><code>

*Commands to manipulate automatically assigned roles (roles which get automatically granted to a user who enters the guild). If invoked without command, either lists or adds automatic role depending if argument is given.*

**Aliases:**
`ar`

**Overload 0:**

`[role...]` : *Roles to add.*

**Examples:**

```
!ar
```
</code></details>

---

### automaticroles add
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### automaticroles clear
<details><summary markdown='span'>Expand for additional information</summary><code>

*Delete all automatic roles for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c, ca, cl, clearall`

**Examples:**

```
!ar clear
```
</code></details>

---

### automaticroles delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### automaticroles list
<details><summary markdown='span'>Expand for additional information</summary><code>

*View all automatic roles in the current guild.*

**Aliases:**
`print, show, l, p`

**Examples:**

```
!ar list
```
</code></details>

---

## Group: channel
<details><summary markdown='span'>Expand for additional information</summary><code>

*Miscellaneous channel control commands. If invoked without subcommands, prints out channel information.*

**Aliases:**
`channels, c, chn`

**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

</code></details>

---

### channel createcategory
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### channel createtext
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### channel createvoice
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### channel delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### channel info
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### channel modify
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### channel rename
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### channel setparent
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### channel setposition
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### channel settopic
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### channel viewperms
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

## Group: emoji
<details><summary markdown='span'>Expand for additional information</summary><code>

*Manipulate guild emoji. Standalone call lists all guild emoji or gives information about given emoji.*

**Aliases:**
`emojis, e`

**Overload 0:**

`[emoji]` : *Emoji to print information about.*

**Examples:**

```
!emoji
```
</code></details>

---

### emoji add
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### emoji delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### emoji info
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get information for given guild emoji.*

**Aliases:**
`details, information, i`

**Arguments:**

`[emoji]` : *Emoji.*

**Examples:**

```
!emoji info pepe
```
</code></details>

---

### emoji list
<details><summary markdown='span'>Expand for additional information</summary><code>

*View guild emojis.*

**Aliases:**
`print, show, l, p, ls`

**Examples:**

```
!emoji list
```
</code></details>

---

### emoji modify
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

## Group: filter
<details><summary markdown='span'>Expand for additional information</summary><code>

*Message filtering commands. If invoked without subcommand, either lists all filters or adds a new filter for the given word list. Words can be regular expressions.*

**Aliases:**
`f, filters`

**Overload 0:**

`[string...]` : *Filter list. Filter is a regular expression (case insensitive).*

**Examples:**

```
!filter fuck fk f+u+c+k+
```
</code></details>

---

### filter add
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### filter clear
<details><summary markdown='span'>Expand for additional information</summary><code>

*Delete all filters for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c, ca, cl, clearall`

**Examples:**

```
!filter clear
```
</code></details>

---

### filter delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### filter list
<details><summary markdown='span'>Expand for additional information</summary><code>

*Show all filters for this guild.*

**Aliases:**
`ls, l`

**Examples:**

```
!filter list
```
</code></details>

---

## Group: guild
<details><summary markdown='span'>Expand for additional information</summary><code>

*Miscellaneous guild control commands. If invoked without subcommands, prints guild information.*

**Aliases:**
`server, g`

</code></details>

---

### guild bans
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get guild ban list.*

**Requires permissions:**
`View audit log`

**Aliases:**
`banlist, viewbanlist, getbanlist, getbans, viewbans`

**Examples:**

```
!guild banlist
```
</code></details>

---

### guild deleteleavechannel
<details><summary markdown='span'>Expand for additional information</summary><code>

*Remove leave message channel for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`delleavec, dellc, delleave, dlc`

**Examples:**

```
!guild deletewelcomechannel
```
</code></details>

---

### guild deleteleavemessage
<details><summary markdown='span'>Expand for additional information</summary><code>

*Remove leave message for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`delleavem, dellm, delleavemsg, dlm, deletelm, dwlsg`

**Examples:**

```
!guild deleteleavemessage
```
</code></details>

---

### guild deletewelcomechannel
<details><summary markdown='span'>Expand for additional information</summary><code>

*Remove welcome message channel for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`delwelcomec, delwc, delwelcome, dwc, deletewc`

**Examples:**

```
!guild deletewelcomechannel
```
</code></details>

---

### guild deletewelcomemessage
<details><summary markdown='span'>Expand for additional information</summary><code>

*Remove welcome message for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`delwelcomem, delwm, delwelcomemsg, dwm, deletewm, dwmsg`

**Examples:**

```
!guild deletewelcomemessage
```
</code></details>

---

### guild getleavechannel
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get current leave message channel for this guild.*

**Aliases:**
`getleavec, getlc, leavechannel, lc`

**Examples:**

```
!guild getleavechannel
```
</code></details>

---

### guild getleavemessage
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get current leave message for this guild.*

**Aliases:**
`getleavem, getlm, leavemessage, lm, leavemsg, lmsg`

**Examples:**

```
!guild getwelcomemessage
```
</code></details>

---

### guild getwelcomechannel
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get current welcome message channel for this guild.*

**Aliases:**
`getwelcomec, getwc, welcomechannel, wc`

**Examples:**

```
!guild getwelcomechannel
```
</code></details>

---

### guild getwelcomemessage
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get current welcome message for this guild.*

**Aliases:**
`getwelcomem, getwm, welcomemessage, wm, welcomemsg, wmsg`

**Examples:**

```
!guild getwelcomemessage
```
</code></details>

---

### guild info
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get guild information.*

**Aliases:**
`i, information`

**Examples:**

```
!guild info
```
</code></details>

---

### guild listmembers
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get guild member list.*

**Aliases:**
`memberlist, lm, members`

**Examples:**

```
!guild memberlist
```
</code></details>

---

### guild log
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get audit logs.*

**Requires permissions:**
`View audit log`

**Aliases:**
`auditlog, viewlog, getlog, getlogs, logs`

**Examples:**

```
!guild logs
```
</code></details>

---

### guild prune
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### guild rename
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### guild seticon
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### guild setleavechannel
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### guild setleavemessage
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### guild setwelcomechannel
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### guild setwelcomemessage
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### message attachments
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### message delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### message deletefrom
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### message deletereactions
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### message deleteregex
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### message listpinned
<details><summary markdown='span'>Expand for additional information</summary><code>

*List pinned messages in this channel.*

**Aliases:**
`lp, listpins, listpin, pinned`

**Examples:**

```
!messages listpinned
```
</code></details>

---

### message modify
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### message pin
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### message unpin
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### message unpinall
<details><summary markdown='span'>Expand for additional information</summary><code>

*Unpins all pinned messages in this channel.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`upa`

**Examples:**

```
!messages unpinall
```
</code></details>

---

## Group: roles
<details><summary markdown='span'>Expand for additional information</summary><code>

*Miscellaneous role control commands.*

**Aliases:**
`role, rl`

**Overload 0:**

`[role]` : *Role.*

</code></details>

---

### roles create
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### roles delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### roles info
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### roles mentionall
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### roles setcolor
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### roles setmentionable
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### roles setname
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### roles setvisible
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

## Group: selfassignableroles
<details><summary markdown='span'>Expand for additional information</summary><code>

*Commands to manipulate self-assignable roles. If invoked without subcommands, lists all self-assignable roles for this guild or adds a new self-assignable role depending of argument given.*

**Aliases:**
`sar`

**Overload 0:**

`[role...]` : *Roles to add.*

**Examples:**

```
!sar
```
</code></details>

---

### selfassignableroles add
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### selfassignableroles clear
<details><summary markdown='span'>Expand for additional information</summary><code>

*Delete all self-assignable roles for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c, ca, cl, clearall`

**Examples:**

```
!sar clear
```
</code></details>

---

### selfassignableroles delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### selfassignableroles list
<details><summary markdown='span'>Expand for additional information</summary><code>

*View all self-assignable roles in the current guild.*

**Aliases:**
`print, show, l, p`

**Examples:**

```
!sar list
```
</code></details>

---

## Group: user
<details><summary markdown='span'>Expand for additional information</summary><code>

*Miscellaneous user control commands. If invoked without subcommands, prints out user information.*

**Aliases:**
`users, u, usr`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

</code></details>

---

### user addrole
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user avatar
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get avatar from user.*

**Aliases:**
`a, pic, profilepic`

**Arguments:**

`[user]` : *User.*

**Examples:**

```
!user avatar @Someone
```
</code></details>

---

### user ban
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user banid
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user deafen
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user info
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the information about the given user. If the user is not given, uses the sender.*

**Aliases:**
`i, information`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!user info @Someone
```
</code></details>

---

### user kick
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user mute
<details><summary markdown='span'>Expand for additional information</summary><code>

*Mute a member.*

**Requires permissions:**
`Mute voice chat members`

**Aliases:**
`m`

**Arguments:**

`[member]` : *Member to mute.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user mute @Someone
!user mute @Someone Trashtalk
```
</code></details>

---

### user removeallroles
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user removerole
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user setname
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user softban
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user tempban
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user unban
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user undeafen
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### user unmute
<details><summary markdown='span'>Expand for additional information</summary><code>

*Unmute a member.*

**Requires permissions:**
`Mute voice chat members`

**Aliases:**
`um`

**Arguments:**

`[member]` : *Member to unmute.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user unmute @Someone
!user unmute @Someone Some reason
```
</code></details>

---

### user warn
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

