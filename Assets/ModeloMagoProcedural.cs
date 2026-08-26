using System.Collections;
using UnityEngine;

public class ModeloMagoProcedural : MonoBehaviour
{
    [Header("Objetos Raiz das Mãos")]
    public Transform raizCajado;
    public Transform raizGrimorio;
    public Transform pontoDisparoCajado;

    [Header("Elementos Visuais")]
    public Renderer renderizadorGemaCajado;
    public Light luzCajado;
    public GameObject barreiraVisual;
    public LineRenderer feixeLaserSuprema;
    public LineRenderer feixeSifaoNecro;

    private Vector3 posOriginalCajado;
    private Quaternion rotOriginalCajado;
    private Vector3 posOriginalGrimorio;
    private Quaternion rotOriginalGrimorio;

    private bool animandoRecuo = false;
    private bool animandoPagina = false;

    void Awake()
    {
        ConstruirModelosSeNecessario();
    }

    void Start()
    {
        if (raizCajado != null)
        {
            posOriginalCajado = raizCajado.localPosition;
            rotOriginalCajado = raizCajado.localRotation;
        }

        if (raizGrimorio != null)
        {
            posOriginalGrimorio = raizGrimorio.localPosition;
            rotOriginalGrimorio = raizGrimorio.localRotation;
        }

        AtualizarCorEscola(EscolaMagica.Evocacao);
    }

    void Update()
    {
        // Animação de flutuação / respiração mágica suave
        if (!animandoRecuo && raizCajado != null)
        {
            float bobbingY = Mathf.Sin(Time.time * 2f) * 0.015f;
            float bobbingX = Mathf.Cos(Time.time * 1.5f) * 0.01f;
            raizCajado.localPosition = posOriginalCajado + new Vector3(bobbingX, bobbingY, 0f);
        }

        if (!animandoPagina && raizGrimorio != null && (barreiraVisual == null || !barreiraVisual.activeSelf))
        {
            float bobbingY = Mathf.Cos(Time.time * 2.2f) * 0.012f;
            raizGrimorio.localPosition = posOriginalGrimorio + new Vector3(0f, bobbingY, 0f);
        }
    }

    public void AtualizarCorEscola(EscolaMagica escola)
    {
        Color cor = ObterCorDaEscola(escola);

        if (renderizadorGemaCajado != null && renderizadorGemaCajado.material != null)
        {
            renderizadorGemaCajado.material.color = cor;
            if (renderizadorGemaCajado.material.HasProperty("_EmissionColor"))
            {
                renderizadorGemaCajado.material.EnableKeyword("_EMISSION");
                renderizadorGemaCajado.material.SetColor("_EmissionColor", cor * 1.8f);
            }
        }

        if (luzCajado != null)
        {
            luzCajado.color = cor;
        }
    }

    public Color ObterCorDaEscola(EscolaMagica escola)
    {
        switch (escola)
        {
            case EscolaMagica.Evocacao:
                return new Color(0.1f, 0.65f, 1f); // Azul Arcano
            case EscolaMagica.Necromancia:
                return new Color(0.75f, 0.15f, 1f); // Roxo Profundo
            case EscolaMagica.Conjuracao:
                return new Color(0.15f, 1f, 0.75f); // Ciano / Esmeralda
            default:
                return Color.white;
        }
    }

    public void AnimarRecuoDisparo()
    {
        if (!animandoRecuo && raizCajado != null)
        {
            StartCoroutine(RotinaRecuo());
        }
    }

    private IEnumerator RotinaRecuo()
    {
        animandoRecuo = true;
        Vector3 recuo = posOriginalCajado - Vector3.forward * 0.12f + Vector3.up * 0.04f;
        Quaternion rotRecuo = rotOriginalCajado * Quaternion.Euler(-15f, 0f, 0f);

        float t = 0f;
        while (t < 0.08f)
        {
            t += Time.deltaTime;
            raizCajado.localPosition = Vector3.Lerp(posOriginalCajado, recuo, t / 0.08f);
            raizCajado.localRotation = Quaternion.Slerp(rotOriginalCajado, rotRecuo, t / 0.08f);
            yield return null;
        }

        t = 0f;
        while (t < 0.16f)
        {
            t += Time.deltaTime;
            raizCajado.localPosition = Vector3.Lerp(recuo, posOriginalCajado, t / 0.16f);
            raizCajado.localRotation = Quaternion.Slerp(rotRecuo, rotOriginalCajado, t / 0.16f);
            yield return null;
        }

        raizCajado.localPosition = posOriginalCajado;
        raizCajado.localRotation = rotOriginalCajado;
        animandoRecuo = false;
    }

    public void AnimarFolhearPaginas()
    {
        if (!animandoPagina && raizGrimorio != null)
        {
            StartCoroutine(RotinaFolhear());
        }
    }

