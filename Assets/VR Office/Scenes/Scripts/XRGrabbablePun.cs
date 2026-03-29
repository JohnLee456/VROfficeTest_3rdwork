using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

namespace ChiliGames
{
    public class XRGrabbablePun : XRGrabInteractable
    {
        PhotonView pv;
        bool wasKinematic;
        protected override void Awake()
        {
            base.Awake();
            pv = GetComponent<PhotonView>();
            wasKinematic = GetComponent<Rigidbody>().isKinematic;
        }

        //protected override void OnSelectEntered(XRBaseInteractor interactor)
        //{
        //    pv.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
        //    base.OnSelectEntered(interactor);
        //    pv.RPC("SetKinematic", RpcTarget.OthersBuffered, true);
        //}

        //protected override void OnSelectExited(XRBaseInteractor interactor)
        //{
        //    base.OnSelectExited(interactor);
        //    pv.RPC("SetKinematic", RpcTarget.OthersBuffered, wasKinematic);
        //}


        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            pv.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
            base.OnSelectEntered(args);
            pv.RPC("SetKinematic", RpcTarget.OthersBuffered, true);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            pv.RPC("SetKinematic", RpcTarget.OthersBuffered, wasKinematic);
        }

        [PunRPC]
        public void SetKinematic(bool state)
        {
            GetComponent<Rigidbody>().isKinematic = state;
        }
    }
}

