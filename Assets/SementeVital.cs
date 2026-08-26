using UnityEngine;

public class SementeVital : MonoBehaviour
{
    public float curaQuantidade = 15f;
    public float tempoDeVida = 12f;
    public float raioColeta = 1.6f;

    private float tempoCriacao;
    private Transform visualSemente;
    private Light luzSemente;
    private bool coletada = false;

    void Start()
    {
        tempoCriacao = Time.time;
        ConstruirVisualSemente();
    }

    void Update()
    {
        if (coletada) return;

        // Animação de rotação e flutuação da semente
        if (visualSemente != null)
        {
            visualSemente.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
            float flutuacao = Mathf.Sin((Time.time - tempoCriacao) * 3f) * 0.08f;
            visualSemente.localPosition = new Vector3(0f, 0.25f + flutuacao, 0f);
        }

        // Verifica proximidade do Jogador / Aliados
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            float distancia = Vector3.Distance(transform.position, player.transform.position);
            if (distancia <= raioColeta)
            {
                Coletar(player.gameObject);
                return;
            }
        }

        // Expira após o tempo de vida
        if (Time.time >= tempoCriacao + tempoDeVida)
        {
            Desaparecer();
        }
    }

    void Coletar(GameObject jogador)
    {
        coletada = true;

        StatusJogador status = jogador.GetComponent<StatusJogador>();
        if (status == null) status = jogador.GetComponentInParent<StatusJogador>();

        if (status != null)
        {
            status.Curar(curaQuantidade);
            Debug.Log("🌱 Semente Vital coletada! +" + curaQuantidade + " HP!");
        }

        CriarEfeitoColeta();
        Destroy(gameObject);
    }

    void Desaparecer()
    {
        Destroy(gameObject);
    }

    void CriarEfeitoColeta()
    {
        GameObject burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        burst.name = "BurstSemente";
        burst.transform.position = transform.position + Vector3.up * 0.5f;
        burst.transform.localScale = Vector3.one * 0.5f;
        Destroy(burst.GetComponent<Collider>());

        Renderer r = burst.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = new Color(0.2f, 1f, 0.4f, 0.8f);
            r.material = mat;
        }

        Destroy(burst, 0.35f);
    }

    void ConstruirVisualSemente()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

        GameObject objVisual = new GameObject("Visual");
        objVisual.transform.SetParent(transform, false);
        objVisual.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        visualSemente = objVisual.transform;

        // Núcleo da Semente (Gota Esmeralda)
        GameObject nucleo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nucleo.name = "NucleoSemente";
        nucleo.transform.SetParent(visualSemente, false);
        nucleo.transform.localScale = new Vector3(0.22f, 0.32f, 0.22f);
        Destroy(nucleo.GetComponent<Collider>());

        Renderer rendNucleo = nucleo.GetComponent<Renderer>();
        Material matVerde = new Material(litShader);
        matVerde.color = new Color(0.1f, 0.95f, 0.3f);
        if (matVerde.HasProperty("_EmissionColor"))
        {
            matVerde.EnableKeyword("_EMISSION");
            matVerde.SetColor("_EmissionColor", new Color(0.1f, 0.95f, 0.3f) * 1.5f);
        }
        rendNucleo.material = matVerde;

        // Folhas que brotam ao redor
        for (int i = 0; i < 3; i++)
        {
            float angulo = i * 120f;
            GameObject folha = GameObject.CreatePrimitive(PrimitiveType.Cube);
            folha.name = "Folha_" + i;
            folha.transform.SetParent(visualSemente, false);
            folha.transform.localPosition = Quaternion.Euler(0f, angulo, 0f) * new Vector3(0.12f, 0.05f, 0f);
            folha.transform.localRotation = Quaternion.Euler(25f, angulo, 0f);
            folha.transform.localScale = new Vector3(0.08f, 0.14f, 0.02f);
            Destroy(folha.GetComponent<Collider>());

            Renderer rendFolha = folha.GetComponent<Renderer>();
            Material matFolha = new Material(litShader);
            matFolha.color = new Color(0.2f, 0.8f, 0.1f);
            rendFolha.material = matFolha;
        }

        // Luz pulsante verde
        GameObject objLuz = new GameObject("LuzSemente");
        objLuz.transform.SetParent(visualSemente, false);
        luzSemente = objLuz.AddComponent<Light>();
        luzSemente.color = new Color(0.2f, 1f, 0.4f);
        luzSemente.range = 2.5f;
        luzSemente.intensity = 1.2f;
    }
}
