using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EstacaoDruida
{
    Primavera, // Regeneração de vida contínua
    Outono     // Dano aumentado e decomposição nos inimigos
}

public class ControleDruida : MonoBehaviour
{
    [Header("Estação Atual")]
    public EstacaoDruida estacaoAtual = EstacaoDruida.Primavera;

    [Header("Configurações do Chicote de Espinhos")]
    public float alcanceChicote = 6.5f;
    public float danoGolpe1 = 28f;
    public float danoGolpe2 = 28f;
    public float danoGolpe3 = 40f;
    public float forcaPuxao = 4.5f;
    public float tempoResetCombo = 1.2f;

    [Header("Suprema: Bosque da Tranquilidade (F)")]
    public float tempoRecargaSuprema = 20f;
    private float proximaSupremaPermitida = 0f;

    [Header("Defesa: Pele de Árvore (RMB)")]
    public float janelaParry = 0.30f;
    public float curaParry = 25f;
    private float tempoInicioBloqueio = 0f;
    private bool emBloqueio = false;

    [Header("Aura da Primavera")]
    public float curaAuraPorSegundo = 5f;
    private float proximoTickCura = 0f;

    [Header("Referências")]
    public Camera cameraJogador;
    public StatusJogador statusJogador;
    public ModeloDruidaProcedural modeloDruida;
    public ModeloCervoEspectral modeloCervo;

    // Controle de Combo
    private int indiceCombo = 0; // 0 = pronto, 1, 2, 3
    private float tempoUltimoGolpe = 0f;
    private bool atacando = false;

    void Awake()
    {
        if (cameraJogador == null) cameraJogador = Camera.main;
        if (statusJogador == null) statusJogador = GetComponentInParent<StatusJogador>();
        if (modeloDruida == null) modeloDruida = GetComponent<ModeloDruidaProcedural>();
        if (modeloDruida == null) modeloDruida = GetComponentInChildren<ModeloDruidaProcedural>();
        if (modeloDruida == null) modeloDruida = gameObject.AddComponent<ModeloDruidaProcedural>();

        if (modeloCervo == null) modeloCervo = GetComponent<ModeloCervoEspectral>();
        if (modeloCervo == null) modeloCervo = GetComponentInChildren<ModeloCervoEspectral>();
        if (modeloCervo == null) modeloCervo = gameObject.AddComponent<ModeloCervoEspectral>();
    }

    void Update()
    {
        // 1. CICLO DAS ESTAÇÕES (Ctrl)
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            AlternarEstacao();
        }

        // Aura contínua da Primavera
        if (estacaoAtual == EstacaoDruida.Primavera && Time.time >= proximoTickCura)
        {
            proximoTickCura = Time.time + 1.0f;
            if (statusJogador != null) statusJogador.Curar(curaAuraPorSegundo);
        }

        // 2. DEFESA: PELE DE ÁRVORE (RMB)
        GerenciarPeleDeArvore();

        // Se estiver bloqueando ou atacando, não inicia novos golpes
        if (emBloqueio || atacando) return;

        // Reset do combo após inatividade
        if (indiceCombo > 0 && Time.time > tempoUltimoGolpe + tempoResetCombo)
        {
            indiceCombo = 0;
        }

        // 3. ATAQUE BÁSICO: CHICOTE DE ESPINHOS (Clique Esquerdo / Fire1)
        if (Input.GetButtonDown("Fire1"))
        {
            ExecutarProximoGolpeCombo();
        }

