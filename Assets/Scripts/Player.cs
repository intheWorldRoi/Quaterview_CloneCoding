using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenedes;
    public int hasGrenedes;
    public GameObject GrenedeObj;

    public Camera followCamera;

    public int ammo;
    public int coin;
    public int health;

    public int maxammo;
    public int maxcoin;
    public int maxhealth;
    public int maxhasGrenedes;

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool fDown;
    bool gDown;
    bool rDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isReload;
    bool isSwap;
    bool isFireReady = true;
    bool isBorder;

    GameObject nearObject;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay;

    Rigidbody rigid;
    Animator anim;
    Vector3 moveVec;
    Vector3 dodgeVec;
    // Start is called before the first frame update
    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        GetInput();
        Move();
        Turn();
        Jump();
        Grenede();
        Attack();
        Reload();
        Dodge();
        Interaction();
        Swap();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
        {
            moveVec = dodgeVec;
        }

        if (isSwap || isReload ||  !isFireReady)
            moveVec = Vector3.zero;

        if(!isBorder)
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        //#1 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        //#2 마우스에 의한 회전
        if (fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
       
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            rigid.AddForce(Vector3.up * 20, ForceMode.Impulse);
            anim.SetBool("isJump",true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
       
    }
    
    void Grenede()
    {
        if (hasGrenedes == 0)
            return;

        if(gDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 5;

                GameObject instantGrenede = Instantiate(GrenedeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenede = instantGrenede.GetComponent<Rigidbody>();
                rigidGrenede.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenede.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenedes--;
                grenedes[hasGrenedes].SetActive(false);
            }
        }
    }
    void Attack()
    {
        if (equipWeapon == null)
            return;
        if (equipWeapon.type == Weapon.Type.Range && equipWeapon.curAmmo == 0)
            return;
        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");

            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null)
            return;
        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (ammo == 0)
            return;

        if(rDown && !isJump && !isDodge && !isSwap && !isReload)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = equipWeapon.maxAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void Dodge()
    {
        if(jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
    }
    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if(sDown1 || sDown2 || sDown3 && !isDodge && !isJump)
        {
            if (equipWeapon != null) equipWeapon.gameObject.SetActive(false);
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);
            if (hasWeapons[weaponIndex] == true)
            {
                weapons[weaponIndex].SetActive(true);
                equipWeaponIndex = weaponIndex;
                anim.SetTrigger("doSwap");
                isSwap = true;
                Invoke("SwapOut", 0.4f);
            }
            



        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Interaction()
    {
        if (iDown && nearObject != null && !isJump && !isDodge)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
            else if(nearObject.tag == "Item")
            {

            }
        }
    }
    
    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
        
    }
    private void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }
    private void OnCollisionEnter(Collision collision)
    {
         if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }

        Debug.Log(other.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
         if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxammo)
                        ammo = maxammo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxcoin)
                        coin = maxcoin;
                    break;
                case Item.Type.Grenede:
                    grenedes[hasGrenedes].SetActive(true);
                    hasGrenedes += item.value;
                    if (hasGrenedes > maxhasGrenedes)
                        hasGrenedes = maxhasGrenedes;
                    
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxhealth)
                        health = maxhealth;
                    break;

            }
            Destroy(other.gameObject);
        }

        
    }
}
