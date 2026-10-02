# What Lies Behind — Plan de la Primera Entrega (TP1, 7 de octubre)

Qué decidimos para esta entrega, cómo cubrimos las consignas del TP1, qué cambia en el código que ya tenemos, qué hay que agregar y qué escenas quedan por armar.

> En la corrección nos van a preguntar a cada una sobre el código, así que todo se mantiene **simple y entendible**: scripts chicos, de una sola responsabilidad.

---

## 1. Decisiones tomadas

| Tema | Decisión |
|---|---|
| Salir de un recuerdo | Se sale **automáticamente** cuando encontraste los 3 objetos (consistencia, inconsistencia y trigger). Si el último fue el trigger, primero termina su evento (audio o aparición) y después se sale. |
| Volver a entrar | **No se puede.** Salir de un recuerdo te lleva a la pantalla de **Fin del día** y de ahí volvés al despacho. |
| Trigger | Solo se puede activar **después de encontrar la inconsistencia**: la revelación llega después de detectar la mentira. |
| Llegar al final | En esta entrega **siempre descubrís todo** y al final decidís qué hacer. |
| Decisión final | Aparece en el despacho como un **4º documento (tu propio caso)** cuando visitaste los 3 recuerdos. Más adelante va a ser una escena aparte. |
| Orden de los recuerdos | Libre. |
| Nota en la mano | Se reemplaza por una **lista que se abre con Tab** y se va tildando. |
| Input | **Mirar + E**, todo con `PlayerInteractor` y el **Input System nuevo**. |
| Conversación de fondo | Se dispara al **entrar a una zona del despacho** (un trigger collider cerca de donde hablan). Así cubrimos también la consigna de triggers. |
| Linterna | Afuera por ahora. |
| Nombres | Provisorios. En el código se usan como ids (`Mateo`, `Camila`, `Romina`) y se cambian fácil. |
| Timer | No entra en esta entrega. Cuando se sume, se acaba el tiempo = salir del recuerdo (nunca Game Over). |

---

## 2. Consignas del TP1: cómo las cubrimos

| Consigna | Cómo la cubrimos | Estado |
|---|---|---|
| Pitch de venta | Presentación del juego (no es código) | Por hacer |
| Assets en carpetas + Prefabs de cada objeto | Ya está `_Project/` ordenado. Falta hacer prefabs de los objetos nuevos (documentos, pistas, Player) | Parcial |
| 3 componentes con movimiento, rotación, instanciación y/o destrucción | **Movimiento:** `FirstPersonPlayer`. **Rotación:** `Door`. **Instanciación:** el trigger instancia lo que aparece (tu figura, la caja del inyectable) y la lista de pistas instancia sus renglones. **Destrucción:** lo que desaparece al activar un trigger | Parcial |
| Música de fondo + 2 SFX | Ya hay música (menú, despacho) y SFX (puertas). Suman grito, disparo, llamada, radio | Parcial |
| **Cinemachine** para la cámara | La cámara del jugador pasa a ser una `CinemachineCamera`. El zoom de la cinemática es una **segunda CinemachineCamera** con más prioridad: Cinemachine hace el blend solo | **Falta instalar** |
| **Sistema de inputs** | Hoy se usa el `Input` viejo (`Input.GetKeyDown`, `Input.GetAxis`). Pasar a `InputSystem_Actions` (ya existe en el proyecto, con Move, Look e Interact). Sumar la acción de Tab | **Falta** |
| Colisiones / triggers con reacción | Zona trigger en el despacho que dispara la conversación de fondo (`OnTriggerEnter`) | Falta |
| **Carga asincrónica de escenas** | `SceneAdministrator` pasa de `LoadScene` a `LoadSceneAsync` | **Falta** |
| Patrón **Strategy** | `IInteractable`: cada objeto decide qué hace al interactuar (documento, pista, puerta) y `PlayerInteractor` no sabe cuál es | Hecho |
| Patrón **Manager** | `MemoryManager` (un recuerdo) y `SceneAdministrator` (escenas) | Parcial |
| UI con scrolling (layout group) | La lista de pistas (Tab) es un **Scroll View + Vertical Layout Group** | Falta |
| 3 eventos (actions / delegates) con sentido | Nuevo `GameEvents`: `ClueFound`, `MemoryCompleted` y `PlayerLockChanged` (ver sección 5) | Falta |
| Escenas: menú, juego y cierre | Menú ✔, despacho + recuerdos (juego), Final 1 / Final 2 (cierre) | Parcial |
| Patrones **Command + Event queue** | Las secuencias (pesadilla, conversación del despacho, evento del trigger) son **una cola de comandos** que se ejecutan de a uno (ver sección 5) | Falta |

