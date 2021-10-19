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

# Creating the appsettings.json file
For security reasons the mandatory appsettings.json file is not provided in the source code. If you want to run this bot on your machine you will need to create it. Here is the structure it should have:
````json
{
   "BotSettings":{
      "BotName":"ChannelOfTheBot",
      "ClientId":"",
      "AccessToken":"",
      "Channels":[
         {
            "Name":"ChannelOfTheBot",
            "Runner":{
               "Enabled":false,
               "Value":null
            },
            "Game":{
               "Enabled":false,
               "Value":null
            },
            "Category":{
               "Enabled":false,
               "Value":null
            }
         },
         {
            "Name":"OtherChannel",
            "Runner":{
               "Enabled":false,
               "Value":null
            },
            "Game":{
               "Enabled":true,
               "Value":"Celeste"
            },
            "Category":{
               "Enabled":true,
               "Value":"All red berries"
            }
         }
      ]
   }
}
````