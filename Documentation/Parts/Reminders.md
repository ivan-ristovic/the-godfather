# Module: Reminders
*This module contains reminder management commands.*


## Group: remind
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manage reminders. Group call registers a new reminder after a specified timespan or lists registered reminders in the selected channel or DM.*

**Aliases:**
`reminders, reminder, todo, todolist, note`
**Overload 4:**

`[time span]` : *Timespan until reminder*

`[channel]` : *Channel when to send the reminder*

`[string...]` : *Reminder contents*

**Overload 3:**

`[channel]` : *Channel when to send the reminder*

`[time span]` : *Timespan until reminder*

`[string...]` : *Reminder contents*

**Overload 2:**

`[time span]` : *Timespan until reminder*

`[string...]` : *Reminder contents*

**Overload 1:**

`[channel]` : *Channel to list*

**Examples:**

```xml
!remind
!remind 1d Some important announcement!
!remind 1d #my-text-channel Some important announcement!
```
</p></details>

---

## Group: remind at
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a reminder at the exact specified date and time. Specified date and/or time is relative to guild timezone setting, or UTC time if the command is invoked in DM.*

**Aliases:**
`reminders, reminder, todo, todolist, note`
**Overload 2:**

`[date and time]` : *Localized due date*

`[channel]` : *Channel when to send the reminder*

`[string...]` : *Reminder contents*

**Overload 1:**

`[channel]` : *Channel when to send the reminder*

`[date and time]` : *Localized due date*

`[string...]` : *Reminder contents*

**Overload 0:**

`[date and time]` : *Localized due date*

`[string...]` : *Reminder contents*

**Examples:**

```xml
!remind at 11-10-2020 11:00:03 Some important announcement!
!remind at 11-10-2020 11:00:03 #my-text-channel Some important announcement!
```
</p></details>

---

### remind delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes reminders via ID.*

**Aliases:**
`-, remove, rm, del, -=, >, >>, unschedule`
**Arguments:**

`[int...]` : *ID(s)*

**Examples:**

```xml
!remind delete 12345
```
</p></details>

---

### remind deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes all reminders in a given channel or your personal reminders if the channel is not specified.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Overload 1:**

`[channel]` : *Channel whose reminders to remove*

**Examples:**

```xml
!remind deleteall
!remind deleteall #my-text-channel
```
</p></details>

---

## Group: remind here
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a reminder in the current channel after the specified timespan.*

**Aliases:**
`reminders, reminder, todo, todolist, note`
**Guild only.**

**Overload 1:**

`[time span]` : *Timespan until reminder*

`[string...]` : *Reminder contents*

**Examples:**

```xml
!remind here 1d Some important announcement!
```
</p></details>

---

### remind here at
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a reminder in the current channel at the exact specified date and time. Specified date and/or time is relative to guild timezone setting, or UTC time if the command is invoked in DM.*

**Guild only.**

**Arguments:**

`[date and time]` : *Localized due date*

`[string...]` : *Reminder contents*

**Examples:**

```xml
!remind here at 11-10-2020 11:00:03 Some important announcement!
```
</p></details>

---

### remind here in
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a reminder in the current channel after the specified timespan.*

**Guild only.**

**Arguments:**

`[time span]` : *Timespan until reminder*

`[string...]` : *Reminder contents*

**Examples:**

```xml
!remind here in 1d Some important announcement!
```
</p></details>

---

## Group: remind in
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a reminder in the specified channel after the specified timespan.*

**Aliases:**
`reminders, reminder, todo, todolist, note`
**Overload 2:**

`[time span]` : *Timespan until reminder*

`[channel]` : *Channel when to send the reminder*

`[string...]` : *Reminder contents*

**Overload 1:**

`[channel]` : *Channel when to send the reminder*

`[time span]` : *Timespan until reminder*

`[string...]` : *Reminder contents*

**Overload 0:**

`[time span]` : *Timespan until reminder*

`[string...]` : *Reminder contents*

**Examples:**

```xml
!remind in 1d Some important announcement!
!remind in 1d #my-text-channel Some important announcement!
```
</p></details>

---

### remind list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all active personal reminders or channel reminders.*

**Aliases:**
`print, show, view, ls, l, p`
**Overload 1:**

`[channel]` : *Channel to list*

**Examples:**

```xml
!remind list
!remind list #my-text-channel
```
</p></details>

---

### remind repeat
<details><summary markdown='span'>Expand for additional information</summary><p>

*Registers a repeating reminder.*

**Aliases:**
`newrep, +r, ar, +=r, <r, <<r`
**Overload 2:**

`[time span]` : *Localized due date*

`[channel]` : *Channel when to send the reminder*

`[string...]` : *Reminder contents*

**Overload 1:**

`[channel]` : *Channel when to send the reminder*

`[time span]` : *Localized due date*

`[string...]` : *Reminder contents*

**Overload 0:**

`[time span]` : *Localized due date*

`[string...]` : *Reminder contents*

**Examples:**

```xml
!remind repeat
!remind repeat 1d Some important announcement!
!remind repeat 1d #my-text-channel Some important announcement!
```
</p></details>

---

