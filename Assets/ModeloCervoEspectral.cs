using System.Collections;
using UnityEngine;

public class ModeloCervoEspectral : MonoBehaviour
{
    private Transform raizCervo;
    private Camera cameraJogador;
    private Vector3 cameraPosOriginal;
    private Quaternion cameraRotOriginal;
    private bool emTransformacao = false;

    void Awake()
    {
        cameraJogador = Camera.main;
        ConstruirModeloCervo();
        if (raizCervo != null) raizCervo.gameObject.SetActive(false);
    }

    public IEnumerator RotinaPassoDoCervo(float duracao, PlayerController player, Vector3 direcaoMovimento)
    {
        if (emTransformacao) yield break;
        emTransformacao = true;

        if (cameraJogador == null) cameraJogador = Camera.main;

        // Oculta armas de 1ª pessoa do Druida durante a forma animal
        ModeloDruidaProcedural modeloDruida = GetComponentInParent<ModeloDruidaProcedural>();
        if (modeloDruida == null) modeloDruida = GetComponent<ModeloDruidaProcedural>();
        if (modeloDruida != null) modeloDruida.DefinirVisibilidadeArmas(false);

        // Ativa o Cervo Espectral
        if (raizCervo != null)
        {
            raizCervo.gameObject.SetActive(true);
            raizCervo.localPosition = new Vector3(0f, -0.6f, 0f);
        }

        // Posiciona a câmera em 3ª pessoa (3.5m atrás e 1.6m acima)
        Vector3 offsetTerceiraPessoa = new Vector3(0f, 1.6f, -3.5f);
        Vector3 posCamInicial = (cameraJogador != null) ? cameraJogador.transform.localPosition : Vector3.zero;

        float tempoDecorrido = 0f;
        float metadeDuracao = duracao * 0.5f;

        // Animação de Salto e Galope do Cervo
        while (tempoDecorrido < duracao)
        {
            tempoDecorrido += Time.deltaTime;
            float progresso = tempoDecorrido / duracao;

            // Arco do Salto (sobe e desce parabolicamente)
            float alturaArco = Mathf.Sin(progresso * Mathf.PI) * 1.8f;

            if (raizCervo != null)
            {
                raizCervo.localPosition = new Vector3(0f, -0.6f + (alturaArco * 0.4f), 0f);
                // Leve inclinação para frente no salto e subida na descida
                float inclinacaoPitch = Mathf.Lerp(-15f, 20f, progresso);
                raizCervo.localRotation = Quaternion.Euler(inclinacaoPitch, 0f, 0f);
            }

            // Transição suave da câmera para 3ª pessoa e retorno no final
            if (cameraJogador != null)
            {
                float transicaoCam = (progresso < 0.2f) 
                    ? (progresso / 0.2f) 
                    : (progresso > 0.8f ? (1f - (progresso - 0.8f) / 0.2f) : 1f);

                cameraJogador.transform.localPosition = Vector3.Lerp(posCamInicial, offsetTerceiraPessoa, transicaoCam);
            }

            yield return null;
        }

        // Restaura a câmera para 1ª pessoa
        if (cameraJogador != null)
        {
            cameraJogador.transform.localPosition = posCamInicial;
        }

        // Desativa o Cervo e restaura as armas de 1ª pessoa
        if (raizCervo != null)
        {
            raizCervo.gameObject.SetActive(false);
        }

        if (modeloDruida != null)
        {
            modeloDruida.DefinirVisibilidadeArmas(true);
        }

        emTransformacao = false;
    }

    void ConstruirModeloCervo()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

        GameObject objRaiz = new GameObject("CervoEspectral");
        objRaiz.transform.SetParent(transform, false);
        objRaiz.transform.localPosition = new Vector3(0f, -0.6f, 0f);
        raizCervo = objRaiz.transform;

        // Material Espectral Verde Luminoso
        Material matEspectral = new Material(litShader);
        matEspectral.color = new Color(0.2f, 1f, 0.5f, 0.85f);
        if (matEspectral.HasProperty("_EmissionColor"))
        {
            matEspectral.EnableKeyword("_EMISSION");
            matEspectral.SetColor("_EmissionColor", new Color(0.15f, 0.9f, 0.4f) * 1.6f);
        }

        Material matGalhadas = new Material(litShader);
        matGalhadas.color = new Color(0.9f, 1f, 0.4f);
        if (matGalhadas.HasProperty("_EmissionColor"))
        {
            matGalhadas.EnableKeyword("_EMISSION");
            matGalhadas.SetColor("_EmissionColor", new Color(0.9f, 1f, 0.3f) * 2f);
        }

