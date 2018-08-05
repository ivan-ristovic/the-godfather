# Module: SWAT

## Group: swat
<details><summary markdown='span'>Expand for additional information</summary><p>

*SWAT4 related commands.*

**Aliases:**
`s4, swat4`

</p></details>

---

## Group: swat banlist
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*SWAT4 banlist manipulation commands.*

**Aliases:**
`b, blist, bans, ban`

</p></details>

---

### swat banlist add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a player to banlist.*

**Aliases:**
`+, a, +=, <, <<`

**Overload 1:**

`[string]` : *Player name.*

`[CustomIpFormat]` : *IP.*

(optional) `[string...]` : *Reason for ban.* (def: `None`)

**Overload 0:**

`[CustomIpFormat]` : *IP.*

`[string]` : *Player name.*

(optional) `[string...]` : *Reason for ban.* (def: `None`)

**Examples:**

```
!swat banlist add Name 109.70.149.158
!swat banlist add Name 109.70.149.158 Reason for ban
```
</p></details>

---

### swat banlist delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove ban entry from database.*

**Aliases:**
`-, del, d, remove, -=, >, >>, rm`

**Arguments:**

`[CustomIpFormat]` : *IP.*

**Examples:**

```
!swat banlist delete 123.123.123.123
```
</p></details>

---

### swat banlist list
<details><summary markdown='span'>Expand for additional information</summary><p>

*View the banlist.*

**Aliases:**
`ls, l, print`

**Examples:**

```
!swat banlist list
```
</p></details>

---

## Group: swat database
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*SWAT4 player IP database manipulation commands.*

**Aliases:**
`db`

</p></details>

---

### swat database add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a player to IP database.*

**Aliases:**
`+, a, +=, <, <<`

**Overload 0:**

`[string]` : *Player name.*

`[CustomIpFormat]` : *IP.*

(optional) `[string...]` : *Additional info.* (def: `None`)

**Overload 0:**

`[CustomIpFormat]` : *IP.*

`[string]` : *Player name.*

(optional) `[string...]` : *Additional info.* (def: `None`)

**Examples:**

```
!swat db add Name 109.70.149.158
```
</p></details>

---

### swat database delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove ban entry from database.*

**Aliases:**
`-, del, d, -=, >, >>`

**Arguments:**

`[CustomIpFormat]` : *IP or range.*

**Examples:**

```
!swat db remove 123.123.123.123
```
</p></details>

---

### swat database list
<details><summary markdown='span'>Expand for additional information</summary><p>

*View the banlist.*

**Aliases:**
`ls, l`

**Examples:**

```
!swat db list
```
</p></details>

---

### swat ip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Return IP of the registered server by name.*

**Aliases:**
`getip`

**Arguments:**

`[string]` : *Registered name.*

**Examples:**

```
!s4 ip wm
```
</p></details>

---

### swat query
<details><summary markdown='span'>Expand for additional information</summary><p>

*Return server information.*

**Aliases:**
`q, info, i`

**Overload 1:**

`[CustomIpFormat]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Overload 0:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Examples:**

```
!s4 q 109.70.149.158
!s4 q 109.70.149.158:10480
!s4 q wm
```
</p></details>

---

## Group: swat search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*SWAT4 database search commands.*

**Aliases:**
`s, find, lookup`

**Arguments:**

`[string]` : *Player name to search.*

(optional) `[int]` : *Number of results* (def: `10`)

</p></details>

---

### swat search ip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search for a given IP or range.*

**Arguments:**

`[CustomIpFormat]` : *IP.*

(optional) `[int]` : *Number of results* (def: `10`)

**Examples:**

```
!swat search 123.123.123.123
```
</p></details>

---

### swat search name
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search for a given name.*

**Aliases:**
`player, nickname, nick`

**Arguments:**

`[string]` : *Player name.*

(optional) `[int]` : *Number of results* (def: `10`)

**Examples:**

```
!swat search EmoPig
```
</p></details>

---

### swat serverlist
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the serverlist with current player numbers.*

**Examples:**

```
!swat serverlist
```
</p></details>

---

## Group: swat servers
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*SWAT4 serverlist manipulation commands.*

**Aliases:**
`s, srv`

**Examples:**

```
!swat servers
```
</p></details>

---

### swat servers add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a server to serverlist.*

**Aliases:**
`+, a, +=, <, <<`

**Overload 1:**

`[string]` : *Name.*

`[CustomIpFormat]` : *IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Overload 0:**

`[CustomIpFormat]` : *IP.*

`[string]` : *Name.*

(optional) `[int]` : *Query port* (def: `10481`)

**Examples:**

```
!swat servers add 4u 109.70.149.158:10480
!swat servers add 4u 109.70.149.158:10480 10481
```
</p></details>

---

### swat servers delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove a server from serverlist.*

**Aliases:**
`-, del, d, -=, >, >>`

**Arguments:**

`[string]` : *Name.*

**Examples:**

```
!swat servers delete 4u
```
</p></details>

---

### swat servers list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all registered servers.*

**Aliases:**
`ls, l`

**Examples:**

```
!swat servers list
```
</p></details>

---

### swat settimeout
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set checking timeout.*

**Owner-only.**

**Arguments:**

`[int]` : *Timeout (in ms).*

**Examples:**

```
!swat settimeout 500
```
</p></details>

---

### swat startcheck
<details><summary markdown='span'>Expand for additional information</summary><p>

*Start listening for space on a given server and notifies you when there is space.*

**Aliases:**
`checkspace, spacecheck`

**Overload 1:**

`[CustomIpFormat]` : *IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Overload 0:**

`[string]` : *Registered name.*

**Examples:**

```
!s4 startcheck 109.70.149.158
!s4 startcheck 109.70.149.158:10480
!swat startcheck wm
```
</p></details>

---

### swat stopcheck
<details><summary markdown='span'>Expand for additional information</summary><p>

*Stops space checking.*

**Aliases:**
`checkstop`

**Examples:**

```
!swat stopcheck
```
</p></details>

---

