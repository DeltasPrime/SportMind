using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ======= Música =======
    [Header("Música")]
    [SerializeField] private AudioClip actionTrack;
    [SerializeField] private float actionVolume = 1f;

    // ======= Targets / Spawn =======
    [Header("Targets")]
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] public Vector3 spawnAreaCenter;
    [SerializeField] public Vector3 spawnAreaSize;
    [SerializeField] private float moveSpeedEasy = 1.5f;
    [SerializeField] private float moveSpeedHard = 3f;
    [SerializeField] private float moveDistance = 1.5f;
    [SerializeField] private int minTargetsInScene = 4;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip targetDestroySound;

    // ======= Duraciones =======
    [Header("Duraciones (segundos)")]
    [SerializeField] private float easyDuration = 60f;
    [SerializeField] private float hardDuration = 30f;

    // ======= UI =======
    [Header("UI")]
    [SerializeField] private GameObject menuEasyCanvas;         // desactivado al inicio
    [SerializeField] private GameObject menuHardCanvas;         // desactivado al inicio
    [SerializeField] private GameObject hudCanvas;              // tiempo+puntaje (único)
    [SerializeField] private GameObject postSurveyCanvasEasy;   // según dificultad
    [SerializeField] private GameObject postSurveyCanvasHard;   // según dificultad
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Audio Encuesta (mismo para ambas)")]
    [SerializeField] private AudioSource surveyAudioSource;     // un único AudioSource

    private enum Difficulty { None, Easy, Hard }
    private Difficulty selectedDifficulty = Difficulty.None;

    private float currentTime;
    private bool isTimerRunning = false;
    private float score = 0f;
    private float currentMoveSpeed = 2f;

    public float Score => score;
    public bool IsGameRunning => isTimerRunning;

    private void Start()
    {
        // Estado limpio al iniciar la escena (menús apagados; tú los prendes desde otro menú)
        SafeSet(menuEasyCanvas, false);
        SafeSet(menuHardCanvas, false);
        SafeSet(hudCanvas, false);
        SafeSet(postSurveyCanvasEasy, false);
        SafeSet(postSurveyCanvasHard, false);

        if (scoreText) scoreText.text = "Puntaje: 0";
        if (timerText) timerText.text = "Tiempo: 0";
    }

    // =========================================================
    // DIFICULTAD - llamados por los botones de cada menú
    // =========================================================
    public void OnSelectEasy()
    {
        selectedDifficulty = Difficulty.Easy;
        currentMoveSpeed = moveSpeedEasy;
        currentTime = easyDuration;
        BeginSession();
    }

    public void OnSelectHard()
    {
        selectedDifficulty = Difficulty.Hard;
        currentMoveSpeed = moveSpeedHard;
        currentTime = hardDuration;
        BeginSession();
    }

    // Oculta menús, enciende HUD, arranca música y juego
    private void BeginSession()
    {
        SafeSet(menuEasyCanvas, false);
        SafeSet(menuHardCanvas, false);

        SafeSet(postSurveyCanvasEasy, false);
        SafeSet(postSurveyCanvasHard, false);

        // Música de acción
        if (AudioManager.instance != null && actionTrack != null)
            AudioManager.instance.Play(actionTrack, actionVolume);

        StartGame();
    }

    // =========================================================
    // CICLO DE JUEGO
    // =========================================================
    public void StartGame()
    {
        SafeSet(hudCanvas, true);

        score = 0f;
        isTimerRunning = true;
        UpdateScoreDisplay();
        UpdateTimeDisplay();

        // Limpia old targets
        foreach (var t in GameObject.FindGameObjectsWithTag("Target"))
            Destroy(t);

        // Spawnea el pool mínimo
        EnsureMinimumTargets();
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        UpdateTimeDisplay();

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            EndGame();
        }
    }

    private void EndGame()
    {
        isTimerRunning = false;

        // Música ambiente por defecto
        if (AudioManager.instance != null && AudioManager.instance.defaultAmbience != null)
            AudioManager.instance.Play(AudioManager.instance.defaultAmbience, AudioManager.instance.defaultVolume);

        // Guarda puntaje en PlayerDataStore según dificultad
        var pds = PlayerDataStore.Instance;
        if (pds != null && pds.Current != null)
        {
            if (selectedDifficulty == Difficulty.Easy)
                pds.Current.shootingScoreEasy = score;
            else if (selectedDifficulty == Difficulty.Hard)
                pds.Current.shootingScoreHard = score;
        }

        // Limpia targets
        foreach (var t in GameObject.FindGameObjectsWithTag("Target"))
            Destroy(t);

        // HUD off; survey on según dificultad
        SafeSet(hudCanvas, false);

        if (selectedDifficulty == Difficulty.Easy)
        {
            SafeSet(postSurveyCanvasEasy, true);
            SafeSet(postSurveyCanvasHard, false);
        }
        else
        {
            SafeSet(postSurveyCanvasEasy, false);
            SafeSet(postSurveyCanvasHard, true);
        }

        // Reproduce el mismo audio de encuesta
        if (surveyAudioSource)
        {
            surveyAudioSource.Stop();
            surveyAudioSource.Play();
        }

        // listo para próxima sesión cuando actives menú otra vez
    }

    // =========================================================
    // PUNTAJE / TIEMPO
    // =========================================================
    public void PlayerScored(float add)
    {
        score += add;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay() { if (scoreText) scoreText.text = "Puntaje: " + score.ToString("0"); }
    private void UpdateTimeDisplay() { if (timerText) timerText.text = "Tiempo: " + Mathf.Max(0f, currentTime).ToString("0"); }

    // =========================================================
    // TARGETS
    // =========================================================
    public void SpawnNewTarget()
    {
        if (targetPrefab == null) return;

        Vector3 pos = new Vector3(
            Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2),
            Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2),
            Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2, spawnAreaCenter.z + spawnAreaSize.z / 2));

        var target = Instantiate(targetPrefab, pos, Quaternion.identity);
        target.tag = "Target";
        var mover = target.AddComponent<TargetMover>();
        mover.Initialize(currentMoveSpeed, moveDistance, spawnAreaCenter, spawnAreaSize);
    }

    private void EnsureMinimumTargets()
    {
        int count = GameObject.FindGameObjectsWithTag("Target").Length;
        while (count < minTargetsInScene)
        {
            SpawnNewTarget();
            count++;
        }
    }

    // Llamada desde Bullet.OnCollisionEnter(collision)
    public void HandleBulletHit(GameObject hitObject, GameObject bulletGO)
    {
        if (hitObject == null) return;

        if (hitObject.CompareTag("Target"))
        {
            if (explosionEffect)
                Destroy(Instantiate(explosionEffect, hitObject.transform.position, Quaternion.identity), 2f);

            if (targetDestroySound)
                AudioSource.PlayClipAtPoint(targetDestroySound, hitObject.transform.position);

            Destroy(hitObject);
            PlayerScored(1f);
            EnsureMinimumTargets();
        }

        // Destruye la bala al impactar algo (ajusta si quieres que penetre)
        if (bulletGO) Destroy(bulletGO);
    }

    // =========================================================
    // GIZMOS / HELPERS
    // =========================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }

    private void SafeSet(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }

    // ============== TargetMover interno (simple) ==============
    private class TargetMover : MonoBehaviour
    {
        private float speed, distance;
        private Vector3 startPos, direction;
        private Vector3 areaCenter, areaSize;

        public void Initialize(float moveSpeed, float moveDistance, Vector3 spawnCenter, Vector3 spawnSize)
        {
            speed = moveSpeed;
            distance = moveDistance;
            areaCenter = spawnCenter;
            areaSize = spawnSize;

            int axis = Random.Range(0, 2); // 0-x, 1-y

            if (axis == 0)
            {
                direction = Vector3.right;
                float px = Random.Range(-areaSize.x / 2 + distance, areaSize.x / 2 - distance);
                float py = Random.Range(-areaSize.y / 2, areaSize.y / 2);
                float pz = Random.Range(-areaSize.z / 2, areaSize.z / 2);
                startPos = areaCenter + new Vector3(px, py, pz);
            }
            else
            {
                direction = Vector3.up;
                float px = Random.Range(-areaSize.x / 2, areaSize.x / 2);
                float py = Random.Range(-areaSize.y / 2 + distance, areaSize.y / 2 - distance);
                float pz = Random.Range(-areaSize.z / 2, areaSize.z / 2);
                startPos = areaCenter + new Vector3(px, py, pz);
            }

            transform.position = startPos;
            if (Random.value > 0.5f) direction *= -1;
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGameRunning) return;
            float offset = Mathf.Sin(Time.time * speed) * distance;
            transform.position = startPos + direction * offset;
        }
    }
}

