# Module: Polls
*This module contains polling commands through textual or reaction polls.*


## Group: poll
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new poll in the current channel. You can also provide the time for the poll to run.*

**Guild only.**


**Aliases:**
`polls`

**Overload 2:**
- \[`time span`\]: *Time for the poll to run*
- \[`string...`\]: *Poll question*

**Overload 1:**
- \[`string`\]: *Poll question*
- \[`time span`\]: *Time for the poll to run*

**Overload 0:**
- \[`string...`\]: *Poll question*

**Examples:**

```xml
!poll Some poll question?
!poll 10s Some poll question?
!poll Some poll question? 10s
```
</p></details>

---

### poll stop
<details><summary markdown='span'>Expand for additional information</summary><p>

*Stops a running poll.*

**Guild only.**


**Aliases:**
`end, cancel`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!poll stop
```
</p></details>

---

## Group: reactionspoll
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new reactions poll in the current channel. You can also provide the time for the poll to run.*

**Guild only.**


**Aliases:**
`reactionspolls, rpoll, rpolls, pollr, voter`

**Overload 2:**
- \[`time span`\]: *Time for the poll to run*
- \[`string...`\]: *Poll question*

**Overload 1:**
- \[`string`\]: *Poll question*
- \[`time span`\]: *Time for the poll to run*

**Overload 0:**
- \[`string...`\]: *Poll question*

**Examples:**

```xml
!reactionspoll Some poll question?
!reactionspoll 10s Some poll question?
!reactionspoll Some poll question? 10s
```
</p></details>

---

### reactionspoll stop
<details><summary markdown='span'>Expand for additional information</summary><p>

*Stops a running reactions poll.*

**Guild only.**


**Aliases:**
`end, cancel`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!reactionspoll stop
```
</p></details>

---

## Group: vote
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manages voting in running polls. Group call registers a vote in the running poll for the option you entered.*

**Guild only.**


**Aliases:**
`votefor, vf`

**Overload 0:**
- \[`int`\]: *Option to vote for*

**Examples:**

```xml
!vote 5
```
</p></details>

---

### vote cancel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manages voting in running polls. Group call registers a vote in the running poll for the option you entered.*

**Guild only.**


**Aliases:**
`c, reset`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!vote cancel
```
</p></details>

---

