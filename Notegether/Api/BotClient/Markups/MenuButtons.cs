using Telegram.Bot.Types.ReplyMarkups;

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
    
    public static InlineKeyboardMarkup ObjectForEditingMockInlineKeyboardMarkup()
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            // first row
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "Название",
                    "1"),
                InlineKeyboardButton.WithCallbackData(
                    "Описание",
                    "2"),
                InlineKeyboardButton.WithCallbackData(
                    "Текст",
                    "3"),
            },
        });
        
        
        return inlineKeyboard;
    }
    
    public static InlineKeyboardMarkup PermissionsRolesInlineKeyboardMarkup()
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            // first row
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "Читатель",
                    "reader"),
                InlineKeyboardButton.WithCallbackData(
                    "Редактор",
                    "editor")
            }
        });
        
        return inlineKeyboard;
    }
    
    public static InlineKeyboardMarkup EditTypeInlineKeyboardMarkup()
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            // first row
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "Изменить",
                    "rewrite"),
                InlineKeyboardButton.WithCallbackData(
                    "Дописать",
                    "add_text")
            }
        });
        
        return inlineKeyboard;
    }
    
    public static InlineKeyboardMarkup EditTypeInlineMockKeyboardMarkup()
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            // first row
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "Изменить",
                    "4"),
                InlineKeyboardButton.WithCallbackData(
                    "Дописать",
                    "5")
            }
        });
        
        return inlineKeyboard;
    }

}