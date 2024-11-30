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
    public InputField MaxPlayersInputField; // Input Field untuk Max Players

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
    private bool isSceneLoading = false;

    [Header("Character Prefabs")]
    public GameObject KarakterBajuMerahPrefab;
    public GameObject KarakterBajuBiruPrefab;

    private Dictionary<int, GameObject> spawnedCharacters = new Dictionary<int, GameObject>(); // Untuk menyimpan karakter yang di-spawn
    private GameObject localPlayerCharacter; // Referensi karakter lokal


    private Dictionary<string, RoomInfo> cachedDaftarRoom;
    private Dictionary<string, GameObject> daftarRoomGameObjects;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // Sinkronisasi scene otomatis
        ActivatePanel(PanelLogin.name);
        cachedDaftarRoom = new Dictionary<string, RoomInfo>();
        daftarRoomGameObjects = new Dictionary<string, GameObject>();
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
            infoPlayerJoinText.text = "Players: " + roomInfo.PlayerCount + " /" + roomInfo.MaxPlayers;
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
        else
        {
            Debug.LogWarning("Hanya Master Client yang dapat memulai permainan!");
        }
    }
    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " berhasil dibuat.");
        ActivatePanel(DaftarRoomPanel.name);
        OnShowJoinRoomPanel(PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Bergabung dengan room: " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(JoinRoomPanel.name);
        UpdatePlayerInfo();

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master Client: Menunggu pemain lain.");
        }

        // Spawn karakter ketika memasuki gameplay
        SpawnCharacter();
    }

    private void SpawnCharacter()
    {
        GameObject karakterPrefab;

        // Pemain pertama mendapatkan karakter baju merah, pemain kedua baju biru
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            karakterPrefab = KarakterBajuMerahPrefab;
        }
        else if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            karakterPrefab = KarakterBajuBiruPrefab;
        }
        else
        {
            Debug.LogWarning("Karakter hanya didukung untuk dua pemain.");
            return;
        }

        // Spawn karakter
        Vector3 spawnPosition = new Vector3(Random.Range(-5, 5), 0, 0); // Posisi spawn acak
        localPlayerCharacter = PhotonNetwork.Instantiate(karakterPrefab.name, spawnPosition, Quaternion.identity);

        // Aktifkan karakter lokal
        localPlayerCharacter.SetActive(true);
        Debug.Log($"Karakter {karakterPrefab.name} diaktifkan untuk pemain {PhotonNetwork.LocalPlayer.ActorNumber}.");

        // Pastikan karakter pemain lain tidak aktif
        foreach (var entry in spawnedCharacters)
        {
            if (entry.Key != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                entry.Value.SetActive(false);
            }
        }

        spawnedCharacters[PhotonNetwork.LocalPlayer.ActorNumber] = localPlayerCharacter;
    }

    private void UpdateRoomPrefabsUI(List<string> prefabList)
    {
        // Misalnya: Update UI daftar prefabs
        foreach (string prefabName in prefabList)
        {
            Debug.Log($"Prefab: {prefabName} tersedia di room.");
            // Tambahkan prefab ke tampilan UI Anda
        }
    }

    [PunRPC]
    private void SyncCharacterActivation()
    {
        foreach (var entry in spawnedCharacters)
        {
            if (entry.Value != null)
            {
                entry.Value.SetActive(entry.Key == PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " telah keluar dari room.");
        UpdatePlayerInfo(); // Memperbarui informasi pemain saat ada pemain keluar
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
