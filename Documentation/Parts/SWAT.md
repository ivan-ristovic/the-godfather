# Module: SWAT

## Group: swat
<details><summary markdown='span'>Expand for additional information</summary><p>

*SWAT4 related commands.*

**Aliases:**
`s4, swat4`

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

**Arguments:**

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

**Owner-only.**

**Aliases:**
`s, srv`

</p></details>

---

### swat servers add
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

### swat servers delete
<details><summary markdown='span'>Expand for additional information</summary><p>

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
</p></details>

---

### swat servers list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all registered servers.*

**Owner-only.**

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

**Arguments:**

`[string]` : *Registered name or IP.*

(optional) `[int]` : *Query port* (def: `10481`)

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

