--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.6
-- Dumped by pg_dump version 9.6.6

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

SET search_path = gf, pg_catalog;

--
-- Data for Name: accounts; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY accounts (uid, balance) FROM stdin;
116275390695079945	50
420949917574627328	20
201309107267960832	530
222076305850630144	0
\.


--
-- Data for Name: assignable_roles; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY assignable_roles (gid, rid) FROM stdin;
201315884709576705	406033009406640138
201315884709576705	412946068095893505
337570344149975050	419149286249594880
337570344149975050	409681035392057354
\.


--
-- Data for Name: automatic_roles; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY automatic_roles (gid, rid) FROM stdin;
\.


--
-- Data for Name: birthdays; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY birthdays (uid, cid, bday, last_updated) FROM stdin;
201317516407078913	201315884709576705	1992-05-05	2017
154556794490847232	201315884709576705	1984-05-05	2017
243063532097241089	201315884709576705	1996-10-08	2017
229367984701833216	201315884709576705	1988-08-27	2017
241983016220491786	201315884709576705	1988-11-21	2017
249543877491556352	201315884709576705	1977-05-31	2017
228622291297239042	201315884709576705	1992-07-04	2017
220214154500374530	201315884709576705	1995-09-22	2017
217739186864652289	201315884709576705	1980-10-26	2017
249514892464357376	201315884709576705	1986-08-12	2017
201316355356622849	201315884709576705	1994-03-19	2018
235457336750243850	201315884709576705	1991-01-13	2018
217918127739109376	201315884709576705	1983-10-01	2017
249219884993478657	201315884709576705	1991-12-02	2017
290948683044749322	201315884709576705	1991-12-06	2017
227391187730956290	201315884709576705	1994-11-17	2017
234388409932709888	201315884709576705	1900-10-29	2017
302959303525138434	201315884709576705	1993-07-11	2017
234375713183105024	201315884709576705	1992-06-03	2017
306465199165145101	201315884709576705	1993-11-21	2017
233567317949284352	201315884709576705	1989-02-06	2017
224540765173317632	201315884709576705	1993-11-07	2017
241603322501398528	201315884709576705	1992-06-14	2017
222076305850630144	201315884709576705	1998-03-28	2017
424991311481798666	201315884709576705	1989-12-05	2017
154907962169753600	201315884709576705	1984-08-29	2017
\.


--
-- Data for Name: blocked_channels; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY blocked_channels (cid, reason) FROM stdin;
\.


--
-- Data for Name: blocked_users; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY blocked_users (uid, reason) FROM stdin;
\.


