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


SET search_path = gf, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: accounts; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE accounts (
    uid bigint NOT NULL,
    balance integer DEFAULT 0 NOT NULL
);


--
-- Name: assignable_roles; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE assignable_roles (
    gid bigint NOT NULL,
    rid bigint NOT NULL
);


--
-- Name: automatic_roles; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE automatic_roles (
    gid bigint NOT NULL,
    rid bigint NOT NULL
);


--
-- Name: birthdays; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE birthdays (
    uid bigint NOT NULL,
    cid bigint NOT NULL,
    bday date NOT NULL,
    last_updated integer
);


--
-- Name: blocked_channels; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE blocked_channels (
    cid bigint NOT NULL,
    reason character varying(64)
);


--
-- Name: blocked_users; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE blocked_users (
    uid bigint NOT NULL,
    reason character varying(64)
);


--
-- Name: emoji_reactions; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE emoji_reactions (
    gid bigint NOT NULL,
    trigger character varying(128) NOT NULL,
    reaction character varying(64),
    id integer NOT NULL
);


--
-- Name: emoji_reactions_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE emoji_reactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: emoji_reactions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE emoji_reactions_id_seq OWNED BY emoji_reactions.id;


--
-- Name: feeds; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE feeds (
    id integer NOT NULL,
    url text NOT NULL,
    savedurl text DEFAULT ''::text NOT NULL
);


--
-- Name: feeds_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE feeds_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: feeds_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE feeds_id_seq OWNED BY feeds.id;


--
-- Name: filters; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE filters (
    gid bigint NOT NULL,
    filter character varying(64) NOT NULL
);


--
-- Name: guild_cfg; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE guild_cfg (
    gid bigint NOT NULL,
    welcome_cid bigint,
    leave_cid bigint,
    welcome_msg character varying(128),
    leave_msg character varying(128)
);


--
-- Name: insults; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE insults (
    id integer NOT NULL,
    insult character varying(128)
);


--
-- Name: insults_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE insults_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: insults_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE insults_id_seq OWNED BY insults.id;


--
-- Name: memes; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE memes (
    gid bigint NOT NULL,
    name character varying(32) NOT NULL,
    url character varying(128) NOT NULL
);


--
-- Name: msgcount; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE msgcount (
    uid bigint NOT NULL,
    count bigint DEFAULT 1 NOT NULL
);


--
-- Name: prefixes; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE prefixes (
    gid bigint NOT NULL,
    prefix character varying(16)
);


--
-- Name: saved_tasks; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE saved_tasks (
    id integer NOT NULL,
    type smallint NOT NULL,
    uid bigint NOT NULL,
    cid bigint NOT NULL,
    gid bigint NOT NULL,
    comment character varying(128),
    execution_time timestamp(0) without time zone NOT NULL
);


--
-- Name: saved_tasks_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE saved_tasks_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: saved_tasks_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE saved_tasks_id_seq OWNED BY saved_tasks.id;


