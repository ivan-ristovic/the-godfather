# Module: Currency

## Group: bank
<details><summary markdown='span'>Expand for additional information</summary><p>

*WM bank commands. Group call prints out given user's bank balance. Accounts periodically get an increase.*

**Aliases:**
`$, $$, $$$`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!bank
!bank @Someone
```
</p></details>

---

### bank balance
<details><summary markdown='span'>Expand for additional information</summary><p>

*View someone's bank account in this guild.*

**Aliases:**
`s, status, bal, money`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!bank balance @Someone
```
</p></details>

---

### bank currency
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set currency for this guild. Currency can be either emoji or text.*

**Aliases:**
`sc, setcurrency`

**Arguments:**

(optional) `[string...]` : *New currency.* (def: `None`)

**Examples:**

```
!bank currency :euro:
!bank currency My Custom Currency Name
```
</p></details>

---

### bank grant
<details><summary markdown='span'>Expand for additional information</summary><p>

*Magically increase another user's bank balance.*

**Privileged users only.**

**Aliases:**
`give`

**Overload 1:**

`[user]` : *User.*

`[long]` : *Amount.*

**Overload 0:**

`[long]` : *Amount.*

`[user]` : *User.*

**Examples:**

```
!bank grant @Someone 1000
!bank grant 1000 @Someone
```
</p></details>

---

### bank register
<details><summary markdown='span'>Expand for additional information</summary><p>

*Open an account in WM bank for this guild.*

**Aliases:**
`r, signup, activate`

**Examples:**

```
!bank register
```
</p></details>

---

### bank top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the richest users.*

**Aliases:**
`leaderboard, elite`

**Examples:**

```
!bank top
```
</p></details>

---

### bank topglobal
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the globally richest users.*

**Aliases:**
`globalleaderboard, globalelite, gtop, topg, globaltop`

**Examples:**

```
!bank gtop
```
</p></details>

---

### bank transfer
<details><summary markdown='span'>Expand for additional information</summary><p>

*Transfer funds from your account to another one.*

**Aliases:**
`lend`

**Overload 1:**

`[user]` : *User to send credits to.*

`[long]` : *Amount of currency to transfer.*

**Overload 0:**

`[long]` : *Amount of currency to transfer.*

`[user]` : *User to send credits to.*

**Examples:**

```
!bank transfer @Someone 40
!bank transfer 40 @Someone
```
</p></details>

---

### bank unregister
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete an account from WM bank.*

**Privileged users only.**

**Aliases:**
`ur, signout, deleteaccount, delacc, disable, deactivate`

**Overload 1:**

`[user]` : *User whose account to delete.*

(optional) `[boolean]` : *Globally delete?* (def: `False`)

**Overload 0:**

`[member]` : *User whose account to delete.*

**Examples:**

```
!bank unregister @Someone
```
</p></details>

---

## Group: casino
<details><summary markdown='span'>Expand for additional information</summary><p>

*Betting and gambling games.*

**Aliases:**
`vegas, cs, cas`

</p></details>

---

## Group: casino blackjack
<details><summary markdown='span'>Expand for additional information</summary><p>

*Play a blackjack game.*

**Aliases:**
`bj`

**Arguments:**

(optional) `[int]` : *Bid amount.* (def: `5`)

**Examples:**

```
!casino blackjack
```
</p></details>

---

### casino blackjack join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending Blackjack game.*

**Aliases:**
`+, compete, enter, j, <<, <`

**Arguments:**

(optional) `[int]` : *Bid amount.* (def: `5`)

**Examples:**

```
!casino blackjack join
```
</p></details>

---

### casino blackjack rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the Blackjack rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!casino blackjack rules
```
</p></details>

---

## Group: casino holdem
<details><summary markdown='span'>Expand for additional information</summary><p>

*Play a Texas Hold'Em game.*

**Aliases:**
`poker, texasholdem, texas`

**Arguments:**

(optional) `[int]` : *Amount of money required to enter.* (def: `1000`)

**Examples:**

```
!casino holdem 10000
```
</p></details>

---

### casino holdem join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending Texas Hold'Em game.*

**Aliases:**
`+, compete, enter, j, <<, <`

**Examples:**

```
!casino holdem join
```
</p></details>

---

### casino holdem rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the Texas Hold'Em rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!casino holdem rules
```
</p></details>

