using System.Collections.Generic;
using Moq;
using TwitchRestEase;
using TwitchRestEase.Models;

namespace WrBotTests.Mocks
{
    public class TwitchApiMock : Mock<ITwitchApi>
    {
        public void WhenStream(string channel, Stream streamInfo)
        {
            this.Setup(x => x.GetStreams(channel))
                .ReturnsAsync(new Streams
                {
                    Data = new List<Stream> { streamInfo }
                });
        }
    }
}