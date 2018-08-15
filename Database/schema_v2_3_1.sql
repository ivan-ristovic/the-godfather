--
-- PostgreSQL database dump
--

-- Dumped from database version 10.4
-- Dumped by pg_dump version 10.4

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: gf; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA gf;


--
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- Name: fuzzystrmatch; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS fuzzystrmatch WITH SCHEMA gf;


--
-- Name: EXTENSION fuzzystrmatch; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION fuzzystrmatch IS 'determine similarities and distance between strings';


SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: accounts; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.accounts (
    uid bigint DEFAULT 0 NOT NULL,
    balance bigint DEFAULT 0 NOT NULL,
    gid bigint DEFAULT 0 NOT NULL
);


--
-- Name: assignable_roles; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.assignable_roles (
    gid bigint NOT NULL,
    rid bigint NOT NULL
);


--
-- Name: automatic_roles; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.automatic_roles (
    gid bigint NOT NULL,
    rid bigint NOT NULL
);


--
-- Name: birthdays; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.birthdays (
    uid bigint NOT NULL,
    cid bigint NOT NULL,
    bday date NOT NULL,
    last_updated integer
);


--
-- Name: blocked_channels; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.blocked_channels (
    cid bigint NOT NULL,
    reason character varying(64)
);


--
-- Name: blocked_users; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.blocked_users (
    uid bigint NOT NULL,
    reason character varying(64)
);


--
-- Name: chicken_active_upgrades; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.chicken_active_upgrades (
    uid bigint NOT NULL,
    gid bigint NOT NULL,
    wid integer NOT NULL
);


--
-- Name: chicken_upgrades; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.chicken_upgrades (
    wid integer NOT NULL,
    name character varying(32) NOT NULL,
    price bigint DEFAULT 0 NOT NULL,
    upgrades_stat smallint DEFAULT 0 NOT NULL,
    modifier integer DEFAULT 0 NOT NULL
);


--
-- Name: chicken_weapons_wid_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.chicken_weapons_wid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: chicken_weapons_wid_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.chicken_weapons_wid_seq OWNED BY gf.chicken_upgrades.wid;


--
-- Name: chickens; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.chickens (
    uid bigint NOT NULL,
    gid bigint NOT NULL,
    name character varying(32) NOT NULL,
    strength integer DEFAULT 50 NOT NULL,
    vitality integer DEFAULT 100 NOT NULL,
    max_vitality integer DEFAULT 100 NOT NULL
);


--
-- Name: COLUMN chickens.max_vitality; Type: COMMENT; Schema: gf; Owner: -
--

COMMENT ON COLUMN gf.chickens.max_vitality IS '
';


--
-- Name: emoji_reactions; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.emoji_reactions (
    gid bigint NOT NULL,
    trigger character varying(128) NOT NULL,
    reaction character varying(64),
    id integer NOT NULL
);


--
-- Name: emoji_reactions_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.emoji_reactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: emoji_reactions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.emoji_reactions_id_seq OWNED BY gf.emoji_reactions.id;


--
-- Name: feeds; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.feeds (
    id integer NOT NULL,
    url text NOT NULL,
    savedurl text DEFAULT ''::text NOT NULL
);


--
-- Name: feeds_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.feeds_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: feeds_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.feeds_id_seq OWNED BY gf.feeds.id;


--
-- Name: filters; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.filters (
    gid bigint NOT NULL,
    filter character varying(64) NOT NULL,
    id integer NOT NULL
);


--
-- Name: filters_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.filters_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: filters_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.filters_id_seq OWNED BY gf.filters.id;


