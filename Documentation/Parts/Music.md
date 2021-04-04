# Module: Music
*This module contains music playback commands.*


## Group: music
<details><summary markdown='span'>Expand for additional information</summary><p>

*Music playback and queue management commands. Group call prints information about currently playing track.*

**Aliases:**
`songs, song, tracks, track, audio, mu`
**Guild only.**

**Examples:**

```xml
!music
```
</p></details>

---

### music forward
<details><summary markdown='span'>Expand for additional information</summary><p>

*Forwards the track playback by the specified amount.*

**Aliases:**
`fw, f, >, >>`
**Guild only.**

**Arguments:**

[`time span...`]: *Forward amount*

**Examples:**

```xml
!music forward 10s
```
</p></details>

---

### music info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints player information.*

**Aliases:**
`i, player`
**Guild only.**

**Examples:**

```xml
!music info
```
</p></details>

---

### music pause
<details><summary markdown='span'>Expand for additional information</summary><p>

*Toggles playback pause.*

**Aliases:**
`ps`
**Guild only.**

**Examples:**

```xml
!music pause
```
</p></details>

---

### music play
<details><summary markdown='span'>Expand for additional information</summary><p>

*Play audio from given URL.*

**Aliases:**
`p, +, +=, add, a`
**Guild only.**

**Overload 1:**

[`URL`]: *Audio URL*

**Overload 0:**

[`string...`]: *Search query*

**Examples:**

```xml
!music play https://www.youtube.com/watch?v=dQw4w9WgXcQ
!music play Search query
```
</p></details>

---

### music playfile
<details><summary markdown='span'>Expand for additional information</summary><p>

*Play local audio file.*

**Aliases:**
`pf, +f, +=f, addf, af`
**Guild only.**

**Owner-only.**

**Arguments:**

[`string...`]: *Audio URL*

**Examples:**

```xml
!music playfile test.mp3
```
</p></details>

---

### music queue
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints the current playback queue.*

**Aliases:**
`q, playlist`
**Guild only.**

**Examples:**

```xml
!music queue
```
</p></details>

---

### music remove
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes track with given index from the playback queue.*

**Aliases:**
`dequeue, delete, rm, del, d, -, -=`
**Guild only.**

**Arguments:**

[`int`]: *Index (starting from 1)*

**Examples:**

```xml
!music remove 5
```
</p></details>

---

### music repeat
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sets repeat mode (none, single, all).*

**Aliases:**
`loop, l, rep, lp`
**Guild only.**

**Arguments:**

(optional) [`RepeatMode`]: *Repeat mode (0 - none, 1 - single, A - all)* (def: `Single`)

**Examples:**

```xml
!music repeat All
```
</p></details>

---

### music reshuffle
<details><summary markdown='span'>Expand for additional information</summary><p>

*Reshuffles the queue without enabling shuffle mode.*

**Guild only.**

**Examples:**

```xml
!music reshuffle
```
</p></details>

---

### music restart
<details><summary markdown='span'>Expand for additional information</summary><p>

*Restarts the current track.*

**Aliases:**
`res, replay`
**Guild only.**

**Examples:**

```xml
!music restart
```
</p></details>

---

### music resume
<details><summary markdown='span'>Expand for additional information</summary><p>

*Resumes playback.*

**Aliases:**
`unpause, up, rs`
**Guild only.**

**Examples:**

```xml
!music resume
```
</p></details>

---

### music rewind
<details><summary markdown='span'>Expand for additional information</summary><p>

*Rewinds the track by the specified amount.*

**Aliases:**
`bw, rw, <, <<`
**Guild only.**

**Arguments:**

[`time span...`]: *Backward amount*

**Examples:**

```xml
!music rewind 10s
```
</p></details>

---

### music seek
<details><summary markdown='span'>Expand for additional information</summary><p>

*Seeks to a specified point in the track.*

**Aliases:**
`s`
**Guild only.**

**Arguments:**

[`time span...`]: *Point to seek to*

**Examples:**

```xml
!music seek 01:15
```
</p></details>

---

### music shuffle
<details><summary markdown='span'>Expand for additional information</summary><p>

*Toggles queue shuffling mode.*

**Aliases:**
`randomize, rng, sh`
**Guild only.**

**Examples:**

```xml
!music shuffle
```
</p></details>

---

### music skip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Skips the current track.*

**Aliases:**
`next, n, sk`
**Guild only.**

**Examples:**

```xml
!music skip
```
</p></details>

---

### music stop
<details><summary markdown='span'>Expand for additional information</summary><p>

*Stops the playback, empties the queue and leaves the voice channel.*

**Guild only.**

**Examples:**

```xml
!music stop
```
</p></details>

---

### music volume
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sets playback volume.*

**Aliases:**
`vol, v`
**Guild only.**

**Arguments:**

(optional) [`int`]: *Volume to set* (def: `100`)

**Examples:**

```xml
!music volume 50
```
</p></details>

---

## Group: voice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*Voice channel bot commands.*

**Aliases:**
`v`
**Guild only.**

**Privileged users only.**


</p></details>

---

### voice connect
<details><summary markdown='span'>Expand for additional information</summary><p>

*Connects the bot to a voice channel.*

**Aliases:**
`c, con, conn`
**Guild only.**

**Privileged users only.**


**Arguments:**

(optional) [`channel...`]: *Voice channel* (def: `None`)

**Examples:**

```xml
!voice connect My Voice Channel
```
</p></details>

---

### voice disconnect
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disconnects the bot from voice channels.*

**Aliases:**
`d, disconn, dc`
**Guild only.**

**Privileged users only.**


**Examples:**

```xml
!voice disconnect
```
</p></details>

---

