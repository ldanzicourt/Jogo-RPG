using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VorticeTrama : MonoBehaviour
{
    [Header("Configurações do Vórtice")]
    public float duracao = 3f;
    public float raio = 6.5f;
    public float forcaPuxao = 16f;
    public float danoPorSegundo = 15f;
    public float intervaloDano = 0.5f;

    private float tempoCriacao;
    private float proximoTickDano;
    private GameObject discoVisual;

    void Start()
    {
        tempoCriacao = Time.time;
        proximoTickDano = Time.time;
        CriarVisual();
        Destroy(gameObject, duracao);
    }

    void Update()
    {
        // Gira o disco no chão
        if (discoVisual != null)
        {
            discoVisual.transform.Rotate(Vector3.up * 180f * Time.deltaTime);
        }

        // Puxa inimigos para o centro continuamente
        Collider[] alvos = Physics.OverlapSphere(transform.position, raio);
        foreach (var col in alvos)
        {
            if (col.CompareTag("Player") || col.GetComponent<PlayerController>() != null || col.GetComponentInParent<PlayerController>() != null)
            {
                continue;
            }

            Vector3 vetorAoCentro = transform.position - col.transform.position;
            vetorAoCentro.y = 0f; // Puxão horizontal no chão
            float distancia = vetorAoCentro.magnitude;

            if (distancia > 0.3f)
            {
                Vector3 direcao = vetorAoCentro.normalized;
                float intensidade = Mathf.Lerp(forcaPuxao, forcaPuxao * 0.4f, distancia / raio);

                // Rigidbody pull
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(direcao * intensidade, ForceMode.Acceleration);
                }

                // CharacterController pull
                CharacterController cc = col.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.Move(direcao * intensidade * 0.4f * Time.deltaTime);
                }
            }
        }

        // Ticks periódicos de dano, lentidão e aplicação da marca de Conjuração
        if (Time.time >= proximoTickDano)
        {
            proximoTickDano = Time.time + intervaloDano;
            AplicarEfeitoArea();
        }
    }

    void AplicarEfeitoArea()
    {
        Collider[] alvos = Physics.OverlapSphere(transform.position, raio);
        HashSet<GameObject> processados = new HashSet<GameObject>();

        foreach (var col in alvos)
        {
            GameObject root = col.transform.root.gameObject;
            if (processados.Contains(root)) continue;
            processados.Add(root);

            if (col.CompareTag("Player") || col.GetComponent<PlayerController>() != null || col.GetComponentInParent<PlayerController>() != null)
            {
                continue;
            }

            SistemaDeVida vida = col.GetComponent<SistemaDeVida>();
            if (vida == null) vida = col.GetComponentInParent<SistemaDeVida>();

            if (vida != null)
            {
                float danoTick = danoPorSegundo * intervaloDano;
                vida.ReceberDano(danoTick, new Color(0.2f, 1f, 0.8f)); // Ciano/Esmeralda
                vida.AplicarLentidao(0.4f, 2f); // 40% de lentidão por 2s

                MarcaMagica marca = col.GetComponent<MarcaMagica>();
                if (marca == null) marca = col.GetComponentInParent<MarcaMagica>();
                if (marca == null) marca = col.gameObject.AddComponent<MarcaMagica>();

                marca.AdicionarMarca(EscolaMagica.Conjuracao);
            }
        }
    }

    void CriarVisual()
    {
        // Cria um disco plano luminoso no chão
        discoVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        discoVisual.name = "DiscoVortice";
        discoVisual.transform.SetParent(transform);
        discoVisual.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        discoVisual.transform.localScale = new Vector3(raio * 1.8f, 0.02f, raio * 1.8f);

        Collider c = discoVisual.GetComponent<Collider>();
        if (c != null) Destroy(c);

        Renderer rend = discoVisual.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = new Color(0.1f, 0.9f, 0.7f, 0.5f); // Ciano translúcido
            rend.material = mat;
        }

        // Luz no centro
        Light luz = gameObject.AddComponent<Light>();
        luz.color = new Color(0.2f, 1f, 0.8f);
        luz.range = raio * 1.5f;
        luz.intensity = 2.5f;
    }
}
