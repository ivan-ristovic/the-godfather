# Module: Owner
*This module contains owner-only commands.*


## Group: blockedchannels
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*Blocked channels control commands.*

**Aliases:**
`bc, blockedc, blockchannel, bchannels, bchannel, bchn`
**Privileged users only.**


**Overload 2:**

[`channel...`]: *Entities to block*

**Overload 1:**

[`string`]: *Reason for the action*

[`channel...`]: *Entities to block*

**Overload 0:**

[`channel`]: *Entities to block*

[`string...`]: *Reason for the action*

**Examples:**

```xml
!blockedchannels
!blockedchannels #my-text-channel
```
</p></details>

---

### blockedchannels add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Blocks channels from interacting with the bot.*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Privileged users only.**


**Overload 2:**

[`channel...`]: *Entities to block*

**Overload 1:**

[`string`]: *Reason for the action*

[`channel...`]: *Entities to block*

**Overload 0:**

[`channel`]: *Entities to block*

[`string...`]: *Reason for the action*

**Examples:**

```xml
!blockedchannels add #my-text-channel
!blockedchannels add #my-text-channel Because I can!
```
</p></details>

---

### blockedchannels delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes channels from bot block list.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Privileged users only.**


**Arguments:**

[`channel...`]: *Entities to unblock*

**Examples:**

```xml
!blockedchannels delete #my-text-channel
```
</p></details>

---

### blockedchannels list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all blocked channels.*

**Aliases:**
`print, show, view, ls, l, p`
**Privileged users only.**


**Examples:**

```xml
!blockedchannels list
```
</p></details>

---

## Group: blockedguilds
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*Blocked guilds control commands.*

**Aliases:**
`bg, blockedg, blockguild, bguilds, bguild, bgld`
**Privileged users only.**


**Overload 2:**

[`guild...`]: *Entities to block*

**Overload 1:**

[`string`]: *Reason for the action*

[`guild...`]: *Entities to block*

**Overload 0:**

[`guild`]: *Entities to block*

[`string...`]: *Reason for the action*

**Examples:**

```xml
!blockedguilds
!blockedguilds Some Guild
```
</p></details>

---

### blockedguilds add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Blocks guilds from interacting with the bot.*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Privileged users only.**


**Overload 2:**

[`guild...`]: *Entities to block*

**Overload 1:**

[`string`]: *Reason for the action*

[`guild...`]: *Entities to block*

**Overload 0:**

[`guild`]: *Entities to block*

[`string...`]: *Reason for the action*

**Examples:**

```xml
!blockedguilds add Some Guild
!blockedguilds add Some Guild Because I can!
```
</p></details>

---

### blockedguilds delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes guilds from bot block list.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Privileged users only.**


**Arguments:**

[`guild...`]: *Entities to unblock*

**Examples:**

```xml
!blockedguilds delete Some Guild
```
</p></details>

---

### blockedguilds list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all blocked guilds.*

**Aliases:**
`print, show, view, ls, l, p`
**Privileged users only.**


**Examples:**

```xml
!blockedguilds list
```
</p></details>

---

## Group: blockedusers
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*Blocked users control commands.*

**Aliases:**
`bu, blockedu, blockuser, busers, buser, busr`
**Privileged users only.**


**Overload 2:**

[`user...`]: *Entities to block*

**Overload 1:**

[`string`]: *Reason for the action*

[`user...`]: *Entities to block*

**Overload 0:**

[`user`]: *Entities to block*

[`string...`]: *Reason for the action*

**Examples:**

```xml
!blockedusers
!blockedusers @User
```
</p></details>

---

### blockedusers add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Blocks users from interacting with the bot.*

**Aliases:**
`register, reg, a, +, +=, <<, <, <-, <=`
**Privileged users only.**


**Overload 2:**

[`user...`]: *Entities to block*

**Overload 1:**

[`string`]: *Reason for the action*

[`user...`]: *Entities to block*

**Overload 0:**

[`user`]: *Entities to block*

[`string...`]: *Reason for the action*

**Examples:**

```xml
!blockedusers add @User
!blockedusers add @User Because I can!
```
</p></details>

---

### blockedusers delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes users from bot block list.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Privileged users only.**


**Arguments:**

[`user...`]: *Entities to unblock*

**Examples:**

```xml
!blockedusers delete @User
```
</p></details>

---

### blockedusers list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all blocked users.*

**Aliases:**
`print, show, view, ls, l, p`
**Privileged users only.**


**Examples:**

```xml
!blockedusers list
```
</p></details>

---

## Group: commands
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*Bot command manipulation during runtime.*

