#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Group("meme", CanInvokeWithoutSubcommand = true)]
    [Description("Contains some memes. When invoked without subcommand, returns a random one.")]
    [Aliases("pic", "memes", "m")]
    public class CommandsMemes
    {
        public async Task ExecuteGroup(CommandContext ctx)
        {
            var rnd = new Random();
            switch (rnd.Next(0, 44)) {
                #region MEME_LIST
                case 0: await FapGun(ctx); return;
                case 1: await Dildo(ctx); return;
                case 2: await SmileMask(ctx); return;
                case 3: await SoENotSoH(ctx); return;
                case 4: await SwatServers(ctx); return;
                case 5: await ForLifeServer(ctx); return;
                case 6: await ForLifeMemes(ctx); return;
                case 7: await BanWM(ctx); return;
                case 8: await FUvsLTM(ctx); return;
                case 9: await EightD(ctx); return;
                case 10: await Rebi(ctx); return;
                case 11: await Abuse(ctx); return;
                case 12: await Alex(ctx); return;
                case 13: await MaxJoin(ctx); return;
                case 14: await CivilWar(ctx); return;
                case 15: await EightD(ctx); return;
                case 16: await Cockcopter(ctx); return;
                case 17: await Cojones(ctx); return;
                case 18: await Titi(ctx); return;
                case 19: await Eyes(ctx); return;
                case 20: await ForLifeLeave(ctx); return;
                case 21: await Halo(ctx); return;
                case 22: await VitessBdayCake(ctx); return;
                case 23: await JuicedPC(ctx); return;
                case 24: await JoJoReply(ctx); return;
                case 25: await JoJoReply2(ctx); return;
                case 26: await KimJoUn(ctx); return;
                case 27: await Markie(ctx); return;
                case 28: await Mazso(ctx); return;
                case 29: await Panter(ctx); return;
                case 30: await Pardon(ctx); return;
                case 31: await RugiPayment(ctx); return;
                case 32: await RugiServer(ctx); return;
                case 33: await RugiMakeFun(ctx); return;
                case 34: await PepeRage(ctx); return;
                case 35: await Fap(ctx); return;
                case 36: await Fap2(ctx); return;
                case 37: await Banana(ctx); return;
                case 38: await Stfu4(ctx); return;
                case 39: await Donations(ctx); return;
                case 40: await CalmBros(ctx); return;
                case 41: await BravoKick4life(ctx); return;
                case 42: await Rugi(ctx); return;
                case 43: await ZyklonB(ctx); return;
                #endregion
            }
        }

        #region COMMANDS_MEMES
            #region CLASSIC
                [Command("granny")]
                [Aliases("finger")]
                public async Task GrannyFinger(CommandContext ctx)
                {
                    await SendMeme(ctx, "http://i.imgur.com/xmdy9sJ.gif");
                }
            #endregion
            #region 4LIFE
            [Command("fapgun")]
                public async Task FapGun(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/dxbP3LS.gif");
                }

                [Command("dildo")]
                public async Task Dildo(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/NBRCK4x.jpg");
                }

                [Command("smilemask")]
                public async Task SmileMask(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/NBRCK4x.jpg");
                }

                [Command("soenotsoh")]
                public async Task SoENotSoH(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/zhHfrHf.jpg");
                }

                [Command("swatservers")]
                public async Task SwatServers(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/DuBcuke.jpg");
                }

                [Command("4lifeserver")]
                public async Task ForLifeServer(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/RGibyYl.jpg");
                }

                [Command("4lifememes")]
                public async Task ForLifeMemes(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/wUC2o33.jpg");
                }

                [Command("banwm")]
                public async Task BanWM(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/hnDB9UV.jpg");
                }

                [Command("4uvsltm")]
                public async Task FUvsLTM(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/IS6udqN.png");
                }

                [Command("8d")]
                public async Task EightD(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/wMIWU9M.png");
                }

                [Command("rebi")]
                public async Task Rebi(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/sp9t0Vq.png");
                }

                [Command("abuse")]
                public async Task Abuse(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/NfxP6ED.jpg");
                }

                [Command("alex")]
                public async Task Alex(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/mStaB9l.png");
                }

                [Command("maxjoin")]
                public async Task MaxJoin(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/48G6SeI.png");
                }

                [Command("civilwar")]
                public async Task CivilWar(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/CAv6g7B.jpg");
                }

                [Command("cockcopter")]
                public async Task Cockcopter(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/DwF9tc8.gif");
                }

                [Command("cojones")]
                public async Task Cojones(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/kjC6rwV.jpg");
                }

                [Command("titi")]
                public async Task Titi(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/LtpTewU.jpg");
                }

                [Command("eyes")]
                public async Task Eyes(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/RBJD5nh.jpg");
                }

                [Command("4lifeleave")]
                public async Task ForLifeLeave(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/gzLGPDd.jpg");
                }

                [Command("halo")]
                public async Task Halo(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/bElaJ0D.png");
                }

                [Command("vitesscake")]
                public async Task VitessBdayCake(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/tqVbMbC.png");
                }

                [Command("juicedpc")]
                public async Task JuicedPC(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/KYwIfYR.jpg");
                }

                [Command("jojoreply")]
                public async Task JoJoReply(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/AUaaoiG.jpg");
                }

                [Command("jojoreply2")]
                public async Task JoJoReply2(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/qTUloRW.png");
                }

                [Command("kimjoun")]
                public async Task KimJoUn(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/1rOiw00.png");
                }

                [Command("markie")]
                public async Task Markie(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/wzLfO9y.png");
                }

                [Command("mazso")]
                public async Task Mazso(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/IrlGMGo.png");
                }

                [Command("panter")]
                [Aliases("view")]
                public async Task Panter(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/yLkx2uK.png");
                }

                [Command("pardon")]
                public async Task Pardon(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/QttVUtZ.jpg");
                }

                [Command("rugipayment")]
                public async Task RugiPayment(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/vMhZVv3.jpg");
                }

                [Command("rugiserver")]
                public async Task RugiServer(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/IUlOdDl.jpg");
                }

                [Command("rugimakefun")]
                public async Task RugiMakeFun(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/BeedXK9.jpg");
                }

                [Command("peperage")]
                public async Task PepeRage(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/nwRLMJT.gif");
                }

                [Command("fap")]
                public async Task Fap(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/aYEGsxh.gif");
                }

                [Command("fap2")]
                public async Task Fap2(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/6rMWjeP.gif");
                }

                [Command("stfu4")]
                public async Task Stfu4(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/hdl2FX8.jpg");
                }

                [Command("banana")]
                public async Task Banana(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/5Jn2ylk.jpg");
                }

                [Command("donations")]
                public async Task Donations(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/Atcrimv.png");
                }

                [Command("calm")]
                public async Task CalmBros(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/XnEhfha.png");
                }

                [Command("bravokick4life")]
                public async Task BravoKick4life(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/c1BEVOD.png");
                }

                [Command("rugi")]
                public async Task Rugi(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/Y2VvPNs.jpg");
                }

                [Command("zyklon")]
                public async Task ZyklonB(CommandContext ctx)
                {
                    await SendMeme(ctx, "https://i.imgur.com/yHoUdvx.jpg");
                }
                #endregion
        #endregion
        
        #region HELPER_FUNCTIONS
        private async Task SendMeme(CommandContext ctx, string url)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbed {
                Image = new DiscordEmbedImage {
                    Url = url
                }
            };
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion
    }
}
