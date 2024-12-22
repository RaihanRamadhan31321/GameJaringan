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

    [Header("UI Nama Pemain")]
    public Text namaPemainText; // Untuk menampilkan nama pemain di Main Menu
    public Text namaPemainGameText; // Untuk menampilkan nama pemain di Gameplay

    [Header("Room Panel")]
    public GameObject RoomPanel;

    [Header("Membuat Room Panel")]
    public GameObject BuatRoomPanel;
    public InputField NamaRoomInputField;
    public InputField MaxPlayersInputField; // Input Field untuk Max Players

    [Header("Daftar Pemain di Room")]
    public GameObject daftarPemainContainer; // Parent untuk daftar nama pemain
    public GameObject namaPemainPrefab; // Prefab Text untuk nama pemain


    [Header("Daftar Room Panel")]
    public GameObject DaftarRoomPanel;
    public GameObject daftarEntriRoomPrefab;
    public GameObject daftarRoomUtamaGameobject;

    [Header("Join Room dengan Nama")]
    public InputField InputNamaRoomJoin; // Input untuk memasukkan nama room
    public Button tombolGabungDenganNama; // Tombol untuk bergabung berdasarkan nama

    [Header("Solo Mode Panel")]
    public GameObject SoloModePanel; // Panel untuk Mode Solo

    [Header("Panel Join Room")]
    public GameObject JoinRoomPanel;
    public Text namaRoomJoinText;
    public Text infoPlayerJoinText;
    public Button tombolMulaiPermainan;
    private string selectedRoomName;
    private bool isSceneLoading = false;

    private Dictionary<string, RoomInfo> cachedDaftarRoom;
    private Dictionary<string, GameObject> daftarRoomGameObjects;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // Sinkronisasi scene otomatis
        ActivatePanel(PanelLogin.name);
        cachedDaftarRoom = new Dictionary<string, RoomInfo>();
        daftarRoomGameObjects = new Dictionary<string, GameObject>();

        // Tambahkan listener untuk tombol join dengan nama room
        if (tombolGabungDenganNama != null)
        {
            tombolGabungDenganNama.onClick.AddListener(OnJoinRoomByNameClicked);
        }
    }

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

            // Tampilkan nama di UI
            if (namaPemainText != null)
            {
                namaPemainText.text = "" + playerName + "";
            }

            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void OnRoomCreateButtonClicked()
    {
        string NamaRoom = NamaRoomInputField.text;
        string MaxPlayersText = MaxPlayersInputField.text;

        if (string.IsNullOrEmpty(NamaRoom))
        {
            NamaRoom = "Room " + Random.Range(1000, 10000);
            Debug.Log("Nama Room wajib di isi");
            NamaRoomInputField.placeholder.GetComponent<Text>().text = "Nama Tidak Boleh Kosong!";
            NamaRoomInputField.placeholder.GetComponent<Text>().color = Color.red;
        }

        if (string.IsNullOrEmpty(MaxPlayersText))
        {
            Debug.Log("Jumlah pemain maksimal wajib di isi!");
            MaxPlayersInputField.placeholder.GetComponent<Text>().text = "Jumlah pemain maksimal kosong!";
            MaxPlayersInputField.placeholder.GetComponent<Text>().color = Color.red;
            return;
        }

        if (!int.TryParse(MaxPlayersText, out int maxPlayers) || maxPlayers < 2 || maxPlayers > 20)
        {
            Debug.Log("Jumlah pemain maksimal harus angka antara 2 dan 20!");
            MaxPlayersInputField.text = "";
            MaxPlayersInputField.placeholder.GetComponent<Text>().text = "Masukkan angka 2-20!";
            MaxPlayersInputField.placeholder.GetComponent<Text>().color = Color.red;
            return;
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)maxPlayers;
        roomOptions.IsVisible = true; // Room dapat ditemukan
        roomOptions.IsOpen = true;   // Room terbuka untuk pemain lain

        PhotonNetwork.CreateRoom(NamaRoom, roomOptions);

    }

    public override void OnConnected()
    {
        Debug.Log("Terhubung ke Internet");
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
                room.PlayerCount + " /" + room.MaxPlayers;

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
            infoPlayerJoinText.text = "Players: " + roomInfo.PlayerCount + " / " + roomInfo.MaxPlayers;
        }
        else
        {
            namaRoomJoinText.text = "Room: " + roomName;
            infoPlayerJoinText.text = "Players:  /";
        }

        tombolMulaiPermainan.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        tombolMulaiPermainan.onClick.RemoveAllListeners();
        tombolMulaiPermainan.onClick.AddListener(OnStartGameButtonClicked);

        ActivatePanel(JoinRoomPanel.name);
    }


    public void OnJoinRoomByNameClicked()
    {
        string roomName = InputNamaRoomJoin.text;

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("Nama Room tidak boleh kosong!");
            InputNamaRoomJoin.placeholder.GetComponent<Text>().text = "Nama Room Kosong!";
            InputNamaRoomJoin.placeholder.GetComponent<Text>().color = Color.red;
            return;
        }

        // Coba bergabung ke room menggunakan PhotonNetwork.JoinRoom
        PhotonNetwork.JoinRoom(roomName, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Berhasil bergabung dengan room: " + PhotonNetwork.CurrentRoom.Name);
        OnShowJoinRoomPanel(PhotonNetwork.CurrentRoom.Name); // Perbarui UI untuk room
        UpdatePlayerList(); // Perbarui daftar pemain
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("Gagal bergabung dengan room: " + message);
        InputNamaRoomJoin.text = "";
        InputNamaRoomJoin.placeholder.GetComponent<Text>().text = "Room Tidak Ditemukan!";
        InputNamaRoomJoin.placeholder.GetComponent<Text>().color = Color.red;
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
        if (PhotonNetwork.IsMasterClient && !isSceneLoading)
        {
            isSceneLoading = true;
            Debug.Log("Master Client memulai permainan. Memuat scene GamePlay...");
            PhotonNetwork.LoadLevel("GamePlay");
        }
    }


    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " berhasil dibuat.");
        ActivatePanel(DaftarRoomPanel.name);
        OnShowJoinRoomPanel(PhotonNetwork.CurrentRoom.Name);
    }


    public void OnSoloModeButtonClicked()
    {
        Debug.Log("Masuk ke Mode Solo...");
        ActivatePanel(SoloModePanel.name);
    }

    private void UpdatePlayerInfo()
    {
        // Menampilkan jumlah pemain dan batas maksimal
        if (PhotonNetwork.CurrentRoom != null)
        {
            infoPlayerJoinText.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
        }
        else
        {
            infoPlayerJoinText.text = "Players: 0 / 0";
        }
    }

    private void UpdatePlayerList()
    {
        // Bersihkan daftar sebelumnya
        foreach (Transform child in daftarPemainContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Tambahkan nama pemain saat ini
        foreach (var player in PhotonNetwork.PlayerList)
        {
            GameObject namaPemainObj = Instantiate(namaPemainPrefab, daftarPemainContainer.transform);
            Text namaPemainText = namaPemainObj.GetComponent<Text>();
            namaPemainText.text = player.NickName;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " bergabung ke room.");
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " keluar dari room.");
        UpdatePlayerList();
    }

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivatePanel(DaftarRoomPanel.name);
    }

    public void OnStartSoloGameButtonClicked()
    {
        Debug.Log("Memulai permainan Mode Solo...");
        SceneManager.LoadScene("Game1"); // Pastikan Scene ini sudah ada di Build Settings
    }

    public void ActivatePanel(string panelToBeActivated)
    {
        PanelLogin.SetActive(panelToBeActivated.Equals(PanelLogin.name));
        RoomPanel.SetActive(panelToBeActivated.Equals(RoomPanel.name));
        BuatRoomPanel.SetActive(panelToBeActivated.Equals(BuatRoomPanel.name));
        DaftarRoomPanel.SetActive(panelToBeActivated.Equals(DaftarRoomPanel.name));
        JoinRoomPanel.SetActive(panelToBeActivated.Equals(JoinRoomPanel.name));
        SoloModePanel.SetActive(panelToBeActivated.Equals(SoloModePanel.name)); // Aktifkan Panel Solo
    }
}
