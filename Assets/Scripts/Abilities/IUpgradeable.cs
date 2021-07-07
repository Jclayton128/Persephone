using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IUpgradeable
{
    void UpgradeAbility();

    Sprite ReturnNewAbilityIcon();
}
