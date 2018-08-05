# Module: Reactions

## Group: textreaction
<details><summary markdown='span'>Expand for additional information</summary><p>

*Orders a bot to react with given text to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new text reaction to a given trigger word. Note: Trigger words can be regular expressions (use ``textreaction addregex`` command). You can also use "%user%" inside response and the bot will replace it with mention for the user who triggers the reaction. Text reactions have a one minute cooldown.*

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
`Administrator`

**Aliases:**
`clear, da, c, ca, cl, clearall, >>>`

**Examples:**

```
!textreactions clear
```
</p></details>

---

### textreaction list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Show all text reactions for the guild.*

**Aliases:**
`ls, l, print`

**Examples:**

```
!textreactions list
```
</p></details>

---

