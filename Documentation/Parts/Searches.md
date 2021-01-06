# Module: Searches
*This module contains internet search commands such as YouTube, Imgur, Giphy, Steam searches/subscriptions and many more.*


## Group: gif
<details><summary markdown='span'>Expand for additional information</summary><p>

*GIPHY search commands. Group call searches GIPHY with given query and prints first result.*

**Aliases:**
`giphy`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!gif Search query
```
</p></details>

---

### gif random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints a random GIF.*

**Aliases:**
`r, rand, rnd, rng`
**Examples:**

```xml
!gif random
```
</p></details>

---

### gif trending
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints a given amount of trending GIFs.*

**Aliases:**
`t, tr, trend`
**Arguments:**

(optional) `[int]` : *Number of results* (def: `5`)

**Examples:**

```xml
!gif trending
!gif trending 10
```
</p></details>

---

## Group: goodreads
<details><summary markdown='span'>Expand for additional information</summary><p>

*Goodreads search commands. Group call searches books by title, author or ISBN.*

**Aliases:**
`gr`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!goodreads
!goodreads Search query
```
</p></details>

---

### goodreads book
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search Goodreads books by title, author or ISBN.*

**Aliases:**
`books, b`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!goodreads book
!goodreads book Search query
```
</p></details>

---

## Group: imdb
<details><summary markdown='span'>Expand for additional information</summary><p>

*Open Movie Database (IMDB) search commands. Group call searches the database using the provided query as title or ID.*

**Aliases:**
`movies, series, serie, movie, film, cinema, omdb`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!imdb Sharknado
```
</p></details>

---

### imdb id
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches the database using the provided query as ID.*

**Arguments:**

`[string]` : *ID*

**Examples:**

```xml
!imdb id tt1190634
```
</p></details>

---

### imdb search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches the database using the provided query as title or ID.*

**Aliases:**
`s, find`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!imdb search Sharknado
```
</p></details>

---

### imdb title
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches the database using the provided query as a title.*

**Aliases:**
`t, name, n`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!imdb title Sharknado
```
</p></details>

---

## Group: imgur
<details><summary markdown='span'>Expand for additional information</summary><p>

*Imgur search commands. Group call retrieves top ranked images from given subreddit for this day.*

**Aliases:**
`img, im, i`
**Overload 1:**

`[int]` : *Number of results*

`[string...]` : *Subreddit*

**Overload 0:**

`[string]` : *Subreddit*

(optional) `[int]` : *Number of results* (def: `1`)

**Examples:**

```xml
!imgur 10 awww
!imgur awww
```
</p></details>

---

### imgur latest
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves latest images from the given subreddit.*

**Aliases:**
`l, new, newest`
**Overload 1:**

`[int]` : *Number of results*

`[string...]` : *Subreddit*

**Overload 0:**

`[string]` : *Subreddit*

`[int]` : *Number of results*

**Examples:**

```xml
!imgur latest 10 awww
!imgur latest awww
```
</p></details>

---

### imgur top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves top rated images from the given subreddit in the given timespan.*

**Aliases:**
`t`
**Overload 3:**

`[TimeWindow]` : *Time window (day/month/week/year/all)*

`[int]` : *Number of results*

`[string...]` : *Subreddit*

**Overload 2:**

`[TimeWindow]` : *Time window (day/month/week/year/all)*

`[string]` : *Subreddit*

(optional) `[int]` : *Number of results* (def: `1`)

**Overload 1:**

`[int]` : *Number of results*

`[TimeWindow]` : *Time window (day/month/week/year/all)*

`[string...]` : *Subreddit*

**Overload 0:**

`[int]` : *Number of results*

`[string...]` : *Subreddit*

**Examples:**

```xml
!imgur top 10 awww week
!imgur top 10 week awww
!imgur top week 10 awww
```
</p></details>

---

## Group: joke
<details><summary markdown='span'>Expand for additional information</summary><p>

*Joke searching commands. Group call returns a random joke.*

**Aliases:**
`jokes, j`
**Examples:**

```xml
!joke
```
</p></details>

---

### joke search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches for the joke containing the given query.*

