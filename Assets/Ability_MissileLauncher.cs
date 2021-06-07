using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ability_MissileLauncher : Ability
{
    //init
    [SerializeField] AudioClip missileFiringSound = null;
    PersNetworkManager pnm;

    protected override void Start()
    {
        base.Start();
        if (isServer)
        {
            pnm = FindObjectOfType<PersNetworkManager>();
        }
    }
    private Vector2 GetMouseCursorLocation()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        Vector2 flatPos = worldPosition;
        return flatPos;
    }
    public override void MouseClickDown()
    {
        //Check for sufficient Power on client-side
        CmdRequestFireMissile();
    }

    [Command]
    private void CmdRequestFireMissile()
    {
        //Check for sufficient power on server-side
        //Decrement Power Source
        GameObject missile = Instantiate(weaponPrefab, transform.position, transform.rotation) as GameObject;
        missile.layer = 9;
        Missile_AI missileAI = missile.GetComponent<Missile_AI>();
        missileAI.SetNavTarget(GetMouseCursorLocation());
        DamageDealer dd = missile.GetComponent<DamageDealer>();
        dd.SetDamage(hullDamage);
        dd.SetSafeObject(gameObject);
        dd.IsReal = true;
        missileAI.normalSpeed = weaponSpeed;
        missileAI.SetMissileOwner(gameObject);
        missileAI.SetLifetime(weaponLifetime);

        NetworkServer.Spawn(missile); //This isn't spawning things back on the client sides!! :(


        //AudioSource.PlayClipAtPoint(missileFiringSound, gameObject.transform.position);
    }

    public override void MouseClickUp()
    {
        throw new System.NotImplementedException();
    }
}
