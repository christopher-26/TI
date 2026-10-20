using System.Text;
using UnityEngine;
using TMPro;

/// <summary>
/// Controlador del Canvas World Space ubicado sobre la ventanilla de pedidos.
/// Actualiza en tiempo real los indicadores de Dinero, Reputación, Tiempo transcurrido
/// y el listado de problemas o incidentes actualmente activos.
/// </summary>
public class HUDPanel : MonoBehaviour
{
    [Header("Referencias UI (TextMeshProUGUI)")]
    [Tooltip("Texto para mostrar el dinero acumulado en soles.")]
    public TextMeshProUGUI textoDinero;

    [Tooltip("Texto para mostrar el porcentaje de reputación del negocio.")]
    public TextMeshProUGUI textoReputacion;

    [Tooltip("Texto para mostrar el tiempo transcurrido (formato mm:ss).")]
    public TextMeshProUGUI textoTiempo;

    [Tooltip("Texto para listar los incidentes activos y su costo por segundo.")]
    public TextMeshProUGUI textoListaIncidentes;

    private StringBuilder bufferTexto = new StringBuilder();

    private void Update()
    {
        if (GameManager.Instance == null) return;

        ActualizarHUD();
    }

    /// <summary>
    /// Refresca los valores visuales del HUD consultando el estado actual de GameManager.
    /// </summary>
    public void ActualizarHUD()
    {
        GameManager gm = GameManager.Instance;

        // 1. Dinero en Soles (S/) con formato condicional de color
        if (textoDinero != null)
        {
            string colorDinero = gm.Dinero > 1500f ? "#22C55E" : (gm.Dinero > 0f ? "#F59E0B" : "#EF4444");
            textoDinero.text = $"DINERO\n<size=130%><color={colorDinero}><b>S/ {gm.Dinero:N0}</b></color></size>";
        }

        // 2. Reputación (0-100) con aviso si es menor a 40 (multiplicador x1.5)
        if (textoReputacion != null)
        {
            string colorRep = gm.Reputacion >= 70f ? "#38BDF8" : (gm.Reputacion >= 40f ? "#F59E0B" : "#EF4444");
            string alertaMultiplicador = gm.Reputacion < 40f ? "\n<size=75%><color=#EF4444><b>(PÉRDIDAS x1.5)</b></color></size>" : "";
            textoReputacion.text = $"REPUTACIÓN\n<size=130%><color={colorRep}><b>{gm.Reputacion:F0}%</b></color></size>{alertaMultiplicador}";
        }

        // 3. Tiempo de la jornada transcurrido
        if (textoTiempo != null)
        {
            int minutos = Mathf.FloorToInt(gm.TiempoTranscurrido / 60f);
            int segundos = Mathf.FloorToInt(gm.TiempoTranscurrido % 60f);
            textoTiempo.text = $"TIEMPO\n<size=130%><b>{minutos:00}:{segundos:00}</b></size>";
        }

        // 4. Lista de problemas activos
        if (textoListaIncidentes != null)
        {
            var activos = gm.incidentesActivos;
            if (activos == null || activos.Count == 0)
            {
                textoListaIncidentes.text = "<color=#22C55E><b>[OK] SISTEMA ESTABLE</b></color>\nNo hay incidentes activos en este momento.";
            }
            else
            {
                bufferTexto.Clear();
                bufferTexto.AppendLine($"<color=#EF4444><b>PROBLEMAS ACTIVOS ({activos.Count}):</b></color>");
                for (int i = 0; i < activos.Count; i++)
                {
                    Incidente inc = activos[i];
                    if (inc != null && inc.activo)
                    {
                        bufferTexto.AppendLine($"<color=#F87171>•</color> <b>{inc.nombre}</b> <color=#FCA5A5>(-S/ {inc.perdidaPorSegundo:F0}/s)</color>");
                    }
                }
                textoListaIncidentes.text = bufferTexto.ToString();
            }
        }
    }
}
