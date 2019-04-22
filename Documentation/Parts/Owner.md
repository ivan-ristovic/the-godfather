# Module: Owner

## Group: owner
<details><summary markdown='span'>Expand for additional information</summary><p>

*Hidden.*

*Owner-only bot administration commands.*

**Aliases:**
`admin, o`

</p></details>

---

### owner announce
<details><summary markdown='span'>Expand for additional information</summary><p>

*Send a message to all guilds the bot is in.*

**Owner-only.**

**Aliases:**
`a, ann`

**Arguments:**

`[string...]` : *Message to send.*

**Examples:**

```xml
!owner announce SPAM SPAM
```
</p></details>

---

## Group: owner blockedchannels
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate blocked channels. Bot will not listen for commands in blocked channels or react (either with text or emoji) to messages inside.*

**Privileged users only.**

**Aliases:**
`bc, blockedc, blockchannel, bchannels, bchannel, bchn`

**Overload 2:**

`[channel...]` : *Channels to block.*

**Overload 1:**

`[string]` : *Reason (max 60 chars).*

`[channel...]` : *Channels to block.*

**Overload 0:**

`[channel]` : *Channels to block.*

`[string...]` : *Reason (max 60 chars).*

</p></details>

---

### owner blockedchannels add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add channel to blocked channels list.*

**Privileged users only.**

**Aliases:**
`+, a, block, <, <<, +=`

**Overload 2:**

`[channel...]` : *Channels to block.*

**Overload 1:**

`[string]` : *Reason (max 60 chars).*

`[channel...]` : *Channels to block.*

**Overload 0:**

`[channel]` : *Channel to block.*

`[string...]` : *Reason (max 60 chars).*

**Examples:**

```xml
!owner blockedchannels add #channel
!owner blockedchannels add #channel Some reason
!owner blockedchannels add 123123123123123
!owner blockedchannels add #channel 123123123123123
!owner blockedchannels add "This is some reason" #channel 123123123123123
```
</p></details>

---

### owner blockedchannels delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove channel from blocked channels list.*

**Privileged users only.**

**Aliases:**
`-, remove, rm, del, unblock, >, >>, -=`

**Arguments:**

`[channel...]` : *Channels to unblock.*

**Examples:**

```xml
!owner blockedchannels delete #channel
!owner blockedchannels delete 123123123123123
!owner blockedchannels delete #channel1 #channel2 123123123123123
```
</p></details>

---

### owner blockedchannels list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all blocked channels.*

**Privileged users only.**

**Aliases:**
`ls, l, print`

</p></details>

---

## Group: owner blockedusers
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate blocked users. Bot will not allow blocked users to invoke commands and will not react (either with text or emoji) to their messages.*

**Privileged users only.**

**Aliases:**
`bu, blockedu, blockuser, busers, buser, busr`

**Overload 2:**

`[user...]` : *Users to block.*

**Overload 1:**

`[string]` : *Reason (max 60 chars).*

`[user...]` : *Users to block.*

**Overload 0:**

`[user]` : *Users to block.*

`[string...]` : *Reason (max 60 chars).*

</p></details>

---

### owner blockedusers add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add users to blocked users list.*

**Privileged users only.**

**Aliases:**
`+, a, block, <, <<, +=`

**Overload 2:**

`[user...]` : *Users to block.*

**Overload 1:**

`[string]` : *Reason (max 60 chars).*

`[user...]` : *Users to block.*

**Overload 0:**

`[user]` : *Users to block.*

`[string...]` : *Reason (max 60 chars).*

**Examples:**

```xml
!owner blockedusers add @Someone
!owner blockedusers add @Someone Troublemaker
!owner blockedusers add 123123123123123
!owner blockedusers add @Someone 123123123123123
!owner blockedusers add "This is some reason" @Someone 123123123123123
```
</p></details>

---

### owner blockedusers delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove users from blocked users list.*

**Privileged users only.**

**Aliases:**
`-, remove, rm, del, unblock, >, >>, -=`

**Arguments:**

`[user...]` : *Users to unblock.*

**Examples:**

```xml
!owner blockedusers delete @Someone
!owner blockedusers delete 123123123123123
!owner blockedusers delete @Someone 123123123123123
```
</p></details>

---

### owner blockedusers list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all blocked users.*

**Privileged users only.**

**Aliases:**
`ls, l, print`

</p></details>

---

### owner botavatar
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set bot avatar.*

**Owner-only.**

**Aliases:**
`setbotavatar, setavatar`