--
-- Data for Name: emoji_reactions; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY emoji_reactions (gid, trigger, reaction, id) FROM stdin;
201315884709576705	(pepe)? ?rage	:pepe_rage:	1
201315884709576705	(bad ?(time?))|(br(a(s|z)il.*)?)	:pepe__BR_hu3hu3:	2
201315884709576705	smart	:_linux:	3
201315884709576705	suck	:pepe_gulp_gulp:	4
201315884709576705	(g+(a|e)+(y|i)+)|(peder(cina)?)	:_gay:	5
201315884709576705	(ass)|(bo+b(s|(ies|z)))|(tit(s|(ies)))	:pepe_oooh:	6
201315884709576705	hid(e|(ing))?	:pepe_hiding:	7
201315884709576705	fuhrer	:_hitler:	8
201315884709576705	german	:_nazi:	9
201315884709576705	hitler	:_hitler:	10
201315884709576705	nazi	:_nazi:	11
201315884709576705	nein	:_nazi:	12
201315884709576705	wat	:_wat:	13
201315884709576705	(cover)|(hide)	:pepe_hiding:	14
201315884709576705	(4u)|(((for)|4)ever united)	:pepe_disabled:	15
201315884709576705	(47)|(l(i|e)nu((ks)|x))	:_linux:	16
201315884709576705	panter	:pepe_ban_hammer:	17
201315884709576705	goty	:pepe_gay_flag:	18
201315884709576705	monkey	:pepe__afro_american:	19
201315884709576705	think	:pepe_hmmm:	20
201315884709576705	consider	:pepe_hmmm:	21
201315884709576705	thinking	:pepe_hmmm:	22
201315884709576705	measure	:straight_ruler:	23
201315884709576705	size	:straight_ruler:	24
201315884709576705	zugi	:_nazi:	25
201315884709576705	rugi	:_nazi:	26
201315884709576705	tits	:pepe_oooh:	27
201315884709576705	kebab	:pepe__kebab:	28
201315884709576705	(@here)|(@everyone)|(<@!303463460233150464>)(<@303463460233150464>)	:pepe_disturbed:	29
201315884709576705	(8d)|(pain)	:pepe_smol_pener:	30
201315884709576705	(9)|(aush?(ch)?wit?z)	:_nazi:	31
201315884709576705	(asian?)|(chin(a|(ese)))|(char(f(ire)|(rajer))?)	:pepe__ching_chong:	32
201315884709576705	ban+((ed)|(ing))?	:pepe_ban_hammer:	33
201315884709576705	blow((ing)|(jobs?))?	:pepe_gulp_gulp:	34
201315884709576705	(b+o+y+k+a+)|(dildo)	:_dildo:	35
201315884709576705	(disabled?)|(retard(ed)?)|(idiot(s|(ic))?)	:pepe_disabled:	36
201315884709576705	g+a+y+s*	:pepe_gay_flag:	37
201315884709576705	nig+(a|(er)|(let))	:pepe__afro_american:	38
201315884709576705	(emo(pig)?)|((emo)?pig)	:pepe_emo_pig:	39
337570344149975050	bbbb	:')	40
337570344149975050	cc+e*	:')	41
420357097146810378	s?malko(v|m)	:malk:	42
201315884709576705	d(i|e)ck|cocky?	:_8D:	43
201315884709576705	kur(ac|(ci(c|na)))|penis	:_8D:	44
201315884709576705	gun	:pepe____hands_up:	45
\.


--
-- Name: emoji_reactions_id_seq; Type: SEQUENCE SET; Schema: gf; Owner: postgres
--

SELECT pg_catalog.setval('emoji_reactions_id_seq', 45, true);


--
-- Data for Name: feeds; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY feeds (id, url, savedurl) FROM stdin;
11	http://www.cbc.ca/cmlink/rss-topstories	http://www.cbc.ca/news/world/eastern-ghouta-siege-syria-assad-russia-1.4548477?cmp=rss
10	https://www.youtube.com/feeds/videos.xml?channel_id=UC27OPXkuuF3Xdb1mSLbHThg	https://www.youtube.com/watch?v=_m4p8qlgLV8
12	https://www.reddit.com/r/aww/new/.rss	https://www.reddit.com/r/aww/comments/80gzv1/my_golden_retriever_with_her_golden_retriever/
13	https://www.youtube.com/feeds/videos.xml?channel_id=UCF9i903aeMcvcapmP1bS1oA	https://www.youtube.com/watch?v=Ce7xqbfriNc
5	https://www.youtube.com/feeds/videos.xml?channel_id=UCYQT13AtrJC0gsM1far_zJg	https://www.youtube.com/watch?v=KPa4UL5qHRc
7	https://www.youtube.com/feeds/videos.xml?channel_id=UCA5u8UquvO44Jcd3wZApyDg	https://www.youtube.com/watch?v=2oVFr2AaDjc
8	https://www.youtube.com/feeds/videos.xml?channel_id=UCIgnupFT6p_RrcFTjxipm0w	https://www.youtube.com/watch?v=rpwfzAKyTPk
6	https://www.reddit.com/r/boobs/new/.rss	https://www.reddit.com/r/boobs/comments/8787qh/titties_tats_oc/
9	https://www.reddit.com/r/ass/new/.rss	https://www.reddit.com/r/ass/comments/8788kv/comfy_socks/
\.


--
-- Name: feeds_id_seq; Type: SEQUENCE SET; Schema: gf; Owner: postgres
--

SELECT pg_catalog.setval('feeds_id_seq', 13, true);


