using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Gestiona el Problema 3: IMPRESORA DESCONFIGURADA.
/// - Objeto interactuable: La impresora en la mesa lateral.
/// - Pérdida por segundo: S/ 1 por segundo, activo desde el inicio de la jornada.
/// - Síntomas:
///   1. Una pequeña pantalla integrada en la impresora muestra "NO DISPONIBLE".
///   2. La etiqueta del trabajador de "Caja" muestra "Sin boletas".
/// - Causa real: El router cambió la dirección IP de la impresora por DHCP dinámico.
/// - Opciones de solución:
///   1. "Asignar IP fija y actualizar el puerto" -> Correcta: Resuelve permanentemente, reputación +5.
///   2. "Reiniciar la impresora" -> Regular: Resuelve temporalmente y el incidente REAPARECE tras 60 segundos (reapareceTrasSegundos = 60).
///   3. "Cambiar el cartucho" -> Mala: NO resuelve, penalización de 15 s, reputación -2.
/// - Al resolverse, la pantalla de la impresora muestra "LISTA" y la etiqueta del trabajador vuelve a "Caja".
/// </summary>
public class IncidenteImpresora : MonoBehaviour
{
    [Header("Interactable")]
    [Tooltip("Componente interactuable colocado en la impresora física.")]
    public IncidenteInteractuable interactuable;

    [Header("Elementos Visuales")]
    [Tooltip("Label TextMeshPro de la pequeña pantalla LCD en la impresora.")]
    public TextMeshPro pantallaImpresora;

    [Tooltip("Label TextMeshPro de la etiqueta flotante sobre el trabajador de Caja.")]
    public TextMeshPro etiquetaTrabajadorCaja;

    [Header("Datos del Incidente")]
    public Incidente incidente;

    private string textoOriginalTrabajador = "Caja";
    private bool textoGuardado = false;

    private void Awake()
    {
        BuscarReferenciasSiFaltan();
        InicializarIncidente();
        ActivarSintoma();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnIncidenteResuelto += OnIncidenteResuelto;
            GameManager.Instance.OnIncidenteRegistrado += OnIncidenteRegistrado;

            // Registrar el incidente en GameManager para drenar S/ 1/s desde el inicio
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
            GameManager.Instance.OnIncidenteRegistrado -= OnIncidenteRegistrado;
        }
    }

    /// <summary>
    /// Activa los síntomas visuales del fallo: pantalla en "NO DISPONIBLE" y trabajador en "Sin boletas".
    /// </summary>
    public void ActivarSintoma()
    {
        // 1. Guardar texto original del trabajador de Caja si aún no se guardó
        if (etiquetaTrabajadorCaja != null && !textoGuardado)
        {
            textoOriginalTrabajador = etiquetaTrabajadorCaja.text;
            textoGuardado = true;
        }

        // 2. Mostrar síntoma en la pantalla de la impresora
        if (pantallaImpresora != null)
        {
            pantallaImpresora.text = "<color=#EF4444><b>NO DISPONIBLE</b></color>";
        }

        // 3. Mostrar síntoma en la etiqueta del trabajador de Caja
        if (etiquetaTrabajadorCaja != null)
        {
            etiquetaTrabajadorCaja.text = "<b>Caja</b>\n<size=70%><color=#EF4444>(Sin boletas)</color></size>";
        }
    }

    /// <summary>
    /// Restablece los elementos visuales a su estado operativo cuando el problema es solucionado.
    /// </summary>
    public void RestaurarVisuales()
    {
        if (pantallaImpresora != null)
        {
            pantallaImpresora.text = "<color=#22C55E><b>LISTA</b></color>\n<size=65%><color=#38BDF8>192.168.1.50</color></size>";
        }

        if (etiquetaTrabajadorCaja != null)
        {
            etiquetaTrabajadorCaja.text = !string.IsNullOrEmpty(textoOriginalTrabajador) ? textoOriginalTrabajador : "Caja";
        }
    }

    private void OnIncidenteResuelto(Incidente inc)
    {
        if (inc == this.incidente)
        {
            RestaurarVisuales();
            Debug.Log("<color=green>[Impresora]</color> Impresora restaurada a estado operativo.");
        }
    }

    private void OnIncidenteRegistrado(Incidente inc)
    {
        // Si el incidente reapareció (ej. al cumplirse los 60 segundos tras reiniciar la impresora)
        if (inc == this.incidente)
        {
            ActivarSintoma();
            Debug.LogWarning("<color=yellow>[Impresora]</color> ¡El fallo de la impresora reapareció tras 60 segundos por IP dinámica!");
        }
    }

    /// <summary>
    /// Inicializa la configuración de datos y opciones de solución para la impresora.
    /// </summary>
    public void InicializarIncidente()
    {
        if (incidente != null && incidente.opciones != null && incidente.opciones.Count > 0)
        {
            if (interactuable != null) interactuable.incidente = incidente;
            return;
        }

        incidente = new Incidente(
            "Impresora Desconfigurada",
            1f, // Pérdida de S/ 1 por segundo
            new List<OpcionSolucion>
            {
                new OpcionSolucion(
                    "Asignar IP fija y actualizar el puerto",
                    TipoResultado.Correcta,
                    2.0f, // 2s de penalización de tiempo
                    +5,   // +5 de reputación
                    true, // Resuelve permanentemente
                    0f,   // No reaparece
                    "¡Excelente trabajo! Configurar una IP estática en la impresora y actualizar el puerto TCP/IP en la estación de Caja soluciona la causa raíz permanentemente, impidiendo que el router altere la dirección."
                ),
                new OpcionSolucion(
                    "Reiniciar la impresora",
                    TipoResultado.Regular,
                    3.0f,  // 3s de penalización
                    0,     // Sin cambio de reputación
                    true,  // Resuelve temporalmente
                    60.0f, // REAPARECE tras 60 segundos
                    "Reiniciar es un parche temporal: renueva la concesión DHCP temporalmente, pero al persistir la asignación dinámica de IP, el router volvió a cambiarla y el fallo reapareció a los 60 segundos."
                ),
                new OpcionSolucion(
                    "Cambiar el cartucho",
                    TipoResultado.Mala,
                    15.0f, // 15 s de penalización
                    -2,    // -2 de reputación
                    false, // NO resuelve
                    0f,
                    "Cambiar el cartucho fue un error: el mensaje 'NO DISPONIBLE' indicaba fallo de red/IP, no escasez de tóner o tinta. Perdiste 15 segundos y suministros."
                )
            },
            "<b>Diagnóstico de Impresora:</b> Caja reporta 'Sin boletas'. La pantalla LCD indica desconexión.\nCausa raíz: El servidor DHCP del router alteró la dirección IP:"
        );

        if (interactuable != null)
        {
            interactuable.incidente = incidente;
        }
    }

    private void BuscarReferenciasSiFaltan()
    {
        if (interactuable == null)
        {
            interactuable = GetComponent<IncidenteInteractuable>();
        }

        if (pantallaImpresora == null)
        {
            GameObject pantGO = GameObject.Find("Pantalla_Impresora_TMP");
            if (pantGO != null)
            {
                pantallaImpresora = pantGO.GetComponent<TextMeshPro>();
            }
        }

        if (etiquetaTrabajadorCaja == null)
        {
            GameObject puestoCaja = GameObject.Find("Puesto_Caja");
            if (puestoCaja != null)
            {
                Transform et = puestoCaja.transform.Find("Etiqueta_Caja");
                if (et != null)
                {
                    etiquetaTrabajadorCaja = et.GetComponent<TextMeshPro>();
                }
            }
        }
    }
}
