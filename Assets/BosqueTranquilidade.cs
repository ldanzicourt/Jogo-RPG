using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BosqueTranquilidade : MonoBehaviour
{
    public float duracao = 8f;
    public float raio = 10f;
    public float curaPorSegundo = 15f;
    public float danoPorSegundoInimigo = 10f;
    public float duracaoEnraizamento = 4f;

    private float tempoCriacao;
    private HashSet<GameObject> inimigosEnraizados = new HashSet<GameObject>();
    private StatusJogador statusJogador;

    void Start()
    {
        // Garante que o bosque esteja perfeitamente colado ao solo
        AjustarParaSolo();

        tempoCriacao = Time.time;
        statusJogador = FindAnyObjectByType<StatusJogador>();
        ConstruirBosqueVisual();
        StartCoroutine(RotinaEfeitosBosque());
        Destroy(gameObject, duracao);
    }

    void AjustarParaSolo()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 8f, Vector3.down, 40f);
        float melhorY = float.MinValue;
        bool achou = false;
        foreach (var h in hits)
        {
            if (h.collider.isTrigger) continue;
            if (h.collider.CompareTag("Player") || h.collider.GetComponentInParent<PlayerController>() != null) continue;
            if (h.collider.GetComponentInParent<SistemaDeVida>() != null || h.collider.CompareTag("Enemy")) continue;

            if (h.point.y > melhorY)
            {
                melhorY = h.point.y;
                achou = true;
            }
        }
        if (achou)
        {
            transform.position = new Vector3(transform.position.x, melhorY, transform.position.z);
        }
    }

    void Update()
    {
        // Enraíza novos inimigos que entram na área
        Collider[] alvos = Physics.OverlapSphere(transform.position, raio);
        foreach (var col in alvos)
        {
            if (col.CompareTag("Player") || col.GetComponent<PlayerController>() != null) continue;

            SistemaDeVida vida = col.GetComponent<SistemaDeVida>();
            if (vida == null) vida = col.GetComponentInParent<SistemaDeVida>();

            if (vida != null && !inimigosEnraizados.Contains(vida.gameObject))
            {
                inimigosEnraizados.Add(vida.gameObject);

                // Aplica 100% de lentidão (enraizamento / imobilização) por 4s
                vida.AplicarLentidao(1.0f, duracaoEnraizamento);

                // Passiva: Gera uma Semente Vital no inimigo enraizado
                GerarSementeVital(col.transform.position);

                // Cria raízes visuais ao redor dos pés do inimigo
                CriarRaizesInimigo(col.transform.position);
            }
        }
    }

    IEnumerator RotinaEfeitosBosque()
    {
        while (Time.time < tempoCriacao + duracao)
        {
            // Cura o Jogador e Aliados na área
            if (statusJogador != null)
            {
                float distPlayer = Vector3.Distance(transform.position, statusJogador.transform.position);
                if (distPlayer <= raio)
                {
                    statusJogador.Curar(curaPorSegundo * 0.5f);
                }
            }

            // Dano de Natureza contínuo aos inimigos na área
            foreach (var inimigoObj in inimigosEnraizados)
            {
                if (inimigoObj != null)
                {
                    SistemaDeVida vida = inimigoObj.GetComponent<SistemaDeVida>();
                    if (vida != null)
                    {
                        vida.ReceberDano(danoPorSegundoInimigo * 0.5f, new Color(0.2f, 0.9f, 0.3f));
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void GerarSementeVital(Vector3 posicao)
    {
        GameObject sementeObj = new GameObject("SementeVital");
        sementeObj.transform.position = posicao + Vector3.up * 0.1f;
        sementeObj.AddComponent<SementeVital>();
    }

    void CriarRaizesInimigo(Vector3 pos)
    {
        GameObject objRaiz = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        objRaiz.name = "RaizesEnraizamento";
        objRaiz.transform.position = pos + Vector3.up * 0.4f;
        objRaiz.transform.localScale = new Vector3(0.9f, 0.4f, 0.9f);
        Destroy(objRaiz.GetComponent<Collider>());

        Renderer rend = objRaiz.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = new Color(0.35f, 0.22f, 0.12f);
            rend.material = mat;
        }

        Destroy(objRaiz, duracaoEnraizamento);
    }

    void ConstruirBosqueVisual()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

        // 1. Círculo do Santuário no Chão
        GameObject circulo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        circulo.name = "ChaoSantuario";
        circulo.transform.SetParent(transform, false);
        circulo.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        circulo.transform.localScale = new Vector3(raio * 2f, 0.005f, raio * 2f);
        Destroy(circulo.GetComponent<Collider>());

        Renderer rendChao = circulo.GetComponent<Renderer>();
        Material matChao = new Material(litShader);
        matChao.color = new Color(0.15f, 0.55f, 0.22f, 0.6f);
        rendChao.material = matChao;

        // 2. Anel de Raízes e Flores ao redor da circunferência
        int totalArbustos = 14;
        for (int i = 0; i < totalArbustos; i++)
        {
            float angulo = (i * 360f / totalArbustos) * Mathf.Deg2Rad;
            Vector3 posBorda = new Vector3(Mathf.Cos(angulo) * (raio - 0.5f), 0.4f, Mathf.Sin(angulo) * (raio - 0.5f));

            // Raiz/Tronco
            GameObject tronco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tronco.name = "Tronco_" + i;
            tronco.transform.SetParent(transform, false);
            tronco.transform.localPosition = posBorda;
            tronco.transform.localRotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-15f, 15f));
            tronco.transform.localScale = new Vector3(0.35f, 0.8f, 0.35f);
            Destroy(tronco.GetComponent<Collider>());

            Renderer rTronco = tronco.GetComponent<Renderer>();
            Material matTronco = new Material(litShader);
            matTronco.color = new Color(0.28f, 0.18f, 0.10f);
            rTronco.material = matTronco;

            // Copa Floral / Folhagem
            GameObject flor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flor.name = "Folhagem_" + i;
            flor.transform.SetParent(transform, false);
            flor.transform.localPosition = posBorda + Vector3.up * 0.7f;
            flor.transform.localScale = new Vector3(0.9f, 0.7f, 0.9f);
            Destroy(flor.GetComponent<Collider>());

            Renderer rFlor = flor.GetComponent<Renderer>();
            Material matFlor = new Material(litShader);
            matFlor.color = (i % 2 == 0) ? new Color(0.2f, 0.85f, 0.3f) : new Color(0.95f, 0.8f, 0.2f);
            if (matFlor.HasProperty("_EmissionColor"))
            {
                matFlor.EnableKeyword("_EMISSION");
                matFlor.SetColor("_EmissionColor", matFlor.color * 0.5f);
            }
            rFlor.material = matFlor;
        }

        // 3. Luz Central Esmeralda
        GameObject objLuz = new GameObject("LuzSantuario");
        objLuz.transform.SetParent(transform, false);
        objLuz.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        Light luz = objLuz.AddComponent<Light>();
        luz.color = new Color(0.3f, 1f, 0.5f);
        luz.range = raio * 1.5f;
        luz.intensity = 2.0f;
    }
}
