# Module: Polls

## Group: poll
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new poll in the current channel. You can provide also the time for the poll to run.*

**Overload 2:**

`[time span]` : *Time for poll to run.*

`[string...]` : *Question.*

**Overload 1:**

`[string]` : *Question.*

`[time span]` : *Time for poll to run.*

**Overload 0:**

`[string...]` : *Question.*

**Examples:**

```
!poll Do you vote for User1 or User2?
!poll 5m Do you vote for User1 or User2?
```
</p></details>

---

### poll stop
<details><summary markdown='span'>Expand for additional information</summary><p>

*Stops a running poll.*

**Requires user permissions:**
`Administrator`

**Examples:**

```
!poll stop
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

## Group: vote
<details><summary markdown='span'>Expand for additional information</summary><p>

*Commands for voting in running polls. If invoked without subcommands, registers a vote in the current poll to the option you entered.*

**Aliases:**
`votefor, vf`

**Arguments:**

`[int]` : *Option to vote for.*

**Examples:**

```
!vote 1
```
</p></details>

---

### vote cancel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Vote for an option in the current running poll.*

**Aliases:**
`c, reset`

**Examples:**

```
!vote cancel
```
</p></details>

---

