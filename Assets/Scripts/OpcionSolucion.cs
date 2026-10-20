using System;

/// <summary>
/// Tipo de resultado que produce una opción de solución sobre un incidente.
/// </summary>
public enum TipoResultado
{
    Correcta,
    Regular,
    Mala
}

/// <summary>
/// Representa una alternativa de solución seleccionable por el técnico para resolver un incidente.
/// </summary>
[Serializable]
public class OpcionSolucion
{
    // Texto descriptivo de la acción que se mostrará en el botón del panel
    public string texto;

    // Calificación del resultado de la acción (Correcta, Regular, Mala)
    public TipoResultado resultado;

    // Segundos que tarda en ejecutarse la acción (tiempo durante el cual corre la barra de progreso)
    public float penalizacionSegundos;

    // Cambio en los puntos de reputación del negocio tras aplicar la opción (ej. +5, -2, -10)
    public int deltaReputacion;

    // Indica si la opción soluciona efectivamente el incidente o no
    public bool resuelve;

    // Si es mayor a 0, el incidente volverá a activarse tras este número de segundos (parche temporal)
    public float reapareceTrasSegundos;

    // Mensaje de recomendación o retroalimentación técnica para el informe final
    public string mensajeInforme;

    // Si es verdadero, activa la flag clientesMolestos en GameManager al ejecutarse
    public bool setClientesMolestos = false;

    // Acción o callback opcional a ejecutar al aplicar la opción
    public Action alAplicar;

    public OpcionSolucion()
    {
        texto = "Opción de solución";
        resultado = TipoResultado.Correcta;
        penalizacionSegundos = 1f;
        deltaReputacion = 0;
        resuelve = true;
        reapareceTrasSegundos = 0f;
        mensajeInforme = "Sin observaciones.";
        setClientesMolestos = false;
    }

    public OpcionSolucion(
        string texto,
        TipoResultado resultado,
        float penalizacionSegundos,
        int deltaReputacion,
        bool resuelve,
        float reapareceTrasSegundos,
        string mensajeInforme,
        bool setClientesMolestos = false,
        Action alAplicar = null)
    {
        this.texto = texto;
        this.resultado = resultado;
        this.penalizacionSegundos = penalizacionSegundos;
        this.deltaReputacion = deltaReputacion;
        this.resuelve = resuelve;
        this.reapareceTrasSegundos = reapareceTrasSegundos;
        this.mensajeInforme = mensajeInforme;
        this.setClientesMolestos = setClientesMolestos;
        this.alAplicar = alAplicar;
    }
}
