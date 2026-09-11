using UnityEngine;

public static class MateriaisAlienigenas
{
    private static Shader GetLitShader()
    {
        return Shader.Find("Universal Render Pipeline/Lit") ??
               Shader.Find("Standard") ??
               Shader.Find("Diffuse");
    }

    // Chão da Pradaria da Ferrugem: terra escura, arame farpado oxidado e cacos de vidro
    public static Material CriarMaterialChaoFerrugem()
    {
        Material mat = new Material(GetLitShader());
        mat.name = "Mat_PradariaFerrugem";
        mat.color = new Color(0.18f, 0.14f, 0.11f); // Tom terroso escuro e metálico

        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.65f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.45f);

        // Brilho tênue estático refletindo estilhaços no chão
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.04f, 0.08f, 0.12f));
        }
        return mat;
    }

    // Ferro Enferrujado para barricadas, trincheiras e correntes colossais
    public static Material CriarMaterialSucataOxidada()
    {
        Material mat = new Material(GetLitShader());
        mat.name = "Mat_SucataOxidada";
        mat.color = new Color(0.32f, 0.16f, 0.09f); // Laranja ferrugem profundo

        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.75f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.22f);
        return mat;
    }

    // Vidro Temperado Escuro Alienígena (para o Inimigo Casca de Vidro)
    public static Material CriarMaterialVidroEscuro()
    {
        Material mat = new Material(GetLitShader());
        mat.name = "Mat_VidroTemperadoEscuro";
        mat.color = new Color(0.08f, 0.12f, 0.15f, 0.88f); // Vidro fumê escuro espelhado

        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.92f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);

        // Fraturas luminescentes causadas pelo feixe sonoro
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.15f, 0.35f, 0.45f));
        }
        return mat;
    }

    // Metal Polido / Aço Escuro para a Safe Room e Porta Hidráulica
    public static Material CriarMaterialSafeRoom()
    {
        Material mat = new Material(GetLitShader());
        mat.name = "Mat_SafeRoomMetal";
        mat.color = new Color(0.22f, 0.25f, 0.28f);

        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.85f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.6f);
        return mat;
    }

    // Sinalizador Luminoso Seguro (Verde da Safe Room)
    public static Material CriarMaterialLuzSegura()
    {
        Material mat = new Material(GetLitShader());
        mat.name = "Mat_LuzSegura";
        Color corVerde = new Color(0.1f, 0.95f, 0.4f);
        mat.color = corVerde;

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", corVerde * 2.5f);
        }
        return mat;
    }

    // Material de Cristais Estáticos Alienígenas (Ciano brilhante)
    public static Material CriarMaterialCristalEstatico()
    {
        Material mat = new Material(GetLitShader());
        mat.name = "Mat_CristalEstatico";
        Color corCiano = new Color(0.2f, 0.8f, 1f);
        mat.color = corCiano;

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", corCiano * 3.0f);
        }
        return mat;
    }
}
