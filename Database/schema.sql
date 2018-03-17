--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.6
-- Dumped by pg_dump version 9.6.6

-- Started on 2018-03-17 19:53:11

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 8 (class 2615 OID 16607)
-- Name: gf; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA gf;


ALTER SCHEMA gf OWNER TO postgres;

--
-- TOC entry 2 (class 3079 OID 12387)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 2264 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- TOC entry 1 (class 3079 OID 16516)
-- Name: tsm_system_rows; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS tsm_system_rows WITH SCHEMA pg_catalog;


--
-- TOC entry 2265 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION tsm_system_rows; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION tsm_system_rows IS 'TABLESAMPLE method which accepts number of rows as a limit';


SET search_path = gf, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 186 (class 1259 OID 16608)
-- Name: accounts; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE accounts (
    uid bigint NOT NULL,
    balance integer DEFAULT 0 NOT NULL
);


ALTER TABLE accounts OWNER TO postgres;

--
-- TOC entry 204 (class 1259 OID 18519)
-- Name: assignable_roles; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE assignable_roles (
    gid bigint NOT NULL,
    rid bigint NOT NULL
);


ALTER TABLE assignable_roles OWNER TO postgres;

--
-- TOC entry 205 (class 1259 OID 28686)
-- Name: blocked_channels; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE blocked_channels (
    cid bigint NOT NULL,
    reason character varying(64)
);


ALTER TABLE blocked_channels OWNER TO postgres;

--
-- TOC entry 206 (class 1259 OID 28689)
-- Name: blocked_users; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE blocked_users (
    uid bigint NOT NULL,
    reason character varying(64)
);


ALTER TABLE blocked_users OWNER TO postgres;

--
-- TOC entry 187 (class 1259 OID 16612)
-- Name: emoji_reactions; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE emoji_reactions (
    gid bigint NOT NULL,
    trigger character varying(128) NOT NULL,
    reaction character varying(64)
);


ALTER TABLE emoji_reactions OWNER TO postgres;

--
-- TOC entry 188 (class 1259 OID 16615)
-- Name: feeds; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE feeds (
    id integer NOT NULL,
    url text NOT NULL,
    savedurl text DEFAULT ''::text NOT NULL
);


ALTER TABLE feeds OWNER TO postgres;

--
-- TOC entry 189 (class 1259 OID 16622)
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
-- TOC entry 2266 (class 0 OID 0)
-- Dependencies: 189
-- Name: feeds_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE feeds_id_seq OWNED BY feeds.id;


--
-- TOC entry 190 (class 1259 OID 16624)
-- Name: filters; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE filters (
    gid bigint NOT NULL,
    filter character varying(64) NOT NULL
);


ALTER TABLE filters OWNER TO postgres;

--
-- TOC entry 191 (class 1259 OID 16627)
-- Name: guild_cfg; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE guild_cfg (
    gid bigint NOT NULL,
    welcome_cid bigint,
    leave_cid bigint
);


ALTER TABLE guild_cfg OWNER TO postgres;

--
-- TOC entry 192 (class 1259 OID 16630)
-- Name: insults; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE insults (
    id integer NOT NULL,
    insult character varying(128)
);


ALTER TABLE insults OWNER TO postgres;

--
-- TOC entry 193 (class 1259 OID 16633)
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
-- TOC entry 2267 (class 0 OID 0)
-- Dependencies: 193
-- Name: insults_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE insults_id_seq OWNED BY insults.id;


--
-- TOC entry 194 (class 1259 OID 16635)
-- Name: memes; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE memes (
    gid bigint NOT NULL,
    name character varying(32) NOT NULL,
    url character varying(128) NOT NULL
);


ALTER TABLE memes OWNER TO postgres;

--
-- TOC entry 195 (class 1259 OID 16638)
-- Name: msgcount; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE msgcount (
    uid bigint NOT NULL,
    count bigint DEFAULT 1 NOT NULL
);


