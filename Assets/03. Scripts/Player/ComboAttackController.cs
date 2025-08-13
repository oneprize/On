using System;
using UnityEngine;

/// <summary>
/// �ϳ��� �ִϸ��̼� ����(Ŭ��)�� 1~N���� ���(���׸�Ʈ)�� ������� ��,
/// �ִϸ��̼� "�̺�Ʈ"�� �Է�â(open/close)�� ���׸�Ʈ ��(end)�� �˷��ָ�
/// Ŭ�� �Է¿� ���� ���� ���׸�Ʈ�� �Ѿ�� �޺� ��Ʈ�ѷ�.
/// </summary>
public class ComboAttackEventDriver : MonoBehaviour
{
    [Header("�ʼ�")]
    public Animator animator;
    [Tooltip("�޺� �ִϸ��̼��� ����Ǵ� Animator ���� �̸�")]
    public string stateName = "Slash";
    [Tooltip("����� ���̾� �ε���")]
    public int layer = 0;

    [Header("�Է�")]
    public string inputButton = "Fire1";   // �⺻: ���콺 ����

    [Header("���׸�Ʈ ���� ���� (����ȭ �ð�)")]
    [Tooltip("�� ���(1Ÿ/2Ÿ/3Ÿ...)�� ���� ����(0~1)")]
    public float[] segmentStarts = new float[] { 0.00f, 0.33f, 0.66f }; // ��: 3Ÿ

    [Header("���� ����")]
    [Tooltip("���׸�Ʈ�� ������ �� ���̵� �ð�(��)")]
    public float crossFadeTime = 0.05f;

    // ���� ����
    int _currentIndex = -1;     // ���� ���׸�Ʈ (0����)
    bool _isAttacking = false;  // �޺� ������
    bool _windowOpen = false;  // ���� ���׸�Ʈ�� �Է�â ���� ����
    bool _queueNext = false;  // ���� ���׸�Ʈ ���� ����

    void Reset()
    {
        if (!animator) animator = GetComponentInParent<Animator>();
    }

    void Update()
    {
        // �Է� ó��
        if (Input.GetButtonDown(inputButton))
        {
            if (!_isAttacking)
            {
                StartCombo();
            }
            else if (_windowOpen)
            {
                // �Է�â ���� ������ ���� Ÿ ����
                _queueNext = true;
                // Debug.Log("[Combo] Queue Next");
            }
        }
    }

    /// <summary>�޺� ����(1Ÿ�� ����)</summary>
    public void StartCombo()
    {
        if (animator == null || segmentStarts == null || segmentStarts.Length == 0) return;

        _isAttacking = true;
        _queueNext = false;
        _currentIndex = 0;
        _windowOpen = false;

        float startT = Mathf.Clamp01(segmentStarts[_currentIndex]);
        animator.CrossFadeInFixedTime(stateName, crossFadeTime, layer, startT);
        // Debug.Log("[Combo] Start 1st");
    }

    /// <summary>���� ���׸�Ʈ �Է�â ����(�ִϸ��̼� �̺�Ʈ�� ȣ��)</summary>
    /// <param name="segmentIndex">0���� ����</param>
    public void Ev_OpenWindow(int segmentIndex)
    {
        if (!_isAttacking || segmentIndex != _currentIndex) return;
        _windowOpen = true;
        // Debug.Log($"[Combo] OpenWindow seg:{segmentIndex}");
    }

    /// <summary>���� ���׸�Ʈ �Է�â Ŭ����(�ִϸ��̼� �̺�Ʈ�� ȣ��)</summary>
    public void Ev_CloseWindow(int segmentIndex)
    {
        if (!_isAttacking || segmentIndex != _currentIndex) return;
        _windowOpen = false;
        // Debug.Log($"[Combo] CloseWindow seg:{segmentIndex}");
    }

    /// <summary>���� ���׸�Ʈ ���� ����(�ִϸ��̼� �̺�Ʈ�� ȣ��)</summary>
    public void Ev_EndSegment(int segmentIndex)
    {
        if (!_isAttacking || segmentIndex != _currentIndex) return;

        // ���� Ÿ�� �Ѿ �����̸� ���� ���׸�Ʈ ���� �������� ����
        int next = _currentIndex + 1;
        if (_queueNext && next < segmentStarts.Length)
        {
            _queueNext = false;
            _currentIndex = next;
            _windowOpen = false;

            float startT = Mathf.Clamp01(segmentStarts[_currentIndex]);
            animator.CrossFadeInFixedTime(stateName, crossFadeTime, layer, startT);
            // Debug.Log($"[Combo] Go Next seg:{_currentIndex}");
        }
        else
        {
            // �޺� ����
            _isAttacking = false;
            _windowOpen = false;
            _currentIndex = -1;
            // Debug.Log("[Combo] End");
        }
    }

    /// <summary>
    /// (�ɼ�) ������ Ư�� ���׸�Ʈ�� �����ϴ� ��ƿ ? �����/Ư�� �����
    /// </summary>
    public void Ev_GotoSegment(int segmentIndex)
    {
        if (animator == null || segmentIndex < 0 || segmentIndex >= segmentStarts.Length) return;

        _isAttacking = true;
        _queueNext = false;
        _currentIndex = segmentIndex;
        _windowOpen = false;

        animator.CrossFadeInFixedTime(stateName, crossFadeTime, layer,
            Mathf.Clamp01(segmentStarts[_currentIndex]));
    }
}
