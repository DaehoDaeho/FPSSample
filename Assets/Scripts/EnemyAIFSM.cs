// ����������������������������������������������������������������������������������������������������������������������������
// 1) ���� �⺻ Ŭ����(����� �θ�)
// ����������������������������������������������������������������������������������������������������������������������������
using UnityEngine;

public abstract class EnemyAIState
{
    // ���� ����/��Ż ��(�ʿ� ������ ����ֵ� ��)
    public virtual void Enter(EnemyController_OO ctx) { }
    public virtual void Exit(EnemyController_OO ctx) { }

    // �� ������ ȣ��. ���¸� �����ϸ� null, �ٲٷ��� �� ���� �ν��Ͻ��� ��ȯ.
    public abstract EnemyAIState Update(EnemyController_OO ctx);
}

// ����������������������������������������������������������������������������������������������������������������������������
// 3) ��ü ���µ�(Idle / Chase / Attack)
// ����������������������������������������������������������������������������������������������������������������������������
public class IdleState : EnemyAIState
{
    public override EnemyAIState Update(EnemyController_OO ctx)
    {
        float dist = ctx.DistanceToPlayer();

        // ���: �ʹ� �ָ� �����ֱ�
        if (dist > ctx.detectRange)
        {
            ctx.agent.isStopped = true;
            return null; // ���� ����
        }

        // ���� ���� ��: �������� ��ȯ
        return new ChaseState();
    }
}

public class ChaseState : EnemyAIState
{
    public override EnemyAIState Update(EnemyController_OO ctx)
    {
        float dist = ctx.DistanceToPlayer();

        // ���� ��: Idle
        if (dist > ctx.detectRange)
        {
            ctx.agent.isStopped = true;
            return new IdleState();
        }

        // ���� ���� ���� �� Attack
        bool canAttack = false;
        if (ctx.attackMode == EnemyController_OO.AttackAIMode.MeleeOnly)
            canAttack = (dist <= ctx.meleeRange);
        else if (ctx.attackMode == EnemyController_OO.AttackAIMode.RangedOnly)
            canAttack = (dist <= ctx.rangedRange);
        else // Hybrid
            canAttack = (dist <= ctx.rangedRange);

        if (canAttack)
        {
            ctx.agent.isStopped = true;
            return new AttackState();
        }

        // ���� ���
        ctx.agent.isStopped = false;
        ctx.agent.SetDestination(ctx.player.position);
        return null; // ����
    }
}

public class AttackState : EnemyAIState
{
    // ���� ����(������) ����
    MeleeAttackStrategy melee = new MeleeAttackStrategy();
    RangedAttackStrategy ranged = new RangedAttackStrategy();

    public override EnemyAIState Update(EnemyController_OO ctx)
    {
        float dist = ctx.DistanceToPlayer();

        // ���� �� �� Idle
        if (dist > ctx.detectRange)
        {
            ctx.agent.isStopped = true;
            return new IdleState();
        }

        // ���� ���� �� �� Chase
        bool insideAttackArea = false;
        if (ctx.attackMode == EnemyController_OO.AttackAIMode.MeleeOnly)
            insideAttackArea = (dist <= ctx.meleeRange);
        else if (ctx.attackMode == EnemyController_OO.AttackAIMode.RangedOnly)
            insideAttackArea = (dist <= ctx.rangedRange);
        else
            insideAttackArea = (dist <= ctx.rangedRange);

        if (!insideAttackArea)
        {
            return new ChaseState();
        }

        // ���� ����
        ctx.agent.isStopped = true;
        ctx.FacePlayerHorizontally();

        if (ctx.attackMode == EnemyController_OO.AttackAIMode.MeleeOnly)
        {
            melee.TryAttack(ctx, dist);
        }
        else if (ctx.attackMode == EnemyController_OO.AttackAIMode.RangedOnly)
        {
            ranged.TryAttack(ctx, dist);
        }
        else // Hybrid: �Ÿ��� ���� ����(�����׸��ý�)
        {
            // �̹� ���� ���̸� �� �� �־��� ������ ���� ����
            if (ctx.usingMelee)
            {
                if (dist > ctx.meleeRange + ctx.meleeExitBuffer)
                    ctx.usingMelee = false;
            }
            else
            {
                if (dist <= ctx.meleeRange)
                    ctx.usingMelee = true;
            }

            if (ctx.usingMelee) melee.TryAttack(ctx, dist);
            else ranged.TryAttack(ctx, dist);
        }

        return null; // ���� ���� ����
    }
}

// ����������������������������������������������������������������������������������������������������������������������������
// 4) ���� ����(���/������) - �������̽� ��� �߻� �θ� + �ڽ� 2��
// ����������������������������������������������������������������������������������������������������������������������������
public abstract class AttackStrategy
{
    public abstract void TryAttack(EnemyController_OO ctx, float distToPlayer);
}

public class MeleeAttackStrategy : AttackStrategy
{
    public override void TryAttack(EnemyController_OO ctx, float distToPlayer)
    {
        // ��Ÿ��
        if (Time.time < ctx.nextMeleeTime) return;

        // �� �տ� ���� ������ ����
        Vector3 center = ctx.transform.position + ctx.transform.forward * (ctx.meleeRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, ctx.hitRadius, ctx.playerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerStatus ps = hits[i].GetComponent<PlayerStatus>();
            if (ps != null)
            {
                ps.TakeDamage(ctx.meleeDamage); // ���� ������
                break; // �� ����
            }
        }
        ctx.nextMeleeTime = Time.time + ctx.meleeCooldown;
        // (����) ���� �ִ�/���� Ʈ���� ����
    }
}

public class RangedAttackStrategy : AttackStrategy
{
    public override void TryAttack(EnemyController_OO ctx, float distToPlayer)
    {
        // ��Ÿ��
        if (Time.time < ctx.nextRangedTime) return;
        if (ctx.projectilePrefab == null || ctx.firePoint == null) return;

        GameObject proj = GameObject.Instantiate(ctx.projectilePrefab, ctx.firePoint.position, ctx.firePoint.rotation);

        // Projectile �� �����(���� �ʵ� ����)
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.speed = ctx.projectileSpeed;
            p.damage = ctx.projectileDamage;
        }

        ctx.nextRangedTime = Time.time + ctx.rangedCooldown;
        // (����) ���� �÷���/���� ����
    }
}