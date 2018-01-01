--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.6
-- Dumped by pg_dump version 9.6.6

-- Started on 2018-01-01 23:17:40

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 4 (class 2615 OID 16607)
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
-- TOC entry 2232 (class 0 OID 0)
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
-- TOC entry 2233 (class 0 OID 0)
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
-- TOC entry 2234 (class 0 OID 0)
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
    filter character varying(64)
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
-- TOC entry 2235 (class 0 OID 0)
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
-- TOC entry 197 (class 1259 OID 16645)
-- Name: stats; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE stats (
    uid bigint NOT NULL,
    duels_won integer DEFAULT 0 NOT NULL,
    duels_lost integer DEFAULT 0 NOT NULL,
    hangman_won integer DEFAULT 0 NOT NULL,
    nunchis_won integer DEFAULT 0 NOT NULL,
    quizes_won integer DEFAULT 0 NOT NULL,
    races_won integer DEFAULT 0 NOT NULL,
    ttt_won integer DEFAULT 0 NOT NULL,
    ttt_lost integer DEFAULT 0 NOT NULL,
    chain4_won integer DEFAULT 0 NOT NULL,
    chain4_lost integer DEFAULT 0 NOT NULL,
    caro_won integer DEFAULT 0 NOT NULL,
    caro_lost integer DEFAULT 0 NOT NULL
);


ALTER TABLE stats OWNER TO postgres;

--
-- TOC entry 198 (class 1259 OID 16660)
-- Name: statuses; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE statuses (
    id integer NOT NULL,
    status character varying(64)
);


ALTER TABLE statuses OWNER TO postgres;

--
-- TOC entry 199 (class 1259 OID 16663)
-- Name: subscriptions; Type: TABLE; Schema: gf; Owner: postgres
--

CREATE TABLE subscriptions (
    id integer NOT NULL,
    cid bigint NOT NULL,
    qname character varying(64) DEFAULT ''::character varying NOT NULL
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
-- TOC entry 2236 (class 0 OID 0)
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
-- TOC entry 2061 (class 2604 OID 16675)
-- Name: feeds id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY feeds ALTER COLUMN id SET DEFAULT nextval('feeds_id_seq'::regclass);


--
-- TOC entry 2062 (class 2604 OID 16676)
-- Name: insults id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY insults ALTER COLUMN id SET DEFAULT nextval('insults_id_seq'::regclass);


--
-- TOC entry 2077 (class 2604 OID 16677)
-- Name: subscriptions id; Type: DEFAULT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY subscriptions ALTER COLUMN id SET DEFAULT nextval('subscriptions_id_seq'::regclass);


--
-- TOC entry 2079 (class 2606 OID 16679)
-- Name: accounts accounts_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY accounts
    ADD CONSTRAINT accounts_pkey PRIMARY KEY (uid);


--
-- TOC entry 2081 (class 2606 OID 16716)
-- Name: emoji_reactions emoji_reactions_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY emoji_reactions
    ADD CONSTRAINT emoji_reactions_pkey PRIMARY KEY (gid, trigger);


--
-- TOC entry 2084 (class 2606 OID 16681)
-- Name: feeds feeds_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY feeds
    ADD CONSTRAINT feeds_pkey PRIMARY KEY (id);


--
-- TOC entry 2087 (class 2606 OID 16683)
-- Name: guild_cfg guild_cfg_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY guild_cfg
    ADD CONSTRAINT guild_cfg_pkey PRIMARY KEY (gid);


--
-- TOC entry 2089 (class 2606 OID 16685)
-- Name: insults insults_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY insults
    ADD CONSTRAINT insults_pkey PRIMARY KEY (id);


--
-- TOC entry 2091 (class 2606 OID 16687)
-- Name: memes memes_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY memes
    ADD CONSTRAINT memes_pkey PRIMARY KEY (gid, name);


--
-- TOC entry 2093 (class 2606 OID 16689)
-- Name: msgcount msgcount_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY msgcount
    ADD CONSTRAINT msgcount_pkey PRIMARY KEY (uid);


--
-- TOC entry 2095 (class 2606 OID 16691)
-- Name: prefixes prefixes_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY prefixes
    ADD CONSTRAINT prefixes_pkey PRIMARY KEY (gid);


--
-- TOC entry 2097 (class 2606 OID 16693)
-- Name: stats stats_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY stats
    ADD CONSTRAINT stats_pkey PRIMARY KEY (uid);


--
-- TOC entry 2099 (class 2606 OID 16695)
-- Name: statuses statuses_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY statuses
    ADD CONSTRAINT statuses_pkey PRIMARY KEY (id);


--
-- TOC entry 2101 (class 2606 OID 16714)
-- Name: subscriptions subscriptions_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY subscriptions
    ADD CONSTRAINT subscriptions_pkey PRIMARY KEY (id, cid);


--
-- TOC entry 2103 (class 2606 OID 16712)
-- Name: swat_servers swat_servers_name_key; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY swat_servers
    ADD CONSTRAINT swat_servers_name_key UNIQUE (name);


--
-- TOC entry 2105 (class 2606 OID 16710)
-- Name: swat_servers swat_servers_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY swat_servers
    ADD CONSTRAINT swat_servers_pkey PRIMARY KEY (ip);


--
-- TOC entry 2107 (class 2606 OID 16708)
-- Name: text_reactions text_reactions_pkey; Type: CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY text_reactions
    ADD CONSTRAINT text_reactions_pkey PRIMARY KEY (gid, trigger);


--
-- TOC entry 2082 (class 1259 OID 16698)
-- Name: emoji_reactions_trigger_idx; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX emoji_reactions_trigger_idx ON emoji_reactions USING btree (trigger);


--
-- TOC entry 2085 (class 1259 OID 16699)
-- Name: gid_index; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX gid_index ON filters USING btree (gid);

ALTER TABLE filters CLUSTER ON gid_index;


--
-- TOC entry 2108 (class 1259 OID 16700)
-- Name: trigger_index; Type: INDEX; Schema: gf; Owner: postgres
--

CREATE INDEX trigger_index ON text_reactions USING btree (trigger);


--
-- TOC entry 2109 (class 2606 OID 16701)
-- Name: subscriptions subscriptions_id_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: postgres
--

ALTER TABLE ONLY subscriptions
    ADD CONSTRAINT subscriptions_id_fkey FOREIGN KEY (id) REFERENCES feeds(id) ON UPDATE CASCADE ON DELETE CASCADE;


-- Completed on 2018-01-01 23:17:40

--
-- PostgreSQL database dump complete
--

