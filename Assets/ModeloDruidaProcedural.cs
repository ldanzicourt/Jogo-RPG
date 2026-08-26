using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeloDruidaProcedural : MonoBehaviour
{
    [Header("Estruturas")]
    public Transform raizArmas;
    public Transform maoDireita;
    public Transform maoEsquerda;
    public Transform ramoAncestral;
    public Transform escudoCasca;
    public LineRenderer linhaChicote;
    public TrailRenderer rastroChicote;

    [Header("Posicionamento em 1ª Pessoa")]
    public Vector3 posRepousoDireita = new Vector3(0.32f, -0.32f, 0.55f);
    public Quaternion rotRepousoDireita = Quaternion.Euler(15f, -10f, 10f);

    public Vector3 posRepousoEsquerda = new Vector3(-0.32f, -0.34f, 0.52f);
    public Quaternion rotRepousoEsquerda = Quaternion.Euler(15f, 10f, -10f);

    public Vector3 posEscudo = new Vector3(0f, -0.15f, 0.45f);
    public Quaternion rotEscudo = Quaternion.Euler(0f, 0f, 0f);

    private bool animandoGolpe = false;
    private bool emBloqueio = false;
    private Transform[] sementesOrbitando;

    void Awake()
    {
        transform.localScale = Vector3.one;
        ConstruirModeloProcedural();
    }

    void Start()
    {
        if (escudoCasca != null) escudoCasca.gameObject.SetActive(false);
    }

    void Update()
    {
        // Animação de respiração e órbita das sementes na mão esquerda
        if (sementesOrbitando != null && maoEsquerda != null)
        {
            for (int i = 0; i < sementesOrbitando.Length; i++)
            {
                if (sementesOrbitando[i] != null)
                {
                    float angulo = (Time.time * 90f + (i * (360f / sementesOrbitando.Length))) * Mathf.Deg2Rad;
                    sementesOrbitando[i].localPosition = new Vector3(Mathf.Cos(angulo) * 0.12f, 0.15f + Mathf.Sin(Time.time * 2f + i) * 0.03f, Mathf.Sin(angulo) * 0.12f);
                }
            }
        }

        // Sway ocioso suave quando não estiver atacando ou bloqueando
        if (!animandoGolpe && !emBloqueio && maoDireita != null)
        {
            float bob = Mathf.Sin(Time.time * 2f) * 0.004f;
            maoDireita.localPosition = posRepousoDireita + new Vector3(0f, bob, 0f);
            maoEsquerda.localPosition = posRepousoEsquerda + new Vector3(0f, -bob, 0f);
        }
    }

    public void DefinirVisibilidadeArmas(bool visivel)
    {
        if (raizArmas != null) raizArmas.gameObject.SetActive(visivel);
    }

    public void DefinirBloqueio(bool ativo)
    {
        emBloqueio = ativo;
        if (escudoCasca != null)
        {
            escudoCasca.gameObject.SetActive(ativo);
            if (ativo)
            {
                escudoCasca.localPosition = posEscudo;
                escudoCasca.localRotation = rotEscudo;
            }
        }

        if (maoDireita != null) maoDireita.gameObject.SetActive(!ativo);
        if (maoEsquerda != null) maoEsquerda.gameObject.SetActive(!ativo);
    }

    // --- ANIMAÇÃO DO CHICOTE DE ESPINHOS ---
    public IEnumerator ExecutarAnimacaoChicote(int indiceCombo, float duracao, Vector3 direcaoAlvo)
    {
        animandoGolpe = true;
        if (rastroChicote != null) rastroChicote.emitting = true;

        Vector3 posInicial = maoDireita.localPosition;
        Quaternion rotInicial = maoDireita.localRotation;

        Vector3 posRecuo;
        Quaternion rotRecuo;
        Vector3 posEstalada;
        Quaternion rotEstalada;

        if (indiceCombo == 1)
        {
            // Golpe 1: Chicoteada da Direita para a Esquerda
            posRecuo = posInicial + new Vector3(0.12f, 0.08f, -0.15f);
            rotRecuo = rotInicial * Quaternion.Euler(-20f, 25f, -10f);

            posEstalada = posInicial + new Vector3(-0.40f, -0.05f, 0.25f);
            rotEstalada = rotInicial * Quaternion.Euler(30f, -50f, 40f);
        }
        else if (indiceCombo == 2)
        {
            // Golpe 2: Chicoteada Diagonal Descendente (Cima para Baixo)
            posRecuo = posInicial + new Vector3(0.05f, 0.18f, -0.10f);
            rotRecuo = rotInicial * Quaternion.Euler(-35f, 10f, 0f);

            posEstalada = posInicial + new Vector3(-0.15f, -0.25f, 0.28f);
            rotEstalada = rotInicial * Quaternion.Euler(45f, -20f, 10f);
        }
        else
        {
            // Golpe 3 (Tração): Estocada Reta Frontal com Tração
            posRecuo = posInicial + new Vector3(0f, 0.05f, -0.22f);
            rotRecuo = rotInicial * Quaternion.Euler(-15f, 0f, 0f);

            posEstalada = posInicial + new Vector3(0f, 0f, 0.40f);
            rotEstalada = rotInicial * Quaternion.Euler(20f, 0f, 0f);
        }

        float tempoWindup = duracao * 0.25f;
        float tempoEstalada = duracao * 0.45f;
        float tempoRecuperacao = duracao * 0.30f;

        // 1. Windup (Puxa para trás)
        float t = 0f;
        while (t < tempoWindup)
        {
            t += Time.deltaTime;
            maoDireita.localPosition = Vector3.Lerp(posInicial, posRecuo, t / tempoWindup);
            maoDireita.localRotation = Quaternion.Slerp(rotInicial, rotRecuo, t / tempoWindup);
            yield return null;
        }

        // 2. Estalada do Chicote (Lança para a frente)
        t = 0f;
        while (t < tempoEstalada)
        {
            t += Time.deltaTime;
            float curva = Mathf.SmoothStep(0f, 1f, t / tempoEstalada);
            maoDireita.localPosition = Vector3.Lerp(posRecuo, posEstalada, curva);
            maoDireita.localRotation = Quaternion.Slerp(rotRecuo, rotEstalada, curva);
            yield return null;
        }

        if (rastroChicote != null) rastroChicote.emitting = false;

        // 3. Retorno suave à postura de repouso
        t = 0f;
        while (t < tempoRecuperacao)
        {
            t += Time.deltaTime;
            maoDireita.localPosition = Vector3.Lerp(posEstalada, posRepousoDireita, t / tempoRecuperacao);
            maoDireita.localRotation = Quaternion.Slerp(rotEstalada, rotRepousoDireita, t / tempoRecuperacao);
            yield return null;
        }

        animandoGolpe = false;
    }

    public void GerarEsporosParry()
    {
        StartCoroutine(RotinaEsporosParry());
    }

    private IEnumerator RotinaEsporosParry()
    {
        if (escudoCasca == null) yield break;

        // Cria nuvem de esporos curativos verdes
        for (int i = 0; i < 6; i++)
        {
            GameObject esporo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            esporo.name = "EsporoCura";
            esporo.transform.position = escudoCasca.position + (Random.insideUnitSphere * 0.5f) + transform.forward * 0.3f;
            esporo.transform.localScale = Vector3.one * 0.18f;
            Destroy(esporo.GetComponent<Collider>());

            Renderer r = esporo.GetComponent<Renderer>();
            if (r != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
                mat.color = new Color(0.2f, 1f, 0.4f, 0.9f);
                r.material = mat;
            }

            Destroy(esporo, 0.45f);
        }

        yield return null;
    }

    void ConstruirModeloProcedural()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

        GameObject objRaiz = new GameObject("DruidaArmas");
        objRaiz.transform.SetParent(transform, false);
        objRaiz.transform.localPosition = Vector3.zero;
        objRaiz.transform.localRotation = Quaternion.identity;
        objRaiz.transform.localScale = Vector3.one;
        raizArmas = objRaiz.transform;

        Material matPele = new Material(litShader);
        matPele.color = new Color(0.85f, 0.68f, 0.52f);

        Material matMadeira = new Material(litShader);
        matMadeira.color = new Color(0.32f, 0.20f, 0.12f);

        Material matVinha = new Material(litShader);
        matVinha.color = new Color(0.18f, 0.65f, 0.22f);

        Material matEspinho = new Material(litShader);
        matEspinho.color = new Color(0.85f, 0.20f, 0.10f);

        Material matGema = new Material(litShader);
        matGema.color = new Color(0.2f, 0.95f, 0.4f);
        if (matGema.HasProperty("_EmissionColor"))
        {
            matGema.EnableKeyword("_EMISSION");
            matGema.SetColor("_EmissionColor", new Color(0.2f, 0.95f, 0.4f) * 2f);
        }

        // ==================== 1. MÃO DIREITA & RAMO ANCESTRAL ====================
        GameObject objMaoDir = new GameObject("MaoDireita");
        objMaoDir.transform.SetParent(raizArmas, false);
        objMaoDir.transform.localPosition = posRepousoDireita;
        objMaoDir.transform.localRotation = rotRepousoDireita;
        maoDireita = objMaoDir.transform;

        // Palma da Mão
        GameObject palmaDir = GameObject.CreatePrimitive(PrimitiveType.Cube);
        palmaDir.name = "PalmaDir";
        palmaDir.transform.SetParent(maoDireita, false);
        palmaDir.transform.localPosition = new Vector3(0f, 0f, 0f);
        palmaDir.transform.localScale = new Vector3(0.08f, 0.04f, 0.12f);
        Destroy(palmaDir.GetComponent<Collider>());
        palmaDir.GetComponent<Renderer>().material = matPele;

        // Dedos segurando o ramo
        for (int d = 0; d < 4; d++)
        {
            GameObject dedo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dedo.name = "DedoDir_" + d;
            dedo.transform.SetParent(maoDireita, false);
            dedo.transform.localPosition = new Vector3((d * 0.02f) - 0.03f, 0.02f, 0.05f);
            dedo.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            dedo.transform.localScale = new Vector3(0.018f, 0.035f, 0.018f);
            Destroy(dedo.GetComponent<Collider>());
            dedo.GetComponent<Renderer>().material = matPele;
        }

        // Ramo Ancestral de Carvalho Vivo
        GameObject ramo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ramo.name = "RamoAncestral";
        ramo.transform.SetParent(maoDireita, false);
        ramo.transform.localPosition = new Vector3(0f, 0.04f, 0.08f);
        ramo.transform.localRotation = Quaternion.Euler(75f, 0f, 0f);
        ramo.transform.localScale = new Vector3(0.035f, 0.22f, 0.035f);
        Destroy(ramo.GetComponent<Collider>());
        ramo.GetComponent<Renderer>().material = matMadeira;
        ramoAncestral = ramo.transform;

        // Gema Esmeralda no topo do ramo
        GameObject gema = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gema.name = "GemaEsmeralda";
        gema.transform.SetParent(ramoAncestral, false);
        gema.transform.localPosition = new Vector3(0f, 0.24f, 0f);
        gema.transform.localScale = new Vector3(0.06f, 0.08f, 0.06f);
        Destroy(gema.GetComponent<Collider>());
        gema.GetComponent<Renderer>().material = matGema;

        // Chicote de Vinhas projetando a partir do ramo para a FRENTE (+Z)
        GameObject chicoteObj = new GameObject("ChicoteVinhas");
        chicoteObj.transform.SetParent(maoDireita, false);
        chicoteObj.transform.localPosition = new Vector3(0f, 0.06f, 0.25f);

        // Segmentos visuais do chicote
        for (int s = 0; s < 6; s++)
        {
            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            seg.name = "SegmentoVinha_" + s;
            seg.transform.SetParent(chicoteObj.transform, false);
            seg.transform.localPosition = new Vector3(0f, 0f, s * 0.14f);
            seg.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            seg.transform.localScale = new Vector3(0.025f - (s * 0.002f), 0.07f, 0.025f - (s * 0.002f));
            Destroy(seg.GetComponent<Collider>());
            seg.GetComponent<Renderer>().material = matVinha;

            // Espinhos ao longo dos segmentos
            if (s % 2 == 0)
            {
                GameObject espinho = GameObject.CreatePrimitive(PrimitiveType.Cube);
                espinho.name = "Espinho_" + s;
                espinho.transform.SetParent(seg.transform, false);
                espinho.transform.localPosition = new Vector3(0.04f, 0f, 0f);
                espinho.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                espinho.transform.localScale = new Vector3(0.02f, 0.04f, 0.02f);
                Destroy(espinho.GetComponent<Collider>());
                espinho.GetComponent<Renderer>().material = matEspinho;
            }
        }

        // Rastro de Corte na ponta do Chicote
        GameObject objTrail = new GameObject("RastroChicote");
        objTrail.transform.SetParent(chicoteObj.transform, false);
        objTrail.transform.localPosition = new Vector3(0f, 0f, 0.85f);
        rastroChicote = objTrail.AddComponent<TrailRenderer>();
        rastroChicote.time = 0.18f;
        rastroChicote.startWidth = 0.25f;
        rastroChicote.endWidth = 0.02f;
        rastroChicote.emitting = false;

        Material matTrail = new Material(litShader);
        matTrail.color = new Color(0.2f, 0.95f, 0.35f, 0.7f);
        rastroChicote.material = matTrail;

        // ==================== 2. MÃO ESQUERDA & SEMENTES ORBITANTES ====================
        GameObject objMaoEsq = new GameObject("MaoEsquerda");
        objMaoEsq.transform.SetParent(raizArmas, false);
        objMaoEsq.transform.localPosition = posRepousoEsquerda;
        objMaoEsq.transform.localRotation = rotRepousoEsquerda;
        maoEsquerda = objMaoEsq.transform;

        GameObject palmaEsq = GameObject.CreatePrimitive(PrimitiveType.Cube);
        palmaEsq.name = "PalmaEsq";
        palmaEsq.transform.SetParent(maoEsquerda, false);
        palmaEsq.transform.localScale = new Vector3(0.08f, 0.04f, 0.12f);
        Destroy(palmaEsq.GetComponent<Collider>());
        palmaEsq.GetComponent<Renderer>().material = matPele;

        // 3 Sementes Mágicas Orbitando a mão esquerda
        sementesOrbitando = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject sem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sem.name = "SementeOrbital_" + i;
            sem.transform.SetParent(maoEsquerda, false);
            sem.transform.localScale = new Vector3(0.035f, 0.05f, 0.035f);
            Destroy(sem.GetComponent<Collider>());
            sem.GetComponent<Renderer>().material = matGema;
            sementesOrbitando[i] = sem.transform;
        }

        // ==================== 3. ESCUDO PELE DE ÁRVORE (RMB) ====================
        GameObject objEscudo = new GameObject("EscudoPeleDeArvore");
        objEscudo.transform.SetParent(raizArmas, false);
        objEscudo.transform.localPosition = posEscudo;
        objEscudo.transform.localRotation = rotEscudo;
        escudoCasca = objEscudo.transform;

        // Placa de Casca Central
        GameObject casca = GameObject.CreatePrimitive(PrimitiveType.Cube);
        casca.name = "CascaCentral";
        casca.transform.SetParent(escudoCasca, false);
        casca.transform.localScale = new Vector3(0.55f, 0.45f, 0.06f);
        Destroy(casca.GetComponent<Collider>());
        casca.GetComponent<Renderer>().material = matMadeira;

        // Raízes Entrelaçadas
        for (int r = 0; r < 3; r++)
        {
            GameObject raiz = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            raiz.name = "RaizEscudo_" + r;
            raiz.transform.SetParent(escudoCasca, false);
            raiz.transform.localPosition = new Vector3((r * 0.16f) - 0.16f, 0f, -0.02f);
            raiz.transform.localRotation = Quaternion.Euler(0f, 0f, (r - 1) * 20f);
            raiz.transform.localScale = new Vector3(0.045f, 0.25f, 0.045f);
            Destroy(raiz.GetComponent<Collider>());
            raiz.GetComponent<Renderer>().material = matVinha;
        }
    }
}
