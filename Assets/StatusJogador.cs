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

    void Start()
    {
        vidaAtual = vidaMaxima;
        furiaAtual = 0f; // Guerreiro começa com a barra vazia
        AtualizarBarras();
    }

    void Update()
    {
        // Se houver Fúria E já passaram 5 segundos sem acertar golpes...
        if (furiaAtual > 0 && Time.time > tempoUltimoAcerto + tempoParaComecarPerder)
        {
            // Esvazia gradualmente
            furiaAtual -= perdaDeFuriaPorSegundo * Time.deltaTime;

            if (furiaAtual < 0) furiaAtual = 0;

            AtualizarBarras();
        }
    }

    public void GanharFuria(float quantidade)
    {
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

    public void ReceberDano(float quantidade)
    {
        vidaAtual -= quantidade;

        if (vidaAtual <= 0)
        {
            vidaAtual = 0;
            Debug.Log("O Guerreiro caiu!");
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
