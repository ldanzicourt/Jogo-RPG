using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimentação Base")]
    public float velocidadeAndar = 12f;
    public float gravity = -20f;
    public float jumpHeight = 2.5f;

    [Header("Habilidade: Investida (Estilo Reinhardt)")]
    public float velocidadeInvestida = 24f;    // Velocidade do atropelamento
    public float duracaoInvestida = 3f;        // Tempo máximo correndo
    public float danoBatidaParede = 50f;       // Dano ao esmagar o alvo na parede
    public float distanciaEmpurrao = 1.5f;     // Distância que o inimigo fica preso à sua frente

    [Header("Câmera")]
    public float mouseSensitivity = 300f;
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    // Variáveis de controle da Investida
    private bool emInvestida = false;
    private float tempoFimInvestida = 0f;
    private GameObject inimigoPrendido = null;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        //Cursor.lockState = CursorLockMode.Locked;
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
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- 3. ATIVAÇÃO DA INVESTIDA (Shift) ---
        if (Input.GetKeyDown(KeyCode.LeftShift) && !emInvestida)
        {
            emInvestida = true;
            tempoFimInvestida = Time.time + duracaoInvestida;
        }

        // --- 4. CONTROLE DE VELOCIDADE E TEMPO ---
        float velocidadeAtual = velocidadeAndar;
        if (emInvestida)
        {
            velocidadeAtual = velocidadeInvestida;

            // Se o tempo acabou, para de correr
            if (Time.time >= tempoFimInvestida)
            {
                PararInvestida();
            }
            // Se ainda está correndo e tem um inimigo preso, arrasta ele!
            else if (inimigoPrendido != null)
            {
                ArrastarInimigo();
            }
        }

        // --- 5. MOVIMENTAÇÃO (WASD) ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * velocidadeAtual * Time.deltaTime);

        // --- 6. PULO ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- 7. APLICA GRAVIDADE ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // --- SISTEMA DE COLISÃO NATIVO DA UNITY ---
    // Essa função é chamada automaticamente quando o jogador "esbarra" em algo
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!emInvestida) return;

        // 1. CAPTURAR O INIMIGO
        // Se bateu em um objeto com Vida, e ainda não prendemos ninguém...
        if (inimigoPrendido == null && hit.gameObject.GetComponent<SistemaDeVida>() != null)
        {
            inimigoPrendido = hit.gameObject;

            // Desliga a física do inimigo temporariamente para ele não brigar com a gente
            Rigidbody rb = inimigoPrendido.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }

        // 2. ESMAGAR NA PAREDE
        // Se batermos em algo que não é o chão (normal reta) e não é o inimigo que estamos segurando...
        if (inimigoPrendido != null && hit.gameObject != inimigoPrendido)
        {
            // Verifica se é uma superfície vertical (Parede) avaliando o eixo Y da batida
            if (Mathf.Abs(hit.normal.y) < 0.1f)
            {
                // Bateu na parede com o inimigo! Aplica dano e encerra a investida.
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
        // Posiciona o inimigo matematicamente exato na frente do jogador
        Vector3 posicaoAlvo = transform.position + (transform.forward * distanciaEmpurrao);

        // Mantém a altura do inimigo para ele não afundar no chão
        posicaoAlvo.y = inimigoPrendido.transform.position.y;

        inimigoPrendido.transform.position = posicaoAlvo;

        // Se o inimigo morrer no meio do caminho (por exemplo, você bateu nele com a espada), soltamos ele
        if (inimigoPrendido == null || inimigoPrendido.GetComponent<SistemaDeVida>() == null)
        {
            PararInvestida();
        }
    }

    void PararInvestida()
    {
        emInvestida = false;

        // Devolve a física normal para o inimigo ao soltá-lo
        if (inimigoPrendido != null)
        {
            Rigidbody rb = inimigoPrendido.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
            inimigoPrendido = null;
        }
    }
}