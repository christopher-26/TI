# TI en Crisis - Prototipo Universitario VR

Prototipo de realidad virtual universitario desarrollado en **Unity 6 (6000.x)** con **Universal Render Pipeline (URP)**, **XR Interaction Toolkit 3.x (XRI 3.5.1)**, **OpenXR** y el **New Input System**.

El jugador asume el rol del técnico de soporte informático en una empresa con ventanilla de retiro y pedidos. A lo largo del turno, debe diagnosticar y resolver múltiples crisis de TI mientras el dinero de la empresa se agota segundo a segundo y los clientes se impacientan en la ventanilla exterior.

---

## 1. Paquetes Requeridos (Unity Package Manager)

Instalar o verificar a través de `Window > Package Manager`:

| Paquete | ID del Paquete | Versión Utilizada | Notas |
|---|---|---|---|
| **XR Interaction Toolkit** | `com.unity.xr.interaction.toolkit` | `3.5.1` (XRI 3.x) | Importar los samples: `Starter Assets` y `XR Device Simulator`. |
| **XR Plugin Management** | `com.unity.xr.management` | `4.5.x` | Gestor universal de proveedores XR. |
| **OpenXR Plugin** | `com.unity.xr.openxr` | `1.14.x+` | Runtime estándar abierto para visores VR y simulador. |
| **Input System** | `com.unity.inputsystem` | `1.13.x+` | Sistema de eventos y acciones de entrada. |
| **TextMesh Pro** | `com.unity.ugui` / `TextMeshPro` | Nativo Unity | Importar recursos esenciales (`Window > TextMeshPro > Import TMP Essential Resources`). |

---

## 2. Ajustes Manuales del Proyecto (Project Settings)

### A. Input System
1. Abrir `Edit > Project Settings > Player > Other Settings`.
2. En **Active Input Handling**, seleccionar **Input System Package (New)** (o **Both**).
3. Si el Editor solicita reiniciar, confirmar el reinicio.

### B. XR Plug-in Management & OpenXR
1. Abrir `Edit > Project Settings > XR Plug-in Management`.
2. **Pestaña Standalone (PC / Simulador de Escritorio):**
   - Activar la casilla **OpenXR**.
   - Ir a la sub-pestaña `OpenXR` (bajo XR Plug-in Management):
     - **Modo sin visor (Simulador de Escritorio):** En la lista *OpenXR Feature Groups*, activar **Mock Runtime**. Esto permite ejecutar Play mode sin conectar un visor físico.
     - **Modo con visor PC VR (Quest Link / SteamVR):** Si vas a jugar con visor físico conectado a la PC vía cable Link o AirLink, activar **Oculus Touch Controller Profile** en *Interaction Profiles* y desactivar *Mock Runtime*.
3. **Pestaña Android (Meta Quest autónomo):**
   - Activar **OpenXR**.
   - En la sub-pestaña `OpenXR > Android`:
     - Activar **Meta Quest Support** en la lista de Features.
     - En *Interaction Profiles*, agregar **Oculus Touch Controller Profile**.

### C. TextMesh Pro
1. Ejecutar en el menú superior: `Window > TextMeshPro > Import TMP Essential Resources`.

---

## 3. Cómo Construir y Abrir la Escena

El proyecto cuenta con un generador procedimental que crea todo el escenario y enlaza scripts sin requerir ningún asset 3D externo:

1. En la barra de menú superior de Unity, hacer clic en:  
   **`TI en Crisis > Construir escena`**
2. El script generará y guardará:
   - Escena: `Assets/Scenes/TICrisis.unity`
   - Materiales URP generados por código en: `Assets/Materials/`
3. Para abrir la escena en cualquier momento:  
   **`TI en Crisis > Abrir escena`**

---

## 4. Pruebas sin Visor (XR Device Simulator)

El prototipo se puede jugar y depurar completamente desde la computadora usando teclado y ratón:

