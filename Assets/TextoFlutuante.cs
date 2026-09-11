using UnityEngine;
using TMPro;

public class TextoFlutuante : MonoBehaviour
{
    public float velocidadeSubida = 2f;

    void Start()
    {
        // Destroi este texto automaticamente apos 1.2 segundos para nao acumular no jogo
        Destroy(gameObject, 1.2f);
    }

    void Update()
    {
        // Move o texto para cima continuamente
        transform.Translate(Vector3.up * velocidadeSubida * Time.deltaTime, Space.World);

        // Faz o texto virar para a camera do jogador (efeito Billboard)
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }

    public void DefinirTexto(string texto, Color cor)
    {
        TextMeshPro tmp = GetComponent<TextMeshPro>();
        if (tmp == null)
        {
            tmp = gameObject.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 4.5f;
        }
        tmp.text = texto;
        tmp.color = cor;
    }
}