using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManajerJaringan : MonoBehaviourPunCallbacks
{
    [Header("Login UI Panel")]
    public InputField NamaPlayer;
    public GameObject PanelLogin;

    [Header("Room Panel")]
    public GameObject RoomPanel;

    [Header("Membuat Room Panel")]
    public GameObject BuatRoomPanel;
    public InputField NamaRoomInputField;
    public InputField maxPlayerInputField;

    [Header("Daftar Room Panel")]
    public GameObject DaftarRoomPanel;
    public GameObject daftarEntriRoomPrefab;
    public GameObject daftarRoomUtamaGameobject;

    [Header("Panel Join Room")]
    public GameObject JoinRoomPanel;
    public Text namaRoomJoinText;
    public Text infoPlayerJoinText;
    public Button tombolMulaiPermainan;
    private string selectedRoomName;

    private Dictionary<string, RoomInfo> cachedDaftarRoom;
    private Dictionary<string, GameObject> daftarRoomGameObjects;

    private void Start()
    {
        ActivatePanel(PanelLogin.name);
        cachedDaftarRoom = new Dictionary<string, RoomInfo>();
        daftarRoomGameObjects = new Dictionary<string, GameObject>();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Tombol Login
    public void OnLoginButtonClicked()
    {
        string playerName = NamaPlayer.text;

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log("Nama Player Tidak Valid!");
            NamaPlayer.placeholder.GetComponent<Text>().text = "Nama Tidak Boleh Kosong!";
            NamaPlayer.placeholder.GetComponent<Text>().color = Color.red;
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // Tombol buat room
    public void OnRoomCreateButtonClicked()
    {
        string NamaRoom = NamaRoomInputField.text;

        if (string.IsNullOrEmpty(NamaRoom))
        {
            NamaRoom = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)int.Parse(maxPlayerInputField.text)
        };

        PhotonNetwork.CreateRoom(NamaRoom, roomOptions);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Terhubung ke Photon");
        ActivatePanel(RoomPanel.name);
    }

    public override void OnRoomListUpdate(List<RoomInfo> daftarRoom)
    {
        cachedDaftarRoom.Clear();
        foreach (RoomInfo room in daftarRoom)
        {
            if (room.IsOpen && room.IsVisible && !room.RemovedFromList)
            {
                cachedDaftarRoom[room.Name] = room;
            }
        }

        UpdateRoomListView();
    }

    private void UpdateRoomListView()
    {
        ClearRoomListView();

        foreach (RoomInfo room in cachedDaftarRoom.Values)
        {
            GameObject daftarEntriRoomGameobject = Instantiate(daftarEntriRoomPrefab);
            daftarEntriRoomGameobject.transform.SetParent(daftarRoomUtamaGameobject.transform);
            daftarEntriRoomGameobject.transform.localScale = Vector3.one;

            daftarEntriRoomGameobject.transform.Find("NamaRoomText").GetComponent<Text>().text = room.Name;
            daftarEntriRoomGameobject.transform.Find("MaksPlayerText").GetComponent<Text>().text =
                room.PlayerCount + " /10" + room.MaxPlayers;

            daftarEntriRoomGameobject.transform.Find("TombolLihatDetailRoom").GetComponent<Button>()
                .onClick.AddListener(() => OnShowJoinRoomPanel(room.Name));

            daftarRoomGameObjects.Add(room.Name, daftarEntriRoomGameobject);
        }
    }

    private void ClearRoomListView()
    {
        foreach (GameObject daftarRoomGameobject in daftarRoomGameObjects.Values)
        {
            Destroy(daftarRoomGameobject);
        }

        daftarRoomGameObjects.Clear();
    }

    private void OnShowJoinRoomPanel(string roomName)
    {
        selectedRoomName = roomName;
        RoomInfo roomInfo = cachedDaftarRoom.ContainsKey(roomName) ? cachedDaftarRoom[roomName] : null;

        if (roomInfo != null)
        {
            namaRoomJoinText.text = "Room: " + roomName;
            infoPlayerJoinText.text = "Players: " + roomInfo.PlayerCount + " /10" + roomInfo.MaxPlayers;
        }
        else
        {
            namaRoomJoinText.text = "Room: " + roomName;
            infoPlayerJoinText.text = "Players:  /10";
        }

        // Aktifkan tombol Mulai jika pemain adalah Master Client
        tombolMulaiPermainan.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        tombolMulaiPermainan.onClick.RemoveAllListeners();
        tombolMulaiPermainan.onClick.AddListener(OnStartGameButtonClicked);

        ActivatePanel(JoinRoomPanel.name);
    }

    public void OnJoinRoomButtonClicked()
    {
        if (!string.IsNullOrEmpty(selectedRoomName))
        {
            PhotonNetwork.JoinRoom(selectedRoomName);
        }
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!PhotonNetwork.IsMessageQueueRunning)
            {
                Debug.LogWarning("Photon message queue is paused!");
                return;
            }

            PhotonNetwork.LoadLevel("GamePlay");
        }
        else
        {
            Debug.LogWarning("Hanya Master Client yang dapat memulai permainan!");
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " berhasil dibuat.");

        // Pindah ke panel daftar room
        ActivatePanel(DaftarRoomPanel.name);

        // Langsung tampilkan panel join room untuk room yang baru dibuat
        OnShowJoinRoomPanel(PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Bergabung dengan room: " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(JoinRoomPanel.name);
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivatePanel(DaftarRoomPanel.name);
    }

    public void ActivatePanel(string panelToBeActivated)
    {
        PanelLogin.SetActive(panelToBeActivated.Equals(PanelLogin.name));
        RoomPanel.SetActive(panelToBeActivated.Equals(RoomPanel.name));
        BuatRoomPanel.SetActive(panelToBeActivated.Equals(BuatRoomPanel.name));
        DaftarRoomPanel.SetActive(panelToBeActivated.Equals(DaftarRoomPanel.name));
        JoinRoomPanel.SetActive(panelToBeActivated.Equals(JoinRoomPanel.name));
    }
}