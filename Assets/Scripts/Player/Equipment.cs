using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Equipment : MonoBehaviour
{
    public Equip curEquip;
    public Transform equipParent;

    private PlayerController controller;
    private PlayerCondition condition;

    private float originalSpeed;
    private int originalJumpCount;

    void Start()
    {
        controller = CharacterManager.Instance.Player.controller;
        condition = CharacterManager.Instance.Player.condition;

        originalSpeed = controller.moveSpeed;
        originalJumpCount = controller.maxJumpCount;
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed && curEquip != null && controller.canLook)
        {
            curEquip.OnAttackInput();
        }
    }

    public void EquipNew(ItemData data)
    {
        UnEquip();

        curEquip = Instantiate(data.equipPrefab, equipParent).GetComponent<Equip>();

        controller.SetMoveSpeed(controller.originalMoveSpeed + data.speedIncrease);
        controller.maxJumpCount += data.jumpCountIncrease;
    }

    public void UnEquip()
    {
        if (curEquip != null)
        {
            controller.SetMoveSpeed(controller.originalMoveSpeed);
            controller.maxJumpCount = originalJumpCount;

            Destroy(curEquip.gameObject);
            curEquip = null;
        }
    }
}
