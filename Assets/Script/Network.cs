using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Network : MonoBehaviourPunCallbacks
{
    [Header("LoginPanel")]
    public GameObject LoginPanel;
    public InputField NickNameInput;

    [Header("LobbyPanel")]
    public GameObject LobbyPanel;
    public InputField RoomInput;
    public Text MyNickname;
    public Button PreviousBtn;
    public Button NextBtn;
    public Button[] CellBtn;



    [Header("RoomPanel")]
    public GameObject RoomPanel;
    public Text RoomName;
    public Text RoomNum;
    public Text[] ChatText;
    public InputField ChatInput;
    public Text[] RoomNickSpace;

    [Header("ETC")]
    public Text StatusText;
    public PhotonView PV;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple, myNum;
    #region 서버연결
    void Awake() => Screen.SetResolution(1280, 720, false);
    //처음 실행시 스크린의 크기를 지정한다.

    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        //StatusText에 현재의 상태를 출력한다.
        //LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";
        //                    현재 플레이어의 수           - 방안의 플레이어의 수                               접속한 인원의 수
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    //connect 함수 : 연결요청 0(true)과 -1(false)을 반환한다.

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        LobbyPanel.SetActive(true);
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        MyNickname.text = PhotonNetwork.LocalPlayer.NickName;
        PV.RPC("loginRPC", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        myList.Clear();
    }

    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {
        LobbyPanel.SetActive(false);
        LoginPanel.SetActive(true);
    }


    #endregion


    #region 방리스트 갱신
    // ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    public void MyListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        MyListRenewal();
    }

    void MyListRenewal()
    {
        // 최대페이지
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // 이전, 다음버튼
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }
    #endregion


    #region 방
    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 4 });
    //방을만들때 룸이름이 비어있다면 Room과 랜덤한 숫자를 합친 이름으로 생성하고 비어있지 않다면 적혀있는 텍스트로 출력한다.
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    //빠른시작(랜덤한 방으로 입장)
    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnCreatedRoom()
    {
        PV.RPC("InNickRPC", RpcTarget.AllBuffered, PhotonNetwork.NickName);
    }

    public override void OnJoinedRoom()
    {
        RoomPanel.SetActive(true);
        RoomRenewal();
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }
    //룸을 만드는데 실패하게 된다면(방이 중복되는 이름일 경우 실패할 것이다.) 방의 이름을 지우고 랜덤한 이름으로 재생성한다.
    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }
    //들어가는데 실패할 경우 랜덤한 방을 생성한다.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
        PV.RPC("InNickRPC", RpcTarget.AllBuffered, newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
        PV.RPC("OutNickRPC", RpcTarget.AllBuffered, otherPlayer.NickName);
    }

    void RoomRenewal()
    {
        RoomName.text = PhotonNetwork.CurrentRoom.Name;
        RoomNum.text = PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";
        //로비에 있는 사람들의 숫자와 닉네임과 방을 초기화하여 동기화 시켜준다.
    }


    [PunRPC]
    void InNickRPC(string nick)
    {
        for(int i=0;i<RoomNickSpace.Length;i++)
            if(RoomNickSpace[i].text == "")
            {
                RoomNickSpace[i].text = nick;
                break;
            }
    }

    [PunRPC]
    void OutNickRPC(string nick)
    {
        for(int i=0;i<RoomNickSpace.Length;i++)
        {
            if(RoomNickSpace[i].text == nick)
            {
                RoomNickSpace[i].text = "";
                break;
            }
        }
    }
    #endregion


    #region 채팅
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
    #endregion
}
