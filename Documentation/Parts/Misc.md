# Module: Misc
*This module contains miscellaneous commands which do not fall into any of the other categories but do not deserve their own group since they are unique or not extensible.*


## 8ball
<details><summary markdown='span'>Expand for additional information</summary><p>

*Do you ponder the mysteries of our world? Ask the Almighty 8Ball whatever you want! But beware, because the truth can sometimes hurt...*


**Aliases:**
`8b`

**Overload 0:**
- \[`string...`\]: *A question for the Almighty 8Ball*

**Examples:**

```xml
!8ball Some string here
```
</p></details>

---

## Group: birthday
<details><summary markdown='span'>Expand for additional information</summary><p>

*Birthday notifications commands. Group call either lists or adds a new birthday notification(s).*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`birthdays, bday, bd, bdays`

**Overload 3:**
- \[`user`\]: *Birthday boy/girl*

**Overload 2:**
- (optional) \[`channel`\]: *Channel for birthday notifications* (def: `None`)

**Overload 1:**
- \[`user`\]: *Birthday boy/girl*
- \[`channel`\]: *Channel for birthday notifications*
- (optional) \[`string`\]: *Birthday date* (def: `None`)

**Overload 0:**
- \[`user`\]: *Birthday boy/girl*
- \[`string`\]: *Birthday date*
- (optional) \[`channel`\]: *Channel for birthday notifications* (def: `None`)

**Examples:**

```xml
!birthday @User
!birthday #my-text-channel
!birthday @User #my-text-channel 13.10.2000
```
</p></details>

---

### birthday add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new birthday notification for a given user in the given channel (defaults to current channel) at a given date (defaults to current date).*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`

**Overload 1:**
- \[`user`\]: *Birthday boy/girl*
- (optional) \[`channel`\]: *Channel for birthday notifications* (def: `None`)
- (optional) \[`string`\]: *Birthday date* (def: `None`)

**Overload 0:**
- \[`user`\]: *Birthday boy/girl*
- \[`string`\]: *Birthday date*
- (optional) \[`channel`\]: *Channel for birthday notifications* (def: `None`)

**Examples:**

```xml
!birthday add @User
!birthday add @User #my-text-channel
!birthday add @User #my-text-channel 13.10.2000
```
</p></details>

---

### birthday delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes registered birthday notification(s) for a given user or in a given channels.*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`

**Overload 1:**
- \[`user`\]: *Birthday boy/girl*

**Overload 0:**
- \[`channel`\]: *Channel for birthday notifications*

**Examples:**

```xml
!birthday delete @User
!birthday delete #my-text-channel
```
</p></details>

---

### birthday deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all registered birthday notification(s).*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!birthday deleteall
```
</p></details>

---

### birthday list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists birthday notifications for a given user or a given channel.*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`print, show, view, ls, l, p`

**Overload 1:**
- \[`user`\]: *Birthday boy/girl*

**Overload 0:**
- (optional) \[`channel`\]: *Channel for birthday notifications* (def: `None`)

**Examples:**

```xml
!birthday list @User
!birthday list #my-text-channel
```
</p></details>

---

### birthday listall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all birthday notifications for this guild.*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`printall, showall, lsa, la, pa`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!birthday listall
```
</p></details>

---

## coinflip
<details><summary markdown='span'>Expand for additional information</summary><p>

*Flips a coin!*


**Aliases:**
`coin, flip`

**Overload 0:**
- (optional) \[`int`\]: *Reciprocal coinflip ratio* (def: `1`)

**Examples:**

```xml
!coinflip
!coinflip 5
```
</p></details>

---

## dice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Throws a dice!*


**Aliases:**
`die, roll`

**Overload 0:**
- (optional) \[`int`\]: *How many sides will the dice have?* (def: `6`)

**Examples:**

```xml
!dice
!dice 5
```
</p></details>

---

## Group: grant
<details><summary markdown='span'>Expand for additional information</summary><p>

*Requests to grant the sender a certain object (role for example).*

**Guild only.**


**Aliases:**
`give`

**Overload 0:**
- \[`role...`\]: *Roles to add*

**Examples:**

```xml
!grant @Role
!grant SampleName
```
</p></details>

---

### grant nickname
<details><summary markdown='span'>Expand for additional information</summary><p>

*Grants you a given nickname.*

**Guild only.**

