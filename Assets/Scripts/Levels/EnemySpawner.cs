using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    Dictionary<string, EnemyInfo> enemies; //creates dictionary that will store enemies
    Dictionary<string, Level> levels; // creates dictionary that will store levels

    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public GameObject gameOverUI;
    public GameObject winUI;
    public GameObject waveEndUI;
    public SpawnPoint[] SpawnPoints;

<<<<<<< HEAD
    public TMP_Text waveText;
    public TMP_Text enemiesKilledText;
    public TMP_Text timeText;
=======
    public Text waveText;
    public Text enemiesKilledText;
    public Text timeText;
    public Text gameOverText;
>>>>>>> ee494260c384f1f329e02a166c792bb48bab7459

    private Level currentLevel;
    private int currentWave = 0;
    private int enemiesKilledThisWave = 0;
    private float waveStartTime;
    private float waveDuration;
    private bool waitingForNextWave = false;
    private bool spawningFinished = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(typeof(RPNEvaluator.RPNEvaluator));
        enemies = new Dictionary<string, EnemyInfo>(); // go over enemies.json to get all possible enemies
        var enemytext = Resources.Load<TextAsset>("enemies");
        JToken jo = JToken.Parse(enemytext.text);
        foreach (var enemyToken in jo)
        {
            EnemyInfo e = enemyToken.ToObject<EnemyInfo>();
            enemies[e.name] = e;
        }

        levels = new Dictionary<string, Level>(); //go over levels json to get the possible levels (easy, med, diff)
        var leveltext = Resources.Load<TextAsset>("levels");
        JToken jo2 = JToken.Parse(leveltext.text);
        foreach (var levelToken in jo2)
        {
            Level l = levelToken.ToObject<Level>();
            levels[l.name] = l;
        }

        int i = 0;
        foreach (var item in levels) //for every difficulty made
        {
            string levelname = item.Key;
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, -95 + (65 * i));
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(levelname);
            i++;
        }



    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReturnToMenu()
    {
        StopAllCoroutines();
    }
    public void StartLevel(string levelname)
    {
        level_selector.gameObject.SetActive(false);
        if (levels == null)
        {
            Debug.LogError("Levels not found.");
            return;
        }

        // Find Level in Levels; loops through JObjects, looks at name. Return match
        currentLevel = levels[levelname];
        if (currentLevel == null)
        {
            Debug.LogError("Level not found:" + levelname);
            return;
        }

        // Assign the selected level when player clicks button
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();

        // Start first wave
        currentWave = 1;    // increment wave count
        StartCoroutine(WaveLoop());
    }

    // WAVE PREOGRESSIOn & EXECUTION
    // WAVE EXECUTION: Runs a single wave
    IEnumerator RunWave(Level level, int currentWave)
    {
        spawningFinished = false;
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;

        enemiesKilledThisWave = 0;
        waveStartTime = Time.time;

        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        foreach (var spawn in level.spawns)
        {
            yield return StartCoroutine(HandleSpawn(spawn, currentWave));
        }

        spawningFinished = true;
        yield return new WaitUntil(() =>
            spawningFinished && GameManager.Instance.enemy_count == 0);

        waveDuration = Time.time - waveStartTime;
    }

    // WAVE PROGRESSION: Wave Loop
    IEnumerator WaveLoop()
    {
        while (true)
        {
            if (currentLevel.waves > 0 && currentWave > currentLevel.waves)
            {
                GameManager.Instance.state = GameManager.GameState.GAMEOVER;

                if (winUI != null) //the win UI should appear when this condition is met
                {
                    winUI.SetActive(true);
                    gameOverText.text =  "Congratulations, you beat all the waves!";
                }
                if (waveEndUI != null) //and the waveend UI (which has the next button?) should not
                {
                    waveEndUI.SetActive(false);
                }
                waitingForNextWave = false; //and since the game is over, we aren't waiting for the next wave.

                yield break;
            }

            yield return StartCoroutine(RunWave(currentLevel, currentWave));

            int completedWave = currentWave;
            currentWave++;
            GameManager.Instance.state = GameManager.GameState.WAVEEND;

            waitingForNextWave = true;

            if (waveEndUI != null)
            {
                waveEndUI.SetActive(true);

                if (waveText != null)
                    waveText.text = "Wave " + completedWave + " complete";

                if (enemiesKilledText != null)
                    enemiesKilledText.text = "Enemies killed: " + enemiesKilledThisWave;

                if (timeText != null)
                    timeText.text = "Time: " + waveDuration.ToString("F1") + "s";
            }

            yield return new WaitUntil(() => waitingForNextWave == false);

            if (waveEndUI != null)
                waveEndUI.SetActive(false);
        }
    }

    public void nextWave()
    {
        waitingForNextWave = false;
    }

    // Helper Function - Spawns enemies based on the spawn's count, delay, and sequence.
    IEnumerator HandleSpawn(Spawn spawn, int wave)
    {
        EnemyInfo baseEnemy = enemies[spawn.enemy];
        var vars = new Dictionary<string, int>()
        {
            { "wave", wave },
            { "base", baseEnemy.hp }
        };

        // RPNEvaluator allows for float implementation.
        // But totalCount is an int, so we for cast for integer safety
        int totalCount = (int)RPNEvaluator.RPNEvaluator.Evaluate(spawn.count, vars);

        float delayValue = string.IsNullOrEmpty(spawn.delay)
            ? 1f
            : RPNEvaluator.RPNEvaluator.Evaluate(spawn.delay, vars);

        List<int> sequence = (spawn.sequence != null && spawn.sequence.Count > 0)
            ? spawn.sequence
            : new List<int> { 1 };

        int spawned = 0;
        int seqIndex = 0;

        while (spawned < totalCount)
        {
            int burst = sequence[seqIndex % sequence.Count];
            seqIndex++;

            for (int i = 0; i < burst && spawned < totalCount; i++)
            {
                SpawnEnemyWithStats(baseEnemy, spawn);
                spawned++;
            }
            yield return new WaitForSeconds(delayValue);
        }
        yield return null;
    }

    void SpawnEnemyWithStats(EnemyInfo baseEnemy, Spawn spawn)
    {
        SpawnPoint spawn_point = GetSpawnPoint(spawn.location);

        // Edge Case 1: SpawnPoints array is empty or null
        if (SpawnPoints == null || SpawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points configured.");
            return;
        }

        // Edge Case 2: GetSpawnPoint returns null due to invalid location string
        if (spawn_point == null)
        {
            Debug.LogWarning($"No spawn point found for '{spawn.location}', using fallback.");
            spawn_point = SpawnPoints[0];
        }

        Vector2 offset = Random.insideUnitCircle * 1.8f;

        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        // Sprite
        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(baseEnemy.sprite);
        GameManager.Instance.AddEnemy(new_enemy);
        EnemyController en = new_enemy.GetComponent<EnemyController>();

        // default variables for RPN - KEEP IN FUNCTIONS
        // Moving it to a general class-level dictionary will cause 3 issues:
            // Stale Wave Values: old wave value may persist || is not updated everywhere
            // Cross-Level Contamination: values from one level may affect another || if not cleared properly
            // Hard to Debug: tracking variable changes across levels becomes difficult || if variables are modified in unexpected ways
        var vars = new Dictionary<string, int>()
        {
            { "wave", currentWave },
            { "base", baseEnemy.hp }
        };

        // Overrides
        int hp = !string.IsNullOrEmpty(spawn.hp)
            ? RPNEvaluator.RPNEvaluator.Evaluate(spawn.hp, vars)
            : baseEnemy.hp;
        int speed = !string.IsNullOrEmpty(spawn.speed)
            ? RPNEvaluator.RPNEvaluator.Evaluate(spawn.speed, vars)
            : baseEnemy.speed;
        int damage = !string.IsNullOrEmpty(spawn.damage)
            ? RPNEvaluator.RPNEvaluator.Evaluate(spawn.damage, vars)
            : baseEnemy.damage;

        en.hp = new Hittable(hp, Hittable.Team.MONSTERS, new_enemy);

        GameObject enemyObj = new_enemy;
        en.hp.OnDeath += () =>
        {
            enemiesKilledThisWave++;
            GameManager.Instance.RemoveEnemy(enemyObj);
            Destroy(enemyObj);
        };
        en.speed = speed;
        en.damage = damage;
    }

    SpawnPoint GetSpawnPoint(string location)
    {
        if (SpawnPoints == null || SpawnPoints.Length == 0)
        {
            Debug.LogError("SpawnPoints array is empty!");
            return null;
        }

        if (string.IsNullOrEmpty(location) || location == "random")
            return SpawnPoints[Random.Range(0, SpawnPoints.Length)];

        if (location.Contains("red"))
            return SpawnPoints.Where(p => p.kind == SpawnPoint.SpawnName.RED)
                .OrderBy(_ => Random.value).FirstOrDefault();

        if (location.Contains("green"))
            return SpawnPoints.Where(p => p.kind == SpawnPoint.SpawnName.GREEN)
                .OrderBy(_ => Random.value).FirstOrDefault();

        if (location.Contains("bone"))
            return SpawnPoints.Where(p => p.kind == SpawnPoint.SpawnName.BONE)
                .OrderBy(_ => Random.value).FirstOrDefault();

        return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
    }

    // Stores Levels from levels.json; 
    public class Level
    {
        public string name;         // name of level
        public int waves;           // total waves in level
        public List<Spawn> spawns;  // lists of Spawns, Wave behavior 
    }

    // Stores Spawns from levels.json - of a particular enemy.
    public class Spawn
    {
        public string enemy;        // type of enemy to spawn
        public string count;        // number of enemies to spawn
        public List<int> sequence;  // how many enemies to spawn after each delay
        public string delay;        // delay between spawns
        public string location;     // location to spawn (spawn point)

        public string hp;           // hp of enemy to spawn (modification to base hp)
        public string speed;        // speed of enemy to spawn (modification to base speed)
        public string damage;       // damage of enemy to spawn (modification to base damage)
    }

    // Stores enemies from enemies.json
    public class EnemyInfo
    {
        public string name; // name of enemy
        public int sprite;  // index of sprite in EnemySpriteManager
        public int hp;      // hp of enemy
        public int speed;   // speed of enemy
        public int damage;  // damage of enemy (base value)
    }
}