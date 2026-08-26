using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PosturaEspada
{
    Vertical,   // Postura Equilibrada (Padrão)
    Horizontal  // Postura Carnificina Berserker (+Crit, +Dano Recebido)
}

public class AtaqueEspada : MonoBehaviour
{
    [Header("Postura e Modos")]
    public PosturaEspada posturaAtual = PosturaEspada.Vertical;

    [Header("Configurações do Ataque Pesado (Cleave)")]
    public float danoBase = 45f;
    public float alcanceCleave = 4.2f;
    public float anguloCleave = 120f;
    public float forcaKnockback = 1.8f; // Knockback leve e realista
    public float furiaPorAcerto = 15f;

    [Header("Cadências de Golpe")]
    public float tempoEntreAtaquesNormal = 0.85f;
    public float tempoEntreAtaquesFrenesi = 0.30f;
    public float duracaoGolpeNormal = 0.55f;
    public float duracaoGolpeFrenesi = 0.22f;

    [Header("Chances de Crítico")]
    public float chanceCriticoVertical = 0.10f;    // 10% chance
    public float chanceCriticoHorizontal = 0.60f;  // 60% chance (+50% bônus)
    public float multiplicadorCritico = 2.0f;

    [Header("Habilidade Suprema: Frenesi (F)")]
    public float duracaoFrenesi = 8f;
    public float duracaoExaustao = 4f;

    [Header("Referências")]
    public Camera cameraJogador;
    public StatusJogador statusJogador;
    public ModeloEspadaBerserker modeloEspada;

    // Estados de Controle
    private bool atacando = false;
    private bool emGuardaBruta = false;
    private bool emFrenesi = false;
    private bool emExaustao = false;
    private bool golpeDaEsquerdaParaDireita = false;

    private float proximoAtaquePermitido = 0f;
    private float tempoFimFrenesi = 0f;
    private float tempoFimExaustao = 0f;

    void Awake()
    {
        if (cameraJogador == null) cameraJogador = Camera.main;
        if (statusJogador == null) statusJogador = GetComponentInParent<StatusJogador>();
        if (modeloEspada == null) modeloEspada = GetComponent<ModeloEspadaBerserker>();
        if (modeloEspada == null) modeloEspada = GetComponentInChildren<ModeloEspadaBerserker>();
        if (modeloEspada == null) modeloEspada = gameObject.AddComponent<ModeloEspadaBerserker>();
    }

    void OnEnable()
    {
        emGuardaBruta = false;
        if (modeloEspada != null)
        {
            modeloEspada.DefinirPostura(posturaAtual, false);
        }
    }

    void Update()
    {
        GerenciarTemporizadoresFrenesi();

        // 1. MUDANÇA DE POSTURA (Ctrl)
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            AlternarPostura();
        }

        // 2. GUARDA BRUTA (Botão Direito do Mouse / RMB)
        GerenciarGuardaBruta();

        // 3. HABILIDADE SUPREMA: FRENESI (F)
        if (Input.GetKeyDown(KeyCode.F) && !emFrenesi && !emExaustao)
        {
            if (statusJogador != null && statusJogador.EstaEmFuriaMaxima())
            {
                AtivarFrenesi();
            }
            else
            {
                Debug.Log("Frenesi requer 100% de Fúria!");
            }
        }

        // 4. ATUALIZA BRILHO DAS RUNAS NA ESPADA
        if (modeloEspada != null && statusJogador != null)
        {
            modeloEspada.AtualizarBrilhoFuria(statusJogador.ObterPorcentagemFuria(), emFrenesi);
        }

        // Se estiver em Guarda Bruta ou no meio de um golpe, não inicia novo ataque
        if (emGuardaBruta || atacando) return;

