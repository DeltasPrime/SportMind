using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class APIManager : MonoBehaviour
{
    [Header("URL Base de tu API Gateway")]
    [Tooltip("URL base sin el endpoint específico (ej: https://abc123.execute-api.sa-east-1.amazonaws.com/prod)")]
    public string apiBaseUrl = "https://xc06hens94.execute-api.sa-east-1.amazonaws.com/prod";

    // URLs completas de los endpoints
    private string ApiDataUrl => $"{apiBaseUrl}/data";
    private string ApiDatesUrl => $"{apiBaseUrl}/data/dates";
    private string ApiModifyUrl => $"{apiBaseUrl}/data/modify";

    [Header("API Key de AWS (Requerida)")]
    [Tooltip("API Key hardcodeada en el código para evitar errores de tipeo")]
    private const string API_KEY = "fyrKCyfgZ98gk6Y53tmBi1Z9fCsXG1U37FUDrCIv";
    private const string API_KEY_HEADER_NAME = "x-api-key";

    [Header("UI Elements - Respuesta Exitosa")]
    [Tooltip("TextMeshPro para mostrar mensaje de éxito")]
    [SerializeField] private TextMeshProUGUI successText;
    [Tooltip("Imagen para mostrar cuando la respuesta es exitosa")]
    [SerializeField] private Image successImage;

    [Header("UI Elements - Respuesta de Error")]
    [Tooltip("TextMeshPro para mostrar mensaje de error")]
    [SerializeField] private TextMeshProUGUI errorText;
    [Tooltip("Imagen para mostrar cuando hay un error")]
    [SerializeField] private Image errorImage;

    [Header("Gestión de Menús")]
    [Tooltip("Menú que se desactivará (ej: menú de recomendación final)")]
    [SerializeField] private GameObject menuActual;
    [Tooltip("Menú que se activará (ej: menú donde aparecerá el mensaje de respuesta)")]
    [SerializeField] private GameObject menuSiguiente;
    [Tooltip("Tiempo de espera en segundos antes de enviar datos a la API (por defecto 1 segundo)")]
    [SerializeField] private float tiempoEspera = 1f;

    // Llamar esto desde un bot�n: enviar� el CURRENT player
    public void SendCurrentPlayerToAPI()
    {
        StartCoroutine(PostCurrentPlayerCoroutine((response) =>
        {
            Debug.Log("Respuesta de la API: " + response);
            ProcessAPIResponse(response);
        }));
    }

    /// <summary>
    /// Cambia de menú, espera un tiempo y luego envía los datos a la API.
    /// Llamar este método desde un botón en lugar de SendCurrentPlayerToAPI directamente.
    /// </summary>
    public void CambiarMenuYEnviarDatos()
    {
        // IMPORTANTE: Primero activar el menú siguiente (que puede contener este APIManager)
        // antes de desactivar el menú actual, para evitar que el GameObject se desactive
        if (menuSiguiente != null)
        {
            menuSiguiente.SetActive(true);
            Debug.Log("[APIManager] Menú siguiente activado");
        }

        // Asegurarse de que este GameObject esté activo antes de iniciar la corrutina
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[APIManager] El GameObject del APIManager no está activo. Activándolo...");
            gameObject.SetActive(true);
        }

        // Ahora podemos iniciar la corrutina de forma segura
        StartCoroutine(CambiarMenuYEnviarCoroutine());
    }

    /// <summary>
    /// Corrutina que cambia de menú, espera y luego envía los datos
    /// </summary>
    private IEnumerator CambiarMenuYEnviarCoroutine()
    {
        // Esperar un frame para que Unity inicialice los componentes del menú activado
        yield return null;

        // Desactivar el menú actual (ahora que el siguiente ya está activo)
        if (menuActual != null)
        {
            menuActual.SetActive(false);
            Debug.Log("[APIManager] Menú actual desactivado");
        }

        // Esperar el tiempo especificado
        yield return new WaitForSeconds(tiempoEspera);
        Debug.Log($"[APIManager] Espera de {tiempoEspera} segundos completada");

        // Enviar datos a la API
        SendCurrentPlayerToAPI();
    }

    private IEnumerator PostCurrentPlayerCoroutine(Action<string> onResponse)
    {
        // 1. Construir JSON desde PlayerDataStore.Current
        string jsonData = GetJsonDataFromCurrentPlayer();
        if (string.IsNullOrEmpty(jsonData))
        {
            onResponse?.Invoke("Error: PlayerDataStore o Current es null.");
            yield break;
        }

        // 2. Crear request POST
        string url = ApiDataUrl;
        Debug.Log($"[APIManager] Enviando POST a: {url}");
        Debug.Log($"[APIManager] JSON a enviar: {jsonData}");
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        
        // Agregar API Key (siempre requerida)
        request.SetRequestHeader(API_KEY_HEADER_NAME, API_KEY);
        Debug.Log($"[APIManager] API Key enviada: {API_KEY.Substring(0, 10)}...");

        // 3. Enviar
        yield return request.SendWebRequest();

        // 4. Revisar resultado
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[APIManager] Respuesta exitosa: {request.downloadHandler.text}");
            onResponse?.Invoke(request.downloadHandler.text);
        }
        else
        {
            string errorMessage = $"Error: {request.responseCode} - {request.error}";
            Debug.LogError($"[APIManager] {errorMessage}");
            Debug.LogError($"[APIManager] URL intentada: {url}");
            Debug.LogError($"[APIManager] Response headers: {request.GetResponseHeaders()}");
            if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
            {
                Debug.LogError($"[APIManager] Response body: {request.downloadHandler.text}");
            }
            onResponse?.Invoke(errorMessage);
        }
    }

    private IEnumerator GetDatesCoroutine(Action<string> onResponse)
    {
        UnityWebRequest request = UnityWebRequest.Get(ApiDatesUrl);
        
        // Agregar API Key (siempre requerida)
        request.SetRequestHeader(API_KEY_HEADER_NAME, API_KEY);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onResponse?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onResponse?.Invoke($"Error: {request.responseCode} - {request.error}");
        }
    }

    private IEnumerator GetDataByDateCoroutine(string date, Action<string> onResponse)
    {
        string url = $"{apiBaseUrl}/data/{date}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        
        // Agregar API Key (siempre requerida)
        request.SetRequestHeader(API_KEY_HEADER_NAME, API_KEY);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onResponse?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onResponse?.Invoke($"Error: {request.responseCode} - {request.error}");
        }
    }

    private IEnumerator ModifySessionCoroutine(string date, string sessionId, string additionalDataJson, Action<string> onResponse)
    {
        // Construir el JSON para modificar
        string modifyJson = $"{{\"date\":\"{date}\",\"session_id\":\"{sessionId}\",\"additional_data\":{additionalDataJson}}}";

        UnityWebRequest request = new UnityWebRequest(ApiModifyUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(modifyJson);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        // Agregar API Key (siempre requerida)
        request.SetRequestHeader(API_KEY_HEADER_NAME, API_KEY);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onResponse?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onResponse?.Invoke($"Error: {request.responseCode} - {request.error}");
        }
    }

    /// <summary>
    /// Obtiene las fechas disponibles en el servidor
    /// </summary>
    public void GetAvailableDates(Action<string> onResponse)
    {
        StartCoroutine(GetDatesCoroutine(onResponse));
    }

    /// <summary>
    /// Obtiene los datos de una fecha específica
    /// </summary>
    public void GetDataByDate(string date, Action<string> onResponse)
    {
        StartCoroutine(GetDataByDateCoroutine(date, onResponse));
    }

    /// <summary>
    /// Modifica una sesión existente agregando información adicional
    /// </summary>
    public void ModifySession(string date, string sessionId, string additionalDataJson, Action<string> onResponse)
    {
        StartCoroutine(ModifySessionCoroutine(date, sessionId, additionalDataJson, onResponse));
    }

    /// <summary>
    /// Convierte PlayerDataStore.Instance.Current a JSON.
    /// </summary>
    private string GetJsonDataFromCurrentPlayer()
    {
        if (PlayerDataStore.Instance == null)
        {
            Debug.LogError("PlayerDataStore.Instance es null. Aseg�rate de que exista en la primera escena.");
            return null;
        }

        if (PlayerDataStore.Instance.Current == null)
        {
            Debug.LogError("PlayerDataStore.Instance.Current es null. Aseg�rate de inicializarlo antes de enviar.");
            return null;
        }

        // IMPORTANTE: tu clase PlayerData debe tener [System.Serializable]
        string json = JsonUtility.ToJson(PlayerDataStore.Instance.Current);
        Debug.Log("JSON enviado: " + json);
        return json;
    }

    /// <summary>
    /// Procesa la respuesta de la API y muestra los mensajes correspondientes en la UI
    /// </summary>
    private void ProcessAPIResponse(string response)
    {
        // Intentar deserializar la respuesta JSON
        try
        {
            APIResponse apiResponse = JsonUtility.FromJson<APIResponse>(response);
            
            if (apiResponse != null && apiResponse.success)
            {
                // Respuesta exitosa
                ShowSuccessMessage(apiResponse.message, apiResponse.session_id);
            }
            else
            {
                // Respuesta con error o success = false
                string errorMsg = apiResponse != null ? apiResponse.message : "Error desconocido";
                string sessionId = apiResponse != null ? apiResponse.session_id : "";
                ShowErrorMessage(errorMsg, sessionId);
            }
        }
        catch (Exception ex)
        {
            // Si no se puede parsear como JSON, verificar si es un error de red
            if (response.StartsWith("Error:"))
            {
                ShowErrorMessage(response, "");
            }
            else
            {
                Debug.LogError($"[APIManager] Error al procesar respuesta: {ex.Message}");
                ShowErrorMessage("Error al procesar la respuesta del servidor", "");
            }
        }
    }

    /// <summary>
    /// Muestra el mensaje de éxito y activa la imagen correspondiente
    /// </summary>
    private void ShowSuccessMessage(string message, string sessionId)
    {
        // Desactivar elementos de error
        if (errorText != null) errorText.gameObject.SetActive(false);
        if (errorImage != null) errorImage.gameObject.SetActive(false);

        // Activar y configurar elementos de éxito
        if (successText != null)
        {
            string sessionIdShort = GetShortSessionId(sessionId);
            successText.text = $"{message}\nID Sesión: {sessionIdShort}";
            successText.gameObject.SetActive(true);
        }

        if (successImage != null)
        {
            successImage.gameObject.SetActive(true);
        }

        Debug.Log($"[APIManager] Éxito: {message} - Session ID: {sessionId}");
    }

    /// <summary>
    /// Muestra el mensaje de error y activa la imagen correspondiente
    /// </summary>
    private void ShowErrorMessage(string message, string sessionId)
    {
        // Desactivar elementos de éxito
        if (successText != null) successText.gameObject.SetActive(false);
        if (successImage != null) successImage.gameObject.SetActive(false);

        // Activar y configurar elementos de error
        if (errorText != null)
        {
            string sessionIdShort = GetShortSessionId(sessionId);
            string errorMessage = string.IsNullOrEmpty(sessionIdShort) 
                ? message 
                : $"{message}\nID Sesión: {sessionIdShort}";
            errorText.text = errorMessage;
            errorText.gameObject.SetActive(true);
        }

        if (errorImage != null)
        {
            errorImage.gameObject.SetActive(true);
        }

        Debug.LogError($"[APIManager] Error: {message} - Session ID: {sessionId}");
    }

    /// <summary>
    /// Obtiene los primeros 8 caracteres del session_id
    /// </summary>
    private string GetShortSessionId(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return "";

        return sessionId.Length >= 8 ? sessionId.Substring(0, 8) : sessionId;
    }

    /// <summary>
    /// Clase para deserializar la respuesta JSON de la API
    /// </summary>
    [System.Serializable]
    private class APIResponse
    {
        public string message;
        public string filename;
        public string date_folder;
        public string s3_key;
        public string session_id;
        public bool success;
    }
}
