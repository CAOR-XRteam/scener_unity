using UnityEngine;

public class MyScript : MonoBehaviour
{
    void Start() => Debug.Log("======= Program Start =======");
}

public static class Logger {
    public static void say(string truc) => Debug.Log("-----> " + truc);
}