using Moq;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using TwitchLib.Client.Models.Builders;

namespace WrBotTests.Mocks
{
    public class TwitchClientMock : Mock<ITwitchClient>
    {
        public void RaiseChatCommandReceived(string channel, string command, params string[] parameters)
        {
            var chatMessage = ChatMessageBuilder.Create()
                .WithChannel(channel)
                .Build();

            this.RaiseChatCommandReceived(
                ChatCommandBuilder.Create()
                    .WithCommandText(command)
                    .WithArgumentsAsList(parameters)
                    .WithChatMessage(chatMessage)
                    .Build()
            );
        }

        public void RaiseChatCommandReceived(string channel, bool isModerator, bool isBroadcaster, string command, params string[] parameters)
        {
            var chatMessage = ChatMessageBuilder.Create()
                .WithChannel(channel)
                .WithIsModerator(isModerator)
                .WithIsBroadcaster(isBroadcaster)
                .Build();

            this.RaiseChatCommandReceived(
                ChatCommandBuilder.Create()
                    .WithCommandText(command)
                    .WithArgumentsAsList(parameters)
                    .WithChatMessage(chatMessage)
                    .Build()
            );
        }

        public void RaiseChatCommandReceived(string channel, string command, ChatMessage message)
        {
            this.RaiseChatCommandReceived(
                ChatCommandBuilder.Create()
                    .WithCommandText(command)
                    .WithChatMessage(message)
                    .Build()
            );
        }

        public void RaiseChatCommandReceived(ChatCommand chatCommand)
        {
            this.Raise(x => x.OnChatCommandReceived += null, new OnChatCommandReceivedArgs
            {
                Command = chatCommand
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