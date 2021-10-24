using RestEase;

namespace TwitchRestEase
{
    public static class TwitchApi
    {
        public static ITwitchApi Connect(string clientId, string accessToken)
        {
            var twitchApi = RestClient.For<ITwitchApi>("https://api.twitch.tv");
            twitchApi.ClientId = clientId;
            twitchApi.Authorization = "Bearer " + accessToken;

            return twitchApi;
        }       
    }
}