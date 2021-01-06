# Module: Uncategorized
*Uncategorized commands.*


## 8ball
<details><summary markdown='span'>Expand for additional information</summary><p>

*Do you ponder the mysteries of our world? Ask the Almighty 8Ball whatever you want! But beware, because the truth can sometimes hurt...*

**Aliases:**
`8b`
**Arguments:**

`[string...]` : *A question for the Almighty 8Ball*

**Examples:**

```xml
!8ball Some string here
```
</p></details>

---

## cat
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves a random cat image.*

**Aliases:**
`kitty, kitten`
**Examples:**

```xml
!cat
```
</p></details>

---

## coinflip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Flips a coin!*

**Aliases:**
`coin, flip`
**Arguments:**

(optional) `[int]` : *Reciprocal coinflip ratio* (def: `1`)

**Examples:**

```xml
!coinflip
!coinflip 5
```
</p></details>

---

## dice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Throws a dice!*

**Aliases:**
`die, roll`
**Arguments:**

(optional) `[int]` : *How many sides will the dice have?* (def: `6`)

**Examples:**

```xml
!dice
!dice 5
```
</p></details>

---

## dog
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves a random dog image.*

**Aliases:**
`doge, puppy, pup`
**Examples:**

```xml
!dog
```
</p></details>

---

## invite
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get or create an instant invite link for the current guild.*

**Aliases:**
`getinvite`
**Requires permissions:**
`Create instant invites`

**Arguments:**

(optional) `[time span]` : *Invite expiry time* (def: `None`)

**Examples:**

```xml
!invite
!invite 1d
```
</p></details>

---

## ip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves geolocation data for given IP.*

**Aliases:**
`ipstack, geolocation, iplocation, iptracker, iptrack, trackip, iplocate, geoip`
**Arguments:**

`[IPAddress]` : *IP address*

**Examples:**

```xml
!ip 123.123.123.123
```
</p></details>

---

## leave
<details><summary markdown='span'>Expand for additional information</summary><p>

*Makes me leave the guild.*

**Requires permissions:**
`Administrator`

**Examples:**

```xml
!leave
```
</p></details>

---

## leet
<details><summary markdown='span'>Expand for additional information</summary><p>

*Wr1t3s g1v3n tEx7 1n p5EuDo 1337sp34k.*

**Aliases:**
`l33t, 1337`
**Arguments:**

`[string...]` : *Text to repeat*

**Examples:**

```xml
!leet Some string here
```
</p></details>

---

## news
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves latest world news.*

**Aliases:**
`worldnews`
**Arguments:**

(optional) `[string]` : *Topic* (def: `world`)

**Examples:**

```xml
!news
```
</p></details>

---

## penis
<details><summary markdown='span'>Expand for additional information</summary><p>

*An accurate measurement.*

**Aliases:**
`size, length, manhood, dick, dicksize`
**Overload 1:**

`[member...]` : *Member(s)*

**Overload 0:**

`[user...]` : *User(s)*

**Examples:**

```xml
!penis
!penis @User
!penis @User @User @User
```
</p></details>

---

## penisbros
<details><summary markdown='span'>Expand for additional information</summary><p>

*Finds members with same `penis` command result as the given user.*

**Aliases:**
`sizebros, lengthbros, manhoodbros, dickbros, cockbros`
**Guild only.**

**Overload 1:**

`[member]` : *Member*

**Overload 0:**

(optional) `[user]` : *User* (def: `None`)

**Examples:**

```xml
!penisbros
!penisbros @User
```
</p></details>

---

## ping
<details><summary markdown='span'>Expand for additional information</summary><p>

*Pings the bot.*

**Examples:**

```xml
!ping
```
</p></details>

---

## prefix
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets command prefix.*

**Aliases:**
`setprefix, pref, setpref`
**Guild only.**

**Requires permissions:**
`Administrator`

**Arguments:**

(optional) `[string]` : *New command prefix* (def: `None`)

**Examples:**

```xml
!prefix
!prefix .
```
</p></details>

---

## quoteoftheday
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves the quote of the day. You can also specify a category from the list: inspire, management, sports, life, funny, love, art, students.*

**Aliases:**
`qotd, qod, quote, q`
**Arguments:**

(optional) `[string]` : *Topic* (def: `None`)

**Examples:**

```xml
!quoteoftheday
```
</p></details>

---

## rate
<details><summary markdown='span'>Expand for additional information</summary><p>

*A very accurate personality measurement.*

**Aliases:**
`score, graph, rating`
**Requires bot permissions:**
`Attach files`

**Overload 1:**

`[member...]` : *Member(s)*

**Overload 0:**

`[user...]` : *User(s)*

**Examples:**

```xml
!rate
!rate @User
!rate @User @User @User
```
</p></details>

---

## report
<details><summary markdown='span'>Expand for additional information</summary><p>

*Report an issue with the bot.*

**Arguments:**

`[string...]` : *Issue to report*

**Examples:**

```xml
!report Report message containing the detailed issue description
```
</p></details>

---

## rss
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves latest topics from given RSS feed URL.*

