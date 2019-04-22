# Module: Miscellaneous

## Group: birthdays
<details><summary markdown='span'>Expand for additional information</summary><p>

*Birthday notifications commands. Group call either lists or adds birthday depending if argument is given.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`birthday, bday, bd, bdays`

**Overload 1:**

`[user]` : *Birthday boy/girl.*

(optional) `[channel]` : *Channel to send a greeting message to.* (def: `None`)

(optional) `[string]` : *Birth date.* (def: `None`)

**Overload 0:**

`[user]` : *Birthday boy/girl.*

(optional) `[string]` : *Birth date.* (def: `None`)

(optional) `[channel]` : *Channel to send a greeting message to.* (def: `None`)

**Examples:**

```xml
!birthdays 
!birthdays @Someone
!birthdays @Someone #channel
!birthdays @Someone 15.2.1990
!birthdays @Someone #channel 15.2.1990
!birthdays @Someone 15.2.1990 #channel
```
</p></details>

---

### birthdays add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Schedule a birthday notification. If the date is not specified, uses the current date as a birthday date. If the channel is not specified, uses the current channel.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`new, +, a, +=, <, <<`

**Overload 1:**

`[user]` : *Birthday boy/girl.*

(optional) `[channel]` : *Channel to send a greeting message to.* (def: `None`)

(optional) `[string]` : *Birth date.* (def: `None`)

**Overload 0:**

`[user]` : *Birthday boy/girl.*

(optional) `[string]` : *Birth date.* (def: `None`)

(optional) `[channel]` : *Channel to send a greeting message to.* (def: `None`)

**Examples:**

```xml
!birthdays add @Someone
!birthdays add @Someone #channel
!birthdays add @Someone 15.2.1990
!birthdays add @Someone #channel 15.2.1990
!birthdays add @Someone 15.2.1990 #channel
```
</p></details>

---

### birthdays delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove status from running queue.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, rm, del, -=, >, >>`

**Overload 1:**

`[user]` : *User whose birthday to remove.*

**Overload 0:**

`[channel]` : *Channel for which to remove birthdays.*

**Examples:**

```xml
!birthdays delete @Someone
!birthdays delete #channel
```
</p></details>

---

### birthdays list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List registered birthday notifications for this channel.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ls`

**Arguments:**

(optional) `[channel]` : *Channel for which to list.* (def: `None`)

</p></details>

---

### birthdays listall
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all registered birthdays.*

**Privileged users only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`lsa`

</p></details>

---

## Group: grant
<details><summary markdown='span'>Expand for additional information</summary><p>

*Requests to grant the sender a certain object (role for example).*

**Aliases:**
`give`

**Overload 1:**

`[role]` : *Role to grant.*

**Overload 0:**

`[string...]` : *Nickname to set.*

</p></details>

---

### grant nickname
<details><summary markdown='span'>Expand for additional information</summary><p>

*Grants you a given nickname.*

**Requires bot permissions:**
`Manage nicknames`

**Aliases:**
`nick, name, n`

**Arguments:**

`[string...]` : *Nickname to set.*

**Examples:**

```xml
!grant nickname My New Display Name
```
</p></details>

---

### grant role
<details><summary markdown='span'>Expand for additional information</summary><p>

*Grants you a role from this guild's self-assignable roles list.*

**Requires bot permissions:**
`Manage roles`

**Aliases:**
`rl, r`

**Arguments:**

`[role]` : *Role to grant.*

**Examples:**

```xml
!grant role @Announcements
```
</p></details>

---

## Group: insult
<details><summary markdown='span'>Expand for additional information</summary><p>

*Insults manipulation. Group call insults a given user.*

**Aliases:**
`burn, insults, ins, roast`

**Arguments:**

(optional) `[user]` : *User to insult.* (def: `None`)

**Examples:**

```xml
!insult 
!insult @Someone
```
</p></details>

---

### insult add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add insult to list (use %user% instead of user mention).*

**Privileged users only.**

**Aliases:**
`new, a, +, +=, <, <<`

**Arguments:**

`[string...]` : *Insult (must contain ``%user%``).*

**Examples:**

```xml
!insult add %user% is lowering the IQ of the entire street!
```
</p></details>

---

### insult delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove insult with a given ID from list. (use command ``insults list`` to view insult indexes).*

