using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("game_stats")]
    public class GameStats : IEquatable<GameStats>
    {
        public static int WinPercentage(int won, int lost)
            => won + lost == 0 ? 0 : (int)Math.Round((double)won / (won + lost) * 100);


        [Key]
        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("duel_won")]
        public int DuelWon { get; set; }

        [Column("duel_lost")]
        public int DuelLost { get; set; }

        [Column("hangman_won")]
        public int HangmanWon { get; set; }

        [Column("quiz_won")]
        public int QuizWon { get; set; }

        [Column("ar_won")]
        public int AnimalRacesWon { get; set; }

        [Column("nr_won")]
        public int NumberRacesWon { get; set; }

        [Column("ttt_won")]
        public int TicTacToeWon { get; set; }

        [Column("ttt_lost")]
        public int TicTacToeLost { get; set; }

        [Column("c4_won")]
        public int Chain4Won { get; set; }

        [Column("c4_lost")]
        public int Chain4Lost { get; set; }

        [Column("caro_won")]
        public int CaroWon { get; set; }

        [Column("caro_lost")]
        public int CaroLost { get; set; }

        [Column("othello_won")]
        public int OthelloWon { get; set; }

        [Column("othello_lost")]
        public int OthelloLost { get; set; }


        public bool Equals(GameStats? other)
            => !(other is null) && this.UserId == other.UserId;

        public override bool Equals(object? other)
            => this.Equals(other as GameStats);

        public override int GetHashCode()
            => this.UserId.GetHashCode();
    }
}