**Requires bot permissions:**
`Manage nicknames`

**Aliases:**
`nick, name, n`

**Overload 0:**
- \[`string...`\]: *New name*

**Examples:**

```xml
!grant nickname SampleName
```
</p></details>

---

### grant role
<details><summary markdown='span'>Expand for additional information</summary><p>

*Grants you a role from this guild's self-assignable roles list.*

**Guild only.**

**Requires bot permissions:**
`Manage roles`

**Aliases:**
`roles, rl, r`

**Overload 0:**
- \[`role...`\]: *Roles to add*

**Examples:**

```xml
!grant role @Role
```
</p></details>

---

## Group: help
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows the help embed.*


**Aliases:**
`h, ?, ??, ???, man`

**Overload 2:**

*No arguments.*

**Overload 1:**
- \[`ModuleType`\]: *Command module*

**Overload 0:**
- \[`string...`\]: *Command name*

**Examples:**

```xml
!help
!help Administration
!help sample command
```
</p></details>

---

## Group: insult
<details><summary markdown='span'>Expand for additional information</summary><p>

*Writes an insult targeting a user. Alternatively, you can provide text to use as insult target.*


**Aliases:**
`burn, ins, roast`

**Overload 1:**
- (optional) \[`user`\]: *User* (def: `None`)

**Overload 0:**
- \[`string...`\]: *Insult target*

**Examples:**

```xml
!insult @User
!insult Some string here
```
</p></details>

---

## invite
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get or create an instant invite link for the current guild.*


**Aliases:**
`getinvite, inv`

**Overload 0:**
- (optional) \[`time span`\]: *Invite expiry time* (def: `None`)

**Examples:**

```xml
!invite
!invite 1d
```
</p></details>

---

## leave
<details><summary markdown='span'>Expand for additional information</summary><p>

*Makes me leave the guild.*

**Requires permissions:**
`Administrator`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!leave
```
</p></details>

---

## leet
<details><summary markdown='span'>Expand for additional information</summary><p>

*Wr1t3s g1v3n tEx7 1n p5EuDo 1337sp34k.*


**Aliases:**
`l33t, 1337`

**Overload 0:**
- \[`string...`\]: *Text to repeat*

**Examples:**

```xml
!leet Some string here
```
</p></details>

---

## linux
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints a customizable GNU/Linux interjection.*


**Overload 0:**
- (optional) \[`string`\]: *Replacement string* (def: `None`)
- (optional) \[`string`\]: *Replacement string* (def: `None`)

**Examples:**

```xml
!linux
!linux GNU Windows
```
</p></details>

---

## Group: meme
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate guild memes. Group call retrieves a meme from this guild's meme list by it's name or a random one if the name isn't provided.*

**Guild only.**


**Aliases:**
`memes, mm`

**Overload 1:**

*No arguments.*

**Overload 0:**
- \[`string...`\]: *Meme name*

**Examples:**

```xml
!meme
!meme Some string here
```
</p></details>

---

### meme add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new guild meme with a given name and an image (provided via URL or attachment).*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`

**Overload 1:**
- \[`string`\]: *Meme name*
- (optional) \[`URL`\]: *Meme URL* (def: `None`)

**Overload 0:**
- \[`URL`\]: *Meme URL*
- \[`string...`\]: *Meme name*

**Examples:**

```xml
!meme add Some string here
!meme add Some string here http://some-image-url.com/image.png
```
</p></details>

---

### meme create
<details><summary markdown='span'>Expand for additional information</summary><p>

*Creates a new meme from template and top/bottom text(s).*

**Guild only.**

**Requires permissions:**
`Use embeds`

**Aliases:**
`maker, c, make, m`

**Overload 0:**
- \[`string`\]: *Meme template*
- \[`string`\]: *Top text*
- \[`string`\]: *Bottom text*

**Examples:**

```xml
!meme create aag Some string here Some string here
```
</p></details>

---

### meme delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes a meme from guild meme list.*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`

**Overload 0:**
- \[`string...`\]: *Meme name*

**Examples:**

```xml
!meme delete Some string here
```
</p></details>

---

### meme deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes all guild memes.*

**Guild only.**

**Requires user permissions:**
`Administrator`

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da, cl, -a, --, >>>`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!meme deleteall
```
</p></details>

---

### meme list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all guild memes.*