**Aliases:**
`s`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!joke search Search query
```
</p></details>

---

### joke yourmom
<details><summary markdown='span'>Expand for additional information</summary><p>

*Yo mama so...*

**Aliases:**
`mama, m, yomomma, yomom, yomoma, yomamma, yomama`
**Examples:**

```xml
!joke yourmom
```
</p></details>

---

## Group: reddit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Reddit search commands. Group call retrieves hottest posts from given sub.*

**Aliases:**
`r`
**Arguments:**

(optional) `[string]` : *Subreddit* (def: `all`)

**Examples:**

```xml
!reddit
!reddit awww
```
</p></details>

---

### reddit controversial
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves controversial posts from given sub.*

**Aliases:**
`c`
**Arguments:**

`[string]` : *Subreddit*

**Examples:**

```xml
!reddit controversial awww
```
</p></details>

---

### reddit gilded
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves gilded posts from given sub.*

**Aliases:**
`g`
**Arguments:**

`[string]` : *Subreddit*

**Examples:**

```xml
!reddit gilded awww
```
</p></details>

---

### reddit hot
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves hot posts from given sub.*

**Aliases:**
`h`
**Arguments:**

`[string]` : *Subreddit*

**Examples:**

```xml
!reddit hot awww
```
</p></details>

---

### reddit new
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves newest posts from given sub.*

**Aliases:**
`n, newest, latest`
**Arguments:**

`[string]` : *Subreddit*

**Examples:**

```xml
!reddit new awww
```
</p></details>

---

### reddit rising
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves rising posts from given sub.*

**Aliases:**
`r`
**Arguments:**

`[string]` : *Subreddit*

**Examples:**

```xml
!reddit rising awww
```
</p></details>

---

### reddit subscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Subscribes to a given subreddit.*

**Aliases:**
`sub, follow`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Arguments:**

`[channel]` : *Channel*

`[string]` : *Subreddit*

**Examples:**

```xml
!reddit subscribe awww
!reddit subscribe awww #my-text-channel
```
</p></details>

---

### reddit top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves top posts from given sub.*

**Aliases:**
`t`
**Arguments:**

`[string]` : *Subreddit*

**Examples:**

```xml
!reddit top awww
```
</p></details>

---

### reddit unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes a subscription for given subreddit.*

**Aliases:**
`unfollow, unsub`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Arguments:**

`[string]` : *Subreddit*

**Examples:**

```xml
!reddit unsubscribe awww
```
</p></details>

---

## Group: steam
<details><summary markdown='span'>Expand for additional information</summary><p>

*Steam community commands. Group call searches Steam community profiles by ID or username (vanity URL).*

**Aliases:**
`s, st`
**Overload 1:**

`[unsigned long]` : *ID*

**Overload 0:**

`[string...]` : *Username*

**Examples:**

```xml
!steam 361119455792594954
!steam SampleName
```
</p></details>

---

### steam game
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches Steam store by game ID or name.*

**Aliases:**
`g, gm, store`
**Overload 1:**

`[unsigned int]` : *ID*

**Overload 0:**

`[string...]` : *Game name*

**Examples:**

```xml
!steam game 12345
!steam game SampleName
```
</p></details>

---

### steam profile
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches Steam community profiles by ID or username (vanity URL).*

**Aliases:**
`id, user, info`
**Overload 1:**

`[unsigned long]` : *ID*

**Overload 0:**

`[string...]` : *Username*

**Examples:**

```xml
!steam profile 361119455792594954
!steam profile SampleName
```
</p></details>

---

## Group: sticker
<details><summary markdown='span'>Expand for additional information</summary><p>

*GIPHY sticker search commands. Group call searches GIPHY with given query and prints first stricker result.*

**Aliases:**
`stickers`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!sticker Search query
```
</p></details>

---

### sticker random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints a random sticker.*

**Aliases:**
`r, rand, rnd, rng`
**Examples:**

```xml
!sticker random
```
</p></details>

---

### sticker trending
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints a given amount of trending stickers.*

**Aliases:**
`t, tr, trend`
**Arguments:**

(optional) `[int]` : *Number of results* (def: `5`)

**Examples:**

```xml
!sticker trending
!sticker trending 10
```
</p></details>

---

## Group: urbandict
<details><summary markdown='span'>Expand for additional information</summary><p>

*Urban Dictionary commands. Group call searches Urban Dictionary for a given query.*

**Aliases:**
`ud, urban, urbandictionary`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!urbandict Search query
```
</p></details>

---

## Group: weather
<details><summary markdown='span'>Expand for additional information</summary><p>

*Weather search commands. Group call returns weather information for given query.*

**Aliases:**
`w`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!weather London
```
</p></details>

---

### weather forecast
<details><summary markdown='span'>Expand for additional information</summary><p>

*Returns weather forecast for the given city and amount of days in advance.*

**Aliases:**
`f`
**Overload 1:**

`[int]` : *Amount of days*

`[string...]` : *Query*

**Overload 0:**

`[string...]` : *Query*

**Examples:**

```xml
!weather forecast 5 London
```
</p></details>

---

## Group: wikipedia
<details><summary markdown='span'>Expand for additional information</summary><p>

