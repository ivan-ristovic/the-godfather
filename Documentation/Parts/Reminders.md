# Module: Reminders

## Group: remind
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manage reminders.*

**Aliases:**
`reminders, reminder, todo, todolist, note`

**Overload 3:**

`[time span]` : *Time span until reminder.*

`[channel]` : *Channel to send message to.*

`[string...]` : *What to send?*

**Overload 2:**

`[channel]` : *Channel to send message to.*

`[time span]` : *Time span until reminder.*

`[string...]` : *What to send?*

**Overload 1:**

`[time span]` : *Time span until reminder.*

`[string...]` : *What to send?*

**Examples:**

```
!remind 1h Drink water!
```
</p></details>

---

## Group: remind at
<details><summary markdown='span'>Expand for additional information</summary><p>

*Send a reminder at a specific point in time (given by date and time string).*

**Aliases:**
`reminders, reminder, todo, todolist, note`

**Overload 2:**

`[date and time]` : *Date and/or time.*

`[channel]` : *Channel to send message to.*

`[string...]` : *What to send?*

**Overload 1:**

`[channel]` : *Channel to send message to.*

`[date and time]` : *Date and/or time.*

`[string...]` : *What to send?*

**Overload 0:**

`[date and time]` : *Date and/or time.*

`[string...]` : *What to send?*

**Examples:**

```
!remind at 17:20 Drink water!
!remind at 03.15.2019 Drink water!
!remind at "03.15.2019 17:20" Drink water!
```
</p></details>

---

### remind delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unschedules reminders.*

**Aliases:**
`-, remove, rm, del, -=, >, >>, unschedule`

**Arguments:**

`[int...]` : *Reminder ID.*

**Examples:**

```
!remind delete 1
```
</p></details>

---

### remind deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete all your reminders. You can also specify a channel for which to remove reminders.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da`

**Arguments:**

(optional) `[channel]` : *Channel for which to remove reminders.* (def: `None`)

**Examples:**

```
!remind clear
```
</p></details>

---

## Group: remind here
<details><summary markdown='span'>Expand for additional information</summary><p>

*Send a reminder to the current channel after specific time span.*

**Aliases:**
`reminders, reminder, todo, todolist, note`

**Arguments:**

`[time span]` : *Time span until reminder.*

`[string...]` : *What to send?*

**Examples:**

```
!remind here 3h Drink water!
!remind here 3h5m Drink water!
```
</p></details>

---

## Group: remind here at
<details><summary markdown='span'>Expand for additional information</summary><p>

*Send a reminder to the current channel at a specific point in time (given by date and time string).*

**Aliases:**
`reminders, reminder, todo, todolist, note`

**Arguments:**

`[date and time]` : *Date and/or time.*

`[string...]` : *What to send?*

**Examples:**

```
!remind here at 17:20 Drink water!
!remind here at 03.15.2019 Drink water!
!remind here at "03.15.2019 17:20" Drink water!
```
</p></details>

---

## Group: remind here in
<details><summary markdown='span'>Expand for additional information</summary><p>

*Send a reminder to the current channel after specific time span.*

**Aliases:**
`reminders, reminder, todo, todolist, note`

**Arguments:**

`[time span]` : *Time span until reminder.*

`[string...]` : *What to send?*

**Examples:**

```
!remind here in 3h Drink water!
!remind here in 3h5m Drink water!
```
</p></details>

---

## Group: remind in
<details><summary markdown='span'>Expand for additional information</summary><p>

*Send a reminder after specific time span.*

**Aliases:**
`reminders, reminder, todo, todolist, note`

**Overload 2:**

`[time span]` : *Time span until reminder.*

`[channel]` : *Channel to send message to.*

`[string...]` : *What to send?*

**Overload 1:**

`[channel]` : *Channel to send message to.*

`[time span]` : *Time span until reminder.*

`[string...]` : *What to send?*

**Overload 0:**

`[time span]` : *Time span until reminder.*

`[string...]` : *What to send?*

**Examples:**

```
!remind in 3h Drink water!
!remind in 3h5m Drink water!
```
</p></details>

---

### remind list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists your reminders.*

**Aliases:**
`ls`

**Examples:**

```
!remind list
```
</p></details>

---

### remind repeat
<details><summary markdown='span'>Expand for additional information</summary><p>

*Schedule a new repeating reminder. You can also specify a channel where to send the reminder.*

**Aliases:**
`newrep, +r, ar, +=r, <r, <<r`

**Overload 2:**

`[time span]` : *Repeat timespan.*

`[channel]` : *Channel to send message to.*

`[string...]` : *What to send?*

**Overload 1:**

`[channel]` : *Channel to send message to.*

`[time span]` : *Repeat timespan.*

`[string...]` : *What to send?*

**Overload 0:**

`[time span]` : *Repeat timespan.*

`[string...]` : *What to send?*

**Examples:**

```
!remind repeat 1h Drink water!
```
</p></details>

---