    private IEnumerator RotinaFolhear()
    {
        animandoPagina = true;
        Quaternion rotInclinada = rotOriginalGrimorio * Quaternion.Euler(20f, -25f, 15f);
        Vector3 posLevantada = posOriginalGrimorio + new Vector3(0.05f, 0.08f, 0.05f);

        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            raizGrimorio.localPosition = Vector3.Lerp(posOriginalGrimorio, posLevantada, t / 0.12f);
            raizGrimorio.localRotation = Quaternion.Slerp(rotOriginalGrimorio, rotInclinada, t / 0.12f);
            yield return null;
        }

        t = 0f;
        while (t < 0.18f)
        {
            t += Time.deltaTime;
            raizGrimorio.localPosition = Vector3.Lerp(posLevantada, posOriginalGrimorio, t / 0.18f);
            raizGrimorio.localRotation = Quaternion.Slerp(rotInclinada, rotOriginalGrimorio, t / 0.18f);
            yield return null;
        }

        raizGrimorio.localPosition = posOriginalGrimorio;
        raizGrimorio.localRotation = rotOriginalGrimorio;
        animandoPagina = false;
    }

    public void DefinirBarreiraAtiva(bool ativa)
    {
        if (barreiraVisual != null)
        {
            barreiraVisual.SetActive(ativa);
        }

        if (raizGrimorio != null)
        {
            if (ativa)
            {
                // Ergue o grimório para a frente do peito
                raizGrimorio.localPosition = new Vector3(-0.1f, -0.05f, 0.45f);
                raizGrimorio.localRotation = Quaternion.Euler(15f, 25f, -10f);
            }
            else
            {
                raizGrimorio.localPosition = posOriginalGrimorio;
                raizGrimorio.localRotation = rotOriginalGrimorio;
            }
        }
    }

    public void PulsarBarreiraParry()
    {
        if (barreiraVisual != null)
        {
            StartCoroutine(RotinaPulsoBarreira());
        }
    }

    private IEnumerator RotinaPulsoBarreira()
    {
        Vector3 escOriginal = barreiraVisual.transform.localScale;
        barreiraVisual.transform.localScale = escOriginal * 1.5f;

        Renderer r = barreiraVisual.GetComponent<Renderer>();
        Color corOriginal = r != null ? r.material.color : Color.cyan;
        if (r != null) r.material.color = Color.white;

        yield return new WaitForSeconds(0.12f);

        if (barreiraVisual != null) barreiraVisual.transform.localScale = escOriginal;
        if (r != null) r.material.color = corOriginal;
    }

    void ConstruirModelosSeNecessario()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

        // 1. CAJADO NA MÃO DIREITA
        if (raizCajado == null)
        {
            GameObject objCajado = new GameObject("CajadoMago");
            objCajado.transform.SetParent(transform);
            objCajado.transform.localPosition = new Vector3(0.38f, -0.35f, 0.65f);
            objCajado.transform.localRotation = Quaternion.Euler(10f, -5f, 0f);
            raizCajado = objCajado.transform;

            // Haste de Madeira/Metal Escuro
            GameObject haste = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            haste.name = "Haste";
            haste.transform.SetParent(raizCajado);
            haste.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            haste.transform.localScale = new Vector3(0.035f, 0.75f, 0.035f);
            haste.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            Destroy(haste.GetComponent<Collider>());

            Renderer rendHaste = haste.GetComponent<Renderer>();
            Material matHaste = new Material(litShader);
            matHaste.color = new Color(0.2f, 0.15f, 0.12f); // Madeira nobre
            rendHaste.material = matHaste;

            // Pontal / Coroa Dourada do Cajado
            GameObject coroa = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coroa.name = "Coroa";
            coroa.transform.SetParent(raizCajado);
            coroa.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            coroa.transform.localScale = new Vector3(0.065f, 0.06f, 0.065f);
            Destroy(coroa.GetComponent<Collider>());

            Renderer rendCoroa = coroa.GetComponent<Renderer>();
            Material matCoroa = new Material(litShader);
            matCoroa.color = new Color(0.9f, 0.75f, 0.2f); // Ouro arcano
            rendCoroa.material = matCoroa;

            // Gema Mágica Flutuante
            GameObject gema = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gema.name = "GemaArcana";
            gema.transform.SetParent(raizCajado);
            gema.transform.localPosition = new Vector3(0f, 0.68f, 0f);
            gema.transform.localScale = new Vector3(0.11f, 0.15f, 0.11f);
            Destroy(gema.GetComponent<Collider>());

            renderizadorGemaCajado = gema.GetComponent<Renderer>();
            Material matGema = new Material(litShader);
            matGema.color = new Color(0.1f, 0.65f, 1f);
            renderizadorGemaCajado.material = matGema;

            // Luz da Gema
            GameObject objLuz = new GameObject("LuzGema");
            objLuz.transform.SetParent(raizCajado);
            objLuz.transform.localPosition = new Vector3(0f, 0.68f, 0f);
            luzCajado = objLuz.AddComponent<Light>();
            luzCajado.color = new Color(0.1f, 0.65f, 1f);
            luzCajado.range = 3f;
            luzCajado.intensity = 2.5f;

            // Ponto de Disparo
            GameObject pontoDisp = new GameObject("PontoDisparoCajado");
            pontoDisp.transform.SetParent(raizCajado);
            pontoDisp.transform.localPosition = new Vector3(0f, 0.75f, 0.1f);
            pontoDisparoCajado = pontoDisp.transform;
        }

        // 2. GRIMÓRIO NA MÃO ESQUERDA
        if (raizGrimorio == null)
        {
            GameObject objGrimorio = new GameObject("GrimorioMago");
            objGrimorio.transform.SetParent(transform);
            objGrimorio.transform.localPosition = new Vector3(-0.38f, -0.32f, 0.55f);
            objGrimorio.transform.localRotation = Quaternion.Euler(30f, 25f, -15f);
            raizGrimorio = objGrimorio.transform;

            // Capa do Livro (Paralelepípedo Achatado)
            GameObject capa = GameObject.CreatePrimitive(PrimitiveType.Cube);
            capa.name = "CapaLivro";
            capa.transform.SetParent(raizGrimorio);
            capa.transform.localPosition = Vector3.zero;
            capa.transform.localScale = new Vector3(0.24f, 0.05f, 0.32f);
            Destroy(capa.GetComponent<Collider>());

            Renderer rendCapa = capa.GetComponent<Renderer>();
            Material matCapa = new Material(litShader);
            matCapa.color = new Color(0.12f, 0.1f, 0.28f); // Couro Mágico Índigo
            rendCapa.material = matCapa;

            // Páginas Internas
            GameObject paginas = GameObject.CreatePrimitive(PrimitiveType.Cube);
            paginas.name = "Paginas";
            paginas.transform.SetParent(raizGrimorio);
            paginas.transform.localPosition = new Vector3(0f, 0.005f, 0f);
            paginas.transform.localScale = new Vector3(0.22f, 0.045f, 0.3f);
            Destroy(paginas.GetComponent<Collider>());

            Renderer rendPaginas = paginas.GetComponent<Renderer>();
            Material matPaginas = new Material(litShader);
            matPaginas.color = new Color(0.92f, 0.88f, 0.78f); // Pergaminho antigo
            rendPaginas.material = matPaginas;

            // Selo / Runa Dourada na Capa
            GameObject runa = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            runa.name = "SeloArcano";
            runa.transform.SetParent(raizGrimorio);
            runa.transform.localPosition = new Vector3(0f, 0.028f, 0f);
            runa.transform.localScale = new Vector3(0.09f, 0.005f, 0.09f);
            Destroy(runa.GetComponent<Collider>());

            Renderer rendRuna = runa.GetComponent<Renderer>();
            Material matRuna = new Material(litShader);
            matRuna.color = new Color(0.95f, 0.8f, 0.25f);
            rendRuna.material = matRuna;
        }

        // 3. BARREIRA GEOMÉTRICA DE ENERGIA (Escudo do Grimório)
        if (barreiraVisual == null)
        {
            barreiraVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barreiraVisual.name = "BarreiraGeometrica";
            barreiraVisual.transform.SetParent(transform);
            barreiraVisual.transform.localPosition = new Vector3(0f, 0f, 0.9f);
            barreiraVisual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            barreiraVisual.transform.localScale = new Vector3(1.4f, 0.02f, 1.4f);
            Destroy(barreiraVisual.GetComponent<Collider>());

            Renderer rendBarreira = barreiraVisual.GetComponent<Renderer>();
            Material matBarreira = new Material(litShader);
            matBarreira.color = new Color(0.2f, 0.8f, 1f, 0.6f); // Hexágono/Escudo Ciano Translúcido
            rendBarreira.material = matBarreira;

            barreiraVisual.SetActive(false);
        }

        // 4. LINE RENDERERS PARA FEIXES (Sifão de Alma e Suprema Desintegração)
        if (feixeSifaoNecro == null)
        {
            GameObject objSifao = new GameObject("FeixeSifaoNecro");
            objSifao.transform.SetParent(transform);
            feixeSifaoNecro = objSifao.AddComponent<LineRenderer>();
            feixeSifaoNecro.startWidth = 0.08f;
            feixeSifaoNecro.endWidth = 0.04f;
            feixeSifaoNecro.positionCount = 2;
            feixeSifaoNecro.useWorldSpace = true;

            Material matSifao = new Material(litShader);
            matSifao.color = new Color(0.8f, 0.1f, 1f); // Roxo Necromante
            feixeSifaoNecro.material = matSifao;
            feixeSifaoNecro.enabled = false;
        }

        if (feixeLaserSuprema == null)
        {
            GameObject objSuprema = new GameObject("FeixeLaserSuprema");
            objSuprema.transform.SetParent(transform);
            feixeLaserSuprema = objSuprema.AddComponent<LineRenderer>();
            feixeLaserSuprema.startWidth = 0.25f;
            feixeLaserSuprema.endWidth = 0.2f;
            feixeLaserSuprema.positionCount = 2;
            feixeLaserSuprema.useWorldSpace = true;

            Material matLaser = new Material(litShader);
            matLaser.color = new Color(1f, 0.1f, 0.6f); // Magenta Laser Concentrado
            feixeLaserSuprema.material = matLaser;
            feixeLaserSuprema.enabled = false;
        }
    }
}