**Aliases:**
`cmds, cmd`
**Owner-only.**

**Examples:**

```xml
!commands
```
</p></details>

---

### commands add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a new bot command.*

**Aliases:**
`register, reg, new, a, +, +=, <<, <, <-, <=`
**Owner-only.**

**Arguments:**

[`string...`]: *C# code snippet in a markdown code block*

**Examples:**

```xml
!commands add ```cs
[Command("test")]
public Task Test(CommandContext ctx) => ctx.RespondAsync("Hello");
```
```
</p></details>

---

### commands delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes existing bot command.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Owner-only.**

**Arguments:**

[`string...`]: *Command name*

**Examples:**

```xml
!commands delete sample command
```
</p></details>

---

### commands list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all bot commands.*

**Aliases:**
`print, show, view, ls, l, p`
**Owner-only.**

**Examples:**

```xml
!commands list
```
</p></details>

---

## Group: owner
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*Commands restricted to bot owner(s).*

**Aliases:**
`admin, o`
</p></details>

---

### owner announce
<details><summary markdown='span'>Expand for additional information</summary><p>

*Send a message to all guilds the bot is in.*

**Aliases:**
`ann`
**Owner-only.**

**Arguments:**

[`string...`]: *Announcement message*

**Examples:**

```xml
!owner announce Some important announcement!
```
</p></details>

---

### owner avatar
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sets the bot avatar.*

**Aliases:**
`setavatar, setbotavatar, profilepic, a`
**Owner-only.**

**Arguments:**

[`URL`]: *Image URL*

**Examples:**

```xml
!owner avatar http://some-image-url.com/image.png
```
</p></details>

---

### owner dbquery
<details><summary markdown='span'>Expand for additional information</summary><p>

*Queries the bot database using given SQL query or uploaded SQL file.*

**Aliases:**
`sql, dbq, q, query`
**Owner-only.**

**Overload 0:**

[`string...`]: *SQL query*

**Examples:**

```xml
!owner dbquery SELECT * FROM gf.<DATABASE_NAME>
```
</p></details>

---

### owner eval
<details><summary markdown='span'>Expand for additional information</summary><p>

*Evaluates a snippet of C# code, in context.*

**Aliases:**
`evaluate, compile, run, e, c, r, exec`
**Owner-only.**

**Arguments:**

[`string...`]: *C# code snippet in a markdown code block*

**Examples:**

```xml
!owner eval ```cs
[Command("test")]
public Task Test(CommandContext ctx) => ctx.RespondAsync("Hello");
```
```
</p></details>

---

### owner generatecommandlist
<details><summary markdown='span'>Expand for additional information</summary><p>

*Generates bot documentation in markdown ready for GitHub.*

**Aliases:**
`gendocs, generatecommandslist, docs, cmdlist, gencmdlist, gencmds, gencmdslist`
**Owner-only.**

**Arguments:**

(optional) [`string...`]: *Output folder* (def: `None`)

**Examples:**

```xml
!owner generatecommandlist
```
</p></details>

---

### owner leaveguilds
<details><summary markdown='span'>Expand for additional information</summary><p>

*Generates bot documentation in markdown ready for GitHub.*

**Aliases:**
`leave, gtfo`
**Owner-only.**

**Overload 1:**

[`guild...`]: *Guild names or IDs*

**Overload 0:**

[`unsigned long...`]: *Guild names or IDs*

**Examples:**

```xml
!owner leaveguilds Some Guild
!owner leaveguilds 361119455792594954
```
</p></details>

---

### owner log
<details><summary markdown='span'>Expand for additional information</summary><p>

*Logs a given remark or uploads bot log file if remark is not given.*

**Aliases:**
`getlog, remark, rem`
**Owner-only.**

**Overload 1:**

(optional) [`boolean`]: *Bypass current bot configuration?* (def: `False`)

**Overload 0:**

[`LogEventLevel`]: *Log event level*

[`string...`]: *Log message*

**Examples:**

```xml
!owner log
!owner log Information
!owner log Some string here
```
</p></details>

---

### owner name
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sets the bot name.*

**Aliases:**
`botname, setbotname, setname`
**Owner-only.**

**Arguments:**

[`string...`]: *New name*

**Examples:**

```xml
!owner name SampleName
```
</p></details>

---

### owner sendmessage
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a message to user or channel.*

**Aliases:**
`send, s`
**Privileged users only.**


**Arguments:**

[`string`]: *`u` (User) or `c` (Channel)*

[`unsigned long`]: *ID*

[`string...`]: *Message to send*

**Examples:**

```xml
!owner sendmessage u
!owner sendmessage 361119455792594954
!owner sendmessage Sample message
```
</p></details>

