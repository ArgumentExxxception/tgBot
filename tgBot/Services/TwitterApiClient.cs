using System.Collections.Immutable;
using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using tgBot.Repositories.Interfaces;
using tgBot.Services.Interfaces;
using TwitterSharp.Client;
using TwitterSharp.Request;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using Expression = TwitterSharp.Rule.Expression;

namespace tgBot.Services;

public class TwitterApiClient: IHostedService
{
    private readonly TwitterClient _twitterApi;
    private readonly ITelegramBotClient _botClient;
    private readonly IUserRepository _userRepository;

    public TwitterApiClient(TwitterClient twitterApi, ITelegramBotClient botClient, IUserRepository userRepository)
    {
        _twitterApi = twitterApi;
        _botClient = botClient;
        _userRepository = userRepository;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var request = new StreamRequest(
            Expression.Author("binance").Or(
                Expression.Author("VitalikButerin"),
                Expression.Author("cz_binance"),
                Expression.Author("CoinMarketCap"),
                Expression.Author("benbybit")),
            "Halolive"
        );
        await _twitterApi.AddTweetStreamAsync(request);
        _ = Task.Run(async () =>
        {
            await _twitterApi.NextTweetStreamAsync(async (tweet) =>
                {
                    var msg = $""""
                               {tweet.Author}:
                               {tweet.Text}
                               (Rules: {string.Join(',', tweet.MatchingRules.Select(x => x.Tag))})
                               """";
                    var users = await _userRepository.GetAllUsers();
                    foreach (var user in users)
                    {
                         await _botClient.SendTextMessageAsync(
                            chatId: user.Id,
                            text: msg,
                            cancellationToken: cancellationToken);
                    }
                },
                new TweetSearchOptions
                {
                    UserOptions = Array.Empty<UserOption>(),
                });
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _twitterApi.CancelTweetStream();
        _twitterApi.Dispose();
    }
}