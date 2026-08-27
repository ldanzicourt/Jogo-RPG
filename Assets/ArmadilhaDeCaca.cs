using System.Collections;
using UnityEngine;

public class ArmadilhaDeCaca : MonoBehaviour
{
    [Header("Configurações da Armadilha")]
    public float danoImpacto = 25f;
    public float duracaoImobilizacao = 3.5f;
    public float raioAtivacao = 1.2f;
    public float tempoVidaSemAtivar = 45f;

    private bool armada = false;
    private bool disparada = false;
    private Transform mandibulaEsquerda;
    private Transform mandibulaDireita;

    void Awake()
    {
        ConstruirModeloArmadilha();
    }

    void Start()
    {
        // Pequeno atraso de 0.2s para armar e evitar que o jogador ative a própria armadilha ao soltar
        StartCoroutine(RotinaArmar());
        Destroy(gameObject, tempoVidaSemAtivar);
    }

    IEnumerator RotinaArmar()
    {
        yield return new WaitForSeconds(0.2f);
        armada = true;
    }

    void Update()
    {
        if (!armada || disparada) return;

        // Detecta inimigos na área
        Collider[] hits = Physics.OverlapSphere(transform.position, raioAtivacao);
        foreach (var col in hits)
        {
            if (col.transform.root == transform.root || col.CompareTag("Player") || col.GetComponent<PlayerController>() != null)
                continue;

            SistemaDeVida vida = col.GetComponent<SistemaDeVida>();
            if (vida == null) vida = col.GetComponentInParent<SistemaDeVida>();

            if (vida != null)
            {
                AtivarArmadilha(col.gameObject, vida);
                break;
            }
        }
    }

    void AtivarArmadilha(GameObject inimigoObj, SistemaDeVida vida)
    {
        disparada = true;
        armada = false;

        // Animação das mandíbulas se fechando bruscamente
        StartCoroutine(RotinaFecharMandibulas());

        // Causa dano físico de impacto/sangramento
        if (vida != null)
        {
            vida.ReceberDano(danoImpacto, new Color(0.9f, 0.2f, 0.1f));
        }

        // Aplica imobilização no inimigo por 3.5s
        StartCoroutine(RotinaImobilizarInimigo(inimigoObj));

        // Efeito visual de faíscas metálicas
        CriarEfeitoImpacto();

        // Destrói o objeto após a conclusão do efeito
        Destroy(gameObject, duracaoImobilizacao + 1.0f);
    }

    IEnumerator RotinaFecharMandibulas()
    {
        float t = 0f;
        float duracaoFechamento = 0.08f; // Fechamento em fração de segundo (estalido)

        Quaternion rotEsqAberta = Quaternion.Euler(0f, 0f, -80f);
        Quaternion rotEsqFechada = Quaternion.Euler(0f, 0f, 0f);

        Quaternion rotDirAberta = Quaternion.Euler(0f, 0f, 80f);
        Quaternion rotDirFechada = Quaternion.Euler(0f, 0f, 0f);

        while (t < duracaoFechamento)
        {
            t += Time.deltaTime;
            float progresso = t / duracaoFechamento;

            if (mandibulaEsquerda != null)
                mandibulaEsquerda.localRotation = Quaternion.Slerp(rotEsqAberta, rotEsqFechada, progresso);
            if (mandibulaDireita != null)
                mandibulaDireita.localRotation = Quaternion.Slerp(rotDirAberta, rotDirFechada, progresso);

            yield return null;
        }

        if (mandibulaEsquerda != null) mandibulaEsquerda.localRotation = rotEsqFechada;
        if (mandibulaDireita != null) mandibulaDireita.localRotation = rotDirFechada;
    }

