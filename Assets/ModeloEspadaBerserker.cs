using System.Collections;
using UnityEngine;

public class ModeloEspadaBerserker : MonoBehaviour
{
    [Header("Componentes Visuais da Espada")]
    public Transform raizEspada;
    public Renderer renderizadorLaminas;
    public Renderer renderizadorRunas;
    public Light luzFuria;
    public TrailRenderer rastroCorte;

    [Header("Posições e Rotações das Posturas (Espaço da Câmera)")]
    // A espada é construída apontando para FRENTE (+Z). O cabo está em -Z (mãos do jogador).
    // 1. Postura Vertical: Cabo no canto inferior direito, lâmina apontando para frente e inclinada para cima
    public Vector3 posVertical = new Vector3(0.35f, -0.38f, 0.55f);
    public Quaternion rotVertical = Quaternion.Euler(-28f, -12f, 18f);

    // 2. Postura Horizontal: Lâmina deitada a 90°, estendida para frente na parte inferior da tela
    public Vector3 posHorizontal = new Vector3(0.25f, -0.38f, 0.55f);
    public Quaternion rotHorizontal = Quaternion.Euler(0f, -25f, 90f);

    // 3. Guarda Bruta: Lâmina na diagonal cruzando o peito/tela
    public Vector3 posGuarda = new Vector3(0.05f, -0.22f, 0.50f);
    public Quaternion rotGuarda = Quaternion.Euler(-48f, 8f, -45f);

    private Vector3 posAlvo;
    private Quaternion rotAlvo;
    private bool animandoGolpe = false;
    private bool ladoHorizontalEsquerdo = false;

    void Awake()
    {
        transform.localScale = Vector3.one;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        ConstruirEspadaSeNecessario();
    }

    void Start()
    {
        posAlvo = posVertical;
        rotAlvo = rotVertical;
        if (raizEspada != null)
        {
            raizEspada.localPosition = posVertical;
            raizEspada.localRotation = rotVertical;
        }
    }

    void Update()
    {
        if (!animandoGolpe && raizEspada != null)
        {
            raizEspada.localPosition = Vector3.Lerp(raizEspada.localPosition, posAlvo, Time.deltaTime * 14f);
            raizEspada.localRotation = Quaternion.Slerp(raizEspada.localRotation, rotAlvo, Time.deltaTime * 14f);

            // Leve respiração ociosa
            float bob = Mathf.Sin(Time.time * 2.2f) * 0.004f;
            raizEspada.localPosition += new Vector3(0f, bob, 0f);
        }
    }

    public void DefinirPostura(PosturaEspada postura, bool emGuarda)
    {
        if (emGuarda)
        {
            posAlvo = posGuarda;
            rotAlvo = rotGuarda;
        }
        else if (postura == PosturaEspada.Horizontal)
        {
            posAlvo = ladoHorizontalEsquerdo 
                ? new Vector3(-0.25f, -0.38f, 0.55f) 
                : posHorizontal;
            rotAlvo = ladoHorizontalEsquerdo 
                ? Quaternion.Euler(0f, 25f, -90f) 
                : rotHorizontal;
        }
        else
        {
            ladoHorizontalEsquerdo = false;
            posAlvo = posVertical;
            rotAlvo = rotVertical;
        }
    }

    public void AtualizarBrilhoFuria(float porcentagemFuria, bool emFrenesi)
    {
        Color corRuna = emFrenesi 
            ? new Color(1f, 0.08f, 0.08f) * 3.5f
            : Color.Lerp(new Color(0.25f, 0.06f, 0f), new Color(1f, 0.35f, 0f) * 2f, porcentagemFuria);

        if (renderizadorRunas != null && renderizadorRunas.material != null)
        {
            renderizadorRunas.material.color = corRuna;
            if (renderizadorRunas.material.HasProperty("_EmissionColor"))
            {
                renderizadorRunas.material.EnableKeyword("_EMISSION");
                renderizadorRunas.material.SetColor("_EmissionColor", corRuna);
            }
        }

        if (luzFuria != null)
        {
            luzFuria.color = emFrenesi ? Color.red : Color.Lerp(Color.black, new Color(1f, 0.4f, 0.1f), porcentagemFuria);
            luzFuria.intensity = emFrenesi ? 2.5f : porcentagemFuria * 1.8f;
        }
    }

