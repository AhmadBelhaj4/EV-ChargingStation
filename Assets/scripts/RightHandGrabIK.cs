using UnityEngine;

public class RightHandGrabIK : MonoBehaviour
{
    public Animator animator;

    [Header("IK")]
    public bool handActive = false;
    public Transform handTarget;
    public float ikWeight = 1f;
    public Vector3 handOffset = Vector3.zero;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        if (handActive && handTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);

            Vector3 targetPos = handTarget.position + handOffset;
            Quaternion targetRot = handTarget.rotation;

            animator.SetIKPosition(AvatarIKGoal.RightHand, targetPos);
            animator.SetIKRotation(AvatarIKGoal.RightHand, targetRot);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
        }
    }

    public void SetTarget(Transform target)
    {
        handTarget = target;
        handActive = target != null;
    }

    public void ClearTarget()
    {
        handTarget = null;
        handActive = false;
    }
}