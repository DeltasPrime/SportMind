using UnityEngine;

[DefaultExecutionOrder(-1000)] // Se inicializa antes que el resto
public class PlayerDataStore : MonoBehaviour
{
    public static PlayerDataStore Instance { get; private set; }

    [Header("Jugador actual")]
    public PlayerData Current;

    [Header("Defecto")]
    public string defaultPlayerName = "Jugador";

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Inicializar vacio y desde cero
        Current = new PlayerData();
        Current.playerName = defaultPlayerName;
    }

    // Metodos ejemplo para setear algunos valores
    public void SetPlayerName(string name) => Current.playerName = name;
    public void SetSelectedSport(string sport) => Current.selectedSport = sport;
    public void SetGender(string g) => Current.gender = g;
    public void SetEmotionalState(string e) => Current.emotionalState = e;


    public void ResetAll()
    {
        Current = new PlayerData();
        Current.playerName = defaultPlayerName;
    }
}
