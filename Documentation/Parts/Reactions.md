# Module: Reactions

## Group: emojireaction
*Orders a bot to react with given emoji to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new emoji reaction to a given trigger word list. Note: Trigger words can be regular expressions (use ``emojireaction addregex`` command).*

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
---

### emojireaction add
*Add emoji reaction to guild reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new, a`

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
---

### emojireaction addregex
*Add emoji reaction triggered by a regex to guild reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+r, +regex, +regexp, +rgx, newregex, addrgx`

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
---

### emojireaction clear
*Delete all reactions for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c, ca, cl, clearall`

**Examples:**

```
!emojireactions clear
```
---

### emojireaction delete
*Remove emoji reactions for given trigger words.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, del, rm, d`

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
---

### emojireaction list
*Show all emoji reactions for this guild.*

**Aliases:**
`ls, l, view`

**Examples:**

```
!emojireaction list
```
---

## Group: textreaction
*Orders a bot to react with given text to a message containing a trigger word inside (guild specific). If invoked without subcommands, adds a new text reaction to a given trigger word. Note: Trigger words can be regular expressions (use ``textreaction addregex`` command). You can also use "%user%" inside response and the bot will replace it with mention for the user who triggers the reaction.*

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
---

### textreaction add
*Add a new text reaction to guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, new, a`

**Arguments:**

`[string]` : *Trigger string (case insensitive).*

`[string...]` : *Response.*

**Examples:**

```
!textreaction add "hi" "Hello, %user%!"
```
---

### textreaction addregex
*Add a new text reaction triggered by a regex to guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+r, +regex, +regexp, +rgx, newregex, addrgx`

**Arguments:**

`[string]` : *Regex (case insensitive).*

`[string...]` : *Response.*

**Examples:**

```
!textreaction addregex "h(i|ey|ello|owdy)" "Hello, %user%!"
```
---

### textreaction clear
*Delete all text reactions for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`da, c, ca, cl, clearall`

**Examples:**

```
!textreactions clear
```
---

### textreaction delete
*Remove text reaction from guild text reaction list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, del, rm, d`

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
---

### textreaction list
*Show all text reactions for the guild.*

**Aliases:**
`ls, l, view`

**Examples:**

```
!textreactions list
```
---

