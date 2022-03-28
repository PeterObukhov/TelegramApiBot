using Newtonsoft.Json.Linq;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;  
using System.Threading.Tasks;

namespace TelegramBot
{
    internal class Api : IJob
    {
        private readonly string url = "http://dev.virtualearth.net/REST/v1/Routes?wayPoint.1={start}&wayPoint.2={end}&optimize=timeWithTraffic&routeAttributes=routeSummariesOnly&key={token}";
        private readonly HttpClient client = new HttpClient();
        private string tempUrl { get; set; }
        private IScheduler scheduler { get; set; }
        private TelegramBot telegramBot { get; set; }
        private string time { get; set; } = "0 0 3,9,15,21";

        public Api()
        {
            telegramBot = new TelegramBot();
        }

        private async Task<string> CallApi()
        {
            var response = await client.GetAsync(tempUrl);
            var responseString = await response.Content.ReadAsStringAsync();
            JObject jo = JObject.Parse(responseString);
            return (string)jo.SelectToken("resourceSets.[0].resources.[0].travelDurationTraffic");
        }

        public async Task PublicHandler()
        {
            foreach (Coordinates row in DbHandler.ReadPublic())
            {
                string publicMessageText = string.Empty;
                foreach (string time in await Handler(row.coords))
                {
                    if (time != string.Empty) publicMessageText += $"{time}\n";
                    else publicMessageText += "Нет данных";
                }
                telegramBot.SendMessage(publicMessageText, row.chatId);
            }
        }

        public async Task PrivateHandler(long chatId)
        {
            string privateMessageText = string.Empty;
            foreach (string coords in DbHandler.ReadPrivate(chatId))
            {
                foreach (string time in await Handler(coords))
                {
                    if (time != string.Empty) privateMessageText += $"{time}\n";
                    else privateMessageText += "Нет данных";
                }
                telegramBot.SendMessage(privateMessageText, chatId);
            }
        }

        private async Task<List<string>> Handler(string coordinates)
        {
            List<string> ans = new List<string>();
            string[] pointCoordinates = coordinates.Split("\n");
            foreach (string coords in pointCoordinates)
            {
                var coordsArr = coords.Split(" ");
                Dictionary<string, string> replacements = new Dictionary<string, string> { { "{start}", coordsArr[0] }, { "{end}", coordsArr[1] } };
                tempUrl = replacements.Aggregate(url, (current, replacement) => current.Replace(replacement.Key, replacement.Value));
                ans.Add(await CallApiHandler());
            }
            return ans;
        }

        private async Task<string> CallApiHandler()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(400));
            string time = await CallApi();
            return time;
        }

        public async void RunAutomatically()
        {
            Console.WriteLine("Starting to collect info automatically...");
            StdSchedulerFactory factory = new StdSchedulerFactory();
            scheduler = await factory.GetScheduler();

            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<Api>()
                .WithIdentity("job1", "group1")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .WithCronSchedule($"{time} * * ?")
                .ForJob(job)
                .Build();

            if (!await scheduler.CheckExists(job.Key)) await scheduler.ScheduleJob(job, trigger);
        }

        public async void StopRunning()
        {
            if (scheduler != null) await scheduler.Shutdown();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await PublicHandler();
        }
    }
}