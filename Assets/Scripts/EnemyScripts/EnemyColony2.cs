using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;
using TMPro;

public class EnemyColony2 : MonoBehaviour
{

    public Transform target;

    public Transform EnemyTarget;

    public ParticleSystem spinRing;


    public float nextWaypointDistance = 3f;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndofPath = false;

    Seeker seeker;


    [System.Serializable]
    public struct ItemDrops
    {
        public bool isCurrency;
        public bool isAmmo;
        public GameObject[] Drops;
        public float DropPercentage;
        public int NumOfDrop;
        public int[] convertProtein()
        {
            int numOfDigits;
            if (NumOfDrop.ToString().Length >= Drops.Length)
                numOfDigits = Drops.Length;
            else
                numOfDigits = NumOfDrop.ToString().Length;

            int[] convertedArr = new int[numOfDigits];

            float multten = 1;
            for (int i = 0; i < numOfDigits; i++)
            {
                if (i < numOfDigits - 1)
                    convertedArr[i] = (Mathf.FloorToInt(NumOfDrop / multten) % 10);
                else
                    convertedArr[i] = (Mathf.FloorToInt(NumOfDrop / multten));
                multten *= 10;
            }
            return convertedArr;
        }
        public int[] convertAmmo()
        {
            int[] convertedArr = new int[3];
            convertedArr[2] = Mathf.FloorToInt(NumOfDrop / 100f);

            if ((NumOfDrop / 100f) - convertedArr[2] > 0.5)
            {
                convertedArr[1] = 1;
                convertedArr[0] = 1;
            }
            else if ((NumOfDrop / 100f) - convertedArr[2] == 0.5)
                convertedArr[1] = 1;
            else if ((NumOfDrop / 100f) - convertedArr[2] > 0 && (NumOfDrop / 100f) - convertedArr[2] < 0.5)
                convertedArr[0] = 1;

            return convertedArr;
        }
    }

    public GameObject DamageText;
    private SpriteRenderer sprite;
    public Text BossName;
    public Image HealthBar;
    public string NameOfEnemy;
    public GameObject EnemyUI;
    public float DetectRange;
    //DEATH VARIABLE
    [HideInInspector]
    public bool isDead = false;

    [Header("Enemy Stats")]

    public float contactDamage;
    public float meleeDamage;
    public float spinDamage;
    private EnemyManager enemyColony;
    private float MaxHP = 0;
    public float speed = 0;
    public int EXPWorth = 50;
    public float individualHP = 1;
    public float indivMaxHP;

    /*
    [Header("Individual HP Canvas")]
    public Image IndivHealthBar;
    public GameObject IndivEnemyUI;
    public float secondsToShowUI;
    private float UITimer = 0;
    */


    [Header("Movement Settings")]
    public float stoppingDistance = 0;
    public float retreatDistance = 0;
    public bool followPlayer;
    public bool retreat;
    public bool randomMovement;


    [Header("Special Movement/Abilities")]
    //public bool retreatToBoss = false;
    //public float retreatDist;

    public bool canDash = false;
    public bool randomDash = false;
    public float dashForce;
    public float dashBackOnHit;
    public float beginningRangeToDash;
    public float endingRangeToDash;
    private float DashTimer;
    public float timebetweenmultiDash;
    private float timebtwdashtimers;
    public float dashAroundPlayerRadius;

    public bool canDoAirStrike = false;
    public GameObject AirStrike;
    //public int amountOfAirStrikes;
    public float beginningRangeToCallAirStrike;
    public float endingRangeToCallAirStrike;
    private float AirStrikeTimer;

    private bool dashIntoPlayer = false;
    private bool inmiddleofdash = false;
    private bool inmiddleofSwing = false;
    public float swingCoolDown = 1f;
    private float swingTimer;


    [Header("Chase Settings")]
    public float time2chase;
    private float chaseInProgress;