**Guild only.**


**Aliases:**
`print, show, view, ls, l, p`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!meme list
```
</p></details>

---

### meme templates
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all available meme templates.*

**Guild only.**


**Aliases:**
`template, ts, t`

**Overload 0:**
- (optional) \[`string...`\]: *Meme template* (def: `None`)

**Examples:**

```xml
!meme templates
!meme templates aag
```
</p></details>

---

## penis
<details><summary markdown='span'>Expand for additional information</summary><p>

*An accurate measurement.*


**Aliases:**
`size, length, manhood, dick, dicksize`

**Overload 1:**
- \[`member...`\]: *Member(s)*

**Overload 0:**
- \[`user...`\]: *User(s)*

**Examples:**

```xml
!penis
!penis @User
!penis @User @User @User
```
</p></details>

---

## penisbros
<details><summary markdown='span'>Expand for additional information</summary><p>

*Finds members with same `penis` command result as the given user.*

**Guild only.**


**Aliases:**
`sizebros, lengthbros, manhoodbros, dickbros, cockbros`

**Overload 1:**
- \[`member`\]: *Member*

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!penisbros
!penisbros @User
```
</p></details>

---

## ping
<details><summary markdown='span'>Expand for additional information</summary><p>

*Pings the bot.*


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!ping
```
</p></details>

---

## prefix
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets command prefix.*

**Guild only.**

**Requires permissions:**
`Administrator`

**Aliases:**
`setprefix, pref, setpref`

**Overload 0:**
- (optional) \[`string`\]: *New command prefix* (def: `None`)

**Examples:**

```xml
!prefix
!prefix .
```
</p></details>

---

## Group: random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Randomization commands - choices, raffles etc.*


**Aliases:**
`rnd, rand`

</p></details>

---

### random choice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Chooses a random option from a comma separated option list.*


**Aliases:**
`select, choose`

**Overload 0:**
- \[`string...`\]: *Choice list (separated by comma)*

**Examples:**

```xml
!random choice option 1, option 2, option 3
```
</p></details>

---

### random raffle
<details><summary markdown='span'>Expand for additional information</summary><p>

*Choose a user from the online members list optionally belonging to a given role.*


**Aliases:**
`chooseuser`

**Overload 0:**
- (optional) \[`role`\]: *Role* (def: `None`)

**Examples:**

```xml
!random raffle
!random raffle @Role
```
</p></details>

---

## Group: rank
<details><summary markdown='span'>Expand for additional information</summary><p>

*User rank management. Group call prints user rank info.*


**Aliases:**
`ranks, ranking, level, xp`

**Overload 1:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Overload 0:**
- (optional) \[`user`\]: *User* (def: `None`)

**Examples:**

```xml
!rank
!rank @User
```
</p></details>

---

### rank add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a custom name for a given rank in this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`register, rename, mv, newname, reg, a, +, +=, <<, <, <-, <=`

**Overload 0:**
- \[`short`\]: *Rank*
- \[`string...`\]: *Rank name*

**Examples:**

```xml
!rank add 5 SampleName
```
</p></details>

---

### rank delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes a custom name for a given rank in this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`

**Overload 0:**
- \[`short`\]: *Rank*

**Examples:**

```xml
!rank delete 5
```
</p></details>

---

### rank list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print all customized rank names for this guild.*


**Aliases:**
`print, show, view, ls, l, p`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!rank list
```
</p></details>

---

### rank top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints guild rank leaderboard*


**Overload 0:**

*No arguments.*

**Examples:**

```xml
!rank top
```
</p></details>

---

### rank topglobal
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints global rank leaderboard*


**Aliases:**
`bestglobally, globallystrongest, globaltop, topg, gtop, globalbest, bestglobal`

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!rank topglobal
```
</p></details>

---

## rate
<details><summary markdown='span'>Expand for additional information</summary><p>

*A very accurate personality measurement.*

**Requires bot permissions:**
`Attach files`

**Aliases:**
`score, graph, rating`

**Overload 1:**
- \[`member...`\]: *Member(s)*

**Overload 0:**
- \[`user...`\]: *User(s)*

**Examples:**

```xml
!rate
!rate @User
!rate @User @User @User
```
</p></details>

---

## report
<details><summary markdown='span'>Expand for additional information</summary><p>

*Report an issue with the bot.*