--
-- Data for Name: filters; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY filters (gid, filter) FROM stdin;
201315884709576705	90\\.224\\.108\\.75
201315884709576705	201309107267960832
201315884709576705	90\\.224\\.108\\.224
332969689246334976	l+(i|e|y)+n+u+(ks|x)+(\\?)?
337570344149975050	90\\.224\\.108\\.75
337570344149975050	90\\.224\\.108\\.224
420357097146810378	analiza[123]?
201315884709576705	201315884709576705
\.


--
-- Data for Name: guild_cfg; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY guild_cfg (gid, welcome_cid, leave_cid, welcome_msg, leave_msg) FROM stdin;
236994739201769473	\N	\N	\N	\N
332969689246334976	\N	\N	\N	\N
337570344149975050	361119455792594954	0	\N	\N
201315884709576705	201315884709576705	201315884709576705	Welcome, %user%! To subscribe for notifications in this server (free games for example), type ``!giveme Announcements``.	%user% has left the server. (pussy)
420357097146810378	420357097146810380	420357097146810380	%user% je konacno instalirao discord i dosao da se druzi sa nama	%user% je napustio server
\.


--
-- Data for Name: insults; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY insults (id, insult) FROM stdin;
4	Shut up %user%, you will never be the man your mother is.
5	%user% you are a failed abortion whose birth certificate is an apology from the condom factory.
6	%user%, you must have been born on a highway, because that is where most accidents happen.
7	%user%, you're so ugly Hello Kitty said goodbye to you.
8	%user%, you're so ugly that when your mama dropped you off at school she got a fine for littering.
9	%user% it looks like your face caught on fire and someone tried to put it out with a fork.
10	Your family tree must be a cactus, %user%, because everybody on it is a prick.
11	Do you have to leave so soon %user%? I was just about to poison the tea...
12	%user%, you dumbass.
13	%user% is your ass jealous of the amount of shit that just came out of your mouth?
14	%user%, if I wanted to kill myself I'd climb your ego and jump to your IQ.
15	%user%, I'd like to see things from your point of view but I can't seem to get my head that far up my ass.
16	%user% is so old that he gets nostalgic when he sees the Neolithic cave paintings.
17	%user%, you're old enough to remember when emojis were called \\"hieroglyphics\\".
18	Two wrongs don't make a right %user%, take your parents as an example.
19	If laughter is the best medicine, %user%'s face must be curing the world.
20	If ignorance is bliss, %user% must be the happiest person on earth.
21	%user%, I don't engage in mental combat with the unarmed.
22	%user%, the only way you'll ever get laid is if you crawl up a chicken's ass and wait.
23	I wasn't born with enough middle fingers to let you know how I feel about you %user%.
24	Hey %user%, you have something on your chin... No, the 3rd one down.
25	%user% if I had a face like yours, I'd sue my parents.
26	I'm blonde. What's your excuse %user%?
27	%user% shock me, say something intelligent.
28	%user%, you're the reason the gene pool needs a lifeguard.
29	%user%, you're so fake, Barbie is jealous.
30	So, a thought crossed your mind %user%? Must have been a long and lonely journey.
31	%user%, don't you love nature, despite what it did to you?
32	%user% what language are you speaking? Cause it sounds like bullshit.
33	I would love to insult you %user%, but that would be beyond the level of your intelligence.
34	%user%, you do realize makeup isn't going to fix your stupidity?
35	%user%, hell is wallpapered with all your deleted selfies."
36	Aha, I see the Fuck-Up Fairy has visited us again giving us %user%!
37	I may love to shop but I'm not buying your bullshit %user%.
38	%user%, you sound reasonable. It must be time to up my medication!
39	%user%, as an outsider, what do you think of the human race?
\.


--
-- Name: insults_id_seq; Type: SEQUENCE SET; Schema: gf; Owner: postgres
--

SELECT pg_catalog.setval('insults_id_seq', 39, true);


