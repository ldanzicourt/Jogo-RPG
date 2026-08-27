using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EstiloLutaRanger
{
    Arquearia,       // Foco em disparos à distância e precisão
    DuasArmas        // Foco em combate corpo-a-corpo rápido com par de cimitarras
}

public class ControleRanger : MonoBehaviour
{
    [Header("Estilo de Luta")]
    public EstiloLutaRanger estiloAtual = EstiloLutaRanger.Arquearia;

    [Header("Configurações do Arco (Arquearia)")]
    public GameObject prefabFlecha;
    public float danoFlecha = 24f;
    public float cadenciaArco = 0.38f;
    public float velocidadeFlecha = 55f;
    private float proximoTiroArcoPermitido = 0f;

    [Header("Configurações das Cimitarras (Duas Armas)")]
    public float alcanceCimitarra = 3.5f;
    public float danoGolpe1 = 22f;
    public float danoGolpe2 = 24f;
    public float danoGolpe3 = 38f;
    public float tempoResetCombo = 1.0f;
    private int indiceCombo = 0;
    private float tempoUltimoGolpe = 0f;
    private bool atacandoCimitarra = false;

    [Header("Defesa: Esquiva do Rastreador e Armadilha (RMB)")]
    public float cooldownEsquiva = 5.0f;
    public float forcaSaltoCostas = 14f;
    public int maximoArmadilhasAtivas = 2;
    private float proximaEsquivaPermitida = 0f;
    private List<GameObject> armadilhasAtivas = new List<GameObject>();

    [Header("Mobilidade: Kit de Escalada / Gancho (Shift)")]
    public float cooldownGancho = 6.0f;
    public float alcanceGancho = 25f;
    public float velocidadePuxaoGancho = 28f;
    private float proximoGanchoPermitido = 0f;
    private bool emPuxaoGancho = false;

    [Header("Suprema: Conjurar Saraivada (F)")]
    public float cooldownSuprema = 25.0f;
    private float proximaSupremaPermitida = 0f;

    [Header("Referências")]
    public Camera cameraJogador;
    public PlayerController playerController;
    public ModeloRangerProcedural modeloRanger;

    // Passiva: Marca do Caçador
    private GameObject ultimoAlvoAtacado = null;
    private int acertosConsecutivos = 0;

    void Awake()
    {
        if (cameraJogador == null) cameraJogador = Camera.main;
        if (playerController == null) playerController = GetComponentInParent<PlayerController>();
        if (playerController == null) playerController = FindAnyObjectByType<PlayerController>();

        if (modeloRanger == null) modeloRanger = GetComponent<ModeloRangerProcedural>();
        if (modeloRanger == null) modeloRanger = GetComponentInChildren<ModeloRangerProcedural>();
        if (modeloRanger == null) modeloRanger = gameObject.AddComponent<ModeloRangerProcedural>();
    }

    void Start()
    {
        // Garante que o modelo inicie na postura correta
        if (modeloRanger != null)
        {
            modeloRanger.DefinirEstiloVisual(estiloAtual == EstiloLutaRanger.Arquearia, estiloAtual == EstiloLutaRanger.DuasArmas);
        }
    }

    void Update()
    {
        // Se o jogo estiver pausado, não processa comandos de combate
        if (Time.timeScale <= 0.001f) return;

        // 1. ALTERNAR ESTILO DE LUTA (Ctrl ou Scroll do Mouse)
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            AlternarEstiloDeLuta();
        }

        // 2. DEFESA: ESQUIVA DO RASTREADOR & ARMADILHA (Clique Direito / RMB)
        if ((Input.GetMouseButtonDown(1) || Input.GetButtonDown("Fire2")) && Time.time >= proximaEsquivaPermitida && !emPuxaoGancho)
        {
            ExecutarEsquivaEArmadilha();
        }

        // 3. MOBILIDADE: KIT DE ESCALADA / GANCHO (Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= proximoGanchoPermitido && !emPuxaoGancho)
        {
            DispararGanchoEscalada();
        }

