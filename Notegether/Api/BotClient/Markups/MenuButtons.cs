﻿using Telegram.Bot.Types.ReplyMarkups;

namespace Notegether.Api.BotClient.Markups;

public static class MenuButtons
{

    public static InlineKeyboardMarkup ObjectForEditingInlineKeyboardMarkup()
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            // first row
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "Название",
                    "title"),
                InlineKeyboardButton.WithCallbackData(
                    "Описание",
                    "description"),
                InlineKeyboardButton.WithCallbackData(
                    "Текст",
                    "text"),
            },
        });
        
        return inlineKeyboard;
    }

}