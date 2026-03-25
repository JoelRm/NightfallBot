-- =====================================================
-- Crear tabla recordatorio_config
-- =====================================================

CREATE TABLE IF NOT EXISTS recordatorio_config (
    id_recordatorio  SERIAL PRIMARY KEY,
    nombre           VARCHAR(100)  NOT NULL,
    canal_id         NUMERIC       NOT NULL,
    rol_mencion      VARCHAR(100),
    mensaje          VARCHAR(1000) NOT NULL,
    dia_semana       INTEGER       NOT NULL,
    hora             INTEGER       NOT NULL,
    minuto           INTEGER       NOT NULL,
    zona_horaria     VARCHAR(50)   NOT NULL DEFAULT 'America/Lima',
    activo           BOOLEAN       NOT NULL DEFAULT TRUE,
    fecha_creacion   TIMESTAMP     NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_recordatorio_nombre
    ON recordatorio_config (nombre);

-- =====================================================
-- Seed: Recordatorio de Culvert semanal
-- Miércoles (3) a las 6:00 PM hora Perú
-- =====================================================
-- IMPORTANTE: Reemplaza el canal_id con el ID real de tu canal de Discord
-- IMPORTANTE: Reemplaza el rol_mencion con el ID real de tu rol @Miembros
--             Formato: <@&ROL_ID>  (ejemplo: <@&123456789012345678>)
-- =====================================================

INSERT INTO recordatorio_config (nombre, canal_id, rol_mencion, mensaje, dia_semana, hora, minuto, zona_horaria, activo)
VALUES (
    'culvert_semanal',
    0,                          -- ← REEMPLAZA con tu canal ID (ejemplo: 1234567890123456789)
    '<@&0>',                    -- ← REEMPLAZA con tu rol ID   (ejemplo: <@&942603999285235712>)
    '⚠️ ¡No olviden realizar el Culvert semanal! Tienen **00:50 minutos** para el cierre de la semana. ¡Gracias! 🎮',
    3,                          -- 3 = Miércoles (DayOfWeek.Wednesday)
    18,                         -- 6:00 PM
    0,                          -- Minuto 0
    'America/Lima',
    TRUE
)
ON CONFLICT (nombre) DO NOTHING;
