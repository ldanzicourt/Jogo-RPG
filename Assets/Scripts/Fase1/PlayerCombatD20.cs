using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatD20 : MonoBehaviour
{
    [Header("Atributos D&D Nível 1")]
    public float pvMaximo = 12f;
    public float pvAtual = 12f;
    public int classeArmadura = 14; // CA do Jogador
    public int modificadorAtaque = 3; // +3 de Força/Destreza

    [Header("Parâmetros da Arma")]
    public float alcanceAtaque = 3.2f;
    public float cadenciaAtaque = 0.65f;
    public float raioCleave = 1.2f;

    [Header("Referências")]
    public CameraController3P camera3P;
    public Transform pontoDisparoArma;

    private float proximoAtaquePermitido = 0f;
    private bool atacando = false;
    private CharacterController characterController;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (camera3P == null) camera3P = FindAnyObjectByType<CameraController3P>();
        pvAtual = pvMaximo;
    }

    void Update()
    {
        if (Time.timeScale <= 0.001f || pvAtual <= 0f) return;

        // Se o jogador possui uma arma de classe ativa, deixa ela cuidar dos golpes
        if (GetComponentInChildren<AtaqueEspada>() != null ||
            GetComponentInChildren<ControleRanger>() != null ||
            GetComponentInChildren<ControleMago>() != null ||
            GetComponentInChildren<ControleDruida>() != null)
        {
            return;
        }

        // Ataque com Botão Esquerdo do Mouse (LMB) para modo genérico
        bool segurandoCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (Input.GetMouseButton(0) && !segurandoCtrl && Time.time >= proximoAtaquePermitido && !atacando)
        {
            proximoAtaquePermitido = Time.time + cadenciaAtaque;
            StartCoroutine(RotinaExecutarGolpe());
        }
    }

    IEnumerator RotinaExecutarGolpe()
    {
        atacando = true;

        // Animação procedural da espada
        StartCoroutine(AnimarGolpeEspada());

        // Efeito de balanço de arma/cleave visual
        CriarRastroAtaqueVisual();

        // 1. Raycast / Cone frontal na direção da câmera em 3ª pessoa
        Vector3 origem = transform.position + Vector3.up * 1.2f;
        Vector3 direcao = transform.forward;
        if (camera3P != null)
        {
            Vector3 pontoAlvo;
            direcao = camera3P.ObterDirecaoMira(alcanceAtaque + 2f, out pontoAlvo);
            // Faz o jogador virar na direção mirada durante o ataque
            Vector3 lookFlat = new Vector3(direcao.x, 0f, direcao.z).normalized;
            if (lookFlat.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(lookFlat);
            }
        }

        yield return new WaitForSeconds(0.12f); // Tempo de impacto do golpe

        Collider[] hits = Physics.OverlapSphere(origem + direcao * (alcanceAtaque * 0.5f), raioCleave);
        HashSet<GameObject> atingidos = new HashSet<GameObject>();

        foreach (var col in hits)
        {
            if (col.CompareTag("Player") || col.transform.root == transform.root) continue;

            EnemyAIBase inimigo = col.GetComponentInParent<EnemyAIBase>();
            if (inimigo != null)
            {
                if (atingidos.Contains(inimigo.gameObject)) continue;
                atingidos.Add(inimigo.gameObject);

                // Aplica o Cálculo D&D com d20 oculto
                ProcessarDanoD20Oculto(inimigo);
            }
        }

        yield return new WaitForSeconds(0.25f);
        atacando = false;
    }

    // Mecânica D&D com o d20 Oculto na Engine
    public void ProcessarDanoD20Oculto(EnemyAIBase alvo)
    {
        if (alvo == null || !alvo.EstaVivo()) return;

        // 1. Rolagem de d20 escondida na engine
        int d20Oculto = Random.Range(1, 21);
        int totalAtaque = d20Oculto + modificadorAtaque;
        int caAlvo = alvo.classeArmadura;

        // 2. Rolagem da arma de 1d8 + modificador (+3)
        int dadoArma = Random.Range(1, 9); // 1 a 8
        float danoFinal;
        bool ehCritico = false;
        Color corDano;

        if (d20Oculto == 20)
        {
            // CRÍTICO NATURAL: Dano dobrado dos dados + modificador (média ~14 a 17)
            ehCritico = true;
            int segundoDado = Random.Range(1, 9);
            danoFinal = dadoArma + segundoDado + modificadorAtaque;
            corDano = new Color(1f, 0.85f, 0.2f); // Amarelo/Dourado Crítico
            CriarFaiscasImpacto(alvo.transform.position + Vector3.up * 1.2f, true);
        }
        else if (totalAtaque >= caAlvo)
        {
            // ACERTO PLENO: Superou a CA (média calibrada 7 a 9 pontos)
            danoFinal = dadoArma + modificadorAtaque; // Entre 4 e 11, média 7.5
            corDano = new Color(0.95f, 0.95f, 0.95f); // Branco padrão
            CriarFaiscasImpacto(alvo.transform.position + Vector3.up * 1.2f, false);
        }
        else
        {
            // GOLPE RASPÃO: Não superou a CA, mas nunca dá zero (satisfação de ação)
            // Causa ~60% a 70% do dano simulando impacto na armadura
            float danoBase = dadoArma + modificadorAtaque;
            danoFinal = Mathf.Max(4f, Mathf.Round(danoBase * 0.65f));
            corDano = new Color(0.7f, 0.7f, 0.8f); // Cinza metálico
            CriarFaiscasImpacto(alvo.transform.position + Vector3.up * 1.2f, false);
        }

        // Aplica o dano ao inimigo
        alvo.ReceberDano(danoFinal, corDano, ehCritico);
    }

    public void ReceberDanoDoInimigo(float danoBruto, EnemyAIBase atacante)
    {
        if (pvAtual <= 0f) return;

        pvAtual = Mathf.Max(0f, pvAtual - danoBruto);
        Debug.Log($"🩸 JOGADOR ferido! Sofreu {danoBruto:F1} de dano. PV restante: {pvAtual:F1}/{pvMaximo}");

        // Atualiza SistemaDeVida do jogador se presente
        SistemaDeVida vida = GetComponent<SistemaDeVida>();
        if (vida != null)
        {
            vida.ReceberDano(danoBruto, new Color(1f, 0.2f, 0.2f));
            vida.vidaAtual = pvAtual;
        }

        if (pvAtual <= 0f)
        {
            Morrer();
        }
    }

    void Morrer()
    {
        Debug.Log("💀 JOGADOR TOMBOU EM COMBATE!");
        GameManagerFase1 gm = FindAnyObjectByType<GameManagerFase1>();
        if (gm != null)
        {
            gm.NotificarMorteJogador();
        }
    }

    void CriarRastroAtaqueVisual()
    {
        GameObject barraVfx = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barraVfx.name = "TrailAtaque3P";
        barraVfx.transform.position = transform.position + transform.forward * 1.4f + Vector3.up * 1.1f;
        barraVfx.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + 45f, 90f);
        barraVfx.transform.localScale = new Vector3(0.04f, 1.2f, 0.04f);
        Destroy(barraVfx.GetComponent<Collider>());

        Renderer r = barraVfx.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = new Color(0.9f, 0.95f, 1f, 0.6f);
            r.material = mat;
        }

        Destroy(barraVfx, 0.14f);
    }

    void CriarFaiscasImpacto(Vector3 pos, bool critico)
    {
        int qtd = critico ? 6 : 3;
        Color cor = critico ? new Color(1f, 0.8f, 0.2f) : new Color(0.8f, 0.8f, 0.9f);

        for (int i = 0; i < qtd; i++)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p.name = "FaiscaImpacto";
            p.transform.position = pos + Random.insideUnitSphere * 0.2f;
            p.transform.localScale = Vector3.one * (critico ? 0.08f : 0.05f);
            Destroy(p.GetComponent<Collider>());

            Renderer rend = p.GetComponent<Renderer>();
            if (rend != null)
            {
                Material m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
                m.color = cor;
                rend.material = m;
            }
            Destroy(p, 0.22f);
        }
    }

    public void Curar(float quantidade)
    {
        pvAtual = Mathf.Min(pvMaximo, pvAtual + quantidade);
        SistemaDeVida vida = GetComponent<SistemaDeVida>();
        if (vida != null)
        {
            vida.vidaAtual = Mathf.Min(vida.vidaMaxima, vida.vidaAtual + quantidade);
        }
        Debug.Log($"💚 Jogador curado em {quantidade:F0} PV! PV atual: {pvAtual}/{pvMaximo}");
    }

    private GameObject armaVisual3P;
    private Quaternion rotacaoArmaBase = Quaternion.Euler(20f, 15f, -15f);

    public void EquiparArmaVisual3P(Transform parent)
    {
        if (armaVisual3P != null) return;

        armaVisual3P = new GameObject("EspadaAlien3P");
        armaVisual3P.transform.SetParent(parent, false);
        armaVisual3P.transform.localPosition = new Vector3(0.5f, 0.9f, 0.35f);
        armaVisual3P.transform.localRotation = rotacaoArmaBase;

        Material matLamina = MateriaisAlienigenas.CriarMaterialSafeRoom();
        Material matEnergia = MateriaisAlienigenas.CriarMaterialCristalEstatico();

        // Lâmina
        GameObject lamina = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lamina.transform.SetParent(armaVisual3P.transform, false);
        lamina.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        lamina.transform.localScale = new Vector3(0.08f, 1.1f, 0.18f);
        Destroy(lamina.GetComponent<Collider>());
        lamina.GetComponent<Renderer>().material = matLamina;

        // Fio de energia estática
        GameObject energia = GameObject.CreatePrimitive(PrimitiveType.Cube);
        energia.transform.SetParent(armaVisual3P.transform, false);
        energia.transform.localPosition = new Vector3(0f, 0.5f, 0.09f);
        energia.transform.localScale = new Vector3(0.09f, 1.05f, 0.04f);
        Destroy(energia.GetComponent<Collider>());
        energia.GetComponent<Renderer>().material = matEnergia;

        // Guarda
        GameObject guarda = GameObject.CreatePrimitive(PrimitiveType.Cube);
        guarda.transform.SetParent(armaVisual3P.transform, false);
        guarda.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        guarda.transform.localScale = new Vector3(0.14f, 0.06f, 0.42f);
        Destroy(guarda.GetComponent<Collider>());
        guarda.GetComponent<Renderer>().material = matLamina;

        // Cabo
        GameObject cabo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cabo.transform.SetParent(armaVisual3P.transform, false);
        cabo.transform.localPosition = new Vector3(0f, -0.22f, 0f);
        cabo.transform.localScale = new Vector3(0.06f, 0.16f, 0.06f);
        Destroy(cabo.GetComponent<Collider>());
        cabo.GetComponent<Renderer>().material = matLamina;
    }

    IEnumerator AnimarGolpeEspada()
    {
        if (armaVisual3P == null) yield break;

        float duracao = 0.28f;
        float t = 0f;

        Quaternion rotInicial = armaVisual3P.transform.localRotation;
        Quaternion rotGolpe = Quaternion.Euler(75f, -65f, 40f);

        while (t < duracao * 0.45f)
        {
            t += Time.deltaTime;
            float p = t / (duracao * 0.45f);
            if (armaVisual3P != null) armaVisual3P.transform.localRotation = Quaternion.Slerp(rotInicial, rotGolpe, p);
            yield return null;
        }

        t = 0f;
        while (t < duracao * 0.55f)
        {
            t += Time.deltaTime;
            float p = t / (duracao * 0.55f);
            if (armaVisual3P != null) armaVisual3P.transform.localRotation = Quaternion.Slerp(rotGolpe, rotInicial, p);
            yield return null;
        }

        if (armaVisual3P != null) armaVisual3P.transform.localRotation = rotInicial;
    }
}
