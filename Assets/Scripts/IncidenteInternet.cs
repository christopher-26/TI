using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Gestiona el Problema 2: SIN INTERNET.
/// - Objetos interactuables: Cubos Switch y Router en el gabinete de TI (ambos abren este panel).
/// - Pérdida: S/ 3 por segundo desde el inicio de la jornada.
/// - Síntoma: Los 3 monitores de las PCs muestran "SIN CONEXION".
/// - Indicador luminoso: Esfera en el Switch en color rojo/apagado.
/// - Causa real: El cable switch-router está desconectado físicamente en el rack.
/// - Opciones de solución:
///   1. "Reconectar cable switch-router" -> Correcta: Resuelve, reputación +5, cable se conecta y luz pasa a verde.
///   2. "Reiniciar el router" -> Regular: NO resuelve, penalización de 30 s.
///   3. "Llamar al proveedor" -> Mala: NO resuelve, penalización de 45 s, reputación -5.
/// - Al resolverse, los monitores vuelven a su estado operativo normal.
/// </summary>
public class IncidenteInternet : MonoBehaviour
{
    [Header("Interactables (Switch y Router)")]
    public IncidenteInteractuable interactuableSwitch;
    public IncidenteInteractuable interactuableRouter;

    [Header("Elementos Visuales del Rack")]
    public GameObject luzIndicador;
    public GameObject cableDesconectado;
    public GameObject cableConectado;

    [Header("Materiales de la Luz")]
    public Material matLuzRoja;
    public Material matLuzVerde;

    [Header("Monitores de Puestos")]
    public List<TextMeshPro> monitoresPCs = new List<TextMeshPro>();

    [Header("Datos del Incidente")]
    public Incidente incidente;

    [Header("Coordinación")]
    public IncidenteBaseDatos incidenteBaseDatos;

    private Dictionary<TextMeshPro, string> textosOriginales = new Dictionary<TextMeshPro, string>();
    private bool sintomasActivados = false;

    private void Awake()
    {
        BuscarReferenciasSiFaltan();
        InicializarIncidente();
        ActivarSintomaVisual();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnIncidenteResuelto += OnIncidenteResuelto;

            // Registrar el incidente en el gestor para drenar S/ 3/s desde el inicio
            if (incidente != null && !GameManager.Instance.incidentesActivos.Contains(incidente))
            {
                GameManager.Instance.RegistrarIncidente(incidente);
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnIncidenteResuelto -= OnIncidenteResuelto;
        }
    }

    /// <summary>
    /// Configura el estado inicial con el cable desconectado, luz roja y monitores en "SIN CONEXION".
    /// </summary>
    public void ActivarSintomaVisual()
    {
        if (sintomasActivados) return;
        sintomasActivados = true;

        // 1. Estado visual del cable en el rack
        if (cableDesconectado != null) cableDesconectado.SetActive(true);
        if (cableConectado != null) cableConectado.SetActive(false);

        // 2. Luz indicadora en rojo
        if (luzIndicador != null)
        {
            MeshRenderer mr = luzIndicador.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                if (matLuzRoja != null) mr.sharedMaterial = matLuzRoja;
                else mr.material.color = new Color(0.9f, 0.15f, 0.15f);
            }
        }

