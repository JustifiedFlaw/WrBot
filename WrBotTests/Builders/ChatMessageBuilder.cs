using System;
using System.Collections.Generic;
using System.Drawing;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Models;

namespace WrBotTests.Builders
{
    public class ChatMessageBuilder
    {
        public static ChatMessageBuilder Init() => new ChatMessageBuilder();

        string BotName = RandomStringBuilder.Init().Build();
        string Channel = RandomStringBuilder.Init().Build();
        string Message = RandomStringBuilder.Init().Build();
        string Username = RandomStringBuilder.Init().Build();

        public ChatMessageBuilder WithBotName(string botName)
        {
            this.BotName = botName;
            return this;
        }

        public ChatMessageBuilder WithChannel(string channel)
        {
            this.Channel = channel;
            return this;
        }

        public ChatMessageBuilder WithMessage(string message)
        {
            this.Message = message;
            return this;
        }

        public ChatMessageBuilder WithUsername(string username)
        {
            this.Username = username;
            return this;
        }

        public ChatMessage Build()
        {
            return new ChatMessage(
                botUsername: this.BotName,
                channel: this.Channel,
                message: this.Message,
                userId: "",
                userName: this.Username,
                displayName: "", 
                colorHex: "",
                color: Color.AliceBlue,
                emoteSet: null,
                userType: UserType.Viewer,
                id: "",
                isSubscriber: false,
                subscribedMonthCount: 0,
                roomId: "",
                isTurbo: false,
                isModerator: false,
                isMe: false,
                isBroadcaster: false,
                isVip: false,
                isPartner: false,
                isStaff: false,
                noisy: Noisy.True,
                rawIrcMessage: "",
                emoteReplacedMessage: "",
                badges: new List<KeyValuePair<string, string>>(),
                cheerBadge: null,
                bits: 0,
                bitsInDollars: 0);
        }
    }
}