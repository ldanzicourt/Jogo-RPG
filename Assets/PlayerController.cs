using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum TipoClasse
    {
        Guerreiro,
        Ranger,
        Mago,
        Druida
    }

    [Header("Classe Ativa")]
    public TipoClasse classeAtual = TipoClasse.Guerreiro;

    [Header("Movimentação Base")]
    public float velocidadeAndar = 12f;
    public float gravity = -20f;
    public float jumpHeight = 2.5f;

    [Header("Habilidades do Guerreiro (Investida)")]
    public float velocidadeInvestida = 24f;    // Velocidade do atropelamento
    public float duracaoInvestida = 3f;        // Tempo máximo correndo
    public float danoBatidaParede = 50f;       // Dano ao esmagar o alvo na parede
    public float distanciaEmpurrao = 1.5f;     // Distância que o inimigo fica preso à sua frente

    [Header("Habilidades do Ranger (Corrida e Escalada)")]
    public float velocidadeSprint = 18f;       // Velocidade de corrida rápida ao segurar Shift
    public float velocidadeEscalada = 8f;      // Velocidade subindo a parede
    public float distanciaChecagemParede = 0.9f;// Distância do raio para detectar a parede

    [Header("Câmera")]
    public float mouseSensitivity = 300f;
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    // Variáveis de controle do Guerreiro (Investida)
    private bool emInvestida = false;
    private float tempoFimInvestida = 0f;
    private GameObject inimigoPrendido = null;

    // Variáveis de controle do Ranger (Escalada)
    private bool escalandoParede = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void DefinirClasse(TipoClasse novaClasse)
    {
        classeAtual = novaClasse;
        PararInvestida();
        escalandoParede = false;
    }

    void Update()
    {
        // --- 1. VISÃO DO MOUSE ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // --- 2. VERIFICAÇÃO DE CHÃO ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0 && !escalandoParede)
        {
            velocity.y = -2f;
        }

        // --- 3. CONTROLE ESPECÍFICO DE CADA CLASSE ---
        float velocidadeAtual = velocidadeAndar;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (classeAtual == TipoClasse.Guerreiro)
        {
            // Ativação da Investida do Guerreiro (Shift)
            if (Input.GetKeyDown(KeyCode.LeftShift) && !emInvestida)
            {
                emInvestida = true;
                tempoFimInvestida = Time.time + duracaoInvestida;
            }

            if (emInvestida)
            {
                velocidadeAtual = velocidadeInvestida;

                if (Time.time >= tempoFimInvestida)
                {
                    PararInvestida();
                }
                else if (inimigoPrendido != null)
                {
                    ArrastarInimigo();
                }
            }
        }
        else if (classeAtual == TipoClasse.Ranger)
        {
            // Ranger: Shift ativa Corrida e Escalada de Parede
            bool segurandoShift = Input.GetKey(KeyCode.LeftShift);

            if (segurandoShift)
            {
                velocidadeAtual = velocidadeSprint;

                // Verifica se está empurrando para frente contra uma parede
                if (z > 0.1f && ChecarParedeParaEscalada())
                {
                    escalandoParede = true;
                }
                else
                {
                    escalandoParede = false;
                }
            }
            else
            {
                escalandoParede = false;
            }
        }

        // --- 4. MOVIMENTAÇÃO (WASD) ---
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * velocidadeAtual * Time.deltaTime);

        // --- 5. PULO / ESCALADA / GRAVIDADE ---
        if (escalandoParede && classeAtual == TipoClasse.Ranger)
        {
            // Sobe continuamente a parede sem gravidade
            velocity.y = velocidadeEscalada;
        }
        else
        {
            // Pulo padrão
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Aplica gravidade normal
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // Checa se há uma parede vertical na frente do Ranger
    bool ChecarParedeParaEscalada()
    {
        Vector3 origem = transform.position + Vector3.up * 0.8f;
        RaycastHit hit;

        if (Physics.Raycast(origem, transform.forward, out hit, distanciaChecagemParede))
        {
            // Se a superfície for vertical (parede), a normal do eixo Y é próxima de zero
            if (Mathf.Abs(hit.normal.y) < 0.3f && !hit.collider.isTrigger)
            {
                return true;
            }
        }
        return false;
    }

    // --- SISTEMA DE COLISÃO DO GUERREIRO ---
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (classeAtual != TipoClasse.Guerreiro || !emInvestida) return;

        // 1. CAPTURAR O INIMIGO
        if (inimigoPrendido == null && hit.gameObject.GetComponent<SistemaDeVida>() != null)
        {
            inimigoPrendido = hit.gameObject;

            Rigidbody rb = inimigoPrendido.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }

        // 2. ESMAGAR NA PAREDE
        if (inimigoPrendido != null && hit.gameObject != inimigoPrendido)
        {
            if (Mathf.Abs(hit.normal.y) < 0.1f)
            {
                SistemaDeVida vidaInimigo = inimigoPrendido.GetComponent<SistemaDeVida>();
                if (vidaInimigo != null)
                {
                    vidaInimigo.ReceberDano(danoBatidaParede);
                    Debug.Log("Inimigo esmagado na parede!");
                }
                PararInvestida();
            }
        }
    }

    void ArrastarInimigo()
    {
        Vector3 posicaoAlvo = transform.position + (transform.forward * distanciaEmpurrao);
        posicaoAlvo.y = inimigoPrendido.transform.position.y;
        inimigoPrendido.transform.position = posicaoAlvo;

        if (inimigoPrendido == null || inimigoPrendido.GetComponent<SistemaDeVida>() == null)
        {
            PararInvestida();
        }
    }

    void PararInvestida()
    {
        emInvestida = false;

        if (inimigoPrendido != null)
        {
            Rigidbody rb = inimigoPrendido.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
            inimigoPrendido = null;
        }
    }
}
