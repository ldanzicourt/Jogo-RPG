using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class InimigoCascaDeVidro : EnemyAIBase
{
    [Header("Fortitude de Morto-Vivo")]
    public int modificadorConstituicao = 3;
    public int chanceAtivacaoFortitude = 50; // 50% de chance

    private GameObject modeloCorpoVidro;

    protected override void Awake()
    {
        classeArmadura = 8;  // CA muito baixa
        pvMaximo = 22f;      // Vida alta (22 PV)
        pvAtual = pvMaximo;
        danoMedioAoJogador = 2.4f; // ~20% do jogador
        alcanceAtaque = 1.9f;
        intervaloAtaques = 2.0f; // Ataques lentos

        base.Awake();

        if (agent != null)
        {
            agent.speed = 2.5f; // Movimentação lenta
            agent.acceleration = 6f;
            agent.stoppingDistance = alcanceAtaque * 0.8f;
        }
    }

    protected override void ConstruirModeloVisual()
    {
        // Constrói o corpo humanoide cristalizado em vidro temperado escuro
        modeloCorpoVidro = new GameObject("Visual_CascaDeVidro");
        modeloCorpoVidro.transform.SetParent(transform, false);

        Material matVidro = MateriaisAlienigenas.CriarMaterialVidroEscuro();

        // Tronco de vidro
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "TorsoVidro";
        torso.transform.SetParent(modeloCorpoVidro.transform, false);
        torso.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        torso.transform.localScale = new Vector3(0.55f, 0.85f, 0.35f);
        Destroy(torso.GetComponent<Collider>());
        torso.GetComponent<Renderer>().material = matVidro;

        // Cabeça com arestas afiadas de cristal
        GameObject cabeca = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cabeca.name = "CabecaVidro";
        cabeca.transform.SetParent(modeloCorpoVidro.transform, false);
        cabeca.transform.localPosition = new Vector3(0f, 1.75f, 0f);
        cabeca.transform.localScale = new Vector3(0.42f, 0.48f, 0.42f);
        Destroy(cabeca.GetComponent<Collider>());
        cabeca.GetComponent<Renderer>().material = matVidro;

        // Braço Direito (Garras de Vidro Estilhaçado)
        GameObject bracoDir = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bracoDir.name = "GarraVidroDir";
        bracoDir.transform.SetParent(modeloCorpoVidro.transform, false);
        bracoDir.transform.localPosition = new Vector3(0.42f, 1.15f, 0.35f);
        bracoDir.transform.localRotation = Quaternion.Euler(60f, 0f, 15f);
        bracoDir.transform.localScale = new Vector3(0.14f, 0.6f, 0.14f);
        Destroy(bracoDir.GetComponent<Collider>());
        bracoDir.GetComponent<Renderer>().material = matVidro;

        // Braço Esquerdo
        GameObject bracoEsq = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bracoEsq.name = "GarraVidroEsq";
        bracoEsq.transform.SetParent(modeloCorpoVidro.transform, false);
        bracoEsq.transform.localPosition = new Vector3(-0.42f, 1.15f, 0.25f);
        bracoEsq.transform.localRotation = Quaternion.Euler(45f, 0f, -15f);
        bracoEsq.transform.localScale = new Vector3(0.14f, 0.55f, 0.14f);
        Destroy(bracoEsq.GetComponent<Collider>());
        bracoEsq.GetComponent<Renderer>().material = matVidro;

        // Estilhaços proeminentes nas costas (efeito do feixe sonoro alienígena)
        for (int i = 0; i < 4; i++)
        {
            GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shard.name = "CacoCostas_" + i;
            shard.transform.SetParent(modeloCorpoVidro.transform, false);
            shard.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), 1.2f + i * 0.12f, -0.22f);
            shard.transform.localRotation = Quaternion.Euler(Random.Range(30f, 60f), Random.Range(-25f, 25f), 0f);
            shard.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
            Destroy(shard.GetComponent<Collider>());
            shard.GetComponent<Renderer>().material = matVidro;
        }

        // Colisor para o inimigo
        CapsuleCollider col = gameObject.GetComponent<CapsuleCollider>();
        if (col == null) col = gameObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 1.0f, 0f);
        col.radius = 0.5f;
        col.height = 2.0f;
    }

    protected override void ExecutarAtaque()
    {
        StartCoroutine(RotinaAtaqueGarraVidro());
    }

    IEnumerator RotinaAtaqueGarraVidro()
    {
        // Animação procedural: inclina para trás e golpeia para frente
        if (modeloCorpoVidro != null)
        {
            modeloCorpoVidro.transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);
        }

        yield return new WaitForSeconds(0.45f);

        if (modeloCorpoVidro != null)
        {
            modeloCorpoVidro.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
        }

        // Aplica o dano no alvo se ainda estiver no alcance
        if (alvoAtual != null && Vector3.Distance(transform.position, alvoAtual.position) <= alcanceAtaque * 1.35f)
        {
            StatusJogador status = alvoAtual.GetComponent<StatusJogador>();
            if (status != null)
            {
                status.ReceberDano(danoMedioAoJogador * 6f);
                CriarEfeitoEstilhaco(alvoAtual.position + Vector3.up * 1.1f);
            }
            else
            {
                PlayerCombatD20 player = alvoAtual.GetComponent<PlayerCombatD20>();
                if (player != null)
                {
                    player.ReceberDanoDoInimigo(danoMedioAoJogador, this);
                    CriarEfeitoEstilhaco(alvoAtual.position + Vector3.up * 1.1f);
                }
                else
                {
                    SistemaDeVida sVida = alvoAtual.GetComponent<SistemaDeVida>();
                    if (sVida != null)
                    {
                        sVida.ReceberDano(danoMedioAoJogador, new Color(1f, 0.2f, 0.2f));
                        CriarEfeitoEstilhaco(alvoAtual.position + Vector3.up * 1.1f);
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.35f);

        if (modeloCorpoVidro != null)
        {
            modeloCorpoVidro.transform.localRotation = Quaternion.identity;
        }
    }

    // Habilidade Especial D&D: Fortitude de Morto-Vivo
    protected override void ChecarMorteOuSobrevivencia(float danoSofrido)
    {
        // 50% de chance de ativar a rolagem de Constituição
        int chance = Random.Range(1, 101);
        if (chance <= chanceAtivacaoFortitude)
        {
            int d20Salvaguarda = Random.Range(1, 21);
            int testeConstituicao = d20Salvaguarda + modificadorConstituicao;
            float cd = 5f + danoSofrido;

            if (testeConstituicao >= cd || d20Salvaguarda == 20)
            {
                // Sobreviveu com 1 PV!
                pvAtual = 1f;
                Debug.Log($"💀🛡️ FORTITUDE DE MORTO-VIVO ATIVADA! A Casca de Vidro resistiu ao estilhaçamento fatal e voltou com 1 PV! (Teste: {testeConstituicao} vs CD: {cd})");

                CriarTextoFlutuante(0, new Color(0.3f, 0.9f, 1f), false); // Brilho de ressurreição
                CriarEfeitoRessonanciaSonica();
                return;
            }
        }

        // Caso contrário, morre estilhaçado
        Morrer();
    }

    void CriarEfeitoRessonanciaSonica()
    {
        GameObject onda = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        onda.name = "OndaRessonancia";
        onda.transform.position = transform.position + Vector3.up * 1.0f;
        onda.transform.localScale = Vector3.one * 1.8f;
        Destroy(onda.GetComponent<Collider>());

        Renderer r = onda.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = new Color(0.2f, 0.8f, 1.0f, 0.4f);
            r.material = mat;
        }
        Destroy(onda, 0.35f);
    }

    void CriarEfeitoEstilhaco(Vector3 pos)
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject caco = GameObject.CreatePrimitive(PrimitiveType.Cube);
            caco.transform.position = pos + Random.insideUnitSphere * 0.25f;
            caco.transform.localScale = Vector3.one * 0.08f;
            caco.transform.rotation = Random.rotation;
            Destroy(caco.GetComponent<Collider>());

            Renderer r = caco.GetComponent<Renderer>();
            if (r != null) r.material = MateriaisAlienigenas.CriarMaterialVidroEscuro();
            Destroy(caco, 0.4f);
        }
    }

    protected override IEnumerator RotinaMorteVisual()
    {
        // Explosão de estilhaços de vidro negro
        CriarEfeitoEstilhaco(transform.position + Vector3.up * 1.0f);
        CriarEfeitoEstilhaco(transform.position + Vector3.up * 1.6f);

        float t = 0f;
        Vector3 escalaInicial = modeloCorpoVidro != null ? modeloCorpoVidro.transform.localScale : Vector3.one;

        while (t < 0.6f)
        {
            t += Time.deltaTime;
            if (modeloCorpoVidro != null)
            {
                modeloCorpoVidro.transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, t / 0.6f);
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