1. Abrir la escena `Assets/Scenes/TICrisis.unity`.
2. Presionar el botón **Play** en Unity.
3. **Mapeo de Controles del XR Device Simulator:**
   - **Clic derecho + Mover ratón:** Rotar la orientación de la cabeza / visor.
   - **Teclas W / A / S / D:** Desplazarse por la habitación.
   - **Mantener Barra Espaciadora:** Activar y controlar el mando derecho (controlador con rayo).
   - **Mantener Shift Izquierdo:** Activar y controlar el mando izquierdo.
   - **Rueda del ratón (mientras se mantiene Espacio o Shift):** Acercar o alejar el mando.
   - **Clic Izquierdo (mientras se apunta con el rayo):** Disparar el gatillo (Trigger) para interactuar con objetos y botones UI.
   - **Tecla Tab:** Alternar entre el control de la cabeza y el control del mando sin necesidad de mantener presionada la tecla.
   - *(Fallback de conveniencia)*: También puedes hacer clic izquierdo directo con el ratón sobre cualquier equipo para abrir su panel.

---

## 5. Compilación para Meta Quest (APK Autónomo)

Para compilar y cargar el juego directamente en un visor Meta Quest (Quest 2, Quest 3, Quest Pro):

1. **Cambiar Plataforma a Android:**
   - Ir a `File > Build Profiles` (o `File > Build Settings`).
   - Seleccionar **Android** y hacer clic en **Switch Platform**.
   - En **Texture Compression**, elegir **ASTC**.
2. **Ajustes de Player para Quest:**
   - En `Edit > Project Settings > Player > Android`:
     - **Color Space:** `Linear`.
     - **Auto Graphics API:** Desactivar y asegurar que **Vulkan** esté en la parte superior (o *OpenGLES3* como alternativa).
     - **Minimum API Level:** `Android 10.0 (API level 29)` o superior.
     - **Target API Level:** `Automatic` (o nivel 32/34).
     - **Scripting Backend:** `IL2CPP`.
     - **Target Architectures:** Marcar únicamente **ARM64** (desmarcar ARMv7).
3. **Ajustes de XR para Android:**
   - En `Project Settings > XR Plug-in Management > Android`:
     - Confirmar que **OpenXR** está marcado.
     - En `OpenXR > Android Features`, confirmar que **Meta Quest Support** esté activado.
4. **Construir el APK:**
   - En `Build Profiles`, agregar la escena `Assets/Scenes/TICrisis.unity`.
   - Conectar el visor Quest por cable USB con el *Modo Desarrollador* habilitado.
   - Presionar **Build and Run** para instalar y ejecutar automáticamente en el visor.

---

## 6. Resumen de Scripts y Arquitectura

Todos los scripts se encuentran en `Assets/Scripts/` y `Assets/Editor/`, con comentarios pedagógicos en español:

