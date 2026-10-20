using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gestiona la pantalla de Informe Final y Auditoría de Decisiones al concluir la partida.
/// Muestra:
/// 1. Desenlace alcanzado: "QUIEBRA", "NEGOCIO SALVADO" o "SOBREVIVISTE".
/// 2. Métricas finales: Dinero restante, dinero total perdido, reputación final y tiempo total transcurrido.
/// 3. Orden cronológico en que se atendieron los incidentes, opción elegida, resultado y tiempo.
/// 4. Recomendaciones técnicas (mensajeInforme) para todas las decisiones con resultado Regular o Mala.
/// 5. Botón "Reintentar" para reiniciar la escena y volver a jugar.
/// </summary>
public class InformeFinal : MonoBehaviour
{
    [Header("Contenedor Principal")]
    [Tooltip("Raíz visual de la interfaz del informe final.")]
    public GameObject panelRaiz;

    [Header("Textos de Encabezado y Estadísticas")]
    public TextMeshProUGUI textoTituloFinal;
    public TextMeshProUGUI textoSubtituloFinal;
    public TextMeshProUGUI textoMetricas;

    [Header("Textos de Detalle y Recomendaciones")]
    public TextMeshProUGUI textoHistorialDecisiones;
    public TextMeshProUGUI textoRecomendaciones;

    [Header("Botón de Reintento")]
    public Button botonReintentar;

    private StringBuilder bufferTexto = new StringBuilder();

    private void Awake()
    {
        if (botonReintentar != null)
        {
            botonReintentar.onClick.AddListener(ReiniciarPartida);
        }

        if (panelRaiz != null)
        {
            panelRaiz.SetActive(false);
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnJuegoTerminado += MostrarInforme;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnJuegoTerminado -= MostrarInforme;
        }
    }