    IEnumerator RotinaImobilizarInimigo(GameObject alvo)
    {
        if (alvo == null) yield break;

        Rigidbody rb = alvo.GetComponent<Rigidbody>();
        Vector3 posOriginal = alvo.transform.position;
        bool eraKinematic = false;

        if (rb != null)
        {
            eraKinematic = rb.isKinematic;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        float tempoRestante = duracaoImobilizacao;
        while (tempoRestante > 0f)
        {
            if (alvo == null) yield break;

            // Mantém a posição fixa no chão durante a imobilização
            alvo.transform.position = new Vector3(posOriginal.x, alvo.transform.position.y, posOriginal.z);

            tempoRestante -= Time.deltaTime;
            yield return null;
        }

        if (alvo != null && rb != null)
        {
            rb.isKinematic = eraKinematic;
        }
    }

    void CriarEfeitoImpacto()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject faisca = GameObject.CreatePrimitive(PrimitiveType.Cube);
            faisca.name = "FaiscaMetal";
            faisca.transform.position = transform.position + Vector3.up * 0.2f + Random.insideUnitSphere * 0.2f;
            faisca.transform.localScale = Vector3.one * 0.08f;
            Destroy(faisca.GetComponent<Collider>());

            Renderer r = faisca.GetComponent<Renderer>();
            if (r != null)
            {
                Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
                Material mat = new Material(litShader);
                mat.color = new Color(1f, 0.85f, 0.3f);
                r.material = mat;
            }

            Destroy(faisca, 0.3f);
        }
    }

    void ConstruirModeloArmadilha()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

        Material matFerroEscuro = new Material(litShader);
        matFerroEscuro.color = new Color(0.18f, 0.18f, 0.20f);

        Material matAcoDentes = new Material(litShader);
        matAcoDentes.color = new Color(0.45f, 0.48f, 0.52f);

        // 1. Base Central (Mola e Placa de Pressão)
        GameObject basePlaca = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basePlaca.name = "PlacaPressao";
        basePlaca.transform.SetParent(transform, false);
        basePlaca.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        basePlaca.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);
        Destroy(basePlaca.GetComponent<Collider>());
        basePlaca.GetComponent<Renderer>().material = matFerroEscuro;

        // Barra central de sustentação
        GameObject barraCentral = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barraCentral.name = "BarraCentral";
        barraCentral.transform.SetParent(transform, false);
        barraCentral.transform.localPosition = new Vector3(0f, 0.03f, 0f);
        barraCentral.transform.localScale = new Vector3(0.1f, 0.04f, 0.8f);
        Destroy(barraCentral.GetComponent<Collider>());
        barraCentral.GetComponent<Renderer>().material = matFerroEscuro;

        // 2. Mandíbula Esquerda (Pivot)
        GameObject objPivotEsq = new GameObject("PivotMandibulaEsq");
        objPivotEsq.transform.SetParent(transform, false);
        objPivotEsq.transform.localPosition = new Vector3(-0.05f, 0.04f, 0f);
        objPivotEsq.transform.localRotation = Quaternion.Euler(0f, 0f, -80f); // Aberta
        mandibulaEsquerda = objPivotEsq.transform;

        // Arco da Mandíbula Esquerda
        GameObject arcoEsq = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arcoEsq.name = "ArcoEsq";
        arcoEsq.transform.SetParent(mandibulaEsquerda, false);
        arcoEsq.transform.localPosition = new Vector3(-0.35f, 0f, 0f);
        arcoEsq.transform.localScale = new Vector3(0.06f, 0.04f, 0.75f);
        Destroy(arcoEsq.GetComponent<Collider>());
        arcoEsq.GetComponent<Renderer>().material = matFerroEscuro;

        // Dentes de Aço da Mandíbula Esquerda
        for (int d = 0; d < 5; d++)
        {
            GameObject dente = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dente.name = "DenteEsq_" + d;
            dente.transform.SetParent(arcoEsq.transform, false);
            dente.transform.localPosition = new Vector3(0.04f, 0.04f, (d * 0.14f) - 0.28f);
            dente.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            dente.transform.localScale = new Vector3(0.04f, 0.08f, 0.04f);
            Destroy(dente.GetComponent<Collider>());
            dente.GetComponent<Renderer>().material = matAcoDentes;
        }

        // 3. Mandíbula Direita (Pivot)
        GameObject objPivotDir = new GameObject("PivotMandibulaDir");
        objPivotDir.transform.SetParent(transform, false);
        objPivotDir.transform.localPosition = new Vector3(0.05f, 0.04f, 0f);
        objPivotDir.transform.localRotation = Quaternion.Euler(0f, 0f, 80f); // Aberta
        mandibulaDireita = objPivotDir.transform;

        // Arco da Mandíbula Direita
        GameObject arcoDir = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arcoDir.name = "ArcoDir";
        arcoDir.transform.SetParent(mandibulaDireita, false);
        arcoDir.transform.localPosition = new Vector3(0.35f, 0f, 0f);
        arcoDir.transform.localScale = new Vector3(0.06f, 0.04f, 0.75f);
        Destroy(arcoDir.GetComponent<Collider>());
        arcoDir.GetComponent<Renderer>().material = matFerroEscuro;

        // Dentes de Aço da Mandíbula Direita
        for (int d = 0; d < 5; d++)
        {
            GameObject dente = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dente.name = "DenteDir_" + d;
            dente.transform.SetParent(arcoDir.transform, false);
            dente.transform.localPosition = new Vector3(-0.04f, 0.04f, (d * 0.14f) - 0.28f);
            dente.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);
            dente.transform.localScale = new Vector3(0.04f, 0.08f, 0.04f);
            Destroy(dente.GetComponent<Collider>());
            dente.GetComponent<Renderer>().material = matAcoDentes;
        }
    }
}
