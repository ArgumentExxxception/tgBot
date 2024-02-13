using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using tgBot;
using tgBot.Repositories;
using tgBot.Repositories.Interfaces;
using tgBot.Services;
using tgBot.Services.Interfaces;
using TwitterSharp.Client;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddSingleton<ITelegramBotClient,TelegramBotClient>(
    x=> new TelegramBotClient( x.GetRequiredService<IConfiguration>().GetValue<string>("Token") ?? throw new InvalidOperationException()));
builder.Services.AddSingleton<TwitterClient>(
    x=> new TwitterClient(x.GetRequiredService<IConfiguration>().GetValue<string>("TwitterToken")));
builder.Services.AddHostedService<TelegramBotReceivingService>();
builder.Services.AddSingleton<TelegramRouter>();
builder.Services.AddEntityFrameworkNpgsql().
    AddDbContext<Context>(opt =>
    {
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection"));
    });
builder.Services.AddHangfire(config =>
        config.UsePostgreSqlStorage(
        builder.Configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<FlowManager>();
builder.Services.AddHostedService<TwitterApiClient>();
builder.Services.AddHostedService<ScheduleService>();
builder.Services.AddScoped<IFlowUpdateHandler<CoinFlowData>,BaseCoinInfoFlow>();
builder.Services.AddScoped<IFlowUpdateHandler<HelloFlow>,HelloHandler>();
builder.Services.AddScoped<IFlowUpdateHandler<DateCoinInfoFlowData>,DateCoinInfoFlow>();
builder.Services.AddScoped<IFlowUpdateHandler<PriceChangeFlowData>,PriceChangeSubscriptionFlow>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFlowUpdateHandler<RegisterFlowData>, RegisterFlow>();
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<ICoinGeckoApiClient, CoinGeckoApiClient>();
builder.Services.AddScoped<IUserNotificationService, NotificationService>();
var app = builder.Build();
await app.RunAsync();