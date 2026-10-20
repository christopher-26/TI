using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Gestiona el Problema 1: BASE DE DATOS ELIMINADA.
/// - Objeto interactuable: Cubo Servidor en el gabinete de TI.
/// - Pérdida: S/ 5 por segundo desde el inicio de la jornada.
/// - Síntoma: El monitor de la PC "Caja" muestra "SIN REGISTROS" en rojo.
/// - Hora del incidente: 10:30 a.m. (visible en el panel de opciones).
/// - Opciones de restauración con backup:
///   1. "Hoy 6:00 a.m." -> Correcta: Resuelve, Reputación +10.
///   2. "Hoy 11:00 a.m." -> Mala (trampa posterior al borrado y vacía): No resuelve, penalización 20 s.
///   3. "Ayer 11:00 p.m." -> Regular: Resuelve pero faltan órdenes de hoy, Reputación -5.
///   4. "Hace una semana" -> Mala: Resuelve pero se pierde una semana, Reputación -20, clientesMolestos = true en GameManager.
/// - Al resolverse, el monitor de Caja vuelve a su estado normal.
/// </summary>
public class IncidenteBaseDatos : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Componente interactuable colocado en el Servidor.")]
    public IncidenteInteractuable interactuable;

    [Tooltip("Label TMP del monitor de la PC de Caja para reflejar el síntoma.")]
    public TextMeshPro labelMonitorCaja;

    [Header("Datos del Incidente")]
    public Incidente incidente;

    // Guarda el texto y color original del monitor para restaurarlo al solucionar el incidente
    private string textoOriginalMonitor = "";
    private Color colorOriginalMonitor = Color.white;
    private bool textoGuardado = false;

    private void Awake()
    {
        if (interactuable == null)
        {
            interactuable = GetComponent<IncidenteInteractuable>();
        }

        // Si no se asignó en el editor, buscar el monitor de Caja por jerarquía
        if (labelMonitorCaja == null)
        {
            GameObject puestoCaja = GameObject.Find("Puesto_Caja");
            if (puestoCaja != null)
            {
                Transform monitor = puestoCaja.transform.Find("Monitor/Label_Monitor_TMP");
                if (monitor != null)
                {
                    labelMonitorCaja = monitor.GetComponent<TextMeshPro>();
                }
            }
        }

        GuardarYActivarSintomaMonitor();
        InicializarIncidente();
    }

    private void Start()
    {
        // Suscribirse a la resolución en GameManager para restaurar el monitor
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnIncidenteResuelto += OnIncidenteResuelto;

            // Registrar el incidente al inicio si no está ya registrado
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
    /// Guarda el estado original del monitor de Caja y activa el mensaje de error "SIN REGISTROS" en rojo.
    /// </summary>
    public void GuardarYActivarSintomaMonitor()
    {
        if (labelMonitorCaja != null && !textoGuardado)
        {
            textoOriginalMonitor = labelMonitorCaja.text;
            colorOriginalMonitor = labelMonitorCaja.color;
            textoGuardado = true;

            // Mostrar síntoma en rojo
            labelMonitorCaja.text = "<b>CAJA</b>\n<size=75%><color=#EF4444>SIN REGISTROS</color></size>";
        }
    }

    /// <summary>
    /// Restablece el monitor de Caja a su condición operativa normal.
    /// </summary>
    public void RestaurarMonitorCaja()
    {
        if (labelMonitorCaja != null && textoGuardado)
        {
            IncidenteInternet incNet = Object.FindFirstObjectByType<IncidenteInternet>();
            if (incNet != null && incNet.incidente != null && incNet.incidente.activo)
            {
                // Si el fallo de Internet sigue activo, el monitor de Caja debe mostrar SIN CONEXION
                labelMonitorCaja.text = "<b>CAJA</b>\n<size=70%><color=#EF4444>SIN CONEXION</color></size>";
            }
            else
            {
                labelMonitorCaja.text = !string.IsNullOrEmpty(textoOriginalMonitor)
                    ? textoOriginalMonitor
                    : "<b>CAJA</b>\n<size=70%><color=#38BDF8>Terminal Conectada</color></size>";
                labelMonitorCaja.color = colorOriginalMonitor;
            }
        }
    }

    private void OnIncidenteResuelto(Incidente inc)
    {
        if (inc == this.incidente)
        {
            RestaurarMonitorCaja();
        }
    }

    /// <summary>
    /// Construye y configura los datos y opciones del incidente de Base de Datos.
    /// </summary>
    public void InicializarIncidente()
    {
        if (incidente != null && incidente.opciones != null && incidente.opciones.Count > 0)
        {
            // Ya configurado previamente (ej. por ConstructorEscena)
            if (interactuable != null) interactuable.incidente = incidente;
            return;
        }

        incidente = new Incidente(
            "Base de Datos Eliminada",
            5f, // Pérdida de 5 por segundo
            new List<OpcionSolucion>
            {
                new OpcionSolucion(
                    "Hoy 6:00 a.m.",
                    TipoResultado.Correcta,
                    2.0f,
                    +10,
                    true,
                    0f,
                    "Backup de las 6:00 a.m. restaurado con éxito. Se recuperó la base de datos limpia previa al borrado sin perder pedidos del día."
                ),
                new OpcionSolucion(
                    "Hoy 11:00 a.m.",
                    TipoResultado.Mala,
                    20.0f, // 20 s de penalización
                    0,
                    false, // NO resuelve (trampa posterior al borrado)
                    0f,
                    "Debías verificar que la hora del backup fuera anterior al borrado (10:30 a.m.). El backup de las 11:00 a.m. se generó después del incidente y estaba vacío."
                ),
                new OpcionSolucion(
                    "Ayer 11:00 p.m.",
                    TipoResultado.Regular,
                    3.0f,
                    -5,
                    true,
                    0f,
                    "El backup de ayer a las 11:00 p.m. resolvió el problema pero faltan los pedidos ingresados hoy hasta las 10:30 a.m."
                ),
                new OpcionSolucion(
                    "Hace una semana",
                    TipoResultado.Mala,
                    4.0f,
                    -20,
                    true,
                    0f,
                    "Restaurar un backup de hace una semana provocó un desastre: se perdió una semana completa de datos y los clientes están enfurecidos.",
                    setClientesMolestos: true
                )
            },
            "<b>Borrado de BD ocurrido a las:</b> <color=#F87171>10:30 a.m.</color>\nSeleccione un punto de restauración:"
        );

        if (interactuable != null)
        {
            interactuable.incidente = incidente;
        }
    }
}
