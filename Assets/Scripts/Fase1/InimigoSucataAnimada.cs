using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum VarianteSucata
{
    PequenaCorrentes, // CA 16, 25 PV, ataque rápido 1d8+2
    MediaArmadura     // CA 13, 40 PV, impacto pesado 2d6+1
}

public class InimigoSucataAnimada : EnemyAIBase
{
    [Header("Variante")]
    public VarianteSucata variante = VarianteSucata.MediaArmadura;

    [Header("Flutuação Alienígena")]
    public float alturaFlutuacao = 1.3f;
    public float velocidadeOscilacao = 3.5f;
    public float amplitudeOscilacao = 0.25f;

    private GameObject modeloVisualSucata;
    private float offsetTempo;

    protected override void Awake()
    {
        offsetTempo = Random.Range(0f, 10f);

        if (variante == VarianteSucata.PequenaCorrentes)
        {
            classeArmadura = 16;       // CA alta (correntes velozes)
            pvMaximo = 25f;            // PV moderado
            danoMedioAoJogador = 2.0f; // ~15% do jogador
            alcanceAtaque = 2.4f;
            intervaloAtaques = 1.25f;   // Ataques mais rápidos
        }
        else
        {
            classeArmadura = 13;       // CA média
            pvMaximo = 40f;            // PV alto (armadura pesada)
            danoMedioAoJogador = 2.5f; // ~20% do jogador
            alcanceAtaque = 2.8f;
            intervaloAtaques = 1.8f;   // Slam mais pesado
        }

        pvAtual = pvMaximo;
        base.Awake();

        if (agent != null)
        {
            agent.speed = 4.5f; // Movimentação flutuante rápida
            agent.acceleration = 12f;
            agent.baseOffset = alturaFlutuacao; // Flutua na altura do tronco do jogador
            agent.stoppingDistance = alcanceAtaque * 0.75f;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (estaMorto) return;

        // Oscilação de flutuação no ar
        if (modeloVisualSucata != null)
        {
            float yHover = Mathf.Sin((Time.time + offsetTempo) * velocidadeOscilacao) * amplitudeOscilacao;
            modeloVisualSucata.transform.localPosition = new Vector3(0f, yHover, 0f);
            modeloVisualSucata.transform.Rotate(Vector3.up * 45f * Time.deltaTime);
        }
    }

    protected override void ConstruirModeloVisual()
    {
        modeloVisualSucata = new GameObject("Visual_SucataAnimada");
        modeloVisualSucata.transform.SetParent(transform, false);

        Material matFerro = MateriaisAlienigenas.CriarMaterialSucataOxidada();
        Material matCristal = MateriaisAlienigenas.CriarMaterialCristalEstatico();

        if (variante == VarianteSucata.PequenaCorrentes)
        {
            // Núcleo energético estático
            GameObject nucleo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nucleo.transform.SetParent(modeloVisualSucata.transform, false);
            nucleo.transform.localPosition = Vector3.zero;
            nucleo.transform.localScale = Vector3.one * 0.35f;
            Destroy(nucleo.GetComponent<Collider>());
            nucleo.GetComponent<Renderer>().material = matCristal;

            // Anéis e elos de correntes flutuantes
            for (int i = 0; i < 5; i++)
            {
                GameObject elo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                elo.name = "EloCorrente_" + i;
                elo.transform.SetParent(modeloVisualSucata.transform, false);
                float angulo = i * 72f * Mathf.Deg2Rad;
                elo.transform.localPosition = new Vector3(Mathf.Cos(angulo) * 0.5f, (i - 2) * 0.15f, Mathf.Sin(angulo) * 0.5f);
                elo.transform.localRotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), 0f);
                elo.transform.localScale = new Vector3(0.12f, 0.4f, 0.12f);
                Destroy(elo.GetComponent<Collider>());
                elo.GetComponent<Renderer>().material = matFerro;
            }
        }
        else
        {
            // Armadura Média: Placas peitorais enferrujadas e espinhos
            GameObject peitoral = GameObject.CreatePrimitive(PrimitiveType.Cube);
            peitoral.transform.SetParent(modeloVisualSucata.transform, false);
            peitoral.transform.localPosition = Vector3.zero;
            peitoral.transform.localScale = new Vector3(0.75f, 0.9f, 0.5f);
            Destroy(peitoral.GetComponent<Collider>());
            peitoral.GetComponent<Renderer>().material = matFerro;

            // Núcleo estático pulsante entre as frestas da armadura
            GameObject energia = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            energia.transform.SetParent(modeloVisualSucata.transform, false);
            energia.transform.localPosition = new Vector3(0f, 0f, 0.15f);
            energia.transform.localScale = Vector3.one * 0.25f;
            Destroy(energia.GetComponent<Collider>());
            energia.GetComponent<Renderer>().material = matCristal;

            // Ombreiras pesadas
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject ombreira = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ombreira.transform.SetParent(modeloVisualSucata.transform, false);
                ombreira.transform.localPosition = new Vector3(s * 0.55f, 0.35f, 0f);
                ombreira.transform.localRotation = Quaternion.Euler(0f, 0f, s * -25f);
                ombreira.transform.localScale = new Vector3(0.45f, 0.25f, 0.55f);
                Destroy(ombreira.GetComponent<Collider>());
                ombreira.GetComponent<Renderer>().material = matFerro;
            }
        }

        // Colisor esférico para acomodar o objeto flutuante
        SphereCollider col = gameObject.GetComponent<SphereCollider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.center = new Vector3(0f, alturaFlutuacao, 0f);
        col.radius = 0.75f;
    }

    protected override void ExecutarAtaque()
    {
        StartCoroutine(RotinaAtaqueSlam());
    }

    IEnumerator RotinaAtaqueSlam()
    {
        // 1. Recuo rápido de antecipação
        if (modeloVisualSucata != null)
        {
            modeloVisualSucata.transform.localPosition += -transform.forward * 0.4f + Vector3.up * 0.3f;
        }

        yield return new WaitForSeconds(0.22f);

        // 2. Investida violenta (Slam) para frente
        if (modeloVisualSucata != null)
        {
            modeloVisualSucata.transform.localPosition += transform.forward * 0.9f;
        }

        CriarOndaImpactoSlam();

        // 3. Aplica dano ao alvo
        if (alvoAtual != null && Vector3.Distance(transform.position, alvoAtual.position) <= alcanceAtaque * 1.4f)
        {
            StatusJogador status = alvoAtual.GetComponent<StatusJogador>();
            if (status != null)
            {
                status.ReceberDano(danoMedioAoJogador * 8f);
            }
            else
            {
                PlayerCombatD20 player = alvoAtual.GetComponent<PlayerCombatD20>();
                if (player != null)
                {
                    player.ReceberDanoDoInimigo(danoMedioAoJogador, this);
                }
                else
                {
                    SistemaDeVida sVida = alvoAtual.GetComponent<SistemaDeVida>();
                    if (sVida != null) sVida.ReceberDano(danoMedioAoJogador, new Color(1f, 0.2f, 0.2f));
                }
            }
        }

        yield return new WaitForSeconds(0.35f);

        if (modeloVisualSucata != null)
        {
            modeloVisualSucata.transform.localPosition = Vector3.zero;
        }
    }

    void CriarOndaImpactoSlam()
    {
        GameObject slamVfx = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        slamVfx.transform.position = transform.position + transform.forward * 1.2f + Vector3.up * 0.8f;
        slamVfx.transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        slamVfx.transform.localScale = new Vector3(1.2f, 0.05f, 1.2f);
        Destroy(slamVfx.GetComponent<Collider>());

        Renderer r = slamVfx.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = new Color(1f, 0.55f, 0.2f, 0.5f);
            r.material = mat;
        }
        Destroy(slamVfx, 0.18f);
    }

    protected override IEnumerator RotinaMorteVisual()
    {
        // Peças de metal despencam e se desintegram
        for (int i = 0; i < 4; i++)
        {
            GameObject sucata = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sucata.transform.position = transform.position + Vector3.up * alturaFlutuacao + Random.insideUnitSphere * 0.3f;
            sucata.transform.localScale = Vector3.one * 0.18f;
            Destroy(sucata.GetComponent<Collider>());
            Rigidbody rb = sucata.AddComponent<Rigidbody>();
            rb.AddExplosionForce(8f, transform.position, 2f);
            Destroy(sucata, 0.8f);
        }

        if (modeloVisualSucata != null) Destroy(modeloVisualSucata);
        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }
}