--
-- Data for Name: memes; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY memes (gid, name, url) FROM stdin;
201315884709576705	banwm	https://i.imgur.com/hnDB9UV.jpg
201315884709576705	stfu4	https://i.imgur.com/hdl2FX8.jpg
201315884709576705	mazso	https://i.imgur.com/IrlGMGo.png
201315884709576705	rugimakefun	https://i.imgur.com/BeedXK9.jpg
201315884709576705	4lifeleave	https://i.imgur.com/gzLGPDd.jpg
201315884709576705	fap	https://i.imgur.com/aYEGsxh.gif
201315884709576705	juicedpc	https://i.imgur.com/KYwIfYR.jpg
201315884709576705	rugipayment	https://i.imgur.com/vMhZVv3.jpg
201315884709576705	4uvsltm	https://i.imgur.com/IS6udqN.png
201315884709576705	markie	https://i.imgur.com/wzLfO9y.png
201315884709576705	fapcave	https://i.imgur.com/LteB8cX.jpg
201315884709576705	eyes	https://i.imgur.com/RBJD5nh.jpg
201315884709576705	rugiserver	https://i.imgur.com/IUlOdDl.jpg
201315884709576705	swatservers	https://i.imgur.com/DuBcuke.jpg
201315884709576705	cojones	https://i.imgur.com/kjC6rwV.jpg
201315884709576705	cockcopter	https://i.imgur.com/DwF9tc8.gif
201315884709576705	4lifeserver	https://i.imgur.com/RGibyYl.jpg
201315884709576705	panter	https://i.imgur.com/yLkx2uK.png
201315884709576705	jojoreply2	https://i.imgur.com/qTUloRW.png
201315884709576705	zyklon	https://i.imgur.com/yHoUdvx.jpg
201315884709576705	banana	https://i.imgur.com/5Jn2ylk.jpg
201315884709576705	vitesscake	https://i.imgur.com/tqVbMbC.png
201315884709576705	alex	https://i.imgur.com/mStaB9l.png
201315884709576705	rebi	https://i.imgur.com/sp9t0Vq.png
201315884709576705	fap3	https://i.imgur.com/mqvvsjs.gif
201315884709576705	donations	https://i.imgur.com/Atcrimv.png
201315884709576705	pardon	https://i.imgur.com/QttVUtZ.jpg
201315884709576705	calm	https://i.imgur.com/XnEhfha.png
201315884709576705	bravokick4life	https://i.imgur.com/c1BEVOD.png
201315884709576705	spawnkill	https://i.imgur.com/O9ms1vW.jpg
201315884709576705	civilwar	https://i.imgur.com/CAv6g7B.jpg
201315884709576705	kimjoun	https://i.imgur.com/1rOiw00.png
201315884709576705	granny	http://i.imgur.com/xmdy9sJ.gif
201315884709576705	4lifememes	https://i.imgur.com/wUC2o33.jpg
201315884709576705	abuse	https://i.imgur.com/NfxP6ED.jpg
201315884709576705	peperage	https://i.imgur.com/nwRLMJT.gif
201315884709576705	titi	https://i.imgur.com/LtpTewU.jpg
201315884709576705	jojoreply	https://i.imgur.com/AUaaoiG.jpg
201315884709576705	dealwithit	http://img-9gag-lol.9cache.com/photo/ae3Q6yO_460sa_v1.gif
201315884709576705	halo	https://i.imgur.com/bElaJ0D.png
201315884709576705	soenotsoh	https://i.imgur.com/zhHfrHf.jpg
201315884709576705	rugi	https://i.imgur.com/Y2VvPNs.jpg
201315884709576705	fap2	https://i.imgur.com/6rMWjeP.gif
201315884709576705	8d	https://i.imgur.com/wMIWU9M.png
201315884709576705	maxjoin	https://i.imgur.com/48G6SeI.png
337570344149975050	test	https://cdn.discordapp.com/attachments/361119455792594954/415242473938616320/evilracoon.jpg
337570344149975050	test2	http://i0.kym-cdn.com/photos/images/facebook/001/217/729/f9a.jpg
337570344149975050	pepe	http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg
\.


