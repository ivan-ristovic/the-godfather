# Module: Chickens

## Group: chicken
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manage your chicken. If invoked without subcommands, prints out your chicken information.*

**Aliases:**
`cock, hen, chick, coc, cc`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!chicken
!chicken @Someone
```
</p></details>

---

## Group: chicken ambush
<details><summary markdown='span'>Expand for additional information</summary><p>

*Start an ambush for another user's chicken. Other users can put their chickens into your ambush and collectively attack the target chicken combining their strength.*

**Aliases:**
`gangattack`

**Arguments:**

(optional) `[user]` : *Whose chicken to ambush.* (def: `None`)

**Examples:**

```
!chicken ambush @Someone
```
</p></details>

---

### chicken ambush help
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending chicken ambush and help the ambushed chicken.*

**Aliases:**
`h, halp, hlp, ha`

**Examples:**

```
!chicken ambush help
```
</p></details>

---

### chicken ambush join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending chicken ambush as one of the ambushers.*

**Aliases:**
`+, compete, enter, j`

**Examples:**

```
!chicken ambush join
```
</p></details>

---

## Group: chicken buy
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a new chicken in this guild using your credits from WM bank.*

**Aliases:**
`b, shop`

**Arguments:**

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken buy My Chicken Name
```
</p></details>

---

### chicken buy alien
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy an alien chicken.*

**Aliases:**
`a, extraterrestrial`

**Arguments:**

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken buy alien My Chicken Name
```
</p></details>

---

### chicken buy default
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a chicken of default strength (cheapest).*

**Aliases:**
`d, def`

**Arguments:**

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken buy default My Chicken Name
```
</p></details>

---

### chicken buy list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all available chicken types.*

**Aliases:**
`ls, view`

**Examples:**

```
!chicken buy list
```
</p></details>

---

### chicken buy steroidempowered
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a steroid-empowered chicken.*

**Aliases:**
`steroid, empowered`

**Arguments:**

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken buy steroidempowered My Chicken Name
```
</p></details>

---

### chicken buy trained
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a trained chicken.*

**Aliases:**
`wf, fed`

**Arguments:**

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken buy trained My Chicken Name
```
</p></details>

---

### chicken buy wellfed
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a well-fed chicken.*

**Aliases:**
`wf, fed`

**Arguments:**

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken buy wellfed My Chicken Name
```
</p></details>

---

### chicken fight
<details><summary markdown='span'>Expand for additional information</summary><p>

*Make your chicken and another user's chicken fight eachother!*

**Aliases:**
`f, duel, attack`

**Arguments:**

`[user]` : *User.*

**Examples:**

```
!chicken duel @Someone
```
</p></details>

---

### chicken flu
<details><summary markdown='span'>Expand for additional information</summary><p>

*Pay a well-known scientist to create a disease that disintegrates weak chickens.*

**Aliases:**
`cancer, disease, blackdeath`

**Examples:**

```
!chicken flu
```
</p></details>

---

### chicken heal
<details><summary markdown='span'>Expand for additional information</summary><p>

*Heal your chicken (+100 HP). There is one medicine made each 10 minutes, so you need to grab it before the others do!*

**Aliases:**
`+hp, hp`

**Examples:**

```
!chicken heal
```
</p></details>

---

### chicken info
<details><summary markdown='span'>Expand for additional information</summary><p>

*View user's chicken info. If the user is not given, views sender's chicken info.*

**Aliases:**
`information, stats`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!chicken info @Someone
```
</p></details>

---

### chicken rename
<details><summary markdown='span'>Expand for additional information</summary><p>

*Rename your chicken.*

**Aliases:**
`rn, name`

**Arguments:**

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken name New Name
```
</p></details>

---

### chicken sell
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sell your chicken.*

**Aliases:**
`s`

**Examples:**

```
!chicken sell
```
</p></details>

---

### chicken top
<details><summary markdown='span'>Expand for additional information</summary><p>

*View the list of strongest chickens in the current guild.*

**Aliases:**
`best, strongest`

**Examples:**

```
!chicken top
```
</p></details>

---

### chicken topglobal
<details><summary markdown='span'>Expand for additional information</summary><p>

*View the list of strongest chickens globally.*

**Aliases:**
`bestglobally, globallystrongest, globaltop, topg, gtop`

**Examples:**

```
!chicken topglobal
```
</p></details>

---

## Group: chicken train
<details><summary markdown='span'>Expand for additional information</summary><p>

*Train your chicken using your credits from WM bank.*

**Aliases:**
`tr, t, exercise`

**Examples:**

```
!chicken train
```
</p></details>

---

### chicken train strength
<details><summary markdown='span'>Expand for additional information</summary><p>

*Train your chicken's strength using your credits from WM bank.*

**Aliases:**
`str, st, s`

**Examples:**

```
!chicken train strength
```
</p></details>

---

### chicken train vitality
<details><summary markdown='span'>Expand for additional information</summary><p>

*Train your chicken's vitality using your credits from WM bank.*

**Aliases:**
`vit, vi, v`

**Examples:**

```
!chicken train vitality
```
</p></details>

---

## Group: chicken upgrades
<details><summary markdown='span'>Expand for additional information</summary><p>

*Upgrade your chicken with items you can buy using your credits from WM bank. Invoking the group lists all upgrades available.*

**Aliases:**
`perks, upgrade, u`

**Overload 1:**

`[int]` : *ID of the upgrade to buy.*

**Examples:**

```
!chicken upgrade
```
</p></details>

---

### chicken upgrades list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all available upgrades.*

**Aliases:**
`ls, view`

**Examples:**

```
!chicken upgrade list
```
</p></details>

---

## Group: chicken war
<details><summary markdown='span'>Expand for additional information</summary><p>

*Declare a chicken war! Other users can put their chickens into teams which names you specify.*

**Aliases:**
`gangwar, battle`

**Arguments:**

(optional) `[string]` : *Team 1 name.* (def: `None`)

(optional) `[string]` : *Team 2 name.* (def: `None`)

**Examples:**

```
!chicken war Team1 Team2
!chicken war "Team 1 name" "Team 2 name
```
</p></details>

---

### chicken war join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending chicken war. Specify a team which you want to join, or numbers 1 or 2 corresponding to team one and team two, respectively.*

**Aliases:**
`+, compete, enter, j`

**Overload 1:**

`[int]` : *Number 1 or 2 depending of team you wish to join.*

**Overload 0:**

`[string...]` : *Team name to join.*

**Examples:**

```
!chicken war join Team Name
```
</p></details>

---

