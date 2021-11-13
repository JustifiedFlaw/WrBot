using System;
using System.Collections.Generic;
using System.Drawing;
using Moq;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;

namespace WrBotTests.Mocks
{
    public class TwitchClientMock : Mock<ITwitchClient>
    {
        public void InvokeMessageReceived(string channel, string message, bool isModerator = false)
        {
            this.Raise(x => x.OnMessageReceived += null, new OnMessageReceivedArgs
            {
                ChatMessage = new ChatMessage("Bot", "", "", "", "", Color.AliceBlue, null, message, UserType.Viewer, channel, "", false, 0, "", false, isModerator, false, false,false, false, false, Noisy.True, "", "", new List<KeyValuePair<string, string>>(), null, 0, 0)
            });
        }

        public void ThenSendMessageCalled(string channel, string message)
        {
            this.Verify(x => x.SendMessage(channel, message, It.IsAny<bool>()));
        }
    }
}