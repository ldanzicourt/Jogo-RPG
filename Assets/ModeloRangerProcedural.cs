using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeloRangerProcedural : MonoBehaviour
{
    [Header("Estruturas Principais")]
    public Transform raizArmas;
    public Transform conjuntoArco;
    public Transform conjuntoCimitarras;
    public Transform cimitarraDireita;
    public Transform cimitarraEsquerda;
    public Transform pontoDisparoFlecha;
    public LineRenderer linhaGancho;
    public Transform ganchoCabecaVisual;

    [Header("Rastros de Lâmina")]
    public TrailRenderer rastroCimitarraDir;
    public TrailRenderer rastroCimitarraEsq;

    [Header("Posições em 1ª Pessoa - Arco")]
    public Vector3 posArcoRepouso = new Vector3(0.24f, -0.22f, 0.42f);
    public Quaternion rotArcoRepouso = Quaternion.Euler(12f, -18f, 22f);

    [Header("Posições em 1ª Pessoa - Cimitarras")]
    public Vector3 posCimitarraDirRepouso = new Vector3(0.26f, -0.25f, 0.42f);
    public Quaternion rotCimitarraDirRepouso = Quaternion.Euler(22f, -18f, 18f);

    public Vector3 posCimitarraEsqRepouso = new Vector3(-0.26f, -0.27f, 0.42f);
    public Quaternion rotCimitarraEsqRepouso = Quaternion.Euler(22f, 18f, -18f);

    private bool animandoAtaque = false;
    private bool animandoTroca = false;

    void Awake()
    {
        transform.localScale = Vector3.one;
        ConstruirModeloProcedural();
    }

    void Start()
    {
        // Inicia no Estilo Arquearia (Arco visível, Cimitarras guardadas)
        DefinirEstiloVisual(true, false);
    }

    void Update()
    {
        // Sway ocioso suave quando em repouso
        if (!animandoAtaque && !animandoTroca)
        {
            float bob = Mathf.Sin(Time.time * 2f) * 0.0025f;
            if (conjuntoArco != null && conjuntoArco.gameObject.activeSelf)
            {
                conjuntoArco.localPosition = posArcoRepouso + new Vector3(0f, bob, 0f);
            }

            if (cimitarraDireita != null && cimitarraEsquerda != null && conjuntoCimitarras.gameObject.activeSelf)
            {
                cimitarraDireita.localPosition = posCimitarraDirRepouso + new Vector3(0f, bob, 0f);
                cimitarraEsquerda.localPosition = posCimitarraEsqRepouso + new Vector3(0f, -bob, 0f);
            }
        }
    }

    // --- TROCA VISUAL DE ESTILO DE LUTA ---
    public void TransicionarEstilo(bool modoArquearia, float duracao = 0.30f)
    {
        StartCoroutine(RotinaTrocaEstilo(modoArquearia, duracao));
    }

    IEnumerator RotinaTrocaEstilo(bool modoArquearia, float duracao)
    {
        animandoTroca = true;
        float meioTempo = duracao * 0.5f;

        // 1. Abaixa a arma atual
        float t = 0f;
        while (t < meioTempo)
        {
            t += Time.deltaTime;
            float prog = t / meioTempo;

            if (modoArquearia)
            {
                if (cimitarraDireita != null) cimitarraDireita.localPosition = Vector3.Lerp(posCimitarraDirRepouso, posCimitarraDirRepouso - Vector3.up * 0.35f, prog);
                if (cimitarraEsquerda != null) cimitarraEsquerda.localPosition = Vector3.Lerp(posCimitarraEsqRepouso, posCimitarraEsqRepouso - Vector3.up * 0.35f, prog);
            }
            else
            {
                if (conjuntoArco != null) conjuntoArco.localPosition = Vector3.Lerp(posArcoRepouso, posArcoRepouso - Vector3.up * 0.35f, prog);
            }
            yield return null;
        }

        // 2. Alterna visibilidade
        DefinirEstiloVisual(modoArquearia, !modoArquearia);

        // 3. Ergue a nova arma
        t = 0f;
        while (t < meioTempo)
        {
            t += Time.deltaTime;
            float prog = t / meioTempo;

            if (modoArquearia)
            {
                if (conjuntoArco != null) conjuntoArco.localPosition = Vector3.Lerp(posArcoRepouso - Vector3.up * 0.35f, posArcoRepouso, prog);
            }
            else
            {
                if (cimitarraDireita != null) cimitarraDireita.localPosition = Vector3.Lerp(posCimitarraDirRepouso - Vector3.up * 0.35f, posCimitarraDirRepouso, prog);
                if (cimitarraEsquerda != null) cimitarraEsquerda.localPosition = Vector3.Lerp(posCimitarraEsqRepouso - Vector3.up * 0.35f, posCimitarraEsqRepouso, prog);
            }
            yield return null;
        }

        if (modoArquearia && conjuntoArco != null) conjuntoArco.localPosition = posArcoRepouso;
        if (!modoArquearia)
        {
            if (cimitarraDireita != null) cimitarraDireita.localPosition = posCimitarraDirRepouso;
            if (cimitarraEsquerda != null) cimitarraEsquerda.localPosition = posCimitarraEsqRepouso;
        }

        animandoTroca = false;
    }

    public void DefinirEstiloVisual(bool arqueariaAtiva, bool cimitarrasAtivas)
    {
        if (conjuntoArco != null) conjuntoArco.gameObject.SetActive(arqueariaAtiva);
        if (conjuntoCimitarras != null) conjuntoCimitarras.gameObject.SetActive(cimitarrasAtivas);
    }

    // --- ANIMAÇÃO DE DISPARO DO ARCO ---
    public IEnumerator ExecutarAnimacaoTiroArco(float duracao)
    {
        animandoAtaque = true;
        if (conjuntoArco == null) { animandoAtaque = false; yield break; }

        Vector3 posPuxada = posArcoRepouso + new Vector3(-0.04f, 0.02f, -0.09f);
        Vector3 posSoltura = posArcoRepouso + new Vector3(0.015f, -0.015f, 0.04f);

        float tempoTensionar = duracao * 0.30f;
        float tempoSoltura = duracao * 0.18f;
        float tempoRetorno = duracao * 0.52f;

        float t = 0f;
        while (t < tempoTensionar)
        {
            t += Time.deltaTime;
            conjuntoArco.localPosition = Vector3.Lerp(posArcoRepouso, posPuxada, t / tempoTensionar);
            yield return null;
        }

        t = 0f;
        while (t < tempoSoltura)
        {
            t += Time.deltaTime;
            conjuntoArco.localPosition = Vector3.Lerp(posPuxada, posSoltura, t / tempoSoltura);
            yield return null;
        }

        t = 0f;
        while (t < tempoRetorno)
        {
            t += Time.deltaTime;
            conjuntoArco.localPosition = Vector3.Lerp(posSoltura, posArcoRepouso, t / tempoRetorno);
            yield return null;
        }

        conjuntoArco.localPosition = posArcoRepouso;
        animandoAtaque = false;
    }

    // --- ANIMAÇÃO DE COMBO DAS CIMITARRAS ---
    public IEnumerator ExecutarAnimacaoCimitarras(int indiceCombo, float duracao)
    {
        animandoAtaque = true;

        if (rastroCimitarraDir != null) rastroCimitarraDir.emitting = true;
        if (rastroCimitarraEsq != null) rastroCimitarraEsq.emitting = true;

        float t = 0f;
        float tempoAtaque = duracao * 0.40f;
        float tempoRecuperacao = duracao * 0.60f;

        if (indiceCombo == 1)
        {
            // Golpe 1: Corte Diagonal Rápido da Cimitarra Direita
            Vector3 alvoDir = posCimitarraDirRepouso + new Vector3(-0.30f, -0.08f, 0.22f);
            Quaternion rotAlvoDir = rotCimitarraDirRepouso * Quaternion.Euler(25f, -40f, 35f);

            while (t < tempoAtaque)
            {
                t += Time.deltaTime;
                float curva = Mathf.SmoothStep(0f, 1f, t / tempoAtaque);
                if (cimitarraDireita != null)
                {
                    cimitarraDireita.localPosition = Vector3.Lerp(posCimitarraDirRepouso, alvoDir, curva);
                    cimitarraDireita.localRotation = Quaternion.Slerp(rotCimitarraDirRepouso, rotAlvoDir, curva);
                }
                yield return null;
            }

            t = 0f;
            while (t < tempoRecuperacao)
            {
                t += Time.deltaTime;
                float curva = t / tempoRecuperacao;
                if (cimitarraDireita != null)
                {
                    cimitarraDireita.localPosition = Vector3.Lerp(alvoDir, posCimitarraDirRepouso, curva);
                    cimitarraDireita.localRotation = Quaternion.Slerp(rotAlvoDir, rotCimitarraDirRepouso, curva);
                }
                yield return null;
            }
        }
        else if (indiceCombo == 2)
        {
            // Golpe 2: Corte Ascendente da Cimitarra Esquerda
            Vector3 alvoEsq = posCimitarraEsqRepouso + new Vector3(0.30f, 0.12f, 0.22f);
            Quaternion rotAlvoEsq = rotCimitarraEsqRepouso * Quaternion.Euler(-25f, 40f, -35f);

            while (t < tempoAtaque)
            {
                t += Time.deltaTime;
                float curva = Mathf.SmoothStep(0f, 1f, t / tempoAtaque);
                if (cimitarraEsquerda != null)
                {
                    cimitarraEsquerda.localPosition = Vector3.Lerp(posCimitarraEsqRepouso, alvoEsq, curva);
                    cimitarraEsquerda.localRotation = Quaternion.Slerp(rotCimitarraEsqRepouso, rotAlvoEsq, curva);
                }
                yield return null;
            }

            t = 0f;
            while (t < tempoRecuperacao)
            {
                t += Time.deltaTime;
                float curva = t / tempoRecuperacao;
                if (cimitarraEsquerda != null)
                {
                    cimitarraEsquerda.localPosition = Vector3.Lerp(alvoEsq, posCimitarraEsqRepouso, curva);
                    cimitarraEsquerda.localRotation = Quaternion.Slerp(rotAlvoEsq, rotCimitarraEsqRepouso, curva);
                }
                yield return null;
            }
        }
        else
        {
            // Golpe 3 (Finisher): Corte Cruzado Duplo em X (Garras de Rapina)
            Vector3 alvoDir = posCimitarraDirRepouso + new Vector3(-0.20f, -0.08f, 0.30f);
            Quaternion rotAlvoDir = rotCimitarraDirRepouso * Quaternion.Euler(35f, -50f, 40f);

            Vector3 alvoEsq = posCimitarraEsqRepouso + new Vector3(0.20f, -0.08f, 0.30f);
            Quaternion rotAlvoEsq = rotCimitarraEsqRepouso * Quaternion.Euler(35f, 50f, -40f);

            while (t < tempoAtaque)
            {
                t += Time.deltaTime;
                float curva = Mathf.SmoothStep(0f, 1f, t / tempoAtaque);
                if (cimitarraDireita != null)
                {
                    cimitarraDireita.localPosition = Vector3.Lerp(posCimitarraDirRepouso, alvoDir, curva);
                    cimitarraDireita.localRotation = Quaternion.Slerp(rotCimitarraDirRepouso, rotAlvoDir, curva);
                }
                if (cimitarraEsquerda != null)
                {
                    cimitarraEsquerda.localPosition = Vector3.Lerp(posCimitarraEsqRepouso, alvoEsq, curva);
                    cimitarraEsquerda.localRotation = Quaternion.Slerp(rotCimitarraEsqRepouso, rotAlvoEsq, curva);
                }
                yield return null;
            }

            t = 0f;
            while (t < tempoRecuperacao)
            {
                t += Time.deltaTime;
                float curva = t / tempoRecuperacao;
                if (cimitarraDireita != null)
                {
                    cimitarraDireita.localPosition = Vector3.Lerp(alvoDir, posCimitarraDirRepouso, curva);
                    cimitarraDireita.localRotation = Quaternion.Slerp(rotAlvoDir, rotCimitarraDirRepouso, curva);
                }
                if (cimitarraEsquerda != null)
                {
                    cimitarraEsquerda.localPosition = Vector3.Lerp(alvoEsq, posCimitarraEsqRepouso, curva);
                    cimitarraEsquerda.localRotation = Quaternion.Slerp(rotAlvoEsq, rotCimitarraEsqRepouso, curva);
                }
                yield return null;
            }
        }

        if (rastroCimitarraDir != null) rastroCimitarraDir.emitting = false;
        if (rastroCimitarraEsq != null) rastroCimitarraEsq.emitting = false;

        if (cimitarraDireita != null)
        {
            cimitarraDireita.localPosition = posCimitarraDirRepouso;
            cimitarraDireita.localRotation = rotCimitarraDirRepouso;
        }
        if (cimitarraEsquerda != null)
        {
            cimitarraEsquerda.localPosition = posCimitarraEsqRepouso;
            cimitarraEsquerda.localRotation = rotCimitarraEsqRepouso;
        }

        animandoAtaque = false;
    }

    // --- LINHA E CABEÇA DO GANCHO ---
    public void AtualizarLinhaGancho(Vector3 pontoOrigem, Vector3 pontoDestino, bool ativa)
    {
        if (linhaGancho == null) return;

        linhaGancho.enabled = ativa;
        if (ativa)
        {
            linhaGancho.SetPosition(0, pontoOrigem);
            linhaGancho.SetPosition(1, pontoDestino);

            if (ganchoCabecaVisual != null)
            {
                ganchoCabecaVisual.gameObject.SetActive(true);
                ganchoCabecaVisual.position = pontoDestino;
                Vector3 dir = (pontoDestino - pontoOrigem);
                if (dir.sqrMagnitude > 0.01f)
                {
                    ganchoCabecaVisual.rotation = Quaternion.LookRotation(dir.normalized);
                }
            }
        }
        else
        {
            if (ganchoCabecaVisual != null)
            {
                ganchoCabecaVisual.gameObject.SetActive(false);
            }
        }
    }

    // ==================== CONSTRUÇÃO PROCEDURAL DE MODELOS LÍMPIDOS E COESOS ====================
    void ConstruirModeloProcedural()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

        // Materiais
        Material matMadeiraNegra = new Material(litShader);
        matMadeiraNegra.color = new Color(0.11f, 0.11f, 0.13f);

        Material matCouroGasto = new Material(litShader);
        matCouroGasto.color = new Color(0.32f, 0.20f, 0.12f);

        Material matPenasCorvo = new Material(litShader);
        matPenasCorvo.color = new Color(0.06f, 0.06f, 0.08f);

        Material matAcoEscurecido = new Material(litShader);
        matAcoEscurecido.color = new Color(0.28f, 0.30f, 0.33f);

        Material matCorda = new Material(litShader);
        matCorda.color = new Color(0.90f, 0.88f, 0.80f);

        // Raiz
        GameObject objRaiz = new GameObject("RangerArmas");
        objRaiz.transform.SetParent(transform, false);
        objRaiz.transform.localPosition = Vector3.zero;
        objRaiz.transform.localRotation = Quaternion.identity;
        raizArmas = objRaiz.transform;

        // ==================== 1. ARCO CURTO (LÍMPIDO & CONTÍNUO) ====================
        GameObject objArco = new GameObject("ConjuntoArco");
        objArco.transform.SetParent(raizArmas, false);
        objArco.transform.localPosition = posArcoRepouso;
        objArco.transform.localRotation = rotArcoRepouso;
        conjuntoArco = objArco.transform;

        // Braçadeira / Mão Segurando
        GameObject bracadeiraArco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bracadeiraArco.name = "BracadeiraArco";
        bracadeiraArco.transform.SetParent(conjuntoArco, false);
        bracadeiraArco.transform.localPosition = new Vector3(0f, -0.04f, -0.06f);
        bracadeiraArco.transform.localRotation = Quaternion.Euler(75f, 0f, 0f);
        bracadeiraArco.transform.localScale = new Vector3(0.07f, 0.10f, 0.07f);
        Destroy(bracadeiraArco.GetComponent<Collider>());
        bracadeiraArco.GetComponent<Renderer>().material = matCouroGasto;

        // Empunhadura Central (Couro Rústico)
        GameObject empunhadura = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        empunhadura.name = "Empunhadura";
        empunhadura.transform.SetParent(conjuntoArco, false);
        empunhadura.transform.localPosition = Vector3.zero;
        empunhadura.transform.localScale = new Vector3(0.038f, 0.08f, 0.038f);
        Destroy(empunhadura.GetComponent<Collider>());
        empunhadura.GetComponent<Renderer>().material = matCouroGasto;

        // Anéis de reforço de couro nas extremidades do punho
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject anelCouro = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            anelCouro.name = "AnelCouro_" + (side > 0 ? "Sup" : "Inf");
            anelCouro.transform.SetParent(conjuntoArco, false);
            anelCouro.transform.localPosition = new Vector3(0f, side * 0.082f, 0f);
            anelCouro.transform.localScale = new Vector3(0.044f, 0.015f, 0.044f);
            Destroy(anelCouro.GetComponent<Collider>());
            anelCouro.GetComponent<Renderer>().material = matCouroGasto;
        }

        // Haste Superior e Inferior do Arco (Geradas com malha sólida contínua recurva)
        ConstruirHasteRecurvaArco("HasteSuperior", conjuntoArco, 1, matMadeiraNegra, matPenasCorvo);
        ConstruirHasteRecurvaArco("HasteInferior", conjuntoArco, -1, matMadeiraNegra, matPenasCorvo);

        // Corda do Arco
        GameObject cordaObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cordaObj.name = "CordaArco";
        cordaObj.transform.SetParent(conjuntoArco, false);
        cordaObj.transform.localPosition = new Vector3(0f, 0f, -0.05f);
        cordaObj.transform.localScale = new Vector3(0.006f, 0.38f, 0.006f);
        Destroy(cordaObj.GetComponent<Collider>());
        cordaObj.GetComponent<Renderer>().material = matCorda;

        // Flecha Nockada Elegante
        GameObject flechaNock = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flechaNock.name = "FlechaNockada";
        flechaNock.transform.SetParent(conjuntoArco, false);
        flechaNock.transform.localPosition = new Vector3(0.015f, 0f, 0.12f);
        flechaNock.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        flechaNock.transform.localScale = new Vector3(0.012f, 0.30f, 0.012f);
        Destroy(flechaNock.GetComponent<Collider>());
        flechaNock.GetComponent<Renderer>().material = matMadeiraNegra;

        // Ponta de Aço da Flecha
        GameObject pontaFlecha = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pontaFlecha.name = "PontaAco";
        pontaFlecha.transform.SetParent(flechaNock.transform, false);
        pontaFlecha.transform.localPosition = new Vector3(0f, 0.33f, 0f);
        pontaFlecha.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        pontaFlecha.transform.localScale = new Vector3(0.028f, 0.06f, 0.028f);
        Destroy(pontaFlecha.GetComponent<Collider>());
        pontaFlecha.GetComponent<Renderer>().material = matAcoEscurecido;

        // Ponto de Saída da Flecha
        GameObject pontoDisp = new GameObject("PontoDisparoFlecha");
        pontoDisp.transform.SetParent(conjuntoArco, false);
        pontoDisp.transform.localPosition = new Vector3(0.015f, 0f, 0.45f);
        pontoDisparoFlecha = pontoDisp.transform;

        // ==================== 2. PAR DE CIMITARRAS (LÂMINAS CURVAS CONTÍNUAS) ====================
        GameObject objCimitarras = new GameObject("ConjuntoCimitarras");
        objCimitarras.transform.SetParent(raizArmas, false);
        objCimitarras.transform.localPosition = Vector3.zero;
        objCimitarras.transform.localRotation = Quaternion.identity;
        conjuntoCimitarras = objCimitarras.transform;

        // Cimitarra Direita
        cimitarraDireita = ConstruirCimitarraCoesa("CimitarraDireita", conjuntoCimitarras, posCimitarraDirRepouso, rotCimitarraDirRepouso, matAcoEscurecido, matCouroGasto, out rastroCimitarraDir);

        // Cimitarra Esquerda
        cimitarraEsquerda = ConstruirCimitarraCoesa("CimitarraEsquerda", conjuntoCimitarras, posCimitarraEsqRepouso, rotCimitarraEsqRepouso, matAcoEscurecido, matCouroGasto, out rastroCimitarraEsq);

        // ==================== 3. LINHA E CABEÇA DO GANCHO ====================
        GameObject objGanchoLinha = new GameObject("LinhaGancho");
        objGanchoLinha.transform.SetParent(raizArmas, false);
        linhaGancho = objGanchoLinha.AddComponent<LineRenderer>();
        linhaGancho.useWorldSpace = true;
        linhaGancho.positionCount = 2;
        linhaGancho.startWidth = 0.035f;
        linhaGancho.endWidth = 0.02f;
        linhaGancho.material = matCouroGasto;
        linhaGancho.enabled = false;

        // Cabeça com garras de escalada
        GameObject objCabeca = new GameObject("CabecaGancho");
        objCabeca.transform.SetParent(raizArmas, false);
        for (int i = 0; i < 3; i++)
        {
            GameObject garra = GameObject.CreatePrimitive(PrimitiveType.Cube);
            garra.name = "GarraGancho_" + i;
            garra.transform.SetParent(objCabeca.transform, false);
            garra.transform.localRotation = Quaternion.Euler(135f, i * 120f, 0f);
            garra.transform.localPosition = garra.transform.forward * 0.08f;
            garra.transform.localScale = new Vector3(0.02f, 0.09f, 0.02f);
            Destroy(garra.GetComponent<Collider>());
            garra.GetComponent<Renderer>().material = matAcoEscurecido;
        }
        objCabeca.SetActive(false);
        ganchoCabecaVisual = objCabeca.transform;
    }

    void ConstruirHasteRecurvaArco(string nome, Transform pai, int sinalY, Material matMadeira, Material matPenas)
    {
        GameObject objHaste = new GameObject(nome);
        objHaste.transform.SetParent(pai, false);

        MeshFilter mf = objHaste.AddComponent<MeshFilter>();
        MeshRenderer mr = objHaste.AddComponent<MeshRenderer>();
        mr.material = matMadeira;

        // Gera uma malha sólida e suave para o membro recurvo do arco
        int segmentos = 14;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float largura = 0.032f;
        float espessura = 0.024f;

        for (int i = 0; i <= segmentos; i++)
        {
            float t = (float)i / segmentos;
            // Curvatura elegante recurva
            float y = (0.08f + t * 0.30f) * sinalY;
            float z = Mathf.Sin(t * Mathf.PI * 0.85f) * 0.055f - (Mathf.Pow(t, 2f) * 0.035f);

            float escalaSecao = Mathf.Lerp(1.0f, 0.35f, t);
            float w = largura * escalaSecao * 0.5f;
            float e = espessura * escalaSecao * 0.5f;

            // 4 vértices por seção transversal
            vertices.Add(new Vector3(-w, y, z - e));
            vertices.Add(new Vector3(w, y, z - e));
            vertices.Add(new Vector3(w, y, z + e));
            vertices.Add(new Vector3(-w, y, z + e));

            if (i > 0)
            {
                int baseIndex = (i - 1) * 4;
                int nextIndex = i * 4;

                // 4 faces laterais
                for (int f = 0; f < 4; f++)
                {
                    int fNext = (f + 1) % 4;
                    if (sinalY > 0)
                    {
                        triangles.Add(baseIndex + f);
                        triangles.Add(nextIndex + f);
                        triangles.Add(nextIndex + fNext);

                        triangles.Add(baseIndex + f);
                        triangles.Add(nextIndex + fNext);
                        triangles.Add(baseIndex + fNext);
                    }
                    else
                    {
                        triangles.Add(baseIndex + f);
                        triangles.Add(nextIndex + fNext);
                        triangles.Add(nextIndex + f);

                        triangles.Add(baseIndex + f);
                        triangles.Add(baseIndex + fNext);
                        triangles.Add(nextIndex + fNext);
                    }
                }
            }
        }

        Mesh malha = new Mesh();
        malha.name = nome + "Mesh";
        malha.vertices = vertices.ToArray();
        malha.triangles = triangles.ToArray();
        malha.RecalculateNormals();
        malha.RecalculateBounds();
        mf.sharedMesh = malha;

        // Penas de corvo discretas na base da haste
        GameObject pena = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pena.name = "PenaCorvo_" + (sinalY > 0 ? "Sup" : "Inf");
        pena.transform.SetParent(objHaste.transform, false);
        pena.transform.localPosition = new Vector3(0.018f, sinalY * 0.12f, -0.015f);
        pena.transform.localRotation = Quaternion.Euler(0f, 15f, sinalY * -35f);
        pena.transform.localScale = new Vector3(0.008f, 0.07f, 0.03f);
        Destroy(pena.GetComponent<Collider>());
        pena.GetComponent<Renderer>().material = matPenas;
    }

    Transform ConstruirCimitarraCoesa(string nome, Transform pai, Vector3 pos, Quaternion rot, Material matAco, Material matCouro, out TrailRenderer rastro)
    {
        GameObject raizCimitarra = new GameObject(nome);
        raizCimitarra.transform.SetParent(pai, false);
        raizCimitarra.transform.localPosition = pos;
        raizCimitarra.transform.localRotation = rot;

        // 1. Braçadeira da Mão
        GameObject bracadeira = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bracadeira.name = "Bracadeira";
        bracadeira.transform.SetParent(raizCimitarra.transform, false);
        bracadeira.transform.localPosition = new Vector3(0f, -0.05f, -0.07f);
        bracadeira.transform.localRotation = Quaternion.Euler(75f, 0f, 0f);
        bracadeira.transform.localScale = new Vector3(0.07f, 0.10f, 0.07f);
        Destroy(bracadeira.GetComponent<Collider>());
        bracadeira.GetComponent<Renderer>().material = matCouro;

        // 2. Empunhadura de Couro
        GameObject cabo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cabo.name = "CaboCouro";
        cabo.transform.SetParent(raizCimitarra.transform, false);
        cabo.transform.localPosition = Vector3.zero;
        cabo.transform.localScale = new Vector3(0.032f, 0.08f, 0.032f);
        Destroy(cabo.GetComponent<Collider>());
        cabo.GetComponent<Renderer>().material = matCouro;

        // 3. Pomo em Aço
        GameObject pomo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pomo.name = "Pomo";
        pomo.transform.SetParent(raizCimitarra.transform, false);
        pomo.transform.localPosition = new Vector3(0f, -0.09f, 0f);
        pomo.transform.localScale = new Vector3(0.045f, 0.045f, 0.045f);
        Destroy(pomo.GetComponent<Collider>());
        pomo.GetComponent<Renderer>().material = matAco;

        // 4. Guarda-Mão Curvo
        GameObject guarda = GameObject.CreatePrimitive(PrimitiveType.Cube);
        guarda.name = "GuardaMao";
        guarda.transform.SetParent(raizCimitarra.transform, false);
        guarda.transform.localPosition = new Vector3(0f, 0.08f, 0.015f);
        guarda.transform.localScale = new Vector3(0.08f, 0.02f, 0.045f);
        Destroy(guarda.GetComponent<Collider>());
        guarda.GetComponent<Renderer>().material = matAco;

        // 5. LÂMINA CURVA EM GARRA DE RAPINA (Malha Sólida Contínua)
        GameObject objLamina = new GameObject("LaminaGarraRapina");
        objLamina.transform.SetParent(raizCimitarra.transform, false);
        objLamina.transform.localPosition = new Vector3(0f, 0.09f, 0f);

        MeshFilter mf = objLamina.AddComponent<MeshFilter>();
        MeshRenderer mr = objLamina.AddComponent<MeshRenderer>();
        mr.material = matAco;

        int segmentos = 16;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float compTotal = 0.55f;
        float larguraBase = 0.055f;
        float espessuraBase = 0.014f;

        Vector3 pontoPonta = Vector3.zero;

        for (int i = 0; i <= segmentos; i++)
        {
            float t = (float)i / segmentos;
            float y = t * compTotal;
            // Curva parabólica acentuada para a frente (garra de ave de rapina)
            float z = Mathf.Pow(t, 1.6f) * 0.16f;

            float fatorLargura = (1.0f - Mathf.Pow(t, 1.8f));
            float w = Mathf.Max(0.002f, larguraBase * fatorLargura);
            float esp = Mathf.Max(0.002f, espessuraBase * (1.0f - t * 0.85f));

            // Perfil triangular da lâmina (Dorso espesso, Fio cortante fino)
            // v0: Dorso Esq, v1: Dorso Dir, v2: Fio Cortante (Frontal)
            vertices.Add(new Vector3(-esp * 0.5f, y, z - w * 0.3f));
            vertices.Add(new Vector3(esp * 0.5f, y, z - w * 0.3f));
            vertices.Add(new Vector3(0f, y, z + w * 0.7f));

            pontoPonta = new Vector3(0f, y, z + w * 0.7f);

            if (i > 0)
            {
                int b = (i - 1) * 3;
                int n = i * 3;

                // Face Esquerda da Lâmina
                triangles.Add(b + 0);
                triangles.Add(n + 0);
                triangles.Add(n + 2);

                triangles.Add(b + 0);
                triangles.Add(n + 2);
                triangles.Add(b + 2);

                // Face Direita da Lâmina
                triangles.Add(b + 1);
                triangles.Add(n + 2);
                triangles.Add(n + 1);

                triangles.Add(b + 1);
                triangles.Add(b + 2);
                triangles.Add(n + 2);

                // Dorso da Lâmina (Costa)
                triangles.Add(b + 0);
                triangles.Add(n + 1);
                triangles.Add(n + 0);

                triangles.Add(b + 0);
                triangles.Add(b + 1);
                triangles.Add(n + 1);
            }
        }

        Mesh malhaLamina = new Mesh();
        malhaLamina.name = "MalhaLaminaRapina";
        malhaLamina.vertices = vertices.ToArray();
        malhaLamina.triangles = triangles.ToArray();
        malhaLamina.RecalculateNormals();
        malhaLamina.RecalculateBounds();
        mf.sharedMesh = malhaLamina;

        // 6. Rastro de Corte na ponta
        GameObject objTrail = new GameObject("RastroLamina");
        objTrail.transform.SetParent(objLamina.transform, false);
        objTrail.transform.localPosition = pontoPonta;

        rastro = objTrail.AddComponent<TrailRenderer>();
        rastro.time = 0.18f;
        rastro.startWidth = 0.22f;
        rastro.endWidth = 0.02f;
        rastro.emitting = false;

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
        Material matRastro = new Material(litShader);
        matRastro.color = new Color(0.85f, 0.90f, 0.95f, 0.5f);
        rastro.material = matRastro;

        return raizCimitarra.transform;
    }
}
