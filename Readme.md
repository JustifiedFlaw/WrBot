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
WrBot needs a WRBOTCFG environment variable. You can set this variable in like any other in your OS, or in VS Code you can add an *env* section in your *configurations* section of *launch.json*

### Contents of launch.json
````json
{
    "version": "0.2.0",
    "configurations": [
        
        {
            "name": "...",
            "console": "internalConsole",
            "...": "..."
            "env": {
                "WRBOTCFG": "..."
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

### Contents of WRBOTCFG
````json
{
   "DatabaseSettings": {
    "Host": "host",
    "Port": 5432,
    "Database": "db",
    "User": "user",
    "Password": "password"
  },
   "BotSettings":{
      "BotName":"ChannelOfTheBot",
      "ClientId":"",
      "AccessToken":"",
      "KeepAlive": 300000,
      "KeepChannelsConnected": 15000
   }
}
````
