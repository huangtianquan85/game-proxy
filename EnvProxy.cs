using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

public class EnvProxy : MonoBehaviour
{
    public static Action<int> handleEnvProxy = null;

    private const string PubKey =
        "<RSAKeyValue>...</RSAKeyValue>";

    private static EnvProxy _instance;

    [RuntimeInitializeOnLoadMethod]
    private static void CheckInstance()
    {
        if (_instance != null || !Application.isPlaying) return;

        var go = new GameObject("EnvProxy");
        GameObject.DontDestroyOnLoad(go);
        _instance = go.AddComponent<EnvProxy>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(Send());
    }

    private static IEnumerator Send()
    {
        var rand = new Random();
        var r = rand.Next(1 << 16, 1 << 24);
        var rawData = Encoding.UTF8.GetBytes(r.ToString());
        var rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(PubKey);
        var cipherText = rsa.Encrypt(rawData, false);
        var base64Text = Convert.ToBase64String(cipherText);

        var request = new UnityWebRequest();
        request.url = "https://env.example.com";

        // add version info
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(base64Text));

        request.downloadHandler = new DownloadHandlerBuffer();
        request.method = UnityWebRequest.kHttpVerbPOST;
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            request.Dispose();
        }
        else
        {
            if (handleEnvProxy != null)
            {
                handleEnvProxy(int.Parse(request.downloadHandler.text) - r);
            }
        }
    }
}