        // 4. SUPREMA: CONJURAR SARAIVADA (F)
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= proximaSupremaPermitida)
        {
            ConjurarSaraivada();
        }

        // Se estiver em animação de gancho, não ataca
        if (emPuxaoGancho) return;

        // 5. ATAQUE BÁSICO (Clique Esquerdo / LMB / Fire1)
        if (estiloAtual == EstiloLutaRanger.Arquearia)
        {
            // Disparos do Arco Curto
            if (Input.GetButton("Fire1") && Time.time >= proximoTiroArcoPermitido)
            {
                proximoTiroArcoPermitido = Time.time + cadenciaArco;
                DispararArco();
            }
        }
        else
        {
            // Combo das Duas Cimitarras
            if (indiceCombo > 0 && Time.time > tempoUltimoGolpe + tempoResetCombo)
            {
                indiceCombo = 0;
            }

            if (Input.GetButtonDown("Fire1") && !atacandoCimitarra)
            {
                ExecutarComboCimitarras();
            }
        }
    }

    // --- 1. ESTILO DE LUTA (Ctrl) ---
    public void AlternarEstiloDeLuta()
    {
        estiloAtual = (estiloAtual == EstiloLutaRanger.Arquearia) ? EstiloLutaRanger.DuasArmas : EstiloLutaRanger.Arquearia;
        if (modeloRanger != null)
        {
            modeloRanger.TransicionarEstilo(estiloAtual == EstiloLutaRanger.Arquearia);
        }
        Debug.Log("🏹 Estilo de Luta alterado para: " + estiloAtual.ToString());
    }

    // --- 2. ATAQUE BÁSICO: ARCO CURTO (Arquearia) ---
    void DispararArco()
    {
        if (cameraJogador == null) return;

        // Ponto de mira central
        Ray ray = cameraJogador.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, 250f);
        Vector3 pontoAlvo = ray.origin + ray.direction * 250f;

        float menorDistancia = float.MaxValue;
        foreach (var hit in hits)
        {
            if (hit.transform.root == transform.root || hit.collider.isTrigger) continue;
            if (hit.distance < menorDistancia && hit.distance > 0.4f)
            {
                menorDistancia = hit.distance;
                pontoAlvo = hit.point;
            }
        }

        Vector3 pontoOrigem = modeloRanger != null && modeloRanger.pontoDisparoFlecha != null
            ? modeloRanger.pontoDisparoFlecha.position
            : cameraJogador.transform.position + cameraJogador.transform.forward * 0.8f + cameraJogador.transform.right * 0.25f - cameraJogador.transform.up * 0.15f;

        Vector3 direcao = (pontoAlvo - pontoOrigem).normalized;

        // Animação de recuo/soltura do arco
        if (modeloRanger != null)
        {
            StartCoroutine(modeloRanger.ExecutarAnimacaoTiroArco(cadenciaArco));
        }

        // Instancia a Flecha
        GameObject flechaObj = null;
        if (prefabFlecha != null)
        {
            flechaObj = Instantiate(prefabFlecha, pontoOrigem, Quaternion.LookRotation(direcao));
        }
        else
        {
            flechaObj = CriarFlechaGenerica(pontoOrigem, direcao);
        }

        // Configura dano e passiva da flecha
        Flecha scriptFlecha = flechaObj.GetComponent<Flecha>();
        if (scriptFlecha != null)
        {
            scriptFlecha.dano = danoFlecha;
        }

        // Ignora colisão com o próprio jogador
        Collider flechaCol = flechaObj.GetComponent<Collider>();
        Collider playerCol = playerController != null ? playerController.GetComponent<Collider>() : null;
        if (flechaCol != null && playerCol != null)
        {
            Physics.IgnoreCollision(flechaCol, playerCol, true);
        }

        // Aplica velocidade balística
        Rigidbody rb = flechaObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = direcao * velocidadeFlecha;
        }
    }

    GameObject CriarFlechaGenerica(Vector3 pos, Vector3 dir)
    {
        GameObject flecha = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flecha.name = "FlechaRanger";
        flecha.transform.position = pos;
        flecha.transform.rotation = Quaternion.LookRotation(dir);
        flecha.transform.localScale = new Vector3(0.02f, 0.4f, 0.02f);

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
        Material mat = new Material(litShader);
        mat.color = new Color(0.2f, 0.15f, 0.10f);
        flecha.GetComponent<Renderer>().material = mat;

        Rigidbody rb = flecha.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        flecha.AddComponent<Flecha>();

        return flecha;
    }

    // --- 3. ATAQUE BÁSICO: DUAS CIMITARRAS (Combate Duplo) ---
    void ExecutarComboCimitarras()
    {
        indiceCombo++;
        if (indiceCombo > 3) indiceCombo = 1;

        tempoUltimoGolpe = Time.time;
        float duracaoGolpe = (indiceCombo == 3) ? 0.38f : 0.26f;

        StartCoroutine(RotinaGolpeCimitarra(indiceCombo, duracaoGolpe));
    }

    IEnumerator RotinaGolpeCimitarra(int combo, float duracao)
    {
        atacandoCimitarra = true;

        if (modeloRanger != null)
        {
            StartCoroutine(modeloRanger.ExecutarAnimacaoCimitarras(combo, duracao));
        }

        yield return new WaitForSeconds(duracao * 0.35f);

        // Processa o acerto corpo a corpo
        ProcessarAcertoCimitarra(combo);

        yield return new WaitForSeconds(duracao * 0.65f);
        atacandoCimitarra = false;
    }

    void ProcessarAcertoCimitarra(int combo)
    {
        if (cameraJogador == null) return;

        Ray raio = new Ray(cameraJogador.transform.position, cameraJogador.transform.forward);
        RaycastHit[] hits = Physics.SphereCastAll(raio, 0.7f, alcanceCimitarra);

        HashSet<GameObject> atingidos = new HashSet<GameObject>();
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Player") || hit.collider.GetComponent<PlayerController>() != null) continue;

            GameObject root = hit.collider.transform.root.gameObject;
            if (atingidos.Contains(root)) continue;
            atingidos.Add(root);

            SistemaDeVida vida = hit.collider.GetComponent<SistemaDeVida>();
            if (vida == null) vida = hit.collider.GetComponentInParent<SistemaDeVida>();

            if (vida != null)
            {
                float danoBase = (combo == 1) ? danoGolpe1 : (combo == 2 ? danoGolpe2 : danoGolpe3);

                // Aplica lógica da Passiva: Marca do Caçador
                RegistrarAcertoPassiva(hit.collider.gameObject, vida, danoBase);
            }
        }
    }

    // --- 4. PASSIVA: MARCA DO CAÇADOR ---
    public void RegistrarAcertoPassiva(GameObject alvo, SistemaDeVida vida, float danoBase)
    {
        if (alvo == null || vida == null) return;

        GameObject raizAlvo = alvo.transform.root.gameObject;

        // Se acertou o mesmo alvo consecutivamente
        if (ultimoAlvoAtacado == raizAlvo)
        {
            acertosConsecutivos++;
        }
        else
        {
            ultimoAlvoAtacado = raizAlvo;
            acertosConsecutivos = 1;
        }

        MarcaDoCacador marca = raizAlvo.GetComponent<MarcaDoCacador>();
        if (marca == null) marca = raizAlvo.GetComponentInChildren<MarcaDoCacador>();

        // Ao atingir 3 acertos consecutivos, aplica a Marca do Caçador!
        if (acertosConsecutivos >= 3 && marca == null)
        {
            marca = raizAlvo.AddComponent<MarcaDoCacador>();
            Debug.Log("🎯 MARCA DO CAÇADOR ATIVADA no alvo " + raizAlvo.name + "!");
        }
        else if (marca != null)
        {
            marca.RenovarDuracao();
        }

        // Calcula dano final
        float danoTotal = danoBase;
        Color corTexto = new Color(0.9f, 0.9f, 0.9f); // Branco padrão

        if (marca != null)
        {
            int danoBonus1d6 = marca.RolarDanoBonus();
            danoTotal += danoBonus1d6;
            corTexto = new Color(1f, 0.78f, 0.15f); // Dourado/Âmbar para o dano da Marca do Caçador
        }

        vida.ReceberDano(danoTotal, corTexto);
    }

    // --- 5. DEFESA: ESQUIVA DO RASTREADOR E ARMADILHA (RMB) ---
    void ExecutarEsquivaEArmadilha()
    {
        proximaEsquivaPermitida = Time.time + cooldownEsquiva;

        // Posição onde a armadilha de caça será colocada
        Vector3 posArmadilha = transform.position;
        RaycastHit hitChao;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out hitChao, 3f))
        {
            posArmadilha = hitChao.point;
        }

        // Instancia Armadilha de Caça física
        GameObject armadilhaObj = new GameObject("ArmadilhaDeCaca");
        armadilhaObj.transform.position = posArmadilha;
        armadilhaObj.AddComponent<ArmadilhaDeCaca>();

        // Gerencia limite de armadilhas (máximo 2 ativas)
        armadilhasAtivas.RemoveAll(item => item == null);
        if (armadilhasAtivas.Count >= maximoArmadilhasAtivas)
        {
            if (armadilhasAtivas[0] != null) Destroy(armadilhasAtivas[0]);
            armadilhasAtivas.RemoveAt(0);
        }
        armadilhasAtivas.Add(armadilhaObj);

        // Executa o mortal para trás
        StartCoroutine(RotinaMortalParaTras());

        Debug.Log("🛡️ ESQUIVA DO RASTREADOR: Mortal para trás realizado e Armadilha armada!");
    }

    IEnumerator RotinaMortalParaTras()
    {
        CharacterController cc = playerController != null ? playerController.GetComponent<CharacterController>() : GetComponent<CharacterController>();
        if (cc == null) yield break;

        Vector3 dirSalto = (-transform.forward * 1.5f + Vector3.up * 0.8f).normalized;
        float duracaoSalto = 0.35f;
        float t = 0f;

        while (t < duracaoSalto)
        {
            t += Time.deltaTime;
            float velocidade = forcaSaltoCostas * (1f - (t / duracaoSalto));
            cc.Move(dirSalto * velocidade * Time.deltaTime);
            yield return null;
        }
    }

    // --- 6. MOBILIDADE: KIT DE ESCALADA / GANCHO (Shift) ---
    void DispararGanchoEscalada()
    {
        if (cameraJogador == null || emPuxaoGancho) return;

        proximoGanchoPermitido = Time.time + cooldownGancho;
        StartCoroutine(RotinaLancarGancho());
    }

    IEnumerator RotinaLancarGancho()
    {
        emPuxaoGancho = true;
        CharacterController cc = playerController != null ? playerController.GetComponent<CharacterController>() : GetComponent<CharacterController>();

        Vector3 direcaoDisparo = cameraJogador.transform.forward;
        Vector3 posOrigem = transform.position + transform.forward * 0.3f + transform.right * 0.2f - transform.up * 0.15f;
        Vector3 posGancho = posOrigem;

        float velocidadeVoo = 55f;
        float distanciaPercorrida = 0f;
        bool acertouSuperficie = false;
        Vector3 pontoImpacto = Vector3.zero;

        // 1. O Gancho viaja pelo ar na direção mirada
        while (distanciaPercorrida < alcanceGancho)
        {
            float passo = velocidadeVoo * Time.deltaTime;
            Vector3 proximaPos = posGancho + direcaoDisparo * passo;

            // Checagem de colisão contínua
            RaycastHit hit;
            if (Physics.Raycast(posGancho, direcaoDisparo, out hit, passo))
            {
                if (!hit.collider.CompareTag("Player") && !hit.collider.isTrigger && hit.transform.root != transform.root)
                {
                    acertouSuperficie = true;
                    pontoImpacto = hit.point;
                    posGancho = hit.point;
                    break;
                }
            }

            posGancho = proximaPos;
            distanciaPercorrida += passo;

            Vector3 origemAtual = transform.position + transform.forward * 0.3f + transform.right * 0.2f - transform.up * 0.15f;
            if (modeloRanger != null)
            {
                modeloRanger.AtualizarLinhaGancho(origemAtual, posGancho, true);
            }

            yield return null;
        }

        // 2. Se acertou uma superfície válida: puxa o jogador rapidamente!
        if (acertouSuperficie)
        {
            CriarFaiscasImpacto(pontoImpacto);

            float tempoMaximoPuxao = 1.2f;
            float tempoDecorrido = 0f;

            while (tempoDecorrido < tempoMaximoPuxao)
            {
                tempoDecorrido += Time.deltaTime;

                Vector3 origemAtual = transform.position + transform.forward * 0.3f + transform.right * 0.2f - transform.up * 0.15f;
                if (modeloRanger != null)
                {
                    modeloRanger.AtualizarLinhaGancho(origemAtual, pontoImpacto, true);
                }

                Vector3 vetorAoDestino = (pontoImpacto - transform.position);
                float distancia = vetorAoDestino.magnitude;

                if (distancia <= 1.4f)
                {
                    break; // Chegou ao destino
                }

                Vector3 dirPuxao = vetorAoDestino.normalized;
                if (cc != null)
                {
                    cc.Move(dirPuxao * velocidadePuxaoGancho * Time.deltaTime);
                }

                yield return null;
            }

            // Impulso vertical final para conquistar o High Ground com facilidade
            if (cc != null)
            {
                float tPouso = 0f;
                while (tPouso < 0.16f)
                {
                    tPouso += Time.deltaTime;
                    cc.Move(Vector3.up * 8f * Time.deltaTime);
                    yield return null;
                }
            }
            Debug.Log("🧗 GANCHO: High Ground alcançado com sucesso!");
        }
        else
        {
            // 3. Se NÃO acertou nada: o gancho retorna/recolhe em alta velocidade até o Ranger!
            Debug.Log("↩️ GANCHO: Alcance máximo atingido sem ancorar. Recolhendo corda...");
            yield return new WaitForSeconds(0.04f);

            float velocidadeRetorno = 75f;
            while (true)
            {
                Vector3 origemAtual = transform.position + transform.forward * 0.3f + transform.right * 0.2f - transform.up * 0.15f;
                Vector3 dirRetorno = (origemAtual - posGancho);
                float distRetorno = dirRetorno.magnitude;

                if (distRetorno <= 1.5f)
                {
                    break; // Recolheu completamente
                }

                posGancho += dirRetorno.normalized * velocidadeRetorno * Time.deltaTime;

                if (modeloRanger != null)
                {
                    modeloRanger.AtualizarLinhaGancho(origemAtual, posGancho, true);
                }

                yield return null;
            }
        }

        // Desativa linha e cabeça visual
        if (modeloRanger != null)
        {
            modeloRanger.AtualizarLinhaGancho(Vector3.zero, Vector3.zero, false);
        }

        emPuxaoGancho = false;
    }

    void CriarFaiscasImpacto(Vector3 pos)
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject faisca = GameObject.CreatePrimitive(PrimitiveType.Cube);
            faisca.name = "FaiscaGancho";
            faisca.transform.position = pos + Random.insideUnitSphere * 0.15f;
            faisca.transform.localScale = Vector3.one * 0.05f;
            Destroy(faisca.GetComponent<Collider>());

            Renderer r = faisca.GetComponent<Renderer>();
            if (r != null)
            {
                Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
                Material mat = new Material(litShader);
                mat.color = new Color(1f, 0.9f, 0.4f);
                r.material = mat;
            }

            Destroy(faisca, 0.25f);
        }
    }

    // --- 7. SUPREMA: CONJURAR SARAIVADA (F) ---
    void ConjurarSaraivada()
    {
        proximaSupremaPermitida = Time.time + cooldownSuprema;

        // Mira o centro da saraivada
        Vector3 pontoCentro = transform.position + transform.forward * 14f;
        Ray ray = cameraJogador.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 40f))
        {
            pontoCentro = hit.point;
        }

        // Garante que o centro fique rente ao solo
        RaycastHit hitSolo;
        if (Physics.Raycast(pontoCentro + Vector3.up * 3f, Vector3.down, out hitSolo, 10f))
        {
            pontoCentro = hitSolo.point;
        }

        // Disparo para o alto com o arco
        if (modeloRanger != null)
        {
            StartCoroutine(modeloRanger.ExecutarAnimacaoTiroArco(0.4f));
        }

        // Cria a entidade da Saraivada
        GameObject saraivadaObj = new GameObject("SaraivadaDeFlechas");
        saraivadaObj.transform.position = pontoCentro;
        saraivadaObj.AddComponent<SaraivadaFlechas>();

        Debug.Log("🏹 SUPREMA: Conjurar Saraivada disparada em área cilíndrica de 12m de raio!");
    }

    // --- GETTERS PARA O HUD ---
    public float ObterTempoRestanteGancho() => Mathf.Max(0f, proximoGanchoPermitido - Time.time);
    public float ObterCooldownGanchoTotal() => cooldownGancho;

    public float ObterTempoRestanteEsquiva() => Mathf.Max(0f, proximaEsquivaPermitida - Time.time);
    public float ObterCooldownEsquivaTotal() => cooldownEsquiva;

    public float ObterTempoRestanteSuprema() => Mathf.Max(0f, proximaSupremaPermitida - Time.time);
    public float ObterCooldownSupremaTotal() => cooldownSuprema;

    public int ObterAcertosConsecutivos() => acertosConsecutivos;
    public int ObterQtdArmadilhasAtivas()
    {
        armadilhasAtivas.RemoveAll(item => item == null);
        return armadilhasAtivas.Count;
    }
}
