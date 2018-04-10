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
-- Name: gf; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA gf;


ALTER SCHEMA gf OWNER TO postgres;

--
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


SET search_path = gf, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: accounts; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE accounts (
    uid bigint NOT NULL,
    balance integer DEFAULT 0 NOT NULL
);


ALTER TABLE accounts OWNER TO postgres;

--
-- Name: assignable_roles; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE assignable_roles (
    gid bigint NOT NULL,
    rid bigint NOT NULL
);


ALTER TABLE assignable_roles OWNER TO postgres;

--
-- Name: automatic_roles; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE automatic_roles (
    gid bigint NOT NULL,
    rid bigint NOT NULL
);


ALTER TABLE automatic_roles OWNER TO postgres;

--
-- Name: birthdays; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE birthdays (
    uid bigint NOT NULL,
    cid bigint NOT NULL,
    bday date NOT NULL,
    last_updated integer
);


ALTER TABLE birthdays OWNER TO postgres;

--
-- Name: blocked_channels; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE blocked_channels (
    cid bigint NOT NULL,
    reason character varying(64)
);


ALTER TABLE blocked_channels OWNER TO postgres;

--
-- Name: blocked_users; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE blocked_users (
    uid bigint NOT NULL,
    reason character varying(64)
);


ALTER TABLE blocked_users OWNER TO postgres;

--
-- Name: emoji_reactions; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE emoji_reactions (
    gid bigint NOT NULL,
    trigger character varying(128) NOT NULL,
    reaction character varying(64),
    id integer NOT NULL
);


ALTER TABLE emoji_reactions OWNER TO postgres;

--
-- Name: emoji_reactions_id_seq; Type: SEQUENCE; Schema: gf; Owner: postgres
--

CREATE SEQUENCE emoji_reactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE emoji_reactions_id_seq OWNER TO postgres;

--
-- Name: emoji_reactions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE emoji_reactions_id_seq OWNED BY emoji_reactions.id;


--
-- Name: feeds; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE feeds (
    id integer NOT NULL,
    url text NOT NULL,
    savedurl text DEFAULT ''::text NOT NULL
);


ALTER TABLE feeds OWNER TO postgres;

--
-- Name: feeds_id_seq; Type: SEQUENCE; Schema: gf; Owner: postgres
--

CREATE SEQUENCE feeds_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE feeds_id_seq OWNER TO postgres;

--
-- Name: feeds_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE feeds_id_seq OWNED BY feeds.id;


--
-- Name: filters; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE filters (
    gid bigint NOT NULL,
    filter character varying(64) NOT NULL
);


ALTER TABLE filters OWNER TO postgres;

--
-- Name: guild_cfg; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE guild_cfg (
    gid bigint NOT NULL,
    welcome_cid bigint,
    leave_cid bigint,
    welcome_msg character varying(128),
    leave_msg character varying(128)
);


ALTER TABLE guild_cfg OWNER TO postgres;

--
-- Name: insults; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE insults (
    id integer NOT NULL,
    insult character varying(128)
);


ALTER TABLE insults OWNER TO postgres;

--
-- Name: insults_id_seq; Type: SEQUENCE; Schema: gf; Owner: postgres
--

CREATE SEQUENCE insults_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE insults_id_seq OWNER TO postgres;

--
-- Name: insults_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE insults_id_seq OWNED BY insults.id;


--
-- Name: memes; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE memes (
    gid bigint NOT NULL,
    name character varying(32) NOT NULL,
    url character varying(128) NOT NULL
);


ALTER TABLE memes OWNER TO postgres;

--
-- Name: msgcount; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE msgcount (
    uid bigint NOT NULL,
    count bigint DEFAULT 1 NOT NULL
);


ALTER TABLE msgcount OWNER TO postgres;

--
-- Name: prefixes; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE prefixes (
    gid bigint NOT NULL,
    prefix character varying(16)
);


ALTER TABLE prefixes OWNER TO postgres;

--
-- Name: saved_tasks; Type: TABLE; Schema: gf; Owner: postgres
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


ALTER TABLE saved_tasks OWNER TO postgres;