    [Header("Line Of Sight")]
    public bool lineofsight;
    public LayerMask IgnoreMe;
    public float shootdistance;
    private float distancefromplayer;
    private float distancefromtarget;

    [Header("Random Movement Settings")]
    public float circleRadius;
    public float timeTillNextMove;
    private float NextMoveCoolDown;
    private bool reachedDestination;
    private Vector2 randPos;

    private float knockbackForce = 0;
    [Header("KnockBack Settings")]
    public float knockForcePlayerContact;
    private bool knockback = false;
    public float knockbackstartrange = 0.4f;
    public float knockbackendrange = 1.0f;
    private float knockbacktime;

    private float timeBtwShots;
    [Header("Gun Settings")]
    public float beginningrangetoshoot;
    public float endingrangetoshoot;
    private float startTimeBtwShots;

    public int AmountOfBullets;
    public float bulletSpread = 0.0f;
    public GameObject projectile;
    public Transform firePoint;
    [HideInInspector]
    public Transform player;
    [HideInInspector]
    public Transform playerStash;
    private Rigidbody2D rb;

    [Header("Burst Settings")]
    public bool burstFire;
    public float timeBtwBurst;
    private float burstTime = 0;

    public int timesToShoot;
    private int TimesShot = 0;



    [Header("Drops")]
    public ItemDrops[] Drops;


    Vector2 direction;
    float a;


    private float critRate = 0;
    private float critDMG = 0;

    private bool hideing = false;


    [Header("Reset Check Setting")]
    public float recalcshortestDist = 3f;
    private float timer2reset;

    [Header("Weapon Animator")]
    public Animator weapon;
    public PolygonCollider2D weaponHurtBox;


    private float HPgained = 0;

    public void getReferences()
    {
        BossName = transform.parent.GetComponent<RememberReference>().GetReference(0).GetComponent<Text>();
        HealthBar = transform.parent.GetComponent<RememberReference>().GetReference(1).GetComponent<Image>();
        EnemyUI = transform.parent.GetComponent<RememberReference>().GetReference(2);

        BossName.text = NameOfEnemy;
        enemyColony = transform.parent.GetComponent<EnemyManager>();

        MaxHP = enemyColony.colonyHealth;
        HealthBar.fillAmount = enemyColony.colonyHealth / MaxHP;
        EnemyUI.SetActive(false);
    }


    // Start is called before the first frame update
    void Start()
    {
        //BossName.text = NameOfEnemy;
        //enemyColony = transform.parent.GetComponent<EnemyManager>();
        //Debug.Log(enemyColony.colonyHealth);
        //MaxHP = enemyColony.colonyHealth;
        //HealthBar = GameObject.Find("EnemyHP").GetComponent<Image>();
        //BossName = GameObject.Find("BossName").GetComponent<Text>();
        sprite = transform.Find("BossSprite").GetComponent<SpriteRenderer>();
        //HealthBar.fillAmount = enemyColony.colonyHealth / MaxHP;

        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        if (GlobalPlayerVariables.GameOver == false)
        {
            //Debug.Log("SETTING PLAYER");
            player = GameObject.FindGameObjectWithTag("Player").transform;
            target = player;
        }
        else
            player = this.transform;


        playerStash = player;
        GlobalPlayerVariables.TotalEnemiesAlive += 1;
        InvokeRepeating("UpdatePath", 0f, 0.5f);
        variation();
        weapon.enabled = false;
        weaponHurtBox.enabled = false;
        //spinRing.GetComponent<ParticleSystem>();
        spinRing.Stop();
    }

    void variation()
    {
        startTimeBtwShots = Random.Range(beginningrangetoshoot, endingrangetoshoot);
        timeBtwShots = startTimeBtwShots;
    }


