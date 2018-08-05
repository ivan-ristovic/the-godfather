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

```
!birthday add @Someone
!birthday add @Someone #channel_to_send_message_to
!birthday add @Someone 15.2.1990
!birthday add @Someone #channel_to_send_message_to 15.2.1990
!birthday add @Someone 15.2.1990 #channel_to_send_message_to
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

```
!birthday delete @Someone
```
</p></details>

---

### birthdays list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all registered birthdays.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ls`

**Examples:**

```
!birthday list
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

```
!insult @Someone
```
</p></details>

---

### insult add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add insult to list (use %user% instead of user mention).*

**Aliases:**
`new, a, +, +=, <, <<`

**Arguments:**

`[string...]` : *Insult (must contain ``%user%``).*

**Examples:**

```
!insult add %user% is lowering the IQ of the entire street!
```
</p></details>

---

### insult delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove insult with a given index from list. (use command ``insults list`` to view insult indexes).*

**Aliases:**
`-, remove, del, rm, rem, d, >, >>, -=`

**Arguments:**

`[int]` : *Index of the insult to remove.*

**Examples:**

```
!insult delete 2
```
</p></details>

---

### insult deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete all insults.*

**Aliases:**
`clear, da, c, ca, cl, clearall, >>>`

**Examples:**

```
!insults clear
```
</p></details>

---

### insult list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Show all insults.*

**Aliases:**
`ls, l`

**Examples:**

```
!insult list
```
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

```
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

```
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

```
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

```
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

**Examples:**

```
!memes clear
```
</p></details>

---

### meme list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all registered memes for this guild.*

**Aliases:**
`ls, l`

**Examples:**

```
!meme list
```
</p></details>

---

### meme templates
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all available meme templates.*

**Aliases:**
`template, t`

**Examples:**

```
!meme templates
```
</p></details>

---

## Group: random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Random gibberish.*

**Aliases:**
`rnd, rand`

</p></details>

---

### random cat
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get a random cat image.*

**Examples:**

```
!random cat
```
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

```
!random choose option 1, option 2, option 3...
```
</p></details>

---

### random dog
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get a random dog image.*

**Examples:**

```
!random dog
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

```
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

```
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

`[int]` : *Rank.*

`[string...]` : *Rank name.*

**Examples:**

```
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

`[int]` : *Rank.*

**Examples:**

```
!rank delete 3
```
</p></details>

---

### rank list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print all customized ranks for this guild.*

**Aliases:**
`levels, ls, l, print`

**Examples:**

```
!rank list
```
</p></details>

---

### rank top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get rank leaderboard.*

**Examples:**

```
!rank top
```
</p></details>

---

## Group: shop
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shop for items using WM credits from your bank account. If invoked without subcommand, lists all available items for purchase.*

**Aliases:**
`store`

**Examples:**

```
!shop
```
</p></details>

---

### shop add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a new item to guild purchasable items list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, a, +=, <, <<, additem`

**Overload 1:**

`[long]` : *Item price.*

`[string...]` : *Item name.*

**Overload 0:**

`[string]` : *Item name.*

`[long]` : *Item price.*

**Examples:**

```
!shop add Barbie 500
!shop add "New Barbie" 500
!shop add 500 Newest Barbie
```
</p></details>

---

### shop buy
<details><summary markdown='span'>Expand for additional information</summary><p>

*Purchase an item from this guild's shop.*

**Aliases:**
`purchase, shutupandtakemymoney, b, p`

**Arguments:**

`[int]` : *Item ID.*

**Examples:**

```
!shop buy 3
```
</p></details>

---

### shop delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove purchasable item from this guild item list. You can remove an item by ID or by name.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, rm, del, -=, >, >>`

**Arguments:**

`[int...]` : *ID list of items to remove.*

**Examples:**

```
!shop delete Barbie
!shop delete 5
!shop delete 1 2 3 4 5
```
</p></details>

---

### shop list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all purchasable items for this guild.*

**Aliases:**
`ls`

**Examples:**

```
!shop list
```
</p></details>

---

### shop sell
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sell a purchased item for half the buy price.*

**Aliases:**
`return`

**Arguments:**

`[int]` : *Item ID.*

**Examples:**

```
!shop sell 3
```
</p></details>

---

