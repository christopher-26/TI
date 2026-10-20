using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Gestiona la fila de clientes en la ventanilla de retiro y pedidos.
/// - Genera cápsulas representando clientes formados frente a la ventanilla.
/// - La cantidad de clientes en la fila aumenta dinámicamente cuando hay incidentes activos.
/// - Los clientes cambian a color rojo si la Reputación cae por debajo de 40.
/// - Los clientes adquieren un color permanentemente enojado (morado/magenta) si la flag clientesMolestos es verdadera.
/// </summary>
public class ClientesFila : MonoBehaviour
{
    [Header("Configuración de Fila")]
    [Tooltip("Posición inicial del primer cliente en la ventanilla (espacio exterior).")]
    public Vector3 posicionInicial = new Vector3(0f, 0.85f, -3.1f);

    [Tooltip("Separación entre clientes sucesivos en la fila.")]
    public float distanciaEntreClientes = 0.70f;

    [Tooltip("Capacidad máxima de clientes en la fila visual.")]
    public int maxClientes = 7;

    [Header("Materiales de Estado")]
    [Tooltip("Material para clientes en estado normal y tranquilo.")]
    public Material matNormal;

    [Tooltip("Material rojo cuando la reputación del negocio cae por debajo de 40.")]
    public Material matRojoEnojado;

    [Tooltip("Material distintivo (morado/magenta) cuando los clientes están permanentemente enfurecidos.")]
    public Material matMolestoPermanente;

    // Lista interna de GameObjects de clientes instanciados
    private List<GameObject> clientesInstanciados = new List<GameObject>();
    private List<TextMeshPro> etiquetasEstado = new List<TextMeshPro>();
    private List<Vector3> posicionesBase = new List<Vector3>();

    private void Awake()
    {
        AsegurarMaterialesPorDefecto();
        ConstruirFilaInicial();
    }

    private void Update()
    {
        ActualizarFila();
        AnimarClientes();
    }

    /// <summary>
    /// Construye la piscina de cápsulas de clientes en la fila.
    /// </summary>
    private void ConstruirFilaInicial()
    {
        for (int i = 0; i < maxClientes; i++)
        {
            // Ligera variación en X para una apariencia humana más orgánica
            float offsetX = (i % 2 == 0) ? 0.04f : -0.04f;
            Vector3 posBase = posicionInicial + new Vector3(offsetX, 0f, -distanciaEntreClientes * i);
            posicionesBase.Add(posBase);

            GameObject cliente = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cliente.name = $"Cliente_Fila_{i + 1}";
            cliente.transform.SetParent(transform, false);
            cliente.transform.position = posBase;
            cliente.transform.localScale = new Vector3(0.48f, 0.75f, 0.48f);

            // Orientar al cliente mirando hacia la ventanilla (+Z)
            cliente.transform.rotation = Quaternion.identity;

            MeshRenderer mr = cliente.GetComponent<MeshRenderer>();
            if (mr != null && matNormal != null)
            {
                mr.sharedMaterial = matNormal;
            }

            // Etiqueta flotante sobre la cabeza del cliente
            GameObject etiquetaGO = new GameObject("Etiqueta_Cliente");
            etiquetaGO.transform.SetParent(cliente.transform, false);
            etiquetaGO.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            etiquetaGO.transform.localRotation = Quaternion.Euler(0, 180, 0); // Mirando al técnico desde la ventanilla
            TextMeshPro tmp = etiquetaGO.AddComponent<TextMeshPro>();
            tmp.text = "<size=60%><color=#94A3B8>Esperando</color></size>";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 1.6f;
            tmp.color = Color.white;
            tmp.rectTransform.sizeDelta = new Vector2(2f, 0.4f);

            cliente.SetActive(false);
            clientesInstanciados.Add(cliente);
            etiquetasEstado.Add(tmp);
        }
    }