--
-- Name: stats; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE stats (
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

CREATE TABLE statuses (
    status character varying(64),
    type smallint DEFAULT 0 NOT NULL,
    id integer NOT NULL
);


--
-- Name: statuses_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE statuses_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: statuses_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE statuses_id_seq OWNED BY statuses.id;


--
-- Name: subscriptions; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE subscriptions (
    id integer NOT NULL,
    cid bigint NOT NULL,
    qname character varying(64)
);


--
-- Name: subscriptions_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE subscriptions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: subscriptions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE subscriptions_id_seq OWNED BY subscriptions.id;


--
-- Name: swat_servers; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE swat_servers (
    ip character varying(32) NOT NULL,
    joinport integer NOT NULL,
    queryport integer NOT NULL,
    name character varying(32) NOT NULL
);


--
-- Name: text_reactions; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE text_reactions (
    gid bigint NOT NULL,
    trigger character varying(128) NOT NULL,
    response character varying(128) NOT NULL,
    id integer NOT NULL
);


--
-- Name: text_reactions_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE text_reactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: text_reactions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE text_reactions_id_seq OWNED BY text_reactions.id;


--
-- Name: emoji_reactions id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY emoji_reactions ALTER COLUMN id SET DEFAULT nextval('emoji_reactions_id_seq'::regclass);


--
-- Name: feeds id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY feeds ALTER COLUMN id SET DEFAULT nextval('feeds_id_seq'::regclass);


--
-- Name: insults id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY insults ALTER COLUMN id SET DEFAULT nextval('insults_id_seq'::regclass);


--
-- Name: saved_tasks id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY saved_tasks ALTER COLUMN id SET DEFAULT nextval('saved_tasks_id_seq'::regclass);


--
-- Name: statuses id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY statuses ALTER COLUMN id SET DEFAULT nextval('statuses_id_seq'::regclass);


--
-- Name: subscriptions id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY subscriptions ALTER COLUMN id SET DEFAULT nextval('subscriptions_id_seq'::regclass);


--
-- Name: text_reactions id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY text_reactions ALTER COLUMN id SET DEFAULT nextval('text_reactions_id_seq'::regclass);


--
-- Name: accounts accounts_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY accounts
    ADD CONSTRAINT accounts_pkey PRIMARY KEY (uid);


--
-- Name: assignable_roles assignable_roles_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY assignable_roles
    ADD CONSTRAINT assignable_roles_pkey PRIMARY KEY (gid, rid);


--
-- Name: automatic_roles automatic_roles_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY automatic_roles
    ADD CONSTRAINT automatic_roles_pkey PRIMARY KEY (gid, rid);


--
-- Name: birthdays birthdays_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY birthdays
    ADD CONSTRAINT birthdays_pkey PRIMARY KEY (uid, cid);


--
-- Name: blocked_channels blocked_channels_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY blocked_channels
    ADD CONSTRAINT blocked_channels_pkey PRIMARY KEY (cid);


--
-- Name: blocked_users blocked_users_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY blocked_users
    ADD CONSTRAINT blocked_users_pkey PRIMARY KEY (uid);


--
-- Name: emoji_reactions emoji_reactions_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY emoji_reactions
    ADD CONSTRAINT emoji_reactions_pkey PRIMARY KEY (id);


--
-- Name: feeds feeds_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY feeds
    ADD CONSTRAINT feeds_pkey PRIMARY KEY (id);


--
-- Name: feeds feeds_url_key; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY feeds
    ADD CONSTRAINT feeds_url_key UNIQUE (url);


--
-- Name: filters filters_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY filters
    ADD CONSTRAINT filters_pkey PRIMARY KEY (gid, filter);


--
-- Name: guild_cfg guild_cfg_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY guild_cfg
    ADD CONSTRAINT guild_cfg_pkey PRIMARY KEY (gid);


--
-- Name: insults insults_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY insults
    ADD CONSTRAINT insults_pkey PRIMARY KEY (id);


--
-- Name: memes memes_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY memes
    ADD CONSTRAINT memes_pkey PRIMARY KEY (gid, name);


--
-- Name: msgcount msgcount_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY msgcount
    ADD CONSTRAINT msgcount_pkey PRIMARY KEY (uid);


--
-- Name: prefixes prefixes_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY prefixes
    ADD CONSTRAINT prefixes_pkey PRIMARY KEY (gid);


--
-- Name: saved_tasks saved_tasks_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY saved_tasks
    ADD CONSTRAINT saved_tasks_pkey PRIMARY KEY (id);


--
-- Name: stats stats_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY stats
    ADD CONSTRAINT stats_pkey PRIMARY KEY (uid);


--
-- Name: statuses statuses_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY statuses
    ADD CONSTRAINT statuses_pkey PRIMARY KEY (id);


--
-- Name: subscriptions subscriptions_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY subscriptions
    ADD CONSTRAINT subscriptions_pkey PRIMARY KEY (id, cid);


--
-- Name: swat_servers swat_servers_name_key; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY swat_servers
    ADD CONSTRAINT swat_servers_name_key UNIQUE (name);


--
-- Name: swat_servers swat_servers_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY swat_servers
    ADD CONSTRAINT swat_servers_pkey PRIMARY KEY (ip);


--
-- Name: text_reactions text_reactions_gid_trigger_key; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY text_reactions
    ADD CONSTRAINT text_reactions_gid_trigger_key UNIQUE (gid, trigger);


--
-- Name: text_reactions text_reactions_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY text_reactions
    ADD CONSTRAINT text_reactions_pkey PRIMARY KEY (id);


--
-- Name: emoji_reactions_trigger_idx; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX emoji_reactions_trigger_idx ON emoji_reactions USING btree (trigger);


--
-- Name: fki_savedtasks_fkey; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX fki_savedtasks_fkey ON saved_tasks USING btree (gid);


--
-- Name: gid_index; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX gid_index ON filters USING btree (gid);


--
-- Name: index_bday; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX index_bday ON birthdays USING btree (bday);

ALTER TABLE birthdays CLUSTER ON index_bday;


--
-- Name: index_er_gid; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX index_er_gid ON emoji_reactions USING btree (gid);

ALTER TABLE emoji_reactions CLUSTER ON index_er_gid;


--
-- Name: index_filters_gid; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX index_filters_gid ON filters USING btree (gid);

ALTER TABLE filters CLUSTER ON index_filters_gid;


--
-- Name: index_memes_cluster; Type: INDEX; Schema: gf; Owner: -
--

CREATE UNIQUE INDEX index_memes_cluster ON memes USING btree (gid, name);

ALTER TABLE memes CLUSTER ON index_memes_cluster;


--
-- Name: index_tr_gid; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX index_tr_gid ON text_reactions USING btree (gid);

ALTER TABLE text_reactions CLUSTER ON index_tr_gid;


--
-- Name: trigger_index; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX trigger_index ON text_reactions USING btree (trigger);


--
-- Name: automatic_roles ar_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY automatic_roles
    ADD CONSTRAINT ar_fkey FOREIGN KEY (gid) REFERENCES guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: emoji_reactions er_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY emoji_reactions
    ADD CONSTRAINT er_fkey FOREIGN KEY (gid) REFERENCES guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: filters f_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY filters
    ADD CONSTRAINT f_fkey FOREIGN KEY (gid) REFERENCES guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: memes memes_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY memes
    ADD CONSTRAINT memes_fkey FOREIGN KEY (gid) REFERENCES guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: prefixes p_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY prefixes
    ADD CONSTRAINT p_fkey FOREIGN KEY (gid) REFERENCES guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: assignable_roles sar_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY assignable_roles
    ADD CONSTRAINT sar_fkey FOREIGN KEY (gid) REFERENCES guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: saved_tasks st_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY saved_tasks
    ADD CONSTRAINT st_fkey FOREIGN KEY (gid) REFERENCES guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: subscriptions subscriptions_id_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY subscriptions
    ADD CONSTRAINT subscriptions_id_fkey FOREIGN KEY (id) REFERENCES feeds(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: text_reactions tr_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY text_reactions
    ADD CONSTRAINT tr_fkey FOREIGN KEY (gid) REFERENCES guild_cfg(gid) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