### ⚠️ Versión de Unity
La consigna pide **Unity 2022.3.35 LTS** y el proyecto está en **Unity 6 (6000.5.7f1)**. Hay que **preguntarles a los profes** si lo aceptan. Bajar de versión puede romper escenas y materiales (URP), así que conviene preguntar antes de seguir.

---

## 3. Flujo del juego

```
Menú → Pesadilla → Despacho ─┬─ Doc. Mateo  → Recuerdo Living  ─┐
                             ├─ Doc. Camila → Recuerdo Auto    ─┼→ Fin del día → Despacho
                             └─ Doc. Romina → Recuerdo Bosque  ─┘

Con los 3 recuerdos visitados aparece el 4º documento → Decisión → Final 1 (confesar) / Final 2 (no hacer nada)
```

---

## 4. Cambios en lo que ya existe

**Criterio: lo que se reutiliza se toca lo mínimo.** No se reestructura nada: se cambian solo las líneas necesarias para cumplir la consigna, y la lógica de cada script queda igual. Lo nuevo va en scripts nuevos.

| Archivo | Cambio (mínimo) | Qué NO cambia |
|---|---|---|
| `FirstPersonPlayer` | Las 4 líneas de `Input.GetAxis` pasan a leer Move y Look del **Input System**. `_playerCamera` pasa a buscar la `CinemachineCamera` hija en vez de la `Camera` (1 línea). `CanMove` pasa a tener `set`, y se suscribe a `PlayerLockChanged` para cambiarlo (unas pocas líneas en `OnEnable` / `OnDisable`). | Toda la lógica de mirar, moverse y gravedad (`HandleLook`, `HandleMove`, `ApplyFinalMovement`). |
| `PlayerInteractor` | `Input.GetKeyDown(key)` pasa a leer Interact del **Input System** (1 línea). Sacar el `Debug.Log`. Commitear lo de `IFocusable`. | Raycast, foco e interacción. |
| `SceneChanger` | `LoadScene` → `LoadSceneAsync` (1 línea). Sacar el `Debug.Log`. | Todo lo demás. |
| `SceneAdministrator` | En cada método, `LoadScene` → `LoadSceneAsync`. Se suman constantes y métodos para Pesadilla, FinDelDia, FinalConfesar y FinalNoHacerNada, iguales a los que ya hay. | Los métodos públicos se llaman igual, así que los botones del menú siguen funcionando sin tocarlos. |
| `GameEvents` | Mismo patrón (clase estática con `event Action` + `Raise...`), pero se sacan los eventos de llave, linterna, regalo, tiempo y victoria (solo los usaba la práctica vieja) y se agregan los 3 nuevos (sección 5). | La forma de declarar y lanzar eventos. |
| `IInteractable`, `IFocusable` | Sin cambios. | — |
| `FlashbackFlashes` | Sin cambios. Se puede usar en el despacho para los flashbacks. | — |
| `Door` | Sin cambios. Cuenta como el componente de rotación. | — |
| Build Settings | Agregar las escenas nuevas y sacar `SampleScene`. |
| Práctica vieja | **Borrado** (queda en el historial de git): `KeyObject`, `FlashlightObject`, `PrizeObject`, `KeyHandView`, `FlashlightHandView`, `FlashlightToggle`, `RoomManager`, `RoomController`, `GameStateManager`, `SampleScene` y `SmallTableWKey.prefab`. Se quedan `TimerController` y `TimerUI`, que ya no dependen de `GameEvents` y se usan en `room.unity`. |
| Paquetes | Instalar **Cinemachine**. |

