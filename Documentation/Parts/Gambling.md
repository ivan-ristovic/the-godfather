# Module: Gambling

## Group: bank
<details><summary markdown='span'>Expand for additional information</summary><code>

*Bank account manipulation. If invoked alone, prints out your bank balance. Accounts periodically get a bonus.*

**Aliases:**
`$, $$, $$$`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!bank
```
</code></details>

---

### bank balance
<details><summary markdown='span'>Expand for additional information</summary><code>

*View account balance for given user. If the user is not given, checks sender's balance.*

**Aliases:**
`s, status, bal, money, credits`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!bank balance @Someone
```
</code></details>

---

### bank grant
<details><summary markdown='span'>Expand for additional information</summary><code>

*Magically give funds to some user.*

**Owner-only.**

**Aliases:**
`give`

**Overload 1:**

`[user]` : *User.*

`[int]` : *Amount.*

**Overload 0:**

`[int]` : *Amount.*

`[user]` : *User.*

**Examples:**

```
!bank grant @Someone 1000
!bank grant 1000 @Someone
```
</code></details>

---

### bank register
<details><summary markdown='span'>Expand for additional information</summary><code>

*Create an account for you in WM bank.*

**Aliases:**
`r, signup, activate`

**Examples:**

```
!bank register
```
</code></details>

---

### bank top
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the richest users.*

**Aliases:**
`leaderboard, elite`

**Examples:**

```
!bank top
```
</code></details>

---

### bank transfer
<details><summary markdown='span'>Expand for additional information</summary><code>

*Transfer funds from your account to another one.*

**Aliases:**
`lend`

**Overload 1:**

`[user]` : *User to send credits to.*

`[int]` : *Amount.*

**Overload 0:**

`[int]` : *Amount.*

`[user]` : *User to send credits to.*

**Examples:**

```
!bank transfer @Someone 40
!bank transfer 40 @Someone
```
</code></details>

---

## Group: cards
<details><summary markdown='span'>Expand for additional information</summary><code>

*Manipulate a deck of cards.*

**Aliases:**
`deck`

</code></details>

---

### cards draw
<details><summary markdown='span'>Expand for additional information</summary><code>

*Draw cards from the top of the deck. If amount of cards is not specified, draws one card.*

**Aliases:**
`take`

**Arguments:**

(optional) `[int]` : *Amount (in range [1-10]).* (def: `1`)

**Examples:**

```
!deck draw 5
```
</code></details>

---

### cards reset
<details><summary markdown='span'>Expand for additional information</summary><code>

*Opens a brand new card deck.*

**Aliases:**
`new, opennew, open`

**Examples:**

```
!deck reset
```
</code></details>

---

### cards shuffle
<details><summary markdown='span'>Expand for additional information</summary><code>

*Shuffles current deck.*

**Aliases:**
`s, sh, mix`

**Examples:**

```
!deck shuffle
```
</code></details>

---

## Group: casino blackjack
<details><summary markdown='span'>Expand for additional information</summary><code>

*Play a blackjack game.*

**Aliases:**
`bj`

**Arguments:**

(optional) `[int]` : *Bid amount.* (def: `5`)

**Examples:**

```
!casino blackjack
```
</code></details>

---

### casino blackjack join
<details><summary markdown='span'>Expand for additional information</summary><code>

*Join a pending Blackjack game.*

**Aliases:**
`+, compete, enter, j`

**Arguments:**

(optional) `[int]` : *Bid amount.* (def: `5`)

**Examples:**

```
!casino blackjack join
```
</code></details>

---

### casino blackjack rules
<details><summary markdown='span'>Expand for additional information</summary><code>

*Explain the Blackjack rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!casino blackjack rules
```
</code></details>

---

### casino slot
<details><summary markdown='span'>Expand for additional information</summary><code>

*Roll a slot machine. You need to specify a bid amount. Default bid amount is 5.*

**Aliases:**
`slotmachine`

**Arguments:**

(optional) `[int]` : *Bid.* (def: `5`)

**Examples:**

```
!casino slot 20
```
</code></details>

---

### gamble coinflip
<details><summary markdown='span'>Expand for additional information</summary><code>

*Flip a coin and bet on the outcome.*

**Aliases:**
`coin, flip`

**Overload 1:**

`[int]` : *Bid.*

`[string]` : *Heads/Tails (h/t).*

**Overload 0:**

`[string]` : *Heads/Tails (h/t).*

`[int]` : *Bid.*

**Examples:**

```
!bet coinflip 10 heads
!bet coinflip tails 20
```
</code></details>

---

### gamble dice
<details><summary markdown='span'>Expand for additional information</summary><code>

*Roll a dice and bet on the outcome.*

**Aliases:**
`roll, die`

**Overload 1:**

`[int]` : *Bid.*

`[string]` : *Number guess (has to be a word one-six).*

**Overload 0:**

`[string]` : *Number guess (has to be a word one-six).*

`[int]` : *Bid.*

**Examples:**

```
!bet dice 50 six
!bet dice three 10
```
</code></details>

---