**Privileged users only.**

**Aliases:**
`-, remove, del, rm, rem, d, >, >>, -=`

**Arguments:**

`[int]` : *ID of the insult to remove.*

**Examples:**

```xml
!insult delete !insult delete 2
```
</p></details>

---

### insult deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete all insults.*

**Privileged users only.**

**Aliases:**
`clear, da, c, ca, cl, clearall, >>>`

</p></details>

---

### insult list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Show all insults.*

**Aliases:**
`ls, l`

</p></details>

---

## Group: meme
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate guild memes. Group call returns a meme from this guild's meme list given by name or a random one if name isn't provided.*

**Aliases:**
`memes, mm`

**Overload 0:**

`[string...]` : *Meme name.*

**Examples:**

```xml
!meme 
!meme SomeMemeNameWhichYouAdded
```
</p></details>

---

### meme add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a new meme to the list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new, a, +=, <, <<`

**Overload 1:**

`[string]` : *Short name (case insensitive).*

(optional) `[URL]` : *URL.* (def: `None`)

**Overload 0:**

`[URL]` : *URL.*

`[string]` : *Short name (case insensitive).*

**Examples:**

```xml
!meme add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg
```
</p></details>

---

### meme create
<details><summary markdown='span'>Expand for additional information</summary><p>

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

```xml
!meme create 1stworld "Top text" "Bottom text"
```
</p></details>

---

### meme delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes a meme from this guild's meme list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, del, remove, rm, d, rem, -=, >, >>`

**Arguments:**

`[string]` : *Short name (case insensitive).*

**Examples:**

```xml
!meme delete pepe
```
</p></details>

---

### meme deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes all guild memes.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`clear, da, ca, cl, clearall, >>>`

</p></details>

---

### meme list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all registered memes for this guild.*

**Aliases:**
`ls, l`

</p></details>

---

### meme templates
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all available meme templates.*

**Aliases:**
`template, t`

</p></details>

---

## Group: random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Random gibberish.*

**Aliases:**
`rnd, rand`

</p></details>

---

### random choose
<details><summary markdown='span'>Expand for additional information</summary><p>

*Choose one of the provided options separated by comma.*

**Aliases:**
`select`

**Arguments:**

`[string...]` : *Option list (comma separated).*

**Examples:**

```xml
!random choose option 1, option 2, option 3...
```
</p></details>

---

### random raffle
<details><summary markdown='span'>Expand for additional information</summary><p>

*Choose a user from the online members list optionally belonging to a given role.*

**Aliases:**
`chooseuser`

**Arguments:**

(optional) `[role]` : *Role.* (def: `None`)

**Examples:**

```xml
!random raffle 
!random raffle Admins
```
</p></details>

---

## Group: rank
<details><summary markdown='span'>Expand for additional information</summary><p>

*User ranking commands. Group command prints given user's rank.*

**Aliases:**
`ranks, ranking, level`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```xml
!rank 
!rank @Someone
```
</p></details>

---

### rank add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a custom name for given rank in this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, a, rename, rn, newname, <, <<, +=`

**Arguments:**

`[short]` : *Rank.*

`[string...]` : *Rank name.*

**Examples:**

```xml
!rank add 1 Private
```
</p></details>

---

### rank delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove a custom name for given rank in this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, rm, del, revert`

**Arguments:**

`[short]` : *Rank.*

**Examples:**

```xml
!rank delete 3
```
</p></details>

---

### rank list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print all customized ranks for this guild.*

**Aliases:**
`levels, ls, l, print`

</p></details>

---

### rank top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get rank leaderboard.*

</p></details>

---

## Group: revoke
<details><summary markdown='span'>Expand for additional information</summary><p>

*Requests to revoke a certain object from the sender (role for example).*

**Aliases:**
`take`

**Arguments:**

`[role]` : *Role to grant.*

</p></details>

---

### revoke role
<details><summary markdown='span'>Expand for additional information</summary><p>

*Revokes from your role list a role from this guild's self-assignable roles list.*

**Requires bot permissions:**
`Manage roles`

**Aliases:**
`rl, r`

**Arguments:**

`[role]` : *Role to revoke.*

**Examples:**

```xml
!revoke role @Announcements
```
</p></details>

---

