using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

// ReSharper disable once CheckNamespace
public class EnvProxy : MonoBehaviour
{
    public static Action<int> HandleEnvProxy = null;
    public static Func<long, bool> IsPackageExpire = timestamp => false;

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

    private static void CheckConfig(string data)
    {
        var rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(PubKey);
        var m = GetBigInt(rsa.ExportParameters(false).Modulus);
        var e = GetBigInt(rsa.ExportParameters(false).Exponent);

        var bytes = Convert.FromBase64String(data);
        var cipher = new BigInteger(bytes.Append(new byte()).ToArray());
        var bigNum = BigInteger.ModPow(cipher, e, m);
        var num = Convert.ToInt64(bigNum.ToString());

        var env = num % 10;
        HandleEnvProxy?.Invoke((int)env);

        var expTimestamp = num / 10;
        IsPackageExpire = t => t > expTimestamp;
    }

    private static BigInteger GetBigInt(byte[] bytes)
    {
        bytes = bytes.Reverse().Append(new byte()).ToArray();
        return new BigInteger(bytes);
    }

    // Start is called before the first frame update
    private void Start()
    {
        var configPath = $"{Application.streamingAssetsPath}/Config/Env.bytes";
#if UNITY_EDITOR
        if (File.Exists(configPath))
        {
            CheckConfig(File.ReadAllText(configPath));
        }
        StartCoroutine(Send());
#else
        var request = UnityWebRequest.Get(configPath);
        request.SendWebRequest().completed += (completedOp) =>
        {
            if (!request.isNetworkError && !request.isHttpError)
            {
                CheckConfig(request.downloadHandler.text);
            }

            request.Dispose();
            StartCoroutine(Send());
        };
#endif
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
            HandleEnvProxy?.Invoke(int.Parse(request.downloadHandler.text) - r);
        }
    }
}
