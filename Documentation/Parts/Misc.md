# Module: Misc
*This module contains miscellaneous commands which do not fall into any of the other categories but do not deserve their own group since they are unique or not extensible.*


## Group: birthday
<details><summary markdown='span'>Expand for additional information</summary><p>

*Birthday notifications commands. Group call either lists or adds a new birthday notification(s).*

**Aliases:**
`birthdays, bday, bd, bdays`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 3:**

`[user]` : *Birthday boy/girl*

**Overload 2:**

(optional) `[channel]` : *Channel for birthday notifications* (def: `None`)

**Overload 1:**

`[user]` : *Birthday boy/girl*

`[channel]` : *Channel for birthday notifications*

(optional) `[string]` : *Birthday date* (def: `None`)

**Overload 0:**

`[user]` : *Birthday boy/girl*

`[string]` : *Birthday date*

(optional) `[channel]` : *Channel for birthday notifications* (def: `None`)

**Examples:**

```xml
!birthday @User
!birthday #my-text-channel
!birthday @User #my-text-channel 13.10.2000
```
</p></details>

---

### birthday add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new birthday notification for a given user in the given channel (defaults to current channel) at a given date (defaults to current date).*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**

`[user]` : *Birthday boy/girl*

(optional) `[channel]` : *Channel for birthday notifications* (def: `None`)

(optional) `[string]` : *Birthday date* (def: `None`)

**Overload 0:**

`[user]` : *Birthday boy/girl*

`[string]` : *Birthday date*

(optional) `[channel]` : *Channel for birthday notifications* (def: `None`)

**Examples:**

```xml
!birthday add @User
!birthday add @User #my-text-channel
!birthday add @User #my-text-channel 13.10.2000
```
</p></details>

---

### birthday delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes registered birthday notification(s) for a given user or in a given channels.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**

`[user]` : *Birthday boy/girl*

**Overload 0:**

`[channel]` : *Channel for birthday notifications*

**Examples:**

```xml
!birthday delete @User
!birthday delete #my-text-channel
```
</p></details>

---

### birthday deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all registered birthday notification(s).*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Examples:**

```xml
!birthday deleteall
```
</p></details>

---

### birthday list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists birthday notifications for a given user or a given channel.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**

`[user]` : *Birthday boy/girl*

**Overload 0:**

(optional) `[channel]` : *Channel for birthday notifications* (def: `None`)

**Examples:**

```xml
!birthday list @User
!birthday list #my-text-channel
```
</p></details>

---

### birthday listall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all birthday notifications for this guild.*

**Aliases:**
`printall, showall, lsa, la, pa`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Examples:**

```xml
!birthday listall
```
</p></details>

---

## Group: grant
<details><summary markdown='span'>Expand for additional information</summary><p>

*Requests to grant the sender a certain object (role for example).*

**Aliases:**
`give`
**Guild only.**

**Arguments:**

`[role...]` : *Roles to add*

**Examples:**

```xml
!grant @Role
!grant SampleName
```
</p></details>

---

### grant nickname
<details><summary markdown='span'>Expand for additional information</summary><p>

*Grants you a given nickname.*

**Aliases:**
`nick, name, n`
**Guild only.**

**Requires bot permissions:**
`Manage nicknames`

**Arguments:**

`[string...]` : *New name*

**Examples:**

```xml
!grant nickname SampleName
```
</p></details>

---

### grant role
<details><summary markdown='span'>Expand for additional information</summary><p>

*Grants you a role from this guild's self-assignable roles list.*

**Aliases:**
`roles, rl, r`
**Guild only.**

**Requires bot permissions:**
`Manage roles`

**Arguments:**

`[role...]` : *Roles to add*

**Examples:**

```xml
!grant role @Role
```
</p></details>

---

## Group: help
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows the help embed.*

**Aliases:**
`h, ?, ??, ???`
**Overload 1:**

`[ModuleType]` : *Command module*

**Overload 0:**

`[string...]` : *Command name*

**Examples:**

```xml
!help
!help Administration
!help sample command
```
</p></details>

---

## Group: insult
<details><summary markdown='span'>Expand for additional information</summary><p>

*Writes an insult targeting a user. Alternatively, you can provide text to use as insult target.*

**Aliases:**
`burn, ins, roast`
**Overload 1:**

(optional) `[user]` : *User* (def: `None`)

**Overload 0:**

`[string...]` : *Insult target*

**Examples:**

```xml
!insult @User
!insult Some string here
```
</p></details>

---

