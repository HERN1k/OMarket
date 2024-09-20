using Microsoft.AspNetCore.Http;

using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Bot;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ITelegramBotClient _client;

        public ExceptionHandlingMiddleware(RequestDelegate next, IBotService bot)
        {
            _next = next;
            _client = bot.Client;
        }

        public async Task InvokeAsync(HttpContext context, IUpdateManager updateManager, II18nService i18n)
        {
            try
            {
                await _next(context);
            }
            catch (TelegramException ex)
            {
                await HandleTelegramExceptionAsync(context, ex, updateManager, i18n);
                return;
            }
            catch (Exception)
            {
                await HandleExceptionAsync(context, updateManager, i18n);
                return;
            }
        }

        private async Task HandleTelegramExceptionAsync(HttpContext context, TelegramException exception, IUpdateManager updateManager, II18nService i18n)
        {
            if (updateManager.Update is null)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"status\":\"error\",\"message\":\"Exception\"}");
                return;
            }

            string exceptionMessage = i18n.T("exception_main");

            if (!string.IsNullOrEmpty(exception.ExceptionMessage))
            {
                exceptionMessage = i18n.T(exception.ExceptionMessage);
            }

            if (exception.Buttons is not null)
            {
                if (updateManager.Update.Type == UpdateType.Message)
                {
                    if (updateManager.Update.Message is not null)
                    {
                        await _client.SendTextMessageAsync(
                        chatId: updateManager.Update.Message.Chat.Id,
                        text: exceptionMessage,
                        replyMarkup: exception.Buttons,
                        parseMode: ParseMode.Html
                      );
                    }
                }
                else if (updateManager.Update.Type == UpdateType.CallbackQuery)
                {
                    if (updateManager.Update.CallbackQuery is not null)
                    {
                        if (updateManager.Update.CallbackQuery.Message is not null)
                        {
                            await _client.SendTextMessageAsync(
                              chatId: updateManager.Update.CallbackQuery.Message.Chat.Id,
                              text: exceptionMessage,
                              replyMarkup: exception.Buttons,
                              parseMode: ParseMode.Html
                            );

                            await _client.AnswerCallbackQueryAsync(updateManager.Update.CallbackQuery.Id);
                        }
                    }
                }

                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"status\":\"error\",\"message\":\"Exception\"}");

                return;
            }

            if (updateManager.Update.Type == UpdateType.Message)
            {
                if (updateManager.Update.Message is not null)
                {
                    await _client.SendTextMessageAsync(updateManager.Update.Message.Chat.Id, exceptionMessage);
                }
            }
            else if (updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                if (updateManager.Update.CallbackQuery is not null)
                {
                    if (updateManager.Update.CallbackQuery.Message is not null)
                    {
                        await _client.SendTextMessageAsync(updateManager.Update.CallbackQuery.Message.Chat.Id, exceptionMessage);
                    }
                    await _client.AnswerCallbackQueryAsync(updateManager.Update.CallbackQuery.Id);
                }
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"status\":\"error\",\"message\":\"Exception\"}");
        }

        private async Task HandleExceptionAsync(HttpContext context, IUpdateManager updateManager, II18nService i18n)
        {
            if (updateManager.Update is null)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"status\":\"error\",\"message\":\"Exception\"}");
                return;
            }

            string exceptionMessage = i18n.T("exception_main");

            if (updateManager.Update.Type == UpdateType.Message)
            {
                if (updateManager.Update.Message is not null)
                {
                    await _client.SendTextMessageAsync(updateManager.Update.Message.Chat.Id, exceptionMessage);
                }
            }
            else if (updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                if (updateManager.Update.CallbackQuery is not null)
                {
                    if (updateManager.Update.CallbackQuery.Message is not null)
                    {
                        await _client.SendTextMessageAsync(updateManager.Update.CallbackQuery.Message.Chat.Id, exceptionMessage);
                    }
                    await _client.AnswerCallbackQueryAsync(updateManager.Update.CallbackQuery.Id);
                }
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"status\":\"error\",\"message\":\"Exception\"}");
        }
    }
}