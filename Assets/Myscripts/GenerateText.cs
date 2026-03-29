using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ChiliGames;
using UnityEngine.XR.Interaction.Toolkit;

public class GenerateText : MonoBehaviour, IPunInstantiateMagicCallback
{
    private SimpleHelvetica simpleHelvetica;
    private XRGrabbablePun xrGrabbablePun;
    private Transform attach; 


    private void Awake()
    {
        simpleHelvetica = gameObject.GetComponent<SimpleHelvetica>();
        attach = gameObject.transform.Find("Attach");
        attach.gameObject.SetActive(false);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        simpleHelvetica.Text = (string)instantiationData[0];
        simpleHelvetica.Text = simpleHelvetica.Text.ToUpper();
        simpleHelvetica.GenerateText();
        ChengeAttachTran();
        xrGrabbablePun = gameObject.AddComponent<XRGrabbablePun>();
        xrGrabbablePun.interactionManager = GameObject.Find("XR Interaction Manager").GetComponent<XRInteractionManager>();
        xrGrabbablePun.attachTransform = attach.transform;
    }

    public void ChengeAttachTran()
    {
        Transform parent = this.transform;
        Vector3 position = parent.position;
        Quaternion rotation = parent.rotation;
        Vector3 scale = parent.localScale;
        parent.position = Vector3.zero;
        parent.rotation = Quaternion.Euler(Vector3.zero);
        parent.localScale = Vector3.one;

        float center = 0;
        Transform[] transforms = gameObject.transform.GetComponentsInChildren<Transform>(false);
        foreach (Transform child in transforms)
        {
            center += child.localPosition.x;
        }

        center /= gameObject.GetComponentsInChildren<Transform>(false).Length - 1;
        attach.position = new Vector3(center, 0, 0);

        parent.position = position;
        parent.rotation = rotation;
        parent.localScale = scale;
    }

    //public void ChengeCollider()
    //{
    //    Transform parent = this.transform;
    //    Vector3 position = parent.position;
    //    Quaternion rotation = parent.rotation;
    //    Vector3 scale = parent.localScale;
    //    parent.position = Vector3.zero;
    //    parent.rotation = Quaternion.Euler(Vector3.zero);
    //    parent.localScale = Vector3.one;

    //    Collider[] colliders = parent.GetComponentsInChildren<Collider>();
    //    foreach(Collider child in colliders)
    //    {
    //        DestroyImmediate(child);
    //    }

    //    Vector3 center = Vector3.zero;
    //    Renderer[] renders = parent.GetComponentsInChildren<Renderer>();
    //    foreach(Renderer child in renders)
    //    {
    //        center += child.bounds.center;
    //    }

    //    center /= parent.GetComponentsInChildren<Renderer>().Length;

    //    Bounds bounds = new Bounds(center, Vector3.zero);
    //    foreach(Renderer child in renders)
    //    {
    //        bounds.Encapsulate(child.bounds);
    //    }

    //    BoxCollider boxCollider = parent.gameObject.AddComponent<BoxCollider>();
    //    boxCollider.center = bounds.center;
    //    boxCollider.size = bounds.size;

    //    parent.position = position;
    //    parent.rotation = rotation;
    //    parent.localScale = scale;
    //}
}
