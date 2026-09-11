using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ClasseBot
{
    Guerreiro,
    Ranger,
    Mago
}

public class BotCompanheiroAI : MonoBehaviour
{
    [Header("Identidade do Bot")]
    public string nomeBot = "Valen (Guerreiro)";
    public ClasseBot classe = ClasseBot.Guerreiro;

    [Header("Atributos")]
    public float pvMaximo = 16f;
    public float pvAtual = 16f;
    public int classeArmadura = 15;

    [Header("Combate")]
    public float alcanceAtaque = 3.0f;
    public float danoPorGolpe = 7.5f;
    public float cadenciaAtaque = 1.4f;

    [Header("Comportamento Co-op")]
    public float distanciaManterJogador = 4.0f;
    public float distanciaDispararSafeRoom = 14f;

    private NavMeshAgent agent;
    private PlayerCombatD20 jogadorHumano;
    private EnemyAIBase inimigoAlvo;
    private PortaSalaSegura safeRoom;
    private float proximoAtaquePermitido = 0f;
    private bool estaMorto = false;
    private bool indoParaSafeRoom = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = 4.2f;
        agent.acceleration = 12f;
        agent.stoppingDistance = 2.0f;

        pvAtual = pvMaximo;
    }

    void Start()
    {
        jogadorHumano = FindAnyObjectByType<PlayerCombatD20>();
        safeRoom = FindAnyObjectByType<PortaSalaSegura>();
        ConstruirVisualBot();
    }

    void Update()
    {
        if (estaMorto || Time.timeScale <= 0.001f) return;

        // Se a Safe Room estiver aberta e próxima, prioriza entrar nela (como em L4D)
        if (safeRoom != null && safeRoom.EstaAberta())
        {
            float distSafe = Vector3.Distance(transform.position, safeRoom.transform.position);
            if (distSafe <= distanciaDispararSafeRoom || (jogadorHumano != null && Vector3.Distance(jogadorHumano.transform.position, safeRoom.transform.position) < 8f))
            {
                indoParaSafeRoom = true;
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(safeRoom.pontoInteriorSafeRoom != null ? safeRoom.pontoInteriorSafeRoom.position : safeRoom.transform.position);
                }
                return;
            }
        }

        // Procura inimigo próximo
        BuscarInimigoMaisProximo();

        if (inimigoAlvo != null && Vector3.Distance(transform.position, inimigoAlvo.transform.position) <= 12f)
        {
            // Modo Combate
            float distInimigo = Vector3.Distance(transform.position, inimigoAlvo.transform.position);
            if (distInimigo <= alcanceAtaque)
            {
                if (agent.isOnNavMesh) agent.isStopped = true;

                // Encare o inimigo
                Vector3 dir = (inimigoAlvo.transform.position - transform.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir);

                if (Time.time >= proximoAtaquePermitido)
                {
                    proximoAtaquePermitido = Time.time + cadenciaAtaque;
                    ExecutarAtaqueNoInimigo();
                }
            }
            else
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(inimigoAlvo.transform.position);
                }
            }
        }
        else if (jogadorHumano != null)
        {
            // Modo Acompanhar Jogador
            float distJogador = Vector3.Distance(transform.position, jogadorHumano.transform.position);
            if (distJogador > distanciaManterJogador)
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(jogadorHumano.transform.position);
                }
            }
            else
            {
                if (agent.isOnNavMesh) agent.isStopped = true;
            }
        }
    }

    void BuscarInimigoMaisProximo()
    {
        EnemyAIBase[] inimigos = FindObjectsByType<EnemyAIBase>(FindObjectsSortMode.None);
        float menorDist = float.MaxValue;
        EnemyAIBase maisPerto = null;

        foreach (var inimi in inimigos)
        {
            if (inimi != null && inimi.EstaVivo())
            {
                float d = Vector3.Distance(transform.position, inimi.transform.position);
                if (d < menorDist)
                {
                    menorDist = d;
                    maisPerto = inimi;
                }
            }
        }

        inimigoAlvo = maisPerto;
    }

    void ExecutarAtaqueNoInimigo()
    {
        if (inimigoAlvo == null || !inimigoAlvo.EstaVivo()) return;

        // Animação visual do ataque
        StartCoroutine(RotinaVisualGolpe());

        // Dano calibrado D&D 1d8+3
        float dano = Random.Range(6.5f, 9.5f);
        inimigoAlvo.ReceberDano(dano, new Color(0.3f, 0.8f, 1f));
    }

    IEnumerator RotinaVisualGolpe()
    {
        GameObject fx = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fx.transform.position = transform.position + transform.forward * 1.2f + Vector3.up * 1.0f;
        fx.transform.localScale = new Vector3(0.1f, 0.1f, 1.2f);
        fx.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + 45f, 0f);
        Destroy(fx.GetComponent<Collider>());

        Renderer r = fx.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            m.color = new Color(0.4f, 0.9f, 1f);
            r.material = m;
        }
        yield return new WaitForSeconds(0.16f);
        Destroy(fx);
    }

    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        pvAtual = Mathf.Max(0f, pvAtual - dano);
        Debug.Log($"🛡️ BOT {nomeBot} recebeu {dano:F1} de dano. PV: {pvAtual}/{pvMaximo}");

        if (pvAtual <= 0f)
        {
            Morrer();
        }
    }

    void Morrer()
    {
        estaMorto = true;
        if (agent.isOnNavMesh) agent.isStopped = true;
        Debug.Log($"💀 BOT {nomeBot} TOMBOU!");
        StartCoroutine(RotinaMorteBot());
    }

    IEnumerator RotinaMorteBot()
    {
        transform.rotation = Quaternion.Euler(75f, transform.eulerAngles.y, 0f);
        yield return new WaitForSeconds(3.5f);
        Destroy(gameObject);
    }

    void ConstruirVisualBot()
    {
        Material matArmadura = MateriaisAlienigenas.CriarMaterialSafeRoom();
        Material matCapa = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
        matCapa.color = classe == ClasseBot.Guerreiro ? new Color(0.7f, 0.2f, 0.2f) : (classe == ClasseBot.Ranger ? new Color(0.2f, 0.6f, 0.25f) : new Color(0.2f, 0.4f, 0.8f));

        // Corpo
        GameObject corpo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        corpo.name = "CorpoBot";
        corpo.transform.SetParent(transform, false);
        corpo.transform.localPosition = new Vector3(0f, 1.0f, 0f);
        corpo.transform.localScale = new Vector3(0.6f, 1.0f, 0.6f);
        Destroy(corpo.GetComponent<Collider>());
        corpo.GetComponent<Renderer>().material = matArmadura;

        // Capa/Insígnia
        GameObject capa = GameObject.CreatePrimitive(PrimitiveType.Cube);
        capa.name = "CapaBot";
        capa.transform.SetParent(transform, false);
        capa.transform.localPosition = new Vector3(0f, 1.0f, -0.32f);
        capa.transform.localScale = new Vector3(0.5f, 1.1f, 0.08f);
        Destroy(capa.GetComponent<Collider>());
        capa.GetComponent<Renderer>().material = matCapa;

        // Colisor do bot
        CapsuleCollider col = gameObject.GetComponent<CapsuleCollider>();
        if (col == null) col = gameObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 1.0f, 0f);
        col.radius = 0.45f;
        col.height = 2.0f;
    }

    public bool EstaVivo() => !estaMorto && pvAtual > 0f;
}
