using UnityEngine;

public class TextoFlutuante : MonoBehaviour
{
    public float velocidadeSubida = 2f;

    void Start()
    {
        // Destrói este texto automaticamente após 1 segundo para não travar o jogo
        Destroy(gameObject, 1f);
    }

    void Update()
    {
        // Move o texto para cima continuamente
        transform.Translate(Vector3.up * velocidadeSubida * Time.deltaTime, Space.World);

        // Faz o texto virar para a câmera do jogador (efeito Billboard)
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}