---

### owner shutdown
<details><summary markdown='span'>Expand for additional information</summary><p>

*Powers off the bot.*

**Aliases:**
`disable, poweroff, exit, quit`
**Privileged users only.**


**Overload 1:**

[`time span`]: *Time until shutdown*

(optional) [`int`]: *Process exit code* (def: `0`)

**Overload 0:**

(optional) [`int`]: *Process exit code* (def: `0`)

**Examples:**

```xml
!owner shutdown
!owner shutdown 10s 5
!owner shutdown 10s
```
</p></details>

---

### owner sudo
<details><summary markdown='span'>Expand for additional information</summary><p>

*Executes command as another user.*

**Aliases:**
`execas, as`
**Guild only.**

**Privileged users only.**


**Arguments:**

[`member`]: *Member*

[`string...`]: *Full command call with arguments*

**Examples:**

```xml
!owner sudo Member
!owner sudo sample command
```
</p></details>

---

### owner toggleignore
<details><summary markdown='span'>Expand for additional information</summary><p>

*Toggle bot listening status.*

**Aliases:**
`ti`
**Privileged users only.**


**Examples:**

```xml
!owner toggleignore
```
</p></details>

---

### owner uptime
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints bot uptime information.*

**Privileged users only.**


**Examples:**

```xml
!owner uptime
```
</p></details>

---

## Group: privilegedusers
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*Commands to manage privileged users. Privileged users have permissions to execute some sensitive bot commands.*

**Aliases:**
`pu, privu, privuser, pusers, puser, pusr`
**Owner-only.**

**Overload 0:**

[`user...`]: *User(s)*

**Examples:**

```xml
!privilegedusers
```
</p></details>

---

### privilegedusers add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds given user(s) to privileged users list.*

**Aliases:**
`register, reg, new, a, +, +=, <<, <, <-, <=`
**Owner-only.**

**Arguments:**

[`user...`]: *User(s)*

**Examples:**

```xml
!privilegedusers add @User
```
</p></details>

---

### privilegedusers delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes given user(s) from privileged users list.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Owner-only.**

**Arguments:**

[`user...`]: *User(s)*

**Examples:**

```xml
!privilegedusers delete @User
```
</p></details>

---

### privilegedusers list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all privileged users.*

**Aliases:**
`print, show, view, ls, l, p`
**Owner-only.**

**Examples:**

```xml
!privilegedusers list
```
</p></details>

---

## Group: status
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*Manipulates bot statuses. Group call either lists all statuses or adds a new status.*

**Aliases:**
`statuses, botstatus, activity, activities`
**Owner-only.**

**Overload 0:**

[`ActivityType`]: *Activity type (Playing/Watching/Streaming/ListeningTo)*

[`string...`]: *Bot status*

**Examples:**

```xml
!status
!status Playing Some Game
```
</p></details>

---

### status add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Adds a new status to the status list.*

**Aliases:**
`register, reg, new, a, +, +=, <<, <, <-, <=`
**Owner-only.**

**Arguments:**

[`ActivityType`]: *Activity type (Playing/Watching/Streaming/ListeningTo)*

[`string...`]: *Bot status*

**Examples:**

```xml
!status add Playing Some Game
```
</p></details>

---

### status delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes a status from the status list.*

**Aliases:**
`unregister, remove, rm, del, d, -, -=, >, >>, ->, =>`
**Owner-only.**

**Arguments:**

[`int...`]: *Bot status ID*

**Examples:**

```xml
!status delete 5
```
</p></details>

---

### status list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Lists all currently registered statuses.*

**Aliases:**
`print, show, view, ls, l, p`
**Owner-only.**

**Examples:**

```xml
!status list
```
</p></details>

---

### status set
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disables automatic rotation of bot statuses and sets the currents status by ID or explicit string until status rotation is enabled again.*

**Aliases:**
`s`
**Owner-only.**

**Overload 1:**

[`ActivityType`]: *Activity type (Playing/Watching/Streaming/ListeningTo)*

[`string...`]: *Bot status*

**Overload 0:**

[`int`]: *Bot status ID*

**Examples:**

```xml
!status set 5
!status set Playing Some Game
```
</p></details>

---

### status setrotation
<details><summary markdown='span'>Expand for additional information</summary><p>

*Enables or disables automatic rotation of bot statuses.*

**Aliases:**
`sr, setr, rotate`
**Owner-only.**

**Arguments:**

(optional) [`boolean`]: *Enable?* (def: `True`)

**Examples:**

```xml
!status setrotation Yes/No
```
</p></details>

---

