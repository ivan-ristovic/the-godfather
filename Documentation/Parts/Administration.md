# Command list

# Module: Administration
*This module contains commands which help administer guilds and automate some actions.*


## Group: actionhistory
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manages action history entries. Action history is a log of all actions performed in the guild and can be queried to get detailed list of all the actions performed on a given user. Action history can be enabled using the `config actionhistory` command.*

**Aliases:**
`history, ah`
**Guild only.**

**Requires user permissions:**
`View audit log`

**Overload 1:**
- [`user`]: *User*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!actionhistory
!actionhistory @User
```
</p></details>

---

### actionhistory add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new custom note entry to the action history for the given user and note.*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`View audit log`

**Overload 0:**
- [`user`]: *User*
- [`string...`]: *Reason for the action*

**Examples:**

```xml
!actionhistory add @User Reason
```
</p></details>

---

## Group: actionhistory delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manages removal of action history entries. Group call removes all entries for given user(s).*

**Aliases:**
`remove, rm, del, d, -, -=, >, >>`
**Guild only.**

**Requires user permissions:**
`View audit log`

**Overload 1:**
- [`user...`]: *User(s)*

**Examples:**

```xml
!actionhistory delete @User
```
</p></details>

---

### actionhistory delete after
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all action history entries which were recorded after the specified time.*

**Aliases:**
`aft, a`
**Guild only.**

**Requires user permissions:**
`View audit log`

**Overload 0:**
- [`date and time`]: *When?*

**Examples:**

```xml
!actionhistory delete after 13.10.2000
```
</p></details>

---

### actionhistory delete before
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all action history entries which were recorded before the specified time.*

**Aliases:**
`due, b`
**Guild only.**

**Requires user permissions:**
`View audit log`

**Overload 0:**
- [`date and time`]: *When?*

**Examples:**

```xml
!actionhistory delete before 13.10.2000
```
</p></details>

---

### actionhistory delete users
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all action history entries for given user(s).*

**Aliases:**
`members, member, mem, user, usr, m, u`
**Guild only.**

**Requires user permissions:**
`View audit log`

**Overload 0:**
- [`user...`]: *User(s)*

**Examples:**

```xml
!actionhistory delete users @User
```
</p></details>

---

### actionhistory deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all action history entries for this guild.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`View audit log`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!actionhistory deleteall
```
</p></details>

---

### actionhistory list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all action history entries for this guild or only those matching the specified user if the user is provided.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`View audit log`

**Overload 1:**
- [`user`]: *User*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!actionhistory list
!actionhistory list @User
```
</p></details>

---

## Group: automaticroles
<details><summary markdown='span'>Expand for additional information</summary><p>

*Automatic roles commands. Automatic roles are automatically granted to a new member of the guild. Group call lists all automatic roles for the guild. Group call with an arbitrary amount of roles will add those roles to the automatic roles list for the guild, effective immediately.*

**Aliases:**
`autoassignroles, autoassign, autoroles, autorole, aroles, arole, arl, ar, aar`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**

*No arguments.*

**Overload 0:**
- [`role...`]: *Roles to add*

**Examples:**

```xml
!automaticroles
!automaticroles @Role RoleName
```
</p></details>

---

### automaticroles add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Registers given role(s) as automatic.*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`role...`]: *Roles to add*

**Examples:**

```xml
!automaticroles add @Role RoleName
```
</p></details>

---

### automaticroles delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes given automatic role(s).*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`role...`]: *Roles to remove*

**Examples:**

```xml
!automaticroles delete @Role RoleName
```
</p></details>

---

### automaticroles deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all automatic roles.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!automaticroles deleteall
```
</p></details>

---

### automaticroles list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all automatic roles.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!automaticroles list
```
</p></details>

---

## Group: channel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Channel administration commands. Group call prints channel information.*

**Aliases:**
`channels, chn, ch, c`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 0:**
- (optional) [`channel`]: *Channel to view* (def: `None`)

**Examples:**

```xml
!channel
!channel #my-text-channel
```
</p></details>

---

## Group: channel add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Channel creation commands.*

**Aliases:**
`create, cr, new, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires permissions:**
`Manage channels`

</p></details>

---

### channel add category
<details><summary markdown='span'>Expand for additional information</summary><p>

*Creates a new channel category.*

**Aliases:**
`addcategory, cat, c, cc, +category, +cat, +c, <c, <<c`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 0:**
- [`string...`]: *Category name*

**Examples:**

```xml
!channel add category My Category
```
</p></details>

---

### channel add text
<details><summary markdown='span'>Expand for additional information</summary><p>

*Creates a new text channel with option to specify parent, user limit and NSFW flag.*

**Aliases:**
`addtext, addtxt, txt, ctxt, ct, +, +txt, +t, <t, <<t`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`string`]: *Channel name*
- (optional) [`channel`]: *Parent category* (def: `None`)
- (optional) [`boolean`]: *NSFW?* (def: `False`)

**Overload 1:**
- [`string`]: *Channel name*
- (optional) [`boolean`]: *NSFW?* (def: `False`)
- (optional) [`channel`]: *Parent category* (def: `None`)

**Overload 0:**
- [`channel`]: *Parent category*
- [`string`]: *Channel name*
- (optional) [`boolean`]: *NSFW?* (def: `False`)

**Examples:**

```xml
!channel add text #my-text-channel
!channel add text #my-text-channel My Category
!channel add text My Category #my-text-channel
!channel add text #my-text-channel My Category Yes/No
!channel add text My Category #my-text-channel Yes/No
!channel add text #my-text-channel My Category Yes/No
```
</p></details>

---

### channel add voice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Creates a new voice channel with option to specify channel parent, user limit and bitrate.*

**Aliases:**
`addvoice, addv, cvoice, cv, +voice, +v, <v, <<v`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`string`]: *Voice channel name*
- (optional) [`channel`]: *Parent category* (def: `None`)
- (optional) [`int`]: *User limit* (def: `None`)
- (optional) [`int`]: *Channel bitrate [8-128]* (def: `None`)

**Overload 1:**
- [`string`]: *Voice channel name*
- (optional) [`int`]: *User limit* (def: `None`)
- (optional) [`int`]: *Channel bitrate [8-128]* (def: `None`)
- (optional) [`channel`]: *Parent category* (def: `None`)

**Overload 0:**
- [`channel`]: *Parent category*
- [`string`]: *Voice channel name*
- (optional) [`int`]: *User limit* (def: `None`)
- (optional) [`int`]: *Channel bitrate [8-128]* (def: `None`)

**Examples:**

```xml
!channel add voice My Voice Channel
!channel add voice My Voice Channel My Category
!channel add voice My Category My Voice Channel
!channel add voice My Voice Channel My Category 10
!channel add voice My Category My Voice Channel 10
!channel add voice My Voice Channel My Category 10 128
```
</p></details>

---

### channel clone
<details><summary markdown='span'>Expand for additional information</summary><p>

*Clones an existing channel (current one if channel is not provided) with an optional new name.*

**Aliases:**
`copy, cp, cln`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 1:**
- [`channel`]: *Channel to clone*
- (optional) [`string...`]: *Channel to clone* (def: `None`)

**Overload 0:**
- (optional) [`string...`]: *Channel to clone* (def: `None`)

**Examples:**

```xml
!channel clone #my-text-channel
!channel clone My Voice Channel
!channel clone #my-text-channel #my-text-channel
!channel clone #my-text-channel My Voice Channel
```
</p></details>

---

### channel delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete a channel/category with optional reason for the action. If a channel is not specified, deletes the current channel.*

**Aliases:**
`remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 1:**
- [`channel`]: *Channel to delete*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`string...`]: *Reason for the action*

**Examples:**

```xml
!channel delete #my-text-channel
!channel delete My Voice Channel
!channel delete #my-text-channel Because I can!
```
</p></details>

---

### channel info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows detailed channel information. If a channel is not specified, uses the current channel.*

**Aliases:**
`information, details, about, i`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 0:**
- (optional) [`channel`]: *Channel to view* (def: `None`)

**Examples:**

```xml
!channel info
!channel info #my-text-channel
!channel info My Voice Channel
!channel info #my-text-channel Because I can!
```
</p></details>

---

## Group: channel modify
<details><summary markdown='span'>Expand for additional information</summary><p>

*Channel modification commands.*

**Aliases:**
`edit, mod, m, e, set, change`
**Guild only.**

**Requires permissions:**
`Manage channels`

</p></details>

---

### channel modify bitrate
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies voice channel bitrate.*

**Aliases:**
`br, bitr, brate, b`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 1:**
- [`channel`]: *Channel to modify*
- [`int`]: *Bitrate*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`int`]: *Bitrate*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!channel modify bitrate My Voice Channel 128
!channel modify bitrate 128 My Voice Channel Because I can!
```
</p></details>

---

### channel modify name
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel name.*

**Aliases:**
`title, nm, n`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`channel`]: *Channel to modify*
- [`string`]: *Name*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`string`]: *Name*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`string...`]: *Reason for the action*

