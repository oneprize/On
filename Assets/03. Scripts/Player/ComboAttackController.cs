using System;
using UnityEngine;

/// <summary>
/// 하나의 애니메이션 상태(클립)에 1~N개의 모션(세그먼트)이 들어있을 때,
/// 애니메이션 "이벤트"로 입력창(open/close)과 세그먼트 끝(end)을 알려주며
/// 클릭 입력에 따라 다음 세그먼트로 넘어가는 콤보 컨트롤러.
/// </summary>
public class ComboAttackEventDriver : MonoBehaviour
{
    [Header("필수")]
    public Animator animator;
    [Tooltip("콤보 애니메이션이 재생되는 Animator 상태 이름")]
    public string stateName = "Slash";
    [Tooltip("재생할 레이어 인덱스")]
    public int layer = 0;

    [Header("입력")]
    public string inputButton = "Fire1";   // 기본: 마우스 왼쪽

    [Header("세그먼트 시작 지점 (정규화 시간)")]
    [Tooltip("각 모션(1타/2타/3타...)의 시작 지점(0~1)")]
    public float[] segmentStarts = new float[] { 0.00f, 0.33f, 0.66f }; // 예: 3타

    [Header("전이 설정")]
    [Tooltip("세그먼트로 점프할 때 페이드 시간(초)")]
    public float crossFadeTime = 0.05f;

    // 내부 상태
    int _currentIndex = -1;     // 현재 세그먼트 (0부터)
    bool _isAttacking = false;  // 콤보 중인지
    bool _windowOpen = false;  // 현재 세그먼트의 입력창 열림 여부
    bool _queueNext = false;  // 다음 세그먼트 예약 여부

    void Reset()
    {
        if (!animator) animator = GetComponentInParent<Animator>();
    }

    void Update()
    {
        // 입력 처리
        if (Input.GetButtonDown(inputButton))
        {
            if (!_isAttacking)
            {
                StartCombo();
            }
            else if (_windowOpen)
            {
                // 입력창 열려 있으면 다음 타 예약
                _queueNext = true;
                // Debug.Log("[Combo] Queue Next");
            }
        }
    }

    /// <summary>콤보 시작(1타로 점프)</summary>
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

    /// <summary>현재 세그먼트 입력창 오픈(애니메이션 이벤트로 호출)</summary>
    /// <param name="segmentIndex">0부터 시작</param>
    public void Ev_OpenWindow(int segmentIndex)
    {
        if (!_isAttacking || segmentIndex != _currentIndex) return;
        _windowOpen = true;
        // Debug.Log($"[Combo] OpenWindow seg:{segmentIndex}");
    }

    /// <summary>현재 세그먼트 입력창 클로즈(애니메이션 이벤트로 호출)</summary>
    public void Ev_CloseWindow(int segmentIndex)
    {
        if (!_isAttacking || segmentIndex != _currentIndex) return;
        _windowOpen = false;
        // Debug.Log($"[Combo] CloseWindow seg:{segmentIndex}");
    }

    /// <summary>현재 세그먼트 종료 시점(애니메이션 이벤트로 호출)</summary>
    public void Ev_EndSegment(int segmentIndex)
    {
        if (!_isAttacking || segmentIndex != _currentIndex) return;

        // 다음 타로 넘어갈 조건이면 다음 세그먼트 시작 지점으로 점프
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
            // 콤보 종료
            _isAttacking = false;
            _windowOpen = false;
            _currentIndex = -1;
            // Debug.Log("[Combo] End");
        }
    }

    /// <summary>
    /// (옵션) 강제로 특정 세그먼트로 점프하는 유틸 ? 디버그/특수 연출용
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
