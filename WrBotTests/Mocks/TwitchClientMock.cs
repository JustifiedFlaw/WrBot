using Moq;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using TwitchLib.Client.Models.Builders;

namespace WrBotTests.Mocks
{
    public class TwitchClientMock : Mock<ITwitchClient>
    {
        public void RaiseMessageReceived(string channel, string message, bool isModerator = false, bool isBroadcaster = false)
        {
            this.RaiseMessageReceived(
                ChatMessageBuilder.Create()
                    .WithChannel(channel)
                    .WithMessage(message)
                    .WithIsModerator(isModerator)
                    .WithIsBroadcaster(isBroadcaster)
                    .Build()
            );
        }

        public void RaiseMessageReceived(ChatMessage chatMessage)
        {
            this.Raise(x => x.OnMessageReceived += null, new OnMessageReceivedArgs
            {
                ChatMessage = chatMessage
            });
        }

        public void ThenSendMessageCalled(string channel, string message)
        {
            this.Verify(x => x.SendMessage(channel, message, It.IsAny<bool>()));
        }

        public void ThenJoinedChannel(string channel)
        {
            this.Verify(x => x.JoinChannel(channel, It.IsAny<bool>()));
        }

        public void ThenNotJoinedChannel(string channel)
        {
            this.Verify(x => x.JoinChannel(channel, It.IsAny<bool>()), Times.Never);
        }

        public void ThenLeaveChannel(string channel)
        {
            this.Verify(x => x.LeaveChannel(channel));
        }

        public void ThenNotLeaveChannel(string channel)
        {
            this.Verify(x => x.LeaveChannel(channel), Times.Never);
        }
    }
}