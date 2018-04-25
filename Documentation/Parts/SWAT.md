# Module: SWAT

### swat ip
<details><summary markdown='span'>Expand for additional information</summary><code>

*Return IP of the registered server by name.*

**Aliases:**
`getip`

**Arguments:**

`[string]` : *Registered name.*

**Examples:**

```
!s4 ip wm
```
</code></details>

---

### swat query
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### swat serverlist
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the serverlist with current player numbers.*

**Examples:**

```
!swat serverlist
```
</code></details>

---

### swat servers add
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### swat servers delete
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### swat servers list
<details><summary markdown='span'>Expand for additional information</summary><code>

*List all registered servers.*

**Owner-only.**

**Aliases:**
`ls, l`

**Examples:**

```
!swat servers list
```
</code></details>

---

### swat settimeout
<details><summary markdown='span'>Expand for additional information</summary><code>

*Set checking timeout.*

**Owner-only.**

**Arguments:**

`[int]` : *Timeout (in ms).*

**Examples:**

```
!swat settimeout 500
```
</code></details>

---

### swat startcheck
<details><summary markdown='span'>Expand for additional information</summary><code>

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
</code></details>

---

### swat stopcheck
<details><summary markdown='span'>Expand for additional information</summary><code>

*Stops space checking.*

**Aliases:**
`checkstop`

**Examples:**

```
!swat stopcheck
```
</code></details>

---