    /// <summary>
    /// Ajusta la cantidad de clientes activos y sus colores según el estado de GameManager.
    /// </summary>
    private void ActualizarFila()
    {
        if (GameManager.Instance == null) return;

        GameManager gm = GameManager.Instance;

        // 1. Calcular cuántos clientes deben estar en la fila según incidentes activos
        int incidentesActivosCount = gm.incidentesActivos != null ? gm.incidentesActivos.Count : 0;
        int clientesDeseados;

        if (gm.juegoTerminado && gm.finalAlcanzado == TipoFinal.NegocioSalvado)
        {
            // Negocio salvado: la fila se despeja rápidamente
            clientesDeseados = 1;
        }
        else if (gm.juegoTerminado && gm.finalAlcanzado == TipoFinal.Quiebra)
        {
            // Quiebra: fila llena reclamando
            clientesDeseados = maxClientes;
        }
        else
        {
            // Base 1 cliente si no hay fallos, + 2 clientes por cada fallo activo
            clientesDeseados = Mathf.Clamp(1 + incidentesActivosCount * 2, 1, maxClientes);
        }

        // 2. Determinar el material y texto según el estado de satisfacción del cliente
        Material matActual = matNormal;
        string textoEstado = "<size=60%><color=#94A3B8>Esperando pedido</color></size>";

        if (gm.clientesMolestos)
        {
            // Pérdida masiva de datos: permanentemente enojados con color morado/magenta
            matActual = matMolestoPermanente;
            textoEstado = "<size=65%><color=#E879F9><b>¡¿DÓNDE ESTÁN MIS DATOS?!</b></color></size>";
        }
        else if (gm.Reputacion < 40f)
        {
            // Reputación baja (< 40): clientes rojos enfurecidos por demoras
            matActual = matRojoEnojado;
            textoEstado = "<size=65%><color=#EF4444><b>¡DEMASIADA DEMORA!</b></color></size>";
        }

        // 3. Activar/desactivar cápsulas y aplicar color
        for (int i = 0; i < clientesInstanciados.Count; i++)
        {
            bool debeEstarActivo = i < clientesDeseados;
            GameObject cli = clientesInstanciados[i];

            if (cli.activeSelf != debeEstarActivo)
            {
                cli.SetActive(debeEstarActivo);
            }

            if (debeEstarActivo)
            {
                MeshRenderer mr = cli.GetComponent<MeshRenderer>();
                if (mr != null && mr.sharedMaterial != matActual)
                {
                    mr.sharedMaterial = matActual;
                }

                if (etiquetasEstado[i] != null)
                {
                    etiquetasEstado[i].text = textoEstado;
                }
            }
        }
    }

    /// <summary>
    /// Aplica una suave oscilación vertical para simular respiración y оживление de los clientes en VR.
    /// </summary>
    private void AnimarClientes()
    {
        for (int i = 0; i < clientesInstanciados.Count; i++)
        {
            if (clientesInstanciados[i] != null && clientesInstanciados[i].activeSelf)
            {
                float desfase = i * 0.7f;
                float oscilacion = Mathf.Sin(Time.time * 2.2f + desfase) * 0.02f;
                Vector3 p = posicionesBase[i];
                p.y += oscilacion;
                clientesInstanciados[i].transform.position = p;
            }
        }
    }

    private void AsegurarMaterialesPorDefecto()
    {
        if (matNormal == null)
        {
            matNormal = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            matNormal.color = new Color(0.25f, 0.55f, 0.90f); // Azul tranquilo
        }

        if (matRojoEnojado == null)
        {
            matRojoEnojado = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            matRojoEnojado.color = new Color(0.92f, 0.20f, 0.20f); // Rojo enfadado
        }

        if (matMolestoPermanente == null)
        {
            matMolestoPermanente = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            matMolestoPermanente.color = new Color(0.55f, 0.15f, 0.75f); // Morado/Magenta enfurecido
        }
    }
}