--
-- Name: guild_cfg; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.guild_cfg (
    gid bigint NOT NULL,
    welcome_cid bigint DEFAULT 0 NOT NULL,
    leave_cid bigint DEFAULT 0 NOT NULL,
    welcome_msg character varying(128),
    leave_msg character varying(128),
    prefix character varying(16) DEFAULT NULL::character varying,
    suggestions_enabled boolean DEFAULT false NOT NULL,
    log_cid bigint DEFAULT 0 NOT NULL,
    linkfilter_enabled boolean DEFAULT false NOT NULL,
    linkfilter_invites boolean DEFAULT false NOT NULL,
    linkfilter_booters boolean DEFAULT true NOT NULL,
    linkfilter_disturbing boolean DEFAULT true NOT NULL,
    linkfilter_iploggers boolean DEFAULT true NOT NULL,
    linkfilter_shorteners boolean DEFAULT true NOT NULL,
    silent_respond boolean DEFAULT true NOT NULL,
    currency character varying(32) DEFAULT NULL::character varying,
    ratelimit_enabled boolean DEFAULT false NOT NULL,
    ratelimit_action smallint DEFAULT 0 NOT NULL,
    ratelimit_sens smallint DEFAULT 5 NOT NULL,
    antiflood_enabled boolean DEFAULT false NOT NULL,
    antiflood_sens smallint DEFAULT 5 NOT NULL,
    antiflood_cooldown smallint DEFAULT 10 NOT NULL,
    antiflood_action smallint DEFAULT 2 NOT NULL
);


--
-- Name: insults; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.insults (
    id integer NOT NULL,
    insult character varying(128)
);


--
-- Name: insults_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.insults_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: insults_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.insults_id_seq OWNED BY gf.insults.id;


--
-- Name: items; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.items (
    id integer NOT NULL,
    gid bigint NOT NULL,
    name character varying(64) NOT NULL,
    price bigint NOT NULL
);


--
-- Name: items_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.items_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: items_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.items_id_seq OWNED BY gf.items.id;


--
-- Name: log_exempt; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.log_exempt (
    id bigint NOT NULL,
    type character(1) NOT NULL,
    gid bigint NOT NULL
);


--
-- Name: memes; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.memes (
    gid bigint NOT NULL,
    name character varying(32) NOT NULL,
    url character varying(128) NOT NULL
);


--
-- Name: msgcount; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.msgcount (
    uid bigint NOT NULL,
    count bigint DEFAULT 1 NOT NULL
);


--
-- Name: privileged; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.privileged (
    uid bigint NOT NULL
);


--
-- Name: purchases; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.purchases (
    id integer NOT NULL,
    uid bigint NOT NULL
);


--
-- Name: purchases_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.purchases_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: purchases_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.purchases_id_seq OWNED BY gf.purchases.id;


--
-- Name: ranks; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.ranks (
    gid bigint NOT NULL,
    rank smallint NOT NULL,
    name character varying(32) NOT NULL
);


--
-- Name: saved_tasks; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.saved_tasks (
    id integer NOT NULL,
    type smallint NOT NULL,
    uid bigint,
    cid bigint,
    gid bigint,
    comment character varying(128),
    execution_time timestamp with time zone NOT NULL,
    rid bigint DEFAULT 0
);


--
-- Name: saved_tasks_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.saved_tasks_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: saved_tasks_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.saved_tasks_id_seq OWNED BY gf.saved_tasks.id;


--
-- Name: stats; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.stats (
    uid bigint NOT NULL,
    duels_won integer DEFAULT 0 NOT NULL,
    duels_lost integer DEFAULT 0 NOT NULL,
    hangman_won integer DEFAULT 0 NOT NULL,
    numraces_won integer DEFAULT 0 NOT NULL,
    quizes_won integer DEFAULT 0 NOT NULL,
    races_won integer DEFAULT 0 NOT NULL,
    ttt_won integer DEFAULT 0 NOT NULL,
    ttt_lost integer DEFAULT 0 NOT NULL,
    chain4_won integer DEFAULT 0 NOT NULL,
    chain4_lost integer DEFAULT 0 NOT NULL,
    caro_won integer DEFAULT 0 NOT NULL,
    caro_lost integer DEFAULT 0 NOT NULL,
    othello_won integer DEFAULT 0 NOT NULL,
    othello_lost integer DEFAULT 0 NOT NULL
);


--
-- Name: statuses; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.statuses (
    status character varying(64),
    type smallint DEFAULT 0 NOT NULL,
    id integer NOT NULL
);


