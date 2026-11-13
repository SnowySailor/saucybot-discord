using Discord;
using SaucyBot.Common;
using SaucyBot.Library;
using SaucyBot.Library.Sites.Twitter;

namespace SaucyBot.Site.Twitter;

/// <summary>
/// Fork-only customization: photo tweets with more images than Discord will merge into one embed grid
/// (<see cref="Constants.MaximumEmbedsPerMessage"/>) are sent as a single Components V2 message with media galleries.
///
/// Kept in its own file so upstream changes to FxTwitter.cs merge cleanly; the only hook is the guard at the top of
/// <c>HandlePhoto</c>.
/// </summary>
public sealed partial class FxTwitterSite
{
    /// <summary>
    /// The maximum number of items Discord allows in a single Components V2 media gallery.
    /// </summary>
    private const int MaximumMediaGalleryItems = 10;

    /// <summary>
    /// Discord limits the combined length of all text across a Components V2 message to 4000 characters.
    /// </summary>
    private const int MaximumComponentsTextLength = 4_000;

    private static List<PhotoResult> SelectPhotos(IEnumerable<PhotoResult> results, bool mainTweetHasMedia)
    {
        var source = mainTweetHasMedia ? ResultSource.MainTweet : ResultSource.QuotedTweet;

        return results.Where(result => result.Source == source).ToList();
    }

    private ProcessResponse HandlePhotoGallery(FxTwitterTweet tweet, IEnumerable<PhotoResult> results, bool mainTweetHasMedia)
    {
        _logger.LogDebug("Processing as photo gallery");

        var photos = SelectPhotos(results, mainTweetHasMedia);

        var authorUrl = tweet.Author.Url ?? $"https://twitter.com/{tweet.Author.ScreenName}";
        var tweetUrl = tweet.Url ?? $"https://twitter.com/{tweet.Author.ScreenName}/status/{tweet.Id}";

        var header = $"**[{EscapeLinkText(tweet.Author.Name)} (@{tweet.Author.ScreenName})]({authorUrl})**";
        var description = GetTweetText(tweet);
        var stats = string.Join("    ", [
            $"💬 {tweet.Replies ?? 0:N0}",
            $"🔁 {tweet.Retweets ?? 0:N0}",
            $"❤️ {tweet.Likes ?? 0:N0}",
            $"👀 {tweet.Views ?? 0:N0}",
        ]);
        var footer = $"-# [Twitter](<{tweetUrl}>) · <t:{tweet.CreatedTimestamp}:f>";

        var textLength = header.Length + description.Length + stats.Length + footer.Length;

        if (textLength >= MaximumComponentsTextLength)
        {
            return new ProcessResponse
            {
                Text = $"https://fxtwitter.com/{tweet.Author.ScreenName}/status/{tweet.Id}",
                IsNsfw = tweet.PossiblySensitive,
            };
        }

        var headerTexts = new List<string> { header };

        if (description is not "")
        {
            headerTexts.Add(description);
        }

        var container = new ContainerBuilder()
            .WithAccentColor(this.Color);

        if (tweet.Author.AvatarUrl is not null)
        {
            var section = new SectionBuilder()
                .WithAccessory(new ThumbnailBuilder(new UnfurledMediaItemProperties(tweet.Author.AvatarUrl)));

            foreach (var text in headerTexts)
            {
                section.WithTextDisplay(text);
            }

            container.WithSection(section);
        }
        else
        {
            foreach (var text in headerTexts)
            {
                container.WithTextDisplay(text);
            }
        }

        foreach (var chunk in photos.Chunk(MaximumMediaGalleryItems))
        {
            container.WithMediaGallery(chunk.Select(photo => GetOriginalResolutionPhotoUrl(photo.Photo.Url)));
        }

        container
            .WithTextDisplay(stats)
            .WithTextDisplay(footer);

        return new ProcessResponse(
            components: new ComponentBuilderV2().WithContainer(container).Build(),
            nsfw: tweet.PossiblySensitive
        );
    }

    private static string EscapeLinkText(string text)
    {
        return Helper.EscapeDiscordMarkdown(text)
            .Replace("[", @"\[")
            .Replace("]", @"\]");
    }
}