        // 3. Monitores de PC en "SIN CONEXION"
        for (int i = 0; i < monitoresPCs.Count; i++)
        {
            TextMeshPro tmp = monitoresPCs[i];
            if (tmp != null)
            {
                if (!textosOriginales.ContainsKey(tmp))
                {
                    textosOriginales[tmp] = tmp.text;
                }

                string nombrePuesto = tmp.transform.parent != null && tmp.transform.parent.parent != null
                    ? tmp.transform.parent.parent.name.Replace("Puesto_", "").ToUpper()
                    : "TERMINAL";

                if (nombrePuesto.Contains("CAJA") && incidenteBaseDatos != null && incidenteBaseDatos.incidente != null && incidenteBaseDatos.incidente.activo)
                {
                    // Si Caja también tiene el fallo de BD, muestra ambos síntomas
                    tmp.text = "<b>CAJA</b>\n<size=65%><color=#EF4444>SIN CONEXION\n(SIN REGISTROS)</color></size>";
                }
                else
                {
                    tmp.text = $"<b>{nombrePuesto}</b>\n<size=70%><color=#EF4444>SIN CONEXION</color></size>";
                }
            }
        }
    }

    /// <summary>
    /// Restablece los monitores a su estado normal (o mantiene el síntoma de BD en Caja si aún no se resuelve).
    /// </summary>
    public void RestaurarMonitores()
    {
        for (int i = 0; i < monitoresPCs.Count; i++)
        {
            TextMeshPro tmp = monitoresPCs[i];
            if (tmp != null)
            {
                string nombrePuesto = tmp.transform.parent != null && tmp.transform.parent.parent != null
                    ? tmp.transform.parent.parent.name.Replace("Puesto_", "").ToUpper()
                    : "TERMINAL";

                if (nombrePuesto.Contains("CAJA") && incidenteBaseDatos != null && incidenteBaseDatos.incidente != null && incidenteBaseDatos.incidente.activo)
                {
                    // Caja sigue con el fallo de base de datos
                    tmp.text = "<b>CAJA</b>\n<size=75%><color=#EF4444>SIN REGISTROS</color></size>";
                }
                else
                {
                    // Restaurar terminal conectada
                    if (textosOriginales.TryGetValue(tmp, out string orig) && !string.IsNullOrEmpty(orig))
                    {
                        tmp.text = orig;
                    }
                    else
                    {
                        tmp.text = $"<b>{nombrePuesto}</b>\n<size=70%><color=#38BDF8>Terminal Conectada</color></size>";
                    }
                }
            }
        }
    }

    private void OnIncidenteResuelto(Incidente inc)
    {
        if (inc == this.incidente)
        {
            // 1. Conectar cable visualmente
            if (cableDesconectado != null) cableDesconectado.SetActive(false);
            if (cableConectado != null) cableConectado.SetActive(true);

            // 2. Luz indicadora del switch a verde
            if (luzIndicador != null)
            {
                MeshRenderer mr = luzIndicador.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    if (matLuzVerde != null) mr.sharedMaterial = matLuzVerde;
                    else mr.material.color = new Color(0.15f, 0.85f, 0.25f);
                }
            }

            // 3. Monitores operativos
            RestaurarMonitores();
            Debug.Log("<color=green>[Red Restaurada]</color> Cable conectado y enlace de red restablecido.");
        }
    }

    /// <summary>
    /// Construye y asigna los datos y opciones del problema de Red/Internet.
    /// </summary>
    public void InicializarIncidente()
    {
        if (incidente != null && incidente.opciones != null && incidente.opciones.Count > 0)
        {
            VincularInteractables();
            return;
        }

        incidente = new Incidente(
            "Sin Conexión a Internet",
            3f, // Pérdida de 3 soles por segundo
            new List<OpcionSolucion>
            {
                new OpcionSolucion(
                    "Reconectar cable switch-router",
                    TipoResultado.Correcta,
                    2.0f,
                    +5,
                    true,
                    0f,
                    "¡Correcto! En problemas de red siempre se debe revisar primero la capa física (cables y luces de estado). Reconectar el cable restauró la red de inmediato."
                ),
                new OpcionSolucion(
                    "Reiniciar el router",
                    TipoResultado.Regular,
                    30.0f, // 30 s de penalización
                    0,
                    false, // NO resuelve
                    0f,
                    "Reiniciar el router no soluciona un corte físico de cable. Perdiste 30 segundos mientras los equipos continuaban sin red."
                ),
                new OpcionSolucion(
                    "Llamar al proveedor",
                    TipoResultado.Mala,
                    45.0f, // 45 s de penalización
                    -5,    // -5 de reputación
                    false, // NO resuelve
                    0f,
                    "Llamar al proveedor fue una pérdida de tiempo y reputación: el problema era local (cable desconectado), no de la empresa de telecomunicaciones."
                )
            },
            "<b>Diagnóstico de Red:</b> Sin salida a Internet en todos los terminales.\nRevise las luces e inspeccione el cableado físico del rack:"
        );

        VincularInteractables();
    }

    private void VincularInteractables()
    {
        if (interactuableSwitch != null)
        {
            interactuableSwitch.incidente = incidente;
        }
        if (interactuableRouter != null)
        {
            interactuableRouter.incidente = incidente;
        }
    }

    private void BuscarReferenciasSiFaltan()
    {
        if (incidenteBaseDatos == null)
        {
            incidenteBaseDatos = Object.FindFirstObjectByType<IncidenteBaseDatos>();
        }

        if (monitoresPCs == null || monitoresPCs.Count == 0)
        {
            string[] puestos = new string[] { "Puesto_Caja", "Puesto_Pedidos online", "Puesto_Administracion" };
            for (int i = 0; i < puestos.Length; i++)
            {
                GameObject go = GameObject.Find(puestos[i]);
                if (go != null)
                {
                    Transform t = go.transform.Find("Monitor/Label_Monitor_TMP");
                    if (t != null)
                    {
                        TextMeshPro tmp = t.GetComponent<TextMeshPro>();
                        if (tmp != null && !monitoresPCs.Contains(tmp))
                        {
                            monitoresPCs.Add(tmp);
                        }
                    }
                }
            }
        }

        if (interactuableSwitch == null)
        {
            GameObject switchGO = GameObject.Find("Cubo_Switch");
            if (switchGO != null) interactuableSwitch = switchGO.GetComponent<IncidenteInteractuable>();
        }

        if (interactuableRouter == null)
        {
            GameObject routerGO = GameObject.Find("Cubo_Router");
            if (routerGO != null) interactuableRouter = routerGO.GetComponent<IncidenteInteractuable>();
        }
    }
}
