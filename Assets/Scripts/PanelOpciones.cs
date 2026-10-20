using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel flotante reutilizable en World Space.
/// Muestra los detalles de un Incidente y genera dinámicamente un botón por cada OpcionSolucion.
/// Al seleccionar una opción, comunica a GameManager el resultado, muestra una barra de progreso
/// por la penalización de tiempo (mientras el dinero sigue drenando) y muestra el resultado final.
/// </summary>
public class PanelOpciones : MonoBehaviour
{
    [Header("Contenedor Principal")]
    [Tooltip("Objeto raíz del panel visual para mostrarlo u ocultarlo.")]
    public GameObject panelRaiz;

    [Header("Textos de Información")]
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoPerdida;
    [Tooltip("Texto opcional para mostrar detalles del fallo como la hora de ocurrencia o contexto.")]
    public TextMeshProUGUI textoDetalle;

    [Header("Contenedor de Botones")]
    [Tooltip("Contenedor donde se instancian los botones de las opciones.")]
    public Transform contenedorBotones;

    [Tooltip("Plantilla de botón que se clonará por cada opción.")]
    public GameObject templateBoton;

    [Header("Barra de Progreso")]
    public GameObject contenedorProgreso;
    public RectTransform barraProgresoRelleno;
    public TextMeshProUGUI textoProgreso;

    [Header("Mensaje de Resultado y Cierre")]
    public TextMeshProUGUI textoResultado;
    public Button botonCerrar;

    // Incidente actualmente cargado en el panel
    private Incidente incidenteActual;
    // Lista de botones instanciados para destruirlos al recargar
    private List<GameObject> botonesInstanciados = new List<GameObject>();
    // Bandera para evitar abrir o cerrar el panel mientras el técnico está trabajando en una opción
    public bool enAccion = false;

    private void Awake()
    {
        if (templateBoton != null)
        {
            templateBoton.SetActive(false);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.AddListener(Ocultar);
        }

        if (panelRaiz != null)
        {
            panelRaiz.SetActive(false);
        }
    }

    /// <summary>
    /// Abre el panel, lo orienta hacia el jugador y construye los botones para el incidente recibido.
    /// </summary>
    public void Mostrar(Incidente incidente, Transform ancla = null)
    {
        if (enAccion)
        {
            Debug.LogWarning("[PanelOpciones] El técnico está ocupado aplicando una solución. Espere a que termine.");
            return;
        }

        if (incidente == null) return;

        incidenteActual = incidente;

        // 1. Posicionar el panel cerca del objeto interactuado mirando al usuario
        PosicionarPanel(ancla);

        // 2. Asignar encabezados
        if (textoTitulo != null)
        {
            textoTitulo.text = $"INCIDENTE: {incidente.nombre.ToUpper()}";
        }

        if (textoPerdida != null)
        {
            textoPerdida.text = $"Pérdida actual: <color=#EF4444>-S/ {incidente.perdidaPorSegundo:F0} por segundo</color>";
        }

        if (textoDetalle != null)
        {
            if (!string.IsNullOrEmpty(incidente.descripcion))
            {
                textoDetalle.text = incidente.descripcion;
                textoDetalle.gameObject.SetActive(true);
            }
            else
            {
                textoDetalle.gameObject.SetActive(false);
            }
        }
        else if (textoPerdida != null && !string.IsNullOrEmpty(incidente.descripcion))
        {
            textoPerdida.text += $"\n<size=80%><color=#FCD34D>{incidente.descripcion}</color></size>";
        }

        // 3. Limpiar botones generados anteriormente
        LimpiarBotones();

        // 4. Instanciar un botón por cada opción de solución
        if (templateBoton != null && contenedorBotones != null)
        {
            contenedorBotones.gameObject.SetActive(true);

            for (int i = 0; i < incidente.opciones.Count; i++)
            {
                OpcionSolucion opcion = incidente.opciones[i];
                GameObject btnGO = Instantiate(templateBoton, contenedorBotones);
                btnGO.name = $"Boton_Opcion_{i + 1}";
                btnGO.SetActive(true);
                botonesInstanciados.Add(btnGO);

                // Configurar texto del botón
                TextMeshProUGUI tmpBtn = btnGO.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpBtn != null)
                {
                    string signoRep = opcion.deltaReputacion >= 0 ? "+" : "";
                    tmpBtn.text = $"<b>{opcion.texto}</b>\n<size=75%><color=#94A3B8>Penalización: {opcion.penalizacionSegundos:F1}s | Reputación: {signoRep}{opcion.deltaReputacion}</color></size>";
                }

                // Asignar evento al hacer clic
                Button btnComp = btnGO.GetComponent<Button>();
                if (btnComp != null)
                {
                    btnComp.onClick.AddListener(() => OnOpcionClick(opcion));
                }
            }
        }