--
-- Name: saved_tasks_id_seq; Type: SEQUENCE; Schema: gf; Owner: postgres
--

CREATE SEQUENCE saved_tasks_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE saved_tasks_id_seq OWNER TO postgres;

--
-- Name: saved_tasks_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE saved_tasks_id_seq OWNED BY saved_tasks.id;


--
-- Name: stats; Type: TABLE; Schema: gf; Owner: postgres
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


ALTER TABLE stats OWNER TO postgres;

--
-- Name: statuses; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE statuses (
    status character varying(64),
    type smallint DEFAULT 0 NOT NULL,
    id integer NOT NULL
);


ALTER TABLE statuses OWNER TO postgres;

--
-- Name: statuses_id_seq; Type: SEQUENCE; Schema: gf; Owner: postgres
--

CREATE SEQUENCE statuses_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE statuses_id_seq OWNER TO postgres;

--
-- Name: statuses_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE statuses_id_seq OWNED BY statuses.id;


--
-- Name: subscriptions; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE subscriptions (
    id integer NOT NULL,
    cid bigint NOT NULL,
    qname character varying(64)
);


ALTER TABLE subscriptions OWNER TO postgres;

--
-- Name: subscriptions_id_seq; Type: SEQUENCE; Schema: gf; Owner: postgres
--

CREATE SEQUENCE subscriptions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE subscriptions_id_seq OWNER TO postgres;

--
-- Name: subscriptions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE subscriptions_id_seq OWNED BY subscriptions.id;


--
-- Name: swat_servers; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE swat_servers (
    ip character varying(32) NOT NULL,
    joinport integer NOT NULL,
    queryport integer NOT NULL,
    name character varying(32) NOT NULL
);


ALTER TABLE swat_servers OWNER TO postgres;

--
-- Name: text_reactions; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE text_reactions (
    gid bigint NOT NULL,
    trigger character varying(128) NOT NULL,
    response character varying(128) NOT NULL,
    id integer NOT NULL
);


ALTER TABLE text_reactions OWNER TO postgres;

--
-- Name: text_reactions_id_seq; Type: SEQUENCE; Schema: gf; Owner: postgres
--

CREATE SEQUENCE text_reactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE text_reactions_id_seq OWNER TO postgres;

--
-- Name: text_reactions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE text_reactions_id_seq OWNED BY text_reactions.id;


--
-- Name: emoji_reactions id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY emoji_reactions ALTER COLUMN id SET DEFAULT nextval('emoji_reactions_id_seq'::regclass);


--
-- Name: feeds id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY feeds ALTER COLUMN id SET DEFAULT nextval('feeds_id_seq'::regclass);


--
-- Name: insults id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY insults ALTER COLUMN id SET DEFAULT nextval('insults_id_seq'::regclass);


--
-- Name: saved_tasks id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY saved_tasks ALTER COLUMN id SET DEFAULT nextval('saved_tasks_id_seq'::regclass);


--
-- Name: statuses id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY statuses ALTER COLUMN id SET DEFAULT nextval('statuses_id_seq'::regclass);


--
-- Name: subscriptions id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY subscriptions ALTER COLUMN id SET DEFAULT nextval('subscriptions_id_seq'::regclass);


--
-- Name: text_reactions id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY text_reactions ALTER COLUMN id SET DEFAULT nextval('text_reactions_id_seq'::regclass);


--
-- Name: accounts accounts_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY accounts
    ADD CONSTRAINT accounts_pkey PRIMARY KEY (uid);


--
-- Name: assignable_roles assignable_roles_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY assignable_roles
    ADD CONSTRAINT assignable_roles_pkey PRIMARY KEY (gid, rid);


--
-- Name: automatic_roles automatic_roles_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY automatic_roles
    ADD CONSTRAINT automatic_roles_pkey PRIMARY KEY (gid, rid);


--
-- Name: birthdays birthdays_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY birthdays
    ADD CONSTRAINT birthdays_pkey PRIMARY KEY (uid, cid);


--
-- Name: blocked_channels blocked_channels_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY blocked_channels
    ADD CONSTRAINT blocked_channels_pkey PRIMARY KEY (cid);


