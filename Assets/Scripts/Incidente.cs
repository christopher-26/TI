using System;
using System.Collections.Generic;

/// <summary>
/// Clase simple que representa un problema o fallo técnico en la empresa.
/// </summary>
[Serializable]
public class Incidente
{
    // Nombre del fallo (ej. "Servidor Principal Caído", "Router Sin Conexión")
    public string nombre;

    // Cantidad de dinero que se drena por segundo mientras este incidente permanezca activo
    public float perdidaPorSegundo;

    // Estado del incidente (true si está causando pérdidas actualmente)
    public bool activo;

    // Segundo del juego en el que inició el incidente
    public float tiempoInicio;

    // Segundo del juego en el que fue resuelto (0 si no se ha resuelto)
    public float tiempoResuelto;

    // Descripción o detalle contextual del incidente (ej. "Borrado accidental ocurrido a las 10:30 a.m.")
    public string descripcion;

    // Lista de posibles alternativas de solución que el técnico puede elegir
    public List<OpcionSolucion> opciones = new List<OpcionSolucion>();

    public Incidente()
    {
        nombre = "Incidente sin nombre";
        descripcion = "";
        perdidaPorSegundo = 10f;
        activo = true;
        tiempoInicio = 0f;
        tiempoResuelto = 0f;
        opciones = new List<OpcionSolucion>();
    }

    public Incidente(string nombre, float perdidaPorSegundo, List<OpcionSolucion> opciones = null, string descripcion = "")
    {
        this.nombre = nombre;
        this.descripcion = descripcion;
        this.perdidaPorSegundo = perdidaPorSegundo;
        this.activo = true;
        this.tiempoInicio = 0f;
        this.tiempoResuelto = 0f;
        this.opciones = opciones ?? new List<OpcionSolucion>();
    }
}