**Examples:**

```xml
!channel modify name new_name
!channel modify name #my-text-channel new_name
!channel modify name My Voice Channel SampleName
!channel modify name new_name #my-text-channel Because I can!
```
</p></details>

---

### channel modify nsfw
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel's NSFW flag.*

**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`channel`]: *Channel to modify*
- [`boolean`]: *Name*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`boolean`]: *Name*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`boolean`]: *Reason for the action*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!channel modify nsfw Yes/No
!channel modify nsfw #my-text-channel Yes/No
!channel modify nsfw Yes/No #my-text-channel Because I can!
```
</p></details>

---

### channel modify parent
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel's parent category.*

**Aliases:**
`par`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 1:**
- [`channel...`]: *Channels to reorganize, including exactly one category to set as the parent*

**Overload 0:**
- [`string`]: *Reason for the action*
- [`channel...`]: *Channels to reorganize, including exactly one category to set as the parent*

**Examples:**

```xml
!channel modify parent #my-text-channel My Category
```
</p></details>

---

### channel modify position
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel's position.*

**Aliases:**
`pos, p, order`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`channel`]: *Channel to modify*
- [`int`]: *Position*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`int`]: *Position*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`int`]: *Position*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!channel modify position #my-text-channel 10
!channel modify position 10 My Voice Channel Because I can!
```
</p></details>

---

### channel modify slowmode
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel's slowmode settings.*

**Aliases:**
`rlimit, rl, ratel, rate, ratelimit, slow, sm, smode`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`channel`]: *Channel to modify*
- [`int`]: *Slowmode value, from set: [0, 5, 10, 15, 30, 45, 60, 75, 90, 120]*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`int`]: *Slowmode value, from set: [0, 5, 10, 15, 30, 45, 60, 75, 90, 120]*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`int`]: *Slowmode value, from set: [0, 5, 10, 15, 30, 45, 60, 75, 90, 120]*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!channel modify slowmode #my-text-channel 10
!channel modify slowmode 10 #my-text-channel Because I can!
```
</p></details>

---

### channel modify topic
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel's topic.*

**Aliases:**
`t, desc, description`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`string`]: *Reason for the action*
- [`channel`]: *Channel to modify*
- [`string...`]: *Channel topic*

**Overload 1:**
- [`channel`]: *Channel to modify*
- [`string...`]: *Channel topic*

**Overload 0:**
- [`string...`]: *Channel topic*

**Examples:**

```xml
!channel modify topic My channel topic!
!channel modify topic #my-text-channel My channel topic!
!channel modify topic My channel topic! #my-text-channel Because I can!
```
</p></details>

---

### channel modify userlimit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies voice channel user limit.*

**Aliases:**
`ul, ulimit, limit, l`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 1:**
- [`channel`]: *Channel to modify*
- [`int`]: *User limit*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`int`]: *User limit*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!channel modify userlimit My Voice Channel 10
!channel modify userlimit 10 My Voice Channel Because I can!
```
</p></details>

---

### channel setbitrate
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies voice channel bitrate.*

**Aliases:**
`setbr, setbitr, setbrate, setb, br, bitrate, bitr, brate`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 1:**
- [`channel`]: *Channel to modify*
- [`int`]: *Bitrate*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`int`]: *Bitrate*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!channel setbitrate My Voice Channel 128
!channel setbitrate 128 My Voice Channel Because I can!
```
</p></details>

---

### channel setname
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel name.*

**Aliases:**
`settitle, rename, changename, rn, rnm, name, mv`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`channel`]: *Channel to modify*
- [`string`]: *Name*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`string`]: *Name*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`string...`]: *Reason for the action*

**Examples:**

```xml
!channel setname new_name
!channel setname #my-text-channel new_name
!channel setname My Voice Channel SampleName
!channel setname new_name #my-text-channel Because I can!
```
</p></details>

---

### channel setnsfw
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel's NSFW flag.*

**Aliases:**
`nsfw`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`channel`]: *Channel to modify*
- [`boolean`]: *Name*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`boolean`]: *Name*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`boolean`]: *Reason for the action*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!channel setnsfw Yes/No
!channel setnsfw #my-text-channel Yes/No
!channel setnsfw Yes/No #my-text-channel Because I can!
```
</p></details>

---

### channel setparent
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel's parent category.*

**Aliases:**
`setpar, parent, par`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 1:**
- [`channel...`]: *Channels to reorganize, including exactly one category to set as the parent*

**Overload 0:**
- [`string`]: *Reason for the action*
- [`channel...`]: *Channels to reorganize, including exactly one category to set as the parent*

**Examples:**

```xml
!channel setparent #my-text-channel My Category
```
</p></details>

---

### channel setposition
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel's position.*

**Aliases:**
`setpos, setp, order, setorder, position, pos`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`channel`]: *Channel to modify*
- [`int`]: *Position*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`int`]: *Position*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`int`]: *Position*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!channel setposition #my-text-channel 10
!channel setposition 10 My Voice Channel Because I can!
```
</p></details>

---

### channel setslowmode
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel's slowmode settings.*

**Aliases:**
`setratel, setrl, setrate, setratelimit, setslow, slowmode, slow, sm, setsmode, smode`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`channel`]: *Channel to modify*
- [`int`]: *Slowmode value, from set: [0, 5, 10, 15, 30, 45, 60, 75, 90, 120]*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`int`]: *Slowmode value, from set: [0, 5, 10, 15, 30, 45, 60, 75, 90, 120]*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`int`]: *Slowmode value, from set: [0, 5, 10, 15, 30, 45, 60, 75, 90, 120]*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!channel setslowmode #my-text-channel 10
!channel setslowmode 10 My Voice Channel Because I can!
```
</p></details>

---

### channel settopic
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies channel's topic.*

**Aliases:**
`t, topic, sett, desc, setdesc, description, setdescription`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 2:**
- [`string`]: *Reason for the action*
- [`channel`]: *Channel to modify*
- [`string...`]: *Channel topic*

**Overload 1:**
- [`channel`]: *Channel to modify*
- [`string...`]: *Channel topic*

**Overload 0:**
- [`string...`]: *Channel topic*

**Examples:**

```xml
!channel settopic My channel topic!
!channel settopic #my-text-channel My channel topic!
!channel settopic My channel topic! #my-text-channel Because I can!
```
</p></details>

---

### channel setuserlimit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies voice channel user limit.*

**Aliases:**
`setul, setulimit, setlimit, setl, userlimit, ul, ulimig, userl`
**Guild only.**

**Requires permissions:**
`Manage channels`

**Overload 1:**
- [`channel`]: *Channel to modify*
- [`int`]: *User limit*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`int`]: *User limit*
- [`channel`]: *Channel to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!channel setuserlimit My Voice Channel 10
!channel setuserlimit 10 My Voice Channel Because I can!
```
</p></details>

---

### channel viewperms
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows permissions for member/role in a given channel.*

**Aliases:**
`perms, permsfor, testperms, listperms, permissions`
**Guild only.**

**Requires permissions:**
`Manage channels`
**Requires bot permissions:**
`Administrator`

**Overload 3:**
- (optional) [`member`]: *Member* (def: `None`)
- (optional) [`channel`]: *Channel to modify* (def: `None`)

**Overload 2:**
- [`channel`]: *Channel to modify*
- (optional) [`member`]: *Member* (def: `None`)

**Overload 1:**
- [`role`]: *Role*
- (optional) [`channel`]: *Channel to modify* (def: `None`)

**Overload 0:**
- [`channel`]: *Channel to modify*
- [`role`]: *Role*

**Examples:**

```xml
!channel viewperms #my-text-channel Member
!channel viewperms @Role #my-text-channel
```
</p></details>

---

## Group: commandrules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bot command rule management (allowing or forbiding certain commands). Group call shows active command rules in specified channel or globally if the channel is not specified.*

**Aliases:**
`cmdrules, crules, cr`
**Guild only.**

**Requires user permissions:**
`Administrator`

**Overload 1:**
- (optional) [`channel`]: *Channel in which to search for active rules* (def: `None`)

**Overload 0:**
- [`string...`]: *Command/Group full name*

**Examples:**

```xml
!commandrules
!commandrules sample command
!commandrules #my-text-channel
```
</p></details>

---

### commandrules allow
<details><summary markdown='span'>Expand for additional information</summary><p>

*Allows command execution only in specified channel(s), or globally if they are not specified.*

**Aliases:**
`only, register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`Administrator`

**Overload 1:**
- [`channel`]: *Channel(s) affected by this action*
- [`string...`]: *Command or group to allow*

**Overload 0:**
- [`string`]: *Command or group to allow*
- [`channel...`]: *Channel(s) affected by this action*

**Examples:**

```xml
!commandrules allow sample command
!commandrules allow #my-text-channel sample command
!commandrules allow sample command #my-text-channel #other-text-channel
```
</p></details>

---

### commandrules deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove all command rules.*

**Aliases:**
`reset, removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`Administrator`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!commandrules deleteall
```
</p></details>

