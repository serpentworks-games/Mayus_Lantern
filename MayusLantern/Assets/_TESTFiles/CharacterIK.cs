using UnityEngine;

public class CharacterIK : MonoBehaviour
{
    public Transform lookAtTarget;

    Animator anim;

    //Foot IK
    Transform leftFoot, rightFoot;
    Vector3 leftFoot_Pos, rightFoot_Pos;
    Quaternion leftFoot_Rot, rightFoot_Rot;
    float leftFoot_Weight, rightFoot_Weight;

    //Hand IK
    Transform leftShoulder, rightShoulder;
    Vector3 leftHand_Pos, rightHand_Pos;
    Quaternion leftHand_Rot, rightHand_Rot;
    float leftHand_Weight, rightHand_Weight;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        leftShoulder = anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
        rightShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        HandleFootIK();
        HandleHandIK();
        HandleHeadIK();
    }

    void HandleFootIK()
    {
        leftFoot_Weight = anim.GetFloat("LeftFoot");
        rightFoot_Weight = anim.GetFloat("RightFoot");

        FindFloorPositions(leftFoot, ref leftFoot_Pos, ref leftFoot_Rot, Vector3.up);
        FindFloorPositions(rightFoot, ref rightFoot_Pos, ref rightFoot_Rot, Vector3.up);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFoot_Weight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFoot_Weight);

        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFoot_Weight);
        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFoot_Weight);

        anim.SetIKPosition(AvatarIKGoal.LeftFoot, leftFoot_Pos);
        anim.SetIKRotation(AvatarIKGoal.LeftFoot, leftFoot_Rot);

        anim.SetIKPosition(AvatarIKGoal.RightFoot, rightFoot_Pos);
        anim.SetIKRotation(AvatarIKGoal.RightFoot, rightFoot_Rot);
    }

    void HandleHandIK()
    {

        FindFloorPositions(leftShoulder, ref leftHand_Pos, ref leftHand_Rot, -transform.forward);
        FindFloorPositions(rightShoulder, ref rightHand_Pos, ref rightHand_Rot, -transform.forward);

        float distanceRightArmObject = Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.RightShoulder).position, rightHand_Pos);
        float distanceLeftArmObject = Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.LeftShoulder).position, leftFoot_Pos);

        leftHand_Weight = Mathf.Clamp01(1 - distanceLeftArmObject);
        rightHand_Weight = Mathf.Clamp01(1 - distanceRightArmObject);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHand_Weight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHand_Weight);

        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHand_Weight);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHand_Weight);

        anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHand_Pos);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHand_Rot);

        anim.SetIKPosition(AvatarIKGoal.RightHand, rightHand_Pos);
        anim.SetIKRotation(AvatarIKGoal.RightHand, rightHand_Rot);
    }

    void HandleHeadIK()
    {
        if (lookAtTarget != null)
        {
            float distanceFaceObject = Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.Head).position, lookAtTarget.position);

            anim.SetLookAtPosition(lookAtTarget.position);

            anim.SetLookAtWeight(Mathf.Clamp01(2 - distanceFaceObject), Mathf.Clamp01(1 - distanceFaceObject));
        }
    }

    void FindFloorPositions(Transform transform, ref Vector3 targetPos, ref Quaternion targetRot, Vector3 direction)
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        rayOrigin += direction * 0.3f;

        Debug.DrawRay(rayOrigin, -direction, Color.green);
        if (Physics.Raycast(rayOrigin, -direction, out hit, 3))
        {
            targetPos = hit.point;
            Quaternion rot = Quaternion.LookRotation(transform.forward);
            targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * rot;
        }
    }
}