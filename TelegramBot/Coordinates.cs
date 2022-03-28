using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBot
{
    [Table("Coordinates")]
    public class Coordinates
    {
        public long id { get; set; }
        public long chatId { get; set; }
        public string coords { get; set; }
    }
}
