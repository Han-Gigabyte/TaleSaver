using UnityEngine;

public class EnemySkillInfo : MonoBehaviour
{
    public GameObject projectilePrefab;     // �߻�ü ������
    public float projectileSpeed = 6f;      // �߻�ü �ӵ�
    public int damage = 10;                 // ������

    public GameObject skillEffectPrefab;    // [�߰�] ���� ����Ʈ (��: �߻� �غ� ����)
    public GameObject hitEffectPrefab;      // [�߰�] ���� ����Ʈ (��: ���� ����Ʈ)
}

