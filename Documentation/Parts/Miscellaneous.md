# Module: Miscellaneous

## 8ball
<details><summary markdown='span'>Expand for additional information</summary><code>

*An almighty ball which knows the answer to any question you ask. Alright, it's random answer, so what?*

**Aliases:**
`8b`

**Arguments:**

`[string...]` : *A question for the almighty ball.*

**Examples:**

```
!8ball Am I gay?
```
</code></details>

---

## coinflip
<details><summary markdown='span'>Expand for additional information</summary><code>

*Flip a coin.*

**Aliases:**
`coin, flip`

**Examples:**

```
!coinflip
```
</code></details>

---

## dice
<details><summary markdown='span'>Expand for additional information</summary><code>

*Roll a dice.*

**Aliases:**
`die, roll`

**Examples:**

```
!dice
```
</code></details>

---

## giveme
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

## Group: insult
<details><summary markdown='span'>Expand for additional information</summary><code>

*Insults manipulation. If invoked without subcommands, insults a given user.*

**Aliases:**
`burn, insults, ins, roast`

**Arguments:**

(optional) `[user]` : *User to insult.* (def: `None`)

**Examples:**

```
!insult @Someone
```
</code></details>

---

### insult add
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### insult clear
<details><summary markdown='span'>Expand for additional information</summary><code>

*Delete all insults.*

**Owner-only.**

**Aliases:**
`da, c, ca, cl, clearall`

**Examples:**

```
!insults clear
```
</code></details>

---

### insult delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### insult list
<details><summary markdown='span'>Expand for additional information</summary><code>

*Show all insults.*

**Aliases:**
`ls, l`

**Examples:**

```
!insult list
```
</code></details>

---

## invite
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get an instant invite link for the current guild.*

**Requires permissions:**
`Create instant invites`

**Aliases:**
`getinvite`

**Examples:**

```
!invite
```
</code></details>

---

## items
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

## leave
<details><summary markdown='span'>Expand for additional information</summary><code>

*Makes Godfather leave the guild.*

**Requires user permissions:**
`Administrator`

**Examples:**

```
!leave
```
</code></details>

---

## leet
<details><summary markdown='span'>Expand for additional information</summary><code>

*Wr1t3s m3ss@g3 1n 1337sp34k.*

**Aliases:**
`l33t`

**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!leet Some sentence
```
</code></details>

---

## Group: meme
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### meme add
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### meme clear
<details><summary markdown='span'>Expand for additional information</summary><code>

*Deletes all guild memes.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, ca, cl, clearall`

**Examples:**

```
!memes clear
```
</code></details>

---

### meme create
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### meme delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### meme list
<details><summary markdown='span'>Expand for additional information</summary><code>

*List all registered memes for this guild.*

**Aliases:**
`ls, l`

**Examples:**

```
!meme list
```
</code></details>

---

### meme templates
<details><summary markdown='span'>Expand for additional information</summary><code>

*Lists all available meme templates.*

**Aliases:**
`template, t`

**Examples:**

```
!meme templates
```
</code></details>

---

## news
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get newest world news.*

**Aliases:**
`worldnews`

**Examples:**

```
!news
```
</code></details>

---

## penis
<details><summary markdown='span'>Expand for additional information</summary><code>

*An accurate measurement.*

**Aliases:**
`size, length, manhood, dick`

**Arguments:**

(optional) `[user]` : *Who to measure.* (def: `None`)

**Examples:**

```
!penis @Someone
```
</code></details>

---

## peniscompare
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

## ping
<details><summary markdown='span'>Expand for additional information</summary><code>

*Ping the bot.*

**Examples:**

```
!ping
```
</code></details>

---

## prefix
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### random cat
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get a random cat image.*

**Examples:**

```
!random cat
```
</code></details>

---

### random choose
<details><summary markdown='span'>Expand for additional information</summary><code>

*Choose one of the provided options separated by comma.*

**Aliases:**
`select`

**Arguments:**

`[string...]` : *Option list (separated by comma).*

**Examples:**

```
!random choose option 1, option 2, option 3...
```
</code></details>

---

### random dog
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get a random dog image.*

**Examples:**

```
!random dog
```
</code></details>

---

### random raffle
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

## Group: rank
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### rank list
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print all available ranks.*

**Aliases:**
`levels`

**Examples:**

```
!rank list
```
</code></details>

---

### rank top
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get rank leaderboard.*

**Examples:**

```
!rank top
```
</code></details>

---

## rate
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

## remind
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

## report
<details><summary markdown='span'>Expand for additional information</summary><code>

*Send a report message to owner about a bug (please don't abuse... please).*

**Arguments:**

`[string...]` : *Issue text.*

**Examples:**

```
!report Your bot sucks!
```
</code></details>

---

## say
<details><summary markdown='span'>Expand for additional information</summary><code>

*Echo echo echo.*

**Aliases:**
`repeat`

**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!say I am gay.
```
</code></details>

---

## Group: shop
<details><summary markdown='span'>Expand for additional information</summary><code>

*Shop for items using WM credits from your bank account. If invoked without subcommand, lists all available items for purchase.*

**Aliases:**
`store`

**Examples:**

```
!shop
```
</code></details>

---

### shop add
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### shop buy
<details><summary markdown='span'>Expand for additional information</summary><code>

*Purchase an item from this guild's shop.*

**Aliases:**
`purchase, shutupandtakemymoney, b, p`

**Arguments:**

`[int]` : *Item ID.*

**Examples:**

```
!shop buy 3
```
</code></details>

---

### shop delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### shop list
<details><summary markdown='span'>Expand for additional information</summary><code>

*List all purchasable items for this guild.*

**Aliases:**
`ls`

**Examples:**

```
!shop list
```
</code></details>

---

### shop sell
<details><summary markdown='span'>Expand for additional information</summary><code>

*Sell a purchased item for half the buy price.*

**Aliases:**
`return`

**Arguments:**

`[int]` : *Item ID.*

**Examples:**

```
!shop sell 3
```
</code></details>

---

## tts
<details><summary markdown='span'>Expand for additional information</summary><code>

*Sends a tts message.*

**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!tts I am gay.
```
</code></details>

---

## zugify
<details><summary markdown='span'>Expand for additional information</summary><code>

*I don't even...*

**Aliases:**
`z`

**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!zugify Some random text
```
</code></details>

---

