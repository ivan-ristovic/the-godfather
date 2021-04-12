# Module: Reactions
*This module contains bot reaction commands through replies or emoji reactions.*


## Group: emojireaction
<details><summary markdown='span'>Expand for additional information</summary><p>

*This groups is used to make the bot react with given emoji to a message containing a trigger word inside (guild specific). Group call either lists currently registered emoji reactions or adds a new emoji reaction to a given trigger word list.
*Note: Trigger words can be regular expressions (use `emojireaction addregex` command).**

**Aliases:**
`ereact, er, emojir, emojireactions`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 2:**

*No arguments.*

**Overload 1:**
- [`emoji`]: *Emoji*
- [`string...`]: *Reaction triggers*

**Overload 0:**
- [`string`]: *Reaction triggers*
- [`emoji`]: *Emoji*

**Examples:**

```xml
!emojireaction
!emojireaction :emoji: triggerword
!emojireaction triggerword :emoji:
```
</p></details>

---

### emojireaction add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new emoji reaction to guild reaction list. Requires emoji to react as and one or more triggers.*

**Aliases:**
`register, reg, new, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 1:**
- [`emoji`]: *Emoji*
- [`string...`]: *Reaction triggers*

**Overload 0:**
- [`string`]: *Reaction triggers*
- [`emoji`]: *Emoji*

**Examples:**

```xml
!emojireaction add :emoji: triggerword
!emojireaction add triggerword :emoji:
```
</p></details>

---

### emojireaction addregex
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new emoji reaction to guild reaction list. Requires emoji to react as and one or more regular expressions that will match the message being tested (each regular expression is word-bounded automatically).*

**Aliases:**
`registerregex, regex, newregex, ar, +r, +=r, <<r, <r, <-r, <=r, +regex, +regexp, +rgx`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 1:**
- [`emoji`]: *Emoji*
- [`string...`]: *Reaction triggers*

**Overload 0:**
- [`string`]: *Reaction triggers*
- [`emoji`]: *Emoji*

**Examples:**

```xml
!emojireaction addregex :emoji: regex?pattern+
!emojireaction addregex regex?pattern+ :emoji:
```
</p></details>

---

### emojireaction delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes existing emoji reaction by it's reaction emoji, ID or trigger.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 2:**
- [`emoji`]: *Emoji*

**Overload 1:**
- [`int...`]: *Reaction IDs to remove*

**Overload 0:**
- [`string...`]: *Reaction triggers*

**Examples:**

```xml
!emojireaction delete 12345
!emojireaction delete :emoji:
!emojireaction delete triggerword
```
</p></details>

---

### emojireaction deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all guild emoji reactions.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires permissions:**
`Manage guild`
**Requires user permissions:**
`Administrator`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!emojireaction deleteall
```
</p></details>

---

### emojireaction find
<details><summary markdown='span'>Expand for additional information</summary><p>

*Finds emoji reactions that are triggered by a given text.*

**Aliases:**
`f, test`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 0:**
- [`string...`]: *Reaction trigger*

**Examples:**

```xml
!emojireaction find triggerword
```
</p></details>

---

### emojireaction list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all emoji reactions for this guild.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!emojireaction list
```
</p></details>

---

## Group: textreaction
<details><summary markdown='span'>Expand for additional information</summary><p>

*This groups is used to make the bot react with given text to a message containing a trigger word inside (guild specific). Group call either lists currently registered text reactions or adds a new text reaction to a given trigger word list.
*Note: Trigger words can be regular expressions (use `textreaction addregex` command).**

**Aliases:**
`treact, tr, txtr, textreactions`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**

*No arguments.*

**Overload 0:**
- [`string`]: *Reaction trigger*
- [`string...`]: *Response*

**Examples:**

```xml
!textreaction
!textreaction triggerword Response message
```
</p></details>

---

### textreaction add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new text reaction to guild reaction list. Requires trigger word and a response.*

**Aliases:**
`register, reg, new, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`string`]: *Reaction trigger*
- [`string...`]: *Response*

**Examples:**

```xml
!textreaction add triggerword Response message
```
</p></details>

---

### textreaction addregex
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new text reaction to guild reaction list. Requires a regular expression that will match the message being tested (each regular expression is word-bounded automatically) and a text to reply when the match occurs.*

**Aliases:**
`registerregex, regex, newregex, ar, +r, +=r, <<r, <r, <-r, <=r, +regex, +regexp, +rgx`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`string`]: *Reaction trigger*
- [`string...`]: *Response*

**Examples:**

```xml
!textreaction addregex regex?pattern+ Response message
```
</p></details>

---

### textreaction delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes existing text reaction by it's ID or trigger word.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**
- [`int...`]: *Reaction IDs to remove*

**Overload 0:**
- [`string...`]: *Reaction triggers*

**Examples:**

```xml
!textreaction delete 12345
!textreaction delete triggerword
```
</p></details>

---

### textreaction deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all guild text reactions.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`Administrator, Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!textreaction deleteall
```
</p></details>

---

### textreaction find
<details><summary markdown='span'>Expand for additional information</summary><p>

*Finds text reactions that are triggered by a given text.*

**Aliases:**
`f, test`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**
- [`string...`]: *Reaction trigger*

**Examples:**

```xml
!textreaction find triggerword
```
</p></details>

---

### textreaction list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all emoji reactions for this guild.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!textreaction list
```
</p></details>

---

