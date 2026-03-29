using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace ChiliGames.VROffice
{
    public class Whiteboard : MonoBehaviourPun
    {
        private int maxTextureSize = 1200;
        private int whiteBoardSizeX;
        private int whiteBoardSizeY;
        private Texture2D texture;
        private Color32[] deleteColor;
        private new Renderer renderer;
        private float lastX, lastY;
        private bool everyOthrFrame;
        private Dictionary<int, MarkerData> markerIDs = new Dictionary<int, MarkerData>();

        private int lastLerpX;
        private int lastLerpY;

        class MarkerData
        {
            public Color32[] color;
            public bool touchingLastFrame;
            public float posX;
            public float posY;
            public int pensize;
            public int pensizeD2;
            public float lastX;
            public float lastY;
        }

        void Awake()
        {
            //Dertermine whiteboard resolution from size.
            if (transform.localScale.x > transform.localScale.y)
            {
                float ratio = transform.localScale.x / transform.localScale.y;
                whiteBoardSizeX = maxTextureSize;
                whiteBoardSizeY = (int)(maxTextureSize / ratio);
            }
            else
            {
                float ratio = transform.localScale.y / transform.localScale.x;
                whiteBoardSizeY = maxTextureSize;
                whiteBoardSizeX = (int)(maxTextureSize / ratio);
            }

            //create whiteboard texture
            renderer = GetComponent<Renderer>();
            texture = new Texture2D(whiteBoardSizeX, whiteBoardSizeY, TextureFormat.RGB24, false);

            //Apply whiteboard texture
            renderer.material.mainTexture = texture;

            //Create white delete color
            deleteColor = Enumerable.Repeat(new Color32(255, 255, 255, 255),
                whiteBoardSizeX * whiteBoardSizeY).ToArray();

            //Set the whiteboard to white on awake
            RPC_ClearWhiteboard();
        }

        //RPC sent by the Marker class so every user gets the information to draw in whiteboard.
        [PunRPC]
        public void DrawAtPosition(int id, float _posX, float _posY)
        {
            if (markerIDs.ContainsKey(id))
            {
                markerIDs[id].posX = _posX;
                markerIDs[id].posY = _posY;
            }
            else
            {
                return;
            }

            int x = (int)(markerIDs[id].posX * whiteBoardSizeX - markerIDs[id].pensizeD2);
            int y = (int)(markerIDs[id].posY * whiteBoardSizeY - markerIDs[id].pensizeD2);

            //If last frame was not touching a marker, we don't need to lerp from last pixel coordinate to new, so we set the last coordinates to the new.
            if (!markerIDs[id].touchingLastFrame)
            {
                lastX = (float)x;
                lastY = (float)y;
                lastLerpX = x;
                lastLerpY = y;
                markerIDs[id].touchingLastFrame = true;
            }

            if (markerIDs[id].touchingLastFrame)
            {
                //Lerp last pixel to new pixel, so we draw a continuous line.
                for (float t = 0.01f; t < 1.00f; t += 0.1f)
                {
                    int lerpX = (int)Mathf.Lerp(lastX, (float)x, t);
                    int lerpY = (int)Mathf.Lerp(lastY, (float)y, t);
                    if(NotTooClose(markerIDs[id].pensizeD2, lerpX, lastLerpX, lerpY, lastLerpY))
                    {
                        texture.SetPixels32(lerpX, lerpY, markerIDs[id].pensize, markerIDs[id].pensize, markerIDs[id].color);
                        lastLerpX = lerpX;
                        lastLerpY = lerpY;
                    }
                }

                if (NotTooClose(markerIDs[id].pensizeD2, x, (int)lastX, y, (int)lastY))
                {
                    texture.SetPixels32(x, y, markerIDs[id].pensize, markerIDs[id].pensize, markerIDs[id].color);
                }

                //We apply the texture every other frame, so we improve performance.
                if (!everyOthrFrame)
                {
                    everyOthrFrame = true;
                }
                else if (everyOthrFrame)
                {
                    texture.Apply();
                    everyOthrFrame = false;
                }
            }

            lastX = (float)x;
            lastY = (float)y;
        }
        private bool NotTooClose(int range, int x1, int x2, int y1, int y2)
        {
            var dx = x1 - x2;
            var dy = y1 - y2;
            return (dx * dx) + (dy * dy) > (range * range);
        }

        //Reset the state of the marker, so it doesn't interpolate/lerp last pixels drawn.
        [PunRPC]
        public void ResetTouch(int id)
        {
            markerIDs[id].touchingLastFrame = false;
        }

        //To clear the whiteboard.
        public void ClearWhiteboard()
        {
            photonView.RPC("RPC_ClearWhiteboard", RpcTarget.AllBuffered);
        }

        [PunRPC]
        public void RPC_StoreMarkerID(int id, int _pensize, float[] _color)
        {
            if (!markerIDs.ContainsKey(id))
            {
                markerIDs.Add(id, new MarkerData { touchingLastFrame = false, pensize = _pensize, pensizeD2 = _pensize/2 }); ;
                markerIDs[id].color = SetColor(new Color(_color[0], _color[1], _color[2]), id);
            }
        }

        public Color32[] SetColor(Color32 color, int id)
        {
            return Enumerable.Repeat(new Color32(color.r, color.g, color.b, 255), markerIDs[id].pensize * markerIDs[id].pensize).ToArray();
        }

        [PunRPC]
        public void RPC_ClearWhiteboard()
        {
            texture.SetPixels32(deleteColor);
            texture.Apply();
        }
    }
}
