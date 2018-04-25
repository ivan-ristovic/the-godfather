# Module: Miscellaneous

## 8ball
<details><summary markdown='span'>Expand for additional information</summary><p>

*An almighty ball which knows the answer to any question you ask. Alright, it's random answer, so what?*

**Aliases:**
`8b`

**Arguments:**

`[string...]` : *A question for the almighty ball.*

**Examples:**

```
!8ball Am I gay?
```
</p></details>

---

## coinflip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Flip a coin.*

**Aliases:**
`coin, flip`

**Examples:**

```
!coinflip
```
</p></details>

---

## dice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Roll a dice.*

**Aliases:**
`die, roll`

**Examples:**

```
!dice
```
</p></details>

---

## giveme
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

## Group: insult
<details><summary markdown='span'>Expand for additional information</summary><p>

*Insults manipulation. If invoked without subcommands, insults a given user.*

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

**Owner-only.**

**Aliases:**
`+, new, a`

**Arguments:**

`[string...]` : *Insult (must contain ``%user%``).*

**Examples:**

```
!insult add You are so dumb, %user%!
```
</p></details>

---

### insult clear
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete all insults.*

**Owner-only.**

**Aliases:**
`da, c, ca, cl, clearall`

**Examples:**

```
!insults clear
```
</p></details>

---

### insult delete
<details><summary markdown='span'>Expand for additional information</summary><p>

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

## invite
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get an instant invite link for the current guild.*

**Requires permissions:**
`Create instant invites`

**Aliases:**
`getinvite`

**Examples:**

```
!invite
```
</p></details>

---

## items
<details><summary markdown='span'>Expand for additional information</summary><p>

*View user's purchased items (see ``bank`` and ``shop``).*

**Requires permissions:**
`Create instant invites`

**Aliases:**
`myitems, purchases`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!items
!items @Someone
```
</p></details>

---

## leave
<details><summary markdown='span'>Expand for additional information</summary><p>

*Makes Godfather leave the guild.*

**Requires user permissions:**
`Administrator`

**Examples:**

```
!leave
```
</p></details>

---

## leet
<details><summary markdown='span'>Expand for additional information</summary><p>

*Wr1t3s m3ss@g3 1n 1337sp34k.*

**Aliases:**
`l33t`

**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!leet Some sentence
```
</p></details>

---

## Group: meme
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate guild memes. When invoked without subcommands, returns a meme from this guild's meme list given by name, otherwise returns random one.*

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
`+, new, a`

**Arguments:**

`[string]` : *Short name (case insensitive).*

`[string]` : *URL.*

**Examples:**

```
!meme add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg
```
</p></details>

---

### meme clear
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes all guild memes.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, ca, cl, clearall`

**Examples:**

```
!memes clear
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
`-, del, remove, rm, d, rem`

**Arguments:**

`[string]` : *Short name (case insensitive).*

**Examples:**

```
!meme delete pepe
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

## news
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest world news.*

**Aliases:**
`worldnews`

**Examples:**

```
!news
```
</p></details>

---

## penis
<details><summary markdown='span'>Expand for additional information</summary><p>

*An accurate measurement.*

**Aliases:**
`size, length, manhood, dick`

**Arguments:**

(optional) `[user]` : *Who to measure.* (def: `None`)

**Examples:**

```
!penis @Someone
```
</p></details>

---

## peniscompare
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

## ping
<details><summary markdown='span'>Expand for additional information</summary><p>

*Ping the bot.*

**Examples:**

```
!ping
```
</p></details>

---

## prefix
<details><summary markdown='span'>Expand for additional information</summary><p>

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

`[string...]` : *Option list (separated by comma).*

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
</p></details>

---

## Group: rank
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

### rank list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print all available ranks.*

**Aliases:**
`levels`

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

## rate
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

## remind
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

## report
<details><summary markdown='span'>Expand for additional information</summary><p>

*Send a report message to owner about a bug (please don't abuse... please).*

**Arguments:**

`[string...]` : *Issue text.*

**Examples:**

```
!report Your bot sucks!
```
</p></details>

---

## say
<details><summary markdown='span'>Expand for additional information</summary><p>

*Echo echo echo.*

**Aliases:**
`repeat`

**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!say I am gay.
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
`+, a`

**Overload 1:**

`[int]` : *Item price.*

`[string...]` : *Item name.*

**Overload 0:**

`[string]` : *Item name.*

`[int]` : *Item price.*

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
`-, remove, rm, del`

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

## tts
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a tts message.*

**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!tts I am gay.
```
</p></details>

---

## zugify
<details><summary markdown='span'>Expand for additional information</summary><p>

*I don't even...*

**Aliases:**
`z`

**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!zugify Some random text
```
</p></details>

---