| Script | Ubicación | Responsabilidad Principal |
|---|---|---|
| **[`GameManager.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/GameManager.cs)** | `Assets/Scripts/` | Singleton central. Controla Dinero (inicio S/ 5,000), Reputación (inicio 70, rango 0-100), reloj de tiempo, lista de incidentes activos y deducción por segundo. Si Reputación < 40, pérdidas $\times 1.5$. Evalúa finales (`Quiebra`, `NegocioSalvado`, `Sobreviviste`) y almacena el historial de auditoría. |
| **[`Incidente.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/Incidente.cs)** | `Assets/Scripts/` | Modelo de datos que define un fallo técnico: nombre, descripción contextual, costo por segundo, estado (`activo`), timestamps y lista de opciones de solución. |
| **[`OpcionSolucion.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/OpcionSolucion.cs)** | `Assets/Scripts/` | Modelo de cada alternativa de solución: texto, resultado (`Correcta`, `Regular`, `Mala`), penalización en segundos (tiempo de trabajo con barra de progreso), variación de reputación, si resuelve, si reaparece tras N segundos y mensaje para el informe final. |
| **[`HUDPanel.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/HUDPanel.cs)** | `Assets/Scripts/` | Controlador del Canvas World Space ubicado sobre la ventanilla. Actualiza en tiempo real los valores de dinero, reputación, cronómetro y el desglose de fallos activos con sus pérdidas. |
| **[`PanelOpciones.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/PanelOpciones.cs)** | `Assets/Scripts/` | Panel World Space flotante reutilizable. Al interactuar con cualquier equipo, se posiciona frente al jugador, genera botones para cada opción, muestra una barra de progreso mientras corre la penalización y retroalimenta el resultado antes de cerrarse. Protegido contra interrupciones o clics concurrentes. |
| **[`IncidenteInteractuable.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/IncidenteInteractuable.cs)** | `Assets/Scripts/` | Componente adjunto a los objetos físicos del rack, PCs e impresora. Emplea `XRSimpleInteractable` para detectar hover (cambio de color) y selección (gatillo XR / rayo) abriendo el `PanelOpciones`. |
| **[`IncidenteBaseDatos.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/IncidenteBaseDatos.cs)** | `Assets/Scripts/` | **Problema 1:** Base de datos borrada a las 10:30 a.m. (-S/ 5/s). Interactuable en el Servidor. Síntoma: monitor de Caja en "SIN REGISTROS". Opciones de restauración por backup con trampa temporal de las 11:00 a.m. y backup semanal que enfurece a los clientes. |
| **[`IncidenteInternet.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/IncidenteInternet.cs)** | `Assets/Scripts/` | **Problema 2:** Corte de red física (-S/ 3/s). Interactuable en Switch y Router. Síntoma: 3 monitores en "SIN CONEXION", luz roja en Switch y cable desconectado colgando. Resolverlo conecta el cable y pasa la luz a verde. |
| **[`IncidenteImpresora.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/IncidenteImpresora.cs)** | `Assets/Scripts/` | **Problema 3:** Desconexión por cambio dinámico de IP DHCP (-S/ 1/s). Interactuable en Impresora. Síntoma: pantalla LCD en "NO DISPONIBLE" y empleado de Caja en "(Sin boletas)". Reiniciar es una solución temporal que reaparece a los 60s; la solución correcta es IP fija. |
| **[`ClientesFila.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/ClientesFila.cs)** | `Assets/Scripts/` | Fila dinámica de clientes cápsula en el exterior de la ventanilla. La cantidad escala según fallos activos ($1 + 2 \times \text{incidentes}$). Se tornan rojos si Reputación < 40 y morados permanentes con reclamos si se pierden datos con el backup semanal. |
| **[`InformeFinal.cs`](file:///c:/TI-Juego/TI/Assets/Scripts/InformeFinal.cs)** | `Assets/Scripts/` | Pantalla de auditoría final en World Space que aparece frente a los ojos del jugador al terminar la partida. Detalla el desenlace, balance financiero, reputación, orden cronológico de atención y recomendaciones técnicas para decisiones regulares o malas, con botón "Reintentar Turno". |
| **[`ConstructorEscena.cs`](file:///c:/TI-Juego/TI/Assets/Editor/ConstructorEscena.cs)** | `Assets/Editor/` | Script de editor con la opción de menú `"TI en Crisis > Construir escena"`. Levanta la habitación 6x5m, mostradores, puestos, equipos de red, cables, luces, HUD, fila exterior, origen XR y simulador, guardando la escena de forma lista para ejecutar. |

---

## 7. Pasos Manuales Restantes y Limitaciones Conocidas

### A. Acciones Manuales Requeridas (solo si se abre en una máquina nueva)
1. **Importar TMP Essentials:** Si los textos aparecen en rosa o vacíos, ejecutar `Window > TextMeshPro > Import TMP Essential Resources`.
2. **Reconstruir la Escena:** Al clonar o abrir el proyecto por primera vez, hacer clic en `TI en Crisis > Construir escena`.
3. **Verificar Mock Runtime en PC:** Asegurar que en `Edit > Project Settings > XR Plug-in Management > OpenXR` esté activo **Mock Runtime** para probar con teclado/ratón sin casco conectado.

### B. Limitaciones Conocidas del Prototipo
1. **Arte y Geometría:** El escenario está compuesto enteramente por primitivas geométricas de Unity (cubos, cilindros, esferas y cápsulas) y materiales URP estándar creados por código, cumpliendo con la restricción de cero dependencias externas.
2. **Persistencia:** No cuenta con guardado en disco ni base de datos local; cada reinicio ("Reintentar Turno") recarga la escena en su estado inicial.
3. **Modo Un Jugador Local:** No cuenta con funciones de red ni multijugador.
4. **Locomoción VR:** Está diseñado para escala de habitación (*Room-Scale*) fija frente al área técnica y ventanilla; no incluye teletransporte ni movimiento continuo para mantener la simplicidad y evitar mareo por movimiento (*motion sickness*).