**Arguments:**

`[URL]` : *URL.*

**Examples:**

```xml
!owner botavatar http://someimage.png
```
</p></details>

---

### owner botname
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set bot name.*

**Owner-only.**

**Aliases:**
`setbotname, setname`

**Arguments:**

`[string...]` : *New name.*

**Examples:**

```xml
!owner botname TheBotfather
```
</p></details>

---

### owner clearlog
<details><summary markdown='span'>Expand for additional information</summary><p>

*Clear bot logs.*

**Owner-only.**

**Aliases:**
`clearlogs, deletelogs, deletelog`

</p></details>

---

## Group: owner commands
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate bot commands in runtime.*

**Owner-only.**

**Aliases:**
`cmds, cmd`

</p></details>

---

### owner commands add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a new command.*

**Owner-only.**

**Aliases:**
`+, a, <, <<, +=`

**Arguments:**

`[string...]` : *Code to evaluate.*

**Examples:**

```xml
!owner commands add \`\`\`[Command("test")] public Task TestAsync(CommandContext ctx) => ctx.RespondAsync("Hello world!");\`\`\`
```
</p></details>

---

### owner commands delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove an existing command.*

**Owner-only.**

**Aliases:**
`-, remove, rm, del, >, >>, -=`

**Arguments:**

`[string...]` : *Command to remove.*

**Examples:**

```xml
!owner commands delete say
```
</p></details>

---

### owner commands list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all privileged users.*

**Owner-only.**

**Aliases:**
`ls, l, print`

</p></details>

---

### owner dbquery
<details><summary markdown='span'>Expand for additional information</summary><p>

*Execute SQL query on the bot database.*

**Owner-only.**

**Aliases:**
`sql, dbq, q`

**Overload 0:**

`[string...]` : *SQL Query.*

**Examples:**

```xml
!owner dbquery 
!owner dbquery SELECT * FROM gf.msgcount;
```
</p></details>

---

### owner eval
<details><summary markdown='span'>Expand for additional information</summary><p>

*Evaluates a snippet of C# code, in context. Surround the code in the code block.*

**Owner-only.**

**Aliases:**
`compile, run, e, c, r`

**Arguments:**

`[string...]` : *Code to evaluate.*

**Examples:**

```xml
!owner eval \`\`\`await Context.RespondAsync("Hello!");\`\`\`
```
</p></details>

---

### owner filelog
<details><summary markdown='span'>Expand for additional information</summary><p>

*Toggle writing to log file.*

**Owner-only.**

**Aliases:**
`setfl, fl, setfilelog`

**Arguments:**

(optional) `[boolean]` : *Enable?* (def: `True`)

**Examples:**

```xml
!owner filelog 
!owner filelog on
!owner filelog off
```
</p></details>

---

### owner generatecommandlist
<details><summary markdown='span'>Expand for additional information</summary><p>

*Generates a markdown command-list. You can also provide a folder for the output.*

**Owner-only.**

**Aliases:**
`cmdlist, gencmdlist, gencmds, gencmdslist`

**Arguments:**

(optional) `[string...]` : *File path.* (def: `None`)

**Examples:**

```xml
!owner generatecommandlist 
!owner generatecommandlist Temp/blabla.md
```
</p></details>

---

### owner leaveguilds
<details><summary markdown='span'>Expand for additional information</summary><p>

*Leaves the given guilds.*

**Owner-only.**

**Aliases:**
`leave, gtfo`

**Arguments:**

`[unsigned long...]` : *Guild ID list.*

**Examples:**

```xml
!owner leaveguilds 337570344149975050
!owner leaveguilds 337570344149975050 201315884709576708
```
</p></details>

---

### owner log
<details><summary markdown='span'>Expand for additional information</summary><p>

*Upload the bot log file or add a remark to it.*

**Owner-only.**

**Aliases:**
`getlog, remark, rem`

**Overload 1:**

(optional) `[boolean]` : *Bypass current configuration and search file anyway?* (def: `False`)

**Overload 0:**

`[string]` : *Log level.*

`[string...]` : *Remark.*

**Examples:**

```xml
!owner log 
!owner log debug Hello world!
```
</p></details>

---

## Group: owner privilegedusers
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate privileged users. Privileged users can invoke commands marked with RequirePrivilegedUsers permission.*

**Owner-only.**

**Aliases:**
`pu, privu, privuser, pusers, puser, pusr`

**Overload 0:**

`[user...]` : *Users to grant privilege to.*

</p></details>

---

