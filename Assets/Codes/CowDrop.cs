using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

using UnityEngine;

public class CowDrop : MonoBehaviour
{
    public float fastFallSpeed = 15f;
    public float slowFallSpeed = 2f;
    public float slowFallDistance = 1f;

    private float landingY;
    private bool hasLanded = false;
    private bool isSlowFalling = false;
    private float slowFallStartY;
    private Rigidbody2D rb;
    public GameObject impactFXPrefab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetLandingY(float y)
    {
        landingY = y;
    }

    private void Update()
    {
        if (!hasLanded)
        {
            // ������ ���� �������� ����
            transform.position += Vector3.down * fastFallSpeed * Time.deltaTime;

            if (transform.position.y <= landingY)
            {
                hasLanded = true;
                isSlowFalling = true;
                slowFallStartY = transform.position.y;

                // �� ���� ��
                if (impactFXPrefab != null)
                {
                    Instantiate(impactFXPrefab, transform.position, Quaternion.identity);
                }

                // �߷� ��Ȱ��ȭ
                if (rb != null)
                {
                    rb.gravityScale = 0f;
                    rb.velocity = Vector2.zero;
                }

                // ī�޶� ����
                //SkillManager skillManager = FindObjectOfType<SkillManager>();
                //if (skillManager != null)
                //{
                    //skillManager.StartCoroutine(skillManager.ShakeCamera(0.3f, 0.2f));
                //}
            }
        }
        else if (isSlowFalling)
        {
            transform.position += Vector3.down * slowFallSpeed * Time.deltaTime;

            if (transform.position.y <= slowFallStartY - slowFallDistance)
            {
                isSlowFalling = false;

                // ���� �� ���� �� �����
                StartCoroutine(FadeOutAndDestroy());
            }
        }
    }

    private IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(0f); // 1�ʰ� ����
        Destroy(gameObject);
    }
}

