using Moq;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models.Builders;

namespace WrBotTests.Mocks
{
    public class TwitchClientMock : Mock<ITwitchClient>
    {
        public void RaiseMessageReceived(string channel, string message, bool isModerator = false, bool isBroadcaster = false)
        {
            this.Raise(x => x.OnMessageReceived += null, new OnMessageReceivedArgs
            {
                ChatMessage = ChatMessageBuilder.Create()
                    .WithChannel(channel)
                    .WithMessage(message)
                    .WithIsModerator(isModerator)
                    .WithIsBroadcaster(isBroadcaster)
                    .Build()
            });
        }

        public void ThenSendMessageCalled(string channel, string message)
        {
            this.Verify(x => x.SendMessage(channel, message, It.IsAny<bool>()));
        }
    }
}