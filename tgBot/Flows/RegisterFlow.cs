using Telegram.Bot;
using Telegram.Bot.Types;
using tgBot.Repositories.Interfaces;

namespace tgBot;


public enum RegisterFlowStates
{
    SetName,
    AddUser,
}
public class RegisterFlowData: IFlowData
{
    public Flows Flow => Flows.Register;
    public RegisterFlowStates States { get; set; }
}

public class RegisterFlow: IFlowUpdateHandler<RegisterFlowData>
{
    private readonly IUserRepository _userRepository;

    public RegisterFlow(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task OnFlowStarted(ITelegramBotClient botClient, UserData userData, Update update, CancellationToken cancellationToken)
    {
        if (userData.FlowData is not RegisterFlowData registerFlowData)
        {
            throw new InvalidOperationException();
        }

        registerFlowData.States = RegisterFlowStates.SetName;
    }

    public async Task<IFlowData?> HandleUpdate(ITelegramBotClient botClient, UserData userData, Update update, CancellationToken cancellationToken)
    {
        if (userData.FlowData is not RegisterFlowData registerFlowData)
        {
            throw new InvalidOperationException();
        }

        switch (registerFlowData.States)
        {
            case RegisterFlowStates.SetName:
                await botClient.SendTextMessageAsync(
                    chatId: userData.Id,
                    text: "Давайте регистрироваться! Введите имя",
                    cancellationToken: cancellationToken);
                registerFlowData.States = RegisterFlowStates.AddUser;
                break;
            case RegisterFlowStates.AddUser:
                if (await _userRepository.GetUserById(userData.Id) != null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: userData.Id,
                        text: "Вы уже регистрировались!",
                        cancellationToken: cancellationToken);
                    return new HelloFlow();
                }
                var name = update.Message.Text.ToLower();
                List<string> list = new();
                list.Add(name);
                //TODO ???????????
                var user = new User()
                {
                    Id = userData.Id,
                    Name = name,
                    CoinIdList = list 
                };
                var result = await _userRepository.AddUserAsync(user);
                if (result == null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: userData.Id,
                        text: "Что то пошло не так( Попробуйте снова",
                        cancellationToken: cancellationToken);
                    return new HelloFlow();
                }
                await botClient.SendTextMessageAsync(
                    chatId: userData.Id,
                    text: "Вы успешно зарегистрировались!",
                    cancellationToken: cancellationToken);
                break;
        }

        return null;
    }
}