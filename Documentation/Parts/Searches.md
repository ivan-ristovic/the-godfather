# Module: Searches

## Group: gif
<details><summary markdown='span'>Expand for additional information</summary><p>

*GIPHY commands. If invoked without a subcommand, searches GIPHY with given query.*

**Aliases:**
`giphy`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!gif wat
```
</p></details>

---

### gif random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Return a random GIF.*

**Aliases:**
`r, rand, rnd`

**Examples:**

```
!gif random
```
</p></details>

---

### gif trending
<details><summary markdown='span'>Expand for additional information</summary><p>

*Return an amount of trending GIFs.*

**Aliases:**
`t, tr, trend`

**Arguments:**

(optional) `[int]` : *Number of results (1-10).* (def: `5`)

**Examples:**

```
!gif trending
!gif trending 3
```
</p></details>

---

## Group: goodreads
<details><summary markdown='span'>Expand for additional information</summary><p>

*Goodreads commands. Group call searches Goodreads books with given query.*

**Aliases:**
`gr`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!goodreads Ender's Game
```
</p></details>

---

### goodreads book
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search Goodreads books by title, author, or ISBN.*

**Aliases:**
`books, b`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!goodreads book Ender's Game
```
</p></details>

---

## Group: imdb
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search Open Movie Database. Group call searches by title.*

**Aliases:**
`movies, series, serie, movie, film, cinema, omdb`

**Arguments:**

`[string...]` : *Title.*

**Examples:**

```
!imdb Airplane
```
</p></details>

---

### imdb id
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search by IMDb ID.*

**Arguments:**

`[string]` : *ID.*

**Examples:**

```
!imdb id tt4158110
```
</p></details>

---

### imdb search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches IMDb for given query and returns paginated results.*

**Aliases:**
`s, find`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!imdb search Kill Bill
```
</p></details>

---

### imdb title
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search by title.*

**Aliases:**
`t, name, n`

**Arguments:**

`[string...]` : *Title.*

**Examples:**

```
!imdb title Airplane
```
</p></details>

---

## Group: imgur
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search imgur. Group call retrieves top ranked images from given subreddit.*

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
</p></details>

---

### imgur latest
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

### imgur top
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

## Group: joke
<details><summary markdown='span'>Expand for additional information</summary><p>

*Group for searching jokes. Group call returns a random joke.*

**Aliases:**
`jokes, j`

**Examples:**

```
!joke
```
</p></details>

---

### joke search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search for the joke containing the given query.*

**Aliases:**
`s`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!joke search blonde
```
</p></details>

---

### joke yourmom
<details><summary markdown='span'>Expand for additional information</summary><p>

*Yo mama so...*

**Aliases:**
`mama, m, yomomma, yomom, yomoma, yomamma, yomama`

**Examples:**

```
!joke yourmom
```
</p></details>

---

## Group: reddit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Reddit commands. Group call prints hottest posts from given sub.*

**Aliases:**
`r`

**Arguments:**

(optional) `[string]` : *Subreddit.* (def: `all`)

**Examples:**

```
!reddit aww
```
</p></details>

---

### reddit controversial
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest controversial posts for a subreddit.*

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!reddit controversial aww
```
</p></details>

---

### reddit gilded
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest gilded posts for a subreddit.*

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!reddit gilded aww
```
</p></details>

---

### reddit hot
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest hot posts for a subreddit.*

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!reddit hot aww
```
</p></details>

---

### reddit new
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest posts for a subreddit.*

**Aliases:**
`newest, latest`

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!reddit new aww
```
</p></details>

---

### reddit rising
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest rising posts for a subreddit.*

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!reddit rising aww
```
</p></details>

---

### reddit subscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add new feed for a subreddit.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`add, a, +, sub`

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!reddit sub aww
```
</p></details>

---

### reddit top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get top posts for a subreddit.*

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```
!reddit top aww
```
</p></details>

---

### reddit unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove a subreddit feed using subreddit name or subscription ID (use command ``feed list`` to see IDs).*

**Requires user permissions:**
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
</p></details>

---

## Group: steam
<details><summary markdown='span'>Expand for additional information</summary><p>

*Steam commands. Group call searches steam profiles for a given ID.*

**Aliases:**
`s, st`

**Examples:**

```
!steam profile 123456123
```
</p></details>

---

### steam profile
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get Steam user information for user based on his ID.*

**Aliases:**
`id, user`

**Arguments:**

`[unsigned long]` : *ID.*

</p></details>

---

## Group: urbandict
<details><summary markdown='span'>Expand for additional information</summary><p>

*Urban Dictionary commands. Group call searches Urban Dictionary for a given query.*

**Aliases:**
`ud, urban`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!urbandict blonde
```
</p></details>

---

## Group: weather
<details><summary markdown='span'>Expand for additional information</summary><p>

*Weather search commands. Group call returns weather information for given query.*

**Aliases:**
`w`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```
!weather london
```
</p></details>

---

### weather forecast
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

## Group: xkcd
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search xkcd. Group call returns random comic or, if an ID is provided, a comic with given ID.*

**Aliases:**
`x`

**Overload 1:**

`[int]` : *Comic ID.*

**Examples:**

```
!xkcd
```
</p></details>

---

### xkcd id
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves comic with given ID from xkcd.*

**Arguments:**

(optional) `[int]` : *Comic ID.* (def: `None`)

**Examples:**

```
!xkcd id 650
```
</p></details>

---

### xkcd latest
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves latest comic from xkcd.*

**Aliases:**
`fresh, newest, l`

**Examples:**

```
!xkcd latest
```
</p></details>

---

### xkcd random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves a random comic.*

**Aliases:**
`rnd, r, rand`

**Examples:**

```
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

`[string...]` : *Search query.*

**Examples:**

```
!youtube never gonna give you up
```
</p></details>

---

### youtube search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Advanced youtube search.*

**Aliases:**
`s`

**Arguments:**

`[int]` : *Amount of results. [1-20]*

`[string...]` : *Search query.*

**Examples:**

```
!youtube search 5 rick astley
```
</p></details>

---

### youtube searchchannel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Advanced youtube search for channels only.*

**Aliases:**
`sc, searchc, channel`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube searchchannel 5 rick astley
```
</p></details>

---

### youtube searchp
<details><summary markdown='span'>Expand for additional information</summary><p>

*Advanced youtube search for playlists only.*

**Aliases:**
`sp, searchplaylist, playlist`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube searchplaylist 5 rick astley
```
</p></details>

---

### youtube searchvideo
<details><summary markdown='span'>Expand for additional information</summary><p>

*Advanced youtube search for videos only.*

**Aliases:**
`sv, searchv, video`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```
!youtube searchvideo 5 rick astley
```
</p></details>

---

### youtube subscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a new subscription for a YouTube channel.*

**Requires user permissions:**
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
</p></details>

---

### youtube unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove a YouTube channel subscription.*

**Requires user permissions:**
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
</p></details>

---

