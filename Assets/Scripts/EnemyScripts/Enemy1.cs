using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Experimental.U2D.Animation;
using System.Linq;

public class Enemy1 : MonoBehaviour
{
    [System.Serializable]
    public struct ItemDrops
    {
        public bool isCurrency;
        public bool isAmmo;
        public GameObject[] Drops;
        public float DropPercentage;
        public float SurvivalDropPercentage;
        public int NumOfDrop;
        public float SurvivalNumOfDrop;
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

    public event System.Action OnDeath;

    [Header("Enemy Stats")]

    public float contactDamage;
    public float HP = 100;
    public float speed = 0;
    public int EXPWorth = 50;

    [Header("Movement Settings")]
    public float stoppingDistance = 0;
    public float retreatDistance = 0;
    public bool followPlayer;
    public bool retreat;
    public bool randomMovement;

    [Header("Line Of Sight")]
    public bool lineofsight;
    public LayerMask IgnoreMe;
    private bool unstuck;
    public float unstuckTime;
    public float shootdistance;
    public float distancefromplayer;
    private Transform UNSTUCKPOS;



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
    public Transform playerStash;
    public Transform player;
    private Rigidbody2D rb;


    [Header("Burst Settings")]
    public bool burstFire;
    public float timeBtwBurst;
    private float burstTime = 0;

    public int timesToShoot;
    private int TimesShot = 0;

    [Header("Drops")]
    public ItemDrops[] Drops;

    [Header("SkinModule")]
    [SerializeField]
    private SpriteLibrary spriteLibrary = default;
    [SerializeField]
    private SpriteResolver targetResolver = default;
    [SerializeField]
    private string targetCategory = default;

    private string[] currSprite;

    //ANIMATION VARIABLES

    //DEATH VARIABLE
    public bool isDead = false;

    Vector2 direction;
    float a;


    private float critRate = 0;
    private float critDMG = 0;

    private float easeOM;

    private float relaMouseAngle;

    [HideInInspector]
    public float facing;


    [Header("Reset Check Setting")]
    public float recalcshortestDist = 1f;
    private float timer2reset;

    private EnemyManager enemyManager = null;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (GlobalPlayerVariables.GameOver == false)
            player = Player.instance.transform;
        //player = GameObject.FindGameObjectWithTag("Player").transform;
        else
            player = this.transform;
        sprite = GetComponent<SpriteRenderer>();

        currSprite = spriteLibrary.spriteLibraryAsset.GetCategoryLabelNames(targetCategory).ToArray();

        GlobalPlayerVariables.TotalEnemiesAlive += 1;

        playerStash = player;

        //if (GameObject.Find("EnemyColony") != null)
        enemyManager = EnemyManager.instance;
        //if (GameObject.Find("EnemyColony2") != null)
            
        variation();
    }

    void variation()
    {
        startTimeBtwShots = Random.Range(beginningrangetoshoot, endingrangetoshoot);
        timeBtwShots = startTimeBtwShots;
    }


