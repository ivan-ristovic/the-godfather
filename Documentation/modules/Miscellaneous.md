# Module: Miscellaneous

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

## coinflip
*Flip a coin.*

**Aliases:**
`coin, flip`

**Examples:**

```
!coinflip
```
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

**Overload 0:**

`[string...]` : *Meme name.*

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

## news
*Get newest world news.*

**Aliases:**
`worldnews`

**Examples:**

```
!news
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

## tts
*Sends a tts message.*

**Arguments:**

`[string...]` : *Text.*

**Examples:**

```
!tts I am gay.
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

