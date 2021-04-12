# Module: Uncategorized
*Uncategorized commands.*


## Group: subscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Commands for adding feed subscriptions. The bot will send a message when the latest topic is changed. Group call subscribes the bot to the given RSS feed URL in given channel or lists active subscriptions for given channel. If channel is not provided, uses current channel.*

**Aliases:**
`sub, subscriptions, subscription`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 2:**
- [`channel`]: *Channel for updates*
- [`URL`]: *RSS feed URL*
- (optional) [`string...`]: *Friendly name* (def: `None`)
**Overload 1:**
- [`URL`]: *RSS feed URL*
- (optional) [`channel`]: *Channel for updates* (def: `None`)
- (optional) [`string...`]: *Friendly name* (def: `None`)
**Overload 0:**
- (optional) [`channel`]: *Channel for updates* (def: `None`)
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

**Overload 0:**
- (optional) [`channel`]: *Channel for updates* (def: `None`)
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
- [`channel`]: *Channel for updates*
- [`string`]: *Subreddit*
**Overload 0:**
- [`string`]: *Subreddit*
- (optional) [`channel`]: *Channel for updates* (def: `None`)
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
- [`channel`]: *Channel for updates*
- [`URL`]: *Channel where to send updates*
- (optional) [`string...`]: *Friendly name* (def: `None`)
**Overload 1:**
- [`URL`]: *Channel where to send updates*
- [`channel`]: *Channel for updates*
- (optional) [`string...`]: *Friendly name* (def: `None`)
**Overload 0:**
- [`URL`]: *Channel where to send updates*
- (optional) [`string...`]: *Friendly name* (def: `None`)
**Examples:**

```xml
!subscribe youtube https://www.youtube.com/channel/UCA5u8UquvO44Jcd3wZApyDg
!subscribe youtube https://www.youtube.com/channel/UCA5u8UquvO44Jcd3wZApyDg SubscriptionName
!subscribe youtube UCA5u8UquvO44Jcd3wZApyDg #my-text-channel
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
- [`int...`]: *ID(s)*
**Overload 0:**
- [`string...`]: *Friendly name*
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

**Overload 0:**
- (optional) [`channel`]: *Channel for updates* (def: `None`)
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

**Overload 0:**
- [`string`]: *Subreddit*
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

**Overload 0:**
- [`string...`]: *YouTube channel URL or friendly name*
**Examples:**

```xml
!unsubscribe youtube SubscriptionName
!unsubscribe youtube https://www.youtube.com/channel/UCA5u8UquvO44Jcd3wZApyDg
```
</p></details>

---