    private void FixedUpdate()
    {
        if (GlobalPlayerVariables.GameOver != false)
        {
            player = this.transform;
        }
        if (GlobalPlayerVariables.EnableAI)
        {
            if(player!=null)
                distancefromplayer = Vector2.Distance(transform.position, player.position);
            if (knockback == true)
            {

                //Debug.Log("KNOCKBACK");
                if (unstuck == false)
                {
                    if(player != null)
                        transform.position = Vector2.MoveTowards(transform.position, player.position, -knockbackForce * speed * Time.deltaTime);
                }
                else
                {
                    //Debug.Log("COMMENCING UNSTUCK");
                    transform.position = Vector2.MoveTowards(transform.position, randPos, -speed * Time.deltaTime);

                }
                getDirection(player);

            }
            else
            {

                if (distancefromplayer >= stoppingDistance && followPlayer == true && lineofsight == true) //follow player
                {
                    reachedDestination = true;
                    easeOM = 1;
                    if(player != null)
                        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
                    getDirection(player);
                }
                else if ((distancefromplayer >= retreatDistance) || GlobalPlayerVariables.GameOver == true || lineofsight == false) //stop /*(Vector2.Distance(transform.position, player.position) <= stoppingDistance && */ 
                {
                    if (randomMovement == false)
                        transform.position = this.transform.position;
                    else //RANDOM MOVEMENT
                    {

                        float disToRandPos = (new Vector2(transform.position.x, transform.position.y) - randPos).magnitude;
                        if (disToRandPos < speed)
                        {
                            if (easeOM > 0)
                                easeOM -= Time.fixedDeltaTime;
                            else
                                easeOM = 0;
                        }
                        else
                        {
                            easeOM = 1;
                        }

                        if (reachedDestination == false)
                            transform.position = Vector2.MoveTowards(transform.position, randPos, speed * Time.deltaTime * easeOM);
                        else
                            transform.position = this.transform.position;

                        if (transform.position.x == randPos.x && transform.position.y == randPos.y)
                        {
                            reachedDestination = true;
                        }
                        direction = randPos;
                        //a = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    }
                }
                else if (distancefromplayer <= retreatDistance && retreat == true && lineofsight == true) //retreat
                {
                    reachedDestination = true;
                    if (player != null)
                        transform.position = Vector2.MoveTowards(transform.position, player.position, -speed * Time.deltaTime);
                    getDirection(player);
                }

            }
        }
    }

    void getDirection(Transform objectpos)
    {
        if (objectpos != null)
            direction = objectpos.position - transform.position;
        facing = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    Vector3 directionToTarget = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        if (GlobalPlayerVariables.EnableAI)
        {
            if(player == null)
                player = playerStash;



            if (GlobalPlayerVariables.GameOver != false)
            {
                player = this.transform;
            }

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
                                    directionToTarget = enemy.position - transform.position;
                                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                                    if (dSqrToTarget < closestDistanceSqr)
                                    {
                                        closestDistanceSqr = dSqrToTarget;
                                        player = enemy;
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


            knockbacktime -= Time.deltaTime;
            if (knockbacktime <= 0)
            {
                knockbacktime = 0;
                knockback = false;
                unstuck = false;
            }
            if (player != null && player != this.transform)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position, Mathf.Infinity, ~IgnoreMe);
                //var rayDirection = player.position - transform.position;
                Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.green);
                if (hit)
                {
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

            }

            if (lineofsight == true && GlobalPlayerVariables.GameOver == false && distancefromplayer <= shootdistance)
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
                        shoot();
                    }
                }
                else
                {
                    timeBtwShots -= Time.deltaTime;
                }

