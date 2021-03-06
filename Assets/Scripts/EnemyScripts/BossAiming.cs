using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAiming : MonoBehaviour
{

    public Transform player;
    private Rigidbody2D rb;
    private bool facingDir = true;
    private float aimAngle;
    private EnemyColony Colony;

    public Animator WeaponAnim;

    [HideInInspector]
    public float AimDir = 0;
    private float angle;
    //public bool isNotEnemy1 = false;
    //public bool isNotHoldingWeapon = false;
    // Start is called before the first frame update


    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody2D>();
        //if (GlobalPlayerVariables.GameOver == false)
        //   player = GameObject.FindGameObjectWithTag("Player").transform;
        
        Colony = transform.parent.GetComponent<EnemyColony>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Colony.player != null)
            player = Colony.player;
        if (GlobalPlayerVariables.GameOver == false && player != null)
        {
            Vector2 direction = player.position - transform.position;

            if (Colony.lineofsight == true)
                angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            else 
            {
                Vector2 dir = Colony.rb.velocity;
                angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }

            
            transform.eulerAngles = new Vector3(0, 0, angle);
            AimDir = angle;
            Vector3 aimLocalScale = Vector3.one;
            if (angle > 90 || angle < -90)
            {
                //transform.position.y *= -1;
                aimLocalScale.y = -1f;
                transform.position = new Vector3(transform.position.x, transform.position.y, 1f);
                //aimLocalScale.y = -1f * scaleX;
            }
            else
            {
                aimLocalScale.y = +1f;
                //aimLocalScale.y = -1f * scaleX;
                transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
            }
            transform.localScale = aimLocalScale;
        }
    }
}
