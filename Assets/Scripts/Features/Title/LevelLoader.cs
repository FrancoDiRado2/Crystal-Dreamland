using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    [Header("UI de Carga")]
    public GameObject panelCarga;
    public TextMeshProUGUI textoProgreso;

    [Header("Configuración")]
    [Tooltip("Qué tan rápido sube el porcentaje. Poné 0.2 para unos 5 segundos.")]
    public float velocidadCargaVisual = 0.2f; 

    public void CargarNivel(int indiceEscena)
    {
        panelCarga.SetActive(true);
        StartCoroutine(CargarAsincrono(indiceEscena));
    }

    IEnumerator CargarAsincrono(int indiceEscena)
    {
        // Empezamos a cargar el nivel por detrás
        AsyncOperation operacion = SceneManager.LoadSceneAsync(indiceEscena);
        
        // Bloqueamos que Unity cambie de escena de golpe
        operacion.allowSceneActivation = false;

        float progresoVisual = 0f;

        while (!operacion.isDone)
        {
            float progresoReal = Mathf.Clamp01(operacion.progress / 0.9f);

            // Subimos nuestro progreso suavemente
            progresoVisual = Mathf.MoveTowards(progresoVisual, progresoReal, velocidadCargaVisual * Time.deltaTime);

            // Convertimos ese 0-1 a un 0-100 absoluto
            float porcentaje = progresoVisual * 100f;

            // Actualizamos SOLO el texto
            textoProgreso.text = "Loading... " + porcentaje.ToString("F0") + "%";

            // Si llegó a 100, entramos al nivel
            if (progresoVisual >= 1f)
            {
                operacion.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}