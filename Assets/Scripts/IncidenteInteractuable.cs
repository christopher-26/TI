using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Componente que se coloca en objetos físicos interactuables (Servidor, Router, PC, Impresora, etc.).
/// Utiliza XRSimpleInteractable para detectar el apuntado (hover) y selección (select con gatillo/rayo)
/// y abrir el PanelOpciones correspondiente al Incidente asignado.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class IncidenteInteractuable : MonoBehaviour
{
    [Header("Configuración del Incidente")]
    [Tooltip("Incidente asociado a este objeto interactuable.")]
    public Incidente incidente;

    [Tooltip("Referencia al panel de opciones flotante (si es nulo, se busca en la escena).")]
    public PanelOpciones panelOpciones;

    [Header("Retroalimentación Visual")]
    [Tooltip("Color de resaltado cuando el rayo XR apunta sobre este objeto.")]
    public Color colorHover = new Color(0.4f, 0.8f, 1.0f);

    private XRSimpleInteractable simpleInteractable;
    private MeshRenderer meshRenderer;
    private Color colorOriginal;
    private bool tieneMaterialPropio = false;

    private void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null && meshRenderer.material != null)
        {
            if (meshRenderer.material.HasProperty("_BaseColor"))
            {
                colorOriginal = meshRenderer.material.GetColor("_BaseColor");
                tieneMaterialPropio = true;
            }
            else if (meshRenderer.material.HasProperty("_Color"))
            {
                colorOriginal = meshRenderer.material.color;
                tieneMaterialPropio = true;
            }
        }
    }

    private void OnEnable()
    {
        if (simpleInteractable == null)
        {
            simpleInteractable = GetComponent<XRSimpleInteractable>();
        }

        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.AddListener(OnSelectEntered);
            simpleInteractable.hoverEntered.AddListener(OnHoverEntered);
            simpleInteractable.hoverExited.AddListener(OnHoverExited);
        }
    }

    private void OnDisable()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnSelectEntered);
            simpleInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            simpleInteractable.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        AbrirPanel();
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (tieneMaterialPropio && meshRenderer != null)
        {
            if (meshRenderer.material.HasProperty("_BaseColor"))
                meshRenderer.material.SetColor("_BaseColor", colorHover);
            else if (meshRenderer.material.HasProperty("_Color"))
                meshRenderer.material.color = colorHover;
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (tieneMaterialPropio && meshRenderer != null)
        {
            if (meshRenderer.material.HasProperty("_BaseColor"))
                meshRenderer.material.SetColor("_BaseColor", colorOriginal);
            else if (meshRenderer.material.HasProperty("_Color"))
                meshRenderer.material.color = colorOriginal;
        }
    }

    /// <summary>
    /// Fallback para pruebas rápidas con ratón en el editor.
    /// </summary>
    private void OnMouseDown()
    {
        AbrirPanel();
    }

    /// <summary>
    /// Abre el panel de opciones y le pasa el incidente asociado.
    /// </summary>
    public void AbrirPanel()
    {
        if (panelOpciones == null)
        {
            panelOpciones = Object.FindFirstObjectByType<PanelOpciones>();
        }

        if (incidente != null && !incidente.activo)
        {
            Debug.Log($"<color=green>[IncidenteInteractuable]</color> {incidente.nombre} ya se encuentra resuelto.");
            return;
        }

        if (panelOpciones != null && incidente != null)
        {
            panelOpciones.Mostrar(incidente, transform);
            Debug.Log($"<color=cyan>[Interacción]</color> Panel de opciones abierto para: {incidente.nombre}");
        }
        else
        {
            Debug.LogWarning($"[IncidenteInteractuable] No se pudo abrir el panel para {gameObject.name}. Panel o Incidente nulo.");
        }
    }
}