--
-- Name: statuses_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.statuses_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: statuses_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.statuses_id_seq OWNED BY gf.statuses.id;


--
-- Name: subscriptions; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.subscriptions (
    id integer NOT NULL,
    cid bigint NOT NULL,
    qname character varying(64)
);


--
-- Name: subscriptions_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.subscriptions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: subscriptions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.subscriptions_id_seq OWNED BY gf.subscriptions.id;


--
-- Name: swat_banlist; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.swat_banlist (
    name character varying(32) NOT NULL,
    ip character varying(16) NOT NULL,
    reason character varying(64) DEFAULT NULL::character varying
);


--
-- Name: swat_ips; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.swat_ips (
    name character varying(32) NOT NULL,
    ip character varying(16) NOT NULL,
    additional_info character varying(128) DEFAULT NULL::character varying
);


--
-- Name: swat_servers; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.swat_servers (
    ip character varying(32) NOT NULL,
    joinport integer NOT NULL,
    queryport integer NOT NULL,
    name character varying(32) NOT NULL
);


--
-- Name: text_reactions; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE gf.text_reactions (
    gid bigint NOT NULL,
    trigger character varying(128) NOT NULL,
    response character varying(128) NOT NULL,
    id integer NOT NULL
);


--
-- Name: text_reactions_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE gf.text_reactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: text_reactions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE gf.text_reactions_id_seq OWNED BY gf.text_reactions.id;


--
-- Name: chicken_upgrades wid; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.chicken_upgrades ALTER COLUMN wid SET DEFAULT nextval('gf.chicken_weapons_wid_seq'::regclass);


--
-- Name: emoji_reactions id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.emoji_reactions ALTER COLUMN id SET DEFAULT nextval('gf.emoji_reactions_id_seq'::regclass);


--
-- Name: feeds id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.feeds ALTER COLUMN id SET DEFAULT nextval('gf.feeds_id_seq'::regclass);


--
-- Name: filters id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.filters ALTER COLUMN id SET DEFAULT nextval('gf.filters_id_seq'::regclass);


--
-- Name: insults id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.insults ALTER COLUMN id SET DEFAULT nextval('gf.insults_id_seq'::regclass);


--
-- Name: items id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.items ALTER COLUMN id SET DEFAULT nextval('gf.items_id_seq'::regclass);


--
-- Name: purchases id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.purchases ALTER COLUMN id SET DEFAULT nextval('gf.purchases_id_seq'::regclass);


--
-- Name: saved_tasks id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.saved_tasks ALTER COLUMN id SET DEFAULT nextval('gf.saved_tasks_id_seq'::regclass);


--
-- Name: statuses id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.statuses ALTER COLUMN id SET DEFAULT nextval('gf.statuses_id_seq'::regclass);


--
-- Name: subscriptions id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.subscriptions ALTER COLUMN id SET DEFAULT nextval('gf.subscriptions_id_seq'::regclass);


--
-- Name: text_reactions id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.text_reactions ALTER COLUMN id SET DEFAULT nextval('gf.text_reactions_id_seq'::regclass);


--
-- Name: accounts accounts_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.accounts
    ADD CONSTRAINT accounts_pkey PRIMARY KEY (uid, gid);


--
-- Name: assignable_roles assignable_roles_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.assignable_roles
    ADD CONSTRAINT assignable_roles_pkey PRIMARY KEY (gid, rid);


--
-- Name: automatic_roles automatic_roles_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.automatic_roles
    ADD CONSTRAINT automatic_roles_pkey PRIMARY KEY (gid, rid);


--
-- Name: birthdays birthdays_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.birthdays
    ADD CONSTRAINT birthdays_pkey PRIMARY KEY (uid, cid);


--
-- Name: blocked_channels blocked_channels_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.blocked_channels
    ADD CONSTRAINT blocked_channels_pkey PRIMARY KEY (cid);


--
-- Name: blocked_users blocked_users_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.blocked_users
    ADD CONSTRAINT blocked_users_pkey PRIMARY KEY (uid);


