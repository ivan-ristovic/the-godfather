namespace TheGodfather.Modules.Chickens.Common;

public sealed class ChickenFightResult
{
    public const int VitLoss = 50;
    public const int RewardPerGainPoint = 200;

    public Chicken Winner { get; }
    public Chicken Loser { get; }
    public int StrGain { get; }
    public int Reward => this.StrGain * RewardPerGainPoint;
    public bool IsLoserDead => this.Loser.Stats.TotalVitality <= 0;


    private ChickenFightResult(Chicken winner, Chicken loser, int gain)
    {
        this.Winner = winner;
        this.Loser = loser;
        this.StrGain = gain;
    }


    public static ChickenFightResult Fight(Chicken c1, Chicken c2)
    {
        int chance = 50 + c1.Stats.TotalStrength - c2.Stats.TotalStrength;

        if (c1.Stats.TotalStrength > c2.Stats.TotalStrength) {
            if (chance > 99)
                chance = 99;
        } else {
            if (chance < 1)
                chance = 1;
        }

        bool c1Win = new SecureRandom().Next(100) < chance;
        Chicken winner = c1Win ? c1 : c2;
        Chicken loser = c1Win ? c2 : c1;
        int gain = winner.DetermineStrengthGain(loser);
        winner.Stats.BareStrength += gain;
        winner.Stats.BareVitality -= gain;
        if (winner.Stats.TotalVitality == 0)
            winner.Stats.BareVitality++;
        loser.Stats.BareVitality -= 50;

        return new ChickenFightResult(winner, loser, gain);
    }
}