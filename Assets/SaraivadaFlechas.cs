using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaraivadaFlechas : MonoBehaviour
{
    [Header("Configurações da Saraivada")]
    public float raioArea = 12f;
    public float alturaCilindro = 6f;
    public float duracao = 2.5f;
    public float intervaloTickDano = 0.25f;
    public float danoPorTick = 18f;

    private LineRenderer anelVisual;
    private float tempoFim;
    private float proximoTickDano;
    private Material matFlechasEspectrais;

    void Awake()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
        matFlechasEspectrais = new Material(litShader);
        Color corEspectral = new Color(0.2f, 0.95f, 0.65f, 0.85f); // Verde/Ciano Espectral
        matFlechasEspectrais.color = corEspectral;

        if (matFlechasEspectrais.HasProperty("_EmissionColor"))
        {
            matFlechasEspectrais.EnableKeyword("_EMISSION");
            matFlechasEspectrais.SetColor("_EmissionColor", corEspectral * 2.0f);
        }

        CriarAnelTatico(litShader, corEspectral);
    }

    void Start()
    {
        tempoFim = Time.time + duracao;
        proximoTickDano = Time.time;
        StartCoroutine(RotinaChuvaDeFlechas());
        Destroy(gameObject, duracao + 0.5f);
    }

    void Update()
    {
        if (Time.time >= tempoFim) return;

        // Pulso de dano periódico para todos os inimigos no cilindro de 12m raio e 6m altura
        if (Time.time >= proximoTickDano)
        {
            proximoTickDano = Time.time + intervaloTickDano;
            AplicarDanoArea();
        }

        // Animação de pulso no anel tático
        if (anelVisual != null)
        {
            float alpha = 0.5f + Mathf.Sin(Time.time * 8f) * 0.25f;
            Color corAtual = new Color(0.2f, 0.95f, 0.65f, alpha);
            anelVisual.startColor = corAtual;
            anelVisual.endColor = corAtual;
        }
    }

    void CriarAnelTatico(Shader shader, Color cor)
    {
        GameObject anelObj = new GameObject("AnelTaticoSaraivada");
        anelObj.transform.SetParent(transform, false);
        anelObj.transform.localPosition = Vector3.up * 0.05f;

        anelVisual = anelObj.AddComponent<LineRenderer>();
        anelVisual.useWorldSpace = false;
        anelVisual.loop = true;
        anelVisual.positionCount = 48;
        anelVisual.startWidth = 0.18f;
        anelVisual.endWidth = 0.18f;

        Material matLinha = new Material(shader);
        matLinha.color = cor;
        anelVisual.material = matLinha;

        for (int i = 0; i < 48; i++)
        {
            float angulo = (i / 48f) * Mathf.PI * 2f;
            float x = Mathf.Cos(angulo) * raioArea;
            float z = Mathf.Sin(angulo) * raioArea;
            anelVisual.SetPosition(i, new Vector3(x, 0f, z));
        }
    }

    IEnumerator RotinaChuvaDeFlechas()
    {
        while (Time.time < tempoFim)
        {
            // Instancia 4 a 6 flechas por frame em posições aleatórias no cilindro
            int qtdFlechasFrame = Random.Range(4, 7);
            for (int i = 0; i < qtdFlechasFrame; i++)
            {
                Vector2 pos2D = Random.insideUnitCircle * (raioArea * 0.95f);
                Vector3 posSpawn = transform.position + new Vector3(pos2D.x, alturaCilindro + Random.Range(0f, 3f), pos2D.y);

                StartCoroutine(AnimarQuedaFlecha(posSpawn));
            }

            yield return new WaitForSeconds(0.04f); // ~25 rajadas por segundo = centenas de flechas
        }
    }

    IEnumerator AnimarQuedaFlecha(Vector3 origem)
    {
        GameObject flecha = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flecha.name = "FlechaSaraivada";
        flecha.transform.position = origem;
        flecha.transform.localScale = new Vector3(0.04f, 0.45f, 0.04f);
        flecha.transform.rotation = Quaternion.Euler(Random.Range(80f, 100f), Random.Range(0f, 360f), 0f);
        Destroy(flecha.GetComponent<Collider>());

        Renderer rend = flecha.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = matFlechasEspectrais;
        }

        Vector3 destino = new Vector3(origem.x + Random.Range(-0.5f, 0.5f), transform.position.y, origem.z + Random.Range(-0.5f, 0.5f));
        float velocidade = Random.Range(28f, 42f);
        float distanciaTotal = Vector3.Distance(origem, destino);
        float tempoQueda = distanciaTotal / velocidade;

        float t = 0f;
        while (t < tempoQueda)
        {
            if (flecha == null) yield break;
            t += Time.deltaTime;
            flecha.transform.position = Vector3.Lerp(origem, destino, t / tempoQueda);
            yield return null;
        }

        if (flecha != null)
        {
            Destroy(flecha);
        }
    }

    void AplicarDanoArea()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, raioArea);
        HashSet<GameObject> atingidos = new HashSet<GameObject>();

        foreach (var col in hits)
        {
            if (col.CompareTag("Player") || col.GetComponent<PlayerController>() != null) continue;

            GameObject root = col.transform.root.gameObject;
            if (atingidos.Contains(root)) continue;
            atingidos.Add(root);

            // Verifica altura dentro do cilindro (6 metros)
            float deltaY = col.transform.position.y - transform.position.y;
            if (deltaY < -1f || deltaY > alturaCilindro) continue;

            SistemaDeVida vida = col.GetComponent<SistemaDeVida>();
            if (vida == null) vida = col.GetComponentInParent<SistemaDeVida>();

            if (vida != null)
            {
                float danoFinal = danoPorTick;

                // Bônus se tiver Marca do Caçador
                MarcaDoCacador marca = col.GetComponent<MarcaDoCacador>();
                if (marca == null) marca = col.GetComponentInParent<MarcaDoCacador>();
                if (marca != null)
                {
                    danoFinal += marca.RolarDanoBonus();
                }

                vida.ReceberDano(danoFinal, new Color(0.2f, 0.95f, 0.65f));
            }
        }
    }
}
