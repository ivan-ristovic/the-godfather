using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Currency.Services
{
    public sealed class WorkService : ITheGodfatherService
    {
        public const int WorkEarnLimit = 500;
        public const int StreetEarnLimit = 1000;
        public const int CrimeEarnLimit = 10000;

        public bool IsDisabled => false;

        private readonly LocalizationService lcs;
        private readonly BankAccountService bas;
        private readonly SecureRandom rng;


        public WorkService(LocalizationService lcs, BankAccountService bas)
        {
            this.bas = bas;
            this.lcs = lcs;
            this.rng = new SecureRandom();
        }


        public async Task<string> WorkAsync(ulong gid, ulong uid)
        {
            RegularWorkType workType = this.rng.ChooseRandomEnumValue<RegularWorkType>();
            int earned = this.rng.Next(WorkEarnLimit) + 1;
            await this.bas.IncreaseBankAccountAsync(gid, uid, earned);
            return this.lcs.GetString(gid, $"fmt-work-{(int)workType}", earned);
        }

        public async Task<string> CrimeAsync(ulong gid, ulong uid)
        {
            CrimeWorkType crimeType = this.rng.ChooseRandomEnumValue<CrimeWorkType>();
            int change = this.rng.Next(CrimeEarnLimit) + 1;
            if (this.rng.NextBool(9)) {
                await this.bas.ModifyBankAccountAsync(gid, uid, v => v - change);
                return this.lcs.GetString(gid, $"fmt-work-crime-fail-{(int)crimeType}", change);
            } else {
                await this.bas.IncreaseBankAccountAsync(gid, uid, change);
                return this.lcs.GetString(gid, $"fmt-work-crime-{(int)crimeType}", change);
            }
        }

        public async Task<string> StreetsAsync(ulong gid, ulong uid)
        {
            int change = this.rng.Next(StreetEarnLimit) + 1;
            if (this.rng.NextBool()) {
                NegativeStreetWorkType streetType = this.rng.ChooseRandomEnumValue<NegativeStreetWorkType>();
                await this.bas.ModifyBankAccountAsync(gid, uid, v => v - change);
                return this.lcs.GetString(gid, $"fmt-work-streets-fail-{(int)streetType}", change);
            } else {
                PositiveStreetWorkType streetType = this.rng.ChooseRandomEnumValue<PositiveStreetWorkType>();
                await this.bas.IncreaseBankAccountAsync(gid, uid, change);
                return this.lcs.GetString(gid, $"fmt-work-streets-{(int)streetType}", change);
            }
        }
    }
}
