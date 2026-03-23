# Base de Datos

Motor utilizado:

PostgreSQL

---

# Tabla personaje

Guarda los personajes del guild.

Campos principales:

- id_personaje
- nombre_personaje
- clase
- level
- puntos_actuales
- monedas_actuales
- imagen_url
- fecha_creacion
- fecha_actualizacion

---

# Tabla registro_semanal_culvert

Historial semanal de Culvert.

Campos:

- id_registro_semanal
- id_personaje
- anio
- semana
- culvert_score
- puntos_ganados
- monedas_ganadas
- participa
- usuario_registro
- fecha_registro

Restricción:
(id_personaje, anio, semana) UNIQUE

---

# Tabla configuracion_puntaje_culvert

Define cuántos puntos se ganan por Culvert.

Campos:

- puntaje_minimo
- puntaje_maximo
- puntos_ganados
- monedas_ganadas

---

# Tablas de tienda

## tienda_item

Items disponibles.

## tienda_compra

Historial de compras.