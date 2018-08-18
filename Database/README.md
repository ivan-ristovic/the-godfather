## Creating a database for TheGodfather

You will need PostgreSQL version >= 9.6 (download the latest version). During the installation, note the port used (default is 5432).

### Steps
1. Create a new database (by default a postgres database is created during installation, you can use it also but i recommend creating a new one). Name it as you wish but note the name you chose.
2. Run the ``schema.sql`` inside the newly created database. This will create a new schema called ``gf``.
3. Now, configure the bot by entering the data into ``Resources/config.json``. Example:
```
"db-config": {
	"hostname": "localhost",
	"port": 5432,
	"database": "gfdb",
	"username": "nab",
	"password": "pw"
}
```
4. Run the bot.