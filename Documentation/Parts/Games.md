# Module: Games

## Group: game animalrace
<details><summary markdown='span'>Expand for additional information</summary><code>

*Start a new animal race!*

**Aliases:**
`r, race, ar`

**Examples:**

```
!game animalrace
```
</code></details>

---

### game animalrace join
<details><summary markdown='span'>Expand for additional information</summary><code>

*Join an existing animal race game.*

**Aliases:**
`+, compete, enter, j`

**Examples:**

```
!game animalrace join
```
</code></details>

---

### game animalrace stats
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game animalrace stats
```
</code></details>

---

## Group: game caro
<details><summary markdown='span'>Expand for additional information</summary><code>

*Starts a "Caro" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.*

**Aliases:**
`c, gomoku, gobang`

**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```
!game caro
!game caro 10s
```
</code></details>

---

### game caro rules
<details><summary markdown='span'>Expand for additional information</summary><code>

*Explain the Caro game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game caro rules
```
</code></details>

---

### game caro stats
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game caro stats
```
</code></details>

---

## Group: game connect4
<details><summary markdown='span'>Expand for additional information</summary><code>

*Starts a "Connect 4" game. Play a move by writing a number from 1 to 9 corresponding to the column where you wish to insert your piece. You can also specify a time window in which player must submit their move.*

**Aliases:**
`connectfour, chain4, chainfour, c4, fourinarow, fourinaline, 4row, 4line, cfour`

**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```
!game connect4
!game connect4 10s
```
</code></details>

---

### game connect4 rules
<details><summary markdown='span'>Expand for additional information</summary><code>

*Explain the Connect4 game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game connect4 rules
```
</code></details>

---

### game connect4 stats
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game connect4 stats
```
</code></details>

---

## Group: game duel
<details><summary markdown='span'>Expand for additional information</summary><code>

*Starts a duel which I will commentate.*

**Aliases:**
`fight, vs, d`

**Arguments:**

`[user]` : *Who to fight with?*

**Examples:**

```
!game duel @Someone
```
</code></details>

---

### game duel rules
<details><summary markdown='span'>Expand for additional information</summary><code>

*Explain the Duel game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game duel rules
```
</code></details>

---

### game duel stats
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game duel stats
```
</code></details>

---

## Group: game hangman
<details><summary markdown='span'>Expand for additional information</summary><code>

*Starts a hangman game.*

**Aliases:**
`h, hang`

**Examples:**

```
!game hangman
```
</code></details>

---

### game hangman rules
<details><summary markdown='span'>Expand for additional information</summary><code>

*Explain the Hangman game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game hangman rules
```
</code></details>

---

### game hangman stats
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game hangman stats
```
</code></details>

---

### game leaderboard
<details><summary markdown='span'>Expand for additional information</summary><code>

*View the global game leaderboard.*

**Aliases:**
`globalstats`

**Examples:**

```
!game leaderboard
```
</code></details>

---

## Group: game numberrace
<details><summary markdown='span'>Expand for additional information</summary><code>

*Number racing game commands.*

**Aliases:**
`nr, n, nunchi, numbers, numbersrace`

**Examples:**

```
!game numberrace
```
</code></details>

---

### game numberrace join
<details><summary markdown='span'>Expand for additional information</summary><code>

*Join an existing number race game.*

**Aliases:**
`+, compete, j, enter`

**Examples:**

```
!game numberrace join
```
</code></details>

---

### game numberrace rules
<details><summary markdown='span'>Expand for additional information</summary><code>

*Explain the number race rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game numberrace rules
```
</code></details>

---

### game numberrace stats
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game numberrace stats
```
</code></details>

---

## Group: game othello
<details><summary markdown='span'>Expand for additional information</summary><code>

*Starts an "Othello" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.*

**Aliases:**
`reversi, oth, rev`

**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```
!game othello
!game othello 10s
```
</code></details>

---

### game othello rules
<details><summary markdown='span'>Expand for additional information</summary><code>

*Explain the Othello game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game othello rules
```
</code></details>

---

### game othello stats
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game othello stats
```
</code></details>

---

## Group: game quiz
<details><summary markdown='span'>Expand for additional information</summary><code>

*List all available quiz categories.*

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

```
!game quiz
!game quiz countries
!game quiz 9
!game quiz history
!game quiz history hard
!game quiz history hard 15
!game quiz 9 hard
!game quiz 9 hard 15
```
</code></details>

---

### game quiz capitals
<details><summary markdown='span'>Expand for additional information</summary><code>

*Country capitals guessing quiz. You can also specify how many questions there will be in the quiz.*

**Aliases:**
`capitaltowns`

**Arguments:**

(optional) `[int]` : *Number of questions.* (def: `10`)

**Examples:**

```
!game quiz capitals
!game quiz capitals 15
```
</code></details>

---

### game quiz countries
<details><summary markdown='span'>Expand for additional information</summary><code>

*Country flags guessing quiz. You can also specify how many questions there will be in the quiz.*

**Aliases:**
`flags`

**Arguments:**

(optional) `[int]` : *Number of questions.* (def: `10`)

**Examples:**

```
!game quiz countries
!game quiz countries 15
```
</code></details>

---

### game quiz stats
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game quiz stats
```
</code></details>

---

### game rps
<details><summary markdown='span'>Expand for additional information</summary><code>

*Rock, paper, scissors game against TheGodfather*

**Aliases:**
`rockpaperscissors`

**Arguments:**

`[string]` : *rock/paper/scissors*

**Examples:**

```
!game rps scissors
```
</code></details>

---

## Group: game russianroulette
<details><summary markdown='span'>Expand for additional information</summary><code>

*Starts a russian roulette game which I will commentate.*

**Aliases:**
`rr, roulette, russianr`

**Examples:**

```
!game russianroulette
```
</code></details>

---

### game russianroulette join
<details><summary markdown='span'>Expand for additional information</summary><code>

*Join an existing Russian roulette game pool.*

**Aliases:**
`+, compete, j, enter`

**Examples:**

```
!game russianroulette join
```
</code></details>

---

### game russianroulette rules
<details><summary markdown='span'>Expand for additional information</summary><code>

*Explain the Russian roulette rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game numberrace rules
```
</code></details>

---

### game stats
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print game stats for given user.*

**Aliases:**
`s, st`

**Arguments:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!game stats
!game stats @Someone
```
</code></details>

---

## Group: game tictactoe
<details><summary markdown='span'>Expand for additional information</summary><code>

*Starts a "Tic-Tac-Toe" game. Play a move by writing a number from 1 to 9 corresponding to the field where you wish to play. You can also specify a time window in which player must submit their move.*

**Aliases:**
`ttt`

**Arguments:**

(optional) `[time span]` : *Move time (def. 30s).* (def: `None`)

**Examples:**

```
!game tictactoe
!game tictactoe 10s
```
</code></details>

---

### game tictactoe rules
<details><summary markdown='span'>Expand for additional information</summary><code>

*Explain the Tic-Tac-Toe game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game tictactoe rules
```
</code></details>

---

### game tictactoe stats
<details><summary markdown='span'>Expand for additional information</summary><code>

*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game tictactoe stats
```
</code></details>

---

### game typingrace
<details><summary markdown='span'>Expand for additional information</summary><code>

*Typing race.*

**Aliases:**
`type, typerace, typing`

**Examples:**

```
!game typingrace
```
</code></details>

---