--
-- Data for Name: msgcount; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY msgcount (uid, count) FROM stdin;
368139918196867090	1
290398127012446221	7
201315299042131970	95
360891983675523074	67
273491399247265792	2
353881771475075072	33
154907962169753600	14
262684271737569280	1
369219870619074563	1
165878893532676096	11
362581849983287306	1
217005749589639169	1
228261125966266380	2
381854207621201920	3
259029137421762561	23
249219884993478657	16
236428034377252865	1
381907129495453699	1
229986751542853632	1
155999779820666880	3
139811257422184448	2
182842265817972737	15
300182458924531723	144
394226949654052865	1
361542478068842508	1
370187405476757525	11
113872135268728837	1
229975418466336768	7
394814102658220032	1
227391187730956290	11
316233512799830016	372
235457336750243850	1
379708708747345940	2
207937299730661376	12
274183519830409216	172
197828025251921920	30
246958507864096768	25
397900968970289165	1
402229799507984384	1
311322609755226114	528
389530406011011072	1
315822154803576832	21
306457082045923328	256
292831561408184331	79
410037932792217612	1
368761831256489985	297
321963531723603968	71
234232467123339264	2
252070801476419584	167
282518534179782657	34
415993157646286849	1
254373265215193088	7
215907034590085120	12
227812400655499264	4
230270254113226754	12
302959303525138434	483
330133515737890816	621
229367984701833216	2
420362473208676394	21
243063532097241089	80
394226449856462849	3
300769588143194132	54
244118890618028062	33
420490683762343936	2
420553113959923732	1
356696991490768896	13
220214154500374530	155
228149139198836737	45
423246573728956436	1
195280510732206080	73
141801113761480705	1
277297018676707328	548
344333577544269824	11
290948683044749322	500
226822346063740928	1
306465199165145101	423
420949917574627328	93
234375713183105024	1049
201292112711516160	118
318896140424577034	162
224540765173317632	383
216783156848754688	7
322842803354730497	106
424991311481798666	28
217918127739109376	1664
201316355356622849	2936
234388409932709888	2982
201317516407078913	4472
222076305850630144	5294
154556794490847232	12110
201309107267960832	21512
233567317949284352	2034
\.


--
-- Data for Name: prefixes; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY prefixes (gid, prefix) FROM stdin;
337570344149975050	!
\.


--
-- Data for Name: saved_tasks; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY saved_tasks (id, type, uid, cid, gid, comment, execution_time) FROM stdin;
\.


--
-- Name: saved_tasks_id_seq; Type: SEQUENCE SET; Schema: gf; Owner: postgres
--

SELECT pg_catalog.setval('saved_tasks_id_seq', 2, true);


--
-- Data for Name: stats; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY stats (uid, duels_won, duels_lost, hangman_won, numraces_won, quizes_won, races_won, ttt_won, ttt_lost, chain4_won, chain4_lost, caro_won, caro_lost, othello_won, othello_lost) FROM stdin;
330133515737890816	1	0	0	0	0	0	0	0	0	0	0	0	0	0
234375713183105024	0	0	0	0	0	1	0	0	0	0	0	0	0	0
385798999996891137	0	0	0	2	0	5	0	0	0	0	0	0	0	0
411196626451824652	0	0	0	0	0	0	1	2	0	1	0	2	0	1
420949917574627328	0	0	0	0	0	1	0	0	0	0	0	0	0	0
303463460233150464	0	0	0	0	0	0	0	1	0	2	1	7	0	0
195280510732206080	0	0	0	1	0	0	0	0	0	0	0	0	0	0
217918127739109376	1	0	0	0	0	0	0	0	0	0	0	0	0	0
201316355356622849	0	1	0	0	0	0	0	0	0	0	0	0	0	0
234388409932709888	4	3	0	0	0	0	0	0	0	0	0	0	0	0
201309107267960832	6	5	8	3	1	6	3	1	3	0	9	1	1	0
154556794490847232	3	3	0	0	0	0	0	0	0	0	0	0	0	0
201317516407078913	7	7	0	0	0	0	0	0	0	0	0	0	0	0
222076305850630144	4	7	0	1	0	0	0	0	0	0	0	0	0	0
\.