        // 5. ATAQUE BÁSICO PESADO COM CLEAVE (Clique Esquerdo / Fire1)
        if (Input.GetButton("Fire1") && Time.time >= proximoAtaquePermitido)
        {
            float cadencia = emFrenesi ? tempoEntreAtaquesFrenesi : tempoEntreAtaquesNormal;
            proximoAtaquePermitido = Time.time + cadencia;

            StartCoroutine(ExecutarGolpeCleave());
        }
    }

    void AlternarPostura()
    {
        posturaAtual = (posturaAtual == PosturaEspada.Vertical) ? PosturaEspada.Horizontal : PosturaEspada.Vertical;
        Debug.Log("Postura da Espada alterada para: " + posturaAtual.ToString());

        if (modeloEspada != null)
        {
            modeloEspada.DefinirPostura(posturaAtual, emGuardaBruta);
        }
    }

    void GerenciarGuardaBruta()
    {
        bool segurandoRMB = (Input.GetMouseButton(1) || Input.GetButton("Fire2")) && !atacando;

        if (segurandoRMB != emGuardaBruta)
        {
            emGuardaBruta = segurandoRMB;
            if (modeloEspada != null)
            {
                modeloEspada.DefinirPostura(posturaAtual, emGuardaBruta);
            }
        }
    }

    void AtivarFrenesi()
    {
        emFrenesi = true;
        tempoFimFrenesi = Time.time + duracaoFrenesi;

        if (statusJogador != null)
        {
            statusJogador.CongelarFuria(true);
        }

        Debug.Log("🔥 FRENESI ATIVADO! Carnificina total por 8 segundos!");
    }

    void GerenciarTemporizadoresFrenesi()
    {
        if (emFrenesi && Time.time >= tempoFimFrenesi)
        {
            emFrenesi = false;
            emExaustao = true;
            tempoFimExaustao = Time.time + duracaoExaustao;

            if (statusJogador != null)
            {
                statusJogador.ConsumirTodaFuria();
            }

            Debug.Log("💨 Frenesi finalizado! Entrando em Exaustão por 4s...");
        }

        if (emExaustao && Time.time >= tempoFimExaustao)
        {
            emExaustao = false;
            Debug.Log("✨ Exaustão finalizada! Forças recuperadas.");
        }
    }

    IEnumerator ExecutarGolpeCleave()
    {
        atacando = true;
        float duracaoAnim = emFrenesi ? duracaoGolpeFrenesi : duracaoGolpeNormal;

        // Inicia a animação apropriada para a postura atual
        if (modeloEspada != null)
        {
            if (posturaAtual == PosturaEspada.Horizontal)
            {
                // Varredura Horizontal Alternada (vassoura: Direita -> Esquerda / Esquerda -> Direita)
                StartCoroutine(modeloEspada.ExecutarAnimacaoHorizontal(duracaoAnim, emFrenesi, golpeDaEsquerdaParaDireita));
                golpeDaEsquerdaParaDireita = !golpeDaEsquerdaParaDireita;
            }
            else
            {
                // Corte Vertical / Diagonal
                StartCoroutine(modeloEspada.ExecutarAnimacaoVertical(duracaoAnim, emFrenesi));
            }
        }

        // Aguarda fração de tempo para o impacto no meio do golpe
        yield return new WaitForSeconds(duracaoAnim * 0.35f);

        // Processa o acerto em área (Cleave)
        ProcessarImpactoCleave();

        yield return new WaitForSeconds(duracaoAnim * 0.65f);
        atacando = false;
    }

    void ProcessarImpactoCleave()
    {
        if (cameraJogador == null) return;

        Vector3 origemJogador = cameraJogador.transform.position;
        Vector3 direcaoOlhar = cameraJogador.transform.forward;

        // Encontra todos os colisores no raio ao redor da frente do jogador
        Collider[] alvos = Physics.OverlapSphere(origemJogador + direcaoOlhar * (alcanceCleave * 0.5f), alcanceCleave);
        HashSet<GameObject> acertados = new HashSet<GameObject>();

        float chanceCrit = (posturaAtual == PosturaEspada.Horizontal) ? chanceCriticoHorizontal : chanceCriticoVertical;

        foreach (var col in alvos)
        {
            if (col.transform.root == transform.root) continue;
            if (col.CompareTag("Player") || col.GetComponent<PlayerController>() != null) continue;

            GameObject rootAlvo = col.transform.root.gameObject;
            if (acertados.Contains(rootAlvo)) continue;

            Vector3 vetorAoInimigo = col.bounds.center - origemJogador;
            float distancia = vetorAoInimigo.magnitude;

            if (distancia > alcanceCleave) continue;

            // Verifica o ângulo do cone de Cleave (120 graus à frente)
            float angulo = Vector3.Angle(direcaoOlhar, vetorAoInimigo);
            if (angulo <= (anguloCleave * 0.5f))
            {
                acertados.Add(rootAlvo);

                SistemaDeVida vida = col.GetComponent<SistemaDeVida>();
                if (vida == null) vida = col.GetComponentInParent<SistemaDeVida>();

                if (vida != null)
                {
                    // Cálculo de dano e Acerto Crítico
                    bool critico = Random.value <= chanceCrit;
                    float danoFinal = critico ? (danoBase * multiplicadorCritico) : danoBase;

                    if (emFrenesi)
                    {
                        danoFinal *= 1.25f; // Bônus de carnificina em Frenesi
                    }

                    // Cor do número de dano: Vermelho/Dourado se crítico, Laranja/Branco se normal
                    Color corTexto = critico ? new Color(1f, 0.15f, 0f) : new Color(1f, 0.6f, 0.1f);
                    vida.ReceberDano(danoFinal, corTexto);

                    // Gera Fúria
                    if (statusJogador != null)
                    {
                        statusJogador.GanharFuria(furiaPorAcerto);
                    }

                    // Impacto e Knockback contínuo
                    Rigidbody rb = col.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dirEmpurrao = (col.transform.position - transform.position).normalized + Vector3.up * 0.35f;
                        rb.AddForce(dirEmpurrao * forcaKnockback, ForceMode.Impulse);
                    }
                }
            }
        }
    }

    public void GerarFaiscasBloqueio()
    {
        if (modeloEspada != null)
        {
            modeloEspada.GerarFaiscasBloqueio();
        }
    }

    // --- GETTERS DE ESTADO ---
    public bool EstaEmGuardaBruta() => emGuardaBruta;
    public bool EstaEmPosturaHorizontal() => posturaAtual == PosturaEspada.Horizontal;
    public bool EstaEmFrenesi() => emFrenesi;
    public bool EstaEmExaustao() => emExaustao;
}
