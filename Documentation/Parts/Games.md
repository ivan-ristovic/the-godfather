# Module: Games

## Group: cards
<details><summary markdown='span'>Expand for additional information</summary><p>

*Miscellaneous playing card commands.*

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

(optional) `[int]` : *Amount (in range [1, 10]).* (def: `1`)

**Examples:**

```xml
!cards draw 
!cards draw 5
```
</p></details>

---

### cards reset
<details><summary markdown='span'>Expand for additional information</summary><p>

*Opens a brand new card deck.*

**Aliases:**
`new, opennew, open`

</p></details>

---

## Group: game
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a game for you to play!*

**Aliases:**
`games, gm`

</p></details>

---

## Group: game animalrace
<details><summary markdown='span'>Expand for additional information</summary><p>

*Start a new animal race!*

**Aliases:**
`animr, arace, ar, animalr`

</p></details>

---

### game animalrace join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join an existing animal race game.*

**Aliases:**
`+, compete, enter, j`

</p></details>

---

### game animalrace stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

</p></details>

---

## Group: game caro
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a "Caro" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which players must submit their move.*

**Aliases:**
`c, gomoku, gobang`

**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```xml
!game caro 
!game caro 10s
```
</p></details>

---

### game caro rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the Caro game rules.*

**Aliases:**
`help, h, ruling, rule`

</p></details>

---

### game caro stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

</p></details>

---

## Group: game connect4
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a "Connect 4" game. Play a move by writing a number from 1 to 9 corresponding to the column where you wish to insert your piece. You can also specify a time window in which player must submit their move.*

**Aliases:**
`connectfour, chain4, chainfour, c4, fourinarow, fourinaline, 4row, 4line, cfour`

**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```xml
!game connect4 
!game connect4 10s
```
</p></details>

---

### game connect4 rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the Connect4 game rules.*

**Aliases:**
`help, h, ruling, rule`

</p></details>

---

### game connect4 stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

</p></details>

---

## Group: game duel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a duel which I will commentate.*

**Aliases:**
`fight, vs, d`

**Arguments:**

`[user]` : *Who to fight with?*

**Examples:**

```xml
!game duel @Someone
```
</p></details>

---

### game duel rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the Duel game rules.*

**Aliases:**
`help, h, ruling, rule`

</p></details>

---

### game duel stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

</p></details>

---

## Group: game hangman
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a hangman game.*

**Aliases:**
`h, hang`

</p></details>

---

### game hangman rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the Hangman game rules.*

**Aliases:**
`help, h, ruling, rule`

</p></details>

---

### game hangman stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

</p></details>

---

### game leaderboard
<details><summary markdown='span'>Expand for additional information</summary><p>

*View the global game leaderboard.*

**Aliases:**
`globalstats`

</p></details>

---

## Group: game numberrace
<details><summary markdown='span'>Expand for additional information</summary><p>

*Number racing game commands.*

**Aliases:**
`nr, n, nunchi, numbers, numbersrace`

</p></details>

---

### game numberrace join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join an existing number race game.*

**Aliases:**
`+, compete, j, enter`

</p></details>

---

### game numberrace rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the number race rules.*

**Aliases:**
`help, h, ruling, rule`

</p></details>

---

### game numberrace stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

</p></details>

---

## Group: game othello
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts an "Othello" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.*

**Aliases:**
`reversi, oth, rev`

**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```xml
!game othello 
!game othello 10s
```
</p></details>

---

### game othello rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the Othello game rules.*

**Aliases:**
`help, h, ruling, rule`

</p></details>

---

### game othello stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

</p></details>

---

## Group: game quiz
<details><summary markdown='span'>Expand for additional information</summary><p>

*Play a quiz! Group call lists all available quiz categories.*

**Aliases:**
`trivia, q`

**Overload 4:**

`[int]` : *ID of the quiz category.*

(optional) `[int]` : *Amount of questions.* (def: `10`)

(optional) `[string]` : *Difficulty. (easy/medium/hard)* (def: `easy`)

**Overload 3:**

`[int]` : *ID of the quiz category.*

(optional) `[string]` : *Difficulty. (easy/medium/hard)* (def: `easy`)

(optional) `[int]` : *Amount of questions.* (def: `10`)

**Overload 2:**

`[string]` : *Quiz category.*

(optional) `[string]` : *Difficulty. (easy/medium/hard)* (def: `easy`)

(optional) `[int]` : *Amount of questions.* (def: `10`)

**Overload 1:**

`[string...]` : *Quiz category.*

**Examples:**

```xml
!game quiz 
!game quiz countries
!game quiz 9
!game quiz history
!game quiz history hard
!game quiz history hard 15
!game quiz 9 hard
!game quiz 9 hard 15
```
</p></details>

---

### game quiz capitals
<details><summary markdown='span'>Expand for additional information</summary><p>

*Country capitals guessing quiz. You can also specify how many questions there will be in the quiz.*

**Aliases:**
`capitaltowns`

**Arguments:**

(optional) `[int]` : *Number of questions.* (def: `10`)

**Examples:**

```xml
!game quiz capitals 
!game quiz capitals 15
```
</p></details>

---

### game quiz countries
<details><summary markdown='span'>Expand for additional information</summary><p>

*Country flags guessing quiz. You can also specify how many questions there will be in the quiz.*

**Aliases:**
`flags`

**Arguments:**

(optional) `[int]` : *Number of questions.* (def: `10`)

**Examples:**

```xml
!game quiz countries 
!game quiz countries 15
```
</p></details>

---

### game quiz stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

</p></details>

---

### game rps
<details><summary markdown='span'>Expand for additional information</summary><p>

*Rock, paper, scissors game against TheGodfather*

**Aliases:**
`rockpaperscissors`

**Arguments:**

`[string]` : *rock/paper/scissors*

**Examples:**

```xml
!game rps scissors
```
</p></details>

---

## Group: game russianroulette
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a russian roulette game which I will commentate.*

**Aliases:**
`rr, roulette, russianr`

</p></details>

---

### game russianroulette join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join an existing Russian roulette game pool.*

**Aliases:**
`+, compete, j, enter`

</p></details>

---

### game russianroulette rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the Russian roulette rules.*

**Aliases:**
`help, h, ruling, rule`

</p></details>

---

### game stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print game stats for given user.*

**Aliases:**
`s, st`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```xml
!game stats 
!game stats @Someone
```
</p></details>

---

## Group: game tictactoe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a "Tic-Tac-Toe" game. Play a move by writing a number from 1 to 9 corresponding to the field where you wish to play. You can also specify a time window in which players must submit their move.*

**Aliases:**
`ttt`

**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```xml
!game tictactoe 
!game tictactoe 10s
```
</p></details>

---

### game tictactoe rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Explain the Tic-Tac-Toe game rules.*

**Aliases:**
`help, h, ruling, rule`

</p></details>

---

### game tictactoe stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

</p></details>

---

## Group: game typingrace
<details><summary markdown='span'>Expand for additional information</summary><p>

*Start a new typing race!*

**Aliases:**
`tr, trace, typerace, typing, typingr`

</p></details>

---

### game typingrace join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Join an existing typing race game.*

**Aliases:**
`+, compete, enter, j`

</p></details>

---

