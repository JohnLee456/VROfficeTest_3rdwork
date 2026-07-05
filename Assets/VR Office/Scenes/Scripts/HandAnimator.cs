using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

namespace ChiliGames.VROffice
{
    public class HandAnimator : MonoBehaviour
    {
        public float speed = 5.0f;
        public XRController controller = null;
        public ActionBasedController controllerAction = null;
        [SerializeField] bool leftHand;
        PhotonView pv;
        bool isActionBasedController;

        private Animator animator = null;

        private readonly List<Finger> gripfingers = new List<Finger>()
        {
            new Finger(FingerType.Middle),
            new Finger(FingerType.Ring),
            new Finger(FingerType.Pinky)
        };

        private readonly List<Finger> pointFingers = new List<Finger>
        {
            new Finger(FingerType.Index),
            new Finger(FingerType.Thumb)
        };

        private void Start()
        {
            pv = GetComponent<PhotonView>();
            animator = GetComponent<Animator>();
            TryResolveController();
/*            if (!pv.IsMine) return;*/
        }

        private void Update()
        {
/*            if (!pv.IsMine) return;*/
            if (animator == null || !TryResolveController())
                return;

            // Store input
            CheckGrip();
            CheckPointer();

            // Smooth input values
            SmoothFinger(pointFingers);
            SmoothFinger(gripfingers);

            // Apply smoothed values
            AnimateFinger(pointFingers);
            AnimateFinger(gripfingers);
        }

        private bool TryResolveController()
        {
            if (controllerAction != null && controllerAction.selectAction.action != null)
            {
                isActionBasedController = true;
                return true;
            }

            if (controller != null)
            {
                isActionBasedController = false;
                return true;
            }

            if (PlatformManager.instance == null || PlatformManager.instance.vrRigParts == null)
                return false;

            int handIndex = leftHand ? 1 : 2;
            if (PlatformManager.instance.vrRigParts.Length <= handIndex || PlatformManager.instance.vrRigParts[handIndex] == null)
                return false;

            Transform handRoot = PlatformManager.instance.vrRigParts[handIndex];
            controllerAction = handRoot.GetComponentInChildren<ActionBasedController>();
            if (controllerAction != null && controllerAction.selectAction.action != null)
            {
                isActionBasedController = true;
                return true;
            }

            controller = handRoot.GetComponentInChildren<XRController>();
            isActionBasedController = false;
            return controller != null;
        }

        private void CheckGrip()
        {
            if (isActionBasedController)
            {
                if (controllerAction == null || controllerAction.selectAction.action == null)
                    return;

                float gripValue = controllerAction.selectAction.action.ReadValue<float>();
                SetFingerTargets(gripfingers, gripValue);
            }
            else
            {
                if (controller == null)
                    return;

                if (controller.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
                    SetFingerTargets(gripfingers, gripValue);
            }
        }

        private void CheckPointer()
        {
            if (isActionBasedController)
            {
                if (controllerAction == null || controllerAction.activateAction.action == null)
                    return;

                float triggerValue = controllerAction.activateAction.action.ReadValue<float>();
                SetFingerTargets(pointFingers, triggerValue);
            }
            else
            {
                if (controller == null)
                    return;

                if (controller.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
                    SetFingerTargets(pointFingers, triggerValue);
            }
        }

        private void SetFingerTargets(List<Finger> fingers, float value)
        {
            foreach (Finger finger in fingers)
                finger.target = value;
        }

        private void SmoothFinger(List<Finger> fingers)
        {
            foreach(Finger finger in fingers)
            {
                float time = speed * Time.unscaledDeltaTime;
                finger.current = Mathf.MoveTowards(finger.current, finger.target, time);
            }
        }

        private void AnimateFinger(List<Finger> fingers)
        {
            foreach (Finger finger in fingers)
                AnimateFinger(finger.type.ToString(), finger.current);
        }

        private void AnimateFinger(string finger, float blend)
        {
            animator.SetFloat(finger, blend);
        }
    }
}
