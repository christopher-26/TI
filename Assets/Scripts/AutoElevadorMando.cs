#pragma warning disable CS0618 // Desactivar aviso de obsolescencia de XRDeviceSimulator en XRI 3.x

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

/// <summary>
/// Script de conveniencia para pruebas en escritorio con el XR Device Simulator.
/// Al iniciar el juego, eleva automáticamente el mando derecho desde el suelo a la altura
/// natural de la mano (1.20 m) y lo deja activo, de modo que el jugador pueda apuntar
/// y usar el rayo láser de inmediato sin tener que levantarlo manualmente.
/// </summary>
public class AutoElevadorMando : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Esperar a que el simulador termine de crear e inicializar los dispositivos
        yield return null;
        yield return null;

        ElevarMandoDerecho();
    }

    /// <summary>
    /// Invoca los métodos internos del simulador para activar el mando derecho y levantarlo del piso.
    /// </summary>
    public void ElevarMandoDerecho()
    {
        XRDeviceSimulator sim = GetComponent<XRDeviceSimulator>() ?? UnityEngine.Object.FindFirstObjectByType<XRDeviceSimulator>();
        if (sim == null) return;

        try
        {
            // 1. Activar / Seleccionar el mando derecho automáticamente (equivalente a pulsar la tecla 'Y')
            MethodInfo toggleMethod = typeof(XRDeviceSimulator).GetMethod(
                "OnToggleManipulateRightPerformed",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (toggleMethod != null)
            {
                toggleMethod.Invoke(sim, new object[] { default(UnityEngine.InputSystem.InputAction.CallbackContext) });
            }

            // 2. Asignar la posición del mando derecho a la altura de la mano (1.20 m del piso)
            FieldInfo fieldState = typeof(XRDeviceSimulator).GetField(
                "m_RightControllerState",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldState != null)
            {
                object state = fieldState.GetValue(sim);
                if (state != null)
                {
                    FieldInfo posField = state.GetType().GetField("devicePosition");
                    if (posField != null)
                    {
                        // Posición cómoda: altura 1.20 m, ligeramente a la derecha (+0.25 m) y al frente (+0.45 m)
                        posField.SetValue(state, new Vector3(0.25f, 1.20f, 0.45f));
                        fieldState.SetValue(sim, state);
                        Debug.Log("<color=green>[AutoElevadorMando]</color> Mando derecho elevado automáticamente a 1.20 m de altura.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[AutoElevadorMando] Aviso: {ex.Message}");
        }
    }
}
