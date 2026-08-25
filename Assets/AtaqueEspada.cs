using System.Collections;
using UnityEngine;

public class AtaqueEspada : MonoBehaviour
{
    [Header("Configurações do Golpe (Espada Longa)")]
    public float tempoAtaque = 0.4f;
    public float anguloCorte = 75f;
    public float tempoEntreAtaques = 0.9f;
    private float proximoAtaquePermitido = 0f;

    [Header("Dano, Alcance e Impacto")]
    public float danoDoAtaque = 35f;
    public float alcance = 3.5f;
    public float forcaKnockback = 15f;
    public Camera cameraJogador;

    [Header("Recursos (Bárbaro)")]
    public float furiaPorAcerto = 20f;
    public StatusJogador statusJogador; // Referência ao script do jogador para encher a barra

    private bool atacando = false;
    private Quaternion rotacaoInicial;
    private Quaternion rotacaoAtaque;

    void Start()
    {
        rotacaoInicial = transform.localRotation;
        rotacaoAtaque = transform.localRotation * Quaternion.Euler(anguloCorte, 0f, 0f);
    }

    void Update()
    {
        // Só ataca se não estiver no meio de um golpe E se a cadência (cooldown) já zerou
        if (Input.GetButtonDown("Fire1") && !atacando && Time.time >= proximoAtaquePermitido)
        {
            proximoAtaquePermitido = Time.time + tempoEntreAtaques;

            TentarAcertarAlvo();
            StartCoroutine(RealizarGolpe());
        }
    }

    void TentarAcertarAlvo()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraJogador.transform.position, cameraJogador.transform.forward, out hit, alcance))
        {
            SistemaDeVida alvo = hit.transform.GetComponent<SistemaDeVida>();

            if (alvo != null)
            {
                // Causa dano ao inimigo
                alvo.ReceberDano(danoDoAtaque);

                // --- SISTEMA DE FÚRIA (NOVO) ---
                // Se o jogador estiver conectado no Inspector, ganha Fúria!
                if (statusJogador != null)
                {
                    statusJogador.GanharFuria(furiaPorAcerto);
                }

                // --- SISTEMA DE KNOCKBACK CORRIGIDO ---
                Rigidbody rbInimigo = hit.transform.GetComponent<Rigidbody>();
                if (rbInimigo != null)
                {
                    // Calcula o vetor de direção (da espada para o inimigo)
                    Vector3 direcaoEmpurrao = hit.transform.position - transform.position;

                    // Adiciona um leve solavanco para cima no vetor Y
                    direcaoEmpurrao.y = 0.5f;
                    direcaoEmpurrao.Normalize();

                    // Aplica o impacto contínuo e mais suave
                    rbInimigo.AddForce(direcaoEmpurrao * forcaKnockback, ForceMode.Force);
                }
            }
        }
    }

    IEnumerator RealizarGolpe()
    {
        atacando = true;
        float progresso = 0f;

        // O golpe descendo 
        while (progresso < 1f)
        {
            progresso += Time.deltaTime / (tempoAtaque / 2);
            transform.localRotation = Quaternion.Lerp(rotacaoInicial, rotacaoAtaque, progresso);
            yield return null;
        }

        progresso = 0f;

        // A arma subindo 
        while (progresso < 1f)
        {
            progresso += Time.deltaTime / (tempoAtaque / 2);
            transform.localRotation = Quaternion.Lerp(rotacaoAtaque, rotacaoInicial, progresso);
            yield return null;
        }

        transform.localRotation = rotacaoInicial;
        atacando = false;
    }
}