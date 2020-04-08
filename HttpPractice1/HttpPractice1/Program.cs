using System;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleUtils
{
    public static class ConsoleUtils
    {
        public enum MessageType
        {
            COMMON_TEXT,
            SUCCESS,
            FAIL,
            HINT,
            WEAK
        }

        private static void EnableColors(MessageType type)
        {
            switch (type)
            {
                case MessageType.COMMON_TEXT:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case MessageType.SUCCESS:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case MessageType.FAIL:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case MessageType.HINT:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case MessageType.WEAK:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
        }
        public static void WriteLine(string msg, MessageType type)
        {
            EnableColors(type);
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void WriteText(string msg, MessageType type)
        {
            EnableColors(type);
            Console.Write(msg);
            Console.ResetColor();
        }
    }
}

namespace HttpPractice1
{
    using ConsoleUtils;

    class MyClient
    {
        public void Init()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000/somedata/");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("appId", "campus-task");
        }

        public async Task GetDataByIdRequest(string id) => await SendRequest(new HttpRequestMessage(HttpMethod.Get, id), false);
        public async Task GetAllDataRequest(bool isSorted) => await SendRequest(new HttpRequestMessage(HttpMethod.Get, isSorted ? "?sorted=True" : ""), false);
        public async Task DeleteDataByIdRequest(string id) => await SendRequest(new HttpRequestMessage(HttpMethod.Delete, id), true);
        public async Task PostRequest(string stringData)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "");

            message.Content = new StringContent(stringData, Encoding.UTF8, "application/json");

            await SendRequest(message, true);
        }
        public async Task PutRequest(string id, string stringData)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Put, id);

            message.Content = new StringContent(stringData, Encoding.UTF8, "application/json");

            await SendRequest(message, true);
        }


        private async Task SendRequest(HttpRequestMessage message, bool showOnlyReturnCode)
        {
            try {
                var result = await client.SendAsync(message);

                ConsoleUtils.WriteLine("Server responce:", ConsoleUtils.MessageType.WEAK);
                if (result.IsSuccessStatusCode)
                {
                    if (showOnlyReturnCode)
                        ConsoleUtils.WriteLine($"Return code: {result.StatusCode}", ConsoleUtils.MessageType.SUCCESS);
                    else
                        ConsoleUtils.WriteLine(await result.Content.ReadAsStringAsync(), ConsoleUtils.MessageType.SUCCESS);
                }
                else
                {
                    ConsoleUtils.WriteLine(
                        $"Return code: {result.StatusCode}\nMessage: {await result.Content.ReadAsStringAsync()}",
                        ConsoleUtils.MessageType.FAIL);
                }
            } catch (HttpRequestException e) {
                ConsoleUtils.WriteLine(e.Message, ConsoleUtils.MessageType.FAIL);
            }
        }

        private HttpClient client;
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            const string getUsage = "Get usage: get [--sorted (True/False)] [--id (dataId)]";
            const string postUsage = "Post usage: post {\"dataId\": \"someId\", \"weight\": 42}";
            const string putUsage = "Put usage: put {id} {\"dataId\": \"someId\", \"weight\": 42}";
            const string deleteUsage = "Delete usage: delete {id}";

            MyClient client = new MyClient();
            client.Init();

            ConsoleUtils.WriteLine(getUsage, ConsoleUtils.MessageType.HINT);
            ConsoleUtils.WriteLine(postUsage, ConsoleUtils.MessageType.HINT);
            ConsoleUtils.WriteLine(putUsage, ConsoleUtils.MessageType.HINT);
            ConsoleUtils.WriteLine(deleteUsage, ConsoleUtils.MessageType.HINT);
            ConsoleUtils.WriteLine("For exit enter 'q'", ConsoleUtils.MessageType.HINT);
            Console.WriteLine();


            while (true)
            {
                ConsoleUtils.WriteText("Command prompt >>> ", ConsoleUtils.MessageType.WEAK);
                string command = Console.ReadLine();
                Console.WriteLine();

                if (command.Equals("q"))
                    break;

                if (Regex.IsMatch(command, "^get( |$)", RegexOptions.IgnoreCase))
                {
                    if (Regex.IsMatch(command, "^get *$", RegexOptions.IgnoreCase))
                        await client.GetAllDataRequest(false);
                    else if (Regex.IsMatch(command, "^get --sorted (True|False) *$", RegexOptions.IgnoreCase))
                        await client.GetAllDataRequest(bool.Parse(command.Substring(13).Trim()));
                    else if (Regex.IsMatch(command, "^get --id .+ *$", RegexOptions.IgnoreCase))
                        await client.GetDataByIdRequest(command.Substring(9).Trim());
                    else
                        ConsoleUtils.WriteLine(getUsage, ConsoleUtils.MessageType.HINT);
                }
                else if (Regex.IsMatch(command, "^post", RegexOptions.IgnoreCase))
                {
                    if (Regex.IsMatch(command, "^post {.+} *$", RegexOptions.IgnoreCase))
                        await client.PostRequest(command.Substring(5).TrimEnd());
                    else
                        ConsoleUtils.WriteLine(postUsage, ConsoleUtils.MessageType.HINT);
                }
                else if (Regex.IsMatch(command, "^put", RegexOptions.IgnoreCase))
                {
                    if (Regex.IsMatch(command, "^put \\S+ {.+} *$", RegexOptions.IgnoreCase))
                    {
                        string[] commandArgs = command.Split(' ');
                        await client.PutRequest(commandArgs[1], command.Substring(commandArgs[0].Length + commandArgs[1].Length + 2));
                    }
                    else
                        ConsoleUtils.WriteLine(putUsage, ConsoleUtils.MessageType.HINT);
                }
                else if (Regex.IsMatch(command, "^delete", RegexOptions.IgnoreCase))
                {
                    if (Regex.IsMatch(command, "^delete \\S+ *$", RegexOptions.IgnoreCase))
                    {
                        await client.DeleteDataByIdRequest(command.Substring(7).Trim());
                    }
                    else
                        ConsoleUtils.WriteLine(deleteUsage, ConsoleUtils.MessageType.HINT);
                }
                else
                {
                    ConsoleUtils.WriteLine("Program: unknown command", ConsoleUtils.MessageType.FAIL);
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
