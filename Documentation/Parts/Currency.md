# Module: Currency
*This module contains currency control commands. Each guild can define it's own currency and members can gain or lose currency with multiple commands. Currency can be used to buy specific guild items or chickens or it can be gambled away in the casino through multiple gambling games. Good luck!*


## Group: bank
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bank account commands (each guild has it's own bank). Group call prints out given user's bank balance. Accounts periodically get a small increase through interest.*

**Aliases:**
`$, $$, $$$`
**Guild only.**

**Arguments:**

(optional) `[member]` : *Member* (def: `None`)

**Examples:**

```xml
!bank
!bank Member
```
</p></details>

---

### bank balance
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints user's bank balance.*

**Aliases:**
`s, status, bal, money`
**Guild only.**

**Arguments:**

(optional) `[member]` : *Member* (def: `None`)

**Examples:**

```xml
!bank balance
!bank balance Member
```
</p></details>

---

### bank currency
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets guild currency.*

**Aliases:**
`setcurrency, curr`
**Guild only.**

**Overload 1:**

`[string]` : *New currency*

**Examples:**

```xml
!bank currency
!bank currency Some string here
```
</p></details>

---

### bank grant
<details><summary markdown='span'>Expand for additional information</summary><p>

*Grants user a certain amount of guild currency.*

**Aliases:**
`give`
**Guild only.**

**Privileged users only.**


**Overload 1:**

`[member]` : *Member*

`[long]` : *Amount*

**Overload 0:**

`[long]` : *Amount*

`[member]` : *Member*

**Examples:**

```xml
!bank grant Member 100000
!bank grant 100000 Member
```
</p></details>

---

### bank register
<details><summary markdown='span'>Expand for additional information</summary><p>

*Opens a new bank account for the sender.*

**Aliases:**
`r, signup, activate`
**Guild only.**

**Examples:**

```xml
!bank register
```
</p></details>

---

### bank top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists wealthiest users in this guild.*

**Aliases:**
`leaderboard, elite`
**Guild only.**

**Examples:**

```xml
!bank top
```
</p></details>

---

### bank topglobal
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists wealthiest users globally.*

**Aliases:**
`globalleaderboard, globalelite, gtop, topg, globaltop`
**Guild only.**

**Examples:**

```xml
!bank topglobal
```
</p></details>

---

### bank transfer
<details><summary markdown='span'>Expand for additional information</summary><p>

*Transfer funds from your account to another user's account.*

**Aliases:**
`lend, tr`
**Guild only.**

**Overload 1:**

`[member]` : *Member*

`[long]` : *Amount*

**Overload 0:**

`[long]` : *Amount*

`[member]` : *Member*

**Examples:**

```xml
!bank transfer Member 100000
!bank transfer 100000 Member
```
</p></details>

---

### bank unregister
<details><summary markdown='span'>Expand for additional information</summary><p>

*Closes a bank account.*

**Aliases:**
`ur, signout, deleteaccount, delacc, disable, deactivate`
**Guild only.**

**Privileged users only.**


**Overload 1:**

`[member]` : *Member*

(optional) `[boolean]` : *Globally?* (def: `False`)

**Overload 0:**

`[member]` : *Member*

**Examples:**

```xml
!bank unregister Member
!bank unregister @User Yes/No
```
</p></details>

---

## Group: casino
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints all available casino games.*

**Aliases:**
`vegas, cs, cas`
**Examples:**

```xml
!casino
```
</p></details>

---

## Group: casino blackjack
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Blackjack game or joins a pending Blackjack game with given bid amount.*

**Aliases:**
`bj`
**Arguments:**

(optional) `[int]` : *Bid amount* (def: `5`)

**Examples:**

```xml
!casino blackjack 100000
```
</p></details>

---

### casino blackjack join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Joins a pending Blackjack game with given bid amount.*

**Aliases:**
`+, compete, enter, j, <<, <`
**Arguments:**

(optional) `[int]` : *Bid amount* (def: `5`)

**Examples:**

```xml
!casino blackjack join 100000
```
</p></details>

---

### casino blackjack rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Blackjack rules.*

**Aliases:**
`help, h, ruling, rule, info`
**Examples:**

```xml
!casino blackjack rules
```
</p></details>

---

## Group: casino holdem
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Texas Hold 'em poker game or joins a pending holdem game with given bid amount.*

**Aliases:**
`poker, texasholdem, texas`
**Arguments:**

(optional) `[int]` : *Total balance for each user* (def: `1000`)

**Examples:**

```xml
!casino holdem 100000
```
</p></details>

---

### casino holdem join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Joins a pending Texas Hold 'em game with given bid amount.*

**Aliases:**
`+, compete, enter, j, <<, <`
**Examples:**

```xml
!casino holdem join 100000
```
</p></details>

---

### casino holdem rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Texas Hold 'em rules.*

**Aliases:**
`help, h, ruling, rule`
**Examples:**

```xml
!casino holdem rules
```
</p></details>

---

## Group: casino lottery
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Lottery game or joins a pending Lottery game with given three numbers. The three numbers are drawn from 1 to 15 and they can't be repeated. Rewards are given for each guess, increasing as the number of guesses increases.*

**Aliases:**
`lotto, bingo`
**Arguments:**

`[int...]` : *3 numbers*

**Examples:**

```xml
!casino lottery 1 5 10
```
</p></details>

---

### casino lottery join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Joins a pending Lottery game with given three numbers.*

**Aliases:**
`+, compete, enter, j, <<, <`
**Arguments:**

`[int...]` : *3 numbers*

**Examples:**

```xml
!casino lottery join 1 5 10
```
</p></details>

---

### casino lottery rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Lottery rules.*

**Aliases:**
`help, h, ruling, rule`
**Examples:**

```xml
!casino lottery rules
```
</p></details>

---

### casino slot
<details><summary markdown='span'>Expand for additional information</summary><p>

*Rolls a Slot Machine.*

**Aliases:**
`slotmachine`
**Overload 1:**

(optional) `[long]` : *Bid* (def: `5`)

**Overload 0:**

`[string...]` : *Bid*

**Examples:**

```xml
!casino slot 100000
```
</p></details>

---

### casino wheeloffortune
<details><summary markdown='span'>Expand for additional information</summary><p>

*Rolls a Wheel of Fortune.*

**Aliases:**
`wof`
**Overload 1:**

(optional) `[long]` : *Bid* (def: `5`)

**Overload 0:**

`[string...]` : *Bid*

**Examples:**

```xml
!casino wheeloffortune 100000
```
</p></details>

---

## Group: gamble
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gambling commands - requires guild currency.*

**Aliases:**
`bet`
**Guild only.**

</p></details>

---

### gamble coinflip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gambles on a coinflip toss. Default bid is 5.*

**Aliases:**
`coin, flip`
**Guild only.**

**Overload 1:**

`[long]` : *Bid amount*

`[string]` : *Heads/Tails*

**Overload 0:**

`[string]` : *Heads/Tails*

(optional) `[long]` : *Bid amount* (def: `5`)

**Examples:**

```xml
!gamble coinflip 100000 heads
!gamble coinflip heads 100000
```
</p></details>

---

### gamble dice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gambles on a dice throw. Default bid is 5.*

**Aliases:**
`roll, die`
**Guild only.**

**Arguments:**

`[long]` : *Bid amount*

(optional) `[int]` : *Dice outcome (1-6)* (def: `5`)

**Examples:**

```xml
!gamble dice 100000 5
```
</p></details>

---

## Group: shop
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shop for items using guild currency from your bank account. Group command lists all available items for purchase.*

**Aliases:**
`store, mall`
**Guild only.**

**Examples:**

```xml
!shop
```
</p></details>

---

### shop add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new item to guild shop.*

**Aliases:**
`register, reg, additem, a, +, +=, <<, <, <-, <=`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Overload 1:**

`[long]` : *Item price*

`[string...]` : *Item name*

**Overload 0:**

`[string]` : *Item name*

`[long]` : *Item price*

**Examples:**

```xml
!shop add SampleName 100000
!shop add 100000 SampleName
```
</p></details>

---

### shop buy
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buys a new item for you from the guild shop.*

**Aliases:**
`purchase, shutupandtakemymoney, b, p`
**Guild only.**

**Overload 1:**

`[int...]` : *Item IDs*

**Overload 1:**

`[string]` : *Item name*

**Examples:**

```xml
!shop buy SampleName
!shop buy 1 5
```
</p></details>

---

### shop delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes an item from guild shop.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Arguments:**

`[int...]` : *Item IDs to remove*

**Examples:**

```xml
!shop delete SampleName
!shop delete 1 5
```
</p></details>

---

### shop deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all items from the guild shop.*

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`
**Guild only.**

**Requires user permissions:**
`Manage guild`

**Examples:**

```xml
!shop deleteall
```
</p></details>

---

### shop list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all items available for purchase in the guild shop.*

**Aliases:**
`print, show, view, ls, l, p`
**Guild only.**

**Examples:**

```xml
!shop list
```
</p></details>

---

### shop purchases
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print user purchases in this guild.*

**Aliases:**
`myitems, purchased, bought`
**Guild only.**

**Arguments:**

(optional) `[member]` : *Member* (def: `None`)

**Examples:**

```xml
!shop purchases
!shop purchases Member
```
</p></details>

---

### shop sell
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sell item(s) bought from the guild shop.*

**Aliases:**
`return`
**Guild only.**

**Arguments:**

`[int...]` : *Item IDs*

**Examples:**

```xml
!shop sell 1 5
```
</p></details>

---

## Group: work
<details><summary markdown='span'>Expand for additional information</summary><p>

*Do something productive with your life or decide to earn money using immoral or illegal ways.*

**Aliases:**
`job`
**Guild only.**

**Examples:**

```xml
!work
```
</p></details>

---

### work crime
<details><summary markdown='span'>Expand for additional information</summary><p>

*Attempt a crime. This can be done once every 5 minutes.*

**Guild only.**

**Examples:**

```xml
!work crime
```
</p></details>

---

### work streets
<details><summary markdown='span'>Expand for additional information</summary><p>

*Work the streets tonight hoping to gather some easy money but beware - there are many threats lurking at that hour. This can be done once every 2 minutes.*

**Aliases:**
`prostitute`
**Guild only.**

**Examples:**

```xml
!work streets
```
</p></details>

---