--
-- Name: chicken_active_upgrades chicken_active_upgrades_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.chicken_active_upgrades
    ADD CONSTRAINT chicken_active_upgrades_pkey PRIMARY KEY (uid, gid, wid);


--
-- Name: chicken_upgrades chicken_weapons_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.chicken_upgrades
    ADD CONSTRAINT chicken_weapons_pkey PRIMARY KEY (wid);


--
-- Name: chickens chickens_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.chickens
    ADD CONSTRAINT chickens_pkey PRIMARY KEY (uid, gid);


--
-- Name: emoji_reactions emoji_reactions_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.emoji_reactions
    ADD CONSTRAINT emoji_reactions_pkey PRIMARY KEY (id);


--
-- Name: feeds feeds_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.feeds
    ADD CONSTRAINT feeds_pkey PRIMARY KEY (id);


--
-- Name: feeds feeds_url_key; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.feeds
    ADD CONSTRAINT feeds_url_key UNIQUE (url);


--
-- Name: filters filters_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.filters
    ADD CONSTRAINT filters_pkey PRIMARY KEY (id);


--
-- Name: filters filters_unique; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.filters
    ADD CONSTRAINT filters_unique UNIQUE (filter, gid);


--
-- Name: guild_cfg guild_cfg_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.guild_cfg
    ADD CONSTRAINT guild_cfg_pkey PRIMARY KEY (gid);


--
-- Name: insults insults_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.insults
    ADD CONSTRAINT insults_pkey PRIMARY KEY (id);


--
-- Name: items items_name_unique; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.items
    ADD CONSTRAINT items_name_unique UNIQUE (name);


--
-- Name: items items_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.items
    ADD CONSTRAINT items_pkey PRIMARY KEY (id);


--
-- Name: memes memes_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.memes
    ADD CONSTRAINT memes_pkey PRIMARY KEY (gid, name);


--
-- Name: msgcount msgcount_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.msgcount
    ADD CONSTRAINT msgcount_pkey PRIMARY KEY (uid);


--
-- Name: privileged priviledged_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.privileged
    ADD CONSTRAINT priviledged_pkey PRIMARY KEY (uid);


--
-- Name: purchases purchases_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.purchases
    ADD CONSTRAINT purchases_pkey PRIMARY KEY (id, uid);


--
-- Name: ranks ranks_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.ranks
    ADD CONSTRAINT ranks_pkey PRIMARY KEY (gid, rank);


--
-- Name: saved_tasks saved_tasks_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.saved_tasks
    ADD CONSTRAINT saved_tasks_pkey PRIMARY KEY (id);


--
-- Name: stats stats_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.stats
    ADD CONSTRAINT stats_pkey PRIMARY KEY (uid);


--
-- Name: statuses statuses_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.statuses
    ADD CONSTRAINT statuses_pkey PRIMARY KEY (id);


--
-- Name: subscriptions subscriptions_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.subscriptions
    ADD CONSTRAINT subscriptions_pkey PRIMARY KEY (id, cid);


--
-- Name: swat_banlist swat_banlist_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.swat_banlist
    ADD CONSTRAINT swat_banlist_pkey PRIMARY KEY (ip);


--
-- Name: swat_banlist swat_banlist_unique; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.swat_banlist
    ADD CONSTRAINT swat_banlist_unique UNIQUE (name, ip);


--
-- Name: swat_ips swat_ips_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.swat_ips
    ADD CONSTRAINT swat_ips_pkey PRIMARY KEY (name, ip);


--
-- Name: swat_servers swat_servers_name_key; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.swat_servers
    ADD CONSTRAINT swat_servers_name_key UNIQUE (name);


--
-- Name: swat_servers swat_servers_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.swat_servers
    ADD CONSTRAINT swat_servers_pkey PRIMARY KEY (ip);


--
-- Name: text_reactions text_reactions_gid_trigger_key; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.text_reactions
    ADD CONSTRAINT text_reactions_gid_trigger_key UNIQUE (gid, trigger);


--
-- Name: text_reactions text_reactions_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.text_reactions
    ADD CONSTRAINT text_reactions_pkey PRIMARY KEY (id);


