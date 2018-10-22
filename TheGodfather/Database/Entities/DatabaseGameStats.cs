using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("game_stats")]
    public class DatabaseGameStats
    {

        public DatabaseGameStats()
        {

        }

        public DatabaseGameStats(ulong uid)
        {
            this.UserIdDb = (long)uid;
        }


        [Key]
        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId => (ulong)this.UserIdDb;

        [Column("duel_won")]
        public int DuelsWon { get; set; } 

        [Column("duel_lost")]
        public int DuelsLost { get; set; } 

        [Column("hangman_won")]
        public int HangmanWon { get; set; }

        [Column("quizes_won")]
        public int QuizesWon { get; set; } 

        [Column("animalraces_won")]
        public int AnimalRacesWon { get; set; }

        [Column("numberraces_won")]
        public int NumberRacesWon { get; set; }

        [Column("ttt_won")]
        public int TicTacToeWon { get; set; } 

        [Column("ttt_lost")]
        public int TicTacToeLost { get; set; } 

        [Column("chain4_won")]
        public int Chain4Won { get; set; } 

        [Column("chain4_lost")]
        public int Chain4Lost { get; set; } 

        [Column("caro_won")]
        public int CaroWon { get; set; } 

        [Column("caro_lost")]
        public int CaroLost { get; set; } 

        [Column("othello_won")]
        public int OthelloWon { get; set; } 

        [Column("othello_lost")]
        public int OthelloLost { get; set; }


        public int CalculateWinPercentage(int won, int lost)
        {
            if (won + lost == 0)
                return 0;

            return (int)Math.Round((double)won / (won + lost) * 100);
        }
    }
}
