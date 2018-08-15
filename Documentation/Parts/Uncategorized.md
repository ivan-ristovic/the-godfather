# Module: Uncategorized

## 8ball
<details><summary markdown='span'>Expand for additional information</summary><p>

*An almighty ball which knows the answer to any question you ask. Alright, the answer is random, so what?*

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

## cat
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get a random cat image.*

**Aliases:**
`kitty, kitten`

**Examples:**

```
!random cat
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

## connect
<details><summary markdown='span'>Expand for additional information</summary><p>

*Connect the bot to a voice channel. If the channel is not given, connects the bot to the same channel you are in.*

**Owner-only.**

**Requires bot permissions:**
`Use voice chat`

**Aliases:**
`con, conn, enter`

**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

**Examples:**

```
!connect
!connect Music
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

## disconnect
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disconnects the bot from the voice channel.*

**Owner-only.**

**Requires bot permissions:**
`Use voice chat`

**Aliases:**
`dcon, dconn, discon, disconn, dc`

**Examples:**

```
!disconnect
```
</p></details>

---

## dog
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get a random dog image.*

**Aliases:**
`doge, puppy, pup`

**Examples:**

```
!random dog
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

## help
<details><summary markdown='span'>Expand for additional information</summary><p>

*Displays command help.*

**Arguments:**

`[string...]` : *Command to provide help for.*

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

## ipstack
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieve IP geolocation information.*

**Aliases:**
`ip, geolocation, iplocation, iptracker, iptrack, trackip, iplocate`

**Arguments:**

`[CustomIPFormat]` : *IP.*

**Examples:**

```
!ipstack 123.123.123.123
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

**Requires permissions:**
`Administrator`

**Examples:**

```
!leave
```
</p></details>

---

## leet
<details><summary markdown='span'>Expand for additional information</summary><p>

*Wr1t3s g1v3n tEx7 1n p5EuDo 1337sp34k.*

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

**Requires permissions:**
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

## quoteoftheday
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get quote of the day. You can also specify a category from the list: inspire, management, sports, life, funny, love, art, students.*

**Aliases:**
`qotd, qod, quote, q`

**Arguments:**

(optional) `[string]` : *Category.* (def: `None`)

**Examples:**

```
!quoteoftheday
!quoteoftheday life
```
</p></details>

---

## rate
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gives a rating chart for the user. If the user is not provided, rates sender.*

**Requires bot permissions:**
`Attach files`

**Aliases:**
`score, graph, rating`

**Arguments:**

(optional) `[user]` : *Who to measure.* (def: `None`)

**Examples:**

```
!rate @Someone
```
</p></details>

---

## reactionspoll
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

## remind
<details><summary markdown='span'>Expand for additional information</summary><p>

*Resend a message after some time.*

**Requires permissions:**
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

`[string...]` : *Text to say.*

**Examples:**

```
!say I am gay.
```
</p></details>

---

## skip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Skip current voice playback.*

**Owner-only.**

**Requires bot permissions:**
`Use voice chat`

**Examples:**

```
!skip
```
</p></details>

---

## stop
<details><summary markdown='span'>Expand for additional information</summary><p>

*Stops current voice playback.*

**Owner-only.**

**Requires bot permissions:**
`Use voice chat`

**Examples:**

```
!stop
```
</p></details>

---

## tts
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a tts message.*

**Requires permissions:**
`Send TTS messages`

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

