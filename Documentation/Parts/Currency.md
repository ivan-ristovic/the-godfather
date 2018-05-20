# Module: Currency

## Group: bank
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bank account manipulation. If invoked alone, prints out your bank balance. Accounts periodically get a bonus.*

**Aliases:**
`$, $$, $$$`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!bank
```
</p></details>

---

### bank balance
<details><summary markdown='span'>Expand for additional information</summary><p>

*View account balance for given user. If the user is not given, checks sender's balance.*

**Aliases:**
`s, status, bal, money, credits`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!bank balance @Someone
```
</p></details>

---

### bank grant
<details><summary markdown='span'>Expand for additional information</summary><p>

*Magically give funds to some user.*

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

*Create an account for you in WM bank.*

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

### bank transfer
<details><summary markdown='span'>Expand for additional information</summary><p>

*Transfer funds from your account to another one.*

**Aliases:**
`lend`

**Overload 1:**

`[user]` : *User to send credits to.*

`[long]` : *Amount.*

**Overload 0:**

`[long]` : *Amount.*

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

**Aliases:**
`ur, signout, deleteaccount, delacc, disable, deactivate`

**Arguments:**

`[user]` : *User whose account to delete.*

**Examples:**

```
!bank unregister @Someone
```
</p></details>

---

## Group: cards
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate a deck of cards.*

**Aliases:**
`deck`

</p></details>

---

### cards draw
<details><summary markdown='span'>Expand for additional information</summary><p>

*Draw cards from the top of the deck. If amount of cards is not specified, draws one card.*

**Aliases:**
`take`

**Arguments:**

(optional) `[int]` : *Amount (in range [1-10]).* (def: `1`)

**Examples:**

```
!deck draw 5
```
</p></details>

---

### cards reset
<details><summary markdown='span'>Expand for additional information</summary><p>

*Opens a brand new card deck.*

**Aliases:**
`new, opennew, open`

**Examples:**

```
!deck reset
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
`+, compete, enter, j`

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
`+, compete, enter, j`

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

*Play a lottery game. The three numbers are drawn from 1 to 15 and they can't repeat.*

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
`+, compete, enter, j`

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

**Arguments:**

(optional) `[long]` : *Bid.* (def: `5`)

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

**Arguments:**

(optional) `[long]` : *Bid.* (def: `5`)

**Examples:**

```
!casino wof 20
```
</p></details>

---

## Group: chicken
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manage your chicken. If invoked without subcommands, prints out your chicken information.*

**Aliases:**
`cock, hen, chick, coc`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!chicken
!chicken @Someone
```
</p></details>

---

## Group: chicken ambush
<details><summary markdown='span'>Expand for additional information</summary><p>

*Start an ambush for another user's chicken. Other users can put their chickens into your ambush and collectively attack the target chicken combining their strength.*

**Aliases:**
`gangattack`

**Arguments:**

(optional) `[user]` : *Whose chicken to ambush.* (def: `None`)

**Examples:**

```
!chicken ambush @Someone
```
</p></details>

---

### chicken ambush join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join a pending chicken ambush.*

**Aliases:**
`+, compete, enter, j`

**Examples:**

```
!chicken ambush join
```
</p></details>

---

## Group: chicken buy
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a new chicken in this guild using your credits from WM bank.*

**Aliases:**
`b, shop`

**Arguments:**

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
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

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
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

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken buy default My Chicken Name
```
</p></details>

---

### chicken buy list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all available chicken types.*

**Aliases:**
`ls, view`

**Examples:**

```
!chicken buy list
```
</p></details>

---

### chicken buy steroidempowered
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a steroid-empowered chicken.*

**Aliases:**
`steroid, empowered`

**Arguments:**

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken buy steroidempowered My Chicken Name
```
</p></details>

---

### chicken buy trained
<details><summary markdown='span'>Expand for additional information</summary><p>

*Buy a trained chicken.*

**Aliases:**
`wf, fed`

**Arguments:**

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
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

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken buy wellfed My Chicken Name
```
</p></details>

---

### chicken fight
<details><summary markdown='span'>Expand for additional information</summary><p>

*Make your chicken and another user's chicken fight until death!*

**Aliases:**
`f, duel, attack`

**Arguments:**

`[user]` : *User.*

**Examples:**

```
!chicken duel @Someone
```
</p></details>

---

### chicken info
<details><summary markdown='span'>Expand for additional information</summary><p>

*View user's chicken info. If the user is not given, views sender's chicken info.*

**Aliases:**
`information, stats`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
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

(optional) `[string...]` : *Chicken name.* (def: `None`)

**Examples:**

```
!chicken name New Name
```
</p></details>

---

### chicken sell
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sell your chicken.*

**Aliases:**
`s`

**Examples:**

```
!chicken sell
```
</p></details>

---

### chicken top
<details><summary markdown='span'>Expand for additional information</summary><p>

*View the list of strongest chickens in the current guild.*

**Aliases:**
`best, strongest`

**Examples:**

```
!chicken top
```
</p></details>

---

### chicken train
<details><summary markdown='span'>Expand for additional information</summary><p>

*Train your chicken using your credits from WM bank.*

**Aliases:**
`tr, t, exercise`

**Examples:**

```
!chicken train
```
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

```
!chicken war Team1 Team2
!chicken war "Team 1 name" "Team 2 name
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

```
!chicken war join Team Name
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

