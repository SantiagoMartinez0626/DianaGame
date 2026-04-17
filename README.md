# Diana en movimiento

## Estudiante

- **Nombre completo:** _(completar)_
- **Parcial:** #2

## Descripción

Juego de tiro al blanco en 2D (Unity 6, URP): una diana se mueve horizontalmente de forma continua. Mantén **Espacio** para cargar potencia y suelta para lanzar la flecha con trayectoria parabólica (gravedad). Solo el **centro de la diana** (bullseye) suma **10 puntos** por acierto; los anillos exteriores muestran el mensaje «¡Fallaste!». La barra inferior indica la fuerza del disparo.

## Controles

| Acción        | Tecla   |
|---------------|---------|
| Cargar / disparar | Espacio (mantener y soltar) |

## Capturas de pantalla

Coloca aquí las imágenes desde la carpeta `Screenshots/` (mínimo 5 según la consigna).

## Video de funcionamiento

[Ver video del juego](https://) _(enlace a YouTube o Google Drive)_

## Cómo ejecutar el proyecto

1. Clonar el repositorio (sin la carpeta `Library/`).
2. Abrir el proyecto con **Unity 6** (6000.x), por ejemplo **6000.4.3f1** o compatible.
3. Abrir la escena `Assets/Scenes/SampleScene.unity`.
4. Pulsar **Play**.

**Nota:** El contenido de juego (diana, fondo, UI, arco) se genera en tiempo de ejecución mediante el objeto **GameBootstrap** en la escena. El prefab de la flecha está en `Assets/Prefabs/Arrow.prefab` y se carga también desde `Assets/Resources/Arrow.prefab` para el disparo.

## Repositorio

- Si el repositorio es **privado**, agregar como colaborador al docente: **dario1091@gmail.com**.
