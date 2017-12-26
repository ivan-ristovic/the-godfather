--
-- PostgreSQL database dump
--

-- Dumped from database version 10.1
-- Dumped by pg_dump version 10.1

-- Started on 2017-12-26 15:28:17

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

DROP DATABASE gfdb;
--
-- TOC entry 2250 (class 1262 OID 16384)
-- Name: gfdb; Type: DATABASE; Schema: -; Owner: -
--

CREATE DATABASE gfdb WITH TEMPLATE = template0 ENCODING = 'UTF8' LC_COLLATE = 'English_United States.1252' LC_CTYPE = 'English_United States.1252';


\connect gfdb

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 8 (class 2615 OID 16391)
-- Name: gf; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA gf;


--
-- TOC entry 2 (class 3079 OID 12278)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 2251 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- TOC entry 1 (class 3079 OID 16525)
-- Name: tsm_system_rows; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS tsm_system_rows WITH SCHEMA pg_catalog;


--
-- TOC entry 2252 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION tsm_system_rows; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION tsm_system_rows IS 'TABLESAMPLE method which accepts number of rows as a limit';


SET search_path = gf, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 198 (class 1259 OID 16505)
-- Name: accounts; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE accounts (
    uid bigint NOT NULL,
    balance integer DEFAULT 0 NOT NULL
);


--
-- TOC entry 208 (class 1259 OID 16589)
-- Name: emoji_reactions; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE emoji_reactions (
    gid bigint NOT NULL,
    trigger character varying(128),
    reaction character varying(64)
);


--
-- TOC entry 210 (class 1259 OID 16595)
-- Name: feeds; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE feeds (
    id integer NOT NULL,
    url text NOT NULL,
    savedurl text DEFAULT ''::text NOT NULL
);


--
-- TOC entry 209 (class 1259 OID 16593)
-- Name: feeds_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE feeds_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 2253 (class 0 OID 0)
-- Dependencies: 209
-- Name: feeds_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE feeds_id_seq OWNED BY feeds.id;


--
-- TOC entry 200 (class 1259 OID 16535)
-- Name: filters; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE filters (
    gid bigint NOT NULL,
    filter character varying(64)
);


--
-- TOC entry 206 (class 1259 OID 16580)
-- Name: guild_cfg; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE guild_cfg (
    gid bigint NOT NULL,
    welcome_cid bigint,
    leave_cid bigint
);


--
-- TOC entry 205 (class 1259 OID 16573)
-- Name: insults; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE insults (
    id integer NOT NULL,
    insult character varying(128)
);


--
-- TOC entry 204 (class 1259 OID 16571)
-- Name: insults_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE insults_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 2254 (class 0 OID 0)
-- Dependencies: 204
-- Name: insults_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE insults_id_seq OWNED BY insults.id;


--
-- TOC entry 202 (class 1259 OID 16554)
-- Name: memes; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE memes (
    gid bigint NOT NULL,
    name character varying(32) NOT NULL,
    url character varying(128) NOT NULL
);


--
-- TOC entry 213 (class 1259 OID 17361)
-- Name: msgcount; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE msgcount (
    uid bigint NOT NULL,
    count bigint DEFAULT 1 NOT NULL
);


--
-- TOC entry 201 (class 1259 OID 16538)
-- Name: prefixes; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE prefixes (
    gid bigint NOT NULL,
    prefix character varying(16)
);


--
-- TOC entry 197 (class 1259 OID 16422)
-- Name: stats; Type: TABLE; Schema: gf; Owner: -
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


--
-- TOC entry 199 (class 1259 OID 16517)
-- Name: statuses; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE statuses (
    id integer NOT NULL,
    status character varying(64)
);


--
-- TOC entry 212 (class 1259 OID 16606)
-- Name: subscriptions; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE subscriptions (
    id integer NOT NULL,
    cid bigint NOT NULL,
    qname character varying(64) DEFAULT ''::character varying NOT NULL
);


--
-- TOC entry 211 (class 1259 OID 16604)
-- Name: subscriptions_id_seq; Type: SEQUENCE; Schema: gf; Owner: -
--

CREATE SEQUENCE subscriptions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 2255 (class 0 OID 0)
-- Dependencies: 211
-- Name: subscriptions_id_seq; Type: SEQUENCE OWNED BY; Schema: gf; Owner: -
--

ALTER SEQUENCE subscriptions_id_seq OWNED BY subscriptions.id;


