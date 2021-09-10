using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine.SceneManagement;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    [SerializeField] private string gameVersion = "0.01";
    [SerializeField] private byte maxPlayerPerRoom = 5;

    [SerializeField] private string nickName = string.Empty;

    [SerializeField] private Button connectButton = null;
    [SerializeField] private Button JoinButton = null;
    [SerializeField] private InputField NickNameInput = null;
    public static float JoinTime = 0;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        connectButton.interactable = true;
    }

    public void Connect()
    {
        {
            if (string.IsNullOrEmpty(nickName))
            {
                Debug.Log("NickName is empty");
                return;
            }

            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                Debug.LogFormat("Connect : {0}", gameVersion);

                PhotonNetwork.GameVersion = gameVersion;

                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }

    public void OnValueChangeNickName(string _nickName)
    {
        nickName = _nickName;

        PhotonNetwork.NickName = nickName;
    }

    public override void OnConnectedToMaster()
    {
        Debug.LogFormat("Connected to Master : {0}", nickName);

        connectButton.interactable = false;

        connectButton.gameObject.SetActive(false);
        NickNameInput.gameObject.SetActive(false);
        JoinButton.gameObject.SetActive(true);

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("Disconnected : {0}", cause);

        connectButton.interactable = true;
        connectButton.gameObject.SetActive(true);
        NickNameInput.gameObject.SetActive(true);
        JoinButton.gameObject.SetActive(false);

        Debug.Log("Create Room");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");

        SceneManager.LoadScene("Room");

        if (PhotonNetwork.IsMasterClient == true)
        {
            JoinTime = Time.time;
        }
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogErrorFormat("JoinRandomFailed({0}):{1}", returnCode, message);

        connectButton.interactable = true;
        connectButton.gameObject.SetActive(true);
        NickNameInput.gameObject.SetActive(true);
        JoinButton.gameObject.SetActive(false);

        Debug.Log("Create Room");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom });
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }
}
