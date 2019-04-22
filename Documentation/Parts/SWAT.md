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

**Privileged users only.**

**Aliases:**
`b, blist, bans, ban`

</p></details>

---

### swat banlist add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a player to banlist.*

**Privileged users only.**

**Aliases:**
`+, a, +=, <, <<`

**Overload 2:**

`[string]` : *Player name.*

`[CustomIPFormat]` : *IP.*

(optional) `[string...]` : *Reason for ban.* (def: `None`)

**Overload 1:**

`[CustomIPFormat]` : *IP.*

`[string]` : *Player name.*

(optional) `[string...]` : *Reason for ban.* (def: `None`)

**Overload 1:**

`[string]` : *Player name.*

(optional) `[string...]` : *Reason for ban.* (def: `None`)

**Examples:**

```xml
!swat banlist add Name 109.70.149.158
!swat banlist add Name 109.70.149.158 Reason for ban
```
</p></details>

---

### swat banlist delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove ban entry from database.*

**Privileged users only.**

**Aliases:**
`-, del, d, remove, -=, >, >>, rm`

**Overload 1:**

`[CustomIPFormat]` : *IP.*

**Overload 1:**

`[string]` : *Player name.*

**Examples:**

```xml
!swat banlist delete 123.123.123.123
```
</p></details>

---

### swat banlist list
<details><summary markdown='span'>Expand for additional information</summary><p>

*View the banlist.*

**Privileged users only.**

**Aliases:**
`ls, l, print`

</p></details>

---

## Group: swat database
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*SWAT4 player IP database manipulation commands.*

**Privileged users only.**

**Aliases:**
`db`

</p></details>

---

### swat database add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a player to IP database.*

**Privileged users only.**

**Aliases:**
`+, a, +=, <, <<`

**Overload 2:**

`[string]` : *Player name.*

`[CustomIPFormat]` : *IP.*

(optional) `[string...]` : *Additional info.* (def: `None`)

**Overload 1:**

`[string]` : *Player name.*

`[CustomIPFormat...]` : *IPs.*

**Overload 0:**

`[CustomIPFormat]` : *IP.*

`[string]` : *Player name.*

(optional) `[string...]` : *Additional info.* (def: `None`)

**Examples:**

```xml
!swat database add Name 109.70.149.158
```
</p></details>

---

### swat database alias
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a player alias to the database.*

**Privileged users only.**

**Aliases:**
`+a, aa, +=a, <a, <<a`

**Overload 2:**

`[string]` : *Player name.*

`[string]` : *Player alias.*

**Overload 1:**

`[string]` : *Player alias.*

`[CustomIPFormat]` : *Player IP.*

**Overload 0:**

`[CustomIPFormat]` : *Player IP.*

`[string]` : *Player alias.*

**Examples:**

```xml
!swat database alias Name Alias
```
</p></details>

---

### swat database delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove IP entry from database.*

**Privileged users only.**

**Aliases:**
`-, del, d, -=, >, >>`

**Overload 1:**

`[CustomIPFormat]` : *IP or range.*

**Overload 0:**

`[string...]` : *Name.*

**Examples:**

```xml
!swat database delete 123.123.123.123
!swat database delete Name
```
</p></details>

---

### swat database list
<details><summary markdown='span'>Expand for additional information</summary><p>

*View the IP list.*

**Privileged users only.**

**Aliases:**
`ls, l, print`

**Arguments:**

(optional) `[int]` : *From which index to view.* (def: `1`)

(optional) `[int]` : *How many results to view.* (def: `10`)

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

```xml
!swat ip wm
```
</p></details>

---

### swat query
<details><summary markdown='span'>Expand for additional information</summary><p>

*Return server information.*

**Aliases:**
`q, info, i`

**Overload 1:**

`[CustomIPFormat]` : *Server IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Overload 0:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Examples:**

```xml
!swat query 109.70.149.158
!swat query 109.70.149.158:10480
!swat query wm
```
</p></details>

---

## Group: swat search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*SWAT4 database search commands.*

**Privileged users only.**

**Aliases:**
`s, find, lookup`

**Overload 1:**

`[CustomIPFormat]` : *IP or range.*

(optional) `[int]` : *Number of results* (def: `10`)

**Overload 0:**

`[string]` : *Player name to search.*

(optional) `[int]` : *Number of results* (def: `10`)

</p></details>

---

### swat search ip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search for a given IP or range.*

**Privileged users only.**

**Arguments:**

`[CustomIPFormat]` : *IP or range.*

(optional) `[int]` : *Number of results* (def: `10`)

**Examples:**

```xml
!swat search ip 123.123.123.123
```
</p></details>

---

### swat search name
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search for a given name.*

**Privileged users only.**

**Aliases:**
`player, nickname, nick`

**Arguments:**

`[string]` : *Player name.*

(optional) `[int]` : *Number of results* (def: `10`)

**Examples:**

```xml
!swat search name EmoPig
```
</p></details>

---

### swat serverlist
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the serverlist with current player numbers.*

**Aliases:**
`sl, list`

**Arguments:**

(optional) `[string]` : *Server name group.* (def: `None`)

**Examples:**

```xml
!swat serverlist 
!swat serverlist wm
```
</p></details>

---

## Group: swat servers
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*SWAT4 serverlist manipulation commands.*

**Privileged users only.**

**Aliases:**
`serv, srv`

</p></details>

---

### swat servers add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a server to serverlist.*

**Privileged users only.**

**Aliases:**
`+, a, +=, <, <<`

**Overload 1:**

`[string]` : *Name.*

`[CustomIPFormat]` : *IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Overload 0:**

`[CustomIPFormat]` : *IP.*

`[string]` : *Name.*

(optional) `[int]` : *Query port* (def: `10481`)

**Examples:**

```xml
!swat servers add 4u 109.70.149.158:10480
!swat servers add 4u 109.70.149.158:10480 10481
```
</p></details>

---

### swat servers delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove a server from serverlist.*

**Privileged users only.**

**Aliases:**
`-, del, d, -=, >, >>`

**Arguments:**

`[string]` : *Name.*

**Examples:**

```xml
!swat servers delete 4u
```
</p></details>

---

### swat servers list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all registered servers.*

**Privileged users only.**

**Aliases:**
`ls, l`

</p></details>

---

### swat settimeout
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*Set checking timeout.*

**Owner-only.**

**Arguments:**

`[int]` : *Timeout (in ms).*

**Examples:**

```xml
!swat settimeout 500
```
</p></details>

---

### swat startcheck
<details><summary markdown='span'>Expand for additional information</summary><p>

*Start listening for space on a given server and notifies you when there is space.*

**Aliases:**
`checkspace, spacecheck, sc`

**Overload 1:**

`[CustomIPFormat]` : *IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Overload 0:**

`[string]` : *Registered name.*

**Examples:**

```xml
!swat startcheck 109.70.149.158
!swat startcheck 109.70.149.158:10480
!swat startcheck wm
```
</p></details>

---

### swat stopcheck
<details><summary markdown='span'>Expand for additional information</summary><p>

*Stops space checking.*

**Aliases:**
`checkstop`

</p></details>

---

