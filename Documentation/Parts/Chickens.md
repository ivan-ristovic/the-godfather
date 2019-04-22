# Module: Chickens

## Group: chicken
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manage your chicken. If invoked without subcommands, prints out your chicken information.*

**Aliases:**
`cock, hen, chick, coc, cc`

**Arguments:**

(optional) `[member]` : *User.* (def: `None`)

**Examples:**

```xml
!chicken 
!chicken @Someone
```
</p></details>

---

## Group: chicken ambush
<details><summary markdown='span'>Expand for additional information</summary><p>

*Start an ambush for another user's chicken. Other users can either help with the ambush or help the ambushed chicken.*

**Aliases:**
`gangattack`

**Overload 1:**

`[member]` : *Whose chicken to ambush?*

**Overload 0:**

`[string]` : *Name of the chicken to fight.*

**Examples:**

```xml
!chicken ambush @Someone
!chicken ambush chicken
```
</p></details>

---

### chicken ambush help
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending chicken ambush and help the ambushed chicken.*

**Aliases:**
`h, halp, hlp, ha`

</p></details>

---

### chicken ambush join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending chicken ambush as one of the ambushers.*

**Aliases:**
`+, compete, enter, j, <, <<`

</p></details>

---

## Group: chicken buy
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a new chicken in this guild using your credits from WM bank.*

**Aliases:**
`b, shop`

**Arguments:**

`[string...]` : *Chicken name.*

**Examples:**

```xml
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

`[string...]` : *Chicken name.*

**Examples:**

```xml
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

`[string...]` : *Chicken name.*

**Examples:**

```xml
!chicken buy default My Chicken Name
```
</p></details>

---

### chicken buy list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all available chicken types.*

**Aliases:**
`ls, view`

</p></details>

---

### chicken buy steroidempowered
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a steroid-empowered chicken.*

**Aliases:**
`steroid, empowered`

**Arguments:**

`[string...]` : *Chicken name.*

**Examples:**

```xml
!chicken buy steroidempowered My Chicken Name
```
</p></details>

---

### chicken buy trained
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a trained chicken.*

**Aliases:**
`tr, train`

**Arguments:**

`[string...]` : *Chicken name.*

**Examples:**

```xml
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

`[string...]` : *Chicken name.*

**Examples:**

```xml
!chicken buy wellfed My Chicken Name
```
</p></details>

---

### chicken fight
<details><summary markdown='span'>Expand for additional information</summary><p>

*Make your chicken and another user's chicken fight eachother!*

**Aliases:**
`f, duel, attack`

**Overload 1:**

`[member]` : *Member whose chicken to fight.*

**Overload 0:**

`[string]` : *Name of the chicken to fight.*

**Examples:**

```xml
!chicken fight @Someone
```
</p></details>

---

### chicken flu
<details><summary markdown='span'>Expand for additional information</summary><p>

*Pay a well-known scientist to create a disease that disintegrates weak chickens.*

**Aliases:**
`cancer, disease, blackdeath`

</p></details>

---

### chicken heal
<details><summary markdown='span'>Expand for additional information</summary><p>

*Heal your chicken (+100 HP). There is one medicine made each 5 minutes, so you need to grab it before the others do!*

**Aliases:**
`+hp, hp`

</p></details>

---

### chicken info
<details><summary markdown='span'>Expand for additional information</summary><p>

*View user's chicken info. If the user is not given, views sender's chicken info.*

**Aliases:**
`information, stats`

**Arguments:**

(optional) `[member]` : *User.* (def: `None`)

**Examples:**

```xml
!chicken info 
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

`[string...]` : *New chicken name.*

**Examples:**

```xml
!chicken rename New Name
```
</p></details>

---

### chicken sell
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sell your chicken.*

**Aliases:**
`s`

</p></details>

---

### chicken top
<details><summary markdown='span'>Expand for additional information</summary><p>

*View the list of strongest chickens in the current guild.*

**Aliases:**
`best, strongest`

</p></details>

---

### chicken topglobal
<details><summary markdown='span'>Expand for additional information</summary><p>

*View the list of strongest chickens globally.*

**Aliases:**
`bestglobally, globallystrongest, globaltop, topg, gtop`

</p></details>

---

## Group: chicken train
<details><summary markdown='span'>Expand for additional information</summary><p>

*Train your chicken using your credits from WM bank.*

**Aliases:**
`tr, t, exercise`

</p></details>

---

### chicken train strength
<details><summary markdown='span'>Expand for additional information</summary><p>

*Train your chicken's strength using your credits from WM bank.*

**Aliases:**
`str, st, s`

</p></details>

---

### chicken train vitality
<details><summary markdown='span'>Expand for additional information</summary><p>

*Train your chicken's vitality using your credits from WM bank.*

**Aliases:**
`vit, vi, v`

</p></details>

---

## Group: chicken upgrades
<details><summary markdown='span'>Expand for additional information</summary><p>

*Upgrade your chicken with items you can buy using your credits from WM bank. Group call lists all available upgrades.*

**Aliases:**
`perks, upgrade, u`

**Overload 0:**

`[int...]` : *IDs of the upgrades to buy.*

**Examples:**

```xml
!chicken upgrades 
!chicken upgrades 1
!chicken upgrades 1 2 3
```
</p></details>

---

### chicken upgrades list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all available upgrades.*

**Aliases:**
`ls, view`

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

```xml
!chicken war 
!chicken war Team1 Team2
!chicken war "Team 1 name" "Team 2 name"
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

```xml
!chicken war join Team Name
```
</p></details>

---

