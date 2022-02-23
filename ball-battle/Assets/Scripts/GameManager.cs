using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int matchOrd = 0;
    public static bool matchOver = false;
    public static int playerPoint = 0;
    public static int enemyPoint = 0;

    public Globals Globals;
    public GameUICtrl GameUICtrl;

    public float timeLeft = Params.timeLimit;
    
    public GameObject gameplayComponents;
    public GameObject ballPrefab;
    GameObject ballObj;
    public Soldier ballHolder;

    public PlayerParams enemy;
    public PlayerParams player;

    public float currentEnemyEnergy = 0;
    public float currentPlayerEnergy = 0;

    List<EnergyPoint> enemyEnergy = new List<EnergyPoint>();
    List<EnergyPoint> playerEnergy = new List<EnergyPoint>();

    public List<SoldierAtk> AtkSoldiers = new List<SoldierAtk>();
    public List<SoldierDef> DefSoldiers = new List<SoldierDef>();

    public GameObject soldierAtkPrefab;
    public GameObject soldierDefPrefab;

    public GameObject enemyGate;
    public GameObject playerGate;

    public void AddEnergy(bool isEnemy, EnergyPoint energyPoint)
    {
        // used by GameUICtrl to add energy
        if (isEnemy)
        {
            enemyEnergy.Add(energyPoint);
        }
        else
        {
            playerEnergy.Add(energyPoint);
        }
    }

    void ClickHandler(Vector2 pos)
    {
        // 2D screen => 3d world space
        Ray ray = Camera.main.ScreenPointToRay(pos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //OnscreenDebugOutput.Log($"Touch at {pos}, hit {hit.transform.tag} at {hit.point}");
            if (hit.transform.tag == "PlayerField")
            {
                if (currentPlayerEnergy > player.energyCost)
                {
                    currentPlayerEnergy -= player.energyCost;
                    if (player.isAtk)
                    {
                        GameObject soldierObjPlayer = Instantiate(soldierAtkPrefab, hit.point, Quaternion.identity, gameplayComponents.transform);
                        SoldierAtk s = soldierObjPlayer.GetComponent<SoldierAtk>();
                        s.ball = ballObj;
                        s.Soldier.SetAttr(true);
                        s.Soldier.GameManager = this;
                        AtkSoldiers.Add(s);
                        //OnscreenDebugOutput.Log($"Added {s} at {hit.point}");
                    }
                    else
                    {
                        GameObject soldierObj = Instantiate(soldierDefPrefab, hit.point, Quaternion.identity, gameplayComponents.transform);
                        SoldierDef s = soldierObj.GetComponent<SoldierDef>();
                        s.Soldier.SetAttr(true);
                        s.Soldier.GameManager = this;
                        DefSoldiers.Add(s);
                        //OnscreenDebugOutput.Log($"Added {s} at {hit.point}");
                    }
                }
            }
            
            else if (hit.transform.tag == "EnemyField")
            {
                if (currentEnemyEnergy > enemy.energyCost)
                {
                    currentEnemyEnergy -= enemy.energyCost;
                    if (enemy.isAtk)
                    {
                        GameObject soldierObjEnemy = Instantiate(soldierAtkPrefab, hit.point, Quaternion.Euler(0, 180, 0), gameplayComponents.transform);
                        SoldierAtk s = soldierObjEnemy.GetComponent<SoldierAtk>();
                        s.ball = ballObj;
                        s.Soldier.SetAttr(false);
                        s.Soldier.GameManager = this;
                        AtkSoldiers.Add(s);
                    }
                    else
                    {
                        GameObject soldierObjEnemy = Instantiate(soldierDefPrefab, hit.point, Quaternion.Euler(0, 180, 0), gameplayComponents.transform);
                        SoldierDef s = soldierObjEnemy.GetComponent<SoldierDef>();
                        s.Soldier.SetAttr(false);
                        s.Soldier.GameManager = this;
                        DefSoldiers.Add(s);
                    }
                }
            }
        }
    }

    public void Start()
    {
        if (!gameplayComponents.activeInHierarchy) return;
        Globals.gamePaused = true;
        Initialize();

        GameUICtrl.StartCountdown();
    }

    public void Initialize()
    {
        // Setting things
        // Doing some cleanups
        matchOrd += 1;
        timeLeft = 140f;
        matchOver = false;
        foreach (SoldierAtk soldierAtk in AtkSoldiers)
        {
            Destroy(soldierAtk.gameObject);
        }
        foreach (SoldierDef soldierDef in DefSoldiers)
        {
            Destroy(soldierDef.gameObject);
        }
        AtkSoldiers = new List<SoldierAtk>();
        DefSoldiers = new List<SoldierDef>();
        currentEnemyEnergy = 0;
        currentPlayerEnergy = 0;
        FillEnergy(enemyEnergy, 0);
        FillEnergy(playerEnergy, 0);

        // Determines whether player/enemy is Attacker/Defender
        enemy = Params.GetPlayerParams(false);
        player = Params.GetPlayerParams(true);
    }

    public void MatchStart()
    {
        // Set the field on which can be put Soldiers
        string tag = "PlayerField";
        if (!player.isAtk)
        {
            tag = "EnemyField";
        }

        // Generate a random position for the ball
        // Better limit this random range to prevent some edge cases
        GameObject field = GameObject.FindGameObjectWithTag(tag);
        Vector3[] vertices = field.GetComponent<MeshFilter>().mesh.vertices;
        Vector3 topLeft = field.transform.TransformPoint(vertices[0]);
        Vector3 botRight = field.transform.TransformPoint(vertices[120]);

        float acceptableRange = .95f; // 5% of the field
        topLeft *= acceptableRange;
        botRight *= acceptableRange;

        if (ballObj == null)
        {
            ballObj = Instantiate(ballPrefab, gameplayComponents.transform);
        }
        // y-position of the ball should be 0
        // But not in AR if I only set its pos once 
        Vector3 pos = new Vector3(Random.Range(topLeft.x, botRight.x), field.transform.position.y, Random.Range(topLeft.z, botRight.z));
        ballObj.transform.position = pos;
        //pos = new Vector3(ballObj.transform.position.x, 0, ballObj.transform.position.z);
        //ballObj.transform.localPosition = pos;
        ballObj.SetActive(true);
    }

    void FillEnergy(List<EnergyPoint> energyPoints, float value)
    {
        // Fill the energy bar, using integer part & fractional part
        // i.e: value == 4.5 => 4 Energy Point filled & 0.5 filling
        int integerPart = (int)(value);
        float fractionalPart = value - integerPart;
        for (int i = 0; i < integerPart; i++)
        {
            EnergyPoint energyPoint = energyPoints[i];
            energyPoint.SetValue(1);
        }
        if (integerPart >= energyPoints.Count) return;
        for (int i = integerPart; i < energyPoints.Count; i++)
        {
            EnergyPoint energyPoint = energyPoints[i];
            energyPoint.SetValue(0);
        }
        energyPoints[integerPart].SetValue(fractionalPart);
    }

    void Update()
    {
        if (Globals.gamePaused) return;

        if (timeLeft <= 0)
        {
            MatchOver(false, true);
        }

        // increase energy over time
        if (currentEnemyEnergy < Params.energyBarCount)
        {
            currentEnemyEnergy += enemy.energyRegen * Time.deltaTime;
            FillEnergy(enemyEnergy, currentEnemyEnergy);
        }
        if (currentPlayerEnergy < Params.energyBarCount)
        {
            currentPlayerEnergy += player.energyRegen * Time.deltaTime;
            FillEnergy(playerEnergy, currentPlayerEnergy);
        }

        // Touch handler
        Touch[] touches = Input.touches;
        if (touches.Length > 0)
        {
            Touch touch = touches[0];
            if (touch.phase == TouchPhase.Ended)
            {
                ClickHandler(touch.position);
            }
        }

        // Mouse handler
        if (Input.GetMouseButtonDown(0)) // use InputSystem later
        {
            ClickHandler(Input.mousePosition);
        }
    }

    public void MatchOver(bool isAtkWon, bool isDraw = false)
    {
        Globals.gamePaused = true;
        matchOver = true;

        foreach (SoldierAtk soldierAtk in AtkSoldiers)
        {
            soldierAtk.Soldier.SetStatus(false);
        }

        foreach (SoldierDef soldierDef in DefSoldiers)
        {
            soldierDef.Soldier.SetStatus(false);
        }

        // Match result
        if (!isDraw)
        {
            if (isAtkWon)
            {
                Debug.Log("Match over. Attacker won");
                if (player.isAtk)
                {
                    playerPoint += 1;
                    GameUICtrl.ShowResult(true, true);
                }
                else
                {
                    enemyPoint += 1;
                    GameUICtrl.ShowResult(false, false);
                }
            }
            else
            {
                Debug.Log("Match over. Defender won");
                if (player.isAtk)
                {
                    enemyPoint += 1;
                    GameUICtrl.ShowResult(false, true);
                }
                else
                {
                    playerPoint += 1;
                    GameUICtrl.ShowResult(true, false);
                }
            }
        }
        else
        {
            GameUICtrl.ShowResult(false, false, true);
        }

        Debug.Log($"Current score: Player {playerPoint} - {enemyPoint} Enemy");
    }
}
