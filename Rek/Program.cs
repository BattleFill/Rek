using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
public class UserRecord
{
    public string Name { get; set; }
    public int CharactersPerMinute { get; set; }
    public int CharactersPerSecond { get; set; }
}
public static class RecordTableManager
{
    private static List<UserRecord> records = new List<UserRecord>();
    public static void AddRecordAndDisplay(UserRecord record)
    {
        records.Add(record);
        DisplayTopRecords(5);
    }
    public static List<UserRecord> GetTopRecords(int count)
    {
        return records.OrderByDescending(r => r.CharactersPerMinute).Take(count).ToList();
    }
    public static void DisplayTopRecords(int count)
    {
        List<UserRecord> topRecords = RecordTableManager.GetTopRecords(count);
        Console.WriteLine("\nТоп рекордов:");
        Console.WriteLine("Имя\tСимволов в минуту\tСимволов в секунду");
        foreach (var record in topRecords)
        {
            Console.WriteLine($"{record.Name}\t{record.CharactersPerMinute}\t\t\t{record.CharactersPerSecond}");
        }
    }

}
public class TypingTestManager
{
    private const int TestDurationSeconds = 60;
    private static readonly string TextToType = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
    private static bool isRunning = false;
    private static System.Timers.Timer timer;
    public static void RunTest(string userName)
    {
        bool timerElapsed = false; 

        Console.WriteLine($"Привет, {userName}! Введите следующий текст:\n{TextToType}");
        Console.WriteLine("Нажмите Enter, чтобы начать.");

        Console.ReadLine(); 

        Console.Clear();
        Console.WriteLine($"Тест начался. Набирайте текст. У вас 1 минута.\n{TextToType}");

        isRunning = true;
        timer = new System.Timers.Timer(TestDurationSeconds * 1000); 
        timer.Elapsed += (sender, e) => { TimerElapsed(sender, e, ref timerElapsed); }; 
        timer.AutoReset = false;
        timer.Start();

        Thread inputThread = new Thread(() => { ReadInput(timerElapsed); });
        inputThread.Start();

        inputThread.Join(); 

        if (!timerElapsed)
        {
            timer.Stop(); 
            Console.WriteLine("\nТест завершен. Ваши результаты сохранены.");
            RecordTableManager.DisplayTopRecords(5); 
        }
    }
    private static void TimerElapsed(object sender, ElapsedEventArgs e, ref bool timerElapsed)
    {
        Console.WriteLine("\nВремя вышло! Тест завершен.");
        isRunning = false;
        timerElapsed = true;
    }

    private static async void ReadInput(bool timerElapsed)
    {
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            var readTask = Console.In.ReadLineAsync();
            var delayTask = Task.Delay(TimeSpan.FromMinutes(1), cancellationTokenSource.Token);

            var completedTask = await Task.WhenAny(readTask, delayTask);

            if (completedTask == readTask)
            {
                cancellationTokenSource.Cancel();
                string userInput = readTask.Result;
                if (isRunning)
                {
                    timer.Stop(); 
                    Console.WriteLine("\nТест завершен. Ваши результаты сохранены.");
                    timerElapsed = false;
                    RecordTableManager.DisplayTopRecords(5);
                    isRunning = false;
                }
            }
            else
            {
                Console.WriteLine("\nВремя вышло! Тест завершен.");
                isRunning = false;
                timerElapsed = true;
            }
        }
    }
    private static void DisplayResults()
    {
        UserRecord record = new UserRecord();
        RecordTableManager.AddRecordAndDisplay(record);
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Добро пожаловать в тест на скоропечатание!");
        Console.Write("Пожалуйста, введите ваше имя: ");
        string userName = Console.ReadLine();

        TypingTestManager.RunTest(userName);
        bool repeat = true;
        while (repeat)
        {
            Console.Write("\nХотите пройти тест еще раз? (да/нет): ");
            string response = Console.ReadLine().ToLower();
            if (response == "да")
            {
                TypingTestManager.RunTest(userName);
            }
            else if (response == "нет")
            {
                repeat = false;
            }
            else
            {
                Console.WriteLine("Неверный ввод. Пожалуйста, введите 'да' или 'нет'.");
            }
        }
    }
}