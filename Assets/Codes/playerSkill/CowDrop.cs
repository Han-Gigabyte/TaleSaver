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
            transform.position += Vector3.down * fastFallSpeed * Time.deltaTime;

            if (transform.position.y <= landingY)
            {
                hasLanded = true;
                transform.position = new Vector3(transform.position.x, landingY, transform.position.z);

                // ���� �ִϸ��̼� ����
                if (impactFXPrefab != null)
                {
                    Vector3 fxPosition = transform.position + new Vector3(0f, 1.5f, 0f); // y�� 0.3f ���� �ø�
                    Debug.Log($" Dust FX ���� ��ġ: {transform.position}");
                    Instantiate(impactFXPrefab, transform.position, Quaternion.identity);

                }

                // 3�� �� �� ����
                StartCoroutine(FadeOutAndDestroy());
            }
        }
    }


    private IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(0.3f); // 1�ʰ� ����
        Destroy(gameObject);
    }
}