        // 1. Tronco / Corpo do Cervo
        GameObject corpo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        corpo.name = "CorpoCervo";
        corpo.transform.SetParent(raizCervo, false);
        corpo.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        corpo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        corpo.transform.localScale = new Vector3(0.55f, 0.85f, 0.55f);
        Destroy(corpo.GetComponent<Collider>());
        corpo.GetComponent<Renderer>().material = matEspectral;

        // 2. Pescoço
        GameObject pescoco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pescoco.name = "Pescoco";
        pescoco.transform.SetParent(raizCervo, false);
        pescoco.transform.localPosition = new Vector3(0f, 1.4f, 0.65f);
        pescoco.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
        pescoco.transform.localScale = new Vector3(0.3f, 0.45f, 0.3f);
        Destroy(pescoco.GetComponent<Collider>());
        pescoco.GetComponent<Renderer>().material = matEspectral;

        // 3. Cabeça
        GameObject cabeca = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cabeca.name = "Cabeca";
        cabeca.transform.SetParent(raizCervo, false);
        cabeca.transform.localPosition = new Vector3(0f, 1.75f, 0.95f);
        cabeca.transform.localScale = new Vector3(0.32f, 0.32f, 0.48f);
        Destroy(cabeca.GetComponent<Collider>());
        cabeca.GetComponent<Renderer>().material = matEspectral;

        // 4. Galhadas Majestosas (Antlers)
        for (int side = -1; side <= 1; side += 2)
        {
            // Ramo Principal da Galhada
            GameObject galhoPrincipal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            galhoPrincipal.name = "GalhadaPrincipal_" + (side > 0 ? "Dir" : "Esq");
            galhoPrincipal.transform.SetParent(raizCervo, false);
            galhoPrincipal.transform.localPosition = new Vector3(side * 0.22f, 2.15f, 0.90f);
            galhoPrincipal.transform.localRotation = Quaternion.Euler(-30f, side * 35f, side * 35f);
            galhoPrincipal.transform.localScale = new Vector3(0.05f, 0.45f, 0.05f);
            Destroy(galhoPrincipal.GetComponent<Collider>());
            galhoPrincipal.GetComponent<Renderer>().material = matGalhadas;

            // Pontas secundárias da Galhada
            for (int p = 0; p < 2; p++)
            {
                GameObject ponta = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ponta.name = "PontaGalhada_" + side + "_" + p;
                ponta.transform.SetParent(raizCervo, false);
                ponta.transform.localPosition = new Vector3(side * (0.28f + (p * 0.12f)), 2.25f + (p * 0.18f), 0.95f + (p * 0.08f));
                ponta.transform.localRotation = Quaternion.Euler(p * 20f - 40f, side * (45f + p * 20f), side * (40f + p * 15f));
                ponta.transform.localScale = new Vector3(0.035f, 0.20f, 0.035f);
                Destroy(ponta.GetComponent<Collider>());
                ponta.GetComponent<Renderer>().material = matGalhadas;
            }
        }

        // 5. Quatro Patas Articuladas
        Vector3[] posPatas = new Vector3[]
        {
            new Vector3(-0.25f, 0.45f, 0.65f),  // Dianteira Esq
            new Vector3(0.25f, 0.45f, 0.65f),   // Dianteira Dir
            new Vector3(-0.25f, 0.45f, -0.65f), // Traseira Esq
            new Vector3(0.25f, 0.45f, -0.65f)   // Traseira Dir
        };

        for (int i = 0; i < posPatas.Length; i++)
        {
            GameObject pata = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pata.name = "Pata_" + i;
            pata.transform.SetParent(raizCervo, false);
            pata.transform.localPosition = posPatas[i];
            pata.transform.localScale = new Vector3(0.10f, 0.45f, 0.10f);
            Destroy(pata.GetComponent<Collider>());
            pata.GetComponent<Renderer>().material = matEspectral;
        }

        // 6. Luz de Aura Espectral
        GameObject objLuz = new GameObject("LuzAuraCervo");
        objLuz.transform.SetParent(raizCervo, false);
        objLuz.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        Light luz = objLuz.AddComponent<Light>();
        luz.color = new Color(0.2f, 1f, 0.5f);
        luz.range = 5f;
        luz.intensity = 2.5f;
    }
}