--
-- Data for Name: statuses; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY statuses (status, type, id) FROM stdin;
@TheGodfather help	0	2
porn	3	3
SWAT4 in 4K	1	4
worldmafia.net/discord	3	1
Half-Life 3	0	9
SWAT 5	0	10
\.


--
-- Name: statuses_id_seq; Type: SEQUENCE SET; Schema: gf; Owner: postgres
--

SELECT pg_catalog.setval('statuses_id_seq', 10, true);


--
-- Data for Name: subscriptions; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY subscriptions (id, cid, qname) FROM stdin;
5	357649243910963201	FlyingKitty
6	371249107928350720	/r/boobs
7	357649243910963201	Linux
8	357649243910963201	Infarlock
10	357649243910963201	SwedishThreesome
9	371249107928350720	ass
\.


--
-- Name: subscriptions_id_seq; Type: SEQUENCE SET; Schema: gf; Owner: postgres
--

SELECT pg_catalog.setval('subscriptions_id_seq', 2, true);


--
-- Data for Name: swat_servers; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY swat_servers (ip, joinport, queryport, name) FROM stdin;
109.70.149.158	10480	10481	4u
31.186.250.32	10480	10481	kos
51.15.152.220	10480	10481	myt
5.9.50.39	8480	8481	sh
89.163.135.60	10480	10481	wm
\.


--
-- Data for Name: text_reactions; Type: TABLE DATA; Schema: gf; Owner: postgres
--