    // --- CORTE VERTICAL / DESCENDENTE ---
    public IEnumerator ExecutarAnimacaoVertical(float duracaoGolpe, bool emFrenesi)
    {
        animandoGolpe = true;
        if (rastroCorte != null) rastroCorte.emitting = true;

        Vector3 posInicial = raizEspada.localPosition;
        Quaternion rotInicial = raizEspada.localRotation;

        // Wind-up: Ergue a lâmina para trás no alto à direita
        Vector3 posRecuo = posInicial + new Vector3(0.08f, 0.10f, -0.12f);
        Quaternion rotRecuo = rotInicial * Quaternion.Euler(-30f, 15f, -8f);

        // Corte potente descendo para a esquerda
        Vector3 posCorte = posInicial + new Vector3(-0.40f, -0.15f, 0.08f);
        Quaternion rotCorte = rotInicial * Quaternion.Euler(40f, -55f, 35f);

        float tempoWindup = duracaoGolpe * 0.25f;
        float tempoCorte = duracaoGolpe * 0.45f;
        float tempoRecuperacao = duracaoGolpe * 0.30f;

        float t = 0f;
        while (t < tempoWindup)
        {
            t += Time.deltaTime;
            raizEspada.localPosition = Vector3.Lerp(posInicial, posRecuo, t / tempoWindup);
            raizEspada.localRotation = Quaternion.Slerp(rotInicial, rotRecuo, t / tempoWindup);
            yield return null;
        }

        t = 0f;
        while (t < tempoCorte)
        {
            t += Time.deltaTime;
            float curva = Mathf.SmoothStep(0f, 1f, t / tempoCorte);
            raizEspada.localPosition = Vector3.Lerp(posRecuo, posCorte, curva);
            raizEspada.localRotation = Quaternion.Slerp(rotRecuo, rotCorte, curva);
            yield return null;
        }

        if (rastroCorte != null) rastroCorte.emitting = false;

        t = 0f;
        while (t < tempoRecuperacao)
        {
            t += Time.deltaTime;
            raizEspada.localPosition = Vector3.Lerp(posCorte, posAlvo, t / tempoRecuperacao);
            raizEspada.localRotation = Quaternion.Slerp(rotCorte, rotAlvo, t / tempoRecuperacao);
            yield return null;
        }

        animandoGolpe = false;
    }

    // --- CORTE HORIZONTAL EM VASSOURA (A lâmina sempre lidera o golpe à frente) ---
    public IEnumerator ExecutarAnimacaoHorizontal(float duracaoGolpe, bool emFrenesi, bool daEsquerdaParaDireita)
    {
        animandoGolpe = true;
        if (rastroCorte != null) rastroCorte.emitting = true;

        Vector3 posInicio;
        Quaternion rotInicio;
        Vector3 posFim;
        Quaternion rotFim;

        if (!daEsquerdaParaDireita)
        {
            // 1. Corte da DIREITA para a ESQUERDA:
            // O cabo fica à direita, a lâmina horizontal aponta para a frente-esquerda e varre até o lado esquerdo
            posInicio = new Vector3(0.35f, -0.36f, 0.55f);
            rotInicio = Quaternion.Euler(0f, -50f, 90f);

            posFim = new Vector3(-0.35f, -0.36f, 0.55f);
            rotFim = Quaternion.Euler(0f, 50f, 90f);

            ladoHorizontalEsquerdo = true;
        }
        else
        {
            // 2. Corte da ESQUERDA para a DIREITA:
            // O cabo fica à esquerda, a lâmina horizontal aponta para a frente-direita e varre de volta para a direita
            posInicio = new Vector3(-0.35f, -0.36f, 0.55f);
            rotInicio = Quaternion.Euler(0f, 50f, -90f);

            posFim = new Vector3(0.35f, -0.36f, 0.55f);
            rotFim = Quaternion.Euler(0f, -50f, -90f);

            ladoHorizontalEsquerdo = false;
        }

        float tempoWindup = duracaoGolpe * 0.15f;
        float tempoVarredura = duracaoGolpe * 0.55f;
        float tempoRecuperacao = duracaoGolpe * 0.30f;

        // 1. Preparação (Wind-up curto)
        float t = 0f;
        Vector3 posOrigem = raizEspada.localPosition;
        Quaternion rotOrigem = raizEspada.localRotation;
        while (t < tempoWindup)
        {
            t += Time.deltaTime;
            raizEspada.localPosition = Vector3.Lerp(posOrigem, posInicio, t / tempoWindup);
            raizEspada.localRotation = Quaternion.Slerp(rotOrigem, rotInicio, t / tempoWindup);
            yield return null;
        }

        // 2. Varredura com a Lâmina cortando à frente no ar
        t = 0f;
        while (t < tempoVarredura)
        {
            t += Time.deltaTime;
            float curva = Mathf.SmoothStep(0f, 1f, t / tempoVarredura);
            raizEspada.localPosition = Vector3.Lerp(posInicio, posFim, curva);
            raizEspada.localRotation = Quaternion.Slerp(rotInicio, rotFim, curva);
            yield return null;
        }

        if (rastroCorte != null) rastroCorte.emitting = false;

        // Posição de repouso no lado onde o golpe terminou
        posAlvo = ladoHorizontalEsquerdo ? new Vector3(-0.25f, -0.38f, 0.55f) : posHorizontal;
        rotAlvo = ladoHorizontalEsquerdo ? Quaternion.Euler(0f, 25f, -90f) : rotHorizontal;

        // 3. Recuperação suave
        t = 0f;
        while (t < tempoRecuperacao)
        {
            t += Time.deltaTime;
            raizEspada.localPosition = Vector3.Lerp(posFim, posAlvo, t / tempoRecuperacao);
            raizEspada.localRotation = Quaternion.Slerp(rotFim, rotAlvo, t / tempoRecuperacao);
            yield return null;
        }

        animandoGolpe = false;
    }

