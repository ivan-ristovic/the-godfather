# Module: Gambling

## Group: bank
*Bank manipulation. If invoked alone, prints out your bank balance.*

**Aliases:**
`$, $$, $$$`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!bank
```
---

### bank balance
*View account balance for given user. If the user is not given, checks sender's balance.*

**Aliases:**
`s, status, bal, money, credits`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!bank balance @Someone
```
---

### bank grant
*Magically give funds to some user.*

**Requires user permissions:**
`Administrator`

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
---

### bank register
*Create an account for you in WM bank.*

**Aliases:**
`r, signup, activate`

**Examples:**

```
!bank register
```
---

### bank top
*Print the richest users.*

**Aliases:**
`leaderboard, elite`

**Examples:**

```
!bank top
```
---

### bank transfer
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
---

## Group: cards
*Manipulate a deck of cards.*

**Aliases:**
`deck`

---

### cards draw
*Draw cards from the top of the deck. If amount of cards is not specified, draws one card.*

**Aliases:**
`take`

**Arguments:**

(optional) `[int]` : *Amount (in range [1-10]).* (def: `1`)

**Examples:**

```
!deck draw 5
```
---

### cards reset
*Opens a brand new card deck.*

**Aliases:**
`new, opennew, open`

**Examples:**

```
!deck reset
```
---

### cards shuffle
*Shuffles current deck.*

**Aliases:**
`s, sh, mix`

**Examples:**

```
!deck shuffle
```
---

### gamble coinflip
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
---

### gamble dice
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
!dice 50 six
!dice three 10
```
---

### gamble slot
*Roll a slot machine.*

**Aliases:**
`slotmachine`

**Arguments:**

(optional) `[int]` : *Bid.* (def: `5`)

**Examples:**

```
!gamble slot 20
```
---

