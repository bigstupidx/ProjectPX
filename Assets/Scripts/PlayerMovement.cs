﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Prime31;
using UnityEngine.EventSystems;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public enum PlayerBehaviour
{
    IDLE,
    MOVE,
    MOVEANDTHROW,
    ATTACK,
    REACT
}

public class PlayerMovement : MonoBehaviour
{
    public PlayerBehaviour playerBehaviour;
    private Animator CoinAnimUI;
    public GameObject coinUI;
    public GameObject normalSelectionCircle;
    public GameObject wallColliderObj;
    public float speed, knifeThrowSpeed;
    public Vector2 velocity;
    private Vector3 target;
    private Animator characterAnimator;

    private AnimatorStateInfo animatorStateInfo;
    float idleDirection, moveDirection, prevMoveDirection, knifeThrowDirection;

    float initialSpeed, intialDistanceToAttack, initialDistanceToThrow;
    float xComponent;
    float yComponent;
    float angle;

    Vector3 touchPos;
    string layerName;

    bool isIdle;
    bool isInMove;
    bool isRun;
    bool isAttack;
    bool isReact;

    public GameObject PotParticleObj;

    GameObject selectedEnemy, selectedObject;

    public float distanceToPoint, distanceToAttack, distanceToThrow;

    public float attackTime;

    public GameObject spinSelectionCircle;
    public float spinRange;
    public float spinTime;
    public float sTime;

    public GameObject knife;

    RaycastHit2D hit;
    RaycastHit hit3D;
    float nextAttackTimer;

    public bool canThrow = false;
    public GameObject[] knifePrefab;
    public GameObject knifeThrowPoint;

    public float fireBallTimer;
    public GameObject fireBallPrefab;
    public GameObject fireBall;
    public float fTimer;

    public float interpolationScale;

    bool canSpin;

    public float spinAttackDistance;
    CoinAnim coinAnim;

    int AttackCombo = 0;
    float CoolOffTime = 10.0f;
    public int prevID = 0;
    public float CoolOffKnife = 1.0f;

    void Awake()
    {
        target = transform.position;
        characterAnimator = GetComponent<Animator>();
        PotParticleObj = Instantiate(PotParticleObj) as GameObject;
        PotParticleObj.SetActive(false);
    }