COPY text_reactions (gid, trigger, response, id) FROM stdin;
201315884709576705	payment	https://www.youtube.com/watch?v=Bv3ol2FkGPk	1
201315884709576705	suck itself	https://cdn.discordapp.com/attachments/303620791969513473/416376694497345536/lol.jpg	2
201315884709576705	(u*h+u+(e|3)+)+	https://www.youtube.com/watch?v=-Smxb-EmCqI	3
201315884709576705	genius	https://pixel.nymag.com/imgs/daily/selectall/2017/02/13/wojak_05.nocrop.w710.h2147483647.jpg	4
201315884709576705	(😮)|(:o+)|(open_mouth)	https://i.imgur.com/e3L1eLI.jpg	5
201315884709576705	murica	https://www.youtube.com/watch?v=IhnUgAaea4M	6
201315884709576705	(😃)|(:\\))|(smile)	https://i.imgur.com/pY58Fn8.jpg	7
201315884709576705	low	https://cdn.discordapp.com/attachments/303620791969513473/381504604426207242/1510522760095.png	8
201315884709576705	on ((the)|(my)) way	https://i.imgur.com/QIQK5mi.jpg	9
201315884709576705	triggered	https://www.youtube.com/watch?v=7HoCKlNsolc	10
201315884709576705	wm wants you!	http://i0.kym-cdn.com/photos/images/newsfeed/000/712/225/7ca.png	11
201315884709576705	cure	https://i.imgur.com/1SE0ZmR.jpg	12
201315884709576705	looks away	https://i.imgur.com/AOzRRQd.jpg	13
201315884709576705	salty	http://i0.kym-cdn.com/photos/images/facebook/000/976/327/98f.png	14
201315884709576705	my work here is done	https://pics.me.me/my-job-here-is-done-but-you-didnt-do-anything-14381907.png	15
201315884709576705	duh	https://i.imgur.com/aivE2Tm.gif	16
201315884709576705	doubt	https://i.ytimg.com/vi/hpbGz9JPadM/hqdefault.jpg	17
201315884709576705	bros	https://i.imgur.com/VEN5soX.jpg	18
201315884709576705	chigga	https://images-cdn.9gag.com/photo/5705876_700b.jpg	19
201315884709576705	fuck logic	https://giphy.com/gifs/funny-black-and-white-bampw-kNpPdmczklF8Q	20
201315884709576705	okay	https://giphy.com/gifs/idiocracy-mike-jude-3o7TKy3KWDYOA7OUSI	21
201315884709576705	bullshit	https://media.giphy.com/media/l3mZliEdhZg89oFRS/giphy.gif	22
201315884709576705	perhaps	https://i.imgur.com/b0ihKFf.jpg	23
201315884709576705	snappy	https://i.imgur.com/TUXAqMW.gif	24
201315884709576705	sad violin	https://www.youtube.com/watch?v=_R9gVc9ggZg	25
201315884709576705	i hate you	http://s2.quickmeme.com/img/c4/c496e1a95be52ef7a763c97feab1af34481d0907cce78a7e15cc788b0cb864f8.jpg	26
201315884709576705	:o+	https://i.imgur.com/e3L1eLI.jpg	27
201315884709576705	looks fine	https://i.imgur.com/h5ET1uR.jpg	28
201315884709576705	deal with it	http://img-9gag-lol.9cache.com/photo/ae3Q6yO_460sa_v1.gif	29
201315884709576705	nsa	https://cdn.discordapp.com/attachments/224287575718756352/390447885218611202/image.jpg	30
201315884709576705	drinks poison	https://img-comment-fun.9cache.com/media/aebmy8b/a3M5zlz4_700w_0.jpg	31
201315884709576705	pathetic	http://i0.kym-cdn.com/entries/icons/mobile/000/022/017/thumb.jpg	32
201315884709576705	osas	https://www.youtube.com/watch?v=ul49TvvRxnE	33
201315884709576705	:\\)	https://i.imgur.com/pY58Fn8.jpg	34
201315884709576705	all in (1|one)	https://img.4plebs.org/boards/pol/image/1459/12/1459127105560.gif	35
201315884709576705	(am i )?((disabled)|(retard(ed)?)\\??)	https://archive-media-0.nyafuu.org/wsr/image/1504/74/1504741147513.jpg	36
201315884709576705	brute?	https://i.imgur.com/4uqAtUl.jpg	37
201315884709576705	(didnt read( lol)?)|(tl(:| )?dr)	https://www.youtube.com/watch?v=5GgflscOmW8	38
201315884709576705	(emo)?pig	https://i.imgur.com/BvbeeuH.gif	39
201315884709576705	(emo)?pig2	https://i.imgur.com/YuGjPo4.gif	40
201315884709576705	(emo)?pig3	https://www.tenor.co/MtUG.gif	41
201315884709576705	(emo)?pig4	https://pbs.twimg.com/media/B8Ty0xLIEAIzMkl.jpg	42
201315884709576705	(emo)?pig ?gf	https://www.tenor.co/G4wN.gif	43
201315884709576705	feel(s|(ium))?	https://img.memecdn.com/feelium_o_1007518.jpg	44
201315884709576705	i have( already)? won	https://i.imgur.com/zB4CJXc.jpg	45
201315884709576705	pepe ?brasil	https://cdn.discordapp.com/attachments/303620791969513473/377044462120992768/1509233804899.png	46
201315884709576705	pepe ?rage	https://cdn.discordapp.com/attachments/201333837253443584/321993860500946944/pepe_rage.gif	47
201315884709576705	pepe ?rs	,  https://img.4plebs.org/boards/pol/image/1483/92/1483920600781.jpg	48
201315884709576705	x-?files	https://www.youtube.com/watch?v=HQoRXhS7vlU	49
201315884709576705	(best)?gore(bra(s|z)il)?	https://cdn.discordapp.com/attachments/303620791969513473/365899734721888256/Pepe_BestGore.png	50
201315884709576705	(c|k)s( |:)?go( bros\\?)?	FUCK OFF ALREADY, %user%! Nobody wants to play fucking csgo with you except your wingman pain now go back to your fapcave!	51
201315884709576705	normies\\?	https://i.imgur.com/ihJAa6u.jpg	52
201315884709576705	blyat	https://www.youtube.com/watch?v=9nQ-PXHV6nw	53
201315884709576705	i failed you	https://media.giphy.com/media/sS8YbjrTzu4KI/giphy.gif	54
201315884709576705	flashbacks?	https://media.giphy.com/media/rBYc4tkIeLCNy/200.gif	55
\.


--
-- Name: text_reactions_id_seq; Type: SEQUENCE SET; Schema: gf; Owner: postgres
--

SELECT pg_catalog.setval('text_reactions_id_seq', 55, true);


--
-- PostgreSQL database dump complete
--