    private void UpdatePath()
    {
        if (seeker.IsDone() && target != null)
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }



    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }


    void Astar()
    {
        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndofPath = true;
            return;
        }
        else
        {
            reachedEndofPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;
        rb.AddForce(force);


        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
        //return;


    }



    private void FixedUpdate()
    {
        if (GlobalPlayerVariables.EnableAI && isDead == false)
        {
            if (target == null)
            {
                player = playerStash;
                chaseInProgress = 0f;
            }
            if (inmiddleofdash == true)
            {
                hurtCircle();
            }

            if (player != null && target != null)
                distancefromplayer = Vector2.Distance(rb.position, player.position);
            if (target == player)
            {
                distancefromtarget = distancefromplayer;
            }
            else if(target != null)
            {
                distancefromtarget = Vector2.Distance(rb.position, target.position);
            }




            if (distancefromtarget >= stoppingDistance || lineofsight == false)
            {
                Astar();
                //Debug.Log("Astar");
            }
            else //RANDOM MOVEMENT
            {
                if (reachedDestination == false)
                {
                    Vector2 direction = (randPos - rb.position).normalized;
                    Vector2 force = direction * speed * Time.deltaTime;
                    rb.AddForce(force);
                    //transform.position = Vector2.MoveTowards(transform.position, randPos, speed * Time.deltaTime); //need to make it where it walks in that direction
                }
                if (transform.position.x == randPos.x && transform.position.y == randPos.y)
                {
                    reachedDestination = true;
                }
            }

        }
    }

    void getDirection(Transform objectpos)
    {
        if (player != null)
            direction = objectpos.position - transform.position;
        a = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }


    void randomDashPos(bool goForPlayer)
    {
        //NextMoveCoolDown = Random.Range(0f, timeTillNextMove);

        if (target != null)
            randPos = target.position;
        else
            randPos = transform.position;

        randPos += Random.insideUnitCircle * dashAroundPlayerRadius;

        Vector3 newpos = new Vector3(randPos.x, randPos.y, 0);
        //Vector3 zfix = new Vector3(randPos.x, randPos.y, 0);
        //randPos = zfix;
        reachedDestination = false;
        Vector2 direction = Vector2.zero;
        //RaycastHit2D hit2 = Physics2D.Raycast(transform.position, newpos - transform.position, Mathf.Infinity, ~IgnoreMe);
        if (lineofsight == true /*&& !hit2.collider.gameObject.CompareTag("Walls")*/ && goForPlayer == false)
        {
            //dash around player
            direction = (newpos - transform.position).normalized;
            /*
            if (target != null && randomDash == false || target != null && distancefromplayer >= shootdistance / 2)
                direction = (target.position - transform.position).normalized;
            else if (target != null && randomDash == true)
            {
                //randomDashPos();
                newpos = new Vector3(randPos.x, randPos.y, 0);
                direction = (newpos - transform.position).normalized;
            }
            */
        }
        else if (goForPlayer == true && lineofsight == true)
        {
            //newpos = new Vector3(randPos.x, randPos.y, 0);
            if (playerStash != null && target != null)
                direction = (target.position - transform.position).normalized;
            else
                direction = transform.position.normalized;
            //direction = (newpos - transform.position).normalized;
        }
        else if (lineofsight == false)
        {
            //lineofsight == false;
            randPos = transform.position;
            randPos += Random.insideUnitCircle * dashAroundPlayerRadius;
            newpos = new Vector3(randPos.x, randPos.y, 0);
            direction = (newpos - transform.position).normalized;

        }

        Vector2 force = direction * dashForce;
        if (goForPlayer == true)
        {
            force = direction * dashForce * 2;
        }

        rb.AddForce(force, ForceMode2D.Impulse);



    }


    // Update is called once per frame
    void Update()
    {
        if (GlobalPlayerVariables.EnableAI && isDead == false)
        {
            if (target == null)
            {
                player = playerStash;
                chaseInProgress = 0f;
            }





            if (playerStash == null)
            {
                player = this.transform;
            }



            /*
            if (target == null)
            {
                target = playerStash;
                //EnemyTarget = playerStash;
                chaseInProgress = 0f;
            }
            */

            if (distancefromtarget <= 5 && swingTimer <= 0 && inmiddleofdash == false && DashTimer > 0) //inmiddleofdash == false && DashTimer > 0 && inmiddleofSwing == false && lineofsight == true)
            {
                swingTimer = swingCoolDown;
                Debug.Log("Swinging");
                //STUCK ON SWINGING
                //ALSO NEED TO MAKE DISTANCE FROM TARGET SO HE CAN PROPERLY ATTACK GLOBINS



                StartCoroutine(Swing());
            }
            if (swingTimer >= 0)
                swingTimer -= Time.deltaTime;

            //  NEED TO MAKE IT WHERE ENEMYTARGET IS THE CLOSEST ENEMY AND IF THAT DIES THEN IT DEFAULTS TO PLAYERSTASH AS
            //  IT IS SET NOW THE PLAYER CHANGES WILL MESS WITH THE PLAYERSTASH DUE TO POINTERS


            if (DashTimer <= 0 && canDash == true && distancefromtarget <= stoppingDistance && lineofsight == true && inmiddleofSwing == false)
            {

                float newDashTimer = Random.Range(beginningRangeToDash, endingRangeToDash);
                DashTimer = newDashTimer;
                StartCoroutine(DashAttackPattern());

                //NEED TO MAKE ANIMATION CONTINOUS SPINNING;




            }
            if (DashTimer >= 0)
                DashTimer -= Time.deltaTime;









            
            //AIRSTRIKES
            if (canDoAirStrike == true && AirStrike != null && AirStrikeTimer <= 0 && lineofsight == true)
            {
                float newAirStrikeTimer = Random.Range(beginningRangeToCallAirStrike, endingRangeToCallAirStrike);
                AirStrikeTimer = newAirStrikeTimer;

                //call in backup;
                Instantiate(AirStrike, transform.position, Quaternion.Euler(0, 0, 0));
                

            }
            if (AirStrikeTimer >= 0)
                AirStrikeTimer -= Time.deltaTime;

            








            //working on clearing up globin vision
            if (timer2reset <= 0)
            {
                timer2reset = recalcshortestDist;
                float closestDistanceSqr = Mathf.Infinity;
                Collider2D[] ColliderArray = Physics2D.OverlapCircleAll(transform.position, shootdistance);
                foreach (Collider2D collider2D in ColliderArray)
                {
                    if (collider2D.TryGetComponent<GoodGuyMarker>(out GoodGuyMarker marked))
                    {
                        if (collider2D.TryGetComponent<Transform>(out Transform enemy))
                        {
                            //Debug.Log("good guy detected");
                            //CAN THEY SEE THEM

                            //can probably optimize this later
                            RaycastHit2D hit2 = Physics2D.Raycast(transform.position, enemy.transform.position - transform.position, Mathf.Infinity, ~IgnoreMe);

                            if (hit2)
                            {
                                if (hit2.collider.gameObject.CompareTag("Player") || hit2.collider.gameObject.CompareTag("Globin"))
                                {
                                    lineofsight = true;
                                    Vector3 directionToTarget = enemy.position - transform.position;
                                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                                    if (dSqrToTarget < closestDistanceSqr)
                                    {

                                        closestDistanceSqr = dSqrToTarget;
                                        target = enemy;
                                        //EnemyTarget = enemy;
                                        //Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.black);
                                        //closest = true;
                                        //Debug.Log("Found target");

                                        //if (EnemyTarget != null && canSeeEnemy == true && closest == true && EnemyTarget == enemy)
                                    }
                                }
                            }


                            //target = enemy;
                        }
                    }
                }
            }




            if (GlobalPlayerVariables.GameOver != false)
            {
                if (hideing == false)
                {
                    player = this.transform;
                    hideing = true;
                }
            }

            if (isDead == true)
            {
                knockbacktime = 0;
                //knockback = false;
            }


            /*
            if (distancefromplayer <= DetectRange && GlobalPlayerVariables.GameOver == false)
            {
                if (hideing == false)
                    EnemyUI.SetActive(true);
            }
            else if (distancefromplayer >= DetectRange)
            {
                EnemyUI.SetActive(false);
            }
            */
            if (enemyColony.enableEnemyUI == false)
            {
                EnemyUI.SetActive(false);
            }
            else
            {
                Debug.Log("Activate boss ui");
                EnemyUI.SetActive(true);
            }


            knockbacktime -= Time.deltaTime;
            if (knockbacktime <= 0)
            {
                knockbacktime = 0;
                knockback = false;
            }
            if (player != null && player != this.transform && target != null)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, target.transform.position - transform.position, Mathf.Infinity, ~IgnoreMe);
                //var rayDirection = player.position - transform.position;
                Debug.DrawRay(transform.position, target.transform.position - transform.position, Color.green);
                if (hit.collider.gameObject.CompareTag("Player") || hit.collider.gameObject.CompareTag("Globin"))
                {
                    lineofsight = true;
                    //Debug.Log("Player is Visable");
                    // enemy can see the player!

                    //Debug.Log("Player is Visable");
                }
                else
                {
                    lineofsight = false;
                    //Debug.Log("Player is NOT Visable");
                }

            }

            if (lineofsight == true && GlobalPlayerVariables.GameOver == false && isDead == false && distancefromtarget <= shootdistance)
            {

                if (timeBtwShots <= 0)
                {

                    if (burstFire == true)
                    {
                        burstTime -= Time.deltaTime;
                        if (TimesShot < timesToShoot)
                        {
                            if (burstTime < 0)
                            {
                                TimesShot++;
                                burst();
                            }

                        }
                        else
                        {
                            TimesShot = 0;
                            variation();
                        }
                    }
                    else
                    {
                        //Debug.Log("SHOOT");
                        shoot();
                    }
                }
                else
                {
                    timeBtwShots -= Time.deltaTime;
                }

                if (NextMoveCoolDown <= 0 || reachedDestination == true)
                {
                    randomPos();
                }
                NextMoveCoolDown -= Time.deltaTime;
            }
            else
            {
                if (NextMoveCoolDown <= 0)
                {

                    randomPos();
                }

            }
        }
        NextMoveCoolDown -= Time.deltaTime;
        timer2reset -= Time.deltaTime;
    }


    [Header("SpinAttackRange")]
    public float spinattackrange;
    public float TimerToDoDamage = 0.2f;
    private float spinRingCounter = 0f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spinattackrange);
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        reachedDestination = true;
        NextMoveCoolDown = timeTillNextMove;

        if (collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("PLAYER CONTACT");
            knockbackForce = knockForcePlayerContact;
            knockbacktime = Random.Range(knockbackstartrange, knockbackendrange);
            knockback = true;
        }
        else if (!collision.gameObject.CompareTag("Enemy") && !collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("ENEMY IS HITTING WALL");
            //UNSTUCKPOS = collision.gameObject.GetComponent<Transform>();
            //knockbacktime = unstuckTime;
            //knockback = true;
            //unstuck = true;
            randomPos();
        }
    }

    private bool globinBullet = false;
    private void OnTriggerEnter2D(Collider2D col)
    {




        if (col.CompareTag("Bullet"))
        {
            //UITimer = secondsToShowUI;
            Bullet collision = col.gameObject.GetComponent<Bullet>();
            float damage = collision.damage;
            float speed = collision.speed;
            critRate = collision.critRate;
            critDMG = collision.critDMG;
            knockbackForce = collision.knockbackForce;
            if (collision.forGlobin == true)
            {
                globinBullet = true;
            }
            else
            {
                globinBullet = false;
            }


            takeDamage(damage, collision.transform, speed);

            reachedDestination = true;

            knockbacktime = Random.Range(knockbackstartrange, knockbackendrange);
            knockback = true;

        }




    }

    public void takeDamage(float damage, Transform impact, float speed)
    {
        //Debug.Log(damage);
        bool iscrit = false;
        float chance2crit = Random.Range(0f, 1f);
        if (chance2crit <= critRate)
        {
            iscrit = true;
            damage *= critDMG;
        }

        damage = Mathf.Round(damage);
        if (individualHP - damage < 0)
        {
            enemyColony.colonyHealth -= individualHP;
        }
        else
            enemyColony.colonyHealth -= damage;
        individualHP -= damage;
        //HealthBar.fillAmount = enemyColony.colonyHealth / MaxHP;
        if (globinBullet == false)
        {
            BossName.text = NameOfEnemy;
            //HealthBar.fillAmount = individualHP / indivMaxHP;
            enemyColony.resetTimer(null, this);
        }


        //enemyColony.colonyHealth -= damage;
        //HealthBar.fillAmount = enemyColony.colonyHealth / MaxHP;
        showDamage(damage, impact, speed, iscrit);
        StartCoroutine(FlashRed());
        if (individualHP <= 0)
        {
            Die();
        }
    }


    void showDamage(float damage, Transform impact, float speed, bool crit)
    {
        damage = Mathf.Round(damage);
        if (damage > 1)
        {
            Vector3 direction = (transform.position - impact.transform.position).normalized;

            //might add to impact to make it go past enemy
            GameObject go = ObjectPool.instance.GetDamagePopUpFromPool();
            go.GetComponent<Animator>().Play("DamagePopUp", -1, 0f);
            go.transform.SetParent(null);
            go.transform.position = impact.position;
            go.transform.rotation = Quaternion.identity;
            if (crit == false)
            {
                go.GetComponent<TextMeshPro>().text = damage.ToString();
            }
            else
            {
                //Debug.Log("CRIT");

                Color colorTop = new Color(0.83529f, 0.06667f, 0.06667f);
                Color colorBottom = new Color(0.98824f, 0.33725f, .90196f);

                go.GetComponent<TextMeshPro>().text = damage.ToString();
                go.GetComponent<TextMeshPro>().colorGradient = new VertexGradient(colorTop, colorTop, colorBottom, colorBottom);
                go.GetComponent<TextMeshPro>().fontSize *= 1.2f;
            }
            go.GetComponent<DestroyText>().spawnPos(direction.x, direction.y, speed / 5);
        }
    }

    WaitForSeconds shortWait = new WaitForSeconds(0.1f);
    public IEnumerator FlashRed()
    {
        sprite.color = Color.red;
        yield return shortWait;
        sprite.color = Color.white;
    }

    void burst()
    {
        for (int i = 0; i < AmountOfBullets; i++)
        {
            float WeaponSpread = Random.Range(-bulletSpread, bulletSpread);
            Quaternion newRot = Quaternion.Euler(firePoint.eulerAngles.x, firePoint.eulerAngles.y, firePoint.eulerAngles.z + WeaponSpread);
            Instantiate(projectile, firePoint.position, newRot);
            burstTime = timeBtwBurst;
        }
    }

    void shoot()
    {
        for (int i = 0; i < AmountOfBullets; i++)
        {
            float WeaponSpread = Random.Range(-bulletSpread, bulletSpread);
            Quaternion newRot = Quaternion.Euler(firePoint.eulerAngles.x, firePoint.eulerAngles.y, firePoint.eulerAngles.z + WeaponSpread);
            Instantiate(projectile, firePoint.position, newRot);
            variation();
        }

    }

    void randomPos()
    {
        if (isDead == false)
        {
            NextMoveCoolDown = Random.Range(0f, timeTillNextMove);

            randPos = transform.position;

            randPos += Random.insideUnitCircle * circleRadius;
            //Vector3 zfix = new Vector3(randPos.x, randPos.y, 0);
            //randPos = zfix;
            reachedDestination = false;
        }


    }



    void Die()
    {
        if (isDead == false)
        {
            isDead = true;
            reachedDestination = true;
            NextMoveCoolDown = 15f;
            GlobalPlayerVariables.expToDistribute += EXPWorth;
            RememberLoadout.totalExperienceEarned += EXPWorth;
            GlobalPlayerVariables.TotalEnemiesAlive -= 1;
            GlobalPlayerVariables.enemiesKilled += 1;
            transform.position = this.transform.position;
            transform.Find("BossSprite").GetComponent<Animator>().SetBool("IsDead", isDead);
            GetComponent<PolygonCollider2D>().enabled = false;
            enemyColony.checkBossCount();
            StartCoroutine(Dying());
            //GameObject.Destroy(gameObject);
        }

    }

    void LifeSteal(float damage)
    {
        if (enemyColony.colonyHealth < MaxHP)
        {
            enemyColony.colonyHealth += damage;
            HPgained += damage;





        }
        else if (enemyColony.colonyHealth + damage >= MaxHP)
            enemyColony.colonyHealth = MaxHP;



        HealthBar.fillAmount = enemyColony.colonyHealth / MaxHP;
    }

    void showLifeGained(float damage)
    {
        damage = Mathf.Round(damage);
        if (damage > 1)
        {
            Vector3 direction = (transform.position - weapon.transform.position).normalized;

            //might add to impact to make it go past enemy
            //var go = Instantiate(DamageText, weapon.transform.position, Quaternion.identity);

            GameObject go = ObjectPool.instance.GetDamagePopUpFromPool();
            go.GetComponent<Animator>().Play("DamagePopUp", -1, 0f);
            go.transform.SetParent(null);
            go.transform.position = weapon.transform.position;
            go.transform.rotation = Quaternion.identity;


            //Debug.Log("CRIT");

            Color colorTop = Color.green;
            Color colorBottom = Color.green;

            go.GetComponent<TextMeshPro>().text = damage.ToString();
            go.GetComponent<TextMeshPro>().colorGradient = new VertexGradient(colorTop, colorTop, colorBottom, colorBottom);
            go.GetComponent<TextMeshPro>().fontSize *= 1.2f;

            go.GetComponent<DestroyText>().spawnPos(direction.x, direction.y, 10);
        }
    }


    void overlapCircleRing()
    {
        Collider2D[] objectsToBurn = Physics2D.OverlapCircleAll(transform.position, spinattackrange);
        foreach (var objectToBurn in objectsToBurn)
        {

            if (objectToBurn.tag == "EnemyMelee")
            {
                objectToBurn.GetComponent<Enemy2>().takeDamage(spinDamage, objectToBurn.transform, 10);
                LifeSteal(spinDamage);
            }
            if (objectToBurn.tag == "Enemy")
            {
                if (objectToBurn.GetComponent<Enemy1>() != null)
                    objectToBurn.GetComponent<Enemy1>().takeDamage(spinDamage, objectToBurn.transform, 10);
                else
                    objectToBurn.GetComponent<Enemy3>().takeDamage(spinDamage, objectToBurn.transform, 10);
                LifeSteal(spinDamage);
            }

            //if (objectToBurn.tag == "Colony") { objectToBurn.GetComponent<EnemyColony>().takeDamage(contactDamage, objectToBurn.transform, 10); }
            if (objectToBurn.tag == "Player")
            {
                objectToBurn.GetComponent<TakeDamage>().takeDamage(spinDamage, objectToBurn.transform, 10);
                LifeSteal(spinDamage);
            }
            if (objectToBurn.tag == "Globin")
            {
                objectToBurn.GetComponent<Globin>().takeDamage(spinDamage, objectToBurn.transform, 10);
                LifeSteal(spinDamage);
            }
        }
    }



    //SPINRING
    void hurtCircle()
    {
        if (spinRingCounter <= 0)
        {
            overlapCircleRing();
            spinRingCounter = TimerToDoDamage;
        }
        else
            spinRingCounter -= Time.deltaTime;

        //yield return new WaitForSecondsRealtime(0.3f);
    }


    IEnumerator Swing()
    {
        Debug.Log("coro");
        weapon.enabled = true;
        inmiddleofSwing = true;
        weaponHurtBox.enabled = true;
        weapon.Play("ColonyWeaponSwing", 0, 0f);
        yield return new WaitForSecondsRealtime(0.6f);
        Debug.Log("Return");
        weaponHurtBox.enabled = false;
        inmiddleofSwing = false;
        //weapon.Play("ColonyWeaponNoAnimation", 0, 0f);
        weapon.enabled = false;
    }

    /*
    IEnumerator SwingShoot()
    {
        Debug.Log("coro");
        weapon.enabled = true;
        inmiddleofSwing = true;
        weaponHurtBox.enabled = true;
        weapon.Play("ColonyWeaponSwing", 0, 0f);
        yield return new WaitForSecondsRealtime(0.6f);
        Debug.Log("Return");
        weaponHurtBox.enabled = false;
        inmiddleofSwing = false;
        //weapon.Play("ColonyWeaponNoAnimation", 0, 0f);
        weapon.enabled = false;
    }
    */

    /*
    IEnumerator InDash()
    {
        //inmiddleofdash = true;
        yield return new WaitForSecondsRealtime(0.25f);
        //inmiddleofdash = false;
    }
    */

    IEnumerator clearParticles()
    {
        yield return new WaitForSeconds(0.1f);
        spinRing.Clear();
    }






    IEnumerator DashAttackPattern()
    {
        HPgained = 0;
        spinRing.Play();
        inmiddleofdash = true;
        weapon.enabled = true;
        randomDashPos(false);
        weapon.Play("ColonyWeapon",0,0f);
        //StartCoroutine(clearParticles());

        yield return new WaitForSeconds(0.6f);
        randomDashPos(false);
        //weapon.Play("ColonyWeapon", 0, 0f);
        //StartCoroutine(clearParticles());
        yield return new WaitForSeconds(0.6f);
        randomDashPos(false);
        //weapon.Play("ColonyWeapon", 0, 0f);
        //StartCoroutine(clearParticles());
        yield return new WaitForSeconds(0.6f);
        randomDashPos(true);
        //weapon.Play("ColonyWeapon", 0, 0f);
        //StartCoroutine(clearParticles());
        yield return new WaitForSeconds(0.6f);
        spinRing.Clear();
        weapon.Play("ColonyWeaponNoAnimation", 0, 0f);
        weapon.enabled = false;
        inmiddleofdash = false;
        spinRing.Stop();
        showLifeGained(HPgained);
    }





    IEnumerator Dying()
    {
        dashForce = 0;
        rb.velocity = Vector2.zero;
        yield return new WaitForSecondsRealtime(2.75f);
        foreach (ItemDrops id in Drops)
        {
            if (Random.Range(0, 100) <= id.DropPercentage)
            {
                if (id.isAmmo == false)
                {
                    if (id.isCurrency == false)
                    {
                        for (int i = 0; i < id.NumOfDrop; i++)
                            Instantiate(id.Drops[0], transform.position, Quaternion.identity);
                    }
                    else if (id.isCurrency == true)
                    {
                        int[] NumArr = id.convertProtein();
                        for (int i = 0; i < NumArr.Length; i++)
                        {
                            for (int j = 0; j < NumArr[i]; j++)
                                Instantiate(id.Drops[i], transform.position, Quaternion.identity);
                        }
                    }
                }
                else
                {
                    int[] NumArr = id.convertAmmo();
                    for (int i = 0; i < NumArr.Length; i++)
                    {
                        for (int j = 0; j < NumArr[i]; j++)
                            Instantiate(id.Drops[i], transform.position, Quaternion.identity);
                    }
                }
            }
        }
        //EnemyUI.SetActive(false);
        if (enemyColony.colonyHealth <= 0)
        {
            EnemyUI.SetActive(false);
        }
        Destroy(transform.gameObject);
    }



}