**Aliases:**
`feed`
**Arguments:**

`[URL]` : *RSS feed URL*

**Examples:**

```xml
!rss http://some.rss.feed.url/.rss
```
</p></details>

---

## say
<details><summary markdown='span'>Expand for additional information</summary><p>

*Echo! Echo! Echo!*

**Aliases:**
`repeat, echo`
**Arguments:**

`[string...]` : *Text to repeat*

**Examples:**

```xml
!say Some string here
```
</p></details>

---

## Group: subscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Commands for adding feed subscriptions. The bot will send a message when the latest topic is changed. Group call subscribes the bot to the given RSS feed URL in given channel or lists active subscriptions for given channel. If channel is not provided, uses current channel.*

**Aliases:**
`sub, subscriptions, subscription`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 2:**

`[channel]` : *Channel for updates*

`[URL]` : *RSS feed URL*

(optional) `[string...]` : *Friendly name* (def: `None`)

**Overload 1:**

`[URL]` : *RSS feed URL*

(optional) `[channel]` : *Channel for updates* (def: `None`)

(optional) `[string...]` : *Friendly name* (def: `None`)

**Overload 0:**

(optional) `[channel]` : *Channel for updates* (def: `None`)

**Examples:**

```xml
!subscribe http://some.rss.feed.url/.rss
!subscribe http://some.rss.feed.url/.rss #my-text-channel SubscriptionName
!subscribe #my-text-channel
```
</p></details>

---

### subscribe list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists active subscriptions for current channel.*

**Aliases:**
`ls, listsubs, listfeeds`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Arguments:**

(optional) `[channel]` : *Channel for updates* (def: `None`)

**Examples:**

```xml
!subscribe list
!subscribe list #my-text-channel
```
</p></details>

---

### subscribe reddit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Subscribes to a given subreddit.*

**Aliases:**
`r`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 1:**

`[channel]` : *Channel for updates*

`[string]` : *Subreddit*

**Overload 0:**

`[string]` : *Subreddit*

(optional) `[channel]` : *Channel for updates* (def: `None`)

**Examples:**

```xml
!subscribe reddit awww
!subscribe reddit awww #my-text-channel
```
</p></details>

---

### subscribe youtube
<details><summary markdown='span'>Expand for additional information</summary><p>

*Subscribes to a given YouTube channel.*

**Aliases:**
`y, yt, ytube`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 2:**

`[channel]` : *Channel for updates*

`[URL]` : *Channel where to send updates*

(optional) `[string...]` : *Friendly name* (def: `None`)

**Overload 1:**

`[URL]` : *Channel where to send updates*

`[channel]` : *Channel for updates*

(optional) `[string...]` : *Friendly name* (def: `None`)

**Overload 0:**

`[URL]` : *Channel where to send updates*

(optional) `[string...]` : *Friendly name* (def: `None`)

**Examples:**

```xml
!subscribe youtube https://www.youtube.com/channel/UCA5u8UquvO44Jcd3wZApyDg
!subscribe youtube https://www.youtube.com/channel/UCA5u8UquvO44Jcd3wZApyDg SubscriptionName
!subscribe youtube UCA5u8UquvO44Jcd3wZApyDg #my-text-channel
```
</p></details>

---

## tts
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a TTS message.*

**Requires permissions:**
`Send TTS messages`

**Arguments:**

`[string...]` : *Text to repeat*

**Examples:**

```xml
!tts Some string here
```
</p></details>

---

## unleet
<details><summary markdown='span'>Expand for additional information</summary><p>

*Attempts to translate message from leetspeak.*

**Aliases:**
`unl33t`
**Arguments:**

`[string...]` : *Text to repeat*

**Examples:**

```xml
!unleet Some string here
```
</p></details>

---

## Group: unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Commands for removing feed subscriptions. Group call unsubscribes the bot from given feed by ID or friendly name.*

**Aliases:**
`unsub`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 1:**

`[int...]` : *ID(s)*

**Overload 0:**

`[string...]` : *Friendly name*

**Examples:**

```xml
!unsubscribe 12345
!unsubscribe SubscriptionName
```
</p></details>

---

### unsubscribe all
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all subscriptions in given channel.*

**Aliases:**
`a`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Arguments:**

(optional) `[channel]` : *Channel for updates* (def: `None`)

**Examples:**

```xml
!unsubscribe all
!unsubscribe all #my-text-channel
```
</p></details>

---

### unsubscribe reddit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unsubscribes from a reddit sub.*

**Aliases:**
`r`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Arguments:**

`[string]` : *Subreddit*

**Examples:**

```xml
!unsubscribe reddit awww
```
</p></details>

---

### unsubscribe youtube
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unsubscribes from a YouTube channel.*

**Aliases:**
`y, yt, ytube`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Arguments:**

`[string...]` : *YouTube channel URL or friendly name*

**Examples:**

```xml
!unsubscribe youtube SubscriptionName
!unsubscribe youtube https://www.youtube.com/channel/UCA5u8UquvO44Jcd3wZApyDg
```
</p></details>

---

