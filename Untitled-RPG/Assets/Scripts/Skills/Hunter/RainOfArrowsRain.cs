using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RainOfArrowsRain : MonoBehaviour
{
    public RainOfArrows skill;
    float frequencyTimer;     

    Collider[] enemiesBelow;
    
    void Start() {
        Destroy(gameObject, skill.skillDistance);
    }
    void Update() {
        if (Time.time - frequencyTimer >= 1/skill.rainFrequency) {
            ShootArrow();
            frequencyTimer = Time.time;
        }
    }

    void ShootArrow () {
        enemiesBelow = Physics.OverlapSphere(transform.position + Vector3.down * 20, skill.rainRadius, LayerMask.GetMask("Enemy"));
        bool shotEnemy = enemiesBelow.Length == 0 ? false : Random.value < skill.rainChanceToHitEnemy ? true : false;

        Vector3 pos;
        if (shotEnemy) {
            int enemyIndex = Random.Range(0, enemiesBelow.Length);
            pos.x = enemiesBelow[enemyIndex].transform.position.x + Random.Range(-enemiesBelow[enemyIndex].GetComponentInParent<Enemy>().globalEnemyBounds().x/2, enemiesBelow[enemyIndex].GetComponentInParent<Enemy>().globalEnemyBounds().x/2);
            pos.z = enemiesBelow[enemyIndex].transform.position.z + Random.Range(-enemiesBelow[enemyIndex].GetComponentInParent<Enemy>().globalEnemyBounds().z/2, enemiesBelow[enemyIndex].GetComponentInParent<Enemy>().globalEnemyBounds().z/2);
        } else {
            pos.x = transform.position.x + Random.Range(-skill.rainRadius, skill.rainRadius);
            pos.z = transform.position.z + Random.Range(-skill.rainRadius, skill.rainRadius);
        }
        pos.y = transform.position.y;

        Arrow ar = Instantiate(skill.arrowPrefab, pos, Quaternion.identity).GetComponent<Arrow>();
        ar.instantShot = true;
        ar.Shoot(10, ar.transform.position + Vector3.down * 10, CalculateDamage.damageInfo(skill.skillTree, skill.baseDamagePercentage), skill.skillName);
    }
}