## Group: meme
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate guild memes. Group call retrieves a meme from this guild's meme list by it's name or a random one if the name isn't provided.*

**Aliases:**
`memes, mm`
**Guild only.**

**Overload 0:**

`[string...]` : *Meme name*

**Examples:**

```xml
!meme
!meme Some string here
```
</p></details>

---

### meme add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new guild meme with a given name and an image (provided via URL or attachment).*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**

`[string]` : *Meme name*

(optional) `[URL]` : *Meme URL* (def: `None`)

**Overload 0:**

`[URL]` : *Meme URL*

`[string...]` : *Meme name*

**Examples:**

```xml
!meme add Some string here
!meme add Some string here http://some-image-url.com/image.png
```
</p></details>

---

### meme create
<details><summary markdown='span'>Expand for additional information</summary><p>

*Creates a new meme from template and top/bottom text(s).*

**Aliases:**
`maker, c, make, m`
**Guild only.**

**Requires permissions:**
`Use embeds`

**Arguments:**

`[string]` : *Meme template*

`[string]` : *Top text*

`[string]` : *Bottom text*

**Examples:**

```xml
!meme create aag Some string here Some string here
```
</p></details>

---

### meme delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes a meme from guild meme list.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Arguments:**

`[string...]` : *Meme name*

**Examples:**

```xml
!meme delete Some string here
```
</p></details>

---

### meme deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all guild memes.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`Administrator`

**Examples:**

```xml
!meme deleteall
```
</p></details>

---

### meme list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all guild memes.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Examples:**

```xml
!meme list
```
</p></details>

---

### meme templates
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all available meme templates.*

**Aliases:**
`template, ts, t`
**Guild only.**

**Arguments:**

(optional) `[string...]` : *Meme template* (def: `None`)

**Examples:**

```xml
!meme templates
!meme templates aag
```
</p></details>

---

## Group: random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Randomization commands - choices, raffles etc.*

**Aliases:**
`rnd, rand`
</p></details>

---

### random choice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Chooses a random option from a comma separated option list.*

**Aliases:**
`select, choose`
**Arguments:**

`[string...]` : *Choice list (separated by comma)*

**Examples:**

```xml
!random choice option 1, option 2, option 3
```
</p></details>

---

### random raffle
<details><summary markdown='span'>Expand for additional information</summary><p>

*Choose a user from the online members list optionally belonging to a given role.*

**Aliases:**
`chooseuser`
**Arguments:**

(optional) `[role]` : *Role* (def: `None`)

**Examples:**

```xml
!random raffle
!random raffle @Role
```
</p></details>

---

## Group: rank
<details><summary markdown='span'>Expand for additional information</summary><p>

*User rank management. Group call prints user rank info.*

**Aliases:**
`ranks, ranking, level`
**Overload 1:**

(optional) `[member]` : *Member* (def: `None`)

**Overload 0:**

(optional) `[user]` : *User* (def: `None`)

**Examples:**

```xml
!rank
!rank @User
```
</p></details>

---

### rank add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a custom name for a given rank in this guild.*

**Aliases:**
`register, rename, mv, newname, reg, a, +, +=, <<, <, <-, <=`
**Requires user permissions:**
`Manage guild`

**Arguments:**

`[short]` : *Rank*

`[string...]` : *Rank name*

**Examples:**

```xml
!rank add 5 SampleName
```
</p></details>

---

### rank delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes a custom name for a given rank in this guild.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Requires user permissions:**
`Manage guild`

**Arguments:**

`[short]` : *Rank*

**Examples:**

```xml
!rank delete 5
```
</p></details>

---

### rank list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print all customized rank names for this guild.*

**Aliases:**
`print, show, view, ls, l, p`
**Examples:**

```xml
!rank list
```
</p></details>

---

### rank top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets highest ranked users leaderboard*

**Examples:**

```xml
!rank top
```
</p></details>

---

## Group: revoke
<details><summary markdown='span'>Expand for additional information</summary><p>

*Requests to revoke a certain object (role for example) from the sender.*

**Aliases:**
`take`
**Guild only.**

**Arguments:**

`[role...]` : *Roles to remove*

**Examples:**

```xml
!revoke @Role
```
</p></details>

---

### revoke role
<details><summary markdown='span'>Expand for additional information</summary><p>

*Revokes a role from this guild's self-assignable roles list.*

**Aliases:**
`rl, r`
**Guild only.**

**Requires bot permissions:**
`Manage roles`

**Arguments:**

`[role...]` : *Roles to remove*

**Examples:**

```xml
!revoke role @Role
```
</p></details>

---

