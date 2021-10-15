using System;
using Xunit;

namespace WrBotTests
{
    public class TimeSpanFormaterTests
    {
        [Theory]
        [InlineData(0, 0, 0, 0, 0, "000ms")]
        [InlineData(0, 0, 0, 0, 987, "987ms")]
        [InlineData(0, 0, 0, 21, 987, "21s 987ms")]
        [InlineData(0, 0, 54, 21, 987, "54m 21s 987ms")]
        [InlineData(0, 2, 54, 21, 987, "02h 54m 21s 987ms")]
        [InlineData(1, 2, 54, 21, 987, "01d 02h 54m 21s 987ms")]
        public void When_Formating_TimeSpan_Then_Formated_Correctly(int days, int hours, int minutes, int seconds, int milliseconds, string expected)
        {
            var timeSpan = new TimeSpan(days, hours, minutes, seconds, milliseconds);
            Assert.Equal(expected, timeSpan.Format());
        }
    }
}