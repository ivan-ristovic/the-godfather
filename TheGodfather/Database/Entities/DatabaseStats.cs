using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("stats")]
    public partial class DatabaseStats
    {
        public long Uid { get; set; }
        public int DuelsWon { get; set; }
        public int DuelsLost { get; set; }
        public int HangmanWon { get; set; }
        public int NumracesWon { get; set; }
        public int QuizesWon { get; set; }
        public int RacesWon { get; set; }
        public int TttWon { get; set; }
        public int TttLost { get; set; }
        public int Chain4Won { get; set; }
        public int Chain4Lost { get; set; }
        public int CaroWon { get; set; }
        public int CaroLost { get; set; }
        public int OthelloWon { get; set; }
        public int OthelloLost { get; set; }
    }
}
