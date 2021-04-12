# Module: Games
*This module contains various games to pass your time. From quizzes to board games and Discord-interactive games, surely you will find something to pass your time.*


## Group: deck
<details><summary markdown='span'>Expand for additional information</summary><p>

*Card deck commands. Group call opens a new shuffled deck in the current channel.*

**Aliases:**
`cards`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!deck
```
</p></details>

---

### deck draw
<details><summary markdown='span'>Expand for additional information</summary><p>

*Draws cards from the top of the deck.*

**Aliases:**
`take`

**Overload 0:**
- (optional) \[`int`\]: *Amount of cards to draw* (def: `1`)

**Examples:**

```xml
!deck draw 5
```
</p></details>

---

### deck reset
<details><summary markdown='span'>Expand for additional information</summary><p>

*Opens a new shuffled deck in the current channel.*

**Aliases:**
`new, opennew, open`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!deck reset
```
</p></details>

---

## Group: game
<details><summary markdown='span'>Expand for additional information</summary><p>

*Game commands. Group call lists all available games.*

**Aliases:**
`games, gm`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game
```
</p></details>

---

## Group: game animalrace
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Animal Race game.*

**Aliases:**
`animr, arace, ar, animalr, race`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game animalrace
```
</p></details>

---

### game animalrace join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Joins a pending Animal Race game.*

**Aliases:**
`+, compete, enter, j, <<, <`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game animalrace join
```
</p></details>

---

### game animalrace stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Animal Race game stats for a given user.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game animalrace stats
!game animalrace stats @User
```
</p></details>

---

### game animalrace top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Animal Race game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game animalrace top
```
</p></details>

---

## Group: game caro
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Caro game for two players. Each player makes a move by writing a pair of numbers representing coordinates in the grid where they wish to play. You can also specify the move time to make the game easier/harder.*

**Aliases:**
`c, gomoku, gobang`
**Guild only.**


**Overload 0:**
- (optional) \[`time span`\]: *Time for a move* (def: `None`)

**Examples:**

```xml
!game caro
!game caro 10s
```
</p></details>

---

### game caro rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Caro game rules.*

**Aliases:**
`help, h, ruling, rule`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game caro rules
```
</p></details>

---

### game caro stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Caro game stats for a given user.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game caro stats
!game caro stats @User
```
</p></details>

---

### game caro top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Caro game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game caro top
```
</p></details>

---

## Group: game connect4
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Connect4 game for two players. Each player makes a move by writing a number representing a column in the grid where they wish to play. You can also specify the move time to make the game easier/harder.*

**Aliases:**
`connectfour, chain4, chainfour, c4, fourinarow, fourinaline, 4row, 4line, cfour`
**Guild only.**


**Overload 0:**
- (optional) \[`time span`\]: *Time for a move* (def: `None`)

**Examples:**

```xml
!game connect4
!game connect4 10s
```
</p></details>

---

### game connect4 rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Connect4 game rules.*

**Aliases:**
`help, h, ruling, rule`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game connect4 rules
```
</p></details>

---

### game connect4 stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Connect4 game stats for a given user.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game connect4 stats
!game connect4 stats @User
```
</p></details>

---

### game connect4 top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Connect4 game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game connect4 top
```
</p></details>

---

## Group: game duel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Duel game for two players which I will commentate.*

**Aliases:**
`fight, vs, d`
**Guild only.**


**Overload 0:**
- \[`member`\]: *Member*

**Examples:**

```xml
!game duel
!game duel Member
```
</p></details>

---

### game duel rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Duel game rules.*

**Aliases:**
`help, h, ruling, rule`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game duel rules
```
</p></details>

---

### game duel stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Duel game stats for a given user.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game duel stats
!game duel stats @User
```
</p></details>

---

### game duel top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Duel game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game duel top
```
</p></details>

---

## Group: game hangman
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Hangman game.*

**Aliases:**
`h, hang, hm`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game hangman
```
</p></details>

---

### game hangman rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Hangman game rules.*

**Aliases:**
`help, h, ruling, rule`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game hangman rules
```
</p></details>

---

### game hangman stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Hangman game stats for a given user.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game hangman stats
!game hangman stats @User
```
</p></details>

---

### game hangman top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Hangman game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game hangman top
```
</p></details>

---

### game leaderboard
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints global leaderboard for all games.*

**Aliases:**
`globalstats`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game leaderboard
```
</p></details>

---

## Group: game minesweeper
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Minesweeper game.*

**Aliases:**
`mines, ms`

**Overload 0:**
- (optional) \[`int`\]: *Rows* (def: `9`)
- (optional) \[`int`\]: *Columns* (def: `9`)
- (optional) \[`int`\]: *Bombs* (def: `10`)

**Examples:**

```xml
!game minesweeper
!game minesweeper 20 20 50
```
</p></details>

---

### game minesweeper rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Minesweeper game rules.*

**Aliases:**
`help, h, ruling, rule`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game minesweeper rules
```
</p></details>

---

## Group: game numberrace
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Number Race game.*

**Aliases:**
`nr, n, nunchi, numbers, numbersrace`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game numberrace
```
</p></details>

---

### game numberrace join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Joins a pending Number Race game.*

**Aliases:**
`+, compete, enter, j, <<, <`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game numberrace join
```
</p></details>

