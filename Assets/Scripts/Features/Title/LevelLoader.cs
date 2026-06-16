using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    [Header("UI de Carga")]
    public GameObject panelCarga;
    public TextMeshProUGUI textoProgreso;
    
    [Tooltip("El texto de la UI donde se va a mostrar el tip de historia.")]
    public TextMeshProUGUI textoTip; // 🌟 Agregamos la referencia al nuevo texto

    [Header("Configuración")]
    [Tooltip("Qué tan rápido sube el porcentaje. Poné 0.2 para unos 5 segundos.")]
    public float velocidadCargaVisual = 0.2f; 

    // 🌟 Acá están tus frases guardadas
    private string[] tipsLore = new string[]
    {
        "\"El bosque de Garmanar guarda secretos que ni la luz del sol puede revelar...\"",
        "\"La Gema del Sol no solo es poder; es una advertencia.\"",
        "\"Aman no busca venganza, busca recuperar lo que le fue arrebatado.\"",
        "\"Cuidado con las sombras que se mueven entre los árboles...\""
    };

    public void CargarNivel(int indiceEscena)
    {
        // 🌟 Antes de prender el panel, elegimos un tip al azar y se lo pasamos a la UI
        if (textoTip != null)
        {
            int tipAleatorio = Random.Range(0, tipsLore.Length);
            textoTip.text = tipsLore[tipAleatorio];
        }

        panelCarga.SetActive(true);
        StartCoroutine(CargarAsincrono(indiceEscena));
    }

    IEnumerator CargarAsincrono(int indiceEscena)
    {
        AsyncOperation operacion = SceneManager.LoadSceneAsync(indiceEscena);
        operacion.allowSceneActivation = false;

        float progresoVisual = 0f;

        while (!operacion.isDone)
        {
            float progresoReal = Mathf.Clamp01(operacion.progress / 0.9f);
            progresoVisual = Mathf.MoveTowards(progresoVisual, progresoReal, velocidadCargaVisual * Time.deltaTime);

            float porcentaje = progresoVisual * 100f;
            textoProgreso.text = "Cargando... " + porcentaje.ToString("F0") + "%";

            if (progresoVisual >= 1f)
            {
                operacion.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}