    public void GerarFaiscasBloqueio()
    {
        StartCoroutine(RotinaFaiscas());
    }

    private IEnumerator RotinaFaiscas()
    {
        if (raizEspada == null) yield break;

        Vector3 posRecuo = posGuarda - Vector3.forward * 0.05f;
        raizEspada.localPosition = posRecuo;

        GameObject faisca = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        faisca.name = "FaiscaBloqueio";
        faisca.transform.position = raizEspada.position + transform.forward * 0.5f;
        faisca.transform.localScale = Vector3.one * 0.15f;
        Destroy(faisca.GetComponent<Collider>());

        Renderer rend = faisca.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = new Color(1f, 0.85f, 0.2f);
            rend.material = mat;
        }

        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            if (faisca != null) faisca.transform.localScale = Vector3.Lerp(Vector3.one * 0.15f, Vector3.one * 0.35f, t / 0.12f);
            yield return null;
        }

        if (faisca != null) Destroy(faisca);
    }

    void ConstruirEspadaSeNecessario()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

        // Limpa filhos antigos se houver
        Transform existente = transform.Find("EspadaBerserker");
        if (existente != null)
        {
            Destroy(existente.gameObject);
        }

        // Cria a raiz da espada com escala uniforme 1,1,1
        // A raiz é o ponto pivô na empunhadura/guarda (z = 0)
        GameObject objRaiz = new GameObject("EspadaBerserker");
        objRaiz.transform.SetParent(transform, false);
        objRaiz.transform.localPosition = posVertical;
        objRaiz.transform.localRotation = rotVertical;
        objRaiz.transform.localScale = Vector3.one;
        raizEspada = objRaiz.transform;

        Material matFerroEscuro = new Material(litShader);
        matFerroEscuro.color = new Color(0.12f, 0.12f, 0.14f);

        Material matAcoLamina = new Material(litShader);
        matAcoLamina.color = new Color(0.38f, 0.40f, 0.42f);

        Material matCouro = new Material(litShader);
        matCouro.color = new Color(0.18f, 0.11f, 0.08f);

        // 1. Cabo / Empunhadura de duas mãos (Eixo -Z: na direção da mão do jogador)
        GameObject cabo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cabo.name = "Cabo";
        cabo.transform.SetParent(raizEspada, false);
        cabo.transform.localPosition = new Vector3(0f, 0f, -0.15f);
        cabo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        cabo.transform.localScale = new Vector3(0.055f, 0.15f, 0.055f);
        Destroy(cabo.GetComponent<Collider>());
        cabo.GetComponent<Renderer>().material = matCouro;

        // 2. Pomo Espinhado (Extremidade mais próxima ao jogador: -Z)
        GameObject pomo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pomo.name = "Pomo";
        pomo.transform.SetParent(raizEspada, false);
        pomo.transform.localPosition = new Vector3(0f, 0f, -0.32f);
        pomo.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
        Destroy(pomo.GetComponent<Collider>());
        pomo.GetComponent<Renderer>().material = matFerroEscuro;

        // 3. Guarda Maciça com Espinhos (No pivô central: z = 0.02f)
        GameObject guarda = GameObject.CreatePrimitive(PrimitiveType.Cube);
        guarda.name = "Guarda";
        guarda.transform.SetParent(raizEspada, false);
        guarda.transform.localPosition = new Vector3(0f, 0f, 0.02f);
        guarda.transform.localScale = new Vector3(0.38f, 0.09f, 0.06f);
        Destroy(guarda.GetComponent<Collider>());
        guarda.GetComponent<Renderer>().material = matFerroEscuro;

        // Espinhos laterais na guarda
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject espinho = GameObject.CreatePrimitive(PrimitiveType.Cube);
            espinho.name = "EspinhoGuarda_" + (side > 0 ? "Dir" : "Esq");
            espinho.transform.SetParent(raizEspada, false);
            espinho.transform.localPosition = new Vector3(side * 0.20f, 0.06f, 0.02f);
            espinho.transform.localRotation = Quaternion.Euler(0f, 0f, side * -40f);
            espinho.transform.localScale = new Vector3(0.04f, 0.10f, 0.06f);
            Destroy(espinho.GetComponent<Collider>());
            espinho.GetComponent<Renderer>().material = matFerroEscuro;
        }

        // 4. Lâmina Principal Colossal (Estende para FRENTE ao longo de +Z)
        GameObject lamina = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lamina.name = "Lamina";
        lamina.transform.SetParent(raizEspada, false);
        lamina.transform.localPosition = new Vector3(0f, 0f, 0.80f);
        lamina.transform.localScale = new Vector3(0.038f, 0.18f, 1.45f);
        Destroy(lamina.GetComponent<Collider>());
        renderizadorLaminas = lamina.GetComponent<Renderer>();
        renderizadorLaminas.material = matAcoLamina;

        // 5. Ponta Afiada da Lâmina (+Z extremo)
        GameObject ponta = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ponta.name = "Ponta";
        ponta.transform.SetParent(raizEspada, false);
        ponta.transform.localPosition = new Vector3(0f, 0f, 1.55f);
        ponta.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        ponta.transform.localScale = new Vector3(0.038f, 0.125f, 0.125f);
        Destroy(ponta.GetComponent<Collider>());
        ponta.GetComponent<Renderer>().material = matAcoLamina;

        // 6. Serrilhado Berserker (Dentes na borda superior da lâmina ao longo de +Z)
        for (int i = 0; i < 4; i++)
        {
            GameObject dente = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dente.name = "Dente_" + i;
            dente.transform.SetParent(raizEspada, false);
            dente.transform.localPosition = new Vector3(0f, 0.10f, 0.35f + (i * 0.28f));
            dente.transform.localRotation = Quaternion.Euler(32f, 0f, 0f);
            dente.transform.localScale = new Vector3(0.036f, 0.055f, 0.12f);
            Destroy(dente.GetComponent<Collider>());
            dente.GetComponent<Renderer>().material = matFerroEscuro;
        }

        // 7. Ranhura de Sangue & Runas Luminosas (Face da lâmina)
        GameObject runa = GameObject.CreatePrimitive(PrimitiveType.Cube);
        runa.name = "RunaFuria";
        runa.transform.SetParent(raizEspada, false);
        runa.transform.localPosition = new Vector3(0.021f, 0f, 0.80f);
        runa.transform.localScale = new Vector3(0.015f, 0.045f, 1.25f);
        Destroy(runa.GetComponent<Collider>());

        renderizadorRunas = runa.GetComponent<Renderer>();
        Material matRuna = new Material(litShader);
        matRuna.color = new Color(0.8f, 0.2f, 0f);
        renderizadorRunas.material = matRuna;

        // 8. Luz Dinâmica de Fúria
        GameObject objLuz = new GameObject("LuzEspada");
        objLuz.transform.SetParent(raizEspada, false);
        objLuz.transform.localPosition = new Vector3(0.10f, 0f, 0.80f);
        luzFuria = objLuz.AddComponent<Light>();
        luzFuria.color = new Color(1f, 0.3f, 0f);
        luzFuria.range = 2.5f;
        luzFuria.intensity = 0.6f;

        // 9. Rastro de Corte na PONTA da lâmina (+Z extremo)
        GameObject objTrail = new GameObject("RastroCorte");
        objTrail.transform.SetParent(raizEspada, false);
        objTrail.transform.localPosition = new Vector3(0f, 0f, 1.55f);
        rastroCorte = objTrail.AddComponent<TrailRenderer>();
        rastroCorte.time = 0.16f;
        rastroCorte.startWidth = 0.45f;
        rastroCorte.endWidth = 0.04f;
        rastroCorte.emitting = false;

        Material matTrail = new Material(litShader);
        matTrail.color = new Color(1f, 0.25f, 0.05f, 0.65f);
        rastroCorte.material = matTrail;
    }
}
