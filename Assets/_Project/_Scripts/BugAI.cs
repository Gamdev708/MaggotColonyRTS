using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.InputSystem;
public class BugAI : MonoBehaviour, IAttackable, IDamagable
{
    [SerializeField] private float health = 10f;

    private IDamagable target;
    [SerializeField] private Animator animator;
    [SerializeField] private int wanderSpread;
    [SerializeField] private int wanderLength;
    [SerializeField] private Seeker seeker;
    [SerializeField] private RangeFinder range;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MovementAstar movement;
    [SerializeField] private SpriteRotation spriteRotation;
    [SerializeField] private GameObject corpse;
    private RandomPath path;
    private void Start()
    {
        Wander();
        InvokeRepeating(nameof(Wander), 0, 3f);
        range.OnUnspot += Range_OnUnspot;
        range.OnSpot += Range_OnSpot;
    }

    private void Range_OnSpot(Transform obj)
    {
        if (obj == null) return;
        if (obj.TryGetComponent(out IDamagable d))
        {
            target = d;
        }
    }

    private void RotateTowardsPosition(Transform transform)
    {
        Vector2 dir = transform.position - this.transform.position;
        dir.Normalize();
        float angle = Mathf.Atan2(dir.y, dir.x);
        spriteRotation.SetAngle((int)angle, true);

    }
    private void Range_OnUnspot()
    {
        Wander();
        target = null;
    }
    private void Update()
    {
        void DamageTarget()
        {
            target.Damage(3f, this);

            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("Attack");
            RotateTowardsPosition(target.transform);
        }
        if (!target.IsAlive() && range.ClosestTarget != null)
        {
            CancelInvoke(nameof(Wander));
            target = range.ClosestTarget.GetComponent<IDamagable>();
        }
        else if (range.ClosestTarget != null && target != null && target.IsAlive() && target.transform == range.ClosestTarget.transform)
        {
            TimerUtils.AddTimer(0.75f, DamageTarget);
        }
        float speed = movement.CurrentSpeed;
        audioSource.enabled = speed > 0;
        if (target != null && target.IsAlive() && (range.ClosestTarget == null || range.ClosestTarget != target.transform))
        {
            seeker.StartPath(transform.position, target.transform.position);
        }
            movement.canMove = !animator.GetBool("IsAttacking");
    }

    private void Wander()
    {
        if (target == null || !target.IsAlive())
        {
            path = RandomPath.Construct(transform.position, wanderLength);
            path.spread = wanderSpread;
            seeker.StartPath(path);
        }
    }

    public void Damage(float damage, IDamagable owner)
    {
        if (health <= 0) return;
        health -= damage;
        target = owner;
        seeker.StartPath(transform.position, target.transform.position);
        if (health <= 0)
        {
            var corpse = Instantiate(this.corpse, transform.position, Quaternion.identity);
            corpse.GetComponent<Animator>().SetInteger("Angle",animator.GetInteger("Angle"));
            gameObject.SetActive(false);
            Destroy(corpse, 10);
            Destroy(gameObject, 1f);
        }
    }
}