---

## 5. Scripts nuevos

Todos son chicos y de una sola responsabilidad.

### `GameProgress`
Clase estática, sin MonoBehaviour. Recuerda qué recuerdos visitaste y sobrevive a los cambios de escena.
- `MarkVisited(id)`, `IsVisited(id)`, `AllVisited` y `Reset()`.
- Un flag para que la conversación del despacho pase una sola vez.

### `GameEvents` (reescrito): los 3 eventos
| Evento | Quién lo lanza | Quién lo escucha |
|---|---|---|
| `ClueFound(MemoryClue)` | `MemoryClue` al apretar E | `MemoryManager` (lleva la cuenta) y `ClueListUI` (tilda el renglón) |
| `MemoryCompleted(string id)` | `MemoryManager` al encontrar los 3 | `GameProgress` (marca visitado) y la salida a Fin del día |
| `PlayerLockChanged(bool)` | Cinemática y panel de decisión | `FirstPersonPlayer` (se frena) |

### Command + Event queue
- **`ICommand`**: una interfaz con un solo método, `IEnumerator Execute()`. Cada acción de una secuencia es un comando.
- **Comandos** (uno por archivo, cada uno de pocas líneas):
  - `PlayAudioCommand`: reproduce un audio y espera a que termine.
  - `ShowTextCommand`: muestra un subtítulo o texto un tiempo.
  - `WaitCommand`: espera X segundos.
  - `SpawnCommand`: instancia un prefab en un punto (la revelación del trigger).
  - `LoadSceneCommand`: carga una escena.
- **`CommandQueue`**: un MonoBehaviour con una `Queue<ICommand>`. `Enqueue(cmd)` agrega y ejecuta los comandos de a uno, en orden. `IsBusy` dice si todavía está ejecutando.
- **Dónde se usa:**
  - La **pesadilla** encola: texto de fecha, grito, disparo, texto "siete años después" y cargar el despacho.
  - La **conversación del despacho** encola: bloquear, audios con subtítulos y desbloquear.
  - El **trigger** de un recuerdo encola: spawn de lo que aparece y su audio. El `MemoryManager` espera a que `IsBusy` sea falso para salir: eso resuelve "si el trigger es el último, primero termina su evento".

### `MemoryClue`
Va en cada objeto interactuable de un recuerdo (implementa `IInteractable` + `IFocusable`: Strategy).
- **Inspector:** tipo (Consistencia / Inconsistencia / Trigger), texto para la lista y objeto de brillo que se prende al mirarlo.
- **Si es Trigger:** también lleva el prefab que aparece, dónde aparece y el audio. Al activarlo los encola en la `CommandQueue`.
- Al apretar E lanza `GameEvents.ClueFound`.

### `MemoryManager`
Uno por escena de recuerdo (Manager).
- **Inspector:** id del recuerdo y sus 3 `MemoryClue`.
- No deja activar el trigger hasta que se encontró la inconsistencia.
- Con los 3 encontrados espera a la cola y llama a `Exit()`.
- `Exit()` es la **única salida del recuerdo**: lanza `MemoryCompleted` y carga Fin del día. El timer, cuando exista, también va a llamar a `Exit()`.

### `ClueListUI`
Con Tab muestra y oculta la lista de pistas.
- Es un **Scroll View con Vertical Layout Group**. Cada renglón es un prefab que se instancia.
- Escucha `ClueFound` para tildar. El trigger no aparece hasta que lo encontrás.

### `MemoryDocument`
Los 3 documentos del despacho.
- Si el recuerdo ya fue visitado, se desactiva.
- Si no, carga la escena del recuerdo.

