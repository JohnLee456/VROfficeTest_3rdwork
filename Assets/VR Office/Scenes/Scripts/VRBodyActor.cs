using UnityEngine;
/*using Photon.Pun;*/
using System.Collections;
/*using Photon.Realtime;*/
using TMPro;

namespace ChiliGames.VROffice
{
    //This script is attached to the VR body, to ensure each part is following the correct tracker. This is done only if the body is owned by the player
    //and replicated around the network with the Photon Transform View component
    public class VRBodyActor: MonoBehaviour
    {
        public Transform[] body;
        [SerializeField] SkinnedMeshRenderer lHand;
        [SerializeField] SkinnedMeshRenderer rHand;
        [SerializeField] SkinnedMeshRenderer bodyRenderer;
        [SerializeField] GameObject vrRig;
        [HideInInspector] public Transform[] vrRigParts;

        public TextMeshProUGUI idLabel;

        private Color playerColor;
/*
        PhotonView pv;
*/
        private void Awake()
        {
            /*         pv = GetComponent<PhotonView>();*/
            vrRigParts = new Transform[3];
            vrRigParts[0] = vrRig.transform.GetChild(0).GetChild(0); //Set camera
            vrRigParts[1] = vrRig.transform.GetChild(0).GetChild(1); //Set left hand
            vrRigParts[2] = vrRig.transform.GetChild(0).GetChild(2); //Set right hand

            //Enable hand renderers if this is my avatar.
            lHand.enabled = true;
            rHand.enabled = true;
        }

        private void Start()
        {
            //测试为模型添加id标签
/*            idLabel.text = pv.Owner.NickName;*/
        }

        // Follow trackers only if it's our body
        void Update()
        {
            for (int i = 0; i < body.Length; i++)
            {
                body[i].position = vrRigParts[i].position;
                body[i].rotation = vrRigParts[i].rotation;
            }
        }

/*        [PunRPC]
        public void RPC_TeleportEffect()
        {
            StopAllCoroutines();
            StartCoroutine(TeleportEffect());
        }*/

        //Lerps the dissolve shader to create a teleportation effect on the avatar.
        IEnumerator TeleportEffect()
        {
            float effectDuration = 0.8f;
            for (float i = 0; i < effectDuration; i += Time.deltaTime)
            {
                bodyRenderer.material.SetFloat("_CutoffHeight", Mathf.Lerp(-1, 4, i / effectDuration));
                yield return null;
            }
        }

        //For setting different colors to each player joining the room.
/*        void SetColor()
        {
            Debug.Log("Setting color " + PlatformManager.instance.spawnPosIndex);
            pv.RPC("RPC_SetColor", RpcTarget.AllBuffered, PlatformManager.instance.spawnPosIndex);
        }*/

/*        [PunRPC]
        void RPC_SetColor(int n)
        {
            n++;
            switch (n)
            {
                case 1:
                    playerColor = Color.red;
                    break;
                case 2:
                    playerColor = Color.cyan;
                    break;
                case 3:
                    playerColor = Color.green;
                    break;
                case 4:
                    playerColor = Color.yellow;
                    break;
                case 5:
                    playerColor = Color.magenta;
                    break;
                case 6:
                    playerColor = Color.blue;
                    break;
                case 7:
                    playerColor = Color.Lerp(Color.yellow, Color.red, 0.5f);
                    break;
                case 8:
                    playerColor = Color.Lerp(Color.blue, Color.red, 0.5f);
                    break;
                case 9:
                    playerColor = Color.Lerp(Color.red, Color.green, 0.5f);
                    break;
                default:
                    playerColor = Color.black;
                    break;
            }
            playerColor = Color.Lerp(Color.white, playerColor, 0.5f);

            //Set body and hands color.
            bodyRenderer.material.SetColor("_Albedo", playerColor);
            lHand.material.SetColor("_BaseColor", playerColor);
            rHand.material.SetColor("_BaseColor", playerColor);
        }*/
    }
}