--
-- Name: blocked_users blocked_users_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY blocked_users
    ADD CONSTRAINT blocked_users_pkey PRIMARY KEY (uid);


--
-- Name: emoji_reactions emoji_reactions_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY emoji_reactions
    ADD CONSTRAINT emoji_reactions_pkey PRIMARY KEY (id);


--
-- Name: feeds feeds_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY feeds
    ADD CONSTRAINT feeds_pkey PRIMARY KEY (id);


--
-- Name: feeds feeds_url_key; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY feeds
    ADD CONSTRAINT feeds_url_key UNIQUE (url);


--
-- Name: filters filters_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY filters
    ADD CONSTRAINT filters_pkey PRIMARY KEY (gid, filter);


--
-- Name: guild_cfg guild_cfg_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY guild_cfg
    ADD CONSTRAINT guild_cfg_pkey PRIMARY KEY (gid);


--
-- Name: insults insults_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY insults
    ADD CONSTRAINT insults_pkey PRIMARY KEY (id);


--
-- Name: memes memes_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY memes
    ADD CONSTRAINT memes_pkey PRIMARY KEY (gid, name);


--
-- Name: msgcount msgcount_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY msgcount
    ADD CONSTRAINT msgcount_pkey PRIMARY KEY (uid);


--
-- Name: prefixes prefixes_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY prefixes
    ADD CONSTRAINT prefixes_pkey PRIMARY KEY (gid);


--
-- Name: saved_tasks saved_tasks_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY saved_tasks
    ADD CONSTRAINT saved_tasks_pkey PRIMARY KEY (id);


--
-- Name: stats stats_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY stats
    ADD CONSTRAINT stats_pkey PRIMARY KEY (uid);


--
-- Name: statuses statuses_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY statuses
    ADD CONSTRAINT statuses_pkey PRIMARY KEY (id);


--
-- Name: subscriptions subscriptions_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY subscriptions
    ADD CONSTRAINT subscriptions_pkey PRIMARY KEY (id, cid);


--
-- Name: swat_servers swat_servers_name_key; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY swat_servers
    ADD CONSTRAINT swat_servers_name_key UNIQUE (name);


--
-- Name: swat_servers swat_servers_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY swat_servers
    ADD CONSTRAINT swat_servers_pkey PRIMARY KEY (ip);


--
-- Name: text_reactions text_reactions_gid_trigger_key; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY text_reactions
    ADD CONSTRAINT text_reactions_gid_trigger_key UNIQUE (gid, trigger);


--
-- Name: text_reactions text_reactions_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY text_reactions
    ADD CONSTRAINT text_reactions_pkey PRIMARY KEY (id);


--
-- Name: emoji_reactions_trigger_idx; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX emoji_reactions_trigger_idx ON emoji_reactions USING btree (trigger);


--
-- Name: gid_index; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX gid_index ON filters USING btree (gid);


--
-- Name: index_bday; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX index_bday ON birthdays USING btree (bday);

ALTER TABLE birthdays CLUSTER ON index_bday;


--
-- Name: index_er_gid; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX index_er_gid ON emoji_reactions USING btree (gid);

ALTER TABLE emoji_reactions CLUSTER ON index_er_gid;


--
-- Name: index_filters_gid; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX index_filters_gid ON filters USING btree (gid);

ALTER TABLE filters CLUSTER ON index_filters_gid;


--
-- Name: index_memes_cluster; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE UNIQUE INDEX index_memes_cluster ON memes USING btree (gid, name);

ALTER TABLE memes CLUSTER ON index_memes_cluster;


--
-- Name: index_tr_gid; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX index_tr_gid ON text_reactions USING btree (gid);

ALTER TABLE text_reactions CLUSTER ON index_tr_gid;


--
-- Name: trigger_index; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX trigger_index ON text_reactions USING btree (trigger);


--
-- Name: subscriptions subscriptions_id_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY subscriptions
    ADD CONSTRAINT subscriptions_id_fkey FOREIGN KEY (id) REFERENCES feeds(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

