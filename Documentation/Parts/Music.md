# Module: Music
*This module contains music playback commands.*


## Group: music
<details><summary markdown='span'>Expand for additional information</summary><p>

*Music playback and queue management commands. Group call prints information about currently playing track.*

**Guild only.**


**Aliases:**
`songs, song, tracks, track, audio, mu`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!music
```
</p></details>

---

### music forward
<details><summary markdown='span'>Expand for additional information</summary><p>

*Forwards the track playback by the specified amount.*

**Guild only.**


**Aliases:**
`fw, f, >, >>`

**Overload 0:**
- \[`time span...`\]: *Forward amount*

**Examples:**

```xml
!music forward 10s
```
</p></details>

---

### music info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints player information.*

**Guild only.**


**Aliases:**
`i, player`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!music info
```
</p></details>

---

### music pause
<details><summary markdown='span'>Expand for additional information</summary><p>

*Toggles playback pause.*

**Guild only.**


**Aliases:**
`ps`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!music pause
```
</p></details>

---

### music play
<details><summary markdown='span'>Expand for additional information</summary><p>

*Play audio from given URL.*

**Guild only.**


**Aliases:**
`p, +, +=, add, a`

**Overload 1:**
- \[`URL`\]: *Audio URL*

**Overload 0:**
- \[`string...`\]: *Search query*

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

**Guild only.**

**Owner-only.**


**Aliases:**
`pf, +f, +=f, addf, af`

**Overload 0:**
- \[`string...`\]: *Audio URL*

**Examples:**

```xml
!music playfile test.mp3
```
</p></details>

---

### music queue
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints the current playback queue.*

**Guild only.**


**Aliases:**
`q, playlist`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!music queue
```
</p></details>

---

### music remove
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes track with given index from the playback queue.*

**Guild only.**


**Aliases:**
`dequeue, delete, rm, del, d, -, -=`

**Overload 0:**
- \[`int`\]: *Index (starting from 1)*

**Examples:**

```xml
!music remove 5
```
</p></details>

---

### music repeat
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sets repeat mode (none, single, all).*

**Guild only.**


**Aliases:**
`loop, l, rep, lp`

**Overload 0:**
- (optional) \[`RepeatMode`\]: *Repeat mode (0 - none, 1 - single, A - all)* (def: `Single`)

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


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!music reshuffle
```
</p></details>

---

### music restart
<details><summary markdown='span'>Expand for additional information</summary><p>

*Restarts the current track.*

**Guild only.**


**Aliases:**
`res, replay`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!music restart
```
</p></details>

---

### music resume
<details><summary markdown='span'>Expand for additional information</summary><p>

*Resumes playback.*

**Guild only.**


**Aliases:**
`unpause, up, rs`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!music resume
```
</p></details>

---

### music rewind
<details><summary markdown='span'>Expand for additional information</summary><p>

*Rewinds the track by the specified amount.*

**Guild only.**


**Aliases:**
`bw, rw, <, <<`

**Overload 0:**
- \[`time span...`\]: *Backward amount*

**Examples:**

```xml
!music rewind 10s
```
</p></details>

---

### music seek
<details><summary markdown='span'>Expand for additional information</summary><p>

*Seeks to a specified point in the track.*

**Guild only.**


**Aliases:**
`s`

**Overload 0:**
- \[`time span...`\]: *Point to seek to*

**Examples:**

```xml
!music seek 01:15
```
</p></details>

---

### music shuffle
<details><summary markdown='span'>Expand for additional information</summary><p>

*Toggles queue shuffling mode.*

**Guild only.**


**Aliases:**
`randomize, rng, sh`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!music shuffle
```
</p></details>

---

### music skip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Skips the current track.*

**Guild only.**


**Aliases:**
`next, n, sk`

**Overload 0:**

*No arguments.*

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


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!music stop
```
</p></details>

---

### music volume
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sets playback volume.*

**Guild only.**


**Aliases:**
`vol, v`

**Overload 0:**
- (optional) \[`int`\]: *Volume to set* (def: `100`)

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

**Guild only.**

**Privileged users only.**


**Aliases:**
`v`

</p></details>

---

### voice connect
<details><summary markdown='span'>Expand for additional information</summary><p>

*Connects the bot to a voice channel.*

**Guild only.**

**Privileged users only.**


**Aliases:**
`c, con, conn`

**Overload 0:**
- (optional) \[`channel...`\]: *Voice channel* (def: `None`)

**Examples:**

```xml
!voice connect My Voice Channel
```
</p></details>

---

### voice disconnect
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disconnects the bot from voice channels.*

**Guild only.**

**Privileged users only.**


**Aliases:**
`d, disconn, dc`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!voice disconnect
```
</p></details>

---