---

## Group: casino lottery
<details><summary markdown='span'>Expand for additional information</summary><p>

*Play a lottery game. The three numbers are drawn from 1 to 15 and they can't be repeated.*

**Aliases:**
`lotto`

**Arguments:**

`[int...]` : *Three numbers.*

**Examples:**

```
!casino lottery 2 10 8
```
</p></details>

---

### casino lottery join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending Lottery game.*

**Aliases:**
`+, compete, enter, j, <<, <`

**Arguments:**

`[int...]` : *Three numbers.*

**Examples:**

```
!casino lottery join 2 10 8
```
</p></details>

---

### casino lottery rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the Lottery rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!casino lottery rules
```
</p></details>

---

### casino slot
<details><summary markdown='span'>Expand for additional information</summary><p>

*Roll a slot machine. You need to specify a bid amount. Default bid amount is 5.*

**Aliases:**
`slotmachine`

**Overload 1:**

(optional) `[long]` : *Bid.* (def: `5`)

**Overload 0:**

`[string...]` : *Bid as a metric number.*

**Examples:**

```
!casino slot 20
```
</p></details>

---

### casino wheeloffortune
<details><summary markdown='span'>Expand for additional information</summary><p>

*Roll a Wheel Of Fortune. You need to specify a bid amount. Default bid amount is 5.*

**Aliases:**
`wof`

**Overload 1:**

(optional) `[long]` : *Bid.* (def: `5`)

**Overload 0:**

`[string...]` : *Bid as a metric number.*

**Examples:**

```
!casino wof 20
```
</p></details>

---

## Group: gamble
<details><summary markdown='span'>Expand for additional information</summary><p>

*Betting and gambling commands.*

**Aliases:**
`bet`

</p></details>

---

### gamble coinflip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Flip a coin and bet on the outcome.*

**Aliases:**
`coin, flip`

**Overload 1:**

`[long]` : *Bid.*

`[string]` : *Heads/Tails (h/t).*

**Overload 0:**

`[string]` : *Heads/Tails (h/t).*

`[long]` : *Bid.*

**Examples:**

```
!bet coinflip 10 heads
!bet coinflip tails 20
```
</p></details>

---

### gamble dice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Roll a dice and bet on the outcome.*

**Aliases:**
`roll, die`

**Overload 1:**

`[long]` : *Bid.*

`[string]` : *Number guess (has to be a word one-six).*

**Overload 0:**

`[string]` : *Number guess (has to be a word one-six).*

`[long]` : *Bid.*

**Examples:**

```
!bet dice 50 six
!bet dice three 10
```
</p></details>

---

## Group: shop
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shop for items using WM credits from your bank account. If invoked without subcommand, lists all available items for purchase.*

**Aliases:**
`store`

**Examples:**

```
!shop
```
</p></details>

---

### shop add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a new item to guild purchasable items list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`+, a, +=, <, <<, additem`

**Overload 1:**

`[long]` : *Item price.*

`[string...]` : *Item name.*

**Overload 0:**

`[string]` : *Item name.*

`[long]` : *Item price.*

**Examples:**

```
!shop add Barbie 500
!shop add "New Barbie" 500
!shop add 500 Newest Barbie
```
</p></details>

---

### shop buy
<details><summary markdown='span'>Expand for additional information</summary><p>

*Purchase an item from this guild's shop.*

**Aliases:**
`purchase, shutupandtakemymoney, b, p`

**Overload 1:**

`[int]` : *Item ID.*

**Overload 1:**

`[string]` : *Item name.*

**Examples:**

```
!shop buy 3
```
</p></details>

---

### shop delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove purchasable item from this guild item list. You can remove an item by ID or by name.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`-, remove, rm, del, -=, >, >>`

**Arguments:**

`[int...]` : *ID list of items to remove.*

**Examples:**

```
!shop delete Barbie
!shop delete 5
!shop delete 1 2 3 4 5
```
</p></details>

---

### shop list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all purchasable items for this guild.*

**Aliases:**
`ls`

**Examples:**

```
!shop list
```
</p></details>

---

### shop sell
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sell a purchased item for half the buy price.*

**Aliases:**
`return`

**Arguments:**

`[int]` : *Item ID.*

**Examples:**

```
!shop sell 3
```
</p></details>

---

