# How to subscribe to the bot for your channel
1. Go to [https://twitch.tv/What_is_wr_bot](https://twitch.tv/What_is_wr_bot)
2. Click on Chat
3. Type !joinme in the chat
4. You should get a message like "Joined [channel]"

# How to use What_is_wr_bot
| Command | Description | Access |
| ----------- | ----------- | ----------- |
| !wr | Says world record for current game and default category |
| !wr game | Says world record for a specific game and default category |
| !wr game category | Says world record for a specific game and category |
| !wr "game name" | Quotes can be used for names with multiple words |
| !wr -setgame "game name" | Sets the default game permanently | Broadcaster and moderators |
| !wr -setcategory category | Sets the default category permanently | Broadcaster and moderators |
| !pb | Says personal best fo broadcaster for current game and default category |
| !pb runner | Says personal best for a specific runner for current game and default category |
| !pb runner game category | Says personal best for a specific runner, game and category |
| !pb -setrunner runner | Sets the default runner permanently | Broadcaster and moderators |
| !pb -setgame game | Sets the default game permanently | Broadcaster and moderators |
| !pb -setcategory "category name" | Sets the default category permanently | Broadcaster and moderators |
| !wr -reset | Resets the defaults permanently | Broadcaster and moderators |

# Mandatory Environment Variable 
WrBot needs environment variables. You can set these variable like any other in your OS, or in VS Code you can add an *env* section in your *configurations* section of *launch.json*

### Contents of launch.json
````json
{
    "version": "0.2.0",
    "configurations": [
        
        {
            "name": "...",
            "console": "internalConsole",
            "...": "...",
            "env": {
                "WRBOT_DB_HOST": "database server host",
                "WRBOT_DB_PORT": "database server port",
                "WRBOT_DB_DB": "database name",
                "WRBOT_DB_USER": "databse user",
                "WRBOT_DB_PWRD": "password",
                "WRBOT_BOT_NAME": "twitch channel of bot",
                "WRBOT_BOT_CID": "client id",
                "WRBOT_BOT_TKN": "access token",
                "WRBOT_BOT_KA": "milliseconds for keep twitch connection alive timer",
                "WRBOT_BOT_KC": "milliseconds for keep channel connection alive timer"
            }
        },
        {
            "name": ".NET Core Attach",
            "type": "coreclr",
            "request": "attach"
        }
    ]
}
````