*Wikipedia search commands. Group call searches Wikipedia with given query.*

**Aliases:**
`wiki`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!wikipedia Search query
```
</p></details>

---

### wikipedia search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches Wikipedia with given query.*

**Aliases:**
`s, find`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!wikipedia search Search query
```
</p></details>

---

## Group: xkcd
<details><summary markdown='span'>Expand for additional information</summary><p>

*xkcd search commands. Group call returns a random comic or, if an ID is provided, a comic with given ID.*

**Aliases:**
`x`
**Overload 1:**

`[int]` : *ID*

**Examples:**

```xml
!xkcd
```
</p></details>

---

### xkcd id
<details><summary markdown='span'>Expand for additional information</summary><p>

*Returns a xkcd comic with given ID.*

**Arguments:**

(optional) `[int]` : *ID* (def: `None`)

**Examples:**

```xml
!xkcd id 5
```
</p></details>

---

### xkcd latest
<details><summary markdown='span'>Expand for additional information</summary><p>

*Returns latest xkcd comic.*

**Aliases:**
`fresh, newest, l`
**Examples:**

```xml
!xkcd latest
```
</p></details>

---

### xkcd random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Returns random xkcd comic.*

**Aliases:**
`rnd, r, rand`
**Examples:**

```xml
!xkcd random
```
</p></details>

---

## Group: youtube
<details><summary markdown='span'>Expand for additional information</summary><p>

*Youtube search commands. Group call searches YouTube for given query.*

**Aliases:**
`y, yt, ytube`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!youtube Search query
```
</p></details>

---

### youtube search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches YouTube for given query and returns given amount of results at most.*

**Aliases:**
`s`
**Arguments:**

`[int]` : *Amount of results to fetch*

`[string...]` : *Query*

**Examples:**

```xml
!youtube search 5 Search query
```
</p></details>

---

### youtube searchchannel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches YouTube for given query and returns given amount of YouTube channels at most.*

**Aliases:**
`searchchannels, sc, searchc, channel`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!youtube searchchannel Search query
```
</p></details>

---

### youtube searchplaylist
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches YouTube for given query and returns given amount of YouTube playlists at most.*

**Aliases:**
`searchplaylists, sp, searchp, playlist`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!youtube searchplaylist Search query
```
</p></details>

---

### youtube searchvideo
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches YouTube for given query and returns given amount of YouTube videos at most.*

**Aliases:**
`searchvideos, sv, searchv, video`
**Arguments:**

`[string...]` : *Query*

**Examples:**

```xml
!youtube searchvideo Search query
```
</p></details>

---

### youtube subscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Subscribes to a YouTube channel.*

**Aliases:**
`sub, follow`
**Requires user permissions:**
`Manage guild`

**Overload 5:**

`[channel]` : *Channel for updates*

`[URL]` : *YouTube channel/video URL*

(optional) `[string...]` : *Friendly name* (def: `None`)

**Overload 4:**

`[URL]` : *YouTube channel/video URL*

`[channel]` : *Channel for updates*

(optional) `[string...]` : *Friendly name* (def: `None`)

**Overload 3:**

`[string]` : *YouTube username or channel ID*

(optional) `[channel]` : *Channel for updates* (def: `None`)

(optional) `[string...]` : *Friendly name* (def: `None`)

**Overload 2:**

`[channel]` : *Channel for updates*

`[string]` : *YouTube username or channel ID*

(optional) `[string...]` : *Friendly name* (def: `None`)

**Overload 1:**

`[string]` : *YouTube username or channel ID*

(optional) `[string...]` : *Friendly name* (def: `None`)

**Overload 0:**

`[URL]` : *YouTube username or channel ID*

(optional) `[string...]` : *Friendly name* (def: `None`)

**Examples:**

```xml
!youtube subscribe https://www.youtube.com/channel/UCA5u8UquvO44Jcd3wZApyDg
!youtube subscribe UCA5u8UquvO44Jcd3wZApyDg
!youtube subscribe https://www.youtube.com/channel/UCA5u8UquvO44Jcd3wZApyDg SubscriptionName
```
</p></details>

---

### youtube unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes registered YouTube subscription.*

**Aliases:**
`unfollow, unsub`
**Requires user permissions:**
`Manage guild`

**Arguments:**

`[string]` : *YouTube channel URL or friendly name*

**Examples:**

```xml
!youtube unsubscribe https://www.youtube.com/channel/UCA5u8UquvO44Jcd3wZApyDg
!youtube unsubscribe UCA5u8UquvO44Jcd3wZApyDg
!youtube unsubscribe SubscriptionName
```
</p></details>

---

