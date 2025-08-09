// using Unity.Netcode;
// using UnityEngine;

// public class MenuUI : MonoBehaviour
// {
//     public void HostBtnClick()
//     {
//         Debug.Log("Host button clicked");
//         NetworkManager.Singleton.StartHost();
//         // Add logic to start hosting a game
//     }
//     public void JoinBtnClick()
//     {
//         Debug.Log("Join button clicked");
//         NetworkManager.Singleton.StartClient();
//         // Add logic to join a game
//     }
// }

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using TMPro;

public class MenuUI : MonoBehaviour
{
    [Header("Join Settings")]
    [SerializeField] TMP_InputField joinEndpointField;   // nhập "127.0.0.1:7777"
    [SerializeField] ushort defaultPort = 7777;

    [Header("Host Settings")]
    [SerializeField] bool useEphemeralPort = true;       // true => để OS tự chọn cổng trống
    [SerializeField] ushort hostPort = 7777;             // dùng khi useEphemeralPort = false

    NetworkManager NM => NetworkManager.Singleton;

    void Awake()
    {
        if (NM == null)
        {
            Debug.LogError("Missing NetworkManager in scene.");
            return;
        }

        NM.OnServerStarted += OnServerStarted;
        NM.OnClientConnectedCallback += OnClientConnected;
        NM.OnClientDisconnectCallback += OnClientDisconnected;
    }

    // ==== UI EVENTS ====
    public void HostBtnClick()
    {
        if (!EnsureReady()) return;

        var utp = NM.NetworkConfig.NetworkTransport as UnityTransport;
        if (utp == null) { Debug.LogError("Missing UnityTransport on NetworkManager"); return; }

        ushort port = useEphemeralPort ? (ushort)0 : hostPort;

        // Host: listen trên tất cả interface. Port = 0 => OS tự cấp cổng rảnh.
        // Nếu project bạn dùng UnityTransport bản rất cũ KHÔNG có overload 3 tham số,
        // hãy dùng: utp.SetConnectionData("0.0.0.0", port);
        utp.SetConnectionData("0.0.0.0", port, "0.0.0.0");

        bool ok = NM.StartHost();
        if (!ok)
        {
            Debug.LogError("StartHost() failed. Port may be in use or transport misconfigured.");
            return;
        }

        // Lấy endpoint thật sự đã bind (đặc biệt hữu ích khi port=0)
        var ep = utp.GetLocalEndpoint();
        Debug.Log($"Host started at {ep.Address}:{ep.Port}");
    }

    public void JoinBtnClick()
    {
        if (!EnsureReady()) return;

        var utp = NM.NetworkConfig.NetworkTransport as UnityTransport;
        if (utp == null) { Debug.LogError("Missing UnityTransport on NetworkManager"); return; }

        if (!TryParseEndpoint(joinEndpointField?.text, out string address, out ushort port))
        {
            address = "127.0.0.1";
            port = defaultPort;
        }

        // Client: chỉ cần địa chỉ + cổng của host
        utp.SetConnectionData(address, port);

        bool ok = NM.StartClient();
        Debug.Log(ok ? $"Connecting to {address}:{port}..." : "StartClient() failed.");
    }

    public void LeaveBtnClick()
    {
        if (NM == null) return;
        if (NM.IsClient || NM.IsServer)
        {
            NM.Shutdown();
            Debug.Log("Networking stopped.");
        }
    }

    // ==== Helpers ====
    bool EnsureReady()
    {
        if (NM == null) { Debug.LogError("Missing NetworkManager"); return false; }
        if (NM.IsClient || NM.IsServer)
        {
            Debug.LogWarning("Networking already running.");
            return false;
        }
        return true;
    }

    void OnServerStarted() => Debug.Log("Server started.");
    void OnClientConnected(ulong clientId)
    {
        if (clientId == NM.LocalClientId)
            Debug.Log("Connected.");
    }
    void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NM.LocalClientId)
            Debug.LogWarning("Disconnected.");
    }

    // Hỗ trợ "host", "host:port" và IPv6 "[::1]:7777"
    bool TryParseEndpoint(string input, out string host, out ushort port)
    {
        host = null;
        port = defaultPort;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim();

        // IPv6 dạng [addr]:port
        if (input.StartsWith("["))
        {
            int r = input.IndexOf(']');
            if (r > 0)
            {
                host = input.Substring(1, r - 1);
                if (r + 1 < input.Length && input[r + 1] == ':' &&
                    ushort.TryParse(input.Substring(r + 2), out var p)) port = p;
                return true;
            }
            return false;
        }

        // Tách theo dấu ":" cuối (IPv6 có dấu ":" đã xử lý ở trên)
        int i = input.LastIndexOf(':');
        if (i > 0 && i < input.Length - 1 && ushort.TryParse(input[(i + 1)..], out var p2))
        {
            host = input[..i];
            port = p2;
            return true;
        }

        // Không có port -> dùng mặc định
        host = input;
        return true;
    }
}