---

### commandrules forbid
<details><summary markdown='span'>Expand for additional information</summary><p>

*Forbids command execution in specified channel(s), or globally if they are not specified.*

**Aliases:**
`f, deny, unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires user permissions:**
`Administrator`

**Overload 0:**
- [`string`]: *Command or group to forbid*
- [`channel...`]: *Channel(s) affected by this action*

**Overload 0:**
- [`channel`]: *Channel(s) affected by this action*
- [`string`]: *Command or group to forbid*

**Examples:**

```xml
!commandrules forbid sample command
!commandrules forbid #my-text-channel sample command
!commandrules forbid sample command #my-text-channel #other-text-channel
```
</p></details>

---

### commandrules list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists active command rules in specified channel or globally if the channel is not specified.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`Administrator`

**Overload 1:**
- [`string...`]: *Command/Group full name*

**Overload 0:**
- (optional) [`channel`]: *Channel in which to search for active rules* (def: `None`)

**Examples:**

```xml
!commandrules list
!commandrules list sample command
!commandrules list #my-text-channel
```
</p></details>

---

## Group: config
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manage bot configuration for this guild. Group call lists current guild configuration.*

**Aliases:**
`configuration, configure, settings, cfg`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config
```
</p></details>

---

### config actionhistory
<details><summary markdown='span'>Expand for additional information</summary><p>

*Views or toggles moderation action recording.*

**Aliases:**
`history, ah`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable action history?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config actionhistory
!config actionhistory Yes/No
```
</p></details>

---

## Group: config antiflood
<details><summary markdown='span'>Expand for additional information</summary><p>

*Punishes guild flooders/raiders. Executes punishment action when more than specified amount of users (sensitivity) enter the guild within a given time window (cooldown).*

**Aliases:**
`antiraid, ar, af`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 5:**
- [`boolean`]: *Enable?*
- [`short`]: *Sensitivity*
- (optional) [`PunishmentAction`]: *Punishment action* (def: `Kick`)
- (optional) [`time span`]: *Cooldown timespan* (def: `None`)

**Overload 4:**
- [`boolean`]: *Enable?*
- [`PunishmentAction`]: *Punishment action*
- (optional) [`short`]: *Sensitivity* (def: `5`)
- (optional) [`time span`]: *Cooldown timespan* (def: `None`)

**Overload 3:**
- [`boolean`]: *Enable?*
- [`PunishmentAction`]: *Punishment action*
- (optional) [`time span`]: *Cooldown timespan* (def: `None`)
- (optional) [`short`]: *Sensitivity* (def: `5`)

**Overload 2:**
- [`boolean`]: *Enable?*
- [`time span`]: *Cooldown timespan*
- (optional) [`PunishmentAction`]: *Punishment action* (def: `Kick`)
- (optional) [`short`]: *Sensitivity* (def: `5`)

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config antiflood
!config antiflood Yes/No
!config antiflood Yes/No 5 Kick 10s
!config antiflood Yes/No Kick 5 10s
!config antiflood Yes/No 10s 5 Kick
```
</p></details>

---

### config antiflood action
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets the antiflood action.*

**Aliases:**
`setaction, setact, act, a`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`Nullable`1`]: *Punishment action* (def: `None`)

**Examples:**

```xml
!config antiflood action
!config antiflood action Kick
```
</p></details>

---

### config antiflood cooldown
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets the antiflood cooldown.*

**Aliases:**
`setcooldown, setcool, cd, c`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`time span`]: *Cooldown timespan* (def: `None`)

**Examples:**

```xml
!config antiflood cooldown
!config antiflood cooldown 10s
```
</p></details>

---

### config antiflood reset
<details><summary markdown='span'>Expand for additional information</summary><p>

*Reverts antiflood configuration to default values.*

**Aliases:**
`default, def, s, rr`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config antiflood reset
```
</p></details>

---

### config antiflood sensitivity
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets the antiflood sensitivity.*

**Aliases:**
`setsensitivity, setsens, sens, s`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`short`]: *Sensitivity* (def: `None`)

**Examples:**

```xml
!config antiflood sensitivity
!config antiflood sensitivity 5
```
</p></details>

---

## Group: config antimention
<details><summary markdown='span'>Expand for additional information</summary><p>

*Punishes users that send more than specified amount mentions (sensitivity) in a message.*

**Aliases:**
`am`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 3:**
- [`boolean`]: *Enable?*
- [`short`]: *Sensitivity*
- (optional) [`PunishmentAction`]: *Punishment action* (def: `TemporaryMute`)

**Overload 2:**
- [`boolean`]: *Enable?*
- [`PunishmentAction`]: *Punishment action*
- (optional) [`short`]: *Sensitivity* (def: `5`)

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config antimention
!config antimention Yes/No
!config antimention Yes/No 5 Kick
!config antimention Yes/No Kick 5
```
</p></details>

---

### config antimention action
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets the anti-mention action.*

**Aliases:**
`setaction, setact, act, a`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`Nullable`1`]: *Punishment action* (def: `None`)

**Examples:**

```xml
!config antimention action
!config antimention action Kick
```
</p></details>

---

### config antimention exempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disable anti-mention for specified users, channels or roles.*

**Aliases:**
`ex, exc`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 2:**
- [`member...`]: *User(s) to exempt*

**Overload 1:**
- [`role...`]: *Role(s) to exempt*

**Overload 0:**
- [`channel...`]: *Channel(s) to exempt*

**Examples:**

```xml
!config antimention exempt @User
!config antimention exempt @Role
!config antimention exempt #my-text-channel
```
</p></details>

---

### config antimention reset
<details><summary markdown='span'>Expand for additional information</summary><p>

*Reverts anti-mention configuration to default values.*

**Aliases:**
`default, def, s, rr`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config antimention reset
```
</p></details>

---

### config antimention sensitivity
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets the anti-mention sensitivity.*

**Aliases:**
`setsensitivity, setsens, sens, s`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`short`]: *Sensitivity* (def: `None`)

**Examples:**

```xml
!config antimention sensitivity
!config antimention sensitivity 5
```
</p></details>

---

### config antimention unexempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes anti-mention exemptions.*

**Aliases:**
`unex, uex`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 2:**
- [`member...`]: *User(s) to unexempt*

**Overload 1:**
- [`role...`]: *Role(s) to unexempt*

**Overload 0:**
- [`channel...`]: *Channel(s) to unexempt*

**Examples:**

```xml
!config antimention unexempt @User
!config antimention unexempt @Role
!config antimention unexempt #my-text-channel
```
</p></details>

---

## Group: config antispam
<details><summary markdown='span'>Expand for additional information</summary><p>

*Punishes users that send the same message atleast specified amount of times (sensitivity).*

**Aliases:**
`as`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 3:**
- [`boolean`]: *Enable?*
- [`short`]: *Sensitivity*
- (optional) [`PunishmentAction`]: *Punishment action* (def: `TemporaryMute`)

**Overload 2:**
- [`boolean`]: *Enable?*
- [`PunishmentAction`]: *Punishment action*
- (optional) [`short`]: *Sensitivity* (def: `5`)

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config antispam
!config antispam Yes/No
!config antispam Yes/No 5 Kick
!config antispam Yes/No Kick 5
```
</p></details>

---

### config antispam action
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets the antispam action.*

**Aliases:**
`setaction, setact, act, a`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`Nullable`1`]: *Punishment action* (def: `None`)

**Examples:**

```xml
!config antispam action
!config antispam action Kick
```
</p></details>

---

### config antispam exempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disable antispam for specified users, channels or roles.*

**Aliases:**
`ex, exc`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 2:**
- [`member...`]: *User(s) to exempt*

**Overload 1:**
- [`role...`]: *Role(s) to exempt*

**Overload 0:**
- [`channel...`]: *Channel(s) to exempt*

**Examples:**

```xml
!config antispam exempt @User
!config antispam exempt @Role
!config antispam exempt #my-text-channel
```
</p></details>

---

### config antispam reset
<details><summary markdown='span'>Expand for additional information</summary><p>

*Reverts antispam configuration to default values.*

**Aliases:**
`default, def, s, rr`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config antispam reset
```
</p></details>

---

### config antispam sensitivity
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets the antispam sensitivity.*

**Aliases:**
`setsensitivity, setsens, sens, s`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`short`]: *Sensitivity* (def: `None`)

**Examples:**

```xml
!config antispam sensitivity
!config antispam sensitivity 5
```
</p></details>

---

### config antispam unexempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes antispam exemptions.*

**Aliases:**
`unex, uex`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 2:**
- [`member...`]: *User(s) to unexempt*

**Overload 1:**
- [`role...`]: *Role(s) to unexempt*

**Overload 0:**
- [`channel...`]: *Channel(s) to unexempt*

**Examples:**

```xml
!config antispam unexempt @User
!config antispam unexempt @Role
!config antispam unexempt #my-text-channel
```
</p></details>

---

## Group: config backup
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manages real-time backup of messages sent in guild channels.*

