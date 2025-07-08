using UnityEngine;

public class TestarEscrita : MonoBehaviour
{

    public Writter escrita;
    public string escritaName;
    public WriteEffectType type;
    public void Escrever()
    {
        escrita.Escrever(escritaName, type);
    }
}