**Overload 0:**
- \[`string...`\]: *Issue to report*

**Examples:**

```xml
!report Report message containing the detailed issue description
```
</p></details>

---

## Group: revoke
<details><summary markdown='span'>Expand for additional information</summary><p>

*Requests to revoke a certain object (role for example) from the sender.*

**Guild only.**


**Aliases:**
`take`

**Overload 0:**
- \[`role...`\]: *Roles to remove*

**Examples:**

```xml
!revoke @Role
```
</p></details>

---

### revoke role
<details><summary markdown='span'>Expand for additional information</summary><p>

*Revokes a role from this guild's self-assignable roles list.*

**Guild only.**

**Requires bot permissions:**
`Manage roles`

**Aliases:**
`rl, r`

**Overload 0:**
- \[`role...`\]: *Roles to remove*

**Examples:**

```xml
!revoke role @Role
```
</p></details>

---

## say
<details><summary markdown='span'>Expand for additional information</summary><p>

*Echo! Echo! Echo!*


**Aliases:**
`repeat, echo`

**Overload 0:**
- \[`string...`\]: *Text to repeat*

**Examples:**

```xml
!say Some string here
```
</p></details>

---

## simulate
<details><summary markdown='span'>Expand for additional information</summary><p>

*Simulates a message from another user.*

**Guild only.**


**Aliases:**
`sim`

**Overload 0:**
- (optional) \[`member`\]: *Member* (def: `None`)

**Examples:**

```xml
!simulate
!simulate @User
```
</p></details>

---

## Group: starboard
<details><summary markdown='span'>Expand for additional information</summary><p>

*Guild starboard commands. Starboard is a channel where member-voted messages will be saved. Something like pins however the starboard is not limited to one channel, has no limits on the number of starred messages, and it is automatically updated. If a message has more than a number of specified emoji reactions, it will be saved in the starboard. The number of reactions before saving is referred to as *sensitivity*, whereas the emoji is referred to as a *star* (star being the default emoji). Both are customziable. Group call shows current starboard information for the guild or enables/disables starboard in given channel and using given star emoji.*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`star, sb`

**Overload 1:**
- \[`boolean`\]: *Enable?*
- \[`emoji`\]: *Emoji*
- \[`channel`\]: *Channel*
- (optional) \[`int`\]: *Sensitivity* (def: `None`)

**Overload 1:**
- \[`boolean`\]: *Enable?*
- \[`channel`\]: *Emoji*
- (optional) \[`emoji`\]: *Channel* (def: `None`)
- (optional) \[`int`\]: *Sensitivity* (def: `None`)

**Overload 0:**

*No arguments.*

**Examples:**

```xml
!starboard
!starboard Yes/No #my-text-channel
!starboard Yes/No #my-text-channel :emoji:
!starboard Yes/No #my-text-channel :emoji: 5
```
</p></details>

---

### starboard channel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sets starboard channel.*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`chn, setchannel, setchn, setc, location`

**Overload 0:**
- \[`channel`\]: *Channel*

**Examples:**

```xml
!starboard channel
```
</p></details>

---

### starboard sensitivity
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets starboard sensitivity.*

**Guild only.**

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setsensitivity, setsens, sens, s`

**Overload 0:**
- (optional) \[`int`\]: *Sensitivity* (def: `None`)

**Examples:**

```xml
!starboard sensitivity
!starboard sensitivity 5
```
</p></details>

---

## time
<details><summary markdown='span'>Expand for additional information</summary><p>

*Shows time in a given timezone or localized guild time if timezone is not provided.*


**Aliases:**
`t`

**Overload 0:**
- (optional) \[`string...`\]: *IANA/Windows/Rails timezone ID* (def: `None`)

**Examples:**

```xml
!time
!time CET
```
</p></details>

---

## tts
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a TTS message.*

**Requires permissions:**
`Send TTS messages`

**Overload 0:**
- \[`string...`\]: *Text to repeat*

**Examples:**

```xml
!tts Some string here
```
</p></details>

---

## unleet
<details><summary markdown='span'>Expand for additional information</summary><p>

*Attempts to translate message from leetspeak.*


**Aliases:**
`unl33t`

**Overload 0:**
- \[`string...`\]: *Text to repeat*

**Examples:**

```xml
!unleet Some string here
```
</p></details>

---

