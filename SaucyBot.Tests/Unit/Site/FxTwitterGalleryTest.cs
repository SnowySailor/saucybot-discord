using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SaucyBot.Library.Sites.Twitter;
using SaucyBot.Services;
using SaucyBot.Site;
using SaucyBot.Site.Twitter;
using Xunit;

namespace SaucyBot.Tests.Unit.Site;

/// <summary>
/// Tests for the fork-only media gallery customization in FxTwitter.Gallery.cs.
/// </summary>
public class FxTwitterGalleryTest
{
    [Fact]
    public async Task FourOrFewerPhotosUseRegularEmbeds()
    {
        var result = await ProcessTweetWithPhotos(4);

        Assert.Null(result.Components);
        Assert.Equal(4, result.Embeds.Count);
    }

    [Fact]
    public async Task MoreThanFourPhotosUseASingleGalleryMessage()
    {
        var result = await ProcessTweetWithPhotos(5);

        Assert.Empty(result.Embeds);

        var container = Assert.IsType<ContainerComponent>(Assert.Single(result.Components!.Components));
        var gallery = Assert.Single(container.Components.OfType<MediaGalleryComponent>());
        Assert.Equal(5, gallery.Items.Count);
    }

    [Fact]
    public async Task SplitsLargePhotoSetsIntoMultipleGalleriesInOneMessage()
    {
        var result = await ProcessTweetWithPhotos(16);

        var container = Assert.IsType<ContainerComponent>(Assert.Single(result.Components!.Components));
        var galleries = container.Components.OfType<MediaGalleryComponent>().ToList();
        Assert.Equal([10, 6], galleries.Select(gallery => gallery.Items.Count));

        var messages = await MessageManager.PartitionMessages(result);
        Assert.IsType<ComponentsV2Message>(Assert.Single(messages));
    }

    private static async Task<ProcessResponse> ProcessTweetWithPhotos(int count)
    {
        var logger = Substitute.For<ILogger<FxTwitterSite>>();
        var config = new ConfigurationBuilder()
            .Build();
        var client = Substitute.For<IFxTwitterClient>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient());

        var photos = Enumerable.Range(1, count)
            .Select(index => new FxTwitterPhoto("photo", $"https://example.com/image{index}.jpg", 800, 600))
            .ToList();

        var tweet = new FxTwitterTweet(
            Id: "123456789",
            Url: "https://twitter.com/testuser/status/123456789",
            Text: "Test tweet content",
            CreatedAt: "2024-01-01T00:00:00Z",
            CreatedTimestamp: 1704067200,
            Author: new FxTwitterAuthor("123", "Test User", "testuser", "https://example.com/avatar.jpg", "https://twitter.com/testuser", null, null),
            Replies: 5,
            Retweets: 10,
            Likes: 20,
            Views: 100,
            Bookmarks: null,
            Color: null,
            TwitterCard: "summary",
            Language: null,
            Source: "web",
            PossiblySensitive: false,
            ReplyingToScreenName: null,
            ReplyingToStatusId: null,
            Translation: null,
            QuotedTweet: null,
            Poll: null,
            Media: new FxTwitterMedia(photos, null)
        );

        client
            .GetTweet(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
            .Returns(new FxTwitterResponse(200, "OK", tweet));

        var site = new FxTwitterSite(logger, config.FxTwitterOptions(), client, httpClientFactory);

        var match = site.Pattern.Matches("https://twitter.com/testuser/status/123456789")[0];

        var result = await site.Process(new ProcessRequest(match));

        Assert.NotNull(result);

        return result;
    }
}
