--
-- PostgreSQL database dump
--

\restrict TB1jYlgTkXvQghPBdSwlY7shwzO4CEktZxld1Ajh0R7LXtBR0gQOjzGfYAXApsE

-- Dumped from database version 14.20 (Homebrew)
-- Dumped by pg_dump version 14.20 (Homebrew)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: unaccent; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS unaccent WITH SCHEMA public;


--
-- Name: EXTENSION unaccent; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION unaccent IS 'text search dictionary that removes accents';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: configuracion_puntaje_culvert; Type: TABLE; Schema: public; Owner: joelrm
--

CREATE TABLE public.configuracion_puntaje_culvert (
    id_configuracion integer NOT NULL,
    puntaje_minimo bigint NOT NULL,
    puntaje_maximo bigint,
    puntos_ganados integer NOT NULL,
    monedas_ganadas integer DEFAULT 0 NOT NULL,
    activo boolean DEFAULT true NOT NULL,
    fecha_creacion timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.configuracion_puntaje_culvert OWNER TO joelrm;

--
-- Name: configuracion_puntaje_culvert_id_configuracion_seq; Type: SEQUENCE; Schema: public; Owner: joelrm
--

ALTER TABLE public.configuracion_puntaje_culvert ALTER COLUMN id_configuracion ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.configuracion_puntaje_culvert_id_configuracion_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: guild_tienda_compra; Type: TABLE; Schema: public; Owner: joelrm
--

CREATE TABLE public.guild_tienda_compra (
    id_compra integer NOT NULL,
    id_item integer NOT NULL,
    id_personaje integer NOT NULL,
    cantidad integer DEFAULT 1 NOT NULL,
    puntos_gastados integer NOT NULL,
    monedas_gastadas integer DEFAULT 0 NOT NULL,
    usuario_compra character varying(100) NOT NULL,
    fecha_compra timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.guild_tienda_compra OWNER TO joelrm;

--
-- Name: guild_tienda_compra_id_compra_seq; Type: SEQUENCE; Schema: public; Owner: joelrm
--

ALTER TABLE public.guild_tienda_compra ALTER COLUMN id_compra ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.guild_tienda_compra_id_compra_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: guild_tienda_item; Type: TABLE; Schema: public; Owner: joelrm
--

CREATE TABLE public.guild_tienda_item (
    id_item integer NOT NULL,
    categoria character varying(50) NOT NULL,
    nombre_item character varying(150) NOT NULL,
    descripcion character varying(300),
    costo_puntos integer NOT NULL,
    costo_monedas integer DEFAULT 0 NOT NULL,
    stock integer,
    activo boolean DEFAULT true NOT NULL,
    fecha_creacion timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.guild_tienda_item OWNER TO joelrm;

--
-- Name: guild_tienda_item_id_item_seq; Type: SEQUENCE; Schema: public; Owner: joelrm
--

ALTER TABLE public.guild_tienda_item ALTER COLUMN id_item ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.guild_tienda_item_id_item_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: movimiento_monedas; Type: TABLE; Schema: public; Owner: joelrm
--

CREATE TABLE public.movimiento_monedas (
    id_movimiento_monedas integer NOT NULL,
    id_personaje integer NOT NULL,
    tipo_movimiento character varying(20) NOT NULL,
    cantidad integer NOT NULL,
    motivo character varying(200) NOT NULL,
    usuario_registro character varying(100) NOT NULL,
    fecha_registro timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT ck_movimiento_monedas_tipo CHECK (((tipo_movimiento)::text = ANY ((ARRAY['SUMA'::character varying, 'RESTA'::character varying])::text[])))
);


ALTER TABLE public.movimiento_monedas OWNER TO joelrm;

--
-- Name: movimiento_monedas_id_movimiento_monedas_seq; Type: SEQUENCE; Schema: public; Owner: joelrm
--

ALTER TABLE public.movimiento_monedas ALTER COLUMN id_movimiento_monedas ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.movimiento_monedas_id_movimiento_monedas_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: movimiento_puntos; Type: TABLE; Schema: public; Owner: joelrm
--

CREATE TABLE public.movimiento_puntos (
    id_movimiento_puntos integer NOT NULL,
    id_personaje integer NOT NULL,
    tipo_movimiento character varying(20) NOT NULL,
    cantidad integer NOT NULL,
    motivo character varying(200) NOT NULL,
    usuario_registro character varying(100) NOT NULL,
    fecha_registro timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT ck_movimiento_puntos_tipo CHECK (((tipo_movimiento)::text = ANY ((ARRAY['SUMA'::character varying, 'RESTA'::character varying])::text[])))
);


ALTER TABLE public.movimiento_puntos OWNER TO joelrm;

--
-- Name: movimiento_puntos_id_movimiento_puntos_seq; Type: SEQUENCE; Schema: public; Owner: joelrm
--

ALTER TABLE public.movimiento_puntos ALTER COLUMN id_movimiento_puntos ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.movimiento_puntos_id_movimiento_puntos_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: personaje; Type: TABLE; Schema: public; Owner: joelrm
--

CREATE TABLE public.personaje (
    id_personaje integer NOT NULL,
    nombre_personaje character varying(150) NOT NULL,
    clase character varying(150),
    level integer NOT NULL,
    imagen_url text,
    puntos_actuales integer DEFAULT 0 NOT NULL,
    monedas_actuales integer DEFAULT 0 NOT NULL,
    activo boolean DEFAULT true NOT NULL,
    fecha_creacion timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    fecha_actualizacion timestamp without time zone
);


ALTER TABLE public.personaje OWNER TO joelrm;

--
-- Name: personaje_id_personaje_seq; Type: SEQUENCE; Schema: public; Owner: joelrm
--

ALTER TABLE public.personaje ALTER COLUMN id_personaje ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.personaje_id_personaje_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: registro_semanal_culvert; Type: TABLE; Schema: public; Owner: joelrm
--

CREATE TABLE public.registro_semanal_culvert (
    id_registro_semanal integer NOT NULL,
    id_personaje integer NOT NULL,
    anio integer NOT NULL,
    semana integer NOT NULL,
    culvert_score bigint NOT NULL,
    puntos_ganados integer NOT NULL,
    monedas_ganadas integer NOT NULL,
    participa boolean DEFAULT true NOT NULL,
    usuario_registro character varying(100) NOT NULL,
    fecha_registro timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    observacion character varying(300)
);


ALTER TABLE public.registro_semanal_culvert OWNER TO joelrm;

--
-- Name: registro_semanal_culvert_id_registro_semanal_seq; Type: SEQUENCE; Schema: public; Owner: joelrm
--

ALTER TABLE public.registro_semanal_culvert ALTER COLUMN id_registro_semanal ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.registro_semanal_culvert_id_registro_semanal_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: configuracion_puntaje_culvert; Type: TABLE DATA; Schema: public; Owner: joelrm
--

COPY public.configuracion_puntaje_culvert (id_configuracion, puntaje_minimo, puntaje_maximo, puntos_ganados, monedas_ganadas, activo, fecha_creacion) FROM stdin;
1	5000	19999	3	0	t	2026-03-09 03:11:13.099855
2	20000	39999	5	0	t	2026-03-09 03:11:13.099855
3	40000	59999	8	0	t	2026-03-09 03:11:13.099855
4	60000	79999	12	0	t	2026-03-09 03:11:13.099855
5	80000	99999	16	0	t	2026-03-09 03:11:13.099855
6	100000	119999	20	0	t	2026-03-09 03:11:13.099855
7	120000	149999	25	0	t	2026-03-09 03:11:13.099855
8	150000	179999	30	0	t	2026-03-09 03:11:13.099855
9	180000	209999	36	0	t	2026-03-09 03:11:13.099855
10	210000	239999	42	0	t	2026-03-09 03:11:13.099855
11	240000	269999	50	0	t	2026-03-09 03:11:13.099855
12	270000	\N	60	0	t	2026-03-09 03:11:13.099855
\.


--
-- Data for Name: guild_tienda_compra; Type: TABLE DATA; Schema: public; Owner: joelrm
--

COPY public.guild_tienda_compra (id_compra, id_item, id_personaje, cantidad, puntos_gastados, monedas_gastadas, usuario_compra, fecha_compra) FROM stdin;
\.


--
-- Data for Name: guild_tienda_item; Type: TABLE DATA; Schema: public; Owner: joelrm
--

COPY public.guild_tienda_item (id_item, categoria, nombre_item, descripcion, costo_puntos, costo_monedas, stock, activo, fecha_creacion) FROM stdin;
1	Cubes	Hard Cubes Service Pack x10	\N	7	0	\N	t	2026-03-09 03:24:00.689639
2	Cubes	Solid Cubes Service Pack x10	\N	70	0	\N	t	2026-03-09 03:24:00.689639
3	Cubes	Glowing Cube (Red)	\N	40	0	\N	t	2026-03-09 03:24:00.689639
4	Cubes	Bright Cube (Black)	\N	70	0	\N	t	2026-03-09 03:24:00.689639
5	Cubes	Glowing Bonus Cube (Green)	\N	80	0	\N	t	2026-03-09 03:24:00.689639
6	Services	1 Hour de FZ	\N	5	0	\N	t	2026-03-09 03:24:00.689639
7	Services	1 Cast de FS	\N	5	0	\N	t	2026-03-09 03:24:00.689639
8	Services	Superior Gollux Scroll	\N	7	0	\N	t	2026-03-09 03:24:00.689639
9	Boss Carries	Black Mage Carry (Liberation)	\N	220	0	\N	t	2026-03-09 03:24:00.689639
10	Equipment	Papulatus Mark	\N	55	0	\N	t	2026-03-09 03:24:00.689639
11	Equipment	Dawn Set	\N	60	0	\N	t	2026-03-09 03:24:00.689639
12	Equipment	Arcane Umbra + CRA Set	\N	85	0	\N	t	2026-03-09 03:24:00.689639
13	Equipment	Superior Gollux Set	\N	150	0	\N	t	2026-03-09 03:24:00.689639
14	Equipment	Kalos / Kalin / TFA Piece	\N	450	0	\N	t	2026-03-09 03:24:00.689639
15	Equipment	Limbo / Baldrix Piece	\N	1200	0	\N	t	2026-03-09 03:24:00.689639
16	Special Rings	Ring of Restraint 4	\N	450	0	\N	t	2026-03-09 03:24:00.689639
17	Special Rings	Continuous Ring 4	\N	100	0	\N	t	2026-03-09 03:24:00.689639
18	Consumables	Wealth + EXP Potion Set (2h)	\N	2	0	\N	t	2026-03-09 03:24:00.689639
19	Consumables	Nodes pack x1000	\N	8	0	\N	t	2026-03-09 03:24:00.689639
20	Consumables	Sol Erda Fragments Pack x50	\N	12	0	\N	t	2026-03-09 03:24:00.689639
21	Consumables	Sol Erda Fragments Pack x100	\N	25	0	\N	t	2026-03-09 03:24:00.689639
22	Consumables	Black Flames Pack x10	\N	56	0	\N	t	2026-03-09 03:24:00.689639
23	Consumables	3 Premium Water of Life	\N	85	0	\N	t	2026-03-09 03:24:00.689639
24	Consumables	Grindstone	\N	280	0	\N	t	2026-03-09 03:24:00.689639
\.


--
-- Data for Name: movimiento_monedas; Type: TABLE DATA; Schema: public; Owner: joelrm
--

COPY public.movimiento_monedas (id_movimiento_monedas, id_personaje, tipo_movimiento, cantidad, motivo, usuario_registro, fecha_registro) FROM stdin;
\.


--
-- Data for Name: movimiento_puntos; Type: TABLE DATA; Schema: public; Owner: joelrm
--

COPY public.movimiento_puntos (id_movimiento_puntos, id_personaje, tipo_movimiento, cantidad, motivo, usuario_registro, fecha_registro) FROM stdin;
\.


--
-- Data for Name: personaje; Type: TABLE DATA; Schema: public; Owner: joelrm
--

COPY public.personaje (id_personaje, nombre_personaje, clase, level, imagen_url, puntos_actuales, monedas_actuales, activo, fecha_creacion, fecha_actualizacion) FROM stdin;
7	Fõxy	Night Lord	296	https://cdn.maplebot.io/images/106/1067886_c75bef11.png	60	0	t	2026-03-08 22:17:20.97152	2026-03-08 23:05:09.94048
9	Annz	Shadower	292	https://cdn.maplebot.io/images/107/1071014_ebb15e42.png	30	0	t	2026-03-08 22:17:32.866999	2026-03-08 23:05:09.975845
8	SalamRulez	Evan	290	https://cdn.maplebot.io/images/107/1074251_c8804507.png	20	0	t	2026-03-08 22:17:27.092593	2026-03-08 23:05:09.977914
10	Feedeta	Night Lord	295	https://cdn.maplebot.io/images/106/1067616_6309a9bd.png	0	0	t	2026-03-09 00:01:46.644931	2026-03-09 00:01:46.719031
\.


--
-- Data for Name: registro_semanal_culvert; Type: TABLE DATA; Schema: public; Owner: joelrm
--

COPY public.registro_semanal_culvert (id_registro_semanal, id_personaje, anio, semana, culvert_score, puntos_ganados, monedas_ganadas, participa, usuario_registro, fecha_registro, observacion) FROM stdin;
8	7	2026	10	270000	60	0	t	foxie2912	2026-03-08 23:05:09.51814	\N
9	9	2026	10	170000	30	0	t	foxie2912	2026-03-08 23:05:09.51814	\N
10	8	2026	10	100000	20	0	t	foxie2912	2026-03-08 23:05:09.51814	\N
11	7	2026	5	150000	30	0	t	test	2026-03-09 04:15:36.1335	\N
12	7	2026	6	170000	30	0	t	test	2026-03-09 04:15:36.1335	\N
13	7	2026	7	190000	36	0	t	test	2026-03-09 04:15:36.1335	\N
14	7	2026	8	210000	42	0	t	test	2026-03-09 04:15:36.1335	\N
15	7	2026	9	240000	50	0	t	test	2026-03-09 04:15:36.1335	\N
16	9	2026	5	120000	25	0	t	test	2026-03-09 04:15:36.1335	\N
17	9	2026	6	110000	20	0	t	test	2026-03-09 04:15:36.1335	\N
18	9	2026	7	0	0	0	f	test	2026-03-09 04:15:36.1335	\N
19	9	2026	8	150000	30	0	t	test	2026-03-09 04:15:36.1335	\N
20	9	2026	9	170000	30	0	t	test	2026-03-09 04:15:36.1335	\N
21	8	2026	5	5000	3	0	t	test	2026-03-09 04:15:36.1335	\N
22	8	2026	6	30000	5	0	t	test	2026-03-09 04:15:36.1335	\N
23	8	2026	7	80000	16	0	t	test	2026-03-09 04:15:36.1335	\N
24	8	2026	8	60000	12	0	t	test	2026-03-09 04:15:36.1335	\N
25	8	2026	9	100000	20	0	t	test	2026-03-09 04:15:36.1335	\N
\.


--
-- Name: configuracion_puntaje_culvert_id_configuracion_seq; Type: SEQUENCE SET; Schema: public; Owner: joelrm
--

SELECT pg_catalog.setval('public.configuracion_puntaje_culvert_id_configuracion_seq', 12, true);


--
-- Name: guild_tienda_compra_id_compra_seq; Type: SEQUENCE SET; Schema: public; Owner: joelrm
--

SELECT pg_catalog.setval('public.guild_tienda_compra_id_compra_seq', 1, false);


--
-- Name: guild_tienda_item_id_item_seq; Type: SEQUENCE SET; Schema: public; Owner: joelrm
--

SELECT pg_catalog.setval('public.guild_tienda_item_id_item_seq', 24, true);


--
-- Name: movimiento_monedas_id_movimiento_monedas_seq; Type: SEQUENCE SET; Schema: public; Owner: joelrm
--

SELECT pg_catalog.setval('public.movimiento_monedas_id_movimiento_monedas_seq', 1, false);


--
-- Name: movimiento_puntos_id_movimiento_puntos_seq; Type: SEQUENCE SET; Schema: public; Owner: joelrm
--

SELECT pg_catalog.setval('public.movimiento_puntos_id_movimiento_puntos_seq', 2, true);


--
-- Name: personaje_id_personaje_seq; Type: SEQUENCE SET; Schema: public; Owner: joelrm
--

SELECT pg_catalog.setval('public.personaje_id_personaje_seq', 10, true);


--
-- Name: registro_semanal_culvert_id_registro_semanal_seq; Type: SEQUENCE SET; Schema: public; Owner: joelrm
--

SELECT pg_catalog.setval('public.registro_semanal_culvert_id_registro_semanal_seq', 25, true);


--
-- Name: configuracion_puntaje_culvert configuracion_puntaje_culvert_pkey; Type: CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.configuracion_puntaje_culvert
    ADD CONSTRAINT configuracion_puntaje_culvert_pkey PRIMARY KEY (id_configuracion);


--
-- Name: guild_tienda_compra guild_tienda_compra_pkey; Type: CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.guild_tienda_compra
    ADD CONSTRAINT guild_tienda_compra_pkey PRIMARY KEY (id_compra);


--
-- Name: guild_tienda_item guild_tienda_item_pkey; Type: CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.guild_tienda_item
    ADD CONSTRAINT guild_tienda_item_pkey PRIMARY KEY (id_item);


--
-- Name: movimiento_monedas movimiento_monedas_pkey; Type: CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.movimiento_monedas
    ADD CONSTRAINT movimiento_monedas_pkey PRIMARY KEY (id_movimiento_monedas);


--
-- Name: movimiento_puntos movimiento_puntos_pkey; Type: CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.movimiento_puntos
    ADD CONSTRAINT movimiento_puntos_pkey PRIMARY KEY (id_movimiento_puntos);


--
-- Name: personaje personaje_pkey; Type: CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.personaje
    ADD CONSTRAINT personaje_pkey PRIMARY KEY (id_personaje);


--
-- Name: registro_semanal_culvert registro_semanal_culvert_pkey; Type: CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.registro_semanal_culvert
    ADD CONSTRAINT registro_semanal_culvert_pkey PRIMARY KEY (id_registro_semanal);


--
-- Name: ux_personaje_nombre_personaje; Type: INDEX; Schema: public; Owner: joelrm
--

CREATE UNIQUE INDEX ux_personaje_nombre_personaje ON public.personaje USING btree (nombre_personaje);


--
-- Name: ux_registro_personaje_anio_semana; Type: INDEX; Schema: public; Owner: joelrm
--

CREATE UNIQUE INDEX ux_registro_personaje_anio_semana ON public.registro_semanal_culvert USING btree (id_personaje, anio, semana);


--
-- Name: movimiento_monedas fk_movimiento_monedas_personaje; Type: FK CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.movimiento_monedas
    ADD CONSTRAINT fk_movimiento_monedas_personaje FOREIGN KEY (id_personaje) REFERENCES public.personaje(id_personaje);


--
-- Name: movimiento_puntos fk_movimiento_puntos_personaje; Type: FK CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.movimiento_puntos
    ADD CONSTRAINT fk_movimiento_puntos_personaje FOREIGN KEY (id_personaje) REFERENCES public.personaje(id_personaje);


--
-- Name: registro_semanal_culvert fk_registro_personaje; Type: FK CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.registro_semanal_culvert
    ADD CONSTRAINT fk_registro_personaje FOREIGN KEY (id_personaje) REFERENCES public.personaje(id_personaje);


--
-- Name: guild_tienda_compra guild_tienda_compra_id_item_fkey; Type: FK CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.guild_tienda_compra
    ADD CONSTRAINT guild_tienda_compra_id_item_fkey FOREIGN KEY (id_item) REFERENCES public.guild_tienda_item(id_item);


--
-- Name: guild_tienda_compra guild_tienda_compra_id_personaje_fkey; Type: FK CONSTRAINT; Schema: public; Owner: joelrm
--

ALTER TABLE ONLY public.guild_tienda_compra
    ADD CONSTRAINT guild_tienda_compra_id_personaje_fkey FOREIGN KEY (id_personaje) REFERENCES public.personaje(id_personaje);


--
-- PostgreSQL database dump complete
--

\unrestrict TB1jYlgTkXvQghPBdSwlY7shwzO4CEktZxld1Ajh0R7LXtBR0gQOjzGfYAXApsE

