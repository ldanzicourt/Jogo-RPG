using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class GeradorPradariaDaFerrugem : MonoBehaviour
{
    [Header("Configurações do Mapa")]
    public bool gerarAoIniciar = true;
    public bool autoBakeNavMesh = true;

    [Header("Prefabs e Templates")]
    public GameObject jogadorPrefab;

    private NavMeshSurface navMeshSurface;
    private GameObject containerMapa;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoIniciarEmCena()
    {
        if (FindAnyObjectByType<GeradorPradariaDaFerrugem>() == null)
        {
            GameObject g = new GameObject("GeradorPradariaDaFerrugem_Auto");
            g.AddComponent<GeradorPradariaDaFerrugem>();
        }
    }

    void Awake()
    {
        if (gerarAoIniciar)
        {
            ConstruirPradariaDaFerrugem();
        }
    }

    [ContextMenu("Construir Pradaria da Ferrugem")]
    public void ConstruirPradariaDaFerrugem()
    {
        // 0. Limpa o cenário legado de teste (chão cinza, cápsulas de teste) e libera o jogo
        LimparCenarioLegado();

        if (containerMapa != null) DestroyImmediate(containerMapa);
        containerMapa = new GameObject("--- PRADARIA DA FERRUGEM ---");

        // 1. Atmosfera e Iluminação Alienígena
        ConfigurarAtmosferaEstatica();

        // 2. Geometria das 3 Seções e Gargalos
        ConstruirGeometriaMapa();

        // 3. Adiciona Elementos Visuais: Correntes Colossais, Trincheiras e Barricadas
        AdicionarSucatasECorrentes();

        // 4. Constrói a Sala Segura (Safe Room)
        ConstruirSafeRoom();

        // 5. Coze o NavMesh para os inimigos e bots
        if (autoBakeNavMesh)
        {
            BakeNavMesh();
        }

        // 6. Spawna Herói, Bots e Hordas de Inimigos
        SpawnarEntidades();

        // 7. Configura o GameManager da Fase
        if (FindAnyObjectByType<GameManagerFase1>() == null)
        {
            GameObject gm = new GameObject("GameManagerFase1");
            gm.AddComponent<GameManagerFase1>();
        }

        Debug.Log("🌌 PRADARIA DA FERRUGEM (FASE 1) CONSTRUÍDA COM SUCESSO!");
    }

    void ConfigurarAtmosferaEstatica()
    {
        // Garante luz direcional sombria e gerenciador de tempestade estática
        Light luzSol = FindAnyObjectByType<Light>();
        if (luzSol == null)
        {
            GameObject luzObj = new GameObject("LuzTempestadeAlien");
            luzSol = luzObj.AddComponent<Light>();
            luzSol.type = LightType.Directional;
            luzObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        if (FindAnyObjectByType<TempestadeEstaticaManager>() == null)
        {
            GameObject tMgr = new GameObject("TempestadeEstaticaManager");
            tMgr.transform.SetParent(containerMapa.transform);
            TempestadeEstaticaManager tem = tMgr.AddComponent<TempestadeEstaticaManager>();
            tem.luzDirecional = luzSol;
        }
    }

    void ConstruirGeometriaMapa()
    {
        Material matChao = MateriaisAlienigenas.CriarMaterialChaoFerrugem();
        Material matParedes = MateriaisAlienigenas.CriarMaterialSucataOxidada();

        // SEÇÃO 1: O Despertar (Z: 0 a 40, X: -16 a 16)
        CriarPlataforma("Chao_Secao1", new Vector3(0f, 0f, 20f), new Vector3(32f, 1f, 40f), matChao);
        CriarParede("ParedeEsq_S1", new Vector3(-16f, 3.5f, 20f), new Vector3(1f, 6f, 40f), matParedes);
        CriarParede("ParedeDir_S1", new Vector3(16f, 3.5f, 20f), new Vector3(1f, 6f, 40f), matParedes);
        CriarParede("ParedeFundo_S1", new Vector3(0f, 3.5f, 0f), new Vector3(33f, 6f, 1f), matParedes);

        // GARGALO 1 (Z: 40 a 70, X: -4 a 4) - Ponto de emboscada estreito
        CriarPlataforma("Chao_Gargalo1", new Vector3(0f, 0f, 55f), new Vector3(8f, 1f, 30f), matChao);
        CriarParede("ParedeEsq_G1", new Vector3(-4f, 3.5f, 55f), new Vector3(1f, 6f, 30f), matParedes);
        // Parede Direita com abertura para o Caminho Alternativo (Dead-End)
        CriarParede("ParedeDir_G1_A", new Vector3(4f, 3.5f, 46f), new Vector3(1f, 6f, 12f), matParedes);
        CriarParede("ParedeDir_G1_B", new Vector3(4f, 3.5f, 64f), new Vector3(1f, 6f, 12f), matParedes);

        // CAMINHO ALTERNATIVO 1 (Bifurcação Sem Saída com Baú de Loot) (X: 4 a 18, Z: 52 a 58)
        CriarPlataforma("Chao_DeadEnd1", new Vector3(11f, 0f, 55f), new Vector3(14f, 1f, 8f), matChao);
        CriarParede("ParedeDeadEnd1_Norte", new Vector3(11f, 3.5f, 59f), new Vector3(14f, 6f, 1f), matParedes);
        CriarParede("ParedeDeadEnd1_Sul", new Vector3(11f, 3.5f, 51f), new Vector3(14f, 6f, 1f), matParedes);
        CriarParede("ParedeDeadEnd1_Leste", new Vector3(18f, 3.5f, 55f), new Vector3(1f, 6f, 9f), matParedes);

        // Baú de Loot no Dead-End 1
        GameObject bau1 = new GameObject("BauDeLoot_1");
        bau1.transform.SetParent(containerMapa.transform);
        bau1.transform.position = new Vector3(16f, 0.5f, 55f);
        bau1.AddComponent<BauDeLoot>();

        // SEÇÃO 2: Campo de Trincheiras e Correntes Gigantes (Z: 70 a 130, X: -22 a 22)
        CriarPlataforma("Chao_Secao2", new Vector3(0f, 0f, 100f), new Vector3(44f, 1f, 60f), matChao);
        CriarParede("ParedeEsq_S2", new Vector3(-22f, 3.5f, 100f), new Vector3(1f, 6f, 60f), matParedes);
        CriarParede("ParedeDir_S2", new Vector3(22f, 3.5f, 100f), new Vector3(1f, 6f, 60f), matParedes);

        // GARGALO 2 (Z: 130 a 160, X: -5 a 5)
        CriarPlataforma("Chao_Gargalo2", new Vector3(0f, 0f, 145f), new Vector3(10f, 1f, 30f), matChao);
        // Parede Esquerda com abertura para Dead-End 2
        CriarParede("ParedeEsq_G2_A", new Vector3(-5f, 3.5f, 136f), new Vector3(1f, 6f, 12f), matParedes);
        CriarParede("ParedeEsq_G2_B", new Vector3(-5f, 3.5f, 154f), new Vector3(1f, 6f, 12f), matParedes);
        CriarParede("ParedeDir_G2", new Vector3(5f, 3.5f, 145f), new Vector3(1f, 6f, 30f), matParedes);

        // CAMINHO ALTERNATIVO 2 (Dead-End Esquerdo com Baú de Suprimentos)
        CriarPlataforma("Chao_DeadEnd2", new Vector3(-13f, 0f, 145f), new Vector3(16f, 1f, 8f), matChao);
        CriarParede("ParedeDeadEnd2_Norte", new Vector3(-13f, 3.5f, 149f), new Vector3(16f, 6f, 1f), matParedes);
        CriarParede("ParedeDeadEnd2_Sul", new Vector3(-13f, 3.5f, 141f), new Vector3(16f, 6f, 1f), matParedes);
        CriarParede("ParedeDeadEnd2_Oeste", new Vector3(-21f, 3.5f, 145f), new Vector3(1f, 6f, 9f), matParedes);

        // Baú de Loot no Dead-End 2
        GameObject bau2 = new GameObject("BauDeLoot_2");
        bau2.transform.SetParent(containerMapa.transform);
        bau2.transform.position = new Vector3(-18f, 0.5f, 145f);
        bau2.AddComponent<BauDeLoot>();

        // SEÇÃO 3: Abordagem da Fortaleza Suspensa (Z: 160 a 210, X: -18 a 18)
        CriarPlataforma("Chao_Secao3", new Vector3(0f, 0f, 185f), new Vector3(36f, 1f, 50f), matChao);
        CriarParede("ParedeEsq_S3", new Vector3(-18f, 3.5f, 185f), new Vector3(1f, 6f, 50f), matParedes);
        CriarParede("ParedeDir_S3", new Vector3(18f, 3.5f, 185f), new Vector3(1f, 6f, 50f), matParedes);
    }

    void AdicionarSucatasECorrentes()
    {
        Material matFerro = MateriaisAlienigenas.CriarMaterialSucataOxidada();

        // 1. Correntes Colossais que ligam a masmorra flutuante ao céu
        CriarCorrenteColossal(new Vector3(-12f, 0.5f, 30f), new Vector3(-25f, 30f, 20f), matFerro);
        CriarCorrenteColossal(new Vector3(14f, 0.5f, 95f), new Vector3(30f, 35f, 110f), matFerro);
        CriarCorrenteColossal(new Vector3(-16f, 0.5f, 115f), new Vector3(-35f, 35f, 130f), matFerro);
        CriarCorrenteColossal(new Vector3(10f, 0.5f, 175f), new Vector3(25f, 30f, 190f), matFerro);

        // 2. Barricadas e Trincheiras de Ferro na Seção 1
        CriarBarricada(new Vector3(-5f, 1.2f, 18f), new Vector3(6f, 2.4f, 1.2f), 15f, matFerro);
        CriarBarricada(new Vector3(6f, 1.2f, 28f), new Vector3(7f, 2.4f, 1.2f), -20f, matFerro);

        // 3. Trincheiras na Seção 2
        CriarBarricada(new Vector3(-8f, 1.3f, 85f), new Vector3(12f, 2.6f, 1.5f), 10f, matFerro);
        CriarBarricada(new Vector3(8f, 1.3f, 105f), new Vector3(14f, 2.6f, 1.5f), -15f, matFerro);
        CriarBarricada(new Vector3(-6f, 1.3f, 120f), new Vector3(10f, 2.6f, 1.5f), 5f, matFerro);

        // 4. Barricadas na Seção 3 (Abordagem final)
        CriarBarricada(new Vector3(-6f, 1.4f, 172f), new Vector3(8f, 2.8f, 1.5f), 25f, matFerro);
        CriarBarricada(new Vector3(6f, 1.4f, 190f), new Vector3(9f, 2.8f, 1.5f), -20f, matFerro);
    }

    void ConstruirSafeRoom()
    {
        Material matAco = MateriaisAlienigenas.CriarMaterialSafeRoom();

        // Local da Safe Room: Z = 210 a 228, X = -8 a 8
        CriarPlataforma("Chao_SafeRoom", new Vector3(0f, 0f, 219f), new Vector3(16f, 1f, 18f), matAco);
        CriarParede("ParedeFundo_SafeRoom", new Vector3(0f, 3.5f, 228f), new Vector3(16f, 6f, 1f), matAco);
        CriarParede("ParedeEsq_SafeRoom", new Vector3(-8f, 3.5f, 219f), new Vector3(1f, 6f, 18f), matAco);
        CriarParede("ParedeDir_SafeRoom", new Vector3(8f, 3.5f, 219f), new Vector3(1f, 6f, 18f), matAco);

        // Teto da Safe Room para sensação de abrigo blindado
        GameObject teto = GameObject.CreatePrimitive(PrimitiveType.Cube);
        teto.name = "TetoSafeRoom";
        teto.transform.SetParent(containerMapa.transform, false);
        teto.transform.position = new Vector3(0f, 6.5f, 219f);
        teto.transform.localScale = new Vector3(16f, 0.8f, 18f);
        teto.GetComponent<Renderer>().material = matAco;

        // Porta Pesada de Metal com Automação
        GameObject portaObj = new GameObject("PortaSalaSegura");
        portaObj.transform.SetParent(containerMapa.transform, false);
        portaObj.transform.position = new Vector3(0f, 0.5f, 210f);
        portaObj.AddComponent<PortaSalaSegura>();
    }

    void BakeNavMesh()
    {
        // Adiciona NavMeshSurface no container do mapa
        navMeshSurface = containerMapa.GetComponent<NavMeshSurface>();
        if (navMeshSurface == null)
        {
            navMeshSurface = containerMapa.AddComponent<NavMeshSurface>();
        }

        navMeshSurface.collectObjects = CollectObjects.Children;
        navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        navMeshSurface.BuildNavMesh();
        Debug.Log("🧭 NAVMESH DA PRADARIA DA FERRUGEM CONSTRUÍDO!");
    }

    void LimparCenarioLegado()
    {
        // 1. Desativa o chão de teste cinza
        GameObject floorAntigo = GameObject.Find("Floor");
        if (floorAntigo != null) floorAntigo.SetActive(false);

        // 2. Desativa paredes antigas de teste (como o Cube que bloqueia a passagem) e bonecos de teste
        GameObject[] todos = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in todos)
        {
            if (obj == null) continue;
            string nome = obj.name;
            if (nome == "Floor" || nome == "Cube" || nome.StartsWith("Cube (") || (nome.StartsWith("Capsule") && obj.GetComponent<PlayerController>() == null && !obj.CompareTag("Player")))
            {
                obj.SetActive(false);
            }
        }
    }

    void SpawnarEntidades()
    {
        Vector3 posPlayer = new Vector3(0f, 1.5f, 6f);

        // 1. Reutiliza o jogador existente da cena ou cria um novo
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj == null)
        {
            PlayerController pcExistente = FindAnyObjectByType<PlayerController>();
            if (pcExistente != null) playerObj = pcExistente.gameObject;
        }

        if (playerObj != null)
        {
            CharacterController cc = playerObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerObj.transform.position = posPlayer;
            playerObj.transform.rotation = Quaternion.identity;
            if (cc != null) cc.enabled = true;

            playerObj.tag = "Player";
        }
        else
        {
            playerObj = new GameObject("HeroiJogador");
            playerObj.transform.position = posPlayer;
            playerObj.tag = "Player";

            CharacterController cc = playerObj.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1.0f, 0f);
            cc.radius = 0.45f;
            cc.height = 2.0f;

            playerObj.AddComponent<PlayerController>();
        }

        // Localiza ou cria a Câmera Principal
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            mainCam = playerObj.GetComponentInChildren<Camera>();
        }
        if (mainCam == null)
        {
            mainCam = FindAnyObjectByType<Camera>();
        }
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }

        // Remove componente de 3ª pessoa se existir na câmera
        CameraController3P cam3P = mainCam.GetComponent<CameraController3P>();
        if (cam3P != null) Destroy(cam3P);

        // Remove componente legado 3P do jogador se existir
        PlayerCombatD20 combat = playerObj.GetComponent<PlayerCombatD20>();
        if (combat != null) Destroy(combat);

        // Vincula a câmera diretamente à visão em 1ª Pessoa do Jogador
        mainCam.transform.SetParent(playerObj.transform);
        mainCam.transform.localPosition = new Vector3(0f, 0.65f, 0.2f);
        mainCam.transform.localRotation = Quaternion.identity;

        // Configura o PlayerController com a câmera em 1ª pessoa
        PlayerController pc = playerObj.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.playerCamera = mainCam.transform;
            pc.enabled = true;
        }

        // 2. Spawna Hordas de Inimigos ao longo das Seções
        // Seção 1: Cascas de Vidro iniciais
        SpawnarCascaDeVidro(new Vector3(-4f, 1.2f, 22f));
        SpawnarCascaDeVidro(new Vector3(5f, 1.2f, 25f));
        SpawnarCascaDeVidro(new Vector3(0f, 1.2f, 32f));

        // Gargalo 1: Emboscada estreita (Sucata de Correntes Voadoras)
        SpawnarSucataAnimada(new Vector3(0f, 1.5f, 52f), VarianteSucata.PequenaCorrentes);
        SpawnarCascaDeVidro(new Vector3(14f, 1.2f, 54f)); // Guarda do Baú 1

        // Seção 2: Horda Principal (Campo aberto com trincheiras)
        SpawnarCascaDeVidro(new Vector3(-9f, 1.2f, 82f));
        SpawnarCascaDeVidro(new Vector3(-6f, 1.2f, 90f));
        SpawnarCascaDeVidro(new Vector3(7f, 1.2f, 88f));
        SpawnarCascaDeVidro(new Vector3(11f, 1.2f, 96f));
        SpawnarSucataAnimada(new Vector3(-4f, 1.5f, 102f), VarianteSucata.MediaArmadura);
        SpawnarSucataAnimada(new Vector3(5f, 1.5f, 112f), VarianteSucata.PequenaCorrentes);
        SpawnarCascaDeVidro(new Vector3(0f, 1.2f, 118f));

        // Gargalo 2: Emboscada de Guarda
        SpawnarSucataAnimada(new Vector3(0f, 1.5f, 142f), VarianteSucata.MediaArmadura);
        SpawnarCascaDeVidro(new Vector3(-16f, 1.2f, 144f)); // Guarda do Baú 2

        // Seção 3: Cerco Final em frente à Safe Room
        SpawnarCascaDeVidro(new Vector3(-8f, 1.2f, 170f));
        SpawnarCascaDeVidro(new Vector3(8f, 1.2f, 174f));
        SpawnarSucataAnimada(new Vector3(-5f, 1.5f, 185f), VarianteSucata.MediaArmadura);
        SpawnarSucataAnimada(new Vector3(5f, 1.5f, 188f), VarianteSucata.MediaArmadura);
        SpawnarCascaDeVidro(new Vector3(0f, 1.2f, 198f));
    }

    void SpawnarCascaDeVidro(Vector3 pos)
    {
        GameObject zumbi = new GameObject("Inimigo_CascaDeVidro");
        zumbi.transform.position = pos;
        zumbi.AddComponent<InimigoCascaDeVidro>();
    }

    void SpawnarSucataAnimada(Vector3 pos, VarianteSucata variante)
    {
        GameObject sucata = new GameObject("Inimigo_SucataAnimada_" + variante.ToString());
        sucata.transform.position = pos;
        InimigoSucataAnimada isa = sucata.AddComponent<InimigoSucataAnimada>();
        isa.variante = variante;
    }

    void CriarPlataforma(string nome, Vector3 pos, Vector3 escala, Material mat)
    {
        GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plat.name = nome;
        plat.transform.SetParent(containerMapa.transform, false);
        plat.transform.position = pos;
        plat.transform.localScale = escala;
        plat.GetComponent<Renderer>().material = mat;
    }

    void CriarParede(string nome, Vector3 pos, Vector3 escala, Material mat)
    {
        GameObject parede = GameObject.CreatePrimitive(PrimitiveType.Cube);
        parede.name = nome;
        parede.transform.SetParent(containerMapa.transform, false);
        parede.transform.position = pos;
        parede.transform.localScale = escala;
        parede.GetComponent<Renderer>().material = mat;
    }

    void CriarBarricada(Vector3 pos, Vector3 escala, float rotacaoY, Material mat)
    {
        GameObject barr = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barr.name = "BarricadaSucata";
        barr.transform.SetParent(containerMapa.transform, false);
        barr.transform.position = pos;
        barr.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), rotacaoY, Random.Range(-5f, 5f));
        barr.transform.localScale = escala;
        barr.GetComponent<Renderer>().material = mat;
    }

    void CriarCorrenteColossal(Vector3 inicio, Vector3 fim, Material mat)
    {
        GameObject corrente = new GameObject("CorrenteColossal");
        corrente.transform.SetParent(containerMapa.transform, false);

        int elos = 14;
        for (int i = 0; i < elos; i++)
        {
            float t = (float)i / (elos - 1);
            Vector3 posElo = Vector3.Lerp(inicio, fim, t);
            // Curvatura gravitacional
            posElo.y -= Mathf.Sin(t * Mathf.PI) * 3.5f;

            GameObject elo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            elo.transform.SetParent(corrente.transform, false);
            elo.transform.position = posElo;
            elo.transform.localScale = new Vector3(1.2f, 1.4f, 1.2f);
            elo.transform.rotation = Quaternion.Euler(t * 180f, 45f, (i % 2 == 0) ? 0f : 90f);
            Destroy(elo.GetComponent<Collider>());
            elo.GetComponent<Renderer>().material = mat;
        }
    }
}
