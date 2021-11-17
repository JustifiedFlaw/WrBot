using RestEase;

namespace SrcFacade
{
    public static class SrcApi
    {
        public static ISrcApi Connect()
        {
            return RestClient.For<ISrcApi>("https://www.speedrun.com/api");
        }
    }
}