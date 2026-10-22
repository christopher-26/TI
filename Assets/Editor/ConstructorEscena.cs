using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Script editor para generar y configurar la escena del prototipo "TI en Crisis".
/// Crea el escenario con primitivas, materiales generados en código, puestos de trabajo,
/// equipamiento de TI, ventanilla de pedidos, HUD interactivo, panel de opciones flotante,
/// GameManager y configuración XR con simulador.
/// </summary>
public static class ConstructorEscena
{
    private const string RUTA_ESCENA = "Assets/Scenes/TICrisis.unity";
    private const string CARPETA_MATERIALES = "Assets/Materials";

    // Auto-construir una sola vez al cargar el proyecto si la escena aún no existe
    [InitializeOnLoadMethod]
    private static void AutoConstruirAlCargar()
    {
        if (!File.Exists(RUTA_ESCENA))
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(RUTA_ESCENA))
                {
                    Debug.Log("[TI en Crisis] Escena no encontrada. Creándola automáticamente...");
                    ConstruirEscena();
                }
            };
        }
    }

    [MenuItem("TI en Crisis/Construir escena", false, 1)]
    public static void ConstruirEscena()
    {
        // 1. Asegurar directorios requeridos
        AsegurarDirectorios();

        // 2. Crear una escena completamente limpia
        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 3. Crear o recuperar materiales básicos para el entorno
        Material matSuelo = ObtenerOCrearMaterial("M_Suelo", new Color(0.72f, 0.74f, 0.77f));
        Material matParedes = ObtenerOCrearMaterial("M_Paredes", new Color(0.92f, 0.93f, 0.94f));
        Material matTecho = ObtenerOCrearMaterial("M_Techo", new Color(0.96f, 0.96f, 0.97f));
        Material matMostrador = ObtenerOCrearMaterial("M_Mostrador", new Color(0.55f, 0.35f, 0.20f));
        Material matEscritorio = ObtenerOCrearMaterial("M_Escritorio", new Color(0.38f, 0.40f, 0.45f));
        Material matComputadora = ObtenerOCrearMaterial("M_Computadora", new Color(0.18f, 0.18f, 0.20f));
        Material matMonitorMarco = ObtenerOCrearMaterial("M_MonitorMarco", new Color(0.12f, 0.12f, 0.14f));
        Material matPantalla = ObtenerOCrearMaterial("M_Pantalla", new Color(0.04f, 0.08f, 0.14f));
        Material matImpresora = ObtenerOCrearMaterial("M_Impresora", new Color(0.85f, 0.86f, 0.88f));
        Material matRack = ObtenerOCrearMaterial("M_RackTI", new Color(0.15f, 0.16f, 0.18f));
        Material matRouter = ObtenerOCrearMaterial("M_Router", new Color(0.12f, 0.30f, 0.55f));
        Material matSwitch = ObtenerOCrearMaterial("M_Switch", new Color(0.25f, 0.30f, 0.36f));
        Material matServidor = ObtenerOCrearMaterial("M_Servidor", new Color(0.55f, 0.60f, 0.65f));

        // Materiales de trabajadores
        Material matCaja = ObtenerOCrearMaterial("M_Trabajador_Caja", new Color(0.20f, 0.50f, 0.90f));
        Material matPedidos = ObtenerOCrearMaterial("M_Trabajador_Pedidos", new Color(0.20f, 0.70f, 0.40f));
        Material matAdmin = ObtenerOCrearMaterial("M_Trabajador_Admin", new Color(0.90f, 0.55f, 0.15f));

        // Materiales de fila de clientes (Task 6)
        Material matClienteNormal = ObtenerOCrearMaterial("M_ClienteNormal", new Color(0.25f, 0.55f, 0.90f));
        Material matClienteRojo = ObtenerOCrearMaterial("M_ClienteRojo", new Color(0.92f, 0.20f, 0.20f));
        Material matClienteMolesto = ObtenerOCrearMaterial("M_ClienteMolesto", new Color(0.55f, 0.15f, 0.75f));

        // 4. Configurar iluminación del entorno
        ConfigurarIluminacion();

        // 5. Construir arquitectura de la habitación (6m ancho x 5m profundidad x 3m altura)
        Transform rootEntorno = new GameObject("=== ARQUITECTURA ===").transform;
        ConstruirHabitacion(rootEntorno, matSuelo, matParedes, matTecho, matMostrador);

        // 6. Construir puestos de trabajo de PCs y trabajadores
        Transform rootPuestos = new GameObject("=== PUESTOS_TRABAJO ===").transform;
        List<TextMeshPro> monitoresPCs = ConstruirPuestos(rootPuestos, matEscritorio, matComputadora, matMonitorMarco, matPantalla, matCaja, matPedidos, matAdmin);

        // 7. Configurar Core del Juego (GameManager)
        Transform rootCore = new GameObject("=== SISTEMA_CORE ===").transform;
        GameObject gmGO = new GameObject("GameManager");
        gmGO.transform.SetParent(rootCore);
        GameManager gm = gmGO.AddComponent<GameManager>();
        gm.Dinero = 5000f;
        gm.Reputacion = 70f;

        // 8. Construir World Space Canvas sobre la ventanilla (HUD), Panel de Opciones flotante e Informe Final
        Transform rootUI = new GameObject("=== INTERFAZ_HUD ===").transform;
        ConstruirCanvasHUD(rootUI);
        PanelOpciones panelOpciones = ConstruirPanelOpciones(rootUI);
        ConstruirCanvasInformeFinal(rootUI);

        // 9. Construir equipamiento TI (Impresora en mesa lateral y Rack en esquina con incidentes)
        Transform rootTI = new GameObject("=== EQUIPAMIENTO_TI ===").transform;
        ConstruirEquiposTI(rootTI, matEscritorio, matImpresora, matRack, matRouter, matSwitch, matServidor, panelOpciones, gm, monitoresPCs, rootPuestos);

        // 10. Construir fila de clientes en la ventanilla exterior
        Transform rootFila = new GameObject("=== FILA_CLIENTES ===").transform;
        ConstruirFilaClientes(rootFila, matClienteNormal, matClienteRojo, matClienteMolesto);

        // 11. Configurar sistema XR (Interaction Manager, EventSystem, XR Origin con rayos y Simulador)
        Transform rootXR = new GameObject("=== SISTEMA_XR ===").transform;
        ConfigurarXR(rootXR);

        // 12. Guardar la escena y actualizar la base de datos de activos
        EditorSceneManager.SaveScene(escena, RUTA_ESCENA);
        Lightmapping.ClearLightingDataAsset();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>[TI en Crisis]</color> ¡Escena creada y guardada exitosamente en <b>{RUTA_ESCENA}</b> con todos los sistemas, fila de clientes e informe final!");
    }

    [MenuItem("TI en Crisis/Abrir escena", false, 2)]
    public static void AbrirEscena()
    {
        if (File.Exists(RUTA_ESCENA))
        {
            EditorSceneManager.OpenScene(RUTA_ESCENA, OpenSceneMode.Single);
            Debug.Log($"[TI en Crisis] Escena cargada: {RUTA_ESCENA}");
        }
        else
        {
            Debug.LogWarning($"[TI en Crisis] La escena {RUTA_ESCENA} aún no existe. Pulsa 'TI en Crisis > Construir escena'.");
        }
    }

    [MenuItem("TI en Crisis/Limpiar datos de iluminacion", false, 10)]
    public static void LimpiarIluminacion()
    {
        Lightmapping.ClearLightingDataAsset();
        Lightmapping.Clear();
        Lightmapping.bakedGI = false;
        Lightmapping.realtimeGI = false;
        Debug.Log("<color=green>[TI en Crisis]</color> Datos de iluminación obsoletos limpiados exitosamente.");
    }

    #region Configuración de Entorno y Arquitectura

    private static void ConfigurarIluminacion()
    {
        // Limpiar cualquier dato de iluminación horneada obsoleto o incompatible de sesiones anteriores
        Lightmapping.ClearLightingDataAsset();
        Lightmapping.bakedGI = false;
        Lightmapping.realtimeGI = false;

        Transform rootLuz = new GameObject("=== ILUMINACION ===").transform;

        // Configuración de luz ambiental suave
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.40f, 0.40f, 0.45f);

        // Luz direccional general
        GameObject dirLightGO = new GameObject("Luz_Direccional");
        dirLightGO.transform.SetParent(rootLuz);
        Light dirLight = dirLightGO.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = new Color(1.0f, 0.98f, 0.92f);
        dirLight.intensity = 0.75f;
        dirLight.shadows = LightShadows.Soft;
        dirLightGO.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

        // Luz de techo central
        GameObject ceilingLightGO = new GameObject("Luz_Techo");
        ceilingLightGO.transform.SetParent(rootLuz);
        ceilingLightGO.transform.position = new Vector3(0f, 2.85f, 0f);
        Light ceilingLight = ceilingLightGO.AddComponent<Light>();
        ceilingLight.type = LightType.Point;
        ceilingLight.color = new Color(0.95f, 0.97f, 1.0f);
        ceilingLight.intensity = 1.8f;
        ceilingLight.range = 10f;
        ceilingLight.shadows = LightShadows.Soft;

        // Plafón visual de la luz
        GameObject plafon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plafon.name = "Plafon_Luz";
        plafon.transform.SetParent(ceilingLightGO.transform, false);
        plafon.transform.localScale = new Vector3(1.0f, 0.05f, 1.0f);
        plafon.GetComponent<MeshRenderer>().sharedMaterial = ObtenerOCrearMaterial("M_PlafonLuz", new Color(0.98f, 0.98f, 0.95f));
    }

    private static void ConstruirHabitacion(Transform padre, Material matSuelo, Material matParedes, Material matTecho, Material matMostrador)
    {
        // 1. Suelo
        GameObject suelo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        suelo.name = "Suelo";
        suelo.transform.SetParent(padre);
        suelo.transform.position = new Vector3(0f, -0.05f, 0f);
        suelo.transform.localScale = new Vector3(6f, 0.1f, 5f);
        suelo.GetComponent<MeshRenderer>().sharedMaterial = matSuelo;

        // 2. Techo
        GameObject techo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        techo.name = "Techo";
        techo.transform.SetParent(padre);
        techo.transform.position = new Vector3(0f, 3.05f, 0f);
        techo.transform.localScale = new Vector3(6f, 0.1f, 5f);
        techo.GetComponent<MeshRenderer>().sharedMaterial = matTecho;

        // 3. Pared Norte (fondo, Z = +2.5)
        GameObject paredNorte = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paredNorte.name = "Pared_Norte";
        paredNorte.transform.SetParent(padre);
        paredNorte.transform.position = new Vector3(0f, 1.5f, 2.55f);
        paredNorte.transform.localScale = new Vector3(6f, 3f, 0.1f);
        paredNorte.GetComponent<MeshRenderer>().sharedMaterial = matParedes;

        // 4. Pared Oeste (izquierda, X = -3.0)
        GameObject paredOeste = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paredOeste.name = "Pared_Oeste";
        paredOeste.transform.SetParent(padre);
        paredOeste.transform.position = new Vector3(-3.05f, 1.5f, 0f);
        paredOeste.transform.localScale = new Vector3(0.1f, 3f, 5f);
        paredOeste.GetComponent<MeshRenderer>().sharedMaterial = matParedes;

        // 5. Pared Este (derecha, X = +3.0)
        GameObject paredEste = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paredEste.name = "Pared_Este";
        paredEste.transform.SetParent(padre);
        paredEste.transform.position = new Vector3(3.05f, 1.5f, 0f);
        paredEste.transform.localScale = new Vector3(0.1f, 3f, 5f);
        paredEste.GetComponent<MeshRenderer>().sharedMaterial = matParedes;

        // 6. Pared Sur con ventanilla de pedidos (Z = -2.5)
        Transform ventanillaRoot = new GameObject("Pared_Sur_Ventanilla").transform;
        ventanillaRoot.SetParent(padre);

        GameObject muroBajo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        muroBajo.name = "Muro_Inferior";
        muroBajo.transform.SetParent(ventanillaRoot);
        muroBajo.transform.position = new Vector3(0f, 0.475f, -2.55f);
        muroBajo.transform.localScale = new Vector3(6f, 0.95f, 0.1f);
        muroBajo.GetComponent<MeshRenderer>().sharedMaterial = matParedes;

        GameObject colIzq = GameObject.CreatePrimitive(PrimitiveType.Cube);
        colIzq.name = "Columna_Izquierda";
        colIzq.transform.SetParent(ventanillaRoot);
        colIzq.transform.position = new Vector3(-2.15f, 1.975f, -2.55f);
        colIzq.transform.localScale = new Vector3(1.7f, 2.05f, 0.1f);
        colIzq.GetComponent<MeshRenderer>().sharedMaterial = matParedes;

        GameObject colDer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        colDer.name = "Columna_Derecha";
        colDer.transform.SetParent(ventanillaRoot);
        colDer.transform.position = new Vector3(2.15f, 1.975f, -2.55f);
        colDer.transform.localScale = new Vector3(1.7f, 2.05f, 0.1f);
        colDer.GetComponent<MeshRenderer>().sharedMaterial = matParedes;

        GameObject dintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dintel.name = "Dintel_Superior";
        dintel.transform.SetParent(ventanillaRoot);
        dintel.transform.position = new Vector3(0f, 2.625f, -2.55f);
        dintel.transform.localScale = new Vector3(2.6f, 0.75f, 0.1f);
        dintel.GetComponent<MeshRenderer>().sharedMaterial = matParedes;

        GameObject mostrador = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mostrador.name = "Mostrador_Atencion";
        mostrador.transform.SetParent(ventanillaRoot);
        mostrador.transform.position = new Vector3(0f, 0.95f, -2.5f);
        mostrador.transform.localScale = new Vector3(2.8f, 0.08f, 0.65f);
        mostrador.GetComponent<MeshRenderer>().sharedMaterial = matMostrador;

        GameObject cartelVentanilla = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cartelVentanilla.name = "Letrero_Ventanilla";
        cartelVentanilla.transform.SetParent(ventanillaRoot);
        cartelVentanilla.transform.position = new Vector3(0f, 2.18f, -2.48f);
        cartelVentanilla.transform.localScale = new Vector3(2.2f, 0.18f, 0.03f);
        cartelVentanilla.GetComponent<MeshRenderer>().sharedMaterial = matMostrador;

        GameObject textoCartel = new GameObject("Texto_Letrero");
        textoCartel.transform.SetParent(cartelVentanilla.transform, false);
        textoCartel.transform.localPosition = new Vector3(0f, 0f, 0.55f);
        textoCartel.transform.localRotation = Quaternion.Euler(0, 180, 0);
        TextMeshPro tmpCartel = textoCartel.AddComponent<TextMeshPro>();
        tmpCartel.text = "VENTANILLA DE PEDIDOS Y RETIRO";
        tmpCartel.alignment = TextAlignmentOptions.Center;
        tmpCartel.fontSize = 1.3f;
        tmpCartel.fontStyle = FontStyles.Bold;
        tmpCartel.color = Color.white;
        tmpCartel.rectTransform.sizeDelta = new Vector2(2.1f, 0.16f);
    }

    #endregion

    #region Puestos de Trabajo y Trabajadores

    private static List<TextMeshPro> ConstruirPuestos(
        Transform padre,
        Material matEscritorio,
        Material matPC,
        Material matMonitorMarco,
        Material matPantalla,
        Material matCaja,
        Material matPedidos,
        Material matAdmin)
    {
        List<TextMeshPro> monitores = new List<TextMeshPro>();
        monitores.Add(CrearPuestoIndividual("Caja", new Vector3(-1.7f, 0f, 1.35f), padre, matEscritorio, matPC, matMonitorMarco, matPantalla, matCaja));
        monitores.Add(CrearPuestoIndividual("Pedidos online", new Vector3(0.0f, 0f, 1.35f), padre, matEscritorio, matPC, matMonitorMarco, matPantalla, matPedidos));
        monitores.Add(CrearPuestoIndividual("Administracion", new Vector3(1.7f, 0f, 1.35f), padre, matEscritorio, matPC, matMonitorMarco, matPantalla, matAdmin));
        return monitores;
    }

    private static TextMeshPro CrearPuestoIndividual(
        string nombre,
        Vector3 posicion,
        Transform padre,
        Material matEscritorio,
        Material matPC,
        Material matMonitorMarco,
        Material matPantalla,
        Material matTrabajador)
    {
        GameObject puestoRoot = new GameObject($"Puesto_{nombre}");
        puestoRoot.transform.SetParent(padre);
        puestoRoot.transform.position = posicion;

        // 1. Mesa de escritorio
        GameObject tablero = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tablero.name = "Mesa_Tablero";
        tablero.transform.SetParent(puestoRoot.transform, false);
        tablero.transform.localPosition = new Vector3(0f, 0.72f, 0f);
        tablero.transform.localScale = new Vector3(1.3f, 0.05f, 0.7f);
        tablero.GetComponent<MeshRenderer>().sharedMaterial = matEscritorio;

        float anchoMesa = 1.2f;
        float profMesa = 0.6f;
        Vector3[] posPatas = new Vector3[]
        {
            new Vector3(-anchoMesa / 2f, 0.35f, -profMesa / 2f),
            new Vector3(anchoMesa / 2f, 0.35f, -profMesa / 2f),
            new Vector3(-anchoMesa / 2f, 0.35f, profMesa / 2f),
            new Vector3(anchoMesa / 2f, 0.35f, profMesa / 2f)
        };
        for (int i = 0; i < posPatas.Length; i++)
        {
            GameObject pata = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pata.name = $"Pata_{i + 1}";
            pata.transform.SetParent(puestoRoot.transform, false);
            pata.transform.localPosition = posPatas[i];
            pata.transform.localScale = new Vector3(0.05f, 0.70f, 0.05f);
            pata.GetComponent<MeshRenderer>().sharedMaterial = matEscritorio;
        }

        // 2. Torre de PC
        GameObject torrePC = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torrePC.name = "Torre_PC";
        torrePC.transform.SetParent(puestoRoot.transform, false);
        torrePC.transform.localPosition = new Vector3(0.5f, 0.22f, 0f);
        torrePC.transform.localScale = new Vector3(0.2f, 0.44f, 0.45f);
        torrePC.GetComponent<MeshRenderer>().sharedMaterial = matPC;

        // 3. Monitor (Marco + Quad con label TextMeshPro)
        Transform monitorRoot = new GameObject("Monitor").transform;
        monitorRoot.SetParent(puestoRoot.transform, false);
        monitorRoot.localPosition = new Vector3(0f, 0.75f, -0.1f);

        GameObject baseMon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseMon.name = "Base_Monitor";
        baseMon.transform.SetParent(monitorRoot, false);
        baseMon.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        baseMon.transform.localScale = new Vector3(0.18f, 0.02f, 0.18f);
        baseMon.GetComponent<MeshRenderer>().sharedMaterial = matMonitorMarco;

        GameObject soporteMon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        soporteMon.name = "Soporte_Monitor";
        soporteMon.transform.SetParent(monitorRoot, false);
        soporteMon.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        soporteMon.transform.localScale = new Vector3(0.04f, 0.22f, 0.04f);
        soporteMon.GetComponent<MeshRenderer>().sharedMaterial = matMonitorMarco;

        GameObject marcoMon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marcoMon.name = "Marco_Monitor";
        marcoMon.transform.SetParent(monitorRoot, false);
        marcoMon.transform.localPosition = new Vector3(0f, 0.28f, 0f);
        marcoMon.transform.localScale = new Vector3(0.6f, 0.38f, 0.03f);
        marcoMon.GetComponent<MeshRenderer>().sharedMaterial = matMonitorMarco;

        GameObject quadPantalla = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadPantalla.name = "Pantalla_Quad";
        quadPantalla.transform.SetParent(monitorRoot, false);
        quadPantalla.transform.localPosition = new Vector3(0f, 0.28f, -0.016f);
        quadPantalla.transform.localRotation = Quaternion.identity;
        quadPantalla.transform.localScale = new Vector3(0.56f, 0.34f, 1f);
        quadPantalla.GetComponent<MeshRenderer>().sharedMaterial = matPantalla;

        GameObject labelMonitor = new GameObject("Label_Monitor_TMP");
        labelMonitor.transform.SetParent(monitorRoot, false);
        labelMonitor.transform.localPosition = new Vector3(0f, 0.28f, -0.018f);
        labelMonitor.transform.localRotation = Quaternion.identity;
        TextMeshPro tmpMonitor = labelMonitor.AddComponent<TextMeshPro>();
        tmpMonitor.text = $"<b>{nombre.ToUpper()}</b>\n<size=70%><color=#38BDF8>Terminal Conectada</color></size>";
        tmpMonitor.alignment = TextAlignmentOptions.Center;
        tmpMonitor.fontSize = 1.3f;
        tmpMonitor.color = Color.white;
        tmpMonitor.rectTransform.sizeDelta = new Vector2(0.54f, 0.32f);

        GameObject teclado = GameObject.CreatePrimitive(PrimitiveType.Cube);
        teclado.name = "Teclado";
        teclado.transform.SetParent(puestoRoot.transform, false);
        teclado.transform.localPosition = new Vector3(0f, 0.755f, -0.22f);
        teclado.transform.localScale = new Vector3(0.40f, 0.015f, 0.14f);
        teclado.GetComponent<MeshRenderer>().sharedMaterial = matPC;

        GameObject raton = GameObject.CreatePrimitive(PrimitiveType.Cube);
        raton.name = "Raton";
        raton.transform.SetParent(puestoRoot.transform, false);
        raton.transform.localPosition = new Vector3(0.28f, 0.755f, -0.22f);
        raton.transform.localScale = new Vector3(0.06f, 0.02f, 0.10f);
        raton.GetComponent<MeshRenderer>().sharedMaterial = matPC;

        // 4. Trabajador (Cápsula)
        GameObject trabajador = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        trabajador.name = $"Trabajador_{nombre}";
        trabajador.transform.SetParent(puestoRoot.transform, false);
        trabajador.transform.localPosition = new Vector3(0f, 0.85f, 0.55f);
        trabajador.transform.localScale = new Vector3(0.5f, 0.75f, 0.5f);
        trabajador.GetComponent<MeshRenderer>().sharedMaterial = matTrabajador;

        GameObject etiquetaNombre = new GameObject($"Etiqueta_{nombre}");
        etiquetaNombre.transform.SetParent(puestoRoot.transform, false);
        etiquetaNombre.transform.localPosition = new Vector3(0f, 1.85f, 0.55f);
        etiquetaNombre.transform.localRotation = Quaternion.identity;
        TextMeshPro tmpNombre = etiquetaNombre.AddComponent<TextMeshPro>();
        tmpNombre.text = nombre;
        tmpNombre.alignment = TextAlignmentOptions.Center;
        tmpNombre.fontSize = 2.6f;
        tmpNombre.fontStyle = FontStyles.Bold;
        tmpNombre.color = Color.white;
        tmpNombre.rectTransform.sizeDelta = new Vector2(2f, 0.5f);

        return tmpMonitor;
    }

    #endregion

    #region Equipamiento de TI

    private static void ConstruirEquiposTI(
        Transform padre,
        Material matMesa,
        Material matImpresora,
        Material matRack,
        Material matRouter,
        Material matSwitch,
        Material matServidor,
        PanelOpciones panelOpciones,
        GameManager gm,
        List<TextMeshPro> monitoresPCs,
        Transform rootPuestos)
    {
        // 1. Mesa lateral con impresora (a la izquierda: X = -2.3f, Z = -0.5f)
        Transform mesaImpresoraRoot = new GameObject("Mesa_Impresora").transform;
        mesaImpresoraRoot.SetParent(padre);
        mesaImpresoraRoot.position = new Vector3(-2.3f, 0f, -0.5f);
        mesaImpresoraRoot.rotation = Quaternion.Euler(0f, 90f, 0f); // Orientar hacia el centro de la sala

        GameObject mesaImp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesaImp.name = "Tablero_Mesa_Impresora";
        mesaImp.transform.SetParent(mesaImpresoraRoot, false);
        mesaImp.transform.localPosition = new Vector3(0f, 0.72f, 0f);
        mesaImp.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
        mesaImp.GetComponent<MeshRenderer>().sharedMaterial = matMesa;

        for (int i = 0; i < 4; i++)
        {
            float px = (i % 2 == 0) ? -0.35f : 0.35f;
            float pz = (i / 2 == 0) ? -0.35f : 0.35f;
            GameObject pata = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pata.name = $"Pata_Mesa_Imp_{i + 1}";
            pata.transform.SetParent(mesaImpresoraRoot, false);
            pata.transform.localPosition = new Vector3(px, 0.35f, pz);
            pata.transform.localScale = new Vector3(0.05f, 0.70f, 0.05f);
            pata.GetComponent<MeshRenderer>().sharedMaterial = matMesa;
        }

        GameObject impresora = GameObject.CreatePrimitive(PrimitiveType.Cube);
        impresora.name = "Impresora";
        impresora.transform.SetParent(mesaImpresoraRoot, false);
        impresora.transform.localPosition = new Vector3(0f, 0.88f, 0f);
        impresora.transform.localScale = new Vector3(0.50f, 0.26f, 0.44f);
        impresora.GetComponent<MeshRenderer>().sharedMaterial = matImpresora;

        GameObject bandeja = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bandeja.name = "Bandeja_Papel";
        bandeja.transform.SetParent(impresora.transform, false);
        bandeja.transform.localPosition = new Vector3(0f, -0.2f, 0.55f);
        bandeja.transform.localScale = new Vector3(0.55f, 0.08f, 0.35f);
        bandeja.GetComponent<MeshRenderer>().sharedMaterial = ObtenerOCrearMaterial("M_PlasticoNegro", new Color(0.15f, 0.15f, 0.15f));

        GameObject labelImp = new GameObject("Label_Impresora");
        labelImp.transform.SetParent(mesaImpresoraRoot, false);
        labelImp.transform.localPosition = new Vector3(0f, 0.90f, 0.23f);
        labelImp.transform.localRotation = Quaternion.identity;
        TextMeshPro tmpImp = labelImp.AddComponent<TextMeshPro>();
        tmpImp.text = "IMPRESORA";
        tmpImp.alignment = TextAlignmentOptions.Center;
        tmpImp.fontSize = 1.6f;
        tmpImp.fontStyle = FontStyles.Bold;
        tmpImp.color = Color.black;
        tmpImp.rectTransform.sizeDelta = new Vector2(0.48f, 0.20f);

        // Pequeña pantalla LCD integrada en la impresora (indicador de estado / error)
        GameObject marcoPantallaImp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marcoPantallaImp.name = "Marco_Pantalla_Impresora";
        marcoPantallaImp.transform.SetParent(mesaImpresoraRoot, false);
        marcoPantallaImp.transform.localPosition = new Vector3(0.08f, 1.03f, 0.12f);
        marcoPantallaImp.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
        marcoPantallaImp.transform.localScale = new Vector3(0.20f, 0.08f, 0.02f);
        marcoPantallaImp.GetComponent<MeshRenderer>().sharedMaterial = ObtenerOCrearMaterial("M_PlasticoNegro", new Color(0.15f, 0.15f, 0.15f));

        GameObject fondoLCD = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fondoLCD.name = "Fondo_LCD_Impresora";
        fondoLCD.transform.SetParent(marcoPantallaImp.transform, false);
        fondoLCD.transform.localPosition = new Vector3(0f, 0f, 0.52f);
        fondoLCD.transform.localScale = new Vector3(0.92f, 0.82f, 0.10f);
        fondoLCD.GetComponent<MeshRenderer>().sharedMaterial = ObtenerOCrearMaterial("M_PantallaLCD", new Color(0.06f, 0.10f, 0.16f));

        GameObject textoPantallaImpGO = new GameObject("Pantalla_Impresora_TMP");
        textoPantallaImpGO.transform.SetParent(marcoPantallaImp.transform, false);
        textoPantallaImpGO.transform.localPosition = new Vector3(0f, 0f, 0.60f);
        textoPantallaImpGO.transform.localRotation = Quaternion.identity;
        TextMeshPro tmpPantallaImp = textoPantallaImpGO.AddComponent<TextMeshPro>();
        tmpPantallaImp.text = "<color=#EF4444><b>NO DISPONIBLE</b></color>";
        tmpPantallaImp.alignment = TextAlignmentOptions.Center;
        tmpPantallaImp.fontSize = 1.0f;
        tmpPantallaImp.fontStyle = FontStyles.Bold;
        tmpPantallaImp.color = Color.white;
        tmpPantallaImp.rectTransform.sizeDelta = new Vector2(0.90f, 0.80f);

        // 2. Armario de TI / Rack en la esquina (X = 2.2f, Z = -1.6f)
        Transform rackRoot = new GameObject("Rack_TI").transform;
        rackRoot.SetParent(padre);
        rackRoot.position = new Vector3(2.2f, 0f, -1.6f);
        rackRoot.rotation = Quaternion.Euler(0f, -90f, 0f);

        GameObject baseRack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseRack.name = "Rack_Base";
        baseRack.transform.SetParent(rackRoot, false);
        baseRack.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        baseRack.transform.localScale = new Vector3(0.70f, 0.08f, 0.60f);
        baseRack.GetComponent<MeshRenderer>().sharedMaterial = matRack;

        GameObject techoRack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        techoRack.name = "Rack_Techo";
        techoRack.transform.SetParent(rackRoot, false);
        techoRack.transform.localPosition = new Vector3(0f, 1.84f, 0f);
        techoRack.transform.localScale = new Vector3(0.70f, 0.08f, 0.60f);
        techoRack.GetComponent<MeshRenderer>().sharedMaterial = matRack;

        for (int i = 0; i < 4; i++)
        {
            float rx = (i % 2 == 0) ? -0.32f : 0.32f;
            float rz = (i / 2 == 0) ? -0.27f : 0.27f;
            GameObject poste = GameObject.CreatePrimitive(PrimitiveType.Cube);
            poste.name = $"Poste_Rack_{i + 1}";
            poste.transform.SetParent(rackRoot, false);
            poste.transform.localPosition = new Vector3(rx, 0.94f, rz);
            poste.transform.localScale = new Vector3(0.04f, 1.80f, 0.04f);
            poste.GetComponent<MeshRenderer>().sharedMaterial = matRack;
        }

        float[] alturasBaldas = new float[] { 0.45f, 0.90f, 1.35f };
        for (int i = 0; i < alturasBaldas.Length; i++)
        {
            GameObject balda = GameObject.CreatePrimitive(PrimitiveType.Cube);
            balda.name = $"Balda_{i + 1}";
            balda.transform.SetParent(rackRoot, false);
            balda.transform.localPosition = new Vector3(0f, alturasBaldas[i], 0f);
            balda.transform.localScale = new Vector3(0.64f, 0.03f, 0.54f);
            balda.GetComponent<MeshRenderer>().sharedMaterial = matRack;
        }

        // 3 Cubos etiquetados dentro del Rack: Servidor (abajo), Switch (medio), Router (arriba)
        GameObject cuboServidor = CrearCuboEquipoTI("Servidor", new Vector3(0f, 0.63f, 0f), new Vector3(0.50f, 0.30f, 0.44f), matServidor, rackRoot);
        GameObject cuboSwitch = CrearCuboEquipoTI("Switch", new Vector3(0f, 1.00f, 0f), new Vector3(0.50f, 0.12f, 0.40f), matSwitch, rackRoot);
        GameObject cuboRouter = CrearCuboEquipoTI("Router", new Vector3(0f, 1.45f, 0f), new Vector3(0.46f, 0.12f, 0.36f), matRouter, rackRoot);

        // Configurar Problema 1: BASE DE DATOS ELIMINADA en el Servidor
        TextMeshPro tmpMonitorCaja = (monitoresPCs != null && monitoresPCs.Count > 0) ? monitoresPCs[0] : null;
        ConfigurarIncidenteBaseDatos(cuboServidor, panelOpciones, gm, tmpMonitorCaja);

        // Configurar Problema 2: SIN INTERNET en Switch y Router (ambos abren el mismo panel)
        IncidenteBaseDatos incBD = cuboServidor.GetComponent<IncidenteBaseDatos>();
        ConfigurarIncidenteInternet(cuboSwitch, cuboRouter, rackRoot, panelOpciones, gm, monitoresPCs, incBD);

        // Configurar Problema 3: IMPRESORA DESCONFIGURADA
        TextMeshPro tmpEtiquetaCaja = rootPuestos != null ? rootPuestos.Find("Puesto_Caja/Etiqueta_Caja")?.GetComponent<TextMeshPro>() : null;
        ConfigurarIncidenteImpresora(impresora, panelOpciones, gm, tmpPantallaImp, tmpEtiquetaCaja);
    }

    private static GameObject CrearCuboEquipoTI(string nombre, Vector3 posLocal, Vector3 escala, Material mat, Transform padre)
    {
        GameObject equipo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        equipo.name = $"Cubo_{nombre}";
        equipo.transform.SetParent(padre, false);
        equipo.transform.localPosition = posLocal;
        equipo.transform.localScale = escala;
        equipo.GetComponent<MeshRenderer>().sharedMaterial = mat;

        GameObject labelEquipo = new GameObject($"Label_{nombre}");
        labelEquipo.transform.SetParent(padre, false);
        labelEquipo.transform.localPosition = posLocal + new Vector3(0f, 0f, -(escala.z / 2f + 0.015f));
        labelEquipo.transform.localRotation = Quaternion.identity;
        TextMeshPro tmp = labelEquipo.AddComponent<TextMeshPro>();
        tmp.text = nombre.ToUpper();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 1.8f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.rectTransform.sizeDelta = new Vector2(escala.x, escala.y);

        return equipo;
    }

    private static void ConfigurarIncidenteBaseDatos(
        GameObject cuboServidor,
        PanelOpciones panelOpciones,
        GameManager gm,
        TextMeshPro tmpMonitorCaja)
    {
        if (cuboServidor == null) return;

        // 1. Agregar componente interactuable de XRI si no lo tiene
        XRSimpleInteractable simple = cuboServidor.GetComponent<XRSimpleInteractable>();
        if (simple == null)
        {
            simple = cuboServidor.AddComponent<XRSimpleInteractable>();
        }

        // 2. Agregar componente IncidenteInteractuable
        IncidenteInteractuable interactuable = cuboServidor.GetComponent<IncidenteInteractuable>();
        if (interactuable == null)
        {
            interactuable = cuboServidor.AddComponent<IncidenteInteractuable>();
        }
        interactuable.panelOpciones = panelOpciones;

        // 3. Agregar componente IncidenteBaseDatos (Problema 1: BASE DE DATOS ELIMINADA)
        IncidenteBaseDatos incBD = cuboServidor.GetComponent<IncidenteBaseDatos>();
        if (incBD == null)
        {
            incBD = cuboServidor.AddComponent<IncidenteBaseDatos>();
        }
        incBD.interactuable = interactuable;
        incBD.labelMonitorCaja = tmpMonitorCaja;
        incBD.GuardarYActivarSintomaMonitor();
        incBD.InicializarIncidente();

        // 4. Registrar en GameManager para que comience a drenar dinero a razón de S/ 5/s desde el inicio
        if (gm != null && incBD.incidente != null)
        {
            gm.RegistrarIncidente(incBD.incidente);
        }
    }

    private static void ConfigurarIncidenteInternet(
        GameObject cuboSwitch,
        GameObject cuboRouter,
        Transform rackRoot,
        PanelOpciones panelOpciones,
        GameManager gm,
        List<TextMeshPro> monitoresPCs,
        IncidenteBaseDatos incBD)
    {
        if (cuboSwitch == null || cuboRouter == null) return;

        // Materiales para la luz de estado y cable
        Material matLuzRoja = ObtenerOCrearMaterial("M_LuzRoja", new Color(0.95f, 0.15f, 0.15f));
        Material matLuzVerde = ObtenerOCrearMaterial("M_LuzVerde", new Color(0.15f, 0.85f, 0.25f));
        Material matCable = ObtenerOCrearMaterial("M_CableRed", new Color(0.20f, 0.55f, 0.95f));
        Material matConector = ObtenerOCrearMaterial("M_ConectorRJ45", new Color(0.85f, 0.85f, 0.85f));

        // 1. Luz indicadora de estado en el frontal del Switch (pequeña esfera)
        GameObject luz = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        luz.name = "Luz_Estado_Switch";
        luz.transform.SetParent(rackRoot, false);
        luz.transform.localPosition = new Vector3(-0.18f, 1.00f, -0.215f);
        luz.transform.localScale = new Vector3(0.035f, 0.035f, 0.035f);
        luz.GetComponent<MeshRenderer>().sharedMaterial = matLuzRoja;

        // 2. Puertos RJ45 visuales en Switch y Router
        GameObject portSwitch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portSwitch.name = "Puerto_Switch_RJ45";
        portSwitch.transform.SetParent(rackRoot, false);
        portSwitch.transform.localPosition = new Vector3(0.12f, 1.00f, -0.205f);
        portSwitch.transform.localScale = new Vector3(0.03f, 0.025f, 0.015f);
        portSwitch.GetComponent<MeshRenderer>().sharedMaterial = matConector;

        GameObject portRouter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portRouter.name = "Puerto_Router_RJ45";
        portRouter.transform.SetParent(rackRoot, false);
        portRouter.transform.localPosition = new Vector3(0.12f, 1.45f, -0.185f);
        portRouter.transform.localScale = new Vector3(0.03f, 0.025f, 0.015f);
        portRouter.GetComponent<MeshRenderer>().sharedMaterial = matConector;

        // 3. Objeto Cable Conectado (se activará al solucionar el problema)
        GameObject cableConectadoRoot = new GameObject("Cable_Switch_Router_Conectado");
        cableConectadoRoot.transform.SetParent(rackRoot, false);

        GameObject segCableCon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        segCableCon.name = "Cuerpo_Cable_Conectado";
        segCableCon.transform.SetParent(cableConectadoRoot.transform, false);
        segCableCon.transform.localPosition = new Vector3(0.12f, 1.225f, -0.21f);
        segCableCon.transform.localScale = new Vector3(0.018f, 0.22f, 0.018f);
        segCableCon.GetComponent<MeshRenderer>().sharedMaterial = matCable;

        GameObject headTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headTop.name = "Conector_RJ45_Router";
        headTop.transform.SetParent(cableConectadoRoot.transform, false);
        headTop.transform.localPosition = new Vector3(0.12f, 1.44f, -0.21f);
        headTop.transform.localScale = new Vector3(0.025f, 0.03f, 0.025f);
        headTop.GetComponent<MeshRenderer>().sharedMaterial = matConector;

        GameObject headBot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headBot.name = "Conector_RJ45_Switch";
        headBot.transform.SetParent(cableConectadoRoot.transform, false);
        headBot.transform.localPosition = new Vector3(0.12f, 1.01f, -0.21f);
        headBot.transform.localScale = new Vector3(0.025f, 0.03f, 0.025f);
        headBot.GetComponent<MeshRenderer>().sharedMaterial = matConector;

        cableConectadoRoot.SetActive(false);

        // 4. Objeto Cable Desconectado (visible desde el inicio con el cable colgando del switch)
        GameObject cableDesconectadoRoot = new GameObject("Cable_Switch_Router_Desconectado");
        cableDesconectadoRoot.transform.SetParent(rackRoot, false);

        GameObject headDescBot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headDescBot.name = "Conector_RJ45_Switch_Base";
        headDescBot.transform.SetParent(cableDesconectadoRoot.transform, false);
        headDescBot.transform.localPosition = new Vector3(0.12f, 1.01f, -0.21f);
        headDescBot.transform.localScale = new Vector3(0.025f, 0.03f, 0.025f);
        headDescBot.GetComponent<MeshRenderer>().sharedMaterial = matConector;

        GameObject segDesc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        segDesc.name = "Cuerpo_Cable_Colgante";
        segDesc.transform.SetParent(cableDesconectadoRoot.transform, false);
        segDesc.transform.localPosition = new Vector3(0.13f, 1.06f, -0.24f);
        segDesc.transform.localRotation = Quaternion.Euler(35f, 0f, 10f);
        segDesc.transform.localScale = new Vector3(0.018f, 0.08f, 0.018f);
        segDesc.GetComponent<MeshRenderer>().sharedMaterial = matCable;

        GameObject headDescTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headDescTop.name = "Conector_RJ45_Desconectado";
        headDescTop.transform.SetParent(cableDesconectadoRoot.transform, false);
        headDescTop.transform.localPosition = new Vector3(0.14f, 1.13f, -0.27f);
        headDescTop.transform.localRotation = Quaternion.Euler(35f, 0f, 10f);
        headDescTop.transform.localScale = new Vector3(0.025f, 0.03f, 0.025f);
        headDescTop.GetComponent<MeshRenderer>().sharedMaterial = matConector;

        cableDesconectadoRoot.SetActive(true);

        // 5. Configurar interactor en Switch
        XRSimpleInteractable simpleSwitch = cuboSwitch.GetComponent<XRSimpleInteractable>();
        if (simpleSwitch == null) simpleSwitch = cuboSwitch.AddComponent<XRSimpleInteractable>();
        IncidenteInteractuable interSwitch = cuboSwitch.GetComponent<IncidenteInteractuable>();
        if (interSwitch == null) interSwitch = cuboSwitch.AddComponent<IncidenteInteractuable>();
        interSwitch.panelOpciones = panelOpciones;

        // 6. Configurar interactor en Router (ambos cubos abren el mismo panel de incidente)
        XRSimpleInteractable simpleRouter = cuboRouter.GetComponent<XRSimpleInteractable>();
        if (simpleRouter == null) simpleRouter = cuboRouter.AddComponent<XRSimpleInteractable>();
        IncidenteInteractuable interRouter = cuboRouter.GetComponent<IncidenteInteractuable>();
        if (interRouter == null) interRouter = cuboRouter.AddComponent<IncidenteInteractuable>();
        interRouter.panelOpciones = panelOpciones;

        // 7. Componente IncidenteInternet en el Switch
        IncidenteInternet incNet = cuboSwitch.GetComponent<IncidenteInternet>();
        if (incNet == null) incNet = cuboSwitch.AddComponent<IncidenteInternet>();
        incNet.interactuableSwitch = interSwitch;
        incNet.interactuableRouter = interRouter;
        incNet.luzIndicador = luz;
        incNet.cableDesconectado = cableDesconectadoRoot;
        incNet.cableConectado = cableConectadoRoot;
        incNet.matLuzRoja = matLuzRoja;
        incNet.matLuzVerde = matLuzVerde;
        incNet.monitoresPCs = monitoresPCs;
        incNet.incidenteBaseDatos = incBD;
        incNet.InicializarIncidente();
        incNet.ActivarSintomaVisual();

        // 8. Registrar en GameManager para drenar S/ 3/s desde el inicio
        if (gm != null && incNet.incidente != null)
        {
            gm.RegistrarIncidente(incNet.incidente);
        }
    }

    private static void ConfigurarIncidenteImpresora(
        GameObject impresora,
        PanelOpciones panelOpciones,
        GameManager gm,
        TextMeshPro tmpPantalla,
        TextMeshPro tmpEtiquetaCaja)
    {
        if (impresora == null) return;

        // 1. Agregar interactor XRSimpleInteractable
        XRSimpleInteractable simple = impresora.GetComponent<XRSimpleInteractable>();
        if (simple == null)
        {
            simple = impresora.AddComponent<XRSimpleInteractable>();
        }

        // 2. Componente IncidenteInteractuable para abrir el panel flotante
        IncidenteInteractuable inter = impresora.GetComponent<IncidenteInteractuable>();
        if (inter == null)
        {
            inter = impresora.AddComponent<IncidenteInteractuable>();
        }
        inter.panelOpciones = panelOpciones;

        // 3. Componente IncidenteImpresora (Problema 3: IMPRESORA DESCONFIGURADA)
        IncidenteImpresora incImp = impresora.GetComponent<IncidenteImpresora>();
        if (incImp == null)
        {
            incImp = impresora.AddComponent<IncidenteImpresora>();
        }
        incImp.interactuable = inter;
        incImp.pantallaImpresora = tmpPantalla;
        incImp.etiquetaTrabajadorCaja = tmpEtiquetaCaja;
        incImp.InicializarIncidente();
        incImp.ActivarSintoma();

        // 4. Registrar en GameManager para drenar S/ 1/s desde el inicio
        if (gm != null && incImp.incidente != null)
        {
            gm.RegistrarIncidente(incImp.incidente);
        }
    }

    #endregion

    #region Interfaz de Usuario (World Space HUD y Panel de Opciones)

    private static void ConstruirCanvasHUD(Transform padre)
    {
        GameObject canvasGO = new GameObject("HUD_Canvas");
        canvasGO.transform.SetParent(padre);
        canvasGO.transform.position = new Vector3(0f, 2.62f, -2.42f);
        canvasGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        RectTransform rect = canvasGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(2200f, 600f);
        rect.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        // Fondo oscuro general
        GameObject bgGO = new GameObject("HUD_Fondo");
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.07f, 0.10f, 0.16f, 0.95f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Encabezado
        GameObject headerGO = new GameObject("HUD_Header");
        headerGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI tmpHeader = headerGO.AddComponent<TextMeshProUGUI>();
        tmpHeader.text = "<b>TI EN CRISIS - MONITOR DE OPERACIONES</b>";
        tmpHeader.fontSize = 32f;
        tmpHeader.alignment = TextAlignmentOptions.Center;
        tmpHeader.color = new Color(0.60f, 0.70f, 0.85f);
        RectTransform headerRect = headerGO.GetComponent<RectTransform>();
        headerRect.anchoredPosition = new Vector2(0f, 240f);
        headerRect.sizeDelta = new Vector2(2000f, 60f);

        // 3 Tarjetas de Estado (Dinero, Reputación, Tiempo)
        GameObject cardDinero = CrearTarjetaHUD("Card_Dinero", new Vector2(-650f, 130f), new Vector2(500f, 130f), canvasGO.transform);
        TextMeshProUGUI tmpDinero = cardDinero.GetComponentInChildren<TextMeshProUGUI>();
        tmpDinero.text = "DINERO\n<size=130%><color=#22C55E><b>S/ 5,000</b></color></size>";

        GameObject cardRep = CrearTarjetaHUD("Card_Reputacion", new Vector2(0f, 130f), new Vector2(500f, 130f), canvasGO.transform);
        TextMeshProUGUI tmpRep = cardRep.GetComponentInChildren<TextMeshProUGUI>();
        tmpRep.text = "REPUTACIÓN\n<size=130%><color=#38BDF8><b>70%</b></color></size>";

        GameObject cardTiempo = CrearTarjetaHUD("Card_Tiempo", new Vector2(650f, 130f), new Vector2(500f, 130f), canvasGO.transform);
        TextMeshProUGUI tmpTiempo = cardTiempo.GetComponentInChildren<TextMeshProUGUI>();
        tmpTiempo.text = "TIEMPO\n<size=130%><b>00:00</b></size>";

        // Caja de problemas activos
        GameObject boxProblemas = new GameObject("Box_Problemas");
        boxProblemas.transform.SetParent(canvasGO.transform, false);
        Image boxImg = boxProblemas.AddComponent<Image>();
        boxImg.color = new Color(0.12f, 0.16f, 0.24f, 0.90f);
        RectTransform boxRect = boxProblemas.GetComponent<RectTransform>();
        boxRect.anchoredPosition = new Vector2(0f, -120f);
        boxRect.sizeDelta = new Vector2(1850f, 260f);

        GameObject textListaGO = new GameObject("Texto_Problemas_Activos");
        textListaGO.transform.SetParent(boxProblemas.transform, false);
        TextMeshProUGUI tmpLista = textListaGO.AddComponent<TextMeshProUGUI>();
        tmpLista.text = "<color=#22C55E><b>[OK] SISTEMA ESTABLE</b></color>\nNo hay incidentes activos en este momento.";
        tmpLista.fontSize = 24f;
        tmpLista.alignment = TextAlignmentOptions.TopLeft;
        tmpLista.color = Color.white;
        RectTransform textListaRect = textListaGO.GetComponent<RectTransform>();
        textListaRect.anchorMin = Vector2.zero;
        textListaRect.anchorMax = Vector2.one;
        textListaRect.offsetMin = new Vector2(30f, 20f);
        textListaRect.offsetMax = new Vector2(-30f, -20f);

        // Componente controlador del HUD
        HUDPanel hud = canvasGO.AddComponent<HUDPanel>();
        hud.textoDinero = tmpDinero;
        hud.textoReputacion = tmpRep;
        hud.textoTiempo = tmpTiempo;
        hud.textoListaIncidentes = tmpLista;
    }

    private static GameObject CrearTarjetaHUD(string nombre, Vector2 pos, Vector2 tam, Transform padre)
    {
        GameObject card = new GameObject(nombre);
        card.transform.SetParent(padre, false);
        Image img = card.AddComponent<Image>();
        img.color = new Color(0.12f, 0.16f, 0.24f, 0.90f);
        RectTransform r = card.GetComponent<RectTransform>();
        r.anchoredPosition = pos;
        r.sizeDelta = tam;

        GameObject textGO = new GameObject("Texto");
        textGO.transform.SetParent(card.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 22f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        return card;
    }

    private static PanelOpciones ConstruirPanelOpciones(Transform padre)
    {
        GameObject canvasGO = new GameObject("Panel_Opciones");
        canvasGO.transform.SetParent(padre);
        canvasGO.transform.position = new Vector3(0f, 1.4f, -0.6f);
        canvasGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(700f, 580f);
        canvasRect.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        // Raíz visual del panel
        GameObject panelRaiz = new GameObject("Panel_Raiz");
        panelRaiz.transform.SetParent(canvasGO.transform, false);
        Image bgImg = panelRaiz.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.12f, 0.20f, 0.98f);
        RectTransform raizRect = panelRaiz.GetComponent<RectTransform>();
        raizRect.anchorMin = Vector2.zero;
        raizRect.anchorMax = Vector2.one;
        raizRect.offsetMin = Vector2.zero;
        raizRect.offsetMax = Vector2.zero;

        // Título del incidente
        GameObject tituloGO = new GameObject("Texto_Titulo");
        tituloGO.transform.SetParent(panelRaiz.transform, false);
        TextMeshProUGUI tmpTitulo = tituloGO.AddComponent<TextMeshProUGUI>();
        tmpTitulo.text = "INCIDENTE";
        tmpTitulo.fontSize = 24f;
        tmpTitulo.fontStyle = FontStyles.Bold;
        tmpTitulo.alignment = TextAlignmentOptions.Center;
        tmpTitulo.color = Color.white;
        RectTransform tituloRect = tituloGO.GetComponent<RectTransform>();
        tituloRect.anchoredPosition = new Vector2(0f, 245f);
        tituloRect.sizeDelta = new Vector2(620f, 40f);

        // Pérdida actual
        GameObject perdidaGO = new GameObject("Texto_Perdida");
        perdidaGO.transform.SetParent(panelRaiz.transform, false);
        TextMeshProUGUI tmpPerdida = perdidaGO.AddComponent<TextMeshProUGUI>();
        tmpPerdida.text = "Pérdida: -S/ 0 por segundo";
        tmpPerdida.fontSize = 18f;
        tmpPerdida.alignment = TextAlignmentOptions.Center;
        tmpPerdida.color = new Color(0.95f, 0.35f, 0.35f);
        RectTransform perdidaRect = perdidaGO.GetComponent<RectTransform>();
        perdidaRect.anchoredPosition = new Vector2(0f, 210f);
        perdidaRect.sizeDelta = new Vector2(620f, 30f);

        // Detalle contextual / hora del incidente
        GameObject detalleGO = new GameObject("Texto_Detalle");
        detalleGO.transform.SetParent(panelRaiz.transform, false);
        TextMeshProUGUI tmpDetalle = detalleGO.AddComponent<TextMeshProUGUI>();
        tmpDetalle.text = "";
        tmpDetalle.fontSize = 16f;
        tmpDetalle.alignment = TextAlignmentOptions.Center;
        tmpDetalle.color = new Color(0.98f, 0.85f, 0.35f);
        RectTransform detalleRect = detalleGO.GetComponent<RectTransform>();
        detalleRect.anchoredPosition = new Vector2(0f, 165f);
        detalleRect.sizeDelta = new Vector2(620f, 40f);

        // Botón Cerrar
        GameObject btnCerrarGO = new GameObject("Boton_Cerrar");
        btnCerrarGO.transform.SetParent(panelRaiz.transform, false);
        Image imgCerrar = btnCerrarGO.AddComponent<Image>();
        imgCerrar.color = new Color(0.25f, 0.30f, 0.40f);
        Button btnCerrar = btnCerrarGO.AddComponent<Button>();
        RectTransform rCerrar = btnCerrarGO.GetComponent<RectTransform>();
        rCerrar.anchoredPosition = new Vector2(305f, 245f);
        rCerrar.sizeDelta = new Vector2(36f, 36f);

        GameObject textoCerrar = new GameObject("Texto_X");
        textoCerrar.transform.SetParent(btnCerrarGO.transform, false);
        TextMeshProUGUI tmpX = textoCerrar.AddComponent<TextMeshProUGUI>();
        tmpX.text = "X";
        tmpX.fontSize = 20f;
        tmpX.fontStyle = FontStyles.Bold;
        tmpX.alignment = TextAlignmentOptions.Center;
        tmpX.color = Color.white;
        RectTransform rX = textoCerrar.GetComponent<RectTransform>();
        rX.anchorMin = Vector2.zero;
        rX.anchorMax = Vector2.one;
        rX.offsetMin = Vector2.zero;
        rX.offsetMax = Vector2.zero;

        // Contenedor de Botones de Opciones
        GameObject contBotonesGO = new GameObject("Contenedor_Botones");
        contBotonesGO.transform.SetParent(panelRaiz.transform, false);
        RectTransform rContBotones = contBotonesGO.AddComponent<RectTransform>();
        rContBotones.anchoredPosition = new Vector2(0f, 5f);
        rContBotones.sizeDelta = new Vector2(620f, 255f);
        VerticalLayoutGroup vlg = contBotonesGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Plantilla de Botón (template)
        GameObject templateBoton = new GameObject("Template_Boton_Opcion");
        templateBoton.transform.SetParent(contBotonesGO.transform, false);
        Image imgBoton = templateBoton.AddComponent<Image>();
        imgBoton.color = new Color(0.18f, 0.24f, 0.35f);
        Button btnBoton = templateBoton.AddComponent<Button>();
        ColorBlock cb = btnBoton.colors;
        cb.highlightedColor = new Color(0.28f, 0.38f, 0.55f);
        cb.pressedColor = new Color(0.12f, 0.16f, 0.24f);
        btnBoton.colors = cb;
        LayoutElement le = templateBoton.AddComponent<LayoutElement>();
        le.minHeight = 55f;
        le.preferredHeight = 58f;

        GameObject textBtnGO = new GameObject("Texto");
        textBtnGO.transform.SetParent(templateBoton.transform, false);
        TextMeshProUGUI tmpBtn = textBtnGO.AddComponent<TextMeshProUGUI>();
        tmpBtn.fontSize = 16f;
        tmpBtn.alignment = TextAlignmentOptions.Center;
        tmpBtn.color = Color.white;
        RectTransform textBtnRect = textBtnGO.GetComponent<RectTransform>();
        textBtnRect.anchorMin = Vector2.zero;
        textBtnRect.anchorMax = Vector2.one;
        textBtnRect.offsetMin = new Vector2(10f, 4f);
        textBtnRect.offsetMax = new Vector2(-10f, -4f);

        // Contenedor de Barra de Progreso
        GameObject contProgreso = new GameObject("Contenedor_Progreso");
        contProgreso.transform.SetParent(panelRaiz.transform, false);
        RectTransform rProg = contProgreso.AddComponent<RectTransform>();
        rProg.anchoredPosition = new Vector2(0f, -155f);
        rProg.sizeDelta = new Vector2(620f, 50f);

        GameObject barraFondo = new GameObject("Barra_Fondo");
        barraFondo.transform.SetParent(contProgreso.transform, false);
        Image imgFondoBarra = barraFondo.AddComponent<Image>();
        imgFondoBarra.color = new Color(0.12f, 0.16f, 0.24f);
        RectTransform rFondoBarra = barraFondo.GetComponent<RectTransform>();
        rFondoBarra.anchoredPosition = new Vector2(0f, -10f);
        rFondoBarra.sizeDelta = new Vector2(600f, 20f);

        GameObject barraRelleno = new GameObject("Barra_Relleno");
        barraRelleno.transform.SetParent(barraFondo.transform, false);
        Image imgRelleno = barraRelleno.AddComponent<Image>();
        imgRelleno.color = new Color(0.22f, 0.74f, 0.97f);
        RectTransform rRelleno = barraRelleno.GetComponent<RectTransform>();
        rRelleno.anchorMin = new Vector2(0f, 0f);
        rRelleno.anchorMax = new Vector2(0f, 1f);
        rRelleno.offsetMin = Vector2.zero;
        rRelleno.offsetMax = Vector2.zero;

        GameObject textProgGO = new GameObject("Texto_Progreso");
        textProgGO.transform.SetParent(contProgreso.transform, false);
        TextMeshProUGUI tmpProg = textProgGO.AddComponent<TextMeshProUGUI>();
        tmpProg.text = "Aplicando solución...";
        tmpProg.fontSize = 16f;
        tmpProg.alignment = TextAlignmentOptions.Center;
        tmpProg.color = Color.white;
        RectTransform rTextProg = textProgGO.GetComponent<RectTransform>();
        rTextProg.anchoredPosition = new Vector2(0f, 14f);
        rTextProg.sizeDelta = new Vector2(600f, 22f);

        // Texto de Resultado final
        GameObject resGO = new GameObject("Texto_Resultado");
        resGO.transform.SetParent(panelRaiz.transform, false);
        TextMeshProUGUI tmpRes = resGO.AddComponent<TextMeshProUGUI>();
        tmpRes.text = "";
        tmpRes.fontSize = 16f;
        tmpRes.alignment = TextAlignmentOptions.Center;
        tmpRes.color = Color.white;
        RectTransform rRes = resGO.GetComponent<RectTransform>();
        rRes.anchoredPosition = new Vector2(0f, -225f);
        rRes.sizeDelta = new Vector2(620f, 45f);

        // Componente controlador del Panel de Opciones
        PanelOpciones panelComp = canvasGO.AddComponent<PanelOpciones>();
        panelComp.panelRaiz = panelRaiz;
        panelComp.textoTitulo = tmpTitulo;
        panelComp.textoPerdida = tmpPerdida;
        panelComp.textoDetalle = tmpDetalle;
        panelComp.contenedorBotones = contBotonesGO.transform;
        panelComp.templateBoton = templateBoton;
        panelComp.contenedorProgreso = contProgreso;
        panelComp.barraProgresoRelleno = rRelleno;
        panelComp.textoProgreso = tmpProg;
        panelComp.textoResultado = tmpRes;
        panelComp.botonCerrar = btnCerrar;

        panelRaiz.SetActive(false);

        return panelComp;
    }

    private static InformeFinal ConstruirCanvasInformeFinal(Transform padre)
    {
        GameObject canvasGO = new GameObject("Canvas_InformeFinal");
        canvasGO.transform.SetParent(padre);
        canvasGO.transform.position = new Vector3(0f, 1.45f, -1.2f);
        canvasGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(960f, 780f);
        canvasRect.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        // Raíz visual del informe
        GameObject panelRaiz = new GameObject("Panel_Informe_Raiz");
        panelRaiz.transform.SetParent(canvasGO.transform, false);
        Image bgImg = panelRaiz.AddComponent<Image>();
        bgImg.color = new Color(0.06f, 0.08f, 0.14f, 0.98f);
        RectTransform raizRect = panelRaiz.GetComponent<RectTransform>();
        raizRect.anchorMin = Vector2.zero;
        raizRect.anchorMax = Vector2.one;
        raizRect.offsetMin = Vector2.zero;
        raizRect.offsetMax = Vector2.zero;

        // Título del desenlace
        GameObject tituloGO = new GameObject("Texto_TituloFinal");
        tituloGO.transform.SetParent(panelRaiz.transform, false);
        TextMeshProUGUI tmpTitulo = tituloGO.AddComponent<TextMeshProUGUI>();
        tmpTitulo.text = "<color=#22C55E><b>¡NEGOCIO SALVADO!</b></color>";
        tmpTitulo.fontSize = 32f;
        tmpTitulo.fontStyle = FontStyles.Bold;
        tmpTitulo.alignment = TextAlignmentOptions.Center;
        tmpTitulo.color = Color.white;
        RectTransform tituloRect = tituloGO.GetComponent<RectTransform>();
        tituloRect.anchoredPosition = new Vector2(0f, 335f);
        tituloRect.sizeDelta = new Vector2(900f, 50f);

        // Subtítulo del desenlace
        GameObject subtituloGO = new GameObject("Texto_SubtituloFinal");
        subtituloGO.transform.SetParent(panelRaiz.transform, false);
        TextMeshProUGUI tmpSubtitulo = subtituloGO.AddComponent<TextMeshProUGUI>();
        tmpSubtitulo.text = "Resumen del desempeño operativo del turno";
        tmpSubtitulo.fontSize = 16f;
        tmpSubtitulo.alignment = TextAlignmentOptions.Center;
        tmpSubtitulo.color = new Color(0.70f, 0.78f, 0.90f);
        RectTransform subtituloRect = subtituloGO.GetComponent<RectTransform>();
        subtituloRect.anchoredPosition = new Vector2(0f, 290f);
        subtituloRect.sizeDelta = new Vector2(900f, 36f);

        // Caja de Métricas Generales
        GameObject boxMetricas = new GameObject("Box_Metricas");
        boxMetricas.transform.SetParent(panelRaiz.transform, false);
        Image imgMetricas = boxMetricas.AddComponent<Image>();
        imgMetricas.color = new Color(0.12f, 0.16f, 0.25f, 0.95f);
        RectTransform rMetricas = boxMetricas.GetComponent<RectTransform>();
        rMetricas.anchoredPosition = new Vector2(0f, 230f);
        rMetricas.sizeDelta = new Vector2(900f, 60f);

        GameObject textoMetricasGO = new GameObject("Texto_Metricas");
        textoMetricasGO.transform.SetParent(boxMetricas.transform, false);
        TextMeshProUGUI tmpMetricas = textoMetricasGO.AddComponent<TextMeshProUGUI>();
        tmpMetricas.text = "<b>Dinero Restante:</b> <color=#22C55E>S/ 5,000</color>  |  <b>Pérdida Total:</b> <color=#EF4444>-S/ 0</color>  |  <b>Reputación:</b> 70%  |  <b>Tiempo:</b> 00:00";
        tmpMetricas.fontSize = 16f;
        tmpMetricas.alignment = TextAlignmentOptions.Center;
        tmpMetricas.color = Color.white;
        RectTransform rTextMetricas = textoMetricasGO.GetComponent<RectTransform>();
        rTextMetricas.anchorMin = Vector2.zero;
        rTextMetricas.anchorMax = Vector2.one;
        rTextMetricas.offsetMin = new Vector2(10f, 4f);
        rTextMetricas.offsetMax = new Vector2(-10f, -4f);

        // Caja de Historial Cronológico de Decisiones
        GameObject boxHistorial = new GameObject("Box_Historial");
        boxHistorial.transform.SetParent(panelRaiz.transform, false);
        Image imgHistorial = boxHistorial.AddComponent<Image>();
        imgHistorial.color = new Color(0.09f, 0.12f, 0.18f, 0.95f);
        RectTransform rHistorial = boxHistorial.GetComponent<RectTransform>();
        rHistorial.anchoredPosition = new Vector2(0f, 75f);
        rHistorial.sizeDelta = new Vector2(900f, 210f);

        GameObject textoHistorialGO = new GameObject("Texto_Historial");
        textoHistorialGO.transform.SetParent(boxHistorial.transform, false);
        TextMeshProUGUI tmpHistorial = textoHistorialGO.AddComponent<TextMeshProUGUI>();
        tmpHistorial.text = "<b>ORDEN DE ATENCIÓN Y DECISIONES TOMADAS:</b>\n<color=#94A3B8>Registrando decisiones...</color>";
        tmpHistorial.fontSize = 14f;
        tmpHistorial.alignment = TextAlignmentOptions.TopLeft;
        tmpHistorial.color = Color.white;
        tmpHistorial.textWrappingMode = TextWrappingModes.Normal;
        RectTransform rTextHistorial = textoHistorialGO.GetComponent<RectTransform>();
        rTextHistorial.anchorMin = Vector2.zero;
        rTextHistorial.anchorMax = Vector2.one;
        rTextHistorial.offsetMin = new Vector2(16f, 10f);
        rTextHistorial.offsetMax = new Vector2(-16f, -10f);

        // Caja de Recomendaciones Técnicas
        GameObject boxRecomendaciones = new GameObject("Box_Recomendaciones");
        boxRecomendaciones.transform.SetParent(panelRaiz.transform, false);
        Image imgRec = boxRecomendaciones.AddComponent<Image>();
        imgRec.color = new Color(0.09f, 0.12f, 0.18f, 0.95f);
        RectTransform rRec = boxRecomendaciones.GetComponent<RectTransform>();
        rRec.anchoredPosition = new Vector2(0f, -145f);
        rRec.sizeDelta = new Vector2(900f, 190f);

        GameObject textoRecGO = new GameObject("Texto_Recomendaciones");
        textoRecGO.transform.SetParent(boxRecomendaciones.transform, false);
        TextMeshProUGUI tmpRec = textoRecGO.AddComponent<TextMeshProUGUI>();
        tmpRec.text = "<b>RECOMENDACIONES DE MEJORA TÉCNICA:</b>\n<color=#94A3B8>Registrando recomendaciones...</color>";
        tmpRec.fontSize = 14f;
        tmpRec.alignment = TextAlignmentOptions.TopLeft;
        tmpRec.color = Color.white;
        tmpRec.textWrappingMode = TextWrappingModes.Normal;
        RectTransform rTextRec = textoRecGO.GetComponent<RectTransform>();
        rTextRec.anchorMin = Vector2.zero;
        rTextRec.anchorMax = Vector2.one;
        rTextRec.offsetMin = new Vector2(16f, 10f);
        rTextRec.offsetMax = new Vector2(-16f, -10f);

        // Botón Reintentar
        GameObject btnReintentarGO = new GameObject("Boton_Reintentar");
        btnReintentarGO.transform.SetParent(panelRaiz.transform, false);
        Image imgBtn = btnReintentarGO.AddComponent<Image>();
        imgBtn.color = new Color(0.15f, 0.48f, 0.85f);
        Button btnReintentar = btnReintentarGO.AddComponent<Button>();
        ColorBlock cb = btnReintentar.colors;
        cb.highlightedColor = new Color(0.25f, 0.60f, 1.0f);
        cb.pressedColor = new Color(0.10f, 0.35f, 0.65f);
        btnReintentar.colors = cb;
        RectTransform rBtn = btnReintentarGO.GetComponent<RectTransform>();
        rBtn.anchoredPosition = new Vector2(0f, -325f);
        rBtn.sizeDelta = new Vector2(300f, 50f);

        GameObject textoBtnGO = new GameObject("Texto_Boton");
        textoBtnGO.transform.SetParent(btnReintentarGO.transform, false);
        TextMeshProUGUI tmpBtn = textoBtnGO.AddComponent<TextMeshProUGUI>();
        tmpBtn.text = "<b>REINTENTAR TURNO</b>";
        tmpBtn.fontSize = 18f;
        tmpBtn.fontStyle = FontStyles.Bold;
        tmpBtn.alignment = TextAlignmentOptions.Center;
        tmpBtn.color = Color.white;
        RectTransform rTextBtn = textoBtnGO.GetComponent<RectTransform>();
        rTextBtn.anchorMin = Vector2.zero;
        rTextBtn.anchorMax = Vector2.one;
        rTextBtn.offsetMin = Vector2.zero;
        rTextBtn.offsetMax = Vector2.zero;

        // Asignar componente controlador InformeFinal
        InformeFinal informe = canvasGO.AddComponent<InformeFinal>();
        informe.panelRaiz = panelRaiz;
        informe.textoTituloFinal = tmpTitulo;
        informe.textoSubtituloFinal = tmpSubtitulo;
        informe.textoMetricas = tmpMetricas;
        informe.textoHistorialDecisiones = tmpHistorial;
        informe.textoRecomendaciones = tmpRec;
        informe.botonReintentar = btnReintentar;

        panelRaiz.SetActive(false);

        return informe;
    }

    private static void ConstruirFilaClientes(Transform padre, Material matNormal, Material matRojo, Material matMolesto)
    {
        GameObject filaGO = new GameObject("Gestor_Fila_Clientes");
        filaGO.transform.SetParent(padre);
        filaGO.transform.position = Vector3.zero;

        ClientesFila fila = filaGO.AddComponent<ClientesFila>();
        fila.posicionInicial = new Vector3(0f, 0.85f, -3.1f);
        fila.distanciaEntreClientes = 0.70f;
        fila.maxClientes = 7;
        fila.matNormal = matNormal;
        fila.matRojoEnojado = matRojo;
        fila.matMolestoPermanente = matMolesto;
    }

    #endregion

    #region Sistema XR y Simulador

    private static void ConfigurarXR(Transform padre)
    {
        // 1. XR Interaction Manager en la escena
        GameObject xrimGO = new GameObject("XR Interaction Manager");
        xrimGO.transform.SetParent(padre);
        xrimGO.AddComponent<XRInteractionManager>();

        // 2. EventSystem compatible con UI XR y nuevo Input System
        GameObject eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.transform.SetParent(padre);
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<XRUIInputModule>();

        // 3. XR Origin (XR Rig) con Near/Far interactors (Ray Interactor)
        GameObject xrOriginPrefab = CargarPrefab("XR Origin (XR Rig)");
        if (xrOriginPrefab != null)
        {
            GameObject rigInstance = (GameObject)PrefabUtility.InstantiatePrefab(xrOriginPrefab);
            rigInstance.name = "XR Origin (XR Rig)";
            rigInstance.transform.SetParent(padre);
            rigInstance.transform.position = new Vector3(0f, 0f, -0.6f);
            rigInstance.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            XROrigin origin = rigInstance.GetComponent<XROrigin>();
            if (origin != null)
            {
                origin.CameraYOffset = 1.36f;
            }
            Debug.Log("[TI en Crisis] XR Origin instanciado desde prefab con éxito.");
        }
        else
        {
            CrearXROriginProcedural(padre);
        }

        // 4. XR Device Simulator
        GameObject simPrefab = CargarPrefab("XR Device Simulator");
        if (simPrefab != null)
        {
            GameObject simInstance = (GameObject)PrefabUtility.InstantiatePrefab(simPrefab);
            simInstance.name = "XR Device Simulator";
            simInstance.transform.SetParent(padre);
            simInstance.AddComponent<AutoElevadorMando>();
            Debug.Log("[TI en Crisis] XR Device Simulator instanciado en la escena con éxito.");
        }
        else
        {
            Debug.LogWarning("[TI en Crisis] No se encontró el prefab 'XR Device Simulator'.");
        }
    }

    private static void CrearXROriginProcedural(Transform padre)
    {
        GameObject originGO = new GameObject("XR Origin");
        originGO.transform.SetParent(padre);
        originGO.transform.position = new Vector3(0f, 0f, -0.6f);
        originGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        XROrigin origin = originGO.AddComponent<XROrigin>();

        GameObject cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.SetParent(originGO.transform, false);
        origin.CameraFloorOffsetObject = cameraOffset;

        GameObject mainCam = new GameObject("Main Camera");
        mainCam.transform.SetParent(cameraOffset.transform, false);
        mainCam.tag = "MainCamera";
        Camera cam = mainCam.AddComponent<Camera>();
        cam.nearClipPlane = 0.05f;
        mainCam.AddComponent<AudioListener>();
        mainCam.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
        origin.Camera = cam;

        GameObject leftController = new GameObject("Left Controller");
        leftController.transform.SetParent(cameraOffset.transform, false);
        leftController.AddComponent<XRRayInteractor>();

        GameObject rightController = new GameObject("Right Controller");
        rightController.transform.SetParent(cameraOffset.transform, false);
        rightController.AddComponent<XRRayInteractor>();

        Debug.Log("[TI en Crisis] XR Origin procedural creado como alternativa.");
    }

    private static GameObject CargarPrefab(string nombrePrefab)
    {
        string[] guids = AssetDatabase.FindAssets($"{nombrePrefab} t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path).Equals(nombrePrefab, System.StringComparison.OrdinalIgnoreCase))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                    return prefab;
            }
        }
        return null;
    }

    #endregion

    #region Utilidades de Materiales y Archivos

    private static void AsegurarDirectorios()
    {
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
        }
        if (!Directory.Exists(CARPETA_MATERIALES))
        {
            Directory.CreateDirectory(CARPETA_MATERIALES);
        }
    }

    private static Material ObtenerOCrearMaterial(string nombre, Color color)
    {
        string assetPath = $"{CARPETA_MATERIALES}/{nombre}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Standard");

            mat = new Material(shader);
            mat.name = nombre;

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            mat.color = color;

            AssetDatabase.CreateAsset(mat, assetPath);
        }
        else
        {
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            mat.color = color;
            EditorUtility.SetDirty(mat);
        }

        return mat;
    }

    #endregion
}
