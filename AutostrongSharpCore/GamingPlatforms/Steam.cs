using AutostrongSharpCore.Infrastructure;

namespace AutostrongSharpCore.GamingPlatforms;

public class Steam : IGamingPlatform
{
    public const string StoreBaseUrl = "https://store.steampowered.com/app";
    public void OpenStoreProductPage(string appId) => $"{StoreBaseUrl}/{appId}".OpenUrl();
}