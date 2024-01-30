using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;
    public Transform Target;
    public bool isChase;

    Rigidbody rigid;
    BoxCollider boxCollider;

    Material mat;
    NavMeshAgent nav;
    Animator anim;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }
    void Update()
    {
        if(isChase)
            nav.SetDestination(Target.position);
    }
    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(onDamage(reactVec, false));

        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;


            Debug.Log("Range : " + curHealth);
            Destroy(other.gameObject);

            StartCoroutine(onDamage(reactVec, false));
        }
    }
        public void HitByGrenede(Vector3 explosionPos)
        {
            curHealth -= 100;
            Vector3 reactVec = transform.position - explosionPos;
            StartCoroutine(onDamage(reactVec, true));
        }

        IEnumerator onDamage(Vector3 reactVec, bool isGrenede)
        {
            mat.color = Color.red;
            yield return new WaitForSeconds(0.1f);

            if(curHealth > 0)
            {
                mat.color = Color.white;
            }
            else
            {
                mat.color = Color.gray;
                gameObject.layer = 12;
                isChase = false;
                nav.enabled = false;
                anim.SetTrigger("doDie");

                if (isGrenede)
                {
                    reactVec = reactVec.normalized;
                    reactVec += Vector3.up * 3;

                    rigid.freezeRotation = false;
                    rigid.AddForce(reactVec * 7, ForceMode.Impulse);
                    rigid.AddTorque(reactVec * 15, ForceMode.Impulse);

                }
                else 
                {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 7, ForceMode.Impulse);

                }

                Destroy(gameObject, 2);
                

            }
        }
    }

