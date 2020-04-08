using System;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpPractice1
{
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
            var result = await client.SendAsync(message);

            Console.WriteLine("Server responce:");
            if (result.IsSuccessStatusCode)
            {
                if (showOnlyReturnCode)
                    Console.WriteLine($"Return code: {result.StatusCode}");
                else
                    Console.WriteLine(await result.Content.ReadAsStringAsync());
            }
            else
            {
                Console.WriteLine($"Return code: {result.StatusCode}\nMessage: {await result.Content.ReadAsStringAsync()}");
            }
            Console.WriteLine();
        }

        private HttpClient client;
    }


    class Program
    {
        static async Task Main(string[] args)
        {
            MyClient client = new MyClient();
            client.Init();

            while (true)
            {
                string command = Console.ReadLine();
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
                        Console.WriteLine("Get usage: get [--sorted (True/False)] [--id (dataId)]");
                }
                else if (Regex.IsMatch(command, "^post", RegexOptions.IgnoreCase))
                {
                    if (Regex.IsMatch(command, "^post {.+} *$", RegexOptions.IgnoreCase))
                        await client.PostRequest(command.Substring(5).TrimEnd());
                    else
                        Console.WriteLine("Post usage: post {\"dataId\": \"someId\", \"weight\": 42}");
                }
                else if (Regex.IsMatch(command, "^put", RegexOptions.IgnoreCase))
                {
                    if (Regex.IsMatch(command, "^put \\S+ {.+} *$", RegexOptions.IgnoreCase))
                    {
                        string[] commandArgs = command.Split(' ');
                        await client.PutRequest(commandArgs[1], command.Substring(commandArgs[0].Length + commandArgs[1].Length + 2));
                    }
                    else
                        Console.WriteLine("Put usage: put {id} {\"dataId\": \"someId\", \"weight\": 42}");
                }
                else if (Regex.IsMatch(command, "^delete", RegexOptions.IgnoreCase))
                {
                    if (Regex.IsMatch(command, "^delete \\S+ *$", RegexOptions.IgnoreCase))
                    {
                        await client.DeleteDataByIdRequest(command.Substring(7).Trim());
                    }
                    else
                        Console.WriteLine("Delete usage: delete {id}");
                }
                else
                {
                    Console.WriteLine("Program: unknown command");
                }
            }
        }
    }
}
