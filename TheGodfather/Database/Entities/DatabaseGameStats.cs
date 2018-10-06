using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("game_stats")]
    public class DatabaseGameStats
    {
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

        [Column("numraces_won")]
        public int HangmanLost { get; set; } 

        [Column("quiz_won")]
        public int QuizWon { get; set; } 

        [Column("animalrace_won")]
        public int AnimalRaceWon { get; set; } 

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
    }
}