ALTER TABLE msgcount OWNER TO postgres;

--
-- TOC entry 196 (class 1259 OID 16642)
-- Name: prefixes; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE prefixes (
    gid bigint NOT NULL,
    prefix character varying(16)
);


ALTER TABLE prefixes OWNER TO postgres;

--
-- TOC entry 207 (class 1259 OID 28692)
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
-- TOC entry 208 (class 1259 OID 28695)
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
-- TOC entry 2268 (class 0 OID 0)
-- Dependencies: 208
-- Name: saved_tasks_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE saved_tasks_id_seq OWNED BY saved_tasks.id;


--
-- TOC entry 197 (class 1259 OID 16645)
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
-- TOC entry 198 (class 1259 OID 16660)
-- Name: statuses; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE statuses (
    status character varying(64),
    type smallint DEFAULT 0 NOT NULL,
    id integer NOT NULL
);


ALTER TABLE statuses OWNER TO postgres;

--
-- TOC entry 203 (class 1259 OID 16734)
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
-- TOC entry 2269 (class 0 OID 0)
-- Dependencies: 203
-- Name: statuses_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE statuses_id_seq OWNED BY statuses.id;


--
-- TOC entry 199 (class 1259 OID 16663)
-- Name: subscriptions; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE subscriptions (
    id integer NOT NULL,
    cid bigint NOT NULL,
    qname character varying(64)
);


ALTER TABLE subscriptions OWNER TO postgres;

--
-- TOC entry 200 (class 1259 OID 16667)
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
-- TOC entry 2270 (class 0 OID 0)
-- Dependencies: 200
-- Name: subscriptions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: postgres
--

ALTER SEQUENCE subscriptions_id_seq OWNED BY subscriptions.id;


--
-- TOC entry 201 (class 1259 OID 16669)
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
-- TOC entry 202 (class 1259 OID 16672)
-- Name: text_reactions; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE text_reactions (
    gid bigint NOT NULL,
    trigger character varying(128) NOT NULL,
    response character varying(128) NOT NULL
);


ALTER TABLE text_reactions OWNER TO postgres;

--
-- TOC entry 2081 (class 2604 OID 28697)
-- Name: feeds id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY feeds ALTER COLUMN id SET DEFAULT nextval('feeds_id_seq'::regclass);


--
-- TOC entry 2082 (class 2604 OID 28698)
-- Name: insults id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY insults ALTER COLUMN id SET DEFAULT nextval('insults_id_seq'::regclass);


--
-- TOC entry 2101 (class 2604 OID 28699)
-- Name: saved_tasks id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY saved_tasks ALTER COLUMN id SET DEFAULT nextval('saved_tasks_id_seq'::regclass);


--
-- TOC entry 2099 (class 2604 OID 28700)
-- Name: statuses id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY statuses ALTER COLUMN id SET DEFAULT nextval('statuses_id_seq'::regclass);


--
-- TOC entry 2100 (class 2604 OID 28701)
-- Name: subscriptions id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY subscriptions ALTER COLUMN id SET DEFAULT nextval('subscriptions_id_seq'::regclass);


--
-- TOC entry 2103 (class 2606 OID 16679)
-- Name: accounts accounts_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY accounts
    ADD CONSTRAINT accounts_pkey PRIMARY KEY (uid);


--
-- TOC entry 2136 (class 2606 OID 28703)
-- Name: blocked_channels blocked_channels_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY blocked_channels
    ADD CONSTRAINT blocked_channels_pkey PRIMARY KEY (cid);


--
-- TOC entry 2138 (class 2606 OID 28705)
-- Name: blocked_users blocked_users_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY blocked_users
    ADD CONSTRAINT blocked_users_pkey PRIMARY KEY (uid);


--
-- TOC entry 2105 (class 2606 OID 19221)
-- Name: emoji_reactions emoji_reactions_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY emoji_reactions
    ADD CONSTRAINT emoji_reactions_pkey PRIMARY KEY (gid, trigger);


