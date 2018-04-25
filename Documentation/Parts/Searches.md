# Module: Searches

## Group: gif
<details><summary markdown='span'>Expand for additional information</summary><code>

*GIPHY commands. If invoked without a subcommand, searches GIPHY with given query.*

**Aliases:**
`giphy`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!gif wat
```
</code></details>

---

### gif random
<details><summary markdown='span'>Expand for additional information</summary><code>

*Return a random GIF.*

**Aliases:**
`r, rand, rnd`

**Examples:**

```
!gif random
```
</code></details>

---

### gif trending
<details><summary markdown='span'>Expand for additional information</summary><code>

*Return an amount of trending GIFs.*

**Aliases:**
`t, tr, trend`

**Arguments:**

(optional) `[int]` : *Number of results (1-10).* (def: `5`)

**Examples:**

```
!gif trending 3
!gif trending
```
</code></details>

---

## Group: imdb
<details><summary markdown='span'>Expand for additional information</summary><code>

*Search Open Movie Database.*

**Aliases:**
`movies, series, serie, movie, film, cinema, omdb`

**Arguments:**

`[string...]` : *Title.*

**Examples:**

```
!imdb Airplane
```
</code></details>

---

### imdb id
<details><summary markdown='span'>Expand for additional information</summary><code>

*Search by IMDb ID.*

**Arguments:**

`[string]` : *ID.*

**Examples:**

```
!imdb id tt4158110
```
</code></details>

---

### imdb search
<details><summary markdown='span'>Expand for additional information</summary><code>

*Searches IMDb for given query and returns paginated results.*

**Aliases:**
`s, find`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!imdb search Kill Bill
```
</code></details>

---

### imdb title
<details><summary markdown='span'>Expand for additional information</summary><code>

*Search by title.*

**Aliases:**
`t, name, n`

**Arguments:**

`[string...]` : *Title.*

**Examples:**

```
!imdb title Airplane
```
</code></details>

---

## Group: imgur
<details><summary markdown='span'>Expand for additional information</summary><code>

*Search imgur. Invoking without subcommand retrieves top ranked images from given subreddit.*

**Aliases:**
`img, im, i`

**Overload 1:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Overload 0:**

`[string]` : *Subreddit.*

(optional) `[int]` : *Number of images to print [1-10].* (def: `1`)

**Examples:**

```
!imgur aww
!imgur 10 aww
!imgur aww 10
```
</code></details>

---

### imgur latest
<details><summary markdown='span'>Expand for additional information</summary><code>

*Return latest images from given subreddit.*

**Aliases:**
`l, new, newest`

**Overload 1:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Overload 0:**

`[string]` : *Subreddit.*

`[int]` : *Number of images to print [1-10].*

**Examples:**

```
!imgur latest 5 aww
!imgur latest aww 5
```
</code></details>

---

### imgur top
<details><summary markdown='span'>Expand for additional information</summary><code>

*Return amount of top rated images in the given subreddit for given timespan.*

**Aliases:**
`t`

**Overload 3:**

`[TimeWindow]` : *Timespan in which to search (day/week/month/year/all).*

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Overload 2:**

`[TimeWindow]` : *Timespan in which to search (day/week/month/year/all).*

`[string]` : *Subreddit.*

(optional) `[int]` : *Number of images to print [1-10].* (def: `1`)

**Overload 1:**

`[int]` : *Number of images to print [1-10].*

`[TimeWindow]` : *Timespan in which to search (day/week/month/year/all).*

`[string...]` : *Subreddit.*

**Overload 0:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Examples:**

```
!imgur top day 10 aww
!imgur top 10 day aww
!imgur top 5 aww
!imgur top day aww
```
</code></details>

---

## Group: joke
<details><summary markdown='span'>Expand for additional information</summary><code>

*Group for searching jokes. If invoked without a subcommand, returns a random joke.*

**Aliases:**
`jokes, j`

**Examples:**

```
!joke
```
</code></details>

---

### joke search
<details><summary markdown='span'>Expand for additional information</summary><code>

*Search for the joke containing the given query.*

**Aliases:**
`s`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!joke search blonde
```
</code></details>

---

### joke yourmom
<details><summary markdown='span'>Expand for additional information</summary><code>

*Yo mama so...*

**Aliases:**
`mama, m, yomomma, yomom, yomoma, yomamma, yomama`

**Examples:**

```
!joke yourmom
```
</code></details>

---

## Group: reddit
<details><summary markdown='span'>Expand for additional information</summary><code>

*Reddit commands.*

**Aliases:**
`r`

**Arguments:**

(optional) `[string]` : *Subreddit.* (def: `all`)

**Examples:**

```
!reddit aww
```
</code></details>

---

### reddit subscribe
<details><summary markdown='span'>Expand for additional information</summary><code>

*Add new feed for a subreddit.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`add, a, +, sub`

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!reddit sub aww
```
</code></details>

---

### reddit unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><code>

