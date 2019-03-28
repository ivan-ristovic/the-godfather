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

## crime
<details><summary markdown='span'>Expand for additional information</summary><p>

*Commit a crime and hope to get away with large amounts of cash. You can attempt to commit a crime once every 10 minutes.*

**Examples:**

```
!crime
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
`ip, geolocation, iplocation, iptracker, iptrack, trackip, iplocate, geoip`

**Arguments:**

`[IPAddress]` : *IP.*

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

## nsfw
<details><summary markdown='span'>Expand for additional information</summary><p>

*Wraps the URL into a special NSFW block.*

**Requires bot permissions:**
`Manage messages`

**Arguments:**

`[URL]` : *URL to wrap.*

(optional) `[string...]` : *Additional info* (def: `None`)

**Examples:**

```
!nsfw some_nasty_nsfw_url_here
!nsfw some_nasty_nsfw_url_here additional info
```
</p></details>

---

## penis
<details><summary markdown='span'>Expand for additional information</summary><p>

*An accurate measurement.*

**Aliases:**
`size, length, manhood, dick, dicksize`

**Overload 1:**

(optional) `[member]` : *Who to measure.* (def: `None`)

**Overload 0:**

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

**Overload 1:**

`[member...]` : *User1.*

**Overload 0:**

`[user...]` : *User1.*

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

**Overload 1:**

`[member...]` : *Who to measure.*

**Overload 1:**

`[user...]` : *Who to measure.*

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

## rss
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get the latest topics from the given RSS feed URL.*

**Aliases:**
`feed`

**Arguments:**

`[URL]` : *RSS feed URL.*

**Examples:**

```
!rss https://news.google.com/news/rss/
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

## simulate
<details><summary markdown='span'>Expand for additional information</summary><p>

*Simulate another user.*

**Aliases:**
`sim`

**Arguments:**

`[member]` : *Member to simulate.*

**Examples:**

```
!simulate @Someone.
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

## slut
<details><summary markdown='span'>Expand for additional information</summary><p>

*Work the streets tonight hoping to gather some easy money but beware, there are many threats lurking at that hour. You can work the streets once per 5s.*

**Examples:**

```
!slut
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

## Group: subscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Commands for managing feed subscriptions. The bot will send a message when the latest topic is changed. Group call subscribes the bot to the given RSS feed URL or lists active subs.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`sub, subscriptions, subscription`

**Overload 1:**

`[URL]` : *URL.*

(optional) `[string...]` : *Friendly name.* (def: `None`)

**Examples:**

```
!subscribe https://news.google.com/news/rss/
!subscribe https://news.google.com/news/rss/ news
```
</p></details>

---

### subscribe list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get feed list for the current channel.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`ls, listsubs, listfeeds`

**Examples:**

```
!subscribe list
```
</p></details>

---

### subscribe reddit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add new subscription for a subreddit.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`r`

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!subscribe reddit aww
```
</p></details>

---

### subscribe youtube
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a new subscription for a YouTube channel.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`y, yt, ytube`

**Arguments:**

`[string]` : *Channel URL.*

(optional) `[string]` : *Friendly name.* (def: `None`)

**Examples:**

```
!subscribe youtube https://www.youtube.com/user/RickAstleyVEVO
!subscribe youtube https://www.youtube.com/user/RickAstleyVEVO rick
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

## unleet
<details><summary markdown='span'>Expand for additional information</summary><p>

*Translates a message from leetspeak (expecting only letters in translated output).*

**Aliases:**
`unl33t`

**Arguments:**

`[string...]` : *Text to unleet.*

**Examples:**

```
!unleet w0W 5uCh C0oL
```
</p></details>

---

## Group: unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove an existing feed subscription.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`unsub`

**Overload 1:**

`[int...]` : *ID of the subscriptions to remove.*

**Overload 0:**

`[string...]` : *Name of the subscription.*

**Examples:**

```
!unsubscribe 1
```
</p></details>

---

### unsubscribe all
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove all subscriptions for the given channel.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`a`

**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

**Examples:**

```
!unsub all
```
</p></details>

---

### unsubscribe reddit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove a subscription using subreddit name or subscription ID (use command ``subscriptions list`` to see IDs).*

**Requires permissions:**
`Manage guild`

**Aliases:**
`r`

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!unsub reddit aww
```
</p></details>

---

### unsubscribe youtube
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove a YouTube channel subscription.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`y, yt, ytube`

**Arguments:**

`[string]` : *Channel URL or subscription name.*

**Examples:**

```
!youtube unsubscribe https://www.youtube.com/user/RickAstleyVEVO
!youtube unsubscribe rick
```
</p></details>

---

## uptime
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints out bot runtime information.*

**Examples:**

```
!uptime
```
</p></details>

---

## work
<details><summary markdown='span'>Expand for additional information</summary><p>

*Do something productive with your life. You can work once per minute.*

**Aliases:**
`job`

**Examples:**

```
!work
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