        // 5. Restablecer barra de progreso y mensaje de resultado
        if (contenedorProgreso != null)
        {
            contenedorProgreso.SetActive(false);
        }
        if (textoResultado != null)
        {
            textoResultado.gameObject.SetActive(false);
        }

        // 6. Activar visualmente el panel
        if (panelRaiz != null)
        {
            panelRaiz.SetActive(true);
        }
    }

    /// <summary>
    /// Maneja el clic en una de las opciones disponibles.
    /// </summary>
    private void OnOpcionClick(OpcionSolucion opcion)
    {
        enAccion = true;
        if (botonCerrar != null) botonCerrar.interactable = false;

        // Desactivar botones para evitar selecciones múltiples
        for (int i = 0; i < botonesInstanciados.Count; i++)
        {
            if (botonesInstanciados[i] != null)
            {
                Button b = botonesInstanciados[i].GetComponent<Button>();
                if (b != null) b.interactable = false;
            }
        }

        // Mostrar sección de barra de progreso
        if (contenedorProgreso != null)
        {
            contenedorProgreso.SetActive(true);
        }

        if (barraProgresoRelleno != null)
        {
            barraProgresoRelleno.anchorMin = new Vector2(0f, 0f);
            barraProgresoRelleno.anchorMax = new Vector2(0f, 1f);
        }

        // Invocar a GameManager para aplicar la opción y su penalización de tiempo
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AplicarOpcion(
                incidenteActual,
                opcion,
                alActualizarProgreso: (progreso) =>
                {
                    if (barraProgresoRelleno != null)
                    {
                        barraProgresoRelleno.anchorMax = new Vector2(progreso, 1f);
                    }
                    if (textoProgreso != null)
                    {
                        textoProgreso.text = $"Trabajando: {Mathf.RoundToInt(progreso * 100)}%";
                    }
                },
                alFinalizar: () =>
                {
                    MostrarResultado(opcion);
                }
            );
        }
    }

    /// <summary>
    /// Muestra la retroalimentación técnica y programa el cierre del panel.
    /// </summary>
    private void MostrarResultado(OpcionSolucion opcion)
    {
        enAccion = false;
        if (botonCerrar != null) botonCerrar.interactable = true;

        if (contenedorProgreso != null)
        {
            contenedorProgreso.SetActive(false);
        }

        if (textoResultado != null)
        {
            string colorHex = opcion.resuelve ? (opcion.reapareceTrasSegundos > 0f ? "#F59E0B" : "#22C55E") : "#EF4444";
            string estado = opcion.reapareceTrasSegundos > 0f
                ? "SOLUCIÓN TEMPORAL (REAPARECERÁ)"
                : (opcion.resuelve ? "¡INCIDENTE RESUELTO!" : "LA ACCIÓN NO RESOLVIÓ EL INCIDENTE");
            textoResultado.text = $"<color={colorHex}><b>{estado}</b></color>\n<size=85%>{opcion.mensajeInforme}</size>";
            textoResultado.gameObject.SetActive(true);
        }

        // Cerrar automáticamente tras 2 segundos para no interrumpir al jugador
        StartCoroutine(RutinaCerrarConRetraso(2.5f));
    }

    private IEnumerator RutinaCerrarConRetraso(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        Ocultar();
    }

    /// <summary>
    /// Cierra el panel y desactiva su visualización.
    /// </summary>
    public void Ocultar()
    {
        if (enAccion) return;

        StopAllCoroutines();
        LimpiarBotones();

        if (panelRaiz != null)
        {
            panelRaiz.SetActive(false);
        }
    }

    private void LimpiarBotones()
    {
        for (int i = 0; i < botonesInstanciados.Count; i++)
        {
            if (botonesInstanciados[i] != null)
            {
                Destroy(botonesInstanciados[i]);
            }
        }
        botonesInstanciados.Clear();
    }

    private void PosicionarPanel(Transform ancla)
    {
        Camera cam = Camera.main;
        Vector3 posCam = cam != null ? cam.transform.position : Vector3.zero;

        if (ancla != null)
        {
            Vector3 posAncla = ancla.position;
            Vector3 dirCam = (posCam - posAncla).normalized;

            // Elevar un poco y acercar al jugador para que no colisione con el mueble o pared
            transform.position = posAncla + Vector3.up * 0.40f + dirCam * 0.45f;
            if (cam != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - posCam);
            }
        }
        else if (cam != null)
        {
            // Posición por defecto flotando a 1 metro frente a la cámara
            transform.position = posCam + cam.transform.forward * 1.0f;
            transform.rotation = Quaternion.LookRotation(transform.position - posCam);
        }
    }
}
