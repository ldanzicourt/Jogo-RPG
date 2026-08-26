using UnityEngine;

public class DardoArcano : MonoBehaviour
{
    [Header("Configurações do Dardo")]
    public float dano = 30f;
    public float tempoDeVida = 4f;
    public float velocidade = 55f;

    private bool jaAcertou = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Garante visualização luminosa
        ConfigurarVisual();
    }

    void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    void ConfigurarVisual()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "VisualDardo";
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.2f, 0.2f, 0.5f);

            Collider colVis = visual.GetComponent<Collider>();
            if (colVis != null) Destroy(colVis);

            Renderer rendVis = visual.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = new Color(0.1f, 0.6f, 1f); // Azul Arcano brilhante
            rendVis.material = mat;
        }

        // Adiciona uma luz pontual azul
        Light luz = GetComponent<Light>();
        if (luz == null)
        {
            luz = gameObject.AddComponent<Light>();
            luz.color = new Color(0.2f, 0.7f, 1f);
            luz.range = 3.5f;
            luz.intensity = 2f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        ProcessarImpacto(other.gameObject);
    }

    void OnCollisionEnter(Collision col)
    {
        ProcessarImpacto(col.gameObject);
    }

    void ProcessarImpacto(GameObject alvo)
    {
        if (jaAcertou) return;

        // Ignora o jogador
        if (alvo.CompareTag("Player") || alvo.GetComponent<PlayerController>() != null || alvo.GetComponentInParent<PlayerController>() != null)
        {
            return;
        }

        jaAcertou = true;

        SistemaDeVida vida = alvo.GetComponent<SistemaDeVida>();
        if (vida == null) vida = alvo.GetComponentInParent<SistemaDeVida>();

        if (vida != null)
        {
            // Dano com cor azul arcano
            vida.ReceberDano(dano, new Color(0.2f, 0.7f, 1f));

            // Aplica Marca de Evocação
            MarcaMagica marca = alvo.GetComponent<MarcaMagica>();
            if (marca == null) marca = alvo.GetComponentInParent<MarcaMagica>();
            if (marca == null) marca = alvo.AddComponent<MarcaMagica>();

            marca.AdicionarMarca(EscolaMagica.Evocacao);

            // Leve impacto físico
            Rigidbody rbInimigo = alvo.GetComponent<Rigidbody>();
            if (rbInimigo != null)
            {
                rbInimigo.AddForce(transform.forward * 0.8f, ForceMode.Impulse);
            }
        }

        CriarImpactoVisual();
        Destroy(gameObject);
    }

    void CriarImpactoVisual()
    {
        GameObject splash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        splash.name = "ImpactoDardo";
        splash.transform.position = transform.position;
        splash.transform.localScale = Vector3.one * 0.4f;

        Collider c = splash.GetComponent<Collider>();
        if (c != null) Destroy(c);

        Renderer r = splash.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            m.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            r.material = m;
        }

        Destroy(splash, 0.2f);
    }
}