                if (NextMoveCoolDown <= 0 && reachedDestination == true)
                {
                    //Vector2 temp = randPos;
                    randomPos();
                    //facing = Mathf.Atan2((temp - randPos).x, (temp - randPos).y) * Mathf.Rad2Deg;
                }
                NextMoveCoolDown -= Time.deltaTime;
            }
            else
            {
                if (NextMoveCoolDown <= 0)
                {
                    Vector2 temp = randPos;
                    randomPos();
                    facing = Mathf.Atan2((temp - randPos).x, (temp - randPos).y) * Mathf.Rad2Deg;
                }
                NextMoveCoolDown -= Time.deltaTime;
            }
            if ((lineofsight && distancefromplayer <= shootdistance))
            {
                facing = transform.Find("EnemyAim").GetComponent<EnemyWeapon>().AimDir;
            }
            //Debug.Log(lineofsight + " " +facing);
            Animate(facing);
            timer2reset -= Time.deltaTime;
        }
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

    private void OnTriggerEnter2D(Collider2D col)
    {

        if (col.CompareTag("Bullet"))
        {
            Bullet collision = col.gameObject.GetComponent<Bullet>();

            float damage = collision.damage;
            float speed = collision.speed;
            critRate = collision.critRate;
            critDMG = collision.critDMG;
            knockbackForce = collision.knockbackForce;


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

        HP -= damage;
        showDamage(damage, impact, speed, iscrit);
        StartCoroutine(FlashRed());
        if (HP <= 0)
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
            //var go = Instantiate(DamageText, impact.position, Quaternion.identity);
            GameObject go = ObjectPool.instance.GetDamagePopUpFromPool();
            go.GetComponent<Animator>().Play("DamagePopUp", -1, 0f);
            go.transform.SetParent(null);
            go.transform.position = impact.position;
            go.transform.rotation = Quaternion.identity;
            if (crit == false)
            {
                go.GetComponent<TextMeshPro>().text = damage.ToString();
                go.GetComponent<TextMeshPro>().fontSize = 9f;
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

            if (projectile.name == "EnemyBullet")
            {
               
                GameObject bullet = ObjectPool.instance.GetBulletFromPool();
                if (bullet != null)
                {
                    bullet.transform.position = firePoint.position;
                    //bullet.transform.rotation = firePoint.rotation;
                    bullet.transform.rotation = newRot;
                    bullet.GetComponent<EnemyProj>().despawnTime = bullet.GetComponent<EnemyProj>().DespawnTimeHolder;
                    bullet.GetComponent<PlaySound>().replaySound();
                    //bullet.GetComponent<EnemyProj>().resetSpeed(); 
                }
                
            }
            else
                Instantiate(projectile, firePoint.position, newRot);
            burstTime = timeBtwBurst;
        }
    }

    void shoot()
    {
        for (int i = 0; i < AmountOfBullets; i++)
        {
            //EnemyWeapon.WeaponAnim.SetBool("IsShooting", true);
            float WeaponSpread = Random.Range(-bulletSpread, bulletSpread);
            Quaternion newRot = Quaternion.Euler(firePoint.eulerAngles.x, firePoint.eulerAngles.y, firePoint.eulerAngles.z + WeaponSpread);
            if (projectile.name == "EnemyBullet")
            {
               
                GameObject bullet = ObjectPool.instance.GetBulletFromPool();
                if (bullet != null)
                {
                    bullet.transform.position = firePoint.position;
                    //bullet.transform.rotation = firePoint.rotation;
                    bullet.transform.rotation = newRot;
                    bullet.GetComponent<EnemyProj>().despawnTime = bullet.GetComponent<EnemyProj>().DespawnTimeHolder;
                    bullet.GetComponent<PlaySound>().replaySound();
                    //bullet.GetComponent<EnemyProj>().resetSpeed();
                }
                
            }
            else
                Instantiate(projectile, firePoint.position, newRot);
            variation();
        }

    }

    void randomPos()
    {

        NextMoveCoolDown = Random.Range(0f, timeTillNextMove);
        randPos = transform.position;
        randPos += Random.insideUnitCircle * circleRadius;
        reachedDestination = false;
    }


    void Die()
    {
        if (isDead == false)
        {
            isDead = true;
            if(enemyManager != null)
            {
                enemyManager.OnEnemyDeath(1);
            }
            GlobalPlayerVariables.expToDistribute += EXPWorth;
            RememberLoadout.totalExperienceEarned += EXPWorth;
            GlobalPlayerVariables.TotalEnemiesAlive -= 1;
            GlobalPlayerVariables.enemiesKilled += 1;
            if (OnDeath != null)
            {
                OnDeath();
            }
            foreach(ItemDrops id in Drops)
            {
                float drPctg = GlobalPlayerVariables.IsSurvival ? id.SurvivalDropPercentage : id.DropPercentage;
                float numOdr = GlobalPlayerVariables.IsSurvival ? id.SurvivalNumOfDrop : id.NumOfDrop;

                if (Random.Range(0, 100) <= drPctg)
                {
                    if (id.isAmmo == false)
                    {
                        if (id.isCurrency == false)
                        {
                            for (int i = 0; i < numOdr; i++)
                                Instantiate(id.Drops[0], transform.position, Quaternion.identity);
                        }
                        else if (id.isCurrency == true)
                        {
                            var go = id;
                            Collider2D[] ColliderArray = Physics2D.OverlapCircleAll(transform.position, 1);
                            foreach (Collider2D collider2D in ColliderArray)
                            {
                                if (collider2D.TryGetComponent<ItemPickup>(out ItemPickup potentialProtein))
                                {
                                    if (potentialProtein.TypeOfItem == "Protein")
                                    {
                                        go.NumOfDrop += potentialProtein.getProteinAmount();
                                        //Debug.Log("NUM OF DROP " + go.NumOfDrop);
                                        Destroy(collider2D.gameObject);
                                    }
                                }
                            }
                                //id.num
                            int[] NumArr = go.convertProtein();
                            for (int i = 0; i < NumArr.Length; i++)
                            {
                                //Debug.Log("index of drops " + i);
                                var remem = Instantiate(go.Drops[i], transform.position, Quaternion.identity);
                                int temp = go.Drops[i].GetComponent<ItemPickup>().getProteinAmount() * NumArr[i];
                                //Debug.Log("temp of drops " + temp);
                                if (temp <= 0)
                                    Destroy(remem);
                                else
                                    remem.GetComponent<ItemPickup>().setNewProteinAmount(temp);
                                /*
                                for (int j = 0; j < NumArr[i]; j++)
                                {
                                    temp = go.Drops[i].GetComponent<ItemPickup>().getProteinAmount();
                                }
                                */

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

            if(transform.Find("StickyGrenade(Clone)") != null)
            {
                transform.Find("StickyGrenade(Clone)").GetComponent<StickyGrenade>().stuck = false;
                transform.Find("StickyGrenade(Clone)").GetComponent<StickyGrenade>().landed = true;
                transform.Find("StickyGrenade(Clone)").GetComponent<CircleCollider2D>().isTrigger = false;
                transform.Find("StickyGrenade(Clone)").parent = null;
            }

            GameObject.Destroy(gameObject);
        }
    }
    public void Animate(float angle)
    {
        //transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(player.Stats.Direction.y, player.Stats.Direction.x) * Mathf.Rad2Deg - 180));
        //relaMouseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        relaMouseAngle = angle;
        if (relaMouseAngle < 0)
            relaMouseAngle = relaMouseAngle + 360;
        //Debug.Log(relaMouseAngle);
        //Debug.Log(relaMouseAngle);
        //New 8 directions system
        /*[0]Down
         *[1]Up
         *[2]Left
         *[3]Right
         *[4]TopLeft
         *[5]TopRight
         *[6]BotLeft
         *[7]BotRight
         */
        if (relaMouseAngle <= 22.5 || relaMouseAngle > 337.5) //Right
        {
            targetResolver.SetCategoryAndLabel(targetCategory, currSprite[3]);
        }
        else if (relaMouseAngle > 22.5 && relaMouseAngle <= 67.5) //TopRight
        {
            targetResolver.SetCategoryAndLabel(targetCategory, currSprite[5]);
        }
        else if (relaMouseAngle > 67.5 && relaMouseAngle <= 112.5) //Up
        {
            targetResolver.SetCategoryAndLabel(targetCategory, currSprite[1]);
        }
        else if (relaMouseAngle > 112.5 && relaMouseAngle <= 157.5) //TopLeft
        {
            targetResolver.SetCategoryAndLabel(targetCategory, currSprite[4]);
        }
        else if (relaMouseAngle > 157.5 && relaMouseAngle <= 202.5) //Left
        {
            targetResolver.SetCategoryAndLabel(targetCategory, currSprite[2]);
        }
        else if (relaMouseAngle > 202.5 && relaMouseAngle <= 247.5) //BotLeft
        {
            targetResolver.SetCategoryAndLabel(targetCategory, currSprite[6]);
        }
        else if (relaMouseAngle > 247.5 && relaMouseAngle <= 292.5) //Down
        {
            targetResolver.SetCategoryAndLabel(targetCategory, currSprite[0]);
        }
        else if (relaMouseAngle > 292.5 && relaMouseAngle <= 337.5) //BotRight
        {
            targetResolver.SetCategoryAndLabel(targetCategory, currSprite[7]);
        }
    }

}
