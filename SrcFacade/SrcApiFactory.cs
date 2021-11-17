using RestEase;

namespace SrcFacade
{
    public static class SrcApiFactory
    {
        public static ISrcApi Connect()
        {
            return RestClient.For<ISrcApi>("https://www.speedrun.com/api");
        }
    }
}