### `FinalCaseDocument`
El 4º documento.
- Solo aparece si `GameProgress.AllVisited`.
- Al interactuar abre un panel con dos botones, **Confesar** y **No hacer nada**. Mientras está abierto bloquea al jugador y muestra el cursor.
- Cada botón carga FinalConfesar o FinalNoHacerNada.

### `CinematicZone`
Trigger collider en el despacho. En `OnTriggerEnter` con el jugador (y solo la primera vez):
- activa la Cinemachine de zoom;
- encola la conversación en la `CommandQueue`;
- al terminar, vuelve a la cámara normal.

---

## 6. Escenas a armar

| Escena | Qué lleva | Código propio |
|---|---|---|
| Pesadilla | Canvas negro, textos, audios y `CommandQueue` con la secuencia | Ya cubierto |
| Despacho | 3 `MemoryDocument`, 1 `FinalCaseDocument`, `CinematicZone` + 2ª Cinemachine y decoración | Ya cubierto |
| Recuerdo 1: Mateo (living) | Escenario, Player prefab, `MemoryManager`, `CommandQueue`, 3 objetos con `MemoryClue` y `ClueListUI` | No |
| Recuerdo 2: Camila (auto) | Ídem | No |
| Recuerdo 3: Romina (bosque) | Ídem | No |
| Fin del día | Texto y botón (o espera) que vuelve al despacho | No |
| Final 1: Confesar | Textos o imágenes y botón al menú | No |
| Final 2: No hacer nada | Textos o imágenes y botón al menú | No |

### Contenido de cada recuerdo

| Recuerdo | Consistencia | Inconsistencia | Trigger (brilla) | Qué aparece al activarlo |
|---|---|---|---|---|
| Mateo · Living | Botellas de alcohol | Caja de cigarrillos de la marca de Luca | Ventana sana (vos la recordás rota) | Tu figura sentada (+ conversación opcional) |
| Camila · Auto | Atrás del auto con cajas y alcohol | Radio y reloj del tablero (el programa no iba a esa hora) | Ruedas con barro | Caja del inyectable en el asiento + llamada de Camila |
| Romina · Bosque | Techo del cobertizo roto | Leña vacía y hacha guardada (y la hora) | Huellas de tacos (dijo que tenía botas) | Tu voz gritando "¡no lo hagas!" + disparo |

---

## 7. Cómo armar cada cosa en el editor

**Nombres de escena** (tienen que ser exactos y estar en Build Settings): `Pesadilla`, `FinDelDia`, `FinalConfesar`, `FinalNoHacerNada`. Los recuerdos, por ejemplo `RecuerdoMateo`, `RecuerdoCamila` y `RecuerdoRomina` (se ponen en el `MemoryDocument`).

**Una sola vez:**
1. Package Manager → instalar **Cinemachine**.
2. En el `Player.prefab`: agregar una `CinemachineCamera` como hija del `Eye`, en la misma posición que la Main Camera, y arrastrarla al campo **Look Target** de `FirstPersonPlayer`. A la Main Camera agregarle `CinemachineBrain`.
3. Menú: cambiar el botón "Jugar" de `LoadDespacho` a **`StartNewGame`** (cuando exista la escena Pesadilla).

**Escena de recuerdo:**
1. Player prefab, un objeto vacío con `CommandQueue` y otro con `MemoryManager` (id, las 3 pistas, la queue).
2. Cada objeto de pista: collider + `MemoryClue` (tipo, texto, highlight opcional). El trigger además lleva el glow, la queue, el prefab y el punto donde aparece, un AudioSource y el audio.
3. Canvas con un Scroll View (en el Content: `Vertical Layout Group` + `Content Size Fitter`) y un prefab de renglón (un TextMeshPro). Un objeto con `ClueListUI` que los referencie.