**Aliases:**
`bk, bak`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config backup
!config backup Yes/No
```
</p></details>

---

### config backup download
<details><summary markdown='span'>Expand for additional information</summary><p>

*Download compressed backup.*

**Aliases:**
`dl, get, zip`
**Guild only.**

**Requires user permissions:**
`Administrator, Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config backup download
```
</p></details>

---

### config backup exempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disable real-time backup for specified channels.*

**Aliases:**
`ex, exc`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`channel...`]: *Channel(s) to exempt*

**Examples:**

```xml
!config backup exempt #my-text-channel
```
</p></details>

---

### config backup unexempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes real-time backup exemptions.*

**Aliases:**
`unex, uex`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`channel...`]: *Channel(s) to unexempt*

**Examples:**

```xml
!config backup unexempt #my-text-channel
```
</p></details>

---

### config currency
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets guild currency.*

**Aliases:**
`setcurrency, curr, $, $$, $$$`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`string`]: *New currency*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config currency
!config currency Some string here
!config currency :emoji:
```
</p></details>

---

## Group: config instantleave
<details><summary markdown='span'>Expand for additional information</summary><p>

*Punishes users that join the guild and instantly leave it. This is a method used to spam ads if users join with ads in name and Discord/bots welcome them.*

**Aliases:**
`il`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 2:**
- [`boolean`]: *Enable?*
- [`short`]: *Sensitivity*

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config instantleave
!config instantleave Yes/No
!config instantleave Yes/No 5
```
</p></details>

---

### config instantleave cooldown
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets the instantleave cooldown.*

**Aliases:**
`setcooldown, setcool, cd, c`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`short`]: *Sensitivity* (def: `None`)

**Examples:**

```xml
!config instantleave cooldown
!config instantleave cooldown 10s
```
</p></details>

---

### config instantleave reset
<details><summary markdown='span'>Expand for additional information</summary><p>

*Reverts instantleave configuration to default values.*

**Aliases:**
`default, def, s, rr`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config instantleave reset
```
</p></details>

---

## Group: config leave
<details><summary markdown='span'>Expand for additional information</summary><p>

*Configures member leave messages.*

**Aliases:**
`quit, lv, lm, l`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable leave messages?*
- (optional) [`channel`]: *Channel where to send leave messages* (def: `None`)

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config leave
!config leave Yes/No #my-text-channel
```
</p></details>

---

### config leave channel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Enables leave messages in specified channel.*

**Aliases:**
`chn, ch, c`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`channel`]: *Channel where to send leave messages* (def: `None`)

**Examples:**

```xml
!config leave channel #my-text-channel
```
</p></details>

---

### config leave message
<details><summary markdown='span'>Expand for additional information</summary><p>

*Customizes leave message.*

**Aliases:**
`msg, m`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`string...`]: *Leave message*

**Examples:**

```xml
!config leave message #my-text-channel
```
</p></details>

---

### config levelup
<details><summary markdown='span'>Expand for additional information</summary><p>

*Configures member levelup notifications.*

**Aliases:**
`lvlup, lvl`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable silent level ups?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config levelup
!config levelup Yes/No
```
</p></details>

---

## Group: config linkfilter
<details><summary markdown='span'>Expand for additional information</summary><p>

*Linkfilter configuration. Group call prints current configuration, or enables/disables linkfilter.*

**Aliases:**
`lf`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config linkfilter
```
</p></details>

---

### config linkfilter booters
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets DDoS/Booter website link filtering.*

**Aliases:**
`ddos, boot, dos`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config linkfilter booters Yes/No
```
</p></details>

---

### config linkfilter invites
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets Discord invite link filtering.*

**Aliases:**
`invite, inv, i`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config linkfilter invites Yes/No
```
</p></details>

---

### config linkfilter iploggers
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets IP logging website link filtering.*

**Aliases:**
`ip, loggers`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config linkfilter iploggers Yes/No
```
</p></details>

---

### config linkfilter shocksites
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets Shock/Gore website link filtering.*

**Aliases:**
`disturbingsites, shock, disturbing, gore`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config linkfilter shocksites Yes/No
```
</p></details>

---

### config linkfilter shorteners
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets URL shortener website link filtering.*

**Aliases:**
`urlshort, shortenurl, urlshorteners`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config linkfilter shorteners Yes/No
```
</p></details>

---

## Group: config localization
<details><summary markdown='span'>Expand for additional information</summary><p>

*Configures the bot locale (language and date formats) for this guild. Group call shows current guild locale.*

**Aliases:**
`locale, language, lang, region`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`string`]: *New locale*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config localization
!config localization en-GB
```
</p></details>

---

### config localization list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all available locales (language and date formats).*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config localization list
```
</p></details>

---

### config localization set
<details><summary markdown='span'>Expand for additional information</summary><p>

*Changes the bot locale (language and date formats) for this guild.*

**Aliases:**
`change`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`string`]: *New locale*

**Examples:**

```xml
!config localization set en-GB
```
</p></details>

---

## Group: config logging
<details><summary markdown='span'>Expand for additional information</summary><p>

*Configures event logging for this guild.*

**Aliases:**
`log, modlog`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 3:**
- [`boolean`]: *Enable?*
- [`channel`]: *New locale*

**Overload 2:**
- [`channel`]: *New locale*

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config logging
!config logging #my-text-channel
!config logging Yes/No #my-text-channel
```
</p></details>

---

### config logging exempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disable action logging for specified users, channels or roles.*

**Aliases:**
`ex, exc`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 2:**
- [`member...`]: *User(s) to exempt*

**Overload 1:**
- [`role...`]: *Role(s) to exempt*

**Overload 0:**
- [`channel...`]: *Channel(s) to exempt*

**Examples:**

```xml
!config logging exempt @User
!config logging exempt @Role
!config logging exempt #my-text-channel
```
</p></details>

---

### config logging unexempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes action logging exemptions.*

**Aliases:**
`unex, uex`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 2:**
- [`member...`]: *User(s) to unexempt*

**Overload 1:**
- [`role...`]: *Role(s) to unexempt*

**Overload 0:**
- [`channel...`]: *Channel(s) to unexempt*

**Examples:**

```xml
!config logging unexempt @User
!config logging unexempt @Role
!config logging unexempt #my-text-channel
```
</p></details>

---

### config muterole
<details><summary markdown='span'>Expand for additional information</summary><p>

*Views or changes the mute role for the guild.*

**Aliases:**
`mr, muterl, mrl`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`role`]: *New mute role* (def: `None`)

**Examples:**

```xml
!config muterole
!config muterole @Role
```
</p></details>

---

## Group: config ratelimit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Punishes users that send more than specified amount of messages (sensitivity) in 5s.*

**Aliases:**
`rl`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 3:**
- [`boolean`]: *Enable?*
- [`short`]: *Sensitivity*
- (optional) [`PunishmentAction`]: *Punishment action* (def: `TemporaryMute`)

**Overload 2:**
- [`boolean`]: *Enable?*
- [`PunishmentAction`]: *Punishment action*
- (optional) [`short`]: *Sensitivity* (def: `5`)

**Overload 1:**
- [`boolean`]: *Enable?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config ratelimit
!config ratelimit Yes/No
!config ratelimit Yes/No 5 Kick
!config ratelimit Yes/No Kick 5
```
</p></details>

---

### config ratelimit action
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets the ratelimit action.*

**Aliases:**
`setaction, setact, act, a`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`Nullable`1`]: *Punishment action* (def: `None`)

**Examples:**

```xml
!config ratelimit action
!config ratelimit action Kick
```
</p></details>

---

### config ratelimit exempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disable ratelimit watch for specified users, channels or roles.*

**Aliases:**
`ex, exc`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 2:**
- [`member...`]: *User(s) to exempt*

**Overload 1:**
- [`role...`]: *Role(s) to exempt*

**Overload 0:**
- [`channel...`]: *Channel(s) to exempt*

**Examples:**

```xml
!config ratelimit exempt @User
!config ratelimit exempt @Role
!config ratelimit exempt #my-text-channel
```
</p></details>

---

### config ratelimit reset
<details><summary markdown='span'>Expand for additional information</summary><p>

*Reverts ratelimit configuration to default values.*

**Aliases:**
`default, def, s, rr`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config ratelimit reset
```
</p></details>

---

### config ratelimit sensitivity
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets the ratelimit sensitivity.*

**Aliases:**
`setsensitivity, setsens, sens, s`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`short`]: *Sensitivity* (def: `None`)

**Examples:**

```xml
!config ratelimit sensitivity
!config ratelimit sensitivity 5
```
</p></details>

---

### config ratelimit unexempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes antispam exemptions.*

**Aliases:**
`unex, uex`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 2:**
- [`member...`]: *User(s) to unexempt*

**Overload 1:**
- [`role...`]: *Role(s) to unexempt*

**Overload 0:**
- [`channel...`]: *Channel(s) to unexempt*

**Examples:**

