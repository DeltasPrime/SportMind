using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

public class TeleportWithFade : MonoBehaviour
{
    [Header("XR")]
    public XROrigin xrOrigin;

    [Header("Fade")]
    public FadeCanvas fader;

    [Header("Destinos (posición de PIES)")]
    public Transform destination1;
    public Transform destination2;
    public Transform destination3;

    [Header("Opciones de orientación")]
    [Tooltip("Alinear solo el yaw (rotación horizontal). Si false, iguala toda la rotación del destino.")]
    public bool matchYawOnly = true;

    [Header("Ajustes finos")]
    [Tooltip("Esperita extra tras mover antes del Fade Out (s)")]
    public float extraAfterMove = 0.05f;

    [Tooltip("Pequeña corrección vertical si lo necesitas (+ sube, - baja)")]
    public float heightOffsetY = 0f;

    bool _isBusy;

    // Llama esto desde el botón pasando 1, 2 o 3
    public void TeleportToIndex(int index)
    {
        if (_isBusy || xrOrigin == null || fader == null) return;

        Transform dest = index == 1 ? destination1 :
                         index == 2 ? destination2 :
                         index == 3 ? destination3 : null;
        if (dest == null) return;

        StartCoroutine(TeleportSequence(dest.position, dest.rotation));
    }

    private IEnumerator TeleportSequence(Vector3 targetFeetPos, Quaternion targetRot)
    {
        _isBusy = true;

        // 1) Fade IN
        fader.StartFadeIn();
        yield return new WaitForSeconds(fader.defaultDuration);

        // 2) Mover XR Origin asegurando que el destino es "pies"
        SafeMoveFeetTo(targetFeetPos, targetRot);

        if (extraAfterMove > 0f)
            yield return new WaitForSeconds(extraAfterMove);

        // 3) Fade OUT
        fader.StartFadeOut();
        yield return new WaitForSeconds(fader.defaultDuration);

        _isBusy = false;
    }

    /// <summary>
    /// Coloca los PIES del XR Origin en targetFeetPos.
    /// Calcula la posición de cámara requerida: feet + offset actual de la cabeza.
    /// </summary>
    private void SafeMoveFeetTo(Vector3 targetFeetPos, Quaternion targetRot)
    {
        var cc = xrOrigin.GetComponent<CharacterController>();
        bool ccWasEnabled = false;
        if (cc != null) { ccWasEnabled = cc.enabled; cc.enabled = false; }

        // offset actual de la cabeza en el espacio del Origin (incluye altura)
        Vector3 headLocal = xrOrigin.CameraInOriginSpacePos; // (x, y, z) respecto al Origin
        // Rotación objetivo que se aplicará al rig (solo yaw o completa)
        Quaternion yawOrFull = matchYawOnly
            ? Quaternion.Euler(0f, targetRot.eulerAngles.y, 0f)
            : targetRot;

        // Ese offset debe rotarse con la orientación objetivo
        Vector3 rotatedHeadOffset = yawOrFull * headLocal;

        // Ajuste opcional de altura
        rotatedHeadOffset.y += heightOffsetY;

        // Para que los pies queden en targetFeetPos, la CÁMARA debe ir a:
        Vector3 targetCameraWorldPos = targetFeetPos + rotatedHeadOffset;

        // 1) Mueve la cámara al punto calculado (esto reubica el Origin correcto)
        xrOrigin.MoveCameraToWorldLocation(targetCameraWorldPos);

        // 2) Alinea la rotación del rig
        if (matchYawOnly)
        {
            var e = xrOrigin.transform.eulerAngles;
            e.y = targetRot.eulerAngles.y;
            xrOrigin.transform.eulerAngles = e;
        }
        else
        {
            xrOrigin.transform.rotation = targetRot;
        }

        if (cc != null) cc.enabled = ccWasEnabled;
    }
}