--
-- TOC entry 203 (class 1259 OID 16562)
-- Name: swat_servers; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE swat_servers (
    ip character varying(32) NOT NULL,
    joinport integer NOT NULL,
    queryport integer NOT NULL,
    name character varying(32) NOT NULL
);


--
-- TOC entry 207 (class 1259 OID 16585)
-- Name: text_reactions; Type: TABLE; Schema: gf; Owner: -
--

CREATE TABLE text_reactions (
    gid bigint NOT NULL,
    trigger character varying(128) NOT NULL,
    response character varying(128) NOT NULL
);


--
-- TOC entry 2097 (class 2604 OID 17827)
-- Name: feeds id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY feeds ALTER COLUMN id SET DEFAULT nextval('feeds_id_seq'::regclass);


--
-- TOC entry 2095 (class 2604 OID 17828)
-- Name: insults id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY insults ALTER COLUMN id SET DEFAULT nextval('insults_id_seq'::regclass);


--
-- TOC entry 2099 (class 2604 OID 17829)
-- Name: subscriptions id; Type: DEFAULT; Schema: gf; Owner: -
--

ALTER TABLE ONLY subscriptions ALTER COLUMN id SET DEFAULT nextval('subscriptions_id_seq'::regclass);


--
-- TOC entry 2104 (class 2606 OID 16510)
-- Name: accounts accounts_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY accounts
    ADD CONSTRAINT accounts_pkey PRIMARY KEY (uid);


--
-- TOC entry 2121 (class 2606 OID 16603)
-- Name: feeds feeds_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY feeds
    ADD CONSTRAINT feeds_pkey PRIMARY KEY (id);


--
-- TOC entry 2117 (class 2606 OID 16584)
-- Name: guild_cfg guild_cfg_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY guild_cfg
    ADD CONSTRAINT guild_cfg_pkey PRIMARY KEY (gid);


--
-- TOC entry 2115 (class 2606 OID 16578)
-- Name: insults insults_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY insults
    ADD CONSTRAINT insults_pkey PRIMARY KEY (id);


--
-- TOC entry 2111 (class 2606 OID 16558)
-- Name: memes memes_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY memes
    ADD CONSTRAINT memes_pkey PRIMARY KEY (gid, name);


--
-- TOC entry 2123 (class 2606 OID 17366)
-- Name: msgcount msgcount_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY msgcount
    ADD CONSTRAINT msgcount_pkey PRIMARY KEY (uid);


--
-- TOC entry 2109 (class 2606 OID 16542)
-- Name: prefixes prefixes_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY prefixes
    ADD CONSTRAINT prefixes_pkey PRIMARY KEY (gid);


--
-- TOC entry 2102 (class 2606 OID 16512)
-- Name: stats stats_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY stats
    ADD CONSTRAINT stats_pkey PRIMARY KEY (uid);


--
-- TOC entry 2106 (class 2606 OID 16522)
-- Name: statuses statuses_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY statuses
    ADD CONSTRAINT statuses_pkey PRIMARY KEY (id);


--
-- TOC entry 2113 (class 2606 OID 16570)
-- Name: swat_servers swat_servers_pkey; Type: CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY swat_servers
    ADD CONSTRAINT swat_servers_pkey PRIMARY KEY (name);


--
-- TOC entry 2119 (class 1259 OID 16592)
-- Name: emoji_reactions_trigger_idx; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX emoji_reactions_trigger_idx ON emoji_reactions USING btree (trigger);


--
-- TOC entry 2107 (class 1259 OID 16551)
-- Name: gid_index; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX gid_index ON filters USING btree (gid);

ALTER TABLE filters CLUSTER ON gid_index;


--
-- TOC entry 2118 (class 1259 OID 16588)
-- Name: trigger_index; Type: INDEX; Schema: gf; Owner: -
--

CREATE INDEX trigger_index ON text_reactions USING btree (trigger);


--
-- TOC entry 2124 (class 2606 OID 16610)
-- Name: subscriptions subscriptions_id_fkey; Type: FK CONSTRAINT; Schema: gf; Owner: -
--

ALTER TABLE ONLY subscriptions
    ADD CONSTRAINT subscriptions_id_fkey FOREIGN KEY (id) REFERENCES feeds(id) ON UPDATE CASCADE ON DELETE CASCADE;


-- Completed on 2017-12-26 15:28:18

--
-- PostgreSQL database dump complete
--