```xml
!config ratelimit unexempt @User
!config ratelimit unexempt @Role
!config ratelimit unexempt #my-text-channel
```
</p></details>

---

### config reset
<details><summary markdown='span'>Expand for additional information</summary><p>

*Resets guild config to default values.*

**Aliases:**
`default, def, s, rr`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config reset
```
</p></details>

---

### config setup
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts an interactive wizard for configuring the guild settings.*

**Aliases:**
`wizard`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`channel`]: *Channel where to execute the setup* (def: `None`)

**Examples:**

```xml
!config setup
!config setup #my-text-channel
```
</p></details>

---

### config silent
<details><summary markdown='span'>Expand for additional information</summary><p>

*Views or toggles silent reaction replies to command execution.*

**Aliases:**
`reactionresponse, silentresponse, s, rr`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable silent replies?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config silent
!config silent Yes/No
```
</p></details>

---

### config suggestions
<details><summary markdown='span'>Expand for additional information</summary><p>

*Views or toggles command suggestion setting when command name is not found.*

**Aliases:**
`suggestion, cmdsug, sugg, sug, help`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable command suggestions?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config suggestions
!config suggestions Yes/No
```
</p></details>

---

## Group: config timezone
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets current guild time zone.*

**Aliases:**
`tz`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**

*No arguments.*

**Overload 0:**
- [`string...`]: *IANA/Windows/Rails timezone ID*

**Examples:**

```xml
!config timezone
!config timezone CET
```
</p></details>

---

### config timezone current
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints current guild time zone.*

**Aliases:**
`curr, active`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config timezone current
```
</p></details>

---

### config timezone info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints information about given time zone.*

**Aliases:**
`i, information`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`string...`]: *IANA/Windows/Rails timezone ID* (def: `None`)

**Examples:**

```xml
!config timezone info
!config timezone info CET
```
</p></details>

---

### config timezone list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all available time zones.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config timezone list
```
</p></details>

---

### config timezone reset
<details><summary markdown='span'>Expand for additional information</summary><p>

*Resets guild time zone information to the default value.*

**Aliases:**
`default, def, rr`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config timezone reset
```
</p></details>

---

### config timezone set
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modifies current guild time zone.*

**Aliases:**
`s`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`string...`]: *IANA/Windows/Rails timezone ID*

**Examples:**

```xml
!config timezone set CET
```
</p></details>

---

### config verbose
<details><summary markdown='span'>Expand for additional information</summary><p>

*Views or toggles verbose replies to command execution.*

**Aliases:**
`fullresponse, verboseresponse, v, vr`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable verbose replies?*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config verbose
!config verbose Yes/No
```
</p></details>

---

## Group: config welcome
<details><summary markdown='span'>Expand for additional information</summary><p>

*Configures member welcome messages.*

**Aliases:**
`enter, join, wlc, wm, w`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`boolean`]: *Enable welcome messages?*
- (optional) [`channel`]: *Channel where to send welcome messages* (def: `None`)

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!config welcome
!config welcome Yes/No #my-text-channel
```
</p></details>

---

### config welcome channel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Enables welcome messages in specified channel.*

**Aliases:**
`chn, ch, c`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- (optional) [`channel`]: *Channel where to send welcome messages* (def: `None`)

**Examples:**

```xml
!config welcome channel #my-text-channel
```
</p></details>

---

### config welcome message
<details><summary markdown='span'>Expand for additional information</summary><p>

*Customizes welcome message.*

**Aliases:**
`msg, m`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`string...`]: *Welcome message*

**Examples:**

```xml
!config welcome message #my-text-channel
```
</p></details>

---

## Group: emoji
<details><summary markdown='span'>Expand for additional information</summary><p>

*Guild emoji administration. Group call lists all guild emoji or prints information about a given emoji.*

**Aliases:**
`emojis, e`
**Guild only.**

**Requires permissions:**
`Manage emoji`

**Overload 1:**

*No arguments.*

**Overload 0:**
- [`emoji`]: *Emoji to view*

**Examples:**

```xml
!emoji
!emoji :emoji:
!emoji emoji_name
```
</p></details>

---

### emoji add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add emoji specified via URL/attachment, or from another guild.*

**Aliases:**
`create, install, register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires permissions:**
`Manage emoji`

**Overload 3:**
- [`string`]: *Name for the emoji*
- (optional) [`URL`]: *Emoji URL* (def: `None`)

**Overload 2:**
- [`URL`]: *Emoji URL*
- [`string`]: *Name for the emoji*

**Overload 1:**
- [`string`]: *Name for the emoji*
- [`emoji`]: *Emoji from another guild*

**Overload 0:**
- [`emoji`]: *Emoji from another guild*
- (optional) [`string`]: *Name for the emoji* (def: `None`)

**Examples:**

```xml
!emoji add emoji_name
!emoji add emoji_name http://some-emoji-image/url
```
</p></details>

---

### emoji delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove guild emoji. Note: Bots can only delete emojis they created!*

**Aliases:**
`unregister, uninstall, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires permissions:**
`Manage emoji`

**Overload 0:**
- [`emoji`]: *Emoji to delete*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!emoji delete emoji_name
!emoji delete emoji_name Because I can!
```
</p></details>

---

### emoji info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints information for given guild emoji.*

**Aliases:**
`information, details, about, i`
**Guild only.**

**Requires permissions:**
`Manage emoji`

**Overload 0:**
- [`emoji`]: *Emoji to view*

**Examples:**

```xml
!emoji info
!emoji info :emoji:
!emoji info emoji_name
```
</p></details>

---

### emoji list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all emojis for this guild.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires permissions:**
`Manage emoji`

**Overload 0:**

*No arguments.*

</p></details>

---

### emoji modify
<details><summary markdown='span'>Expand for additional information</summary><p>

*Edit name of an existing guild emoji.*

**Aliases:**
`edit, mod, e, m, rename, mv, setname`
**Guild only.**

**Requires permissions:**
`Manage emoji`

**Overload 1:**
- [`emoji`]: *Emoji to modify*
- [`string`]: *Name*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`string`]: *Name*
- [`emoji`]: *Emoji to modify*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!emoji modify :emoji: new_name
```
</p></details>

---

## Group: filter
<details><summary markdown='span'>Expand for additional information</summary><p>

*Message filtering administration. Group call either lists all filters or adds a new filter for given regular expression(s).*

**Aliases:**
`f, filters, autodel`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**

*No arguments.*

**Overload 0:**
- [`string...`]: *Filter patterns (regular expressions, case insensitive)*

**Examples:**

```xml
!filter
!filter regex?pattern+
```
</p></details>

---

### filter add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new filter for given regular expression(s).*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`string...`]: *Filter patterns (regular expressions, case insensitive)*

**Examples:**

```xml
!filter add regex?pattern+
```
</p></details>

---

## Group: filter delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes filter(s) by ID, pattern or matching string.*

**Aliases:**
`remove, rm, del, d, -, -=, >, >>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`int...`]: *IDs of filters to remove*

**Overload 0:**
- [`string...`]: *Filters to remove*

**Examples:**

```xml
!filter delete 12345
!filter delete regex?pattern+
```
</p></details>

---

### filter deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all guild filters.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!filter deleteall
```
</p></details>

---

### filter list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all filters for this guild.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!filter list
```
</p></details>

---

## Group: forbiddennames
<details><summary markdown='span'>Expand for additional information</summary><p>

*Forbidden name administration. Group call shows all registered forbidden names for this guild or adds a new forbidden name pattern if it has been provided.*

**Aliases:**
`forbiddenname, forbiddennicknames, disallowednames, fnames, fname, fn`
**Guild only.**

**Requires permissions:**
`Manage nicknames`
**Requires user permissions:**
`Manage guild`

**Overload 1:**

*No arguments.*

**Overload 0:**
- [`string...`]: *Forbidden name patterns (regular expressions, case insensitive)*

**Examples:**

```xml
!forbiddennames
!forbiddennames regex?pattern+
```
</p></details>

---

### forbiddennames add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Registers new forbidden name patterns.*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires permissions:**
`Manage nicknames`
**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`string...`]: *Forbidden name patterns (regular expressions, case insensitive)*

**Examples:**

```xml
!forbiddennames add regex?pattern+
```
</p></details>

---

## Group: forbiddennames delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes forbidden name by ID, pattern or matching string.*

**Aliases:**
`remove, rm, del, d, -, -=, >, >>`
**Guild only.**

**Requires permissions:**
`Manage nicknames`
**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`int...`]: *IDs of forbidden names to remove*

**Overload 0:**
- [`string...`]: *Forbidden names to remove*

**Examples:**

```xml
!forbiddennames delete 12345
!forbiddennames delete regex?pattern+
```
</p></details>

---

### forbiddennames deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all guild forbidden names.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires permissions:**
`Manage nicknames`
**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!forbiddennames deleteall
```
</p></details>

---

### forbiddennames list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all forbidden names for this guild.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires permissions:**
`Manage nicknames`
**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!forbiddennames list
```
</p></details>

---

## Group: guild
<details><summary markdown='span'>Expand for additional information</summary><p>

*Guild control commands. Group call prints guild information.*

**Aliases:**
`server, gld, svr, g`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!guild
```
</p></details>