### owner privilegedusers add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add users to privileged users list.*

**Owner-only.**

**Aliases:**
`+, a, <, <<, +=`

**Arguments:**

`[user...]` : *Users to grant privilege to.*

**Examples:**

```xml
!owner privilegedusers add add @Someone
!owner privilegedusers add add @Someone @SomeoneElse
```
</p></details>

---

### owner privilegedusers delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove users from privileged users list.*

**Owner-only.**

**Aliases:**
`-, remove, rm, del, >, >>, -=`

**Arguments:**

`[user...]` : *Users to revoke privileges from.*

**Examples:**

```xml
!owner privilegedusers delete remove @Someone
!owner privilegedusers delete remove 123123123123123
!owner privilegedusers delete remove @Someone 123123123123123
```
</p></details>

---

### owner privilegedusers list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all privileged users.*

**Owner-only.**

**Aliases:**
`ls, l, print`

</p></details>

---

### owner sendmessage
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a message to a user or channel.*

**Privileged users only.**

**Aliases:**
`send, s`

**Arguments:**

`[string]` : *u/c (for user or channel.)*

`[unsigned long]` : *User/Channel ID.*

`[string...]` : *Message.*

**Examples:**

```xml
!owner sendmessage u 303463460233150464 Hi to user!
!owner sendmessage c 120233460278590414 Hi to channel!
```
</p></details>

---

### owner shutdown
<details><summary markdown='span'>Expand for additional information</summary><p>

*Triggers the dying in the vineyard scene (power off the bot).*

**Privileged users only.**

**Aliases:**
`disable, poweroff, exit, quit`

**Overload 1:**

`[time span]` : *Time until shutdown.*

**Examples:**

```xml
!owner shutdown 
!owner shutdown 10s
```
</p></details>

---

## Group: owner statuses
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bot status manipulation. If invoked without command, either lists or adds status depending if argument is given.*

**Owner-only.**

**Aliases:**
`status, botstatus, activity, activities`

**Overload 0:**

`[ActivityType]` : *Activity type (Playing/Watching/Streaming/ListeningTo).*

`[string...]` : *Status.*

</p></details>

---

### owner statuses add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a status to running status queue.*

**Owner-only.**

**Aliases:**
`+, a, <, <<, +=`

**Arguments:**

`[ActivityType]` : *Activity type (Playing/Watching/Streaming/ListeningTo).*

`[string...]` : *Status.*

**Examples:**

```xml
!owner statuses add Playing CS:GO
!owner statuses add Streaming on Twitch
```
</p></details>

---

### owner statuses delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove status from running queue.*

**Owner-only.**

**Aliases:**
`-, remove, rm, del, >, >>, -=`

**Arguments:**

`[int]` : *Status ID.*

**Examples:**

```xml
!owner statuses delete 2
```
</p></details>

---

### owner statuses list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all bot statuses.*

**Owner-only.**

**Aliases:**
`ls, l, print`

</p></details>

---

### owner statuses set
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set status to given string or status with given index in database. This sets rotation to false.*

**Owner-only.**

**Aliases:**
`s`

**Overload 1:**

`[ActivityType]` : *Activity type (Playing/Watching/Streaming/ListeningTo).*

`[string...]` : *Status.*

**Overload 0:**

`[int]` : *Status ID.*

**Examples:**

```xml
!owner statuses set Playing with fire
!owner statuses set 5
```
</p></details>

---

### owner statuses setrotation
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set automatic rotation of bot statuses.*

**Owner-only.**

**Aliases:**
`sr, setr`

**Arguments:**

(optional) `[boolean]` : *Enabled?* (def: `True`)

**Examples:**

```xml
!owner statuses setrotation 
!owner statuses setrotation off
```
</p></details>

---

### owner sudo
<details><summary markdown='span'>Expand for additional information</summary><p>

*Executes a command as another user.*

**Privileged users only.**

**Aliases:**
`execas, as`

**Arguments:**

`[member]` : *Member to execute as.*

`[string...]` : *Command text to execute.*

**Examples:**

```xml
!owner sudo @Someone rate
```
</p></details>

---

### owner toggleignore
<details><summary markdown='span'>Expand for additional information</summary><p>

*Toggle bot's reaction to commands.*

**Privileged users only.**

**Aliases:**
`ti`

</p></details>

---

### owner update
<details><summary markdown='span'>Expand for additional information</summary><p>

*Update and restart the bot.*

**Owner-only.**

**Aliases:**
`upd, u`

</p></details>

---

