# Module: Reactions

## Group: emojireaction
<details><summary markdown='span'>Expand for additional information</summary><p>

*Orders a bot to react with given emoji to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new emoji reaction to a given trigger word list. Note: Trigger words can be regular expressions (use ``emojireaction addregex`` command).*

**Requires permissions:**
`Manage guild`

**Aliases:**
`ereact, er, emojir, emojireactions`

**Overload 1:**

`[emoji]` : *Emoji to send.*

`[string...]` : *Trigger word list.*

**Overload 0:**

`[string]` : *Trigger word (case-insensitive).*

`[emoji]` : *Emoji to send.*

**Examples:**

```
!emojireaction :smile: haha laughing
```
</p></details>

---

### emojireaction add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add emoji reaction to guild reaction list.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`+, new, a, +=, <, <<`

**Overload 1:**

`[emoji]` : *Emoji to send.*

`[string...]` : *Trigger word list (case-insensitive).*

**Overload 0:**

`[string]` : *Trigger word (case-insensitive).*

`[emoji]` : *Emoji to send.*

**Examples:**

```
!emojireaction add :smile: haha
!emojireaction add haha :smile:
```
</p></details>

---

### emojireaction addregex
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add emoji reaction triggered by a regex to guild reaction list.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`+r, +regex, +regexp, +rgx, newregex, addrgx, +=r, <r, <<r`

**Overload 1:**

`[emoji]` : *Emoji to send.*

`[string...]` : *Trigger word list (case-insensitive).*

**Overload 0:**

`[string]` : *Trigger word (case-insensitive).*

`[emoji]` : *Emoji to send.*

**Examples:**

```
!emojireaction addregex :smile: (ha)+
!emojireaction addregex (ha)+ :smile:
```
</p></details>

---

### emojireaction delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove emoji reactions for given trigger words.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`-, remove, del, rm, d, -=, >, >>`

**Overload 2:**

`[emoji]` : *Emoji to remove reactions for.*

**Overload 1:**

`[int...]` : *IDs of the reactions to remove.*

**Overload 0:**

`[string...]` : *Trigger words to remove.*

**Examples:**

```
!emojireaction delete haha sometrigger
!emojireaction delete 5
!emojireaction delete 5 4
!emojireaction delete :joy:
```
</p></details>

---

### emojireaction deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete all reactions for the current guild.*

**Requires permissions:**
`Manage guild`

**Requires user permissions:**
`Administrator`

**Aliases:**
`clear, da, c, ca, cl, clearall, >>>`

**Examples:**

```
!emojireactions clear
```
</p></details>

---

### emojireaction find
<details><summary markdown='span'>Expand for additional information</summary><p>

*Show all emoji reactions that matches the specified trigger.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`f`

**Arguments:**

`[string...]` : *Specific trigger.*

**Examples:**

```
!emojireactions find hello
```
</p></details>

---

### emojireaction list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Show all emoji reactions for this guild.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`ls, l, print`

**Examples:**

```
!emojireaction list
```
</p></details>

---

## Group: textreaction
<details><summary markdown='span'>Expand for additional information</summary><p>

*Orders a bot to react with given text to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new text reaction to a given trigger word. Note: Trigger words can be regular expressions (use ``textreaction addregex`` command). You can also use "%user%" inside response and the bot will replace it with mention for the user who triggers the reaction. Text reactions have a one minute cooldown.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`treact, tr, txtr, textreactions`

**Overload 0:**

`[string]` : *Trigger string (case insensitive).*

`[string...]` : *Response.*

**Examples:**

```
!textreaction hi hello
!textreaction "hi" "Hello, %user%!"
```
</p></details>

---

### textreaction add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a new text reaction to guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new, a, +=, <, <<`

**Arguments:**

`[string]` : *Trigger string (case insensitive).*

`[string...]` : *Response.*

**Examples:**

```
!textreaction add "hi" "Hello, %user%!"
```
</p></details>

---

### textreaction addregex
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a new text reaction triggered by a regex to guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+r, +regex, +regexp, +rgx, newregex, addrgx, +=r, <r, <<r`

**Arguments:**

`[string]` : *Regex (case insensitive).*

`[string...]` : *Response.*

**Examples:**

```
!textreaction addregex "h(i|ey|ello|owdy)" "Hello, %user%!"
```
</p></details>

---

### textreaction delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove text reaction from guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, del, rm, d, -=, >, >>`

**Overload 1:**

`[int...]` : *IDs of the reactions to remove.*

**Overload 0:**

`[string...]` : *Trigger words to remove.*

**Examples:**

```
!textreaction delete 5
!textreaction delete 5 8
!textreaction delete hi
```
</p></details>

---

### textreaction deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete all text reactions for the current guild.*

**Requires user permissions:**
`Administrator, Manage guild`

**Aliases:**
`clear, da, c, ca, cl, clearall, >>>`

**Examples:**

```
!textreactions clear
```
</p></details>

---

### textreaction find
<details><summary markdown='span'>Expand for additional information</summary><p>

*Show a text reactions that matches the specified trigger.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`f`

**Arguments:**

`[string...]` : *Specific trigger.*

**Examples:**

```
!textreactions find hello
```
</p></details>

---

### textreaction list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Show all text reactions for the guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ls, l, print`

**Examples:**

```
!textreactions list
```
</p></details>

---

