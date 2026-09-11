using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EstadoInimigo
{
    Idle,
    Chase,
    Attack,
    Die
}

public abstract class EnemyAIBase : MonoBehaviour
{
    [Header("Atributos D&D")]
    public int classeArmadura = 10;
    public float pvMaximo = 20f;
    public float pvAtual = 20f;
    public float danoMedioAoJogador = 2.2f; // ~15% a 20% da vida total do jogador (12 PV)

    [Header("Detecção e Combate")]
    public float raioPercepcao = 25f;
    public float alcanceAtaque = 2.2f;
    public float intervaloAtaques = 1.6f;

    [Header("Estado Atual")]
    public EstadoInimigo estadoAtual = EstadoInimigo.Idle;

    protected NavMeshAgent agent;
    protected Transform alvoAtual;
    protected float proximoAtaquePermitido = 0f;
    protected bool estaMorto = false;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        pvAtual = pvMaximo;
        tag = "Enemy";

        SistemaDeVida vida = GetComponent<SistemaDeVida>();
        if (vida == null) vida = gameObject.AddComponent<SistemaDeVida>();
        vida.vidaMaxima = pvMaximo;
        vida.vidaAtual = pvAtual;
    }

    protected virtual void Start()
    {
        ConstruirModeloVisual();
    }

    protected virtual void Update()
    {
        if (estaMorto || Time.timeScale <= 0.001f) return;

        AtualizarAlvoMaisProximo();

        switch (estadoAtual)
        {
            case EstadoInimigo.Idle:
                if (alvoAtual != null)
                {
                    estadoAtual = EstadoInimigo.Chase;
                }
                break;

            case EstadoInimigo.Chase:
                if (alvoAtual == null)
                {
                    estadoAtual = EstadoInimigo.Idle;
                    if (agent.isOnNavMesh) agent.isStopped = true;
                    return;
                }

                float distanciaAlvo = Vector3.Distance(transform.position, alvoAtual.position);

                if (distanciaAlvo <= alcanceAtaque)
                {
                    estadoAtual = EstadoInimigo.Attack;
                    if (agent.isOnNavMesh) agent.isStopped = true;
                }
                else
                {
                    if (agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(alvoAtual.position);
                    }
                }
                break;

            case EstadoInimigo.Attack:
                if (alvoAtual == null)
                {
                    estadoAtual = EstadoInimigo.Idle;
                    return;
                }

                // Rotaciona em direção ao alvo
                Vector3 direcaoAoAlvo = (alvoAtual.position - transform.position);
                direcaoAoAlvo.y = 0f;
                if (direcaoAoAlvo.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direcaoAoAlvo), Time.deltaTime * 8f);
                }

                float dist = Vector3.Distance(transform.position, alvoAtual.position);
                if (dist > alcanceAtaque * 1.35f)
                {
                    estadoAtual = EstadoInimigo.Chase;
                    if (agent.isOnNavMesh) agent.isStopped = false;
                }
                else if (Time.time >= proximoAtaquePermitido)
                {
                    proximoAtaquePermitido = Time.time + intervaloAtaques;
                    ExecutarAtaque();
                }
                break;

            case EstadoInimigo.Die:
                break;
        }
    }

    protected void AtualizarAlvoMaisProximo()
    {
        PlayerController jogador = FindAnyObjectByType<PlayerController>();
        if (jogador != null)
        {
            float dist = Vector3.Distance(transform.position, jogador.transform.position);
            alvoAtual = (dist <= raioPercepcao) ? jogador.transform : null;
        }
        else
        {
            PlayerCombatD20 pD20 = FindAnyObjectByType<PlayerCombatD20>();
            if (pD20 != null && pD20.pvAtual > 0)
            {
                float dist = Vector3.Distance(transform.position, pD20.transform.position);
                alvoAtual = (dist <= raioPercepcao) ? pD20.transform : null;
            }
            else
            {
                alvoAtual = null;
            }
        }
    }

    public virtual void ReceberDano(float dano, Color corTexto, bool ehCritico = false)
    {
        if (estaMorto) return;

        pvAtual -= dano;
        CriarTextoFlutuante(dano, corTexto, ehCritico);

        if (pvAtual <= 0f)
        {
            pvAtual = 0f;
            ChecarMorteOuSobrevivencia(dano);
        }
        else
        {
            // Reação a dano: foca no atacante
            PlayerCombatD20 player = FindAnyObjectByType<PlayerCombatD20>();
            if (player != null) alvoAtual = player.transform;
            if (estadoAtual == EstadoInimigo.Idle) estadoAtual = EstadoInimigo.Chase;
        }
    }

    protected virtual void ChecarMorteOuSobrevivencia(float danoSofrido)
    {
        Morrer();
    }

    protected virtual void Morrer()
    {
        estaMorto = true;
        estadoAtual = EstadoInimigo.Die;
        if (agent.isOnNavMesh) agent.isStopped = true;

        // Desativa colisores
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = false;

        StartCoroutine(RotinaMorteVisual());
    }

    protected abstract void ExecutarAtaque();
    protected abstract void ConstruirModeloVisual();
    protected abstract IEnumerator RotinaMorteVisual();

    protected void CriarTextoFlutuante(float dano, Color cor, bool critico)
    {
        GameObject txtObj = new GameObject("TextoDano");
        txtObj.transform.position = transform.position + Vector3.up * 2.0f + Random.insideUnitSphere * 0.2f;
        TextoFlutuante tf = txtObj.AddComponent<TextoFlutuante>();
        string texto = dano <= 0f ? "FORTITUDE!" : ((critico ? "CRÍTICO! " : "") + dano.ToString("F0"));
        tf.DefinirTexto(texto, cor);
    }

    public bool EstaVivo() => !estaMorto && pvAtual > 0f;
}
