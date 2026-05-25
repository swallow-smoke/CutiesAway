using System;
using _002_Scripts.Data.Message;
using MessagePipe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VContainer;

namespace _002_Scripts.Controller
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        private Vector2 _startPos;
        private Vector2 _currentPos;
        private Vector2 finalPos;
        private bool isJumped = false;
        private bool isDrag = false;
        [SerializeField] private float maxDragDistance = 5.0f;
        [SerializeField] private float launchForce = 5.0f;
        [SerializeField] private AnimationCurve animCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private int trajectionPoints = 30;
        [SerializeField] private float times = 0.1f;
        [SerializeField] private LayerMask checkLayer;
        [SerializeField] private InputActionReference touchPosRef;

        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private float _jumpTime = 0f;
        [SerializeField] private float _minJumpDur = 0.2f;

        [SerializeField] private Animator _animator;

        private IPublisher<PlayerFlagMsg> _playerFlagPublisher;
        private IDisposable bag;

        private void FixedUpdate()
        {
            if (!isJumped) return;

            _jumpTime += Time.fixedDeltaTime;
            if (_jumpTime > _minJumpDur && _rb.velocity.magnitude < 0.1f)
            {
                isJumped = false;
                _animator.SetBool("isJump", false);
                _jumpTime = 0f;
            }
        }

        // private void OnCollisionEnter2D(Collision2D other)
        // {
        //     if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //     {
        //         isJumped = false;
        //         _animator.SetBool("isJump", false);
        //     }
        // }

        public void OnTouch(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                if (isJumped) return;
                Vector2 screenPos = touchPosRef.action.ReadValue<Vector2>();

                _startPos = Camera.main.ScreenToWorldPoint(
                    new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z));
                _lineRenderer.enabled = true;
                isDrag = false;
            }

            if (ctx.canceled)
            {
                if (isJumped || !isDrag) return;
                ReleaseTouch();
                isJumped = true;
                isDrag = false;
                _animator.SetBool("isJump", true);
            }
        }

        public void ReleaseTouch()
        {
            _rb.AddForce(finalPos, ForceMode2D.Impulse);
            _lineRenderer.enabled = false;
        }

        public void OnDrag(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                _currentPos = ctx.ReadValue<Vector2>();

                finalPos = CalculateVelocity(_currentPos);
                ShowTranjection(finalPos);
                isDrag = true;
            }
        }

        private Vector2 CalculateVelocity(Vector2 pos)
        {
            Vector2 fingerWorldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(_currentPos.x, _currentPos.y, -Camera.main.transform.position.z));

            Vector2 dragDelta = _startPos - fingerWorldPos;
            Vector2 clamped = Vector2.ClampMagnitude(dragDelta, maxDragDistance);
            float dragForce = clamped.magnitude / maxDragDistance;

            // Debug.Log($"finger: {fingerWorldPos} / delta: {dragDelta} / magnitude: {dragDelta.magnitude} / power: {dragForce}");

            float curved = animCurve.Evaluate(dragForce);
            return clamped.normalized * curved * launchForce;
        }

        private void ShowTranjection(Vector2 inputPos)
        {
            _lineRenderer.positionCount = trajectionPoints;
            Vector2 velocity = finalPos / _rb.mass;
            Vector2 pos = transform.position;

            for (int i = 0; i < trajectionPoints; i++)
            {
                Vector2 nextPos = pos
                                  + velocity * times
                                  + 0.5f * Physics2D.gravity * times * times;

                RaycastHit2D hit = Physics2D.Raycast(pos, velocity.normalized, (nextPos - pos).magnitude,
                    checkLayer);

                if (hit.collider != null)
                {
                    float bounce = 1f;

                    if (hit.collider.sharedMaterial != null)
                    {
                        bounce = hit.collider.sharedMaterial.bounciness;
                    }

                    _lineRenderer.SetPosition(i, hit.point);
                    velocity = Vector2.Reflect(velocity, hit.normal) * bounce;
                    pos = hit.point + hit.normal * 0.1f;
                }
                else
                {
                    _lineRenderer.SetPosition(i, nextPos);
                    pos = nextPos;
                }

                velocity += Physics2D.gravity * times;
            }
        }

        [Inject]
        public void Construct(IPublisher<PlayerFlagMsg> playerFlagPublisher)
        {
            var builder = DisposableBag.CreateBuilder();
            _playerFlagPublisher = playerFlagPublisher;


            bag = builder.Build();
        }

        private void OnDestroy()
        {
            bag.Dispose();
        }
    }
}