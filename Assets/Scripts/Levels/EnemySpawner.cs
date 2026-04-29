using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class EnemyInfo //stores enemies; their names, sprite, hp, speed, and damage (base values)
{
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;
    

}

public class SpawnCharacteristics //class that manages spawns of a particular enemy, and modifications to it
{
    public string enemy;
    public string count;
    public string hp;
    public string damage;
    public int delay;
    public List<int> sequence;
    public string location;
}
public class Level // level characteristics; the difficulty name, how many waves its got, the list of spawns it comes with
{
    public string name;
    public int waves;
    public List<SpawnCharacteristics> spawns;
}

public class EnemySpawner : MonoBehaviour
{

    Dictionary<string, EnemyInfo> enemies; //creates dictionary that will store enemies
    Dictionary<string, Level> levels; // creates dictionary that will store levels

    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemies = new Dictionary<string, EnemyInfo>(); // go over enemies.json to get all possible enemies
        var enemytext = Resources.Load<TextAsset>("enemies");
        JToken jo = JToken.Parse(enemytext.text);
        foreach(var enemyToken in jo)
        {
            EnemyInfo e = enemyToken.ToObject<EnemyInfo>(); 
            enemies[e.name] = e;
        }

        levels = new Dictionary<string, Level>(); //go over levels json to get the possible levels (easy, med, diff)
        var leveltext = Resources.Load<TextAsset>("levels");
        JToken jo2 = JToken.Parse(leveltext.text);
        foreach(var levelToken in jo2)
        {
            Level l = levelToken.ToObject<Level>();
            levels[l.name] = l;
        }

       int i = 0;
       foreach(var item in levels) //for every difficulty made
        {
            string levelname = item.Key;
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, 130*(i + 1));
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(levelname);
            i++;
        }

    

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    {
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        Level leveltostart = levels[levelname];
        StartCoroutine(SpawnWave(leveltostart));
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave(Level wave)
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        for (int i = 0; i < 10; ++i)
        {
            yield return SpawnEnemy();
        }
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }

    IEnumerator SpawnEnemy(EnemyInfo enemyToSpawn)
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
                
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(enemyToSpawn.sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(enemyToSpawn.hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = enemyToSpawn.speed;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
}
