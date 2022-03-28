using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBot
{
    public class DbHandler
    {
        private static readonly TelegramDbContext dataBase = new TelegramDbContext();
        public static void Save(long chatId, string ans)
        {
            dataBase.Coordinates.Add(new Coordinates
            {
                chatId = chatId,
                coords = ans
            });
            dataBase.SaveChanges();
        }
        public static IEnumerable<string> ReadPrivate(long chatId)
        {
            IEnumerable<string> ans = null;
            ans = dataBase.Coordinates.Where(x => x.chatId == chatId).Select(x => x.coords);
            if (ans != null) return ans;
            else throw new Exception("Нет доступных записей в базе");
        }
        public static IEnumerable<Coordinates> ReadPublic()
        {
            if (dataBase.Coordinates.Count() > 0)
            {
                foreach (Coordinates row in dataBase.Coordinates)
                {
                    yield return row;
                }
            }
            else throw new Exception("В базе данных отстутствуют записи");
        }
        public static void Delete(long chatId)
        {
            var tempCoords = dataBase.Coordinates.Where(x => x.chatId == chatId).FirstOrDefault();
            if (tempCoords != null)
            {
                dataBase.Coordinates.Remove(tempCoords);
                dataBase.SaveChanges();
            }
            else throw new Exception("Нет доступных записей в базе");
        }
    }
}