---

### guild bans
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows guild ban list.*

**Aliases:**
`banlist, viewbanlist, getbanlist, getbans, viewbans`
**Guild only.**

**Requires permissions:**
`View audit log`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!guild bans
```
</p></details>

---

### guild icon
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets guild icon.*

**Aliases:**
`seticon, si`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 1:**
- [`URL`]: *Icon URL*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!guild icon
!guild icon http://some-image-url.com/image.png
```
</p></details>

---

### guild info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows guild information.*

**Aliases:**
`i, information`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!guild info
```
</p></details>

---

### guild log
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows guild audit log.*

**Aliases:**
`auditlog, viewlog, getlog, getlogs, logs`
**Guild only.**

**Requires permissions:**
`View audit log`

**Overload 0:**
- (optional) [`int`]: *Amount of log entries to fetch* (def: `10`)
- (optional) [`member`]: *Filter by member* (def: `None`)

**Examples:**

```xml
!guild log
!guild log 10
```
</p></details>

---

### guild memberlist
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows all guild members.*

**Aliases:**
`listmembers, members`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!guild memberlist
```
</p></details>

---

### guild prune
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes guild members who weren't active in the given amount of days.*

**Aliases:**
`p, clean, purge`
**Guild only.**

**Requires permissions:**
`Kick members`
**Requires user permissions:**
`Administrator`

**Overload 6:**
- [`int`]: *Days of inactivity*
- [`string`]: *Reason for the action*
- [`role...`]: *Additional roles to prune*

**Overload 5:**
- [`string`]: *Reason for the action*
- [`int`]: *Days of inactivity*
- [`role...`]: *Additional roles to prune*

**Overload 4:**
- [`string`]: *Reason for the action*
- [`role...`]: *Additional roles to prune*

**Overload 3:**
- [`int`]: *Days of inactivity*
- [`role...`]: *Additional roles to prune*

**Overload 2:**
- [`int`]: *Days of inactivity*
- [`role`]: *Additional roles to prune*
- [`string...`]: *Reason for the action*

**Overload 1:**
- [`int`]: *Days of inactivity*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!guild prune
!guild prune 5 Because I can!
!guild prune 5 @Role
```
</p></details>

---

### guild rename
<details><summary markdown='span'>Expand for additional information</summary><p>

*Renames the current guild.*

**Aliases:**
`r, name, setname, mv`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 0:**
- [`string...`]: *New name*

**Examples:**

```xml
!guild rename SampleName
```
</p></details>

---

## Group: levelroles
<details><summary markdown='span'>Expand for additional information</summary><p>

*Level roles management. Level roles are granted to a member of the guild upon gaining a specified XP rank. Group call lists all level roles for the guild. Group call adds a role to the level roles list for the specific rank, effective immediately.*

**Aliases:**
`lr, levelrole, lvlroles, levelrl, lvlrole, lvlr, lvlrl, lrole`
**Guild only.**

**Requires user permissions:**
`Manage guild`
**Requires bot permissions:**
`Manage roles`

**Overload 2:**
- [`short`]: *Rank*
- [`role`]: *Role to grant*

**Overload 1:**
- [`role`]: *Role to grant*
- [`short`]: *Rank*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!levelroles
!levelroles @Role 5
!levelroles 5 @Role
```
</p></details>

---

### levelroles add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Registers given role as a level role for the specified rank.*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`Manage guild`
**Requires bot permissions:**
`Manage roles`

**Overload 1:**
- [`role`]: *Role to grant*
- [`short`]: *Rank*

**Overload 0:**
- [`short`]: *Rank*
- [`role`]: *Role to grant*

**Examples:**

```xml
!levelroles add @Role 5
!levelroles add 5 @Role
```
</p></details>

---

### levelroles delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes given level role(s) by rank or role.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires user permissions:**
`Manage guild`
**Requires bot permissions:**
`Manage roles`

**Overload 1:**
- [`role...`]: *Roles to remove*

**Overload 1:**
- [`short...`]: *Ranks*

**Examples:**

```xml
!levelroles delete @Role RoleName
!levelroles delete 5 10
```
</p></details>

---

### levelroles deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all level roles.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`Manage guild`
**Requires bot permissions:**
`Manage roles`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!levelroles deleteall
```
</p></details>

---

### levelroles list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all level roles.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`Manage guild`
**Requires bot permissions:**
`Manage roles`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!levelroles list
```
</p></details>

---

## Group: message
<details><summary markdown='span'>Expand for additional information</summary><p>

*Message control commands.*

**Aliases:**
`m, msg, msgs, messages`

</p></details>

---

### message attachments
<details><summary markdown='span'>Expand for additional information</summary><p>

*View all message attachments. If the message is not provided, scans the last sent message before command invocation.*

**Aliases:**
`a, files, la`
**Requires permissions:**
`Read message history`

**Overload 0:**
- (optional) [`message`]: *Discord message* (def: `None`)

**Examples:**

```xml
!message attachments
!message attachments 361119455792594954
```
</p></details>

---

## Group: message delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes messages from the current channel. Group call deletes given amount of most recent messages.*

**Aliases:**
`-, prune, del, d`
**Requires permissions:**
`Manage messages`
**Requires user permissions:**
`Administrator`

**Overload 0:**
- (optional) [`int`]: *Amount of messages to delete* (def: `1`)
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!message delete
!message delete 10
!message delete 10 Because I can!
```
</p></details>

---

### message delete after
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes given amount messages after a specified message.*

**Aliases:**
`aft, af`
**Requires permissions:**
`Manage messages`
**Requires user permissions:**
`Administrator`

**Overload 0:**
- [`message`]: *Discord message*
- (optional) [`int`]: *Amount of messages to delete* (def: `1`)
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!message delete after 361119455792594954 10
```
</p></details>

---

### message delete before
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes given amount messages before a specified message.*

**Aliases:**
`bef, bf`
**Requires permissions:**
`Manage messages`
**Requires user permissions:**
`Administrator`

**Overload 0:**
- [`message`]: *Discord message*
- (optional) [`int`]: *Amount of messages to delete* (def: `1`)
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!message delete before 361119455792594954 10
```
</p></details>

---

### message delete from
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes given amount of most recent messages sent by the given member.*

**Aliases:**
`f, frm`
**Requires permissions:**
`Manage messages`
**Requires user permissions:**
`Administrator`

**Overload 1:**
- [`member`]: *Member*
- (optional) [`int`]: *Amount of messages to delete* (def: `1`)
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`int`]: *Amount of messages to delete*
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!message delete from @User 10
!message delete from 10 @User
```
</p></details>

---

### message delete reactions
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes all message reactions.*

**Aliases:**
`react, re`
**Requires permissions:**
`Manage messages`
**Requires user permissions:**
`Administrator`

**Overload 0:**
- (optional) [`message`]: *Discord message* (def: `None`)
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!message delete reactions 361119455792594954
```
</p></details>

---

### message delete regex
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes given amount of most-recent messages that match a given regular expression.*

**Aliases:**
`r, rgx, regexp, reg`
**Requires permissions:**
`Manage messages`
**Requires user permissions:**
`Administrator`

**Overload 1:**
- [`string`]: *Pattern (regular expression)*
- (optional) [`int`]: *Amount of messages to delete* (def: `5`)
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`int`]: *Amount of messages to delete*
- [`string`]: *Pattern (regular expression)*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!message delete regex regex?pattern+
!message delete regex regex?pattern+ 10
```
</p></details>

---

### message flag
<details><summary markdown='span'>Expand for additional information</summary><p>

*Flags the message given by ID for deletion vote. If the message is not provided, flags the last sent message before command invocation.*

**Aliases:**
`f`
**Requires permissions:**
`Read message history`
**Requires bot permissions:**
`Manage messages`

**Overload 0:**
- (optional) [`message`]: *Discord message* (def: `None`)
- (optional) [`time span`]: *Voting timespan* (def: `None`)

**Examples:**

```xml
!message flag
!message flag 361119455792594954
```
</p></details>

---

### message listpinned
<details><summary markdown='span'>Expand for additional information</summary><p>

*List pinned messages in a given channel. If the channel is not provided, uses the current one.*

**Aliases:**
`lp, listpins, listpin, pinned`

**Overload 0:**
- (optional) [`channel`]: *Channel which pins to view* (def: `None`)

**Examples:**

```xml
!message listpinned
!message listpinned #my-text-channel
```
</p></details>

---

### message pin
<details><summary markdown='span'>Expand for additional information</summary><p>

*Pins the given message. If the message is not provided, uses the last message before command invocation.*

**Aliases:**
`p`
**Requires permissions:**
`Manage messages`

**Overload 0:**
- (optional) [`message`]: *Discord message* (def: `None`)

**Examples:**

```xml
!message pin
!message pin 361119455792594954
```
</p></details>

---

