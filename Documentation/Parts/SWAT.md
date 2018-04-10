# Module: SWAT

### swat ip
*Return IP of the registered server by name.*

**Aliases:**
`getip`

**Arguments:**

`[string]` : *Registered name.*

**Examples:**

```
!s4 ip wm
```
---

### swat query
*Return server information.*

**Aliases:**
`q, info, i`

**Arguments:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Examples:**

```
!s4 q 109.70.149.158
!s4 q 109.70.149.158:10480
!s4 q wm
```
---

### swat serverlist
*Print the serverlist with current player numbers.*

**Examples:**

```
!swat serverlist
```
---

### swat servers add
*Add a server to serverlist.*

**Owner-only.**

**Aliases:**
`+, a`

**Arguments:**

`[string]` : *Name.*

`[string]` : *IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Examples:**

```
!swat servers add 4u 109.70.149.158:10480
!swat servers add 4u 109.70.149.158:10480 10481
```
---

### swat servers delete
*Remove a server from serverlist.*

**Owner-only.**

**Aliases:**
`-, del, d`

**Arguments:**

`[string]` : *Name.*

**Examples:**

```
!swat servers delete 4u
```
---

### swat servers list
*List all registered servers.*

**Owner-only.**

**Aliases:**
`ls, l`

**Examples:**

```
!swat servers list
```
---

### swat settimeout
*Set checking timeout.*

**Owner-only.**

**Arguments:**

`[int]` : *Timeout (in ms).*

**Examples:**

```
!swat settimeout 500
```
---

### swat startcheck
*Start listening for space on a given server and notifies you when there is space.*

**Aliases:**
`checkspace, spacecheck`

**Arguments:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

**Examples:**

```
!s4 startcheck 109.70.149.158
!s4 startcheck 109.70.149.158:10480
!swat startcheck wm
```
---

### swat stopcheck
*Stops space checking.*

**Aliases:**
`checkstop`

**Examples:**

```
!swat stopcheck
```
---

