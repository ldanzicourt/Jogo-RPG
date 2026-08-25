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

    [Header("Recursos (Guerreiro)")]
    public float furiaPorAcerto = 20f;
    public StatusJogador statusJogador; // Referência ao script do jogador para gerenciar a Fúria

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
                // Verifica se o jogador está em fúria máxima para dobrar o dano (Modo Berserker)
                bool emFuria = (statusJogador != null && statusJogador.EstaEmFuriaMaxima());
                float danoFinal = emFuria ? (danoDoAtaque * 2f) : danoDoAtaque;

                // Causa dano ao inimigo (passando o flag de golpe de fúria para colorir o texto)
                alvo.ReceberDano(danoFinal, emFuria);

                // --- SISTEMA DE FÚRIA ---
                // Adiciona fúria ao jogador por acerto e reinicia a contagem de 5s
                if (statusJogador != null)
                {
                    statusJogador.GanharFuria(furiaPorAcerto);
                }

                // --- SISTEMA DE KNOCKBACK ---
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
