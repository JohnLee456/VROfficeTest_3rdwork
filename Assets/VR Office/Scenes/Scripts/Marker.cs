using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

namespace ChiliGames.VROffice
{
    //This class sends a Raycast from the marker and detect if it's hitting the whiteboard (tag: Finish)
    [RequireComponent(typeof(XRGrabbablePun))]
    public class Marker : MonoBehaviour
    {
        private Whiteboard whiteboard;
        [SerializeField] private Transform drawingPoint;
        [SerializeField] private Renderer markerTip;
        private RaycastHit touch;
        private bool touching;
        private bool firstTimeTouching = true;
        private float drawingDistance = 0.015f;
        private Quaternion lastAngle;
        private PhotonView pv;
        private XRGrabbablePun grabbable;
        [SerializeField] private int penSize = 6;
        [SerializeField] Color32 color = Color.blue;
        private bool grabbed;
        private int currentFrame = 0;


        private void Awake() {
            pv = GetComponent<PhotonView>();
            grabbable = GetComponent<XRGrabbablePun>();
        }

        private void Start()
        {
            //Subscribe to grabbed and dropped events
            grabbable.selectEntered.AddListener(MarkerGrabbed);
            grabbable.selectExited.AddListener(MarkerDropped);

            var block = new MaterialPropertyBlock();

            //Set the color from inspector to marker tip
            block.SetColor("_BaseColor", color);
            markerTip.SetPropertyBlock(block);
        }

        private void MarkerGrabbed(SelectEnterEventArgs arg0) {
            grabbed = true;
        }

        private void MarkerDropped(SelectExitEventArgs arg0) {
            grabbed = false;
        }

        void Update()
        {
            //Run raycast every 2 frames for performance
            currentFrame++;
            if (currentFrame % 2 != 0)
            {
                currentFrame = 0;
            }

            //if the marker is not in possesion of the user, or is not grabbed, we don't run update.
            if (!pv.IsMine) return;
            if (!grabbed) return;

            //Cast a raycast to detect whiteboard.
            if (Physics.Raycast(drawingPoint.position, drawingPoint.up, out touch, drawingDistance))
            {
                //The whiteboard has the tag "Finish".
                if (touch.collider.CompareTag("Finish"))
                {
                    if (!touching)
                    {
                        touching = true;
                        //store angle so while drawing, marker doesn't rotate
                        lastAngle = transform.rotation;
                        whiteboard = touch.collider.GetComponent<Whiteboard>();
                    }
                    if (whiteboard == null) return;
                    //Save reference of marker ID, color and size the first time we touch the whiteboard
                    if (firstTimeTouching)
                    {
                        whiteboard.photonView.RPC("RPC_StoreMarkerID", RpcTarget.AllBuffered, pv.ViewID, penSize, new float[] { color.r, color.g, color.b });
                        firstTimeTouching = false;
                    }

                    //Only send the RPC every 4 frames to optimize
                    whiteboard.DrawAtPosition(pv.ViewID, 1-touch.textureCoord.x, 1-touch.textureCoord.y);
                    if (currentFrame % 4 == 0)
                    {
                        whiteboard.photonView.RPC("DrawAtPosition", RpcTarget.OthersBuffered, pv.ViewID, 1-touch.textureCoord.x, 1-touch.textureCoord.y);
                        //reset frame counter every 4 frames so we dont hit the int max value
                        currentFrame = 0;
                    }
                }
            }
            else if (whiteboard != null)
            {
                touching = false;
                whiteboard.photonView.RPC("ResetTouch", RpcTarget.AllBuffered, pv.ViewID);
                whiteboard = null;
            }
        }

        private void OnDestroy() {
            if(grabbable != null) {
                grabbable.selectEntered.RemoveListener(MarkerGrabbed);
                grabbable.selectExited.RemoveListener(MarkerDropped);
            }
        }

        private void LateUpdate()
        {
            if (!pv.IsMine) return;

            //lock rotation of marker when touching whiteboard.
            if (touching)
            {
                transform.rotation = lastAngle;
            }
        }
    }
}