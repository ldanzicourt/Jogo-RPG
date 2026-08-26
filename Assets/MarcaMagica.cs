using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum EscolaMagica
{
    Evocacao,     // Dardo Arcano (Azul)
    Necromancia,  // Sifão de Alma (Roxo)
    Conjuracao    // Vórtice de Trama (Ciano / Dourado)
}

public class MarcaMagica : MonoBehaviour
{
    private Dictionary<EscolaMagica, float> marcasAtivas = new Dictionary<EscolaMagica, float>();
    public float duracaoPadrao = 8f;
    public float danoExplosaoAlvo = 70f;
    public float danoExplosaoArea = 40f;
    public float raioExplosao = 5f;

    private SistemaDeVida sistemaDeVida;
    private GameObject containerIndicadores;
    private Dictionary<EscolaMagica, GameObject> orbesIndicadores = new Dictionary<EscolaMagica, GameObject>();

    void Awake()
    {
        sistemaDeVida = GetComponent<SistemaDeVida>();
        if (sistemaDeVida == null) sistemaDeVida = GetComponentInParent<SistemaDeVida>();
        CriarIndicadoresVisuais();
    }

    void Update()
    {
        // Atualiza os cronômetros das marcas
        List<EscolaMagica> expiradas = new List<EscolaMagica>();

        var chaves = new List<EscolaMagica>(marcasAtivas.Keys);
        foreach (var escola in chaves)
        {
            marcasAtivas[escola] -= Time.deltaTime;
            if (marcasAtivas[escola] <= 0)
            {
                expiradas.Add(escola);
            }
        }

        foreach (var exp in expiradas)
        {
            marcasAtivas.Remove(exp);
        }

        AtualizarVisualIndicadores();
    }

    public void AdicionarMarca(EscolaMagica escola)
    {
        marcasAtivas[escola] = duracaoPadrao;
        AtualizarVisualIndicadores();

        // Se acumulou as 3 marcas diferentes -> DETONAÇÃO ARCANA!
        if (marcasAtivas.ContainsKey(EscolaMagica.Evocacao) &&
            marcasAtivas.ContainsKey(EscolaMagica.Necromancia) &&
            marcasAtivas.ContainsKey(EscolaMagica.Conjuracao))
        {
            ExecutarDetonacaoArcana();
        }
    }

    void ExecutarDetonacaoArcana()
    {
        marcasAtivas.Clear();
        AtualizarVisualIndicadores();

        // 1. Dano massivo no alvo principal
        if (sistemaDeVida != null)
        {
            sistemaDeVida.ReceberDano(danoExplosaoAlvo, new Color(0.9f, 0.4f, 1f)); // Roxo/dourado
        }

        // 2. Dano em área nos inimigos próximos
        Collider[] inimigosProximos = Physics.OverlapSphere(transform.position, raioExplosao);
        foreach (var hit in inimigosProximos)
        {
            if (hit.gameObject == gameObject || hit.transform.root == transform.root) continue;
            if (hit.CompareTag("Player") || hit.GetComponent<PlayerController>() != null) continue;

            SistemaDeVida vidaInimigo = hit.GetComponent<SistemaDeVida>();
            if (vidaInimigo != null)
            {
                vidaInimigo.ReceberDano(danoExplosaoArea, new Color(1f, 0.8f, 0.2f)); // Dourado
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direcao = (hit.transform.position - transform.position).normalized + Vector3.up * 0.2f;
                rb.AddForce(direcao * 2.0f, ForceMode.Impulse);
            }
        }

        // 3. Efeito visual da explosão
        CriarEfeitoExplosao();
    }

    void CriarEfeitoExplosao()
    {
        GameObject blast = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        blast.name = "ExplosaoArcana";
        blast.transform.position = transform.position + Vector3.up * 1f;
        blast.transform.localScale = Vector3.one * 0.5f;

        Collider c = blast.GetComponent<Collider>();
        if (c != null) Destroy(c);

        Renderer r = blast.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            m.color = new Color(0.9f, 0.2f, 1f, 0.7f);
            r.material = m;
        }

        StartCoroutine(AnimarExplosao(blast));
    }

    IEnumerator AnimarExplosao(GameObject blast)
    {
        float t = 0f;
        float duracao = 0.45f;
        Vector3 escalaInicial = Vector3.one * 0.5f;
        Vector3 escalaFinal = Vector3.one * (raioExplosao * 2f);

        while (t < duracao)
        {
            if (blast == null) yield break;
            t += Time.deltaTime;
            blast.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t / duracao);
            yield return null;
        }

        if (blast != null) Destroy(blast);
    }

    void CriarIndicadoresVisuais()
    {
        containerIndicadores = new GameObject("IndicadoresMarcas");
        containerIndicadores.transform.SetParent(transform);
        containerIndicadores.transform.localPosition = new Vector3(0f, 2.2f, 0f);

        // Orbe Evocação (Azul)
        CriarOrbeIndicador(EscolaMagica.Evocacao, new Vector3(-0.4f, 0f, 0f), new Color(0.2f, 0.7f, 1f));
        // Orbe Necromancia (Roxo)
        CriarOrbeIndicador(EscolaMagica.Necromancia, new Vector3(0f, 0.3f, 0f), new Color(0.8f, 0.2f, 1f));
        // Orbe Conjuração (Dourado/Ciano)
        CriarOrbeIndicador(EscolaMagica.Conjuracao, new Vector3(0.4f, 0f, 0f), new Color(0.2f, 1f, 0.8f));

        AtualizarVisualIndicadores();
    }

    void CriarOrbeIndicador(EscolaMagica escola, Vector3 offset, Color cor)
    {
        GameObject orbe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orbe.name = "Marca_" + escola.ToString();
        orbe.transform.SetParent(containerIndicadores.transform);
        orbe.transform.localPosition = offset;
        orbe.transform.localScale = Vector3.one * 0.25f;

        Collider col = orbe.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer rend = orbe.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = cor;
            rend.material = mat;
        }

        orbesIndicadores[escola] = orbe;
    }

    void AtualizarVisualIndicadores()
    {
        if (containerIndicadores == null) return;

        // Faz o container olhar para a câmera do jogador
        if (Camera.main != null)
        {
            containerIndicadores.transform.LookAt(containerIndicadores.transform.position + Camera.main.transform.forward);
        }

        foreach (var kvp in orbesIndicadores)
        {
            bool ativa = marcasAtivas.ContainsKey(kvp.Key);
            if (kvp.Value != null)
            {
                kvp.Value.SetActive(ativa);
                if (ativa)
                {
                    // Efeito pulsante
                    float pulso = 0.25f + Mathf.Sin(Time.time * 8f) * 0.05f;
                    kvp.Value.transform.localScale = Vector3.one * pulso;
                }
            }
        }
    }
}
