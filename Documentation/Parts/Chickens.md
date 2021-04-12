# Module: Chickens
*This module contains chicken management commands. Each user can own a chicken in the guild which he can nurture and train. Upgrade your chicken using multiple items at your disposal to increase chicken strength or vitality. When ready, battle your way through other chicken owners and gain experience and strength until you become the strongest chicken holder in all Discord. Take care though, other users can fight/ambush/poison your chicken, so make sure you are prepared!*


## Group: chicken
<details><summary markdown='span'>Expand for additional information</summary><p>

*Chicken management commands. If invoked without subcommands, prints out your chicken information.*

**Aliases:**
`chickens, cock, hen, chick, coc, cc`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 1:**
- \[`string`\]: *Chicken name*

**Examples:**

```xml
!chicken
!chicken Member
!chicken SampleName
```
</p></details>

---

## Group: chicken ambush
<details><summary markdown='span'>Expand for additional information</summary><p>

*Start an ambush for another user's chicken. Other users can either help with the ambush or help the ambushed chicken.*

**Aliases:**
`gangattack`
**Guild only.**


**Overload 1:**
- \[`member`\]: *Member*

**Overload 0:**
- \[`string`\]: *Chicken name*

**Examples:**

```xml
!chicken ambush Member
!chicken ambush SampleName
```
</p></details>

---

### chicken ambush help
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending chicken ambush and help the ambushed chicken.*

**Aliases:**
`h, halp, hlp, ha`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken ambush help
```
</p></details>

---

### chicken ambush join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending chicken ambush as one of the ambushers.*

**Aliases:**
`+, compete, enter, j, <, <<`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken ambush join
```
</p></details>

---

## Group: chicken buy
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buys a chicken! Group call buys the cheapest chicken type. To list all available chicken types, use command `chicken buy list`.*

**Aliases:**
`b, shop`
**Guild only.**


**Overload 0:**
- \[`string...`\]: *Chicken name*

**Examples:**

```xml
!chicken buy SampleName
```
</p></details>

---

### chicken buy alien
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buys a chicken from another planet far, far away.*

**Aliases:**
`a, extraterrestrial`
**Guild only.**


**Overload 0:**
- \[`string...`\]: *Chicken name*

**Examples:**

```xml
!chicken buy alien SampleName
```
</p></details>

---

### chicken buy default
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buys a default chicken.*

**Aliases:**
`d, def`
**Guild only.**


**Overload 0:**
- \[`string...`\]: *Chicken name*

**Examples:**

```xml
!chicken buy default SampleName
```
</p></details>

---

### chicken buy list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all chicken types available for purchase.*

**Aliases:**
`ls, view`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken buy list
```
</p></details>

---

### chicken buy steroidempowered
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buys a chicken that was fed with steroids since the day it was born.*

**Aliases:**
`s, steroid, empowered`
**Guild only.**


**Overload 0:**
- \[`string...`\]: *Chicken name*

**Examples:**

```xml
!chicken buy steroidempowered SampleName
```
</p></details>

---

### chicken buy trained
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buys a chicken trained by Jackie Chan.*

**Aliases:**
`tr, train`
**Guild only.**


**Overload 0:**
- \[`string...`\]: *Chicken name*

**Examples:**

```xml
!chicken buy trained SampleName
```
</p></details>

---

### chicken buy wellfed
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buys a well-fed chicken.*

**Aliases:**
`wf, fed`
**Guild only.**


**Overload 0:**
- \[`string...`\]: *Chicken name*

**Examples:**

```xml
!chicken buy wellfed SampleName
```
</p></details>

---

### chicken fight
<details><summary markdown='span'>Expand for additional information</summary><p>

*Make your chicken fight another member's chicken.*

**Aliases:**
`f, duel, attack`
**Guild only.**


**Overload 1:**
- \[`member`\]: *Member*

**Overload 0:**
- \[`string`\]: *Chicken name*

**Examples:**

```xml
!chicken fight Member
!chicken fight SampleName
```
</p></details>

---

### chicken heal
<details><summary markdown='span'>Expand for additional information</summary><p>

*Heals your chicken. There is one medicine made per certain time period, so you need to grab it before the others do!*

**Aliases:**
`+hp, hp`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken heal
```
</p></details>

---

### chicken info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints chicken information.*

**Aliases:**
`information, stats`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 1:**
- \[`string`\]: *Chicken name*

**Examples:**

```xml
!chicken info
!chicken info Member
!chicken info SampleName
```
</p></details>

---

### chicken list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all chickens in this guild.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken list
```
</p></details>

---

### chicken rename
<details><summary markdown='span'>Expand for additional information</summary><p>

*Renames your chicken.*

**Aliases:**
`rn, name`
**Guild only.**


**Overload 0:**
- \[`string...`\]: *New name*

**Examples:**

```xml
!chicken rename SampleName
```
</p></details>

---

### chicken sell
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sells your chicken.*

**Aliases:**
`s`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken sell
```
</p></details>

---

### chicken top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows all strongest chickens in this guild.*

**Aliases:**
`best, strongest`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken top
```
</p></details>

---

### chicken topglobal
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows the strongest chickens in the world.*

**Aliases:**
`bestglobally, globallystrongest, globaltop, topg, gtop, globalbest, bestglobal`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken topglobal
```
</p></details>

---

## Group: chicken train
<details><summary markdown='span'>Expand for additional information</summary><p>

*Trains your chicken at the cost of some guild currency. Group call trains your chicken's strength.*

**Aliases:**
`tr, t, exercise`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken train
```
</p></details>

---

### chicken train strength
<details><summary markdown='span'>Expand for additional information</summary><p>

*Trains your chicken's strength at the cost of some guild currency.*

**Aliases:**
`str, st, s`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken train strength
```
</p></details>

---

### chicken train vitality
<details><summary markdown='span'>Expand for additional information</summary><p>

*Trains your chicken's vitality at the cost of some guild currency.*

**Aliases:**
`vit, vi, v`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken train vitality
```
</p></details>

---

## Group: chicken upgrade
<details><summary markdown='span'>Expand for additional information</summary><p>

*Upgrade your chicken with items you can buy using guild currency. Group call lists all available upgrades or buys an upgrade with specified ID.*

**Aliases:**
`perks, upgrades, upg, u`
**Guild only.**


**Overload 1:**

*No arguments.*

**Overload 0:**
- \[`int...`\]: *Chicken upgrade ID(s) to buy*

**Examples:**

```xml
!chicken upgrade 5 10
```
</p></details>

---

### chicken upgrade list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all available chicken upgrades.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!chicken upgrade list
```
</p></details>

---

## Group: chicken war
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a chicken war! Users can make their chickens join one of the teams.*

**Aliases:**
`gangwar, battle`
**Guild only.**


**Overload 0:**
- (optional) \[`string`\]: *Team 1 name* (def: `None`)
- (optional) \[`string`\]: *Team 2 name* (def: `None`)

**Examples:**

```xml
!chicken war
!chicken war SampleName SampleName
```
</p></details>

---

### chicken war join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Joins a specified team (via name or number) in a pending chicken war.*

**Aliases:**
`+, compete, enter, j, <, <<`
**Guild only.**


**Overload 1:**
- \[`int`\]: *Team number to join*

**Overload 0:**
- \[`string...`\]: *Team name to join*

**Examples:**

```xml
!chicken war join 1
!chicken war join SampleName
```
</p></details>

---

