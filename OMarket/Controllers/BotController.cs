using Microsoft.AspNetCore.Mvc;

using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Bot;
using OMarket.Domain.Interfaces.Application.Services.Distributor;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OMarket.Controllers
{
    [ApiController]
    [Route("api/bot")]
    public class BotController : ControllerBase
    {
        private readonly IUpdateManager _updateManager;

        private readonly IDistributorService _distributor;

        private readonly II18nService _i18n;

        private readonly ITelegramBotClient _client;

        private readonly string _secretToken;

        public BotController(
                IUpdateManager updateManager,
                IDistributorService distributor,
                II18nService i18n,
                IBotService bot
            )
        {
            _updateManager = updateManager;
            _distributor = distributor;
            _i18n = i18n;
            _client = bot.Client;

            string? jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("JWT_KEY", "The JWT key string environment variable is not set.");
            }

            _secretToken = jwtKey;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Result = "Bot is running..." });
        }

        [HttpPost]
        public async Task<IActionResult> Post(Update update, CancellationToken token)
        {
            if (!Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var secretTokenValue))
            {
                return Unauthorized();
            }

            string secretToken = secretTokenValue.ToString();

            if (string.IsNullOrEmpty(secretToken) || !secretToken.Equals(_secretToken, StringComparison.Ordinal))
            {
                return Unauthorized();
            }

            try
            {
                _updateManager.Update = update;

                await _distributor.Distribute(token);

                return Ok();
            }
            catch (OperationCanceledException)
            {
                return Ok();
            }
            catch (TelegramException ex)
            {
                await HandleTelegramExceptionAsync(ex);
                return Ok();
            }
            catch (Exception)
            {
                await HandleExceptionAsync();
                return Ok();
            }
        }

        private async Task HandleTelegramExceptionAsync(TelegramException exception)
        {
            if (_updateManager.Update is null)
            {
                return;
            }

            string exceptionMessage = _i18n.T("exception_main");

            if (!string.IsNullOrEmpty(exception.ExceptionMessage))
            {
                exceptionMessage = _i18n.T(exception.ExceptionMessage);
            }

            if (exception.Buttons is not null)
            {
                if (_updateManager.Update.Type == UpdateType.Message)
                {
                    if (_updateManager.Update.Message is not null)
                    {
                        await _client.SendTextMessageAsync(
                        chatId: _updateManager.Update.Message.Chat.Id,
                        text: exceptionMessage,
                        replyMarkup: exception.Buttons,
                        parseMode: ParseMode.Html
                      );
                    }
                }
                else if (_updateManager.Update.Type == UpdateType.CallbackQuery)
                {
                    if (_updateManager.Update.CallbackQuery is not null)
                    {
                        if (_updateManager.Update.CallbackQuery.Message is not null)
                        {
                            await _client.SendTextMessageAsync(
                              chatId: _updateManager.Update.CallbackQuery.Message.Chat.Id,
                              text: exceptionMessage,
                              replyMarkup: exception.Buttons,
                              parseMode: ParseMode.Html
                            );

                            await _client.AnswerCallbackQueryAsync(_updateManager.Update.CallbackQuery.Id);
                        }
                    }
                }

                return;
            }

            if (_updateManager.Update.Type == UpdateType.Message)
            {
                if (_updateManager.Update.Message is not null)
                {
                    await _client.SendTextMessageAsync(_updateManager.Update.Message.Chat.Id, exceptionMessage);
                }
            }
            else if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                if (_updateManager.Update.CallbackQuery is not null)
                {
                    if (_updateManager.Update.CallbackQuery.Message is not null)
                    {
                        await _client.SendTextMessageAsync(_updateManager.Update.CallbackQuery.Message.Chat.Id, exceptionMessage);
                    }
                    await _client.AnswerCallbackQueryAsync(_updateManager.Update.CallbackQuery.Id);
                }
            }
        }

        private async Task HandleExceptionAsync()
        {
            if (_updateManager.Update is null)
            {
                return;
            }

            string exceptionMessage = _i18n.T("exception_main");

            if (_updateManager.Update.Type == UpdateType.Message)
            {
                if (_updateManager.Update.Message is not null)
                {
                    await _client.SendTextMessageAsync(_updateManager.Update.Message.Chat.Id, exceptionMessage);
                }
            }
            else if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                if (_updateManager.Update.CallbackQuery is not null)
                {
                    if (_updateManager.Update.CallbackQuery.Message is not null)
                    {
                        await _client.SendTextMessageAsync(_updateManager.Update.CallbackQuery.Message.Chat.Id, exceptionMessage);
                    }
                    await _client.AnswerCallbackQueryAsync(_updateManager.Update.CallbackQuery.Id);
                }
            }
        }
    }
}