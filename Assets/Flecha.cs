using UnityEngine;

public class Flecha : MonoBehaviour
{
    [Header("Configurações da Flecha")]
    public float dano = 20f;
    public float tempoDeVida = 5f;

    private bool jaAcertou = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        // Alinha a flecha com a sua trajetória no ar
        if (!jaAcertou && rb != null && rb.linearVelocity.sqrMagnitude > 1f)
        {
            transform.forward = rb.linearVelocity.normalized;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        ProcessarImpacto(col.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        ProcessarImpacto(other.gameObject);
    }

    void ProcessarImpacto(GameObject alvo)
    {
        if (jaAcertou) return;

        // Ignora se colidir com o próprio jogador
        if (alvo.CompareTag("Player") || alvo.GetComponent<PlayerController>() != null || alvo.GetComponentInParent<PlayerController>() != null)
        {
            return;
        }

        jaAcertou = true;

        // Se atingiu um inimigo com vida
        SistemaDeVida vida = alvo.GetComponent<SistemaDeVida>();
        if (vida == null)
        {
            vida = alvo.GetComponentInParent<SistemaDeVida>();
        }

        if (vida != null)
        {
            ControleRanger ranger = FindAnyObjectByType<ControleRanger>();
            if (ranger != null)
            {
                ranger.RegistrarAcertoPassiva(alvo, vida, dano);
            }
            else
            {
                vida.ReceberDano(dano);
            }

            // Leve empurrão físico
            Rigidbody rbInimigo = alvo.GetComponent<Rigidbody>();
            if (rbInimigo != null)
            {
                rbInimigo.AddForce(transform.forward * 1.0f, ForceMode.Impulse);
            }
        }

        // Destrói a flecha após colidir com qualquer coisa (inimigo, chão, parede)
        Destroy(gameObject);
    }
}