*Remove a subreddit feed using subreddit name or subscription ID (use command ``feed list`` to see IDs).*

**Requires permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`

**Overload 1:**

`[string]` : *Subreddit.*

**Overload 0:**

`[int]` : *Subscription ID.*

**Examples:**

```
!reddit unsub aww
!reddit unsub 12
```
</code></details>

---

## Group: rss
<details><summary markdown='span'>Expand for additional information</summary><code>

*Commands for RSS feed querying or subscribing. If invoked without subcommand, gives the latest topic from the given RSS URL.*

**Aliases:**
`feed`

**Arguments:**

`[string...]` : *RSS URL.*

**Examples:**

```
!rss https://news.google.com/news/rss/
```
</code></details>

---

### rss list
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get feed list for the current channel.*

**Aliases:**
`ls, listsubs, listfeeds`

**Examples:**

```
!feed list
```
</code></details>

---

### rss subscribe
<details><summary markdown='span'>Expand for additional information</summary><code>

*Subscribe to given RSS feed URL. The bot will send a message when the latest topic is changed.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`sub, add, +`

**Arguments:**

`[string]` : *URL.*

(optional) `[string]` : *Friendly name.* (def: `None`)

**Examples:**

```
!rss subscribe https://news.google.com/news/rss/
!rss subscribe https://news.google.com/news/rss/ news
```
</code></details>

---

### rss unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><code>

*Remove an existing feed subscription.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`

**Overload 1:**

`[int]` : *ID of the subscription.*

**Overload 0:**

`[string]` : *Name of the subscription.*

**Examples:**

```
!rss unsubscribe 1
```
</code></details>

---

### rss wm
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get newest topics from WM forum.*

**Examples:**

```
!rss wm
```
</code></details>

---

### steam profile
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get Steam user information for user based on his ID.*

**Aliases:**
`id, user`

**Arguments:**

`[unsigned long]` : *ID.*

</code></details>

---

## Group: urbandict
<details><summary markdown='span'>Expand for additional information</summary><code>

*Urban Dictionary commands. If invoked without subcommand, searches Urban Dictionary for a given query.*

**Aliases:**
`ud, urban`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!urbandict blonde
```
</code></details>

---

## Group: weather
<details><summary markdown='span'>Expand for additional information</summary><code>

*Weather search commands. If invoked without subcommands, returns weather information for given query.*

**Aliases:**
`w`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!weather london
```
</code></details>

---

### weather forecast
<details><summary markdown='span'>Expand for additional information</summary><code>

*Get weather forecast for the following days (def: 7).*

**Aliases:**
`f`

**Overload 1:**

`[int]` : *Amount of days to fetch the forecast for.*

`[string...]` : *Query.*

**Overload 0:**

`[string...]` : *Query.*

**Examples:**

```
!weather forecast london
!weather forecast 5 london
```
</code></details>

---

## Group: youtube
<details><summary markdown='span'>Expand for additional information</summary><code>

*Youtube search commands. If invoked without subcommands, searches YouTube for given query.*

**Aliases:**
`y, yt, ytube`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube never gonna give you up
```
</code></details>

---

### youtube search
<details><summary markdown='span'>Expand for additional information</summary><code>

*Advanced youtube search.*

**Aliases:**
`s`

**Arguments:**

`[int]` : *Amount of results. [1-10]*

`[string...]` : *Search query.*

**Examples:**

```
!youtube search 5 rick astley
```
</code></details>

---

### youtube searchchannel
<details><summary markdown='span'>Expand for additional information</summary><code>

*Advanced youtube search for channels only.*

**Aliases:**
`sc, searchc`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube searchchannel 5 rick astley
```
</code></details>

---

### youtube searchp
<details><summary markdown='span'>Expand for additional information</summary><code>

*Advanced youtube search for playlists only.*

**Aliases:**
`sp, searchplaylist`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube searchplaylist 5 rick astley
```
</code></details>

---

### youtube searchvideo
<details><summary markdown='span'>Expand for additional information</summary><code>

*Advanced youtube search for videos only.*

**Aliases:**
`sv, searchv`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube searchvideo 5 rick astley
```
</code></details>

---

### youtube subscribe
<details><summary markdown='span'>Expand for additional information</summary><code>

*Add a new subscription for a YouTube channel.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`add, a, +, sub`

**Arguments:**

`[string]` : *Channel URL.*

(optional) `[string]` : *Friendly name.* (def: `None`)

**Examples:**

```
!youtube subscribe https://www.youtube.com/user/RickAstleyVEVO
!youtube subscribe https://www.youtube.com/user/RickAstleyVEVO rick
```
</code></details>

---

### youtube unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><code>

*Remove a YouTube channel subscription.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`

**Arguments:**

`[string]` : *Channel URL or subscription name.*

**Examples:**

```
!youtube unsubscribe https://www.youtube.com/user/RickAstleyVEVO
!youtube unsubscribe rick
```
</code></details>

---