--
-- TOC entry 2108 (class 2606 OID 16681)
-- Name: feeds feeds_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY feeds
    ADD CONSTRAINT feeds_pkey PRIMARY KEY (id);


--
-- TOC entry 2110 (class 2606 OID 19929)
-- Name: filters filters_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY filters
    ADD CONSTRAINT filters_pkey PRIMARY KEY (gid, filter);


--
-- TOC entry 2113 (class 2606 OID 16683)
-- Name: guild_cfg guild_cfg_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY guild_cfg
    ADD CONSTRAINT guild_cfg_pkey PRIMARY KEY (gid);


--
-- TOC entry 2115 (class 2606 OID 16685)
-- Name: insults insults_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY insults
    ADD CONSTRAINT insults_pkey PRIMARY KEY (id);


--
-- TOC entry 2117 (class 2606 OID 16687)
-- Name: memes memes_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY memes
    ADD CONSTRAINT memes_pkey PRIMARY KEY (gid, name);


--
-- TOC entry 2119 (class 2606 OID 16689)
-- Name: msgcount msgcount_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY msgcount
    ADD CONSTRAINT msgcount_pkey PRIMARY KEY (uid);


--
-- TOC entry 2121 (class 2606 OID 16691)
-- Name: prefixes prefixes_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY prefixes
    ADD CONSTRAINT prefixes_pkey PRIMARY KEY (gid);


--
-- TOC entry 2140 (class 2606 OID 28707)
-- Name: saved_tasks saved_tasks_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY saved_tasks
    ADD CONSTRAINT saved_tasks_pkey PRIMARY KEY (id);


--
-- TOC entry 2123 (class 2606 OID 16693)
-- Name: stats stats_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY stats
    ADD CONSTRAINT stats_pkey PRIMARY KEY (uid);


--
-- TOC entry 2125 (class 2606 OID 16741)
-- Name: statuses statuses_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY statuses
    ADD CONSTRAINT statuses_pkey PRIMARY KEY (id);


--
-- TOC entry 2127 (class 2606 OID 16714)
-- Name: subscriptions subscriptions_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY subscriptions
    ADD CONSTRAINT subscriptions_pkey PRIMARY KEY (id, cid);


--
-- TOC entry 2129 (class 2606 OID 16712)
-- Name: swat_servers swat_servers_name_key; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY swat_servers
    ADD CONSTRAINT swat_servers_name_key UNIQUE (name);


--
-- TOC entry 2131 (class 2606 OID 16710)
-- Name: swat_servers swat_servers_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY swat_servers
    ADD CONSTRAINT swat_servers_pkey PRIMARY KEY (ip);


--
-- TOC entry 2133 (class 2606 OID 16708)
-- Name: text_reactions text_reactions_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY text_reactions
    ADD CONSTRAINT text_reactions_pkey PRIMARY KEY (gid, trigger);


--
-- TOC entry 2106 (class 1259 OID 28708)
-- Name: emoji_reactions_trigger_idx; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX emoji_reactions_trigger_idx ON emoji_reactions USING btree (trigger);


--
-- TOC entry 2111 (class 1259 OID 16699)
-- Name: gid_index; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX gid_index ON filters USING btree (gid);

ALTER TABLE filters CLUSTER ON gid_index;


--
-- TOC entry 2134 (class 1259 OID 16700)
-- Name: trigger_index; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX trigger_index ON text_reactions USING btree (trigger);


--
-- TOC entry 2141 (class 2606 OID 16701)
-- Name: subscriptions subscriptions_id_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY subscriptions
    ADD CONSTRAINT subscriptions_id_fkey FOREIGN KEY (id) REFERENCES feeds(id) ON UPDATE CASCADE ON DELETE CASCADE;


-- Completed on 2018-03-17 19:53:11

--
-- PostgreSQL database dump complete
--

