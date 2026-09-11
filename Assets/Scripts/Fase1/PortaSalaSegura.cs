using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortaSalaSegura : MonoBehaviour
{
    [Header("Estrutura da Porta")]
    public Transform portaPainelEsquerdo;
    public Transform portaPainelDireito;
    public Transform pontoInteriorSafeRoom;
    public float larguraAbertura = 2.4f;

    [Header("Status")]
    public bool portaAberta = false;
    public bool portaTrancadaFimFase = false;

    [Header("Detecção dos Jogadores")]
    public BoxCollider gatilhoFimDeFase;

    private HashSet<GameObject> jogadoresNaSala = new HashSet<GameObject>();
    private GameManagerFase1 gameManager;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManagerFase1>();
        ConstruirPortaVisual();
    }

    void Update()
    {
        if (portaTrancadaFimFase) return;

        // Se o jogador estiver perto da porta, abre automaticamente ou com [E]
        Transform playerT = null;
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) playerT = pc.transform;
        else
        {
            PlayerCombatD20 pCombat = FindAnyObjectByType<PlayerCombatD20>();
            if (pCombat != null) playerT = pCombat.transform;
        }

        if (playerT != null && !portaAberta)
        {
            float dist = Vector3.Distance(transform.position, playerT.position);
            if (dist <= 6.0f)
            {
                AbrirPorta();
            }
        }

        // Checagem de Fim de Fase: todos os jogadores vivos (player + bots) entraram na Safe Room?
        if (portaAberta && !portaTrancadaFimFase)
        {
            ChecarSeTodosEntraram();
        }
    }

    public void AbrirPorta()
    {
        if (portaAberta || portaTrancadaFimFase) return;
        portaAberta = true;
        Debug.Log("🚪 PORTA DA SALA SEGURA ABERTA!");
        StartCoroutine(RotinaMovimentoPorta(true));
    }

    public void FecharPortaETerminarFase()
    {
        if (portaTrancadaFimFase) return;
        portaTrancadaFimFase = true;
        Debug.Log("🔒 SALA SEGURA TRANCADA! TODOS OS SOBREVIVENTES ESTÃO A SALVO!");
        StartCoroutine(RotinaMovimentoPorta(false));

        if (gameManager != null)
        {
            gameManager.ConcluirFaseComSucesso();
        }
    }

    void ChecarSeTodosEntraram()
    {
        if (pontoInteriorSafeRoom == null) return;

        // Verifica se o jogador entrou na Sala Segura
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            float dist = Vector3.Distance(player.transform.position, pontoInteriorSafeRoom.position);
            if (dist <= 7.0f)
            {
                FecharPortaETerminarFase();
            }
        }
        else
        {
            PlayerCombatD20 pD20 = FindAnyObjectByType<PlayerCombatD20>();
            if (pD20 != null && pD20.pvAtual > 0)
            {
                float dist = Vector3.Distance(pD20.transform.position, pontoInteriorSafeRoom.position);
                if (dist <= 7.0f)
                {
                    FecharPortaETerminarFase();
                }
            }
        }
    }

    IEnumerator RotinaMovimentoPorta(bool abrir)
    {
        float duracao = 1.2f;
        float t = 0f;
        float offsetAlvo = abrir ? larguraAbertura : 0f;
        float offsetInicial = abrir ? 0f : larguraAbertura;

        while (t < duracao)
        {
            t += Time.deltaTime;
            float lerp = Mathf.SmoothStep(0f, 1f, t / duracao);
            float curX = Mathf.Lerp(offsetInicial, offsetAlvo, lerp);

            if (portaPainelEsquerdo != null)
                portaPainelEsquerdo.localPosition = new Vector3(-curX, 1.8f, 0f);

            if (portaPainelDireito != null)
                portaPainelDireito.localPosition = new Vector3(curX, 1.8f, 0f);

            yield return null;
        }
    }

    void ConstruirPortaVisual()
    {
        Material matAco = MateriaisAlienigenas.CriarMaterialSafeRoom();
        Material matVerde = MateriaisAlienigenas.CriarMaterialLuzSegura();

        // Moldura do Portal
        GameObject portal = new GameObject("MolduraPortal");
        portal.transform.SetParent(transform, false);

        // Coluna Esquerda
        GameObject colEsq = GameObject.CreatePrimitive(PrimitiveType.Cube);
        colEsq.transform.SetParent(portal.transform, false);
        colEsq.transform.localPosition = new Vector3(-2.8f, 2.0f, 0f);
        colEsq.transform.localScale = new Vector3(0.8f, 4.0f, 1.0f);
        colEsq.GetComponent<Renderer>().material = matAco;

        // Coluna Direita
        GameObject colDir = GameObject.CreatePrimitive(PrimitiveType.Cube);
        colDir.transform.SetParent(portal.transform, false);
        colDir.transform.localPosition = new Vector3(2.8f, 2.0f, 0f);
        colDir.transform.localScale = new Vector3(0.8f, 4.0f, 1.0f);
        colDir.GetComponent<Renderer>().material = matAco;

        // Viga Superior
        GameObject vigaSup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vigaSup.transform.SetParent(portal.transform, false);
        vigaSup.transform.localPosition = new Vector3(0f, 4.2f, 0f);
        vigaSup.transform.localScale = new Vector3(6.4f, 0.8f, 1.2f);
        vigaSup.GetComponent<Renderer>().material = matAco;

        // Painel Esquerdo Deslizante
        GameObject painelEsq = GameObject.CreatePrimitive(PrimitiveType.Cube);
        painelEsq.name = "PainelPortaEsq";
        painelEsq.transform.SetParent(transform, false);
        painelEsq.transform.localPosition = new Vector3(-1.2f, 1.8f, 0f);
        painelEsq.transform.localScale = new Vector3(2.4f, 3.6f, 0.35f);
        painelEsq.GetComponent<Renderer>().material = matAco;
        portaPainelEsquerdo = painelEsq.transform;

        // Painel Direito Deslizante
        GameObject painelDir = GameObject.CreatePrimitive(PrimitiveType.Cube);
        painelDir.name = "PainelPortaDir";
        painelDir.transform.SetParent(transform, false);
        painelDir.transform.localPosition = new Vector3(1.2f, 1.8f, 0f);
        painelDir.transform.localScale = new Vector3(2.4f, 3.6f, 0.35f);
        painelDir.GetComponent<Renderer>().material = matAco;
        portaPainelDireito = painelDir.transform;

        // Sinalizadores Verdes de Segurança no topo
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject luzObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            luzObj.name = "SinalizadorSeguro";
            luzObj.transform.SetParent(portal.transform, false);
            luzObj.transform.localPosition = new Vector3(i * 1.5f, 4.2f, -0.65f);
            luzObj.transform.localScale = Vector3.one * 0.35f;
            Destroy(luzObj.GetComponent<Collider>());
            luzObj.GetComponent<Renderer>().material = matVerde;

            Light lt = luzObj.AddComponent<Light>();
            lt.color = new Color(0.1f, 0.95f, 0.4f);
            lt.range = 6.0f;
            lt.intensity = 2.5f;
        }

        // Ponto Interior de referência
        if (pontoInteriorSafeRoom == null)
        {
            GameObject pt = new GameObject("PontoInteriorSafeRoom");
            pt.transform.SetParent(transform, false);
            pt.transform.localPosition = new Vector3(0f, 0f, 5.0f);
            pontoInteriorSafeRoom = pt.transform;
        }
    }

    public bool EstaAberta() => portaAberta;
}
