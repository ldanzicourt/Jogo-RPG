using UnityEngine;
using UnityEngine.UI;

public class StatusJogador : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;
    private float vidaAtual;
    public Image imagemBarraVida;

    [Header("Fúria")]
    public float furiaMaxima = 100f;
    private float furiaAtual = 0f;
    public Image imagemBarraFuria;

    [Header("Decaimento de Fúria")]
    public float tempoParaComecarPerder = 5f; // 5 segundos sem bater antes de perder Fúria
    public float perdaDeFuriaPorSegundo = 20f; // Esvazia a barra gradualmente (~5s para esvaziar 100%)
    private float tempoUltimoAcerto;
    private bool furiaCongelada = false;

    void Start()
    {
        vidaAtual = vidaMaxima;
        furiaAtual = 0f; // Guerreiro começa com a barra vazia
        AtualizarBarras();
    }

    void Update()
    {
        // Se houver Fúria E não estiver congelada E já passaram 5 segundos sem acertar golpes...
        if (!furiaCongelada && furiaAtual > 0 && Time.time > tempoUltimoAcerto + tempoParaComecarPerder)
        {
            // Esvazia gradualmente
            furiaAtual -= perdaDeFuriaPorSegundo * Time.deltaTime;

            if (furiaAtual < 0) furiaAtual = 0;

            AtualizarBarras();
        }
    }

    public void CongelarFuria(bool congelar)
    {
        furiaCongelada = congelar;
        if (congelar)
        {
            furiaAtual = furiaMaxima;
            AtualizarBarras();
        }
    }

    public void ConsumirTodaFuria()
    {
        furiaAtual = 0f;
        furiaCongelada = false;
        AtualizarBarras();
    }

    public void GanharFuria(float quantidade)
    {
        if (furiaCongelada) return;

        furiaAtual += quantidade;

        // Limita ao máximo permitido
        if (furiaAtual > furiaMaxima) furiaAtual = furiaMaxima;

        // Reinicia o cronômetro para garantir os 5 segundos sem decaimento a partir deste acerto
        tempoUltimoAcerto = Time.time;

        AtualizarBarras();
    }

    public bool EstaEmFuriaMaxima()
    {
        return furiaAtual >= furiaMaxima;
    }

    public float ObterFuriaAtual()
    {
        return furiaAtual;
    }

    public float ObterPorcentagemFuria()
    {
        return furiaAtual / furiaMaxima;
    }

    public void Curar(float quantidade)
    {
        vidaAtual = Mathf.Min(vidaMaxima, vidaAtual + quantidade);
        AtualizarBarras();
    }

    public void ReceberDano(float quantidade)
    {
        // 1. Verifica se a Barreira de Estudo do Mago está ativa
        ControleMago mago = GetComponentInChildren<ControleMago>();
        if (mago == null) mago = GetComponent<ControleMago>();

        if (mago != null && mago.EstaComBarreiraAtiva())
        {
            if (mago.ChecarBloqueioPerfeito())
            {
                // Bloqueio Perfeito: 0 de dano e reduz cooldowns!
                mago.ExecutarBloqueioPerfeito();
                return;
            }
            else
            {
                // Bloqueio Normal: 50% de redução
                quantidade *= 0.5f;
            }
        }

        // 2. Verifica as defesas e posturas do Guerreiro (Guarda Bruta / Postura Berserker / Exaustão)
        AtaqueEspada guerreiro = GetComponentInChildren<AtaqueEspada>();
        if (guerreiro == null) guerreiro = GetComponent<AtaqueEspada>();

        if (guerreiro != null)
        {
            if (guerreiro.EstaEmGuardaBruta())
            {
                // Guarda Bruta: 75% de redução de dano!
                quantidade *= 0.25f;
                guerreiro.GerarFaiscasBloqueio();
            }
            else if (guerreiro.EstaEmPosturaHorizontal())
            {
                // Postura Horizontal Berserker: +35% de dano recebido
                quantidade *= 1.35f;
            }

            if (guerreiro.EstaEmExaustao())
            {
                // Exaustão pós-Frenesi: +30% de dano recebido
                quantidade *= 1.30f;
            }
        }

        // 3. Verifica a defesa e Perfect Parry do Druida (Pele de Árvore)
        ControleDruida druida = GetComponentInChildren<ControleDruida>();
        if (druida == null) druida = GetComponent<ControleDruida>();

        if (druida != null && druida.EstaEmBloqueio())
        {
            if (druida.ChecarPerfectParry())
            {
                // Perfect Parry Druídico: Anula 100% do dano e libera esporos de cura (+25 HP)
                druida.ExecutarPerfectParry();
                return;
            }
            else
            {
                // Bloqueio de Casca Normal: 50% de redução
                quantidade *= 0.5f;
            }
        }

        vidaAtual -= quantidade;

        if (vidaAtual <= 0)
        {
            vidaAtual = 0;
            Debug.Log("O Jogador caiu!");
            // Futuro Game Over
        }
        AtualizarBarras();
    }

    void AtualizarBarras()
    {
        if (imagemBarraVida != null)
            imagemBarraVida.fillAmount = vidaAtual / vidaMaxima;

        if (imagemBarraFuria != null)
            imagemBarraFuria.fillAmount = furiaAtual / furiaMaxima;
    }
}