---

### game numberrace rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Number Race rules.*

**Aliases:**
`help, h, ruling, rule`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game numberrace rules
```
</p></details>

---

### game numberrace stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Number Race game stats for a given user.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game numberrace stats
!game numberrace stats @User
```
</p></details>

---

### game numberrace top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Number Race game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game numberrace top
```
</p></details>

---

## Group: game othello
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Othello game for two players. Each player makes a move by writing a pair of numbers representing coordinates in the grid where they wish to play. You can also specify the move time to make the game easier/harder.*

**Aliases:**
`reversi, oth, rev`
**Guild only.**


**Overload 0:**
- (optional) \[`time span`\]: *Time for a move* (def: `None`)

**Examples:**

```xml
!game othello
!game othello 10s
```
</p></details>

---

### game othello rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Othello game rules.*

**Aliases:**
`help, h, ruling, rule`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game othello rules
```
</p></details>

---

### game othello stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Othello game stats for a given user.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game othello stats
!game othello stats @User
```
</p></details>

---

### game othello top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Othello game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game othello top
```
</p></details>

---

## Group: game quiz
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a Quiz! Group call lists available quiz categories.*

**Aliases:**
`trivia, q`
**Guild only.**


**Overload 2:**
- \[`int`\]: *ID of the quiz category*
- (optional) \[`int`\]: *Amount of questions* (def: `10`)
- (optional) \[`int`\]: *Difficulty (0, 1, 2)* (def: `0`)

**Overload 1:**
- \[`string`\]: *ID of the quiz category*
- (optional) \[`int`\]: *Difficulty (0, 1, 2)* (def: `0`)
- (optional) \[`int`\]: *Amount of questions* (def: `10`)

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game quiz
!game quiz 10
!game quiz 10 10
```
</p></details>

---

### game quiz capitals
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new country capitals Quiz!*

**Aliases:**
`capitaltowns`
**Guild only.**


**Overload 0:**
- (optional) \[`int`\]: *Amount of questions* (def: `10`)

**Examples:**

```xml
!game quiz capitals
!game quiz capitals 10
```
</p></details>

---

### game quiz countries
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new country flag Quiz!*

**Aliases:**
`flags`
**Guild only.**


**Overload 0:**
- (optional) \[`int`\]: *Amount of questions* (def: `10`)

**Examples:**

```xml
!game quiz countries
!game quiz countries 10
```
</p></details>

---

### game quiz stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Quiz game stats.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game quiz stats
!game quiz stats @User
```
</p></details>

---

### game quiz top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Quiz game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game quiz top
```
</p></details>

---

## Group: game rps
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new rock-paper-scissors game against TheGodfather.*

**Aliases:**
`rockpaperscissors`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game rps
```
</p></details>

---

### game rps rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints rock-paper-scissors game rules.*

**Aliases:**
`help, h, ruling, rule`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game rps rules
```
</p></details>

---

## Group: game russianroulette
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Russian Roulette game.*

**Aliases:**
`rr, roulette, russianr`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game russianroulette
```
</p></details>

---

### game russianroulette join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Joins a pending Russian Roulette game.*

**Aliases:**
`+, compete, enter, j, <<, <`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game russianroulette join
```
</p></details>

---

### game russianroulette stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Russian Roulette game stats for a given user.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game russianroulette stats
!game russianroulette stats @User
```
</p></details>

---

### game russianroulette top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Russian Roulette game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game russianroulette top
```
</p></details>

---

### game stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints game stats for a given user.*

**Aliases:**
`s, st`

**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game stats
!game stats @User
```
</p></details>

---

## Group: game tictactoe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new TicTacToe game for two players. Each player makes a move by writing a number representing a field where they wish to place their mark. You can also specify the move time to make the game easier/harder.*

**Aliases:**
`ttt`
**Guild only.**


**Overload 0:**
- (optional) \[`time span`\]: *Time for a move* (def: `None`)

**Examples:**

```xml
!game tictactoe
!game tictactoe 10s
```
</p></details>

---

### game tictactoe rules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints TicTacToe game rules.*

**Aliases:**
`help, h, ruling, rule`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game tictactoe rules
```
</p></details>

---

### game tictactoe stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints TicTacToe game stats for a given user.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game tictactoe stats
!game tictactoe stats @User
```
</p></details>

---

### game tictactoe top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints TicTacToe game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game tictactoe top
```
</p></details>

---

## Group: game typingrace
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts a new Typing Race game.*

**Aliases:**
`tr, trace, typerace, typing, typingr`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game typingrace
```
</p></details>

---

### game typingrace join
<details><summary markdown='span'>Expand for additional information</summary><p>

*Joins a pending Typing Race game.*

**Aliases:**
`+, compete, enter, j, <<, <`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game typingrace join
```
</p></details>

---

### game typingrace stats
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Typing Race game stats for a given user.*

**Aliases:**
`s`
**Guild only.**


**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!game typingrace stats
!game typingrace stats @User
```
</p></details>

---

### game typingrace top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints Typing Race game leaderboard.*

**Aliases:**
`t, leaderboard`
**Guild only.**


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!game typingrace top
```
</p></details>

---