### message unpin
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unpins the message by index (starting from 1) or message ID. If the index is not given, unpins the most recent one.*

**Aliases:**
`up`
**Requires permissions:**
`Manage messages`

**Overload 1:**
- [`message`]: *Discord message*

**Overload 0:**
- (optional) [`int`]: *Index (starting from 1)* (def: `1`)

**Examples:**

```xml
!message unpin 361119455792594954
!message unpin 5
```
</p></details>

---

### message unpinall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unpins all pinned messages in given channel. If the channel is not provided, uses the current one.*

**Aliases:**
`upa`
**Requires permissions:**
`Manage messages`

**Overload 0:**
- (optional) [`channel`]: *Channel which pins to remove* (def: `None`)

**Examples:**

```xml
!message unpinall
!message unpinall #my-text-channel
```
</p></details>

---

## Group: reactionroles
<details><summary markdown='span'>Expand for additional information</summary><p>

*Reaction roles management. Reaction roles are granted to a member of the guild when the member reacts to a message with the special emoji. Group call lists all reaction roles for the guild. Group call adds a role to the reaction roles list triggered by given emoji, effective immediately.*

**Aliases:**
`rr, reactionrole, reactroles, reactionrl, reactrole, reactr, reactrl, rrole`
**Guild only.**

**Requires user permissions:**
`Manage guild`
**Requires bot permissions:**
`Manage roles`

**Overload 2:**
- [`emoji`]: *Emoji*
- [`role`]: *Role to grant*

**Overload 1:**
- [`role`]: *Role to grant*
- [`emoji`]: *Emoji*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!reactionroles
!reactionroles @Role :emoji:
!reactionroles :emoji: @Role
```
</p></details>

---

### reactionroles add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Registers given role as a reaction role for the specified emoji.*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`Manage guild`
**Requires bot permissions:**
`Manage roles`

**Overload 1:**
- [`role`]: *Role to grant*
- [`emoji`]: *Emoji*

**Overload 0:**
- [`emoji`]: *Emoji*
- [`role`]: *Role to grant*

**Examples:**

```xml
!reactionroles add @Role :emoji:
!reactionroles add :emoji: @Role
```
</p></details>

---

### reactionroles delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes given reaction role(s) by emoji or role.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires user permissions:**
`Manage guild`
**Requires bot permissions:**
`Manage roles`

**Overload 1:**
- [`role...`]: *Roles to remove*

**Overload 1:**
- [`emoji...`]: *Ranks*

**Examples:**

```xml
!reactionroles delete @Role RoleName
!reactionroles delete :emoji: emoji_name
```
</p></details>

---

### reactionroles deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all reaction roles.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`Manage guild`
**Requires bot permissions:**
`Manage roles`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!reactionroles deleteall
```
</p></details>

---

### reactionroles list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all reaction roles.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`Manage guild`
**Requires bot permissions:**
`Manage roles`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!reactionroles list
```
</p></details>

---

## Group: role
<details><summary markdown='span'>Expand for additional information</summary><p>

*Role control commands. Group call lists all the roles in this guild or prints information about a given role.*

**Aliases:**
`roles, rl`
**Guild only.**


**Overload 1:**
- [`role`]: *Role*

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!role
!role @Role
```
</p></details>

---

### role create
<details><summary markdown='span'>Expand for additional information</summary><p>

*Creates a new role.*

**Aliases:**
`new, add, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 1:**
- [`string`]: *New name*
- (optional) [`color`]: *Color (hex or RGB)* (def: `None`)
- (optional) [`boolean`]: *Hoisted (visible in online list)?* (def: `False`)
- (optional) [`boolean`]: *Mentionable?* (def: `False`)
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`color`]: *Color (hex or RGB)*
- [`string...`]: *New name*

**Examples:**

```xml
!role create RoleName
!role create RoleName #ff00ff
!role create RoleName #ff00ff Yes/No
```
</p></details>

---

### role delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes specified role.*

**Aliases:**
`remove, rm, del, d, -, -=, >, >>`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 0:**
- [`role`]: *Role*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!role delete @Role
```
</p></details>

---

### role info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints details for given role.*

**Aliases:**
`i`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 0:**
- [`role`]: *Role*

**Examples:**

```xml
!role info @Role
```
</p></details>

---

### role list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all guild roles.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!role list
!role list @Role
```
</p></details>

---

### role mention
<details><summary markdown='span'>Expand for additional information</summary><p>

*Mention the given role. This will bypass the mentionable status for the given role.*

**Aliases:**
`mentionall, @, ma`
**Guild only.**

**Requires user permissions:**
`Administrator`
**Requires bot permissions:**
`Manage roles`

**Overload 0:**
- [`role`]: *Role*

**Examples:**

```xml
!role mention @Role
```
</p></details>

---

### role setcolor
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sets the role color.*

**Aliases:**
`clr, c, sc, setc`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 1:**
- [`role`]: *Role*
- [`color`]: *Color (hex or RGB)*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`color`]: *Color (hex or RGB)*
- [`role`]: *Role*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!role setcolor @Role #ff00ff
!role setcolor #ff00ff @Role
```
</p></details>

---

### role setmentionable
<details><summary markdown='span'>Expand for additional information</summary><p>

*Allows or forbids the role to be mentionable.*

**Aliases:**
`mentionable, m, setm`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 1:**
- [`role`]: *Role*
- (optional) [`boolean`]: *Mentionable?* (def: `True`)
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`boolean`]: *Mentionable?*
- [`role`]: *Role*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!role setmentionable @Role
!role setmentionable SampleName @Role
```
</p></details>

---

### role setname
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sets the role name.*

**Aliases:**
`name, rename, n, mv`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 1:**
- [`string`]: *New name*
- [`role`]: *Role*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`role`]: *Role*
- [`string...`]: *New name*

**Examples:**

```xml
!role setname @Role SampleName
!role setname SampleName @Role
```
</p></details>

---

### role setvisibility
<details><summary markdown='span'>Expand for additional information</summary><p>

*Allows or forbids the role to be hoisted (grouped in online list).*

**Aliases:**
`setvisible, separate, h, seth, hoist, sethoist`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 1:**
- [`role`]: *Role*
- (optional) [`boolean`]: *Hoisted (visible in online list)?* (def: `True`)
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`boolean`]: *Hoisted (visible in online list)?*
- [`role`]: *Role*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!role setvisibility @Role
!role setvisibility SampleName @Role
```
</p></details>

---

## Group: selfassignableroles
<details><summary markdown='span'>Expand for additional information</summary><p>

*Self-assignable roles commands. Self-assignable roles can be granted to members by themselves using the `give` command. Group call lists all self-assignable roles for the guild. Group call with an arbitrary amount of roles will add those roles to the self-assignable roles list for the guild, effective immediately.*

**Aliases:**
`sar, selfassignablerole, selfroles, selfrole, sr, srl, srole`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**

*No arguments.*

**Overload 0:**
- [`role...`]: *Roles to add*

**Examples:**

```xml
!selfassignableroles
!selfassignableroles @Role RoleName
```
</p></details>

---

### selfassignableroles add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Registers given role(s) as self-assignable.*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`role...`]: *Roles to add*

**Examples:**

```xml
!selfassignableroles add @Role RoleName
```
</p></details>

---

### selfassignableroles delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes given roles from self-assignable role list.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`role...`]: *Roles to remove*

**Examples:**

```xml
!selfassignableroles delete @Role RoleName
```
</p></details>

---

### selfassignableroles deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all self-assignable roles.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!selfassignableroles deleteall
```
</p></details>

---

### selfassignableroles list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all self-assignable roles.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!selfassignableroles list
```
</p></details>

---

## Group: user
<details><summary markdown='span'>Expand for additional information</summary><p>

*User administration commands. Group call prints information about given user or guild member.*

**Aliases:**
`users, u, usr, member, mem`

**Overload 1:**
- (optional) [`member`]: *Member* (def: `None`)

**Overload 0:**
- (optional) [`user`]: *User* (def: `None`)

**Examples:**

```xml
!user @User
```
</p></details>

---

### user avatar
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows user avatar in full size.*

**Aliases:**
`a, pic, profilepic`

**Overload 0:**
- [`user`]: *User*

**Examples:**

```xml
!user avatar @User
```
</p></details>

---

### user ban
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bans member from the guild.*

**Aliases:**
`b`
**Guild only.**

**Requires permissions:**
`Ban members`

**Overload 3:**
- [`user`]: *User*
- [`int`]: *Delete messages in past number of days*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 2:**
- [`member`]: *Member*
- [`int`]: *Delete messages in past number of days*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`user`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user ban @User
!user ban @User Because I can!
!user ban @User 5 Because I can!
```
</p></details>

---

### user deafen
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deafens a member.*

**Aliases:**
`deaf, d, df`
**Guild only.**

**Requires permissions:**
`Deafen voice chat members`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user deafen @User
```
</p></details>

---

### user grantrole
<details><summary markdown='span'>Expand for additional information</summary><p>

*Grants specified role(s) to specified user.*

