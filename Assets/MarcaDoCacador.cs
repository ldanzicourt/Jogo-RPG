using UnityEngine;

public class MarcaDoCacador : MonoBehaviour
{
    [Header("Configurações da Marca")]
    public float duracaoMarca = 15f;
    private float tempoExpiracao;
    private GameObject visualMarca;
    private Transform cameraJogador;

    void Start()
    {
        tempoExpiracao = Time.time + duracaoMarca;
        cameraJogador = Camera.main != null ? Camera.main.transform : null;
        CriarVisualMarca();
    }

    void Update()
    {
        if (Time.time >= tempoExpiracao)
        {
            RemoverMarca();
            return;
        }

        // Faz o glifo da marca olhar para a câmera do jogador e pulsar suavemente
        if (visualMarca != null && cameraJogador != null)
        {
            visualMarca.transform.LookAt(visualMarca.transform.position + cameraJogador.forward);
            float escalaPulso = 0.4f + Mathf.Sin(Time.time * 6f) * 0.05f;
            visualMarca.transform.localScale = Vector3.one * escalaPulso;
        }
    }

    public void RenovarDuracao()
    {
        tempoExpiracao = Time.time + duracaoMarca;
    }

    public int RolarDanoBonus()
    {
        // 1d6 = bônus clássico de 1 a 6 de dano
        return Random.Range(1, 7);
    }

    void CriarVisualMarca()
    {
        visualMarca = new GameObject("VisualMarcaDoCacador");
        visualMarca.transform.SetParent(transform, false);

        // Posiciona ligeiramente acima da cabeça do inimigo
        Collider col = GetComponent<Collider>();
        float altura = col != null ? col.bounds.extents.y * 2f + 0.5f : 2.2f;
        visualMarca.transform.localPosition = new Vector3(0f, altura, 0f);

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
        Material matGlifo = new Material(litShader);
        Color corOuro = new Color(1f, 0.78f, 0.15f, 0.9f);
        matGlifo.color = corOuro;

        if (matGlifo.HasProperty("_EmissionColor"))
        {
            matGlifo.EnableKeyword("_EMISSION");
            matGlifo.SetColor("_EmissionColor", corOuro * 2.5f);
        }

        // Anel central (mira do caçador)
        GameObject anel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        anel.name = "AnelMira";
        anel.transform.SetParent(visualMarca.transform, false);
        anel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        anel.transform.localScale = new Vector3(0.55f, 0.015f, 0.55f);
        Destroy(anel.GetComponent<Collider>());
        anel.GetComponent<Renderer>().material = matGlifo;

        // Ponto central de foco
        GameObject ponto = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ponto.name = "PontoCentro";
        ponto.transform.SetParent(visualMarca.transform, false);
        ponto.transform.localScale = Vector3.one * 0.15f;
        Destroy(ponto.GetComponent<Collider>());
        ponto.GetComponent<Renderer>().material = matGlifo;

        // Quatro marcadores cardeais da mira
        for (int i = 0; i < 4; i++)
        {
            GameObject traco = GameObject.CreatePrimitive(PrimitiveType.Cube);
            traco.name = "TracoMira_" + i;
            traco.transform.SetParent(visualMarca.transform, false);
            float angulo = i * 90f;
            traco.transform.localRotation = Quaternion.Euler(0f, 0f, angulo);
            traco.transform.localPosition = traco.transform.up * 0.35f;
            traco.transform.localScale = new Vector3(0.04f, 0.16f, 0.02f);
            Destroy(traco.GetComponent<Collider>());
            traco.GetComponent<Renderer>().material = matGlifo;
        }
    }

    public void RemoverMarca()
    {
        if (visualMarca != null)
        {
            Destroy(visualMarca);
        }
        Destroy(this);
    }

    void OnDestroy()
    {
        if (visualMarca != null)
        {
            Destroy(visualMarca);
        }
    }
}