        // 4. SUPREMA: BOSQUE DA TRANQUILIDADE (F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (Time.time >= proximaSupremaPermitida)
            {
                ConjurarBosqueDaTranquilidade();
            }
            else
            {
                Debug.Log("Bosque em recarga! Restam " + Mathf.Ceil(proximaSupremaPermitida - Time.time) + "s");
            }
        }
    }

    void AlternarEstacao()
    {
        estacaoAtual = (estacaoAtual == EstacaoDruida.Primavera) ? EstacaoDruida.Outono : EstacaoDruida.Primavera;
        Debug.Log("🌿 Estação alterada para: " + estacaoAtual.ToString());
    }

    void GerenciarPeleDeArvore()
    {
        bool segurandoRMB = (Input.GetMouseButton(1) || Input.GetButton("Fire2")) && !atacando;

        if (segurandoRMB && !emBloqueio)
        {
            emBloqueio = true;
            tempoInicioBloqueio = Time.time;
            if (modeloDruida != null) modeloDruida.DefinirBloqueio(true);
        }
        else if (!segurandoRMB && emBloqueio)
        {
            emBloqueio = false;
            if (modeloDruida != null) modeloDruida.DefinirBloqueio(false);
        }
    }

    void ExecutarProximoGolpeCombo()
    {
        indiceCombo++;
        if (indiceCombo > 3) indiceCombo = 1;

        tempoUltimoGolpe = Time.time;
        float duracaoGolpe = (indiceCombo == 3) ? 0.45f : 0.32f;

        StartCoroutine(RotinaGolpeChicote(indiceCombo, duracaoGolpe));
    }

    IEnumerator RotinaGolpeChicote(int combo, float duracao)
    {
        atacando = true;

        if (modeloDruida != null)
        {
            StartCoroutine(modeloDruida.ExecutarAnimacaoChicote(combo, duracao, cameraJogador.transform.forward));
        }

        yield return new WaitForSeconds(duracao * 0.4f);

        // Processa o acerto do chicote
        ProcessarAcertoChicote(combo);

        yield return new WaitForSeconds(duracao * 0.6f);
        atacando = false;
    }

    void ProcessarAcertoChicote(int combo)
    {
        if (cameraJogador == null) return;

        Ray raio = new Ray(cameraJogador.transform.position, cameraJogador.transform.forward);
        RaycastHit hit;

        if (Physics.SphereCast(raio, 0.6f, out hit, alcanceChicote))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.GetComponent<PlayerController>() != null) return;

            SistemaDeVida vida = hit.collider.GetComponent<SistemaDeVida>();
            if (vida == null) vida = hit.collider.GetComponentInParent<SistemaDeVida>();

            if (vida != null)
            {
                float dano = (combo == 1) ? danoGolpe1 : (combo == 2 ? danoGolpe2 : danoGolpe3);

                // Bônus de Outono: +25% de dano e decomposição
                if (estacaoAtual == EstacaoDruida.Outono)
                {
                    dano *= 1.25f;
                }

                Color corDano = (estacaoAtual == EstacaoDruida.Outono) ? new Color(0.95f, 0.5f, 0.1f) : new Color(0.2f, 0.9f, 0.3f);
                vida.ReceberDano(dano, corDano);

                // No Golpe 3: Puxa o inimigo levemente para perto do Druida e gera Semente Vital!
                if (combo == 3)
                {
                    Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dirPuxao = (transform.position - hit.collider.transform.position).normalized + Vector3.up * 0.2f;
                        rb.AddForce(dirPuxao * forcaPuxao, ForceMode.Impulse);
                    }

                    // Passiva: Gera semente vital no local
                    GerarSementeVital(hit.point);
                }
            }
        }
    }

    void ConjurarBosqueDaTranquilidade()
    {
        proximaSupremaPermitida = Time.time + tempoRecargaSuprema;

        Vector3 posChao = transform.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out hit, 10f))
        {
            posChao = hit.point;
        }

        GameObject bosqueObj = new GameObject("BosqueDaTranquilidade");
        bosqueObj.transform.position = posChao;
        bosqueObj.AddComponent<BosqueTranquilidade>();

        Debug.Log("🌳 SUPREMA: Bosque da Tranquilidade convocado!");
    }

    void GerarSementeVital(Vector3 posicao)
    {
        GameObject sementeObj = new GameObject("SementeVital");
        sementeObj.transform.position = posicao + Vector3.up * 0.1f;
        sementeObj.AddComponent<SementeVital>();
    }

    // --- DEFESA E PARRY ---
    public bool EstaEmBloqueio() => emBloqueio;

    public bool ChecarPerfectParry()
    {
        return emBloqueio && (Time.time <= tempoInicioBloqueio + janelaParry);
    }

    public void ExecutarPerfectParry()
    {
        if (statusJogador != null)
        {
            statusJogador.Curar(curaParry);
        }

        if (modeloDruida != null)
        {
            modeloDruida.GerarEsporosParry();
        }

        Debug.Log("🛡️ PERFECT PARRY DRUIDA! Esporos curativos liberados (+25 HP)!");
    }

    public float ObterTempoRestanteSuprema()
    {
        return Mathf.Max(0f, proximaSupremaPermitida - Time.time);
    }
}
