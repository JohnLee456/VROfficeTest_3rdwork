using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System.Collections;
using UnityEngine.Events;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.XR.Interaction.Toolkit;

namespace ChiliGames.VROffice
{
    //This script handles the different modes: VR or Screen (pc, tablet, phone, etc)
    public class PlatformManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] GameObject vrRig;
        public GameObject screenRig;
        [SerializeField] Transform[] startingPositions;
        [SerializeField] Transform observerPositions;
        [SerializeField] TeleportationArea floor;

        [SerializeField] private GameObject vrBody;
        private VRBody localVrBody;

        [HideInInspector] public Transform[] vrRigParts;
        public Transform[] screenRigParts;

        [SerializeField] GameObject screenBody;

        //Seats
        Hashtable h = new Hashtable();
        bool initialized;
        bool seated;
        int actorNum;
        string nickName;

        //Modes
        public enum Mode { VR, Screen };
        [Tooltip("Choose the mode before building")]
        public Mode mode;

        //For setting color in VRBody.cs
        public UnityEvent onSpawned;
        [HideInInspector] public int spawnPosIndex;

        //Singleton to access this script from everywhere.
        public static PlatformManager instance;

        private bool IsLocalLoginScene
        {
            get { return SceneManager.GetActiveScene().name == "OfficeLoggedIn"; }
        }

        void Awake()
        {
            //If not connected go to lobby to connect
            if (!PhotonNetwork.IsConnected)
            {
                if (!IsLocalLoginScene)
                {
                    SceneManager.LoadScene(0);
                    return;
                }

                Debug.Log("OfficeLoggedIn opened without Photon connection for local login test.");
            }

            instance = this;
            actorNum = PhotonNetwork.IsConnected ? PhotonNetwork.LocalPlayer.ActorNumber : 0;
            nickName = PhotonNetwork.IsConnected ? PhotonNetwork.LocalPlayer.NickName : PlayerPrefs.GetString("LobbyUserName", "LocalUser");
            Debug.Log("User's nickname:" + nickName + ".");

            //If student connecting from phone, limit the fps to save battery. Also avoid sleep.
            if (mode == Mode.Screen)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 30;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            vrRigParts = new Transform[3];
            vrRigParts[0] = vrRig.transform.GetChild(0).GetChild(0); //Set camera
            vrRigParts[1] = vrRig.transform.GetChild(0).GetChild(1); //Set left hand
            vrRigParts[2] = vrRig.transform.GetChild(0).GetChild(2); //Set right hand

            floor.teleportationProvider = vrRig.GetComponent<TeleportationProvider>();
        }

        private void Start()
        {
            if (!PhotonNetwork.IsConnected && IsLocalLoginScene)
            {
                StartLocalLoginMode();
                return;
            }

            //if this is the first player to connect, initialize the students list
            if (PhotonNetwork.IsMasterClient && !initialized)
            {
                InitializePositionsList();
            }  
            if (mode == Mode.VR)
            {
                vrRig.SetActive(true);
                //todo：如果是observer将vrbody不可见
                CreateVRBody();
                if (PhotonNetwork.CurrentRoom.CustomProperties["Initialized"] != null)
                {
                    if(nickName == "observer")
                    {
                        SetPosition(-2);
                    }
                    else
                    {
                        SetPosition(GetFreePosition());
                    }
                }
            }
            //if it's a student, create it's body and sit in right position if the student list already exists
            else if (mode == Mode.Screen)
            {
                screenRig.SetActive(true);
                CreateScreenBody();
                if (PhotonNetwork.CurrentRoom.CustomProperties["Initialized"] != null)
                {
                    SetPosition(GetFreePosition());
                }
            }
        }

        private void StartLocalLoginMode()
        {
            if (mode == Mode.VR)
            {
                vrRig.SetActive(true);
                if (observerPositions != null)
                {
                    vrRig.transform.position = observerPositions.position;
                    vrRig.transform.rotation = observerPositions.rotation;
                }
                else if (startingPositions != null && startingPositions.Length > 0)
                {
                    vrRig.transform.position = startingPositions[0].position;
                    vrRig.transform.rotation = startingPositions[0].rotation;
                }
            }
            else if (mode == Mode.Screen)
            {
                screenRig.SetActive(true);
                if (startingPositions != null && startingPositions.Length > 0)
                {
                    screenRig.transform.position = startingPositions[0].position;
                    screenRig.transform.rotation = startingPositions[0].rotation;
                }
            }

            initialized = true;
            seated = true;
            Debug.Log("OfficeLoggedIn local login mode initialized.");
        }

        void CreateVRBody()
        {
            localVrBody = PhotonNetwork.Instantiate(vrBody.name, transform.position, transform.rotation).GetComponent<VRBody>();
        }

        //暂时没被引用
        public void SetMaleAvatar()
        {
            photonView.RPC("ChangeTeacherAvatar", RpcTarget.AllBuffered, "male");
            localVrBody.GetComponent<PhotonView>().RPC("SetAvatarFollow", RpcTarget.AllBuffered);
        }

        //暂时没被引用
        public void SetFemaleAvatar()
        {
            photonView.RPC("ChangeTeacherAvatar", RpcTarget.AllBuffered, "female");
            localVrBody.GetComponent<PhotonView>().RPC("SetAvatarFollow", RpcTarget.AllBuffered);
        }

        void CreateScreenBody()
        {
            PhotonNetwork.Instantiate(screenBody.name, transform.position, transform.rotation);
        }

        public void TeleportEffect()
        {
            if(localVrBody != null)
            {
                localVrBody.GetComponent<PhotonView>().RPC("RPC_TeleportEffect", RpcTarget.Others);
            }
        }


        //So we stop loading scenes if we quit app
        private void OnApplicationQuit()
        {
            StopAllCoroutines();
        }

        //This creates an empty list of positions matching the number of spawn positions
        void InitializePositionsList()
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties["Initialized"] == null)
            {
                h.Add("Initialized", actorNum);
                for (int i = 0; i < startingPositions.Length; i++)
                {
                    h.Add("" + i, 0);
                }

                //增加观察者在哈希表中的键值对
                h.Add("observer", 0);
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(h);
        }

        //Gets the first sit that is free (that has a value of 0)
        int GetFreePosition()
        {
            for (int i = 0; i < startingPositions.Length; i++)
            {
                if ((int)PhotonNetwork.CurrentRoom.CustomProperties["" + i] == 0)
                {
                    return i;
                }
            }
            return -1;
        }


        //Puts the user in the correspondant spawning position
        void SetPosition(int n)
        {
            if (n == -1)
            {
                Debug.LogError("No positions available");
            }
            if (n == -2)
            {
                vrRig.transform.position = observerPositions.position;
                vrRig.transform.rotation = observerPositions.rotation;
                seated = true;

                h["observer"] = actorNum;
                PhotonNetwork.CurrentRoom.SetCustomProperties(h);

            }
            else
            {
                Debug.Log("Spawning user in position number: " + n);
                if (mode == Mode.VR)
                {
                    vrRig.transform.position = startingPositions[n].position;
                    vrRig.transform.rotation = startingPositions[n].rotation;
                }
                else if (mode == Mode.Screen)
                {
                    screenRig.transform.position = startingPositions[n].position;
                    screenRig.transform.rotation = startingPositions[n].rotation;
                }
                seated = true;

                //Store in room properties what actor number was set in the position
                h["" + n] = actorNum;
                PhotonNetwork.CurrentRoom.SetCustomProperties(h);

                spawnPosIndex = n;
                onSpawned.Invoke();
            }
        }

        //This is called when the room properties are updated, for example, when the positions list is created
        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            base.OnRoomPropertiesUpdate(propertiesThatChanged);

            if (propertiesThatChanged.ContainsKey("Initialized") && !initialized)
            {
                Debug.Log("Positions list initialized");
                initialized = true;

                if (!seated)
                {   
                    if(nickName == "observer")
                    {
                        SetPosition(-2);
                    }
                    else
                    {
                        SetPosition(GetFreePosition());
                    }               
                }
            }
        }

        //This is called when a player leaves the room, so we can free the student's place
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            //get the seat number of plaer that left the room, and update room properties with the free seat (value to 0)
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < startingPositions.Length; i++)
                {
                    if ((int)PhotonNetwork.CurrentRoom.CustomProperties["" + i] == otherPlayer.ActorNumber)
                    {
                        h["" + i] = 0;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(h);
                        Debug.Log("User " + otherPlayer.ActorNumber + " left room, freeing up seat " + i);
                        return;
                    }
                }
                if ((int)PhotonNetwork.CurrentRoom.CustomProperties["observer"] == otherPlayer.ActorNumber)
                {
                    h["observer"] = 0;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(h);
                    Debug.Log("User " + otherPlayer.ActorNumber + " left room, freeing up seat observer");
                    return;
                }
            }
        }

        //If the new master client is this client, get a copy of the room properties
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);
            if (newMasterClient.ActorNumber == actorNum)
            {
                h = PhotonNetwork.CurrentRoom.CustomProperties;
            }
        }

        //If disconnected from server, return to lobby to reconnect.
        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            GoToScene(0);
        }

        //Class to load scenes async
        void GoToScene(int n)
        {
            StartCoroutine(LoadScene(n));
        }

        IEnumerator LoadScene(int n)
        {
            yield return new WaitForSeconds(0.5f);

            AsyncOperation async = SceneManager.LoadSceneAsync(n);
            async.allowSceneActivation = false;

            yield return new WaitForSeconds(1);
            async.allowSceneActivation = true;
            if (n == 0) //if going back to menu destroy instance
            {
                Destroy(gameObject);
            }
        }
    }
}
