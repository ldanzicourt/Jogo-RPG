using System.Collections;
using UnityEngine;

public class TempestadeEstaticaManager : MonoBehaviour
{
    [Header("Luz Direcional (Clarões Celestes)")]
    public Light luzDirecional;
    public float intensidadeBase = 0.35f;
    public float intensidadeRelampago = 2.8f;
    public Color corBase = new Color(0.12f, 0.16f, 0.22f); // Céu noturno escuro alienígena
    public Color corRelampago = new Color(0.65f, 0.88f, 1.0f); // Azul/Ciano elétrico

    [Header("Frequência dos Relâmpagos")]
    public float intervaloMinimo = 4.0f;
    public float intervaloMaximo = 9.0f;

    [Header("Luz Ambiente")]
    public Color ambienteEscuro = new Color(0.08f, 0.10f, 0.14f);
    public Color ambienteRelampago = new Color(0.25f, 0.35f, 0.48f);

    private float proximoRelampago;

    void Awake()
    {
        if (luzDirecional == null)
        {
            Light[] todasLuzes = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in todasLuzes)
            {
                if (l.type == LightType.Directional)
                {
                    luzDirecional = l;
                    break;
                }
            }
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambienteEscuro;

        if (luzDirecional != null)
        {
            luzDirecional.color = corBase;
            luzDirecional.intensity = intensidadeBase;
        }

        proximoRelampago = Time.time + Random.Range(intervaloMinimo, intervaloMaximo);
    }

    void Update()
    {
        if (Time.time >= proximoRelampago)
        {
            proximoRelampago = Time.time + Random.Range(intervaloMinimo, intervaloMaximo);
            StartCoroutine(RotinaClarãoRelampago());
        }
    }

    IEnumerator RotinaClarãoRelampago()
    {
        int pulsoCount = Random.Range(2, 4); // Sequência de flashes rápidos típicos de tempestades elétricas
        for (int p = 0; p < pulsoCount; p++)
        {
            if (luzDirecional != null)
            {
                luzDirecional.color = corRelampago;
                luzDirecional.intensity = Random.Range(intensidadeRelampago * 0.7f, intensidadeRelampago);
            }
            RenderSettings.ambientLight = ambienteRelampago;

            yield return new WaitForSeconds(Random.Range(0.04f, 0.09f));

            if (luzDirecional != null)
            {
                luzDirecional.color = corBase;
                luzDirecional.intensity = intensidadeBase * 0.8f;
            }
            RenderSettings.ambientLight = ambienteEscuro;

            yield return new WaitForSeconds(Random.Range(0.03f, 0.07f));
        }

        // Clarão residual suave
        float t = 0f;
        float duracaoFim = 0.25f;
        while (t < duracaoFim)
        {
            t += Time.deltaTime;
            float lerp = 1f - (t / duracaoFim);
            if (luzDirecional != null)
            {
                luzDirecional.color = Color.Lerp(corBase, corRelampago, lerp * 0.3f);
                luzDirecional.intensity = Mathf.Lerp(intensidadeBase, intensidadeRelampago * 0.4f, lerp);
            }
            yield return null;
        }

        if (luzDirecional != null)
        {
            luzDirecional.color = corBase;
            luzDirecional.intensity = intensidadeBase;
        }
        RenderSettings.ambientLight = ambienteEscuro;
    }
}
