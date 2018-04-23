# Module: Games

## Group: game animalrace
*Start a new animal race!*

**Aliases:**
`r, race, ar`

**Examples:**

```
!game animalrace
```
---

### game animalrace join
*Join an existing animal race game.*

**Aliases:**
`+, compete, enter, j`

**Examples:**

```
!game animalrace join
```
---

### game animalrace stats
*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game animalrace stats
```
---

## Group: game caro
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
---

### game caro rules
*Explain the Caro game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game caro rules
```
---

### game caro stats
*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game caro stats
```
---

## Group: game connect4
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
---

### game connect4 rules
*Explain the Connect4 game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game connect4 rules
```
---

### game connect4 stats
*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game connect4 stats
```
---

## Group: game duel
*Starts a duel which I will commentate.*

**Aliases:**
`fight, vs, d`

**Arguments:**

`[user]` : *Who to fight with?*

**Examples:**

```
!game duel @Someone
```
---

### game duel rules
*Explain the Duel game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game duel rules
```
---

### game duel stats
*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game duel stats
```
---

## Group: game hangman
*Starts a hangman game.*

**Aliases:**
`h, hang`

**Examples:**

```
!game hangman
```
---

### game hangman rules
*Explain the Hangman game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game hangman rules
```
---

### game hangman stats
*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game hangman stats
```
---

### game leaderboard
*View the global game leaderboard.*

**Aliases:**
`globalstats`

**Examples:**

```
!game leaderboard
```
---

## Group: game numberrace
*Number racing game commands.*

**Aliases:**
`nr, n, nunchi, numbers, numbersrace`

**Examples:**

```
!game numberrace
```
---

### game numberrace join
*Join an existing number race game.*

**Aliases:**
`+, compete, j, enter`

**Examples:**

```
!game numberrace join
```
---

### game numberrace rules
*Explain the number race rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game numberrace rules
```
---

### game numberrace stats
*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game numberrace stats
```
---

## Group: game othello
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
---

### game othello rules
*Explain the Othello game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game othello rules
```
---

### game othello stats
*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game othello stats
```
---

## Group: game quiz
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
---

### game quiz capitals
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
---

### game quiz countries
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
---

### game quiz stats
*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game quiz stats
```
---

### game rps
*Rock, paper, scissors game against TheGodfather*

**Aliases:**
`rockpaperscissors`

**Arguments:**

`[string]` : *rock/paper/scissors*

**Examples:**

```
!game rps scissors
```
---

## Group: game russianroulette
*Starts a russian roulette game which I will commentate.*

**Aliases:**
`rr, roulette, russianr`

**Examples:**

```
!game russianroulette
```
---

### game russianroulette join
*Join an existing Russian roulette game pool.*

**Aliases:**
`+, compete, j, enter`

**Examples:**

```
!game russianroulette join
```
---

### game russianroulette rules
*Explain the Russian roulette rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game numberrace rules
```
---

### game stats
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
---

## Group: game tictactoe
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
---

### game tictactoe rules
*Explain the Tic-Tac-Toe game rules.*

**Aliases:**
`help, h, ruling, rule`

**Examples:**

```
!game tictactoe rules
```
---

### game tictactoe stats
*Print the leaderboard for this game.*

**Aliases:**
`top, leaderboard`

**Examples:**

```
!game tictactoe stats
```
---

### game typingrace
*Typing race.*

**Aliases:**
`type, typerace, typing`

**Examples:**

```
!game typingrace
```
---