**Aliases:**
`+role, +r, <r, <<r, ar, addr, +roles, addroles, giverole, giveroles, addrole, grantroles, gr`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 1:**
- [`member`]: *Member*
- [`role...`]: *Roles to add*

**Overload 0:**
- [`role`]: *Role*
- [`member`]: *Member*

**Examples:**

```xml
!user grantrole @User @Role
!user grantrole @Role @User
```
</p></details>

---

### user info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints user/member information.*

**Aliases:**
`i, information`

**Overload 1:**
- (optional) [`member`]: *Member* (def: `None`)

**Overload 0:**
- (optional) [`user`]: *User* (def: `None`)

**Examples:**

```xml
!user info @User
```
</p></details>

---

### user kick
<details><summary markdown='span'>Expand for additional information</summary><p>

*Kicks member from the guild.*

**Aliases:**
`k`
**Guild only.**

**Requires permissions:**
`Kick members`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user kick @User
```
</p></details>

---

### user kickvoice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Kicks member from the voice channels.*

**Aliases:**
`kv`
**Guild only.**

**Requires permissions:**
`Mute voice chat members`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user kickvoice @User
```
</p></details>

---

### user mute
<details><summary markdown='span'>Expand for additional information</summary><p>

*Mutes member by assigning a mute role.*

**Aliases:**
`m`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user mute @User
```
</p></details>

---

### user mutevoice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Mutes member in the voice channels.*

**Aliases:**
`mv, voicemute, vmute, mutev, vm`
**Guild only.**

**Requires permissions:**
`Mute voice chat members`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user mutevoice @User
```
</p></details>

---

### user revokeallroles
<details><summary markdown='span'>Expand for additional information</summary><p>

*Revokes all roles from specified user.*

**Aliases:**
`--roles, --r, >>>r, rar, removeallr, remallr, removeallroles, takeallroles, revallroles, tar`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user revokeallroles @User
```
</p></details>

---

### user revokerole
<details><summary markdown='span'>Expand for additional information</summary><p>

*Revokes specified role(s) from specified user.*

**Aliases:**
`-role, -r, >r, >>r, rr, remover, remr, -roles, removeroles, removerole, revokeroles, takeroles, revrole, revroles, tr`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 1:**
- [`member`]: *Member*
- [`role...`]: *Roles to remove*

**Overload 0:**
- [`role`]: *Role*
- [`member`]: *Member*

**Examples:**

```xml
!user revokerole @User @Role
!user revokerole @Role @User
```
</p></details>

---

### user setname
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sets a nickname for the specified member in the current guild.*

**Aliases:**
`nick, newname, name, rename, nickname`
**Guild only.**

**Requires permissions:**
`Manage nicknames`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *New name* (def: `None`)

**Examples:**

```xml
!user setname @User SampleName
```
</p></details>

---

### user softban
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bans a member and then immediately unbans him.*

**Aliases:**
`sb, sban`
**Guild only.**

**Requires permissions:**
`Ban members`

**Overload 1:**
- [`member`]: *Member*
- [`int`]: *Delete messages in past number of days*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user softban @User
!user softban @User Because I can!
```
</p></details>

---

### user tempban
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bans a member and unbans him after given timespan.*

**Aliases:**
`tb, tban, tmpban, tmpb`
**Guild only.**

**Requires permissions:**
`Ban members`

**Overload 3:**
- [`time span`]: *Time span*
- [`user`]: *User*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 2:**
- [`member`]: *User*
- [`time span`]: *Time span*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`time span`]: *Time span*
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`user`]: *User*
- [`time span`]: *Time span*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user tempban @User 1d
!user tempban 1d @User
```
</p></details>

---

### user tempmute
<details><summary markdown='span'>Expand for additional information</summary><p>

*Mutes a member and unmutes him after given timespan.*

**Aliases:**
`tm, tmute, tmpmute, tmpm`
**Guild only.**

**Requires permissions:**
`Manage roles`

**Overload 1:**
- [`time span`]: *Time span*
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`member`]: *User*
- [`time span`]: *Time span*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user tempmute @User 1d
!user tempmute 1d @User
```
</p></details>

---

### user unban
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unbans a user.*

**Aliases:**
`ub, removeban, revokeban, rb`
**Guild only.**

**Requires permissions:**
`Ban members`

**Overload 1:**
- [`user`]: *User*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user unban @User
```
</p></details>

---

### user undeafen
<details><summary markdown='span'>Expand for additional information</summary><p>

*Undeafens a member.*

**Aliases:**
`undeaf, ud, udf`
**Guild only.**

**Requires permissions:**
`Deafen voice chat members`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user undeafen @User
```
</p></details>

---

### user unmute
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unmutes a member by revoking mute role.*

**Aliases:**
`um`
**Guild only.**

**Requires permissions:**
`Mute voice chat members`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user unmute @User
```
</p></details>

---

### user unmutevoice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unmutes a member in voice channels.*

**Aliases:**
`umv, voiceunmute, vunmute, unmutev, vum`
**Guild only.**

**Requires permissions:**
`Mute voice chat members`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Examples:**

```xml
!user unmutevoice @User
```
</p></details>

---

### user warn
<details><summary markdown='span'>Expand for additional information</summary><p>

*Warns a member by a direct message with a given warning text.*

**Aliases:**
`w`
**Requires permissions:**
`Administrator`

**Overload 0:**
- [`member`]: *Member*
- (optional) [`string...`]: *Warning message* (def: `None`)

**Examples:**

```xml
!user warn @User
!user warn @User This is a warning!
```
</p></details>

---

## Group: webhook
<details><summary markdown='span'>Expand for additional information</summary><p>

*Webhook management commands. Group call lists webhooks for a given channel.*

**Aliases:**
`wh, webhooks, whook`
**Guild only.**

**Requires permissions:**
`Manage webhooks`

**Overload 0:**
- (optional) [`channel`]: *Channel whose webhooks to show* (def: `None`)

**Examples:**

```xml
!webhook
!webhook #my-text-channel
```
</p></details>

---

### webhook add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Creates a webhook.*

**Aliases:**
`create, c, register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires permissions:**
`Manage webhooks`

**Overload 6:**
- [`URL`]: *Avatar URL*
- [`channel`]: *Channel where to add the webhook*
- [`string`]: *Webhook name*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 5:**
- [`URL`]: *Avatar URL*
- [`string`]: *Webhook name*
- [`channel`]: *Channel where to add the webhook*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 4:**
- [`channel`]: *Channel where to add the webhook*
- [`URL`]: *Avatar URL*
- [`string`]: *Webhook name*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 3:**
- [`channel`]: *Channel where to add the webhook*
- [`string`]: *Webhook name*
- [`URL`]: *Avatar URL*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 2:**
- [`string`]: *Webhook name*
- [`URL`]: *Avatar URL*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 1:**
- [`URL`]: *Avatar URL*
- [`string`]: *Webhook name*
- (optional) [`string...`]: *Reason for the action* (def: `None`)

**Overload 0:**
- [`string...`]: *Webhook name*

**Examples:**

```xml
!webhook add
!webhook add http://some-image-url.com/image.png #my-text-channel SampleName
```
</p></details>

---

### webhook delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes a webhook by it's name or ID.*

**Aliases:**
`remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires permissions:**
`Manage webhooks`

**Overload 3:**
- [`channel`]: *Channel whose webhooks to delete*
- [`string...`]: *Webhook name*

**Overload 2:**
- [`channel`]: *Channel whose webhooks to delete*
- [`unsigned long`]: *ID*

**Overload 1:**
- [`string`]: *Webhook name*
- (optional) [`channel`]: *Channel whose webhooks to delete* (def: `None`)

**Overload 0:**
- [`unsigned long`]: *ID*
- (optional) [`channel`]: *Channel whose webhooks to delete* (def: `None`)

**Examples:**

```xml
!webhook delete 361119455792594954
!webhook delete MyWebhookName
!webhook delete #my-text-channel MyWebhookName
!webhook delete #my-text-channel 361119455792594954
```
</p></details>

---

### webhook deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes all webhooks in a given channel list.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires permissions:**
`Manage webhooks`

**Overload 1:**
- (optional) [`channel`]: *Channel whose webhooks to delete* (def: `None`)

**Overload 0:**
- [`channel...`]: *Channel whose webhooks to delete*

**Examples:**

```xml
!webhook deleteall
!webhook deleteall #my-text-channel
```
</p></details>

---

### webhook list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows webhooks for a given channel.*

**Aliases:**
`l, ls, show, s, print`
**Guild only.**

**Requires permissions:**
`Manage webhooks`

**Overload 0:**
- (optional) [`channel`]: *Channel whose webhooks to show* (def: `None`)

**Examples:**

```xml
!webhook list
!webhook list #my-text-channel
```
</p></details>

---

### webhook listall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows webhooks for the entire guild.*

**Aliases:**
`la, lsa, showall, printall`
**Guild only.**

**Requires permissions:**
`Manage webhooks`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!webhook listall
```
</p></details>

---

