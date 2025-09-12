using System.Collections;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject[] enemy; // 적
    public GameObject[] boss; // 보스
    public GameObject meteor; // 메테오
    public GameObject meteorLine; // 메테오 경고
    public Transform[] spawnPoint; // 생성 위치

    [Header("Spawn Rate")]
    public float minSpawnRate; // 최소 적 생성 시간
    public float maxSpawnRate; // 최대 적 생성 시간

    [Header("Boss Spawn Timer")]
    public float bossSpawnTime; // 보스 생성 시간
    

    float nextSpawn = 0f;
    float bossTimer = 0f; // 보스 타이머
    int stageLevel = 0; // 스테이지 레벨
    bool bossStage = false; // 보스 스테이지

    void Update()
    {
        if(!bossStage) // 보스 스테이지가 아닐때는 적과 메테오 소환
        {
            bossTimer += Time.deltaTime;
            RandomSpawn();

            if(bossTimer >= bossSpawnTime) // 보스 생성 시간이 되면 보스 스테이지를 활성화 하고 보스 생성
            {
                bossStage = true;
                BossSpawn();
            }
        }
    }
    void RandomSpawn() // 적을 랜덤한 시간에 생성
    {
        nextSpawn -= Time.deltaTime; // 생성 시간에서 현재 시간을 계속 감소시켜 0이 되면 적을 생성, 생성 시킨 후에 다시 생성 시간을 랜덤으로 설정
        if (nextSpawn <= 0f)
        {
            EnemySpawn();
            MeteorLineSpawn();
            nextSpawn = Random.Range(minSpawnRate, maxSpawnRate);
        }
    }
    void EnemySpawn() // 랜덤한 적을 랜덤한 위치에 생성
    {
        int randomEnemy = Random.Range(0, 3);
        int randomSpawn = Random.Range(0, 5);
        Instantiate(enemy[randomEnemy], spawnPoint[randomSpawn].position, spawnPoint[randomSpawn].rotation);
    }
    void BossSpawn() // 보스 레벨에 맞게 보스 생성
    {
        Instantiate(boss[stageLevel], spawnPoint[2].position, spawnPoint[2].rotation);
    }
    public void NextStage() // 다음 스테이지
    {
        bossTimer = 0f; // 보스 생성 시간 초기화
        stageLevel += 1; // 스테이지 레벨 +1
        bossStage = false; // 보스 스테이지 상태 끄기
    }
    void MeteorLineSpawn() // 메테오 경고 랜덤 생성
    {
        int randomSpawn = Random.Range(0, 5);
        StartCoroutine(MeteorSpawn(randomSpawn));
    }
    IEnumerator MeteorSpawn(int index) // 메테오 생성
    {
        GameObject line = Instantiate(meteorLine, spawnPoint[index].position + new Vector3(0, -4, 0), spawnPoint[index].rotation);
        yield return new WaitForSeconds(1f);
        Destroy(line);
        yield return new WaitForSeconds(0.5f);
        Instantiate(meteor, spawnPoint[index].position, spawnPoint[index].rotation);
    }
}

