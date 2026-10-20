using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tipos de desenlace final para la partida.
/// </summary>
public enum TipoFinal
{
    EnJuego,
    Quiebra,
    NegocioSalvado,
    Sobreviviste
}

/// <summary>
/// Registro histórico de cada acción/decisión tomada por el técnico.
/// </summary>
[Serializable]
public class RegistroDecision
{
    public string nombreIncidente;
    public string opcionTexto;
    public TipoResultado resultado;
    public float tiempoMomento;
    public float penalizacionSegundos;
    public int deltaReputacion;
    public string mensajeInforme;
}

/// <summary>
/// Gestor principal del juego. Administra la economía (Dinero), la satisfacción del negocio (Reputación),
/// el reloj de la partida y la lista de incidentes activos.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Economía y Reputación")]
    [Tooltip("Dinero actual del negocio en soles (S/). Inicia en 5000.")]
    public float Dinero = 5000f;

    [Tooltip("Reputación del negocio (0 a 100). Inicia en 70.")]
    public float Reputacion = 70f;

    [Header("Estado del Negocio")]
    [Tooltip("Indica si los clientes están enfurecidos por pérdida de información histórica.")]
    public bool clientesMolestos = false;

    [Header("Tiempo")]
    [Tooltip("Segundos transcurridos desde el inicio de la jornada.")]
    public float TiempoTranscurrido = 0f;

    [Header("Incidentes")]
    [Tooltip("Lista de incidentes actualmente activos y generando pérdidas.")]
    public List<Incidente> incidentesActivos = new List<Incidente>();

    [Header("Estado de Partida y Finales")]
    [Tooltip("Desenlace alcanzado en la partida.")]
    public TipoFinal finalAlcanzado = TipoFinal.EnJuego;

    [Tooltip("Indica si la partida ha concluido por quiebra o resolución de todos los incidentes.")]
    public bool juegoTerminado = false;

    [Tooltip("Historial cronológico de decisiones tomadas.")]
    public List<RegistroDecision> historialDecisiones = new List<RegistroDecision>();

    [Tooltip("Conjunto de todos los incidentes registrados en la partida.")]
    public List<Incidente> incidentesTotales = new List<Incidente>();

    [Tooltip("Incidentes que tienen una reaparición pendiente (solución temporal).")]
    public List<Incidente> incidentesTemporalesPendientes = new List<Incidente>();

    // Acumulador de tiempo para ejecutar la deducción monetaria exactamente cada segundo
    private float temporizadorSegundo = 0f;

    // Eventos opcionales para actualizar componentes desacoplados
    public event Action OnDatosActualizados;
    public event Action<Incidente> OnIncidenteRegistrado;
    public event Action<Incidente> OnIncidenteResuelto;
    public event Action<TipoFinal> OnJuegoTerminado;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (juegoTerminado) return;

        // 1. Avanzar el reloj de tiempo transcurrido
        TiempoTranscurrido += Time.deltaTime;

        // 2. Acumular y procesar la pérdida de dinero cada segundo exacto
        temporizadorSegundo += Time.deltaTime;
        if (temporizadorSegundo >= 1f)
        {
            temporizadorSegundo -= 1f;
            ProcesarPerdidasPorSegundo();
        }

        // 3. Evaluar quiebra o despido en tiempo real
        if (Dinero <= 0f || Reputacion <= 0f)
        {
            if (Dinero < 0f) Dinero = 0f;
            EvaluarFinal();
        }
    }

    /// <summary>
    /// Calcula y resta el dinero correspondiente a la suma de pérdidas de todos los incidentes activos.
    /// Si la reputación cae por debajo de 40, las pérdidas se multiplican por 1.5.
    /// </summary>
    private void ProcesarPerdidasPorSegundo()
    {
        float perdidaTotal = 0f;

        // Sumar las pérdidas por segundo de cada incidente activo
        for (int i = 0; i < incidentesActivos.Count; i++)
        {
            if (incidentesActivos[i] != null && incidentesActivos[i].activo)
            {
                perdidaTotal += incidentesActivos[i].perdidaPorSegundo;
            }
        }

        // Penalización por baja reputación: si Reputación < 40, pérdidas x 1.5
        if (Reputacion < 40f)
        {
            perdidaTotal *= 1.5f;
        }

        // Descontar del dinero acumulado
        Dinero -= perdidaTotal;

        OnDatosActualizados?.Invoke();
    }

    /// <summary>
    /// Modifica la reputación del negocio asegurando que siempre permanezca en el rango 0 a 100.
    /// </summary>
    public void ModificarReputacion(int delta)
    {
        Reputacion = Mathf.Clamp(Reputacion + delta, 0f, 100f);
        OnDatosActualizados?.Invoke();
        if (Reputacion <= 0f)
        {
            EvaluarFinal();
        }
    }

    /// <summary>
    /// Registra un nuevo incidente en la lista de activos e inicia su contador de tiempo.
    /// </summary>
    public void RegistrarIncidente(Incidente incidente)
    {
        if (incidente == null) return;

        incidente.activo = true;
        incidente.tiempoInicio = TiempoTranscurrido;

        if (!incidentesActivos.Contains(incidente))
        {
            incidentesActivos.Add(incidente);
        }

        if (!incidentesTotales.Contains(incidente))
        {
            incidentesTotales.Add(incidente);
        }

        OnIncidenteRegistrado?.Invoke(incidente);
        OnDatosActualizados?.Invoke();
        Debug.Log($"<color=orange>[Incidente Registrado]</color> {incidente.nombre} (-S/ {incidente.perdidaPorSegundo}/s)");
    }

    /// <summary>
    /// Marca un incidente como resuelto, registra su tiempo y lo remueve de la lista activa.
    /// </summary>
    public void ResolverIncidente(Incidente incidente)
    {
        if (incidente == null) return;

        incidente.activo = false;
        incidente.tiempoResuelto = TiempoTranscurrido;
        incidentesActivos.Remove(incidente);

        OnIncidenteResuelto?.Invoke(incidente);
        OnDatosActualizados?.Invoke();
        Debug.Log($"<color=green>[Incidente Resuelto]</color> {incidente.nombre} en el segundo {TiempoTranscurrido:F1}");

        EvaluarFinal();
    }

    /// <summary>
    /// Aplica el resultado de una opción elegida por el técnico:
    /// Modifica reputación, ejecuta la penalización de tiempo (mientras el dinero sigue drenando)
    /// y resuelve o programa la reaparición del incidente.
    /// </summary>
    public void AplicarOpcion(
        Incidente incidente,
        OpcionSolucion opcion,
        Action<float> alActualizarProgreso = null,
        Action alFinalizar = null)
    {
        if (incidente == null || opcion == null) return;

        StartCoroutine(RutinaAplicarOpcion(incidente, opcion, alActualizarProgreso, alFinalizar));
    }

    private IEnumerator RutinaAplicarOpcion(
        Incidente incidente,
        OpcionSolucion opcion,
        Action<float> alActualizarProgreso,
        Action alFinalizar)
    {
        // 1. Aplicar cambio de reputación
        ModificarReputacion(opcion.deltaReputacion);

        // 2. Simular tiempo de trabajo del técnico según la penalización de la opción
        float duracion = Mathf.Max(0f, opcion.penalizacionSegundos);
        if (duracion > 0f)
        {
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float progreso = Mathf.Clamp01(tiempo / duracion);
                alActualizarProgreso?.Invoke(progreso);
                yield return null; // El dinero continúa drenando en Update()
            }
        }

        alActualizarProgreso?.Invoke(1f);

        // Guardar la decisión en el historial de auditoría para el informe final
        RegistroDecision registro = new RegistroDecision
        {
            nombreIncidente = incidente.nombre,
            opcionTexto = opcion.texto,
            resultado = opcion.resultado,
            tiempoMomento = TiempoTranscurrido,
            penalizacionSegundos = duracion,
            deltaReputacion = opcion.deltaReputacion,
            mensajeInforme = opcion.mensajeInforme
        };
        historialDecisiones.Add(registro);

        // 3. Si la opción resuelve el problema, detener el drenaje de este incidente
        if (opcion.resuelve)
        {
            ResolverIncidente(incidente);
        }

        // 4. Si la opción activa clientes molestos
        if (opcion.setClientesMolestos)
        {
            clientesMolestos = true;
            Debug.LogWarning("<color=red>[GameManager] ¡Flag clientesMolestos activado!</color> Pérdida masiva de datos de la semana.");
        }

        // 5. Ejecutar callback personalizado si existe
        opcion.alAplicar?.Invoke();

        // 6. Si la opción es una solución temporal (reapareceTrasSegundos > 0), programar reinicio
        if (opcion.reapareceTrasSegundos > 0f)
        {
            if (!incidentesTemporalesPendientes.Contains(incidente))
            {
                incidentesTemporalesPendientes.Add(incidente);
            }
            StartCoroutine(RutinaReaparicion(incidente, opcion.reapareceTrasSegundos));
        }
        else if (opcion.resuelve)
        {
            incidentesTemporalesPendientes.Remove(incidente);
        }

        alFinalizar?.Invoke();
        EvaluarFinal();
    }

    private IEnumerator RutinaReaparicion(Incidente incidente, float retraso)
    {
        yield return new WaitForSeconds(retraso);
        incidentesTemporalesPendientes.Remove(incidente);
        RegistrarIncidente(incidente);
        Debug.LogWarning($"<color=yellow>[Incidente Reapareció]</color> {incidente.nombre} tras {retraso} segundos.");
    }

    /// <summary>
    /// Evalúa en cualquier momento si se han cumplido las condiciones de final de partida:
    /// - Quiebra: Dinero <= 0.
    /// - Negocio salvado: Todos los incidentes resueltos (sin parches temporales pendientes) con Dinero >= 2500 y Reputación >= 70.
    /// - Sobreviviste: Todos los incidentes resueltos en cualquier otro caso.
    /// </summary>
    public void EvaluarFinal()
    {
        if (juegoTerminado) return;

        // 1. Quiebra inmediata por falta de fondos o despido por reputación en 0
        if (Dinero <= 0f || Reputacion <= 0f)
        {
            FinalizarPartida(TipoFinal.Quiebra);
            return;
        }

        // 2. Verificar resolución completa de todos los incidentes sin parches temporales
        bool todosRegistrados = incidentesTotales.Count >= 3;
        bool sinActivos = incidentesActivos.Count == 0;
        bool sinTemporalesPendientes = incidentesTemporalesPendientes.Count == 0;

        if (todosRegistrados && sinActivos && sinTemporalesPendientes)
        {
            if (Dinero >= 2500f && Reputacion >= 70f)
            {
                FinalizarPartida(TipoFinal.NegocioSalvado);
            }
            else
            {
                FinalizarPartida(TipoFinal.Sobreviviste);
            }
        }
    }

    private void FinalizarPartida(TipoFinal tipo)
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        finalAlcanzado = tipo;
        OnDatosActualizados?.Invoke();
        OnJuegoTerminado?.Invoke(tipo);
        Debug.Log($"<color=yellow>[GameManager] ¡JUEGO TERMINADO! Desenlace: {tipo}</color> (Dinero: S/ {Dinero:F0}, Reputación: {Reputacion:F0}%)");
    }
}