    /// <summary>
    /// Construye y despliega la pantalla de informe final orientándola hacia el jugador.
    /// </summary>
    public void MostrarInforme(TipoFinal final)
    {
        if (GameManager.Instance == null) return;

        GameManager gm = GameManager.Instance;

        // 1. Posicionar el informe en un plano cómodo frente al usuario
        PosicionarFrenteAlUsuario();

        // 2. Encabezado del desenlace
        ConfigurarEncabezado(final);

        // 3. Métricas económicas y de reputación
        ConfigurarMetricas(gm);

        // 4. Historial cronológico de atención de incidentes
        ConfigurarHistorial(gm);

        // 5. Recomendaciones de mejora para decisiones Regular o Mala
        ConfigurarRecomendaciones(gm);

        // 6. Activar panel visual
        if (panelRaiz != null)
        {
            panelRaiz.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void ConfigurarEncabezado(TipoFinal final)
    {
        if (textoTituloFinal == null) return;

        switch (final)
        {
            case TipoFinal.NegocioSalvado:
                textoTituloFinal.text = "<color=#22C55E><b>¡NEGOCIO SALVADO!</b></color>";
                if (textoSubtituloFinal != null)
                    textoSubtituloFinal.text = "¡Excelente gestión técnica! Resolviste todas las crisis manteniendo alta solvencia y excelente reputación.";
                break;

            case TipoFinal.Sobreviviste:
                textoTituloFinal.text = "<color=#F59E0B><b>SOBREVIVISTE A LA CRISIS</b></color>";
                if (textoSubtituloFinal != null)
                    textoSubtituloFinal.text = "Lograste estabilizar los sistemas, pero los fondos económicos o la satisfacción del cliente sufrieron pérdidas significativas.";
                break;

            case TipoFinal.Quiebra:
                textoTituloFinal.text = "<color=#EF4444><b>QUIEBRA COMERCIAL</b></color>";
                if (textoSubtituloFinal != null)
                    textoSubtituloFinal.text = "Los fondos del negocio llegaron a S/ 0 antes de solucionar los problemas. El negocio tuvo que cerrar sus puertas.";
                break;

            default:
                textoTituloFinal.text = "<b>INFORME DE TURNO</b>";
                break;
        }
    }

    private void ConfigurarMetricas(GameManager gm)
    {
        if (textoMetricas == null) return;

        float dineroRestante = Mathf.Max(0f, gm.Dinero);
        float dineroPerdido = 5000f - dineroRestante;

        int minutos = Mathf.FloorToInt(gm.TiempoTranscurrido / 60f);
        int segundos = Mathf.FloorToInt(gm.TiempoTranscurrido % 60f);

        string colorDinero = dineroRestante >= 2500f ? "#22C55E" : (dineroRestante > 0f ? "#F59E0B" : "#EF4444");
        string colorRep = gm.Reputacion >= 70f ? "#38BDF8" : (gm.Reputacion >= 40f ? "#F59E0B" : "#EF4444");

        textoMetricas.text = $"<b>Dinero Restante:</b> <color={colorDinero}>S/ {dineroRestante:N0}</color>  |  " +
                             $"<b>Pérdida Total:</b> <color=#EF4444>-S/ {dineroPerdido:N0}</color>  |  " +
                             $"<b>Reputación Final:</b> <color={colorRep}>{gm.Reputacion:F0}%</color>  |  " +
                             $"<b>Tiempo Total:</b> {minutos:00}:{segundos:00}";
    }

    private void ConfigurarHistorial(GameManager gm)
    {
        if (textoHistorialDecisiones == null) return;

        bufferTexto.Clear();
        bufferTexto.AppendLine("<b>ORDEN DE ATENCIÓN Y DECISIONES TOMADAS:</b>");

        if (gm.historialDecisiones == null || gm.historialDecisiones.Count == 0)
        {
            bufferTexto.AppendLine("<color=#94A3B8>No se registraron intervenciones antes del fin de la partida.</color>");
        }
        else
        {
            for (int i = 0; i < gm.historialDecisiones.Count; i++)
            {
                RegistroDecision d = gm.historialDecisiones[i];
                int min = Mathf.FloorToInt(d.tiempoMomento / 60f);
                int seg = Mathf.FloorToInt(d.tiempoMomento % 60f);

                string colorRes;
                switch (d.resultado)
                {
                    case TipoResultado.Correcta: colorRes = "#22C55E"; break;
                    case TipoResultado.Regular: colorRes = "#F59E0B"; break;
                    default: colorRes = "#EF4444"; break;
                }

                string signoRep = d.deltaReputacion >= 0 ? "+" : "";
                bufferTexto.AppendLine($"<b>{i + 1}.</b> [{min:00}:{seg:00}] <b>{d.nombreIncidente}</b> -> \"{d.opcionTexto}\" " +
                                       $"[<color={colorRes}><b>{d.resultado}</b></color>] (Tiempo: {d.penalizacionSegundos:F0}s | Rep: {signoRep}{d.deltaReputacion})");
            }
        }

        textoHistorialDecisiones.text = bufferTexto.ToString();
    }

    private void ConfigurarRecomendaciones(GameManager gm)
    {
        if (textoRecomendaciones == null) return;

        bufferTexto.Clear();
        bufferTexto.AppendLine("<b>RECOMENDACIONES DE MEJORA TÉCNICA:</b>");

        int advertencias = 0;
        if (gm.historialDecisiones != null)
        {
            for (int i = 0; i < gm.historialDecisiones.Count; i++)
            {
                RegistroDecision d = gm.historialDecisiones[i];
                if (d.resultado == TipoResultado.Regular || d.resultado == TipoResultado.Mala)
                {
                    advertencias++;
                    string colorBadge = d.resultado == TipoResultado.Regular ? "#F59E0B" : "#EF4444";
                    bufferTexto.AppendLine($"<color={colorBadge}>• [{d.nombreIncidente} - {d.resultado}]:</color> {d.mensajeInforme}");
                }
            }
        }

        if (advertencias == 0)
        {
            bufferTexto.AppendLine("<color=#22C55E><b>¡Cero errores técnicos!</b> Todas las intervenciones fueron acertadas, atacando la causa raíz de inmediato sin parches innecesarios.</color>");
        }

        textoRecomendaciones.text = bufferTexto.ToString();
    }

    private void PosicionarFrenteAlUsuario()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 posCam = cam.transform.position;
            Vector3 forwardPlano = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            if (forwardPlano.sqrMagnitude < 0.01f) forwardPlano = Vector3.forward;

            // Situar el panel a 1.6 metros frente al jugador a la altura de la vista
            transform.position = posCam + forwardPlano * 1.6f + Vector3.up * 0.1f;
            transform.rotation = Quaternion.LookRotation(forwardPlano);
        }
    }

    /// <summary>
    /// Recarga la escena activa para permitir un nuevo intento inmediato en VR.
    /// </summary>
    public void ReiniciarPartida()
    {
        Debug.Log("<color=cyan>[InformeFinal]</color> Reiniciando partida...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