    void Start()
    {
        initialSpeed = speed;
        if (coinUI != null)
            CoinAnimUI = coinUI.GetComponent<Animator>();
        intialDistanceToAttack = distanceToAttack;
        initialDistanceToThrow = distanceToThrow;
        idleDirection = 5;
        characterAnimator.SetFloat("idleDirection", idleDirection);
        sTime = spinTime;
        fTimer = fireBallTimer;
        spinSelectionCircle.SetActive((false));
        normalSelectionCircle.SetActive(true);
        GameGlobalVariablesManager.isPlayerSpin = false;

        GameGlobalVariablesManager.playerHealth = 80 + GameGlobalVariablesManager.PlayerLevel * 20;

        GameObject curObj = GameObject.FindGameObjectWithTag("CoinParticles") as GameObject;
        if (curObj != null)
        {
            coinAnim = curObj.GetComponent<CoinAnim>() as CoinAnim;
        }

        for (int curLayer = 31; curLayer >= 0; curLayer--)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, curLayer, false);
        }
        Debug.Log("Playing level : " + GameGlobalVariablesManager.currentLevelnumber);

        GameGlobalVariablesManager.DecreaseEnergy();
        SavedData.Inst.SaveAllData();

        isIdle = true;
        isInMove = false;
        isRun = false;
        isAttack = false;
        isReact = false;
    }


    void CalculateAngle(float angle)
    {
        if (angle > 0)
        {
            if (angle > 75 && angle <= 105)
            {
                // up 
                moveDirection = 1;
                prevMoveDirection = 1;
                idleDirection = -1;
            }
            else if (angle > 15 && angle <= 75)
            {
                // up right
                moveDirection = 2;
                prevMoveDirection = 2;
                idleDirection = -1;
            }
            else if (angle >= 0 && angle <= 15)
            {
                // right
                moveDirection = 3;
                prevMoveDirection = 3;
                idleDirection = -1;
            }
            else if (angle > 105 && angle <= 165)
            {
                // up left
                moveDirection = 8;
                prevMoveDirection = 8;
                idleDirection = -1;
            }
            else //if(angle>165 && angle<=180)
            {
                // left
                moveDirection = 7;
                prevMoveDirection = 7;
                idleDirection = -1;
            }
        }

        // Mapping angle to 8 directions 0 - -180
        else
        {
            if (angle < -75 && angle >= -105)
            {
                // down
                moveDirection = 5;
                prevMoveDirection = 5;
                idleDirection = -1;
            }
            else if (angle < -15 && angle >= -75)
            {
                //down right
                moveDirection = 4;
                prevMoveDirection = 4;
                idleDirection = -1;
            }
            else if (angle <= 0 && angle >= -15)
            {
                // right
                moveDirection = 3;
                prevMoveDirection = 3;
                idleDirection = -1;
            }
            else if (angle < -105 && angle >= -165)
            {
                //down left
                moveDirection = 6;
                prevMoveDirection = 6;
                idleDirection = -1;
            }
            else //if(angle<-165 && angle>=-180)
            {
                // left 
                moveDirection = 7;
                prevMoveDirection = 7;
                idleDirection = -1;
            }
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("OnTriggerEnter2D");
        layerName = LayerMask.LayerToName(other.gameObject.layer);

        switch (layerName)
        {
            case "Player":

                break;

            case "AI":
                if (canSpin)
                {
                    //other.gameObject.GetComponent<AIComponent> ().healthBar.SetActive (false);
                    //other.gameObject.GetComponent<AIComponent> ().Death ();
                }
                break;
            case "EnemyTrigger0":
                Debug.Log("touched trigger " + other.gameObject.name);
                LevelManager.instance.activateAISpawn[0] = true;
                other.gameObject.SetActive(false);
                break;
            case "EnemyTrigger1":
                Debug.Log("touched trigger " + other.gameObject.name);
                LevelManager.instance.activateAISpawn[1] = true;
                other.gameObject.SetActive(false);
                break;
            case "EnemyTrigger2":
                Debug.Log("touched trigger " + other.gameObject.name);
                LevelManager.instance.activateAISpawn[2] = true;
                other.gameObject.SetActive(false);
                break;
            case "EnemyTrigger3":
                Debug.Log("touched trigger " + other.gameObject.name);
                LevelManager.instance.activateAISpawn[3] = true;
                other.gameObject.SetActive(false);
                break;
            case "EnemyTrigger4":
                Debug.Log("touched trigger " + other.gameObject.name);
                LevelManager.instance.activateAISpawn[4] = true;
                other.gameObject.SetActive(false);
                break;
            case "EnemyTrigger5":
                Debug.Log("touched trigger " + other.gameObject.name);
                LevelManager.instance.activateAISpawn[5] = true;
                other.gameObject.SetActive(false);
                break;
            case "EnemyTrigger6":
                Debug.Log("touched trigger " + other.gameObject.name);
                LevelManager.instance.activateAISpawn[6] = true;
                other.gameObject.SetActive(false);
                break;

            case "Door0":

                if (LevelManager.instance.stageCompleted[0])
                {
                    LevelManager.instance.doorsToBeOpened[0].GetComponent<Doors>().OpenDoor();
                }

                break;
            case "Door1":

                if (LevelManager.instance.stageCompleted[1])
                {
                    LevelManager.instance.doorsToBeOpened[1].GetComponent<Doors>().OpenDoor();
                }

                break;
            case "Door2":

                if (LevelManager.instance.stageCompleted[2])
                {
                    LevelManager.instance.doorsToBeOpened[2].GetComponent<Doors>().OpenDoor();
                }

                break;
            case "Door3":

                if (LevelManager.instance.stageCompleted[3])
                {
                    LevelManager.instance.doorsToBeOpened[3].GetComponent<Doors>().OpenDoor();
                }

                break;
            case "Door4":

                if (LevelManager.instance.stageCompleted[4])
                {
                    LevelManager.instance.doorsToBeOpened[4].GetComponent<Doors>().OpenDoor();
                }

                break;
            case "Door5":

                if (LevelManager.instance.stageCompleted[5])
                {
                    LevelManager.instance.doorsToBeOpened[5].GetComponent<Doors>().OpenDoor();
                }

                break;
            case "Door6":

                if (LevelManager.instance.stageCompleted[6])
                {
                    LevelManager.instance.doorsToBeOpened[6].GetComponent<Doors>().OpenDoor();
                }

                break;

            case "Portal":
                Debug.Log("LevelsCleared, portal : " + GameGlobalVariablesManager.LevelsCleared);
                LevelManager.instance.ClosePortal();
                GameGlobalVariablesManager.OnLevelCleared();
                SavedData.Inst.SaveAllData();
                Application.LoadLevel(GameGlobalVariablesManager.LevelSelection);
                break;

            case "WallLightLayer":
                Debug.Log("WallLightLayer: " + transform.position + other.gameObject.name);
                Idle();
                playerBehaviour = PlayerBehaviour.IDLE;
                break;

            default:
                break;
        }
    }


    void Spin()
    {
        if (sTime >= 0f)
        {
            if (!animatorStateInfo.IsName("VijaySpin"))
            {
                characterAnimator.SetBool("isSpin", canSpin);
                normalSelectionCircle.SetActive(false);
                spinSelectionCircle.SetActive((true));
                //playerSpinCircleCollider.radius = 8f;
            }

            if (animatorStateInfo.IsName("VijaySpin"))
            {
                GameObject[] enemyList = GameObject.FindGameObjectsWithTag("AI");
                foreach (var e in enemyList)
                {
                    Debug.Log(e.gameObject.name);
                    Debug.Log(Vector2.Distance(e.gameObject.transform.position, this.transform.position));
                    if (Vector2.Distance(e.gameObject.transform.position, this.transform.position) < spinRange)
                    {
                        e.gameObject.GetComponent<AIComponent>().healthBar.SetActive(false);
                        e.gameObject.GetComponent<AIComponent>().Death();
                    }
                }
            }
        }
        else
        {
            Debug.Log("time over");
            GameGlobalVariablesManager.isPlayerSpin = false;
            canSpin = false;
            sTime = spinTime;
            normalSelectionCircle.SetActive(true);
            spinSelectionCircle.SetActive((false));
            //playerSpinCircleCollider.radius = 3.5f;
            characterAnimator.SetBool("isSpin", canSpin);
        }
    }


    void ShowFireBallCircle()
    {
        if (fTimer >= 0f)
        {
            //Debug.Log ("show circle");
            //fireBallPrefab.SetActive (true);
            if (fireBall == null)
                fireBall = GameObject.Instantiate(fireBallPrefab, this.transform.position, Quaternion.identity) as GameObject;
        }
        else
        {
            Debug.Log("hide circle");
            GameGlobalVariablesManager.isFireBallThrown = false;
            fTimer = fireBallTimer;
            Destroy(fireBall.gameObject);
        }
    }


    void MoveTowardsPoint()
    {
        Vector3 oldPos = transform.position;
        distanceToPoint = Vector2.Distance(transform.position, touchPos);
        if (distanceToPoint < distanceToAttack)
        {
            Stop();
        }
        else
        {
            bool isColl = false;
            xComponent = -transform.position.x + touchPos.x;
            yComponent = -transform.position.y + touchPos.y;

            angle = Mathf.Atan2(yComponent, xComponent) * Mathf.Rad2Deg;
            transform.position = Vector2.MoveTowards(transform.position, touchPos, speed * Time.deltaTime);
            //Debug.Log("radius : " + wallColliderObj.GetComponent<CircleCollider2D>().radius);

            int wallLayerMask = 1 << GameGlobalVariablesManager.WallLightLayer;
            Vector2 dest = new Vector2(touchPos.x, touchPos.y);
            Vector2 org = new Vector2(wallColliderObj.transform.position.x, wallColliderObj.transform.position.y);

            Collider2D[] allColliders = Physics2D.OverlapCircleAll(wallColliderObj.transform.position, 1.5f, wallLayerMask);
            for (int i = 0; i < allColliders.Length && !isColl; i++)
            {
                if (allColliders[i].gameObject.layer == GameGlobalVariablesManager.WallLightLayer)
                {
                    isColl = true;
                }
            }

            if (isColl)
            {
                Stop();
                playerBehaviour = PlayerBehaviour.IDLE;
                isAttack = false;
                nextAttackTimer = 0;
                transform.position = oldPos;
                Debug.Log("oldPos : stop()");
            }
            else
            {
                //Debug.Log("oldPos : " + oldPos + "newPos: " + transform.position);
                if (selectedObject == null)
                {
                    isRun = true;
                    distanceToAttack = 1;
                    speed = initialSpeed;
                }
                else
                {
                    if (distanceToPoint <= distanceToAttack * 2)
                    {
                        isRun = false;
                        speed = initialSpeed / 2;
                    }
                    else
                    {
                        isRun = true;
                        speed = initialSpeed;
                    }
                }
                // always run
                isRun = true;
                isInMove = true;
            }
        }

        if (isInMove)
        {
            characterAnimator.StopPlayback();
            CalculateAngle(angle);
            isIdle = false;
        }

        if (transform.position == touchPos)
        {
            isInMove = false;
            isRun = false;
            isIdle = true;
            idleDirection = prevMoveDirection;
            distanceToAttack = 0;
        }

        characterAnimator.SetBool("isInMove", isInMove);
        characterAnimator.SetBool("isRun", isRun);
        characterAnimator.SetBool("isIdle", isIdle);
        if (isInMove)
            characterAnimator.SetBool("isAttack", false);
        else
            characterAnimator.SetBool("isAttack", isAttack);
        characterAnimator.SetBool("isReact", isReact);
        characterAnimator.SetFloat("idleDirection", idleDirection);
        characterAnimator.SetFloat("moveDirection", moveDirection);
    }


    void MoveTowardsThrowPoint()
    {
        Vector3 oldPos = transform.position;
        distanceToPoint = Vector2.Distance(transform.position, touchPos);

        if (distanceToPoint < distanceToThrow)
        {
            StopAndThrow();
        }
        else
        {
            bool isColl = false;
            xComponent = -transform.position.x + touchPos.x;
            yComponent = -transform.position.y + touchPos.y;

            angle = Mathf.Atan2(yComponent, xComponent) * Mathf.Rad2Deg;
            transform.position = Vector2.MoveTowards(transform.position, touchPos, speed * Time.deltaTime);

            int wallLayerMask = 1 << GameGlobalVariablesManager.WallLightLayer;
            Vector2 dest = new Vector2(touchPos.x, touchPos.y);
            Vector2 org = new Vector2(wallColliderObj.transform.position.x, wallColliderObj.transform.position.y);

            Collider2D[] allColliders = Physics2D.OverlapCircleAll(wallColliderObj.transform.position, 1.5f, wallLayerMask);
            for (int i = 0; i < allColliders.Length && !isColl; i++)
            {
                if (allColliders[i].gameObject.layer == GameGlobalVariablesManager.WallLightLayer)
                {
                    isColl = true;
                }
            }

            if (isColl)
            {
                StopAndThrow();
                playerBehaviour = PlayerBehaviour.IDLE;
                transform.position = oldPos;
                Debug.Log("knife oldPos : stop()");
            }
            else
            {
                Debug.Log("knife oldPos : " + oldPos + "newPos: " + transform.position);
                if (selectedObject == null)
                {
                    isRun = true;
                    distanceToAttack = 1;
                    speed = initialSpeed;
                }
                else
                {
                    if (distanceToPoint <= distanceToAttack * 2)
                    {
                        isRun = false;
                        speed = initialSpeed / 2;
                    }
                    else
                    {
                        isRun = true;
                        speed = initialSpeed;
                    }
                }
                // always run
                isRun = true;
                isInMove = true;
            }
        }

        if (isInMove)
        {
            characterAnimator.StopPlayback();
            CalculateAngle(angle);
            isIdle = false;
        }

        if (transform.position == touchPos)
        {
            isInMove = false;
            isRun = false;
            isIdle = true;
            idleDirection = prevMoveDirection;
            distanceToThrow = 0;
        }

        characterAnimator.SetBool("isInMove", isInMove);
        characterAnimator.SetBool("isRun", isRun);
        characterAnimator.SetBool("isIdle", isIdle);
        if (isInMove)
            characterAnimator.SetBool("isAttack", false);
        else
            characterAnimator.SetBool("isAttack", isAttack);
        characterAnimator.SetBool("isReact", isReact);
        characterAnimator.SetFloat("idleDirection", idleDirection);
        characterAnimator.SetFloat("moveDirection", moveDirection);
    }


    void StopAndThrow()
    {
        Idle();
        if (canThrow)
        {
            ThrowKnife();
        }
    }


    void Stop()
    {
        Idle();

        if (selectedObject != null)
        {
            if (!animatorStateInfo.IsTag("AttackTag") && (!animatorStateInfo.IsTag("ReactTag")))
            {
                if (!GameGlobalVariablesManager.isKnifeThrow)
                {
                    if (nextAttackTimer <= 0f)
                    {
                        // call Attack()
                        float distanceToPoint1 = Vector2.Distance(transform.position, selectedObject.transform.position);
                        if (distanceToPoint1 < distanceToAttack)
                        {
                            Attack();
                        }
                        nextAttackTimer = attackTime;
                    }
                    nextAttackTimer -= Time.deltaTime;
                }
                else
                {
                    //ThrowKnife ();
                }
            }
        }
        else
        {
            isAttack = false;
        }

        return;
    }


    void Idle()
    {
        if (!animatorStateInfo.IsTag("ReactTag"))
        {
            xComponent = -transform.position.x + touchPos.x;
            yComponent = -transform.position.y + touchPos.y;

            angle = Mathf.Atan2(yComponent, xComponent) * Mathf.Rad2Deg;

            CalculateAngle(angle);

            isInMove = false;
            isRun = false;
            idleDirection = prevMoveDirection;

            if (!animatorStateInfo.IsTag("AttackTag"))
            {
                isIdle = true;
            }

            characterAnimator.SetBool("isInMove", isInMove);
            characterAnimator.SetBool("isRun", isRun);
            characterAnimator.SetBool("isIdle", isIdle);
            characterAnimator.SetBool("isAttack", isAttack);
            characterAnimator.SetBool("isReact", isReact);
            characterAnimator.SetFloat("idleDirection", idleDirection);
            characterAnimator.SetFloat("moveDirection", moveDirection);
        }
    }


    void Attack()
    {
        Debug.Log("Attack()");
        //if(Input.GetKeyDown(KeyCode.A))
        if (selectedObject != null)
        {
            characterAnimator.SetFloat("idleDirection", idleDirection);
            characterAnimator.SetFloat("moveDirection", moveDirection);
            Debug.Log("Attack lay " + selectedObject.layer);
            if (LayerMask.LayerToName(selectedObject.layer).Equals("Objects"))
            {
                Debug.Log("Attack 2");
                characterAnimator.SetInteger("AttackRandom", 3 + Random.Range(0, 2));
            }
            else
            {
                Debug.Log("Attack 1");
                int r = Random.Range(1, 3);
                characterAnimator.SetInteger("AttackRandom", r);
            }
            characterAnimator.SetTrigger("Attack");
        }
        else
        {
            isAttack = false;
        }
    }


    void ThrowKnife()
    {
        if (selectedObject != null)
        {
            if (LayerMask.LayerToName(selectedObject.layer).Equals("Objects"))
                return;
        }
        else
        {
            return;
        }
        if (canThrow)
        {
            Debug.Log("ThrowKnife");
            characterAnimator.SetFloat("idleDirection", idleDirection);
            characterAnimator.SetFloat("moveDirection", moveDirection);
            characterAnimator.SetTrigger("Throw");
            canThrow = false;
        }
    }


    public void LaunchKnife()
    {
        if (selectedObject == null)
        {
            return;
        }
        else
        {
            if (LayerMask.LayerToName(selectedObject.layer).Equals("Objects"))
            {
                return;
            }
        }        
        GameGlobalVariablesManager.KnifeCount -= 1;
        if (GameGlobalVariablesManager.KnifeCount <= 0)
        {
            GameGlobalVariablesManager.KnifeCount = 0;
        }
        else
        {
            Debug.Log("LaunchKnife");
            knife = Instantiate(knifePrefab[(int)moveDirection], knifeThrowPoint.transform.position, Quaternion.identity) as GameObject;
            knife.SetActive(true);
            knife.GetComponent<ThrowKnife>().ThowKnifeTo(touchPos, selectedObject, true);
            AudioMgr.Inst.PlaySfx(SfxVals.Knife);
        }
    }


    void DestroyUsingKnife()
    {
        if (knife != null)
        {
            knife.transform.position = Vector2.MoveTowards(knife.transform.position, 
                new Vector2(selectedObject.transform.position.x, selectedObject.transform.position.y + 5), 
                knifeThrowSpeed * Time.deltaTime);
            if (knife.transform.position.x == selectedObject.transform.position.x)
            {
                if (selectedObject.GetComponent<AIComponent>() != null)
                {
                    selectedObject.GetComponent<AIComponent>().Death();
                    if (knife != null)
                        Destroy(knife.gameObject);
                    GameGlobalVariablesManager.isKnifeThrow = false;
                }
                else
                {
                    Destroy(selectedObject);
                    if (knife != null)
                        Destroy(knife.gameObject);
                    GameGlobalVariablesManager.isKnifeThrow = false;
                }
            }
        }
    }


    public void React()
    {
        if (!animatorStateInfo.IsTag("ReactTag") || !animatorStateInfo.IsTag("MovementTag"))
        {
            Debug.Log("Health : " + GameGlobalVariablesManager.playerHealth);
            GameGlobalVariablesManager.playerHealth -= GameGlobalVariablesManager.MaxLevel - GameGlobalVariablesManager.ArmorLevel + 1;
            UpdateAttackCombo(0);
            //Debug.Log ("Play react anim");
            characterAnimator.SetFloat("idleDirection", idleDirection);
            characterAnimator.SetFloat("moveDirection", moveDirection);
            int r = Random.Range(1, 5);
            //Debug.Log("Random value "+ 4);
            characterAnimator.SetInteger("ReactRandom", 1);
            characterAnimator.SetTrigger("React");

            StartCoroutine(ShowAttackColors());
        }
    }


    IEnumerator ShowAttackColors()
    {
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
        sr.color = new Color(1, 0.5f, 0.5f, 1);
        yield return new WaitForSeconds(0.2f);
        sr.color = new Color(1, 1, 1, 1);
    }


    public void PlayerDead()
    {
        // player dead
        if (GameGlobalVariablesManager.playerHealth <= 0 && !GameGlobalVariablesManager.PlayerDied)
        {
			if (GameGlobalVariablesManager.IsShowAd)
			{
				Advertisement.Show();
				Debug.Log("Showing ad");
			}	
            InGameHUD.instance.EnableDialogueHUD("Well fought ! \n better luck next time");
            GameGlobalVariablesManager.PlayerDied = true;
            SavedData.Inst.SaveAllData();
        }
    }


    public void AttackEnemy()
    {
        Debug.Log("AttackEnemy() : executing attack in player script");
        if (selectedObject != null)
        {
            int curID = selectedObject.GetInstanceID();
            //if (curID == prevID)
            switch (LayerMask.LayerToName(selectedObject.layer))
            {
                case "AI":
                    if (selectedObject.GetComponent<AIComponent>().isDead)
                    {
                        Debug.Log("selectedObject.GetComponent<AIComponent>()");
                    }
                    else
                    {
                        AudioMgr.Inst.PlaySfx(SfxVals.Sword);
                        if (coinUI != null)
                            CoinAnimUI.SetTrigger("CanBlink");
                        selectedObject.GetComponent<AIComponent>().React();
                        UpdateAttackCombo(1);
                    }
                    break;

                case "Objects":
                    PotParticleObj.SetActive(true);
                    PotParticleObj.transform.position = selectedObject.transform.position + new Vector3(0, 1.3f, 0);
                    StartCoroutine(HideAfterTime(0.6f));

                    Destroy(selectedObject.gameObject);
                    if (CoinAnimUI != null)
                        CoinAnimUI.SetTrigger("CanBlink");

                    if (coinAnim != null)
                        coinAnim.PlayCoinAnim(selectedObject.transform.position);

                    AudioMgr.Inst.PlaySfx(SfxVals.PotCrash);
                    AudioMgr.Inst.PlaySfx(SfxVals.CoinCollect);
                    GameGlobalVariablesManager.totalNumberOfCoins += 20;
                    break;
            }
            prevID = selectedObject.GetInstanceID();
        }
        else
        {
            isAttack = false;

            characterAnimator.SetBool("isInMove", isInMove);
            characterAnimator.SetBool("isRun", isRun);
            characterAnimator.SetBool("isIdle", isIdle);
            characterAnimator.SetBool("isAttack", isAttack);
            characterAnimator.SetBool("isReact", isReact);
            characterAnimator.SetFloat("idleDirection", idleDirection);
            characterAnimator.SetFloat("moveDirection", moveDirection);
        }
    }


    IEnumerator HideAfterTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        PotParticleObj.SetActive(false);
    }


    void Update()
    {
        animatorStateInfo = characterAnimator.GetCurrentAnimatorStateInfo(0);

        if (!GameGlobalVariablesManager.isKnifeThrow)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                hit = Physics2D.Raycast(target, Vector2.zero);

                if (hit.collider != null)
                {
                    layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
                    Debug.Log(layerName);
                    switch (layerName)
                    {
                        case "AI":
                            selectedObject = hit.collider.gameObject;
                            distanceToAttack = initialSpeed;
                            if (selectedObject.GetComponent<AIComponent>().selectionMarker != null)
                            {
                                selectedObject.GetComponent<AIComponent>().selectionMarker.SetActive(true);
                            }
                            touchPos = selectedObject.transform.position;
                            isAttack = true;
                            nextAttackTimer = 0;
                            playerBehaviour = PlayerBehaviour.MOVE;
                            break;

                        case "Objects":
                            selectedObject = hit.collider.gameObject;
                            distanceToAttack = intialDistanceToAttack / 2;
                            touchPos = selectedObject.transform.position;

                            Debug.Log("clicked on Objects");
                            isAttack = true;
                            nextAttackTimer = 0;
                            playerBehaviour = PlayerBehaviour.MOVE;
                            break;

                        case "WallLightLayer":
                            touchPos = this.transform.position;
                            isAttack = false;
                            nextAttackTimer = 0;
                            break;

                        case "AreaLock":
                            touchPos = this.transform.position;
                            isAttack = false;
                            nextAttackTimer = 0;
                            break;

                        case "Player":
                            touchPos = this.transform.position;
                            isAttack = false;
                            nextAttackTimer = 0;
                            break;

                        default:
                            touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            if (selectedObject != null)
                            {
                                //ponz.2do null check
                                //selectedObject.GetComponent<AIComponent>().selectionMarker.SetActive (false);

                                if (selectedObject.GetComponent<AIComponent>() != null)
                                    selectedObject.GetComponent<AIComponent>().selectionMarker.SetActive(false);
                            }
                            isAttack = false;
                            nextAttackTimer = 0;
                            playerBehaviour = PlayerBehaviour.MOVE;
                            break;
                    }// end of switch
                } //if  collider != null
                else
                {
                    touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Debug.Log("collider = null =" + touchPos);
                    if (selectedObject != null)
                    {
                        // ponz.2do
                        if (selectedObject.GetComponent<AIComponent>() != null)
                            selectedObject.GetComponent<AIComponent>().selectionMarker.SetActive(false);
                        selectedObject = null;
                    }
                    playerBehaviour = PlayerBehaviour.MOVE;
                }
            }

            if (GameGlobalVariablesManager.isPlayerSpin)
            {
                canSpin = true;
                sTime -= Time.deltaTime;
                Spin();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !canThrow)
            {
                distanceToThrow = initialDistanceToThrow;
                target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                hit = Physics2D.Raycast(target, Vector2.zero);

                if (hit.collider != null)
                {
                    layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
                    Debug.Log(layerName);

                    switch (layerName)
                    {
                        case "AI":
                            selectedObject = hit.collider.gameObject;
                            if (selectedObject.GetComponent<AIComponent>().selectionMarker != null)
                            {
                                selectedObject.GetComponent<AIComponent>().selectionMarker.SetActive(true);
                            }
                            touchPos = selectedObject.transform.position;
                            canThrow = true;
                            CoolOffKnife = 3.0f;
                            playerBehaviour = PlayerBehaviour.MOVEANDTHROW;
                            break;

                        case "Objects":
                            selectedObject = hit.collider.gameObject;
                            touchPos = selectedObject.transform.position;
                            Debug.Log("touch pos is generated");
                            canThrow = true;
                            playerBehaviour = PlayerBehaviour.MOVEANDTHROW;
                            break;

                        case "WallLightLayer":
                            touchPos = this.transform.position;
                            break;

                        case "AreaLock":
                            touchPos = this.transform.position;
                            break;

                        default:
                            touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            if (selectedObject != null)
                            {
                                selectedObject.GetComponent<AIComponent>().selectionMarker.SetActive(false);
                            }
                            playerBehaviour = PlayerBehaviour.MOVEANDTHROW;
                            break;
                    }
                }
                else
                {
                    touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (selectedObject != null)
                    {
                        selectedObject.GetComponent<AIComponent>().selectionMarker.SetActive(false);
                        selectedObject = null;
                    }
                    playerBehaviour = PlayerBehaviour.MOVEANDTHROW;
                }
            }
        }


        switch (playerBehaviour)
        {
            case PlayerBehaviour.IDLE:
                break;

            case PlayerBehaviour.MOVE:
                if (!GameGlobalVariablesManager.isFireBallThrown)
                    MoveTowardsPoint();
                break;

            case PlayerBehaviour.MOVEANDTHROW:
                MoveTowardsThrowPoint();
                break;

            case PlayerBehaviour.ATTACK:
                break;

            case PlayerBehaviour.REACT:
                break;
        }

        if (GameGlobalVariablesManager.isFireBallThrown)
        {
            fTimer -= Time.deltaTime;
            ShowFireBallCircle();
        }

        PlayerDead();

        Debug.DrawLine(wallColliderObj.transform.position, touchPos, Color.red, 0.5f);
        if (CoolOffTime < 0)
        {
            CoolOffTime = 10.0f;
            UpdateAttackCombo(0);
        }
        else
        {
            CoolOffTime -= Time.deltaTime;
        }

        if (canThrow)
        {
            CoolOffKnife -= Time.deltaTime;
            if (CoolOffKnife < 0)
            {
                CoolOffKnife = 3.0f;
                canThrow = false;
            }
        }
    }


    public void UpdateAttackCombo(int val)
    {
        if (val == 0)
            AttackCombo = 0;
        else
            AttackCombo += val;
        CoolOffTime = 10.0f;
        InGameHUD.instance.UpdateAttackCombo(AttackCombo);
    }


    public void AttackToIdleState()
    {
        isAttack = false;

        characterAnimator.SetBool("isInMove", isInMove);
        characterAnimator.SetBool("isRun", isRun);
        characterAnimator.SetBool("isIdle", isIdle);
        characterAnimator.SetBool("isAttack", isAttack);
        characterAnimator.SetBool("isReact", isReact);
        characterAnimator.SetFloat("idleDirection", idleDirection);
        characterAnimator.SetFloat("moveDirection", moveDirection);
    }
}