**Despacho:**
1. Los 3 documentos: collider + `MemoryDocument` (id igual al del `MemoryManager` + nombre de la escena).
2. **4º documento (tu caso).** No aparece hasta que visitaste los 3 recuerdos: cuando volvés del tercero ya está ahí. Al tocarlo con E se abre un panel para decidir. Para armarlo:
   - Un objeto (el expediente) con collider + `FinalCaseDocument`.
   - En el Canvas, un panel **apagado** con 2 botones: "Confesar", que llama a `SceneAdministrator.LoadFinalConfesar`, y "No hacer nada", que llama a `SceneAdministrator.LoadFinalNoHacerNada`. Para eso, en la escena tiene que haber un objeto con `SceneAdministrator`.
   - Arrastrar el panel al campo **Decision Panel** del `FinalCaseDocument`.
   - Al abrirse el panel, el jugador se frena y aparece el mouse para poder hacer click (eso lo hace el script).
3. Conversación: un Box Collider con **Is Trigger** + `CinematicZone`, una segunda `CinemachineCamera` (con menos FOV, mirando hacia la charla), un AudioSource, un TextMeshPro para subtítulos, una `CommandQueue` y las líneas de diálogo.

**Pesadilla:** Canvas negro con un TextMeshPro, un AudioSource, una `CommandQueue` y `NightmareSequence` (grito y disparo).

**Fin del día.** Es una pantalla de transición, no un documento. Aparece cada vez que salís de un recuerdo, se muestra unos segundos y vuelve sola al despacho (sin botón). Como no se puede volver a entrar a un recuerdo, cada recuerdo es "un día" de investigación. Para armarla:
- Escena nueva llamada exactamente `FinDelDia`, agregada a Build Settings.
- Un Canvas con fondo negro y un TextMeshPro (ej. "Fin del día").
- Un objeto vacío con `CommandQueue` + `EndOfDaySequence`. Arrastrar la `CommandQueue` al campo **Queue** y, si quieren, cambiar **Seconds** (3 por defecto).

**Finales:** Canvas con textos o imágenes y un botón que llame a `SceneAdministrator.LoadMenu`.

---

## 8. Checklist

**Antes que nada**
- [ ] Preguntar a los profes por la versión de Unity (6 vs 2022.3.35 LTS)

**Código (hecho)**
- [x] Borrar la práctica vieja y `SampleScene`
- [x] Pasar `FirstPersonPlayer` y `PlayerInteractor` al Input System (+ acción Tab)
- [x] `SceneAdministrator` asincrónico + escenas nuevas
- [x] `GameEvents` (3 eventos) + `GameProgress`
- [x] `ICommand` + comandos + `CommandQueue`
- [x] `MemoryClue` + `MemoryManager` + `ClueListUI` (scroll)
- [x] `MemoryDocument` + `FinalCaseDocument` + `CinematicZone`
- [x] `NightmareSequence` + `EndOfDaySequence`

**Setup general (editor)**
- [ ] Instalar Cinemachine y pasar la cámara del Player (`CinemachineCamera` en el `Eye` + `CinemachineBrain` en la Main Camera)
- [ ] Botón "Jugar" del menú → `StartNewGame` (cuando exista la Pesadilla)
- [ ] Agregar cada escena nueva a Build Settings
- [ ] Prefabs de todos los objetos nuevos (documentos, pistas, renglón de la lista)

**Escenas**
- [ ] Pesadilla
- [ ] Despacho: los 3 documentos de los sospechosos (`MemoryDocument`)
- [ ] Despacho: el 4º documento, tu caso (`FinalCaseDocument`) + panel de decisión con 2 botones
- [ ] Despacho: conversación de fondo (`CinematicZone` + cámara de zoom + subtítulos)
- [ ] Recuerdo Mateo
- [ ] Recuerdo Camila
- [ ] Recuerdo Romina
- [ ] Fin del día (pantalla de transición que vuelve sola al despacho)
- [ ] Final Confesar
- [ ] Final No hacer nada

**Otros**
- [ ] Pitch de venta
- [ ] Audios: grito, disparo, llamada de Camila, radio, conversación del despacho
