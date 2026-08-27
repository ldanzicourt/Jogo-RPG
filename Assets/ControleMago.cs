using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControleMago : MonoBehaviour
{
    [Header("Escola Atual")]
    public EscolaMagica escolaAtual = EscolaMagica.Evocacao;

    [Header("Prefabs e Referências")]
    public GameObject prefabDardoArcano;
    public GameObject prefabVorticeTrama;
    public Camera cameraJogador;
    public StatusJogador statusJogador;
    public ModeloMagoProcedural modeloVisual;

    [Header("Tempos de Recarga (Cooldowns)")]
    public float cooldownVortice = 6f;
    public float cooldownSaltoEtereo = 4f;
    public float cooldownSuprema = 25f;

    [Header("Dano e Eficácia das Habilidades")]
    public float curaPorSegundoSifao = 18f;
    public float danoPorSegundoSifao = 28f;
    public float danoPorSegundoSuprema = 80f;
    public float duracaoSuprema = 2.5f;

    // Cronômetros de Cooldown
    private float tempoRecargaVortice = 0f;
    private float tempoRecargaSalto = 0f;
    private float tempoRecargaSuprema = 0f;

    // Controle de Disparo Evocação
    private float cadenciaDardo = 0.35f;
    private float proximoDardoPermitido = 0f;

    // Estado da Barreira de Estudo (Clique Direito)
    private bool barreiraAtiva = false;
    private float tempoAtivacaoBarreira = 0f;
    private float janelaParryPerfeito = 0.3f;

    // Estado de Canalização (Necromancia & Suprema)
    private bool canalizandoSifao = false;
    private bool canalizandoSuprema = false;

    void Awake()
    {
        if (cameraJogador == null) cameraJogador = Camera.main;
        if (statusJogador == null) statusJogador = GetComponentInParent<StatusJogador>();
        if (modeloVisual == null) modeloVisual = GetComponent<ModeloMagoProcedural>();
        if (modeloVisual == null) modeloVisual = GetComponentInChildren<ModeloMagoProcedural>();
    }

    void OnEnable()
    {
        if (modeloVisual != null)
        {
            modeloVisual.AtualizarCorEscola(escolaAtual);
        }
    }

    void Update()
    {
        if (Time.timeScale <= 0.001f) return;

        AtualizarCronometros();

        // 1. FOLHEAR GRIMÓRIO (Ctrl ou Scroll do Mouse)
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.05f)
        {
            if (!canalizandoSuprema)
            {
                AvancarEscolaMagica();
            }
        }

        // 2. BARREIRA DE ESTUDO (Clique Direito / Fire2)
        if (!canalizandoSuprema)
        {
            GerenciarBarreira();
        }

        // Se a barreira estiver ativa, não dispara magias com a mão direita
        if (barreiraAtiva)
        {
            if (canalizandoSifao) PararSifao();
            return;
        }

        // 3. HABILIDADE SUPREMA: DESINTEGRAÇÃO (F)
        if (Input.GetKeyDown(KeyCode.F) && !canalizandoSuprema && Time.time >= tempoRecargaSuprema)
        {
            StartCoroutine(ExecutarSupremaDesintegracao());
        }

        if (canalizandoSuprema) return;

        // 4. DISPARO DE MAGIAS DO GRIMÓRIO (Clique Esquerdo / Fire1)
        GerenciarAtaquesEscolas();
    }

    void AtualizarCronometros()
    {
        // Cooldowns diminuem naturalmente
    }

    // --- 1. FOLHEAR GRIMÓRIO ---
    public void AvancarEscolaMagica()
    {
        if (canalizandoSifao) PararSifao();

        int proximoIndice = ((int)escolaAtual + 1) % 3;
        escolaAtual = (EscolaMagica)proximoIndice;

        if (modeloVisual != null)
        {
            modeloVisual.AtualizarCorEscola(escolaAtual);
            modeloVisual.AnimarFolhearPaginas();
        }
    }

    // --- 2. BARREIRA DE ESTUDO ---
    void GerenciarBarreira()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Fire2"))
        {
            barreiraAtiva = true;
            tempoAtivacaoBarreira = Time.time;
            if (modeloVisual != null) modeloVisual.DefinirBarreiraAtiva(true);
        }
        else if (Input.GetMouseButtonUp(1) || Input.GetButtonUp("Fire2"))
        {
            barreiraAtiva = false;
            if (modeloVisual != null) modeloVisual.DefinirBarreiraAtiva(false);
        }
    }

    public bool EstaComBarreiraAtiva()
    {
        return barreiraAtiva;
    }

    public bool ChecarBloqueioPerfeito()
    {
        return barreiraAtiva && (Time.time <= tempoAtivacaoBarreira + janelaParryPerfeito);
    }

    public void ExecutarBloqueioPerfeito()
    {
        Debug.Log("⚡ BLOQUEIO PERFEITO DA BARREIRA DE ESTUDO! Cooldowns reduzidos!");
        ReduzirTodosCooldowns(4f);

        if (modeloVisual != null)
        {
            modeloVisual.PulsarBarreiraParry();
        }
    }

    public void ReduzirTodosCooldowns(float segundos)
    {
        tempoRecargaVortice = Mathf.Max(Time.time, tempoRecargaVortice - segundos);
        tempoRecargaSalto = Mathf.Max(Time.time, tempoRecargaSalto - segundos);
        tempoRecargaSuprema = Mathf.Max(Time.time, tempoRecargaSuprema - segundos);
    }

    // --- 3. ATAQUES DAS ESCOLAS MÁGICAS ---
    void GerenciarAtaquesEscolas()
    {
        switch (escolaAtual)
        {
            case EscolaMagica.Evocacao:
                if (canalizandoSifao) PararSifao();

                if (Input.GetButton("Fire1") && Time.time >= proximoDardoPermitido)
                {
                    proximoDardoPermitido = Time.time + cadenciaDardo;
                    DispararDardoArcano();
                }
                break;

            case EscolaMagica.Necromancia:
                if (Input.GetButton("Fire1"))
                {
                    ProcessarSifaoDeAlma();
                }
                else if (canalizandoSifao)
                {
                    PararSifao();
                }
                break;

            case EscolaMagica.Conjuracao:
                if (canalizandoSifao) PararSifao();

                if (Input.GetButtonDown("Fire1"))
                {
                    if (Time.time >= tempoRecargaVortice)
                    {
                        ConjurarVorticeDeTrama();
                    }
                    else
                    {
                        Debug.Log("Vórtice de Trama em recarga!");
                    }
                }
                break;
        }
    }

    // --- EVOCAÇÃO: DARDO ARCANO ---
    void DispararDardoArcano()
    {
        if (cameraJogador == null) return;

        // Ponto de mira
        Ray ray = cameraJogador.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 pontoAlvo = ray.origin + ray.direction * 150f;

        if (Physics.Raycast(ray, out hit, 150f))
        {
            if (hit.transform.root != transform.root)
            {
                pontoAlvo = hit.point;
            }
        }

        Vector3 pontoOrigem = modeloVisual != null && modeloVisual.pontoDisparoCajado != null
            ? modeloVisual.pontoDisparoCajado.position
            : cameraJogador.transform.position + cameraJogador.transform.forward * 0.7f + cameraJogador.transform.right * 0.3f;

        Vector3 direcao = (pontoAlvo - pontoOrigem).normalized;

        GameObject dardo = null;
        if (prefabDardoArcano != null)
        {
            dardo = Instantiate(prefabDardoArcano, pontoOrigem, Quaternion.LookRotation(direcao));
        }
        else
        {
            dardo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dardo.transform.position = pontoOrigem;
            dardo.transform.rotation = Quaternion.LookRotation(direcao);
            dardo.transform.localScale = Vector3.one * 0.3f;
            dardo.AddComponent<DardoArcano>();
        }

        Rigidbody rb = dardo.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direcao * 55f;
        }

        if (modeloVisual != null) modeloVisual.AnimarRecuoDisparo();
    }

    // --- NECROMANCIA: SIFÃO DE ALMA ---
    void ProcessarSifaoDeAlma()
    {
        canalizandoSifao = true;
        if (cameraJogador == null) return;

        Ray ray = cameraJogador.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        float alcanceSifao = 16f;

        Vector3 pontoOrigem = modeloVisual != null && modeloVisual.pontoDisparoCajado != null
            ? modeloVisual.pontoDisparoCajado.position
            : cameraJogador.transform.position + cameraJogador.transform.forward * 0.7f;

        Vector3 pontoFinal = pontoOrigem + cameraJogador.transform.forward * alcanceSifao;

        if (Physics.Raycast(ray, out hit, alcanceSifao))
        {
            if (hit.transform.root != transform.root)
            {
                pontoFinal = hit.point;

                SistemaDeVida vidaInimigo = hit.transform.GetComponent<SistemaDeVida>();
                if (vidaInimigo == null) vidaInimigo = hit.transform.GetComponentInParent<SistemaDeVida>();

                if (vidaInimigo != null)
                {
                    // Dano contínuo
                    float danoTick = danoPorSegundoSifao * Time.deltaTime;
                    vidaInimigo.ReceberDano(danoTick, new Color(0.75f, 0.15f, 1f));

                    // Aplica Marca de Necromancia
                    MarcaMagica marca = hit.transform.GetComponent<MarcaMagica>();
                    if (marca == null) marca = hit.transform.GetComponentInParent<MarcaMagica>();
                    if (marca == null) marca = hit.transform.gameObject.AddComponent<MarcaMagica>();
                    marca.AdicionarMarca(EscolaMagica.Necromancia);

                    // Cura o Mago
                    if (statusJogador != null)
                    {
                        statusJogador.Curar(curaPorSegundoSifao * Time.deltaTime);
                    }
                }
            }
        }

        // Atualiza feixe visual
        if (modeloVisual != null && modeloVisual.feixeSifaoNecro != null)
        {
            modeloVisual.feixeSifaoNecro.enabled = true;
            modeloVisual.feixeSifaoNecro.SetPosition(0, pontoOrigem);
            modeloVisual.feixeSifaoNecro.SetPosition(1, pontoFinal);
        }
    }

    void PararSifao()
    {
        canalizandoSifao = false;
        if (modeloVisual != null && modeloVisual.feixeSifaoNecro != null)
        {
            modeloVisual.feixeSifaoNecro.enabled = false;
        }
    }

    // --- CONJURAÇÃO: VÓRTICE DE TRAMA ---
    void ConjurarVorticeDeTrama()
    {
        if (cameraJogador == null) return;

        Ray ray = cameraJogador.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 pontoSpawn = cameraJogador.transform.position + cameraJogador.transform.forward * 12f;

        if (Physics.Raycast(ray, out hit, 25f))
        {
            pontoSpawn = hit.point;
        }

        tempoRecargaVortice = Time.time + cooldownVortice;

        if (prefabVorticeTrama != null)
        {
            Instantiate(prefabVorticeTrama, pontoSpawn, Quaternion.identity);
        }
        else
        {
            GameObject vort = new GameObject("VorticeDeTrama");
            vort.transform.position = pontoSpawn;
            vort.AddComponent<VorticeTrama>();
        }

        if (modeloVisual != null) modeloVisual.AnimarRecuoDisparo();
    }

    // --- SUPREMA: DESINTEGRAÇÃO (F) ---
    IEnumerator ExecutarSupremaDesintegracao()
    {
        canalizandoSuprema = true;
        tempoRecargaSuprema = Time.time + cooldownSuprema;

        if (canalizandoSifao) PararSifao();

        float tempoDecorrido = 0f;
        float alcanceLaser = 80f;

        while (tempoDecorrido < duracaoSuprema)
        {
            tempoDecorrido += Time.deltaTime;

            Vector3 pontoOrigem = modeloVisual != null && modeloVisual.pontoDisparoCajado != null
                ? modeloVisual.pontoDisparoCajado.position
                : cameraJogador.transform.position + cameraJogador.transform.forward * 0.7f;

            Ray ray = cameraJogador.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 pontoFinal = pontoOrigem + cameraJogador.transform.forward * alcanceLaser;

            // Perfura todos os alvos na linha
            RaycastHit[] hits = Physics.SphereCastAll(ray.origin, 0.6f, ray.direction, alcanceLaser);
            HashSet<GameObject> atingidosNesteFrame = new HashSet<GameObject>();

            foreach (var hit in hits)
            {
                if (hit.transform.root == transform.root) continue;
                if (hit.collider.CompareTag("Player")) continue;

                GameObject rootObj = hit.transform.root.gameObject;
                if (atingidosNesteFrame.Contains(rootObj)) continue;
                atingidosNesteFrame.Add(rootObj);

                SistemaDeVida vida = hit.transform.GetComponent<SistemaDeVida>();
                if (vida == null) vida = hit.transform.GetComponentInParent<SistemaDeVida>();

                if (vida != null)
                {
                    // Verifica execução molecular (<= 10% de vida)
                    if (vida.ObterPorcentagemVida() <= 0.10f)
                    {
                        vida.Desintegrar();
                    }
                    else
                    {
                        float danoTick = danoPorSegundoSuprema * Time.deltaTime;
                        vida.ReceberDano(danoTick, new Color(1f, 0.1f, 0.6f));

                        // Se a vida caiu para <= 10% após o dano, desintegra imediatamente!
                        if (vida.ObterPorcentagemVida() <= 0.10f)
                        {
                            vida.Desintegrar();
                        }
                    }
                }
            }

            if (modeloVisual != null && modeloVisual.feixeLaserSuprema != null)
            {
                modeloVisual.feixeLaserSuprema.enabled = true;
                modeloVisual.feixeLaserSuprema.SetPosition(0, pontoOrigem);
                modeloVisual.feixeLaserSuprema.SetPosition(1, pontoFinal);
            }

            yield return null;
        }

        if (modeloVisual != null && modeloVisual.feixeLaserSuprema != null)
        {
            modeloVisual.feixeLaserSuprema.enabled = false;
        }

        canalizandoSuprema = false;
    }

    // --- GETTERS PARA O HUD ---
    public float ObterCooldownVorticeRestante() => Mathf.Max(0f, tempoRecargaVortice - Time.time);
    public float ObterCooldownVorticeTotal() => cooldownVortice;

    public float ObterCooldownSaltoRestante() => Mathf.Max(0f, tempoRecargaSalto - Time.time);
    public float ObterCooldownSaltoTotal() => cooldownSaltoEtereo;
    public void DispararCooldownSalto() => tempoRecargaSalto = Time.time + cooldownSaltoEtereo;
    public bool PodeUsarSalto() => Time.time >= tempoRecargaSalto;

    public float ObterCooldownSupremaRestante() => Mathf.Max(0f, tempoRecargaSuprema - Time.time);
    public float ObterCooldownSupremaTotal() => cooldownSuprema;

    public bool EstaCanalizandoSuprema() => canalizandoSuprema;
}