--
-- Name: emoji_reactions_trigger_idx; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX emoji_reactions_trigger_idx ON gf.emoji_reactions USING btree (trigger);


--
-- Name: fki_items_fkey; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX fki_items_fkey ON gf.items USING btree (gid);


--
-- Name: fki_savedtasks_fkey; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX fki_savedtasks_fkey ON gf.saved_tasks USING btree (gid);


--
-- Name: index_bday; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX index_bday ON gf.birthdays USING btree (bday);

ALTER TABLE gf.birthdays CLUSTER ON index_bday;


--
-- Name: index_er_gid; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX index_er_gid ON gf.emoji_reactions USING btree (gid);

ALTER TABLE gf.emoji_reactions CLUSTER ON index_er_gid;


--
-- Name: index_filters_gid; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX index_filters_gid ON gf.filters USING btree (gid);

ALTER TABLE gf.filters CLUSTER ON index_filters_gid;


--
-- Name: index_memes_cluster; Type: INDEX; Schema: gf; Owner: -
--

CREATE UNIQUE INDEX index_memes_cluster ON gf.memes USING btree (gid, name);

ALTER TABLE gf.memes CLUSTER ON index_memes_cluster;


--
-- Name: index_tr_gid; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX index_tr_gid ON gf.text_reactions USING btree (gid);

ALTER TABLE gf.text_reactions CLUSTER ON index_tr_gid;


--
-- Name: items_gid_index; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX items_gid_index ON gf.items USING btree (gid);

ALTER TABLE gf.items CLUSTER ON items_gid_index;


--
-- Name: log_exempt_clustered_index; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX log_exempt_clustered_index ON gf.log_exempt USING btree (gid, type);

ALTER TABLE gf.log_exempt CLUSTER ON log_exempt_clustered_index;


--
-- Name: purchases_id_index; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX purchases_id_index ON gf.purchases USING hash (id);


--
-- Name: purchases_uid_index; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX purchases_uid_index ON gf.purchases USING btree (uid);

ALTER TABLE gf.purchases CLUSTER ON purchases_uid_index;


--
-- Name: trigger_index; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX trigger_index ON gf.text_reactions USING btree (trigger);


--
-- Name: automatic_roles ar_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.automatic_roles
    ADD CONSTRAINT ar_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: chicken_active_upgrades chicken_upgrades_uid_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.chicken_active_upgrades
    ADD CONSTRAINT chicken_upgrades_uid_fkey FOREIGN KEY (uid, gid) REFERENCES gf.chickens(uid, gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: chicken_active_upgrades chicken_upgrades_wid_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.chicken_active_upgrades
    ADD CONSTRAINT chicken_upgrades_wid_fkey FOREIGN KEY (wid) REFERENCES gf.chicken_upgrades(wid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: emoji_reactions er_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.emoji_reactions
    ADD CONSTRAINT er_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: filters f_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.filters
    ADD CONSTRAINT f_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: filters filters_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.filters
    ADD CONSTRAINT filters_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: items items_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.items
    ADD CONSTRAINT items_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: log_exempt log_exempt_gid_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.log_exempt
    ADD CONSTRAINT log_exempt_gid_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: memes memes_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.memes
    ADD CONSTRAINT memes_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: purchases purchases_id_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.purchases
    ADD CONSTRAINT purchases_id_fkey FOREIGN KEY (id) REFERENCES gf.items(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: ranks ranks_gid_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.ranks
    ADD CONSTRAINT ranks_gid_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: assignable_roles sar_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.assignable_roles
    ADD CONSTRAINT sar_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: saved_tasks st_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.saved_tasks
    ADD CONSTRAINT st_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: subscriptions subscriptions_id_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.subscriptions
    ADD CONSTRAINT subscriptions_id_fkey FOREIGN KEY (id) REFERENCES gf.feeds(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: text_reactions tr_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY gf.text_reactions
    ADD CONSTRAINT tr_fkey FOREIGN KEY (gid) REFERENCES gf.guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

