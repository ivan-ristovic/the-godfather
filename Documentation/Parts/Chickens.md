# Module: Chickens
*This module contains chicken management commands. Each user can own a chicken in the guild which he can nurture and train. Upgrade your chicken using multiple items at your disposal to increase chicken strength or vitality. When ready, battle your way through other chicken owners and gain experience and strength until you become the strongest chicken holder in all Discord. Take care though, other users can fight/ambush/poison your chicken, so make sure you are prepared!*


## Group: chicken
<details><summary markdown='span'>Expand for additional information</summary><p>

*Chicken management commands. If invoked without subcommands, prints out your chicken information.*

**Guild only.**


**Aliases:**
`chickens, cock, hen, chick, coc, cc`

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

**Guild only.**


**Aliases:**
`gangattack`

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

**Guild only.**


**Aliases:**
`h, halp, hlp, ha`

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

**Guild only.**


**Aliases:**
`+, compete, enter, j, <, <<`

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

**Guild only.**


**Aliases:**
`b, shop`

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

**Guild only.**


**Aliases:**
`a, extraterrestrial`

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

**Guild only.**


**Aliases:**
`d, def`

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

**Guild only.**


**Aliases:**
`ls, view`

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

**Guild only.**


**Aliases:**
`s, steroid, empowered`

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

**Guild only.**


**Aliases:**
`tr, train`

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

**Guild only.**


**Aliases:**
`wf, fed`

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

**Guild only.**


**Aliases:**
`f, duel, attack`

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

**Guild only.**


**Aliases:**
`+hp, hp`

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

**Guild only.**


**Aliases:**
`information, stats`

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

**Guild only.**


**Aliases:**
`print, show, view, ls, l, p`

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

**Guild only.**


**Aliases:**
`rn, name`

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

**Guild only.**


**Aliases:**
`s`

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

**Guild only.**


**Aliases:**
`best, strongest`

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

**Guild only.**


**Aliases:**
`bestglobally, globallystrongest, globaltop, topg, gtop, globalbest, bestglobal`

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

**Guild only.**


**Aliases:**
`tr, t, exercise`

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

**Guild only.**


**Aliases:**
`str, st, s`

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

**Guild only.**


**Aliases:**
`vit, vi, v`

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

**Guild only.**


**Aliases:**
`perks, upgrades, upg, u`

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

**Guild only.**


**Aliases:**
`print, show, view, ls, l, p`

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

**Guild only.**


**Aliases:**
`gangwar, battle`

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

**Guild only.**


**Aliases:**
`+, compete, enter, j, <, <<`

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

