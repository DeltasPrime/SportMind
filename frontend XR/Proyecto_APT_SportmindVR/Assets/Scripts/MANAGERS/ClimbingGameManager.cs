using System.Collections;
using UnityEngine;

using Unity.XR.CoreUtils;
using TMPro;

public class ClimbingGameManager : MonoBehaviour
{
    public static ClimbingGameManager Instance { get; private set; }

    private enum Difficulty { None, Easy, Hard }

    [Header("Duración por Dificultad (segundos)")]
    [SerializeField] private float easyDuration = 60f;
    [SerializeField] private float hardDuration = 30f;

    [Header("UI / Menús")]
    [SerializeField] private GameObject menuEasyCanvas;   // Menú para Fácil (lo activas desde otro menú)
    [SerializeField] private GameObject menuHardCanvas;   // Menú para Difícil (lo activas desde otro menú)

    [Header("UI / Juego (mismo canvas para countdown + HUD)")]
    [SerializeField] private GameObject climbCanvas;      // Un solo canvas
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("UI / Encuestas")]
    [SerializeField] private GameObject surveyCanvasEasy;
    [SerializeField] private GameObject surveyCanvasHard;

    [Header("XR Origin / Teleport")]
    [SerializeField] private XROrigin xrOrigin;          // Asignar en inspector
    [SerializeField] private bool matchYawOnly = true;    // Alinear solo yaw por defecto
    [SerializeField] private float heightOffsetY = 0f;    // Ajuste opcional de altura

    [Header("Puntos de Inicio / Retorno")]
    [SerializeField] private Transform spawnEasyPoint;    // Inicio para Fácil (pies)
    [SerializeField] private Transform spawnHardPoint;    // Inicio para Difícil (pies)
    [SerializeField] private Transform returnPoint;       // Punto de retorno al terminar (pies)

    [Header("Música de Fondo")]
    [SerializeField] private AudioClip climbingTrack;
    [Range(0f, 1f)] [SerializeField] private float climbingVolume = 1f;

    [Header("Audio Encuesta (mismo para ambas)")]
    [SerializeField] private AudioSource surveyAudioSource;

    [Header("XRI Providers (opcional)")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbProvider climbProvider;                 // Arrástralo si lo usas (paquete samples)


    // Estado
    private Difficulty selectedDifficulty = Difficulty.None;
    private float currentTime = 0f;
    private bool isRunning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Todo apagado al iniciar esta escena (tú los activas desde otro menú)
        SafeSet(menuEasyCanvas, false);
        SafeSet(menuHardCanvas, false);
        SafeSet(climbCanvas, false);
        SafeSet(surveyCanvasEasy, false);
        SafeSet(surveyCanvasHard, false);

        if (countdownText) countdownText.text = string.Empty;
        if (timerText) timerText.text = string.Empty;

        if (xrOrigin == null) xrOrigin = FindObjectOfType<XROrigin>();
    }

    // === Botones UI (OnClick) ===
    public void OnSelectEasy()
    {
        selectedDifficulty = Difficulty.Easy;
        StartGameSequence(spawnEasyPoint);
    }

    public void OnSelectHard()
    {
        selectedDifficulty = Difficulty.Hard;
        StartGameSequence(spawnHardPoint);
    }

    private void StartGameSequence(Transform spawnPoint)
    {
        // Oculta ambos menús
        SafeSet(menuEasyCanvas, false);
        SafeSet(menuHardCanvas, false);

        // Apaga encuestas previamente
        SafeSet(surveyCanvasEasy, false);
        SafeSet(surveyCanvasHard, false);

        // Teletransporte robusto a spawn correspondiente (pies)
        if (spawnPoint != null)
            StartCoroutine(ForceReleaseThenTeleport(spawnPoint.position, spawnPoint.rotation));

        // Música escalada
        if (AudioManager.instance != null && climbingTrack != null)
            AudioManager.instance.Play(climbingTrack, climbingVolume);

        // Cuenta regresiva + arranque
        StartCoroutine(CountdownAndStart());
    }

    private IEnumerator CountdownAndStart()
    {
        SafeSet(climbCanvas, true);

        if (countdownText)
        {
            countdownText.text = "3";
            yield return new WaitForSeconds(1f);

            countdownText.text = "2";
            yield return new WaitForSeconds(1f);

            countdownText.text = "1";
            yield return new WaitForSeconds(1f);

            countdownText.text = "¡Escala!";
            yield return new WaitForSeconds(0.6f);
            countdownText.text = string.Empty;
        }

        currentTime = (selectedDifficulty == Difficulty.Easy) ? easyDuration : hardDuration;
        isRunning = true;

        UpdateTimerLabel();
    }

    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            UpdateTimerLabel();
            EndByTime();
            return;
        }

        UpdateTimerLabel();
    }

    private void UpdateTimerLabel()
    {
        if (!timerText) return;
        timerText.text = Mathf.Max(0f, currentTime).ToString("0.00");
    }

    private void EndByTime()
    {
        if (!isRunning) return;
        isRunning = false;
        StartCoroutine(EndRoutine());
    }

    public void EndByGoal()
    {
        if (!isRunning) return;
        isRunning = false;
        StartCoroutine(EndRoutine());
    }

    private IEnumerator EndRoutine()
    {
        SafeSet(climbCanvas, false);

        float duration = (selectedDifficulty == Difficulty.Easy) ? easyDuration : hardDuration;
        float elapsed = Mathf.Clamp(duration - currentTime, 0f, duration);
        float rounded = Mathf.Round(elapsed * 100f) / 100f;

        var pds = PlayerDataStore.Instance;
        if (pds != null && pds.Current != null)
        {
            if (selectedDifficulty == Difficulty.Easy)
                pds.Current.climbingTimeEasy = rounded;
            else if (selectedDifficulty == Difficulty.Hard)
                pds.Current.climbingTimeHard = rounded;
        }

        // Vuelve a punto de retorno (pies) con release forzado
        if (returnPoint != null)
            yield return StartCoroutine(ForceReleaseThenTeleport(returnPoint.position, returnPoint.rotation));

        // Música por defecto de tu AudioManager
        if (AudioManager.instance != null && AudioManager.instance.defaultAmbience != null)
            AudioManager.instance.Play(AudioManager.instance.defaultAmbience, AudioManager.instance.defaultVolume);

        yield return new WaitForSeconds(0.25f);

        // Activa encuesta según dificultad
        if (selectedDifficulty == Difficulty.Easy)
        {
            SafeSet(surveyCanvasEasy, true);
            SafeSet(surveyCanvasHard, false);
        }
        else
        {
            SafeSet(surveyCanvasEasy, false);
            SafeSet(surveyCanvasHard, true);
        }

        // Mismo audio para ambas encuestas
        if (surveyAudioSource)
        {
            surveyAudioSource.Stop();
            surveyAudioSource.Play();
        }

        // Reset
        selectedDifficulty = Difficulty.None;
    }

    // ========= TELETRANSPORTE ROBUSTO =========

    // Suelta todo (grab/selección) y reinicia ClimbProvider
    private void ForceReleaseAll()
    {
        // 1) Reinicia el ClimbProvider (evita quedarse “pegado”)
        if (climbProvider != null)
        {
            bool wasEnabled = climbProvider.enabled;
            climbProvider.enabled = false;
            climbProvider.enabled = wasEnabled; // toggle para resetear
        }

        // 2) Fuerza SelectExit en todos los interactores con selección
        var interactors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
        foreach (var interactor in interactors)
        {
            while (interactor.hasSelection && interactor.firstInteractableSelected != null)
            {
                var mgr = interactor.interactionManager;
                if (mgr != null)
                    mgr.SelectExit(interactor, interactor.firstInteractableSelected);
                else
                    break;
            }
        }
    }

    // Teletransporta con máxima robustez: suelta, espera física, pausa locomoción, mueve y restaura
    private IEnumerator ForceReleaseThenTeleport(Vector3 targetFeetPos, Quaternion targetRot)
    {
        ForceReleaseAll();

        // Espera un FixedUpdate para limpiar contactos antes de mover
        yield return new WaitForFixedUpdate();


        // Mueve por pies a la pose objetivo
        SafeMoveFeetTo(targetFeetPos, targetRot);

    }

    // ========= TU MÉTODO DE TELEPORT POR PIES =========
    private void SafeMoveFeetTo(Vector3 targetFeetPos, Quaternion targetRot)
    {
        if (xrOrigin == null) return;

        var cc = xrOrigin.GetComponent<CharacterController>();
        bool ccWasEnabled = false;
        if (cc != null) { ccWasEnabled = cc.enabled; cc.enabled = false; }

        // Offset actual de la cabeza en espacio del Origin
        Vector3 headLocal = xrOrigin.CameraInOriginSpacePos;

        // Rotación objetivo (solo yaw o completa)
        Quaternion yawOrFull = matchYawOnly
            ? Quaternion.Euler(0f, targetRot.eulerAngles.y, 0f)
            : targetRot;

        // Offset de cabeza rotado a la orientación objetivo
        Vector3 rotatedHeadOffset = yawOrFull * headLocal;

        // Ajuste opcional de altura
        rotatedHeadOffset.y += heightOffsetY;

        // Para que los pies queden en targetFeetPos, la CÁMARA debe ir a:
        Vector3 targetCameraWorldPos = targetFeetPos + rotatedHeadOffset;

        // 1) Mueve la cámara (reposiciona correctamente el Origin)
        xrOrigin.MoveCameraToWorldLocation(targetCameraWorldPos);

        // 2) Alinea rotación del rig
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
    // ==============================================

    private void SafeSet(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }
}
