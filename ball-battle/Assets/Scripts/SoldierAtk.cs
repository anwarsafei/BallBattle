using System.Collections;
using UnityEngine;

public class SoldierAtk : MonoBehaviour
{
    public Globals Globals;
    public GameManager GameManager;
    public Soldier Soldier;
    public GameObject ball;
    public GameObject CircleIndicator;

    public bool isCaught;

    Vector3 gatePos;

    void Start()
    {
        GameManager = Soldier.GameManager;
        if (Soldier.isPlayer)
        {
             gatePos = GameManager.enemyGate.transform.position;
        }
        else
        {
            gatePos = GameManager.playerGate.transform.position;
        }
    }

    SoldierAtk GetNearestAtker()
    {
        // iterate through GameManager.AtkSoldiers and compare those distances
        float minDistance = float.PositiveInfinity;
        SoldierAtk potentialAtker = null;
        foreach (SoldierAtk soldierAtk in GameManager.AtkSoldiers)
        {
            if (!soldierAtk.Soldier.isActive || soldierAtk == this) continue;
            float distance = Vector3.Distance(soldierAtk.transform.position, transform.position);
            if (distance < minDistance)
            {
                potentialAtker = soldierAtk;
                minDistance = distance;
            }
        }
        return potentialAtker;
    }

    IEnumerator Reactivate()
    {
        if (Globals.gamePaused) yield return null;
        yield return new WaitForSeconds(Params.reactivateTimeAtk);
        if (!GameManager.matchOver)
        {
            Soldier.SetStatus(true);
            Soldier.SetKinematic(true);
            isCaught = false;
        }
    }

    void Update()
    {
        if (Globals.gamePaused)
        {
            Soldier.animator.SetBool("Run", false);
            return;
        }
        if (Soldier.isActive)
        {
            Soldier.animator.SetBool("Run", true);

            // chase the ball if it's not hold by an Attacker
            if (GameManager.ballHolder == null)
            {
                Soldier.animator.speed = 1;

                CircleIndicator.SetActive(false);
                Soldier.SetKinematic(true);
                transform.position = Vector3.MoveTowards(transform.position, ball.transform.position, Params.speedNormalMultiplierAtk * Time.deltaTime);

                // there should be an *acceptable range* that allows Atker
                // to *catch* the ball if it's in the range
                if (Vector3.Distance(transform.position, ball.transform.position) <= Params.spacingBallAtk)
                {
                    GameManager.ballHolder = Soldier;
                    ball.transform.parent = Soldier.transform.parent;
                }
            }

            // if holding the ball
            else if (GameManager.ballHolder == Soldier)
            {
                Soldier.animator.speed = Params.speedCarryMultiplier / Params.speedNormalMultiplierAtk;

                CircleIndicator.SetActive(true);
                // if caught: find nearest Atker, pass the ball, become inactive
                // if no nearest Atker available, game over
                if (isCaught)
                {
                    CircleIndicator.SetActive(false);
                    //Debug.Log($"Caught by Defender at {transform.position}");
                    SoldierAtk nearestAtker = GetNearestAtker(); //
                    if (nearestAtker == null)
                    {
                        GameManager.MatchOver(false);
                        Soldier.SetStatus(false);
                        return;
                    }
                    
                    // rotate to the nearest Atker & pass the ball
                    Vector3 targetDirection = nearestAtker.transform.position - transform.position;
                    Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360, 0f);
                    transform.rotation = Quaternion.LookRotation(newDirection);

                    Soldier.animator.SetBool("Pass", true);
                    Soldier.animator.SetBool("Run", false);
                    Soldier.animator.SetBool("Pass", false);

                    GameManager.ballHolder = null;
                    //Debug.Log($"Nearest Attacker at {nearestAtker.transform.position}");
                    Soldier.SetStatus(false);
                    Soldier.SetKinematic(true);

                    ball.transform.parent = null;
                    ball.GetComponent<Ball>().passedTo = nearestAtker;
                    
                    StartCoroutine(Reactivate());
                }
                else
                {
                    Soldier.animator.speed = 1;
                    Soldier.SetKinematic(false);
                    transform.position = Vector3.MoveTowards(transform.position, gatePos, Params.speedCarryMultiplier * Time.deltaTime);
                }
            }

            // If no ball to chase or hold, move towards the opponent's fence
            // and destroy on collision
            // Using *kinematic* allows SoldierAtk to *pass through* others,
            // but it can't collide with fences (except for changing
            // project collision settings - but that may cause some
            // unexpected results - idk, not enough time to test xD)
            // So the simpler approach is to compare current position
            // with the Gate's z-position. If equal => Destroy
            else
            {
                Soldier.animator.speed = 1;

                CircleIndicator.SetActive(false);
                Soldier.SetKinematic(true);
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, gatePos.y, gatePos.z), Params.speedNormalMultiplierAtk * Time.deltaTime);

                if (transform.position.z == gatePos.z)
                {
                    GameManager.AtkSoldiers.Remove(this);

                    // This animation needs a bit *tweaking*
                    Soldier.animator.SetBool("Down", true);
                    Soldier.SetStatus(false);
                    Destroy(gameObject, 1.2f);
                }
            }
        }
    }
}
