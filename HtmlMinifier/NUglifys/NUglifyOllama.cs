using HtmlMinifier.Interfaces;
using Ollama;

namespace HtmlMinifier.NUglifys;

public class NUglifyOllama : INUglifyProcess
{
    private OllamaApiClient ollama;
    private Chat chat;

    private const string Promt = @"

Вы — помощник по коду. Получите фрагмент JavaScript/TypeScript, выполните следующие шаги и верните результат в JSON с полями: ""explanation"" (краткое понятное объяснение, 2–4 предложения), ""issues"" (массив обнаруженных проблем с указанием строки/функции и уровня важности: ""critical""/""warning""/""style""), ""fixes"" (массив предложенных правок: краткое описание и фрагмент патча/исправленного кода), ""improvedCode"" (полный исправленный код), ""tests"" (предложения по тестам или небольшие юнит-тесты), ""notes"" (краткие рекомендации по производительности/безопасности). Требования:
- Сохраняйте исходный стиль кода по возможности.
- Если обнаружены синтаксические ошибки — исправьте их в ""improvedCode"" и отметьте в ""issues"" как ""critical"".
- Если возможны уязвимости (XSS, injection, небезопасные eval/innerHTML, небезопасное чтение внешних данных) — отметьте с уровнем ""critical"" и дайте безопасную альтернативу.
- Для изменений показывайте минимальные патчи — только изменённые функции/строки.
- Если код использует внешние библиотеки — укажите версии API, которые вы предполагаете, и предложите изменения совместимости, если требуется.
- Не добавляйте новых зависимости без явной необходимости; если добавляете — объясните почему.
- Пример входа: фрагмент кода между тройными обратными кавычками. Обрабатывайте только код, который внутри кавычек.
- Формат ответа строго JSON.

Теперь обработай следующий код:

";

    public NUglifyOllama()
    {
        ollama = new OllamaApiClient();
        chat = ollama.Chat(
            model: "llama3.2",
            systemMessage: Promt,
            autoCallTools: true);
    }

    public Task<string> Call(string content)
    {
        // try
        // {
        //     _ = await chat.SendAsync(string.Join("\n", scripts));
        // }
        // finally
        // {
        //     scripts = chat.History.Where(s => s.Role == MessageRole.User).Select(m => m.Content).ToArray();
        //
        //
        //     
        // }
        return null;
    }

    public void AddBaseDirectory(string directory)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        ollama.Dispose();
    }
}