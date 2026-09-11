using System.Collections;
using UnityEngine;

public class BauDeLoot : MonoBehaviour
{
    [Header("Configurações do Loot")]
    public float curaFornecida = 6f; // Recupera ~50% da vida máxima do jogador nível 1
    public float distanciaInteracao = 3.0f;

    private bool foiAberto = false;
    private GameObject tampaBau;
    private GameObject luzLoot;

    void Start()
    {
        ConstruirBauVisual();
    }

    void Update()
    {
        if (foiAberto) return;

        Transform playerTransform = null;
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            playerTransform = pc.transform;
        }
        else
        {
            PlayerCombatD20 playerCombat = FindAnyObjectByType<PlayerCombatD20>();
            if (playerCombat != null) playerTransform = playerCombat.transform;
        }

        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= distanciaInteracao)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    AbrirBau(playerTransform.gameObject);
                }
            }
        }
    }

    public void AbrirBau(GameObject playerObj)
    {
        if (foiAberto) return;
        foiAberto = true;

        // Animação de abrir tampa
        if (tampaBau != null)
        {
            tampaBau.transform.localRotation = Quaternion.Euler(-75f, 0f, 0f);
        }

        if (luzLoot != null)
        {
            Destroy(luzLoot);
        }

        // Cura o jogador
        if (playerObj != null)
        {
            StatusJogador status = playerObj.GetComponent<StatusJogador>();
            if (status != null) status.Curar(curaFornecida * 10f);

            SistemaDeVida sVida = playerObj.GetComponent<SistemaDeVida>();
            if (sVida != null) sVida.Curar(curaFornecida * 10f);

            PlayerCombatD20 pD20 = playerObj.GetComponent<PlayerCombatD20>();
            if (pD20 != null) pD20.Curar(curaFornecida);
        }

        Debug.Log("🎁 BAÚ DE SUPRIMENTOS ABERTO! Poções de vida alienígenas consumidas!");
    }

    public void AbrirBau(PlayerCombatD20 player)
    {
        if (player != null) AbrirBau(player.gameObject);
    }

    void ConstruirBauVisual()
    {
        Material matFerro = MateriaisAlienigenas.CriarMaterialSucataOxidada();
        Material matOuro = MateriaisAlienigenas.CriarMaterialCristalEstatico();

        // Base do Baú
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObj.name = "BaseBau";
        baseObj.transform.SetParent(transform, false);
        baseObj.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        baseObj.transform.localScale = new Vector3(1.1f, 0.7f, 0.75f);
        baseObj.GetComponent<Renderer>().material = matFerro;

        // Tampa Articulada
        tampaBau = new GameObject("PivoTampa");
        tampaBau.transform.SetParent(transform, false);
        tampaBau.transform.localPosition = new Vector3(0f, 0.7f, -0.35f);

        GameObject tampaMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tampaMesh.name = "TampaMesh";
        tampaMesh.transform.SetParent(tampaBau.transform, false);
        tampaMesh.transform.localPosition = new Vector3(0f, 0.12f, 0.35f);
        tampaMesh.transform.localScale = new Vector3(1.15f, 0.25f, 0.8f);
        tampaMesh.GetComponent<Renderer>().material = matFerro;
        Destroy(tampaMesh.GetComponent<Collider>());

        // Fechadura / Cristal Energético
        GameObject fecho = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fecho.name = "FechoEnergia";
        fecho.transform.SetParent(tampaMesh.transform, false);
        fecho.transform.localPosition = new Vector3(0f, 0f, 0.42f);
        fecho.transform.localScale = Vector3.one * 0.2f;
        fecho.GetComponent<Renderer>().material = matOuro;
        Destroy(fecho.GetComponent<Collider>());

        // Luz pontual indicando loot
        luzLoot = new GameObject("LuzLoot");
        luzLoot.transform.SetParent(transform, false);
        luzLoot.transform.localPosition = new Vector3(0f, 1.0f, 0f);
        Light l = luzLoot.AddComponent<Light>();
        l.color = new Color(0.2f, 0.8f, 1.0f);
        l.range = 5f;
        l.intensity = 2.5f;
